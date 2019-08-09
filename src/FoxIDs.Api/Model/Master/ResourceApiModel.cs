﻿using FoxIDs.Infrastructure.DataAnnotations;
using FoxIDs.Models;
using System.Collections.Generic;

namespace FoxIDs.Model
{
    public class ResourceApiModel
    {
        [Length(0, 20, 5)]
        public List<string> SupportedCultures { get; set; }

        [Length(1, 5000)]
        public List<ResourceName> Names { get; set; }

        [Length(1, 5000)]
        public List<ResourceItem> Resources { get; set; }
    }
}
