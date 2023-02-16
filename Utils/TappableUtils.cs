﻿using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Features;
using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Uuid;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Utils
{
    /// <summary>
    /// Some simple utilities to interface with generated files from Tappy
    /// </summary>
    public static class TappableUtils
    {
        private static Version4Generator version4Generator = new Version4Generator();

        // TODO: Consider turning this into a dictionary (or pull it out to a separate file) and building out a spawn-weight system? 
        public static string[] TappableTypes = new[]
        {
            "genoa:stone_mound_a_tappable_map", "genoa:stone_mound_b_tappable_map",
            "genoa:stone_mound_c_tappable_map", "genoa:grass_mound_a_tappable_map",
            "genoa:grass_mound_b_tappable_map", "genoa:grass_mound_c_tappable_map", "genoa:tree_oak_a_tappable_map",
            "genoa:tree_oak_b_tappable_map", "genoa:tree_oak_c_tappable_map", "genoa:tree_birch_a_tappable_map",
            "genoa:tree_spruce_a_tappable_map", "genoa:chest_tappable_map", "genoa:sheep_tappable_map",
            "genoa:cow_tappable_map", "genoa:pig_tappable_map", "genoa:chicken_tappable_map"
        };

        private static Random random = new Random();

        // For json deserialization
        public class PossibleItemCount
        {
            public int min { get; set; }
            public int max { get; set; }
        }
        public class TappableLootTable
        {
            public string tappableID { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public Item.Rarity rarity { get; set; }
            public int? experiencePoints { get; set; }
            public List<List<Guid>> possibleDropSets { get; set; }
            public Dictionary<Guid, PossibleItemCount> possibleItemCount { get; set; }
        }

        public static Dictionary<string, TappableLootTable> loadAllTappableSets()
        {
            Log.Information("[Tappables] Loading tappable data.");
            Dictionary<string, TappableLootTable> tappableData = new Dictionary<string, TappableLootTable>();
            string[] files = Directory.GetFiles(SavePath_Server + "tappable", "*.json");
            foreach (string file in files) {
                TappableLootTable table = JsonConvert.DeserializeObject<TappableLootTable>(System.IO.File.ReadAllText(file));
                tappableData.Add(table.tappableID, table);
                Log.Information($"Loaded {table.possibleDropSets.Count} drop sets for tappable ID {table.tappableID} | Path: {file}");
            }

            return tappableData;
        }

        /// <summary>
        /// Generate a new tappable in a given radius of a given cord set
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="radius">Optional. Spawn Radius if not provided, will default to value specified in config</param>
        /// <param name="type">Optional. If not provided, a random type will be picked from TappableUtils.TappableTypes</param>
        /// <returns></returns>
        //double is default set to negative because its *extremely unlikely* someone will set a negative value intentionally, and I can't set it to null.
        public static LocationResponse.ActiveLocation createTappableInRadiusOfCoordinates(double latitude, double longitude, double radius = -1.0, string type = null)
        {
            //if null we do random
            type ??= TappableUtils.TappableTypes[random.Next(0, TappableUtils.TappableTypes.Length)];
            if (radius == -1.0) {
                radius = StateSingleton.config.tappableSpawnRadius;
            }
            Item.Rarity rarity;

            try {
                rarity = StateSingleton.tappableData[type].rarity;
            }
            catch (Exception e) {
                Log.Error("[Tappables] Tappable rarity was not found for tappable type " + type + ". Using common");
                rarity = Item.Rarity.Common;
            }

            DateTime currentTime = DateTime.UtcNow;

            //Nab tile loc
            string tileId = Tile.getTileForCoordinates(latitude, longitude);
            LocationResponse.ActiveLocation tappable = new LocationResponse.ActiveLocation
            {
                id = Guid.NewGuid(), // Generate a random GUID for the tappable
                tileId = tileId,
                coordinate = new Coordinate
                {
                    latitude = Math.Round(latitude + (random.NextDouble() * 2 - 1) * radius, 6), // Round off for the client to be happy
                    longitude = Math.Round(longitude + (random.NextDouble() * 2 - 1) * radius, 6)
                },
                spawnTime = currentTime,
                expirationTime = currentTime.AddMinutes(10), //Packet captures show that typically earth keeps Tappables around for 10 minutes
                type = "Tappable", // who wouldve guessed?
                icon = type,
                metadata = new LocationResponse.Metadata
                {
                    rarity = rarity,
                    rewardId = version4Generator.NewUuid().ToString() // Seems to always be uuidv4 from official responses so generate one
                },
                encounterMetadata = null, //working captured responses have this, its fine
                tappableMetadata = new LocationResponse.TappableMetadata
                {
                    rarity = rarity //assuming this and the above need to allign. Why have 2 occurances? who knows.
                }
            };

            Rewards rewards = GenerateRewardsForTappable(tappable.icon);

            LocationResponse.ActiveLocationStorage storage = new LocationResponse.ActiveLocationStorage { location = tappable, rewards = rewards };

            StateSingleton.activeTappables.Add(tappable.id, storage);

            return tappable;
        }

        public static TappableResponse RedeemTappableForPlayer(string playerId, TappableRequest request)
        {
            if (!StateSingleton.activeTappables.ContainsKey(request.id)) {
                Log.Warning($"Tappable {request.id} wasn't found, cant be redeemed");
                return new TappableResponse()
                {
                    updates = new Updates(),
                };
            }

            LocationResponse.ActiveLocationStorage tappable = StateSingleton.activeTappables[request.id];

            TappableResponse response = new TappableResponse()
            {
                result = new TappableResponse.Result()
                {
                    token = new Token()
                    {
                        clientProperties = new Dictionary<string, string>(),
                        clientType = "redeemtappable",
                        lifetime = "Persistent",
                        rewards = tappable.rewards
                    }
                },
                updates = RewardUtils.RedeemRewards(playerId, tappable.rewards, EventLocation.Tappable)
            };

            EventUtils.HandleEvents(playerId, new TappableEvent { eventId = tappable.location.id });
            StateSingleton.activeTappables.Remove(tappable.location.id);

            return response;
        }

        public static Rewards GenerateRewardsForTappable(string type)
        {
            List<List<Guid>> availableDropSets;
            Dictionary<Guid, PossibleItemCount> availableItemCounts;
            int? experiencePoints;

            try {
                availableDropSets = StateSingleton.tappableData[type].possibleDropSets;
                availableItemCounts = StateSingleton.tappableData[type].possibleItemCount;
                experiencePoints = StateSingleton.tappableData[type].experiencePoints;
            }
            catch (Exception e) {
                Log.Error("[Tappables] no json file for tappable type " + type + " exists in data/tappables. Using backup of dirt (f0617d6a-c35a-5177-fcf2-95f67d79196d)");
                availableDropSets = new List<List<Guid>>
                {
                    new List<Guid>() {Guid.Parse("f0617d6a-c35a-5177-fcf2-95f67d79196d")}
                };
                availableItemCounts = new Dictionary<Guid, PossibleItemCount> { { Guid.Parse("f0617d6a-c35a-5177-fcf2-95f67d79196d"), new PossibleItemCount() { min = 1, max = 3 } } };
                experiencePoints = 1;
                //dirt for you... sorry :/
            }

            List<Guid> targetDropSet = availableDropSets[random.Next(0, availableDropSets.Count)];
            if (targetDropSet == null) {
                Log.Error($"[Tappables] targetDropSet is null! Available drop set count was {availableDropSets.Count}");
            }

            RewardComponent[] itemRewards = new RewardComponent[targetDropSet.Count];
            for (int i = 0; i < targetDropSet.Count; i++) {
                itemRewards[i] = new RewardComponent() { Amount = random.Next(availableItemCounts[targetDropSet[i]].min, availableItemCounts[targetDropSet[i]].max), Id = targetDropSet[i] };
            }

            Rewards rewards = new Rewards { Inventory = itemRewards, ExperiencePoints = experiencePoints, Rubies = 1 };

            return rewards;
        }

        public static LocationResponse.Root GetActiveLocations(double lat, double lon, double radius = -1.0)
        {
            if (radius == -1.0) radius = StateSingleton.config.tappableSpawnRadius;
            Coordinate maxCoordinates = new Coordinate { latitude = lat + radius, longitude = lon + radius };

            List<LocationResponse.ActiveLocation> tappables = StateSingleton.activeTappables
                .Where(pred =>
                    (pred.Value.location.coordinate.latitude >= lat && pred.Value.location.coordinate.latitude <= maxCoordinates.latitude)
                    && (pred.Value.location.coordinate.longitude >= lon && pred.Value.location.coordinate.longitude <= maxCoordinates.longitude))
                .ToDictionary(pred => pred.Key, pred => pred.Value.location).Values.ToList();

            if (tappables.Count <= StateSingleton.config.maxTappableSpawnAmount) {
                int count = random.Next(StateSingleton.config.minTappableSpawnAmount,
                    StateSingleton.config.maxTappableSpawnAmount);
                count -= tappables.Count;
                for (; count > 0; count--) {
                    LocationResponse.ActiveLocation tappable = createTappableInRadiusOfCoordinates(lat, lon);
                    tappables.Add(tappable);
                }
            }

            List<LocationResponse.ActiveLocation> encounters = AdventureUtils.GetEncountersForLocation(lat, lon);
            tappables.AddRange(encounters.Where(pred =>
                    (pred.coordinate.latitude >= lat && pred.coordinate.latitude <= maxCoordinates.latitude)
                    && (pred.coordinate.longitude >= lon && pred.coordinate.longitude <= maxCoordinates.longitude)).ToList());

            return new LocationResponse.Root
            {
                result = new LocationResponse.Result
                {
                    killSwitchedTileIds = new List<object> { }, //havent seen this used. Debugging thing maybe?
                    activeLocations = tappables,
                },
                expiration = null,
                continuationToken = null,
                updates = new Updates()
            };
        }
    }
}
