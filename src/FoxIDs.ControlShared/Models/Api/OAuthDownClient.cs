﻿using FoxIDs.Infrastructure.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models.Api
{
    public class OAuthDownClient
    {
        [Length(Constants.Models.OAuthDownParty.Client.ResourceScopesMin, Constants.Models.OAuthDownParty.Client.ResourceScopesMax)]
        [Display(Name = "Resource and scopes")]
        public List<OAuthDownResourceScope> ResourceScopes { get; set; }

        [Length(Constants.Models.OAuthDownParty.Client.ScopesMin, Constants.Models.OAuthDownParty.Client.ScopesMax)]
        [Display(Name = "Scopes")]
        public List<OAuthDownScope> Scopes { get; set; }

        [Length(Constants.Models.OAuthDownParty.Client.ClaimsMin, Constants.Models.OAuthDownParty.Client.ClaimsMax)]
        [Display(Name = "Claims")]
        public List<OAuthDownClaim> Claims { get; set; }

        [Length(Constants.Models.OAuthDownParty.Client.ResponseTypesMin, Constants.Models.OAuthDownParty.Client.ResponseTypesMax, Constants.Models.OAuthDownParty.Client.ResponseTypeLength)]
        [Display(Name = "Response types")]
        public List<string> ResponseTypes { get; set; }

        [Length(Constants.Models.OAuthDownParty.Client.RedirectUrisMin, Constants.Models.OAuthDownParty.Client.RedirectUrisMax, Constants.Models.OAuthDownParty.Client.RedirectUriLength)]
        [Display(Name = "Redirect uris")]
        public List<string> RedirectUris { get; set; }

        /// <summary>
        /// Enable PKCE, default false.
        /// </summary>
        [Display(Name = "Enable PKCE")]
        public bool? EnablePkce { get; set; } = false;

        [Range(Constants.Models.OAuthDownParty.Client.AuthorizationCodeLifetimeMin, Constants.Models.OAuthDownParty.Client.AuthorizationCodeLifetimeMax)]
        [Display(Name = "Authorization code lifetime")]
        public int? AuthorizationCodeLifetime { get; set; } = 30;

        /// <summary>
        /// Default 60 minutes.
        /// </summary>
        [Range(Constants.Models.OAuthDownParty.Client.AccessTokenLifetimeMin, Constants.Models.OAuthDownParty.Client.AccessTokenLifetimeMax)]
        [Display(Name = "Access token lifetime")]
        public int AccessTokenLifetime { get; set; } = 3600;

        [Range(Constants.Models.OAuthDownParty.Client.RefreshTokenLifetimeMin, Constants.Models.OAuthDownParty.Client.RefreshTokenLifetimeMax)]
        [Display(Name = "Refresh token lifetime")]
        public int? RefreshTokenLifetime { get; set; } = 2160;

        [Range(Constants.Models.OAuthDownParty.Client.RefreshTokenAbsoluteLifetimeMin, Constants.Models.OAuthDownParty.Client.RefreshTokenAbsoluteLifetimeMax)]
        [Display(Name = "Refresh token absolute lifetime")]
        public int? RefreshTokenAbsoluteLifetime { get; set; } = 10080;

        [Display(Name = "Only use refresh token one time")]
        public bool? RefreshTokenUseOneTime { get; set; }

        [Display(Name = "Refresh token lifetime unlimited")]
        public bool? RefreshTokenLifetimeUnlimited { get; set; }
    }
}
