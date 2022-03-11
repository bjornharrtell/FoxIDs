﻿using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models.ViewModels
{
    public class RegisterTwoFactorViewModel : ViewModel
    {
        public string BarcodeImageUrl { get; set; }

        [Display(Name = "Manual setup code (optional)")]
        public string ManualSetupCode { get; set; }

        [Display(Name = "Authenticator app code")]
        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Please enter valid number")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer number")]
        public string AppCode { get; set; }
    }
}
