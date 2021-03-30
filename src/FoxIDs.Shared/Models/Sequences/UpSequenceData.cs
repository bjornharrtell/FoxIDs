﻿using FoxIDs.Models.Logic;
using FoxIDs.Models.Session;
using ITfoxtec.Identity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models.Sequences
{
    public class UpSequenceData : ISequenceData, IValidatableObject
    {
        [JsonProperty(PropertyName = "es")]
        public bool ExternalInitiatedSingleLogout { get; set; } = false;

        [JsonProperty(PropertyName = "dp")]
        public DownPartySessionLink DownPartyLink { get; set; }

        [Required]
        [JsonProperty(PropertyName = "ui")]
        public string UpPartyId { get; set; }

        [JsonProperty(PropertyName = "la")]
        public LoginAction LoginAction { get; set; }

        [JsonProperty(PropertyName = "u")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "ma")]
        public int? MaxAge { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (!ExternalInitiatedSingleLogout)
            {
                if (DownPartyLink == null)
                {
                    results.Add(new ValidationResult($"The field {nameof(DownPartyLink)} is required if not external initiated single logout.", new[] { nameof(DownPartyLink) }));
                }
            }

            return results;
        }
    }
}
