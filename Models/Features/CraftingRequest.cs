﻿using Newtonsoft.Json;

namespace MCE_API_SERVER.Models.Features
{
    public class CraftingRequest
    {
        [JsonProperty("ingredients")]
        public InputItem[] Ingredients { get; set; }

        [JsonProperty("multiplier")]
        public int Multiplier { get; set; }

        [JsonProperty("recipeId")]
        public string RecipeId { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
}
