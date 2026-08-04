using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace YhumiTweaks.Models
{
    [JsonObject]
    public class CarbyPlushyConfig
    {
        [JsonProperty("filters")]
        public CarbyPlushyFilters Filters { get; set; }

        [JsonProperty("upcomingWindowFormat")]
        public string UpcomingWindowFormat { get; set; }

        [JsonProperty("sortingType")]
        public string SortingType { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("completed")]
        public List<uint> Completed { get; set; }

        [JsonProperty("pinned")]
        public List<uint> Pinned { get; set; }

        [JsonProperty("latestPatch")]
        public decimal LatestPatch { get; set; }

        [JsonProperty("dimFishOnVacation")]
        public bool DimFishOnVacation { get; set; }
    }

    [JsonObject]
    public class CarbyPlushyFilters
    {
        [JsonProperty("completion")]
        public string Completion { get; set; }

        [JsonProperty("patch")]
        public List<decimal> Patch { get; set; }

        [JsonProperty("extra")]
        public string Extra { get; set; }

        [JsonProperty("hideAlwaysAvailable")]
        public bool HideAlwaysAvailable { get; set; }
    }
}
