﻿using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models.Config
{
    public class FileDataSettings
    {
        /// <summary>
        /// Save data in directory if the DataStore option is File.
        /// </summary>
        [Required]
        public string DataPath { get; set; } = "..\\..\\";

        /// <summary>
        /// The background file data service wait period in seconds.
        /// </summary>
        [Required]
        public int BackgroundServiceWaitPeriod { get; set; } = 900; // 15 minutes
    }
}
