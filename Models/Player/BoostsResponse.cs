﻿using MCE_API_SERVER.Models.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Models.Player
{
	public class MiniFigRecords { }

	public class StatusEffects
	{
		public int? tappableInteractionRadius { get; set; }
		public int? experiencePointRate { get; set; }
		public List<int?> itemExperiencePointRates { get; set; }
		public int? attackDamageRate { get; set; }
		public int? playerDefenseRate { get; set; }
		public int? blockDamageRate { get; set; }
		public int? maximumPlayerHealth { get; set; }
		public int? craftingSpeed { get; set; }
		public int? smeltingFuelIntensity { get; set; }
		public int? foodHealthRate { get; set; }
	}

	public class ScenarioBoosts
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<ActiveBoost> death { get; set; } // Only on death for now
	}

	public class ActiveBoost
	{
		public List<Item.Effect> effects { get; set; }
		public bool enabled { get; set; }
		public DateTime? expiration { get; set; }
		public string instanceId { get; set; }
	}

	public class BoostResult
	{
		public List<Potion> potions { get; set; }

		public List<object> miniFigs { get; set; }

		public MiniFigRecords miniFigRecords { get; set; }

		public List<ActiveEffect> activeEffects { get; set; }

		public StatusEffects statusEffects { get; set; }

		public ScenarioBoosts scenarioBoosts { get; set; }

		public DateTime? expiration { get; set; }
	}

	public class Potion
	{
		public bool enabled { get; set; }
		public DateTime? expiration { get; set; }
		public string instanceId { get; set; }
		public Guid itemId { get; set; }
	}

	public class ActiveEffect
	{
		public Item.Effect effect { get; set; }
		public DateTime? expiration { get; set; }
	}

	public class BoostResponse
	{
		public BoostResult result { get; set; }
		public Updates updates { get; set; }

		public BoostResponse()
		{
			result = new BoostResult
			{
				activeEffects = new List<ActiveEffect>(),
				miniFigRecords = new MiniFigRecords(),
				miniFigs = new List<object>(),
				potions = new List<Potion>(),
				scenarioBoosts = new ScenarioBoosts(),
				statusEffects = new StatusEffects { itemExperiencePointRates = new List<int?>() }
			};
			updates = new Updates();
		}
	}
}
