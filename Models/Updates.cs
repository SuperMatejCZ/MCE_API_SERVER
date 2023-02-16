﻿using Newtonsoft.Json;

namespace MCE_API_SERVER.Models
{
    public class Updates
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint characterProfile { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint crafting { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint inventory { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint playerJournal { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint smelting { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint tokens { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint challenges { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint boosts { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint buildplates { get; set; }
    }

    public class UpdateResponse
    {
        public Updates updates { get; set; }
    }
}
