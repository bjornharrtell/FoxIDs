﻿using ITfoxtec.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FoxIDs.Models.Config
{
    public class FoxIDsControlSettings : Settings, IValidatableObject
    {
        [Required]
        public string DownParty { get; set; }

        [ValidateComplexType]
        public ApplicationInsightsSettings ApplicationInsights { get; set; }

        public bool DisableBackgroundQueueService { get; set; }

        /// <summary>
        /// Enable master seed if true.
        /// </summary>
        public bool MasterSeedEnabled { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = base.Validate(validationContext).ToList();

            if (FoxIDsEndpoint.IsNullOrEmpty())
            {
                results.Add(new ValidationResult($"The field {nameof(FoxIDsEndpoint)} is required.", new[] { nameof(FoxIDsEndpoint) }));
            }
            if (FoxIDsControlEndpoint.IsNullOrEmpty())
            {
                results.Add(new ValidationResult($"The field {nameof(FoxIDsControlEndpoint)} is required.", new[] { nameof(FoxIDsControlEndpoint) }));
            }

            if (Options.Log == LogOptions.ApplicationInsights)
            {
                if (ApplicationInsights == null)
                {
                    results.Add(new ValidationResult($"The field {nameof(ApplicationInsights)} is required if {nameof(Options.Log)} is {Options.Log}.", new[] { nameof(ApplicationInsights) }));
                }
            }

            return results;
        }
    }
}
