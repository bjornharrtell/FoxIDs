﻿using FoxIDs.Infrastructure.DataAnnotations;
using ITfoxtec.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FoxIDs.Models
{
    public class UpParty : Party, IValidatableObject, IUpParty
    {
        public static async Task<string> IdFormatAsync(IdKey idKey)
        {
            if (idKey == null) new ArgumentNullException(nameof(idKey));
            await idKey.ValidateObjectAsync();

            return $"{Constants.Models.DataType.UpParty}:{idKey.TenantName}:{idKey.TrackName}:{idKey.PartyName}";
        }

        public static async Task<string> IdFormatAsync(RouteBinding routeBinding, string name)
        {
            if (routeBinding == null) new ArgumentNullException(nameof(routeBinding));
            if (name == null) new ArgumentNullException(nameof(name));

            var idKey = new IdKey
            {
                TenantName = routeBinding.TenantName,
                TrackName = routeBinding.TrackName,
                PartyName = name,
            };

            return await IdFormatAsync(idKey);
        }

        // Support back words capability in CosmosDB - single issuer in SAML 2.0 up-parties
        private bool hasSingleIssuer;
        private List<string> issuers;
        [MaxLength(Constants.Models.Party.IssuerLength)]
        [JsonProperty(PropertyName = "issuer")]
        public string Issuer
        {
            get
            {
                return issuers?.FirstOrDefault();
            }
            set
            {
                if (!value.IsNullOrWhiteSpace())
                {
                    hasSingleIssuer = true;
                    issuers = new List<string> { value };
                }
            }
        }

        [ListLength(Constants.Models.UpParty.IssuersBaseMin, Constants.Models.UpParty.IssuersMax, Constants.Models.Party.IssuerLength)]
        [JsonProperty(PropertyName = "issuers")]
        public virtual List<string> Issuers
        {
            get
            {
                return issuers;
            }
            set
            {
                if (!hasSingleIssuer)
                {
                    issuers = value;
                }
            }
        }

        /// <summary>
        /// SP issuer / audience
        /// For OAuth 2.0 and OIDC, only used in relation to token exchange trust.
        /// </summary>
        [MaxLength(Constants.Models.Party.IssuerLength)]
        [JsonProperty(PropertyName = "sp_issuer")]
        public string SpIssuer { get; set; }

        [JsonProperty(PropertyName = "party_binding_pattern")]
        public PartyBindingPatterns PartyBindingPattern { get; set; } = PartyBindingPatterns.Brackets;

        [Range(Constants.Models.UpParty.SessionLifetimeMin, Constants.Models.UpParty.SessionLifetimeMax)]
        [JsonProperty(PropertyName = "session_lifetime")]
        public int SessionLifetime { get; set; } = 36000;

        [Range(Constants.Models.UpParty.SessionAbsoluteLifetimeMin, Constants.Models.UpParty.SessionAbsoluteLifetimeMax)]
        [JsonProperty(PropertyName = "session_absolute_lifetime")]
        public int SessionAbsoluteLifetime { get; set; } = 86400;

        [Range(Constants.Models.UpParty.PersistentAbsoluteSessionLifetimeMin, Constants.Models.UpParty.PersistentAbsoluteSessionLifetimeMax)]
        [JsonProperty(PropertyName = "persistent_session_absolute_lifetime")]
        public int PersistentSessionAbsoluteLifetime { get; set; }

        [JsonProperty(PropertyName = "persistent_session_lifetime_unlimited")]
        public bool PersistentSessionLifetimeUnlimited { get; set; }

        [JsonProperty(PropertyName = "disable_single_logout")]
        public bool DisableSingleLogout { get; set; }

        [ListLength(Constants.Models.UpParty.HrdDomainMin, Constants.Models.UpParty.HrdDomainMax, Constants.Models.UpParty.HrdDomainLength, Constants.Models.UpParty.HrdDomainRegExPattern)]
        [JsonProperty(PropertyName = "hrd_domains")]
        public List<string> HrdDomains { get; set; }

        [JsonProperty(PropertyName = "hrd_show_buttom_with_domain")]
        public bool HrdShowButtonWithDomain { get; set; }

        [MaxLength(Constants.Models.UpParty.HrdDisplayNameLength)]
        [RegularExpression(Constants.Models.UpParty.HrdDisplayNameRegExPattern)]
        [JsonProperty(PropertyName = "hrd_display_name")]
        public string HrdDisplayName { get; set; }

        [MaxLength(Constants.Models.UpParty.HrdLogoUrlLength)]
        [RegularExpression(Constants.Models.UpParty.HrdLogoUrlRegExPattern)]
        [JsonProperty(PropertyName = "hrd_logo_url")]
        public string HrdLogoUrl { get; set; }

        [JsonProperty(PropertyName = "disable_user_authentication_trust")]
        public bool DisableUserAuthenticationTrust { get; set; }

        [JsonProperty(PropertyName = "disable_token_exchange_trust")]
        public bool DisableTokenExchangeTrust { get; set; }

        [JsonProperty(PropertyName = "profiles")]
        public List<UpPartyProfile> Profiles { get; set; }

        public async Task SetIdAsync(IdKey idKey)
        {
            if (idKey == null) new ArgumentNullException(nameof(idKey));

            Id = await IdFormatAsync(idKey);
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (DisableUserAuthenticationTrust && DisableTokenExchangeTrust)
            {
                results.Add(new ValidationResult($"Both the {nameof(DisableUserAuthenticationTrust)} and the {nameof(DisableTokenExchangeTrust)} can not be disabled at the same time.", [nameof(DisableUserAuthenticationTrust), nameof(DisableTokenExchangeTrust)]));
            }

            if (Profiles?.Count() > 0)
            {
                var count = 0;
                foreach (var profile in Profiles)
                {
                    count++;
                    if ((Name.Length + profile.Name.Length) > Constants.Models.Party.NameLength)
                    {
                        results.Add(new ValidationResult($"The fields {nameof(Name)} (value: '{Name}') and {nameof(profile.Name)} (value: '{profile.Name}') must not be more then {Constants.Models.Party.NameLength} in total.", [nameof(Name), $"{nameof(profile)}[{count}].{nameof(profile.Name)}"]));
                    }
                }
            }
            return results;
        }
    }
}