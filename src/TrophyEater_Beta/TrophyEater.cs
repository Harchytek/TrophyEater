using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TrophyEater
{
    public class ConfigurationManagerAttributes
    {
        public int? Order;
    }

    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TrophyEaterPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "Harchytek.TrophyEater";
        public const string ModName = "TrophyEater";
        public const string ModVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(ModGUID);

        public enum SkillChoice { Random, Axes, Blocking, Bows, Clubs, Crossbows, Knives, Polearms, Spears, Swords, Unarmed, BloodMagic, ElementalMagic, Pickaxes, WoodCutting, Farming, Fishing, Crafting, Cooking, Dodge, Jump, Ride, Run, Sneak, Swim }

        public class TrophySettings
        {
            public ConfigEntry<float> Points;
            public ConfigEntry<SkillChoice> Skill;
            public ConfigEntry<float> Duration;
        }

        public static Dictionary<string, TrophySettings> TrophyConfigs = new Dictionary<string, TrophySettings>();

        public static ConfigEntry<bool> EnableBossCategory;
        public static ConfigEntry<bool> EnableBossSummoningCategory;

        public static readonly string[] BossItems = { "TrophyEikthyr", "TrophyTheElder", "TrophyBonemass", "TrophyDragonQueen", "TrophyGoblinKing", "TrophySeekerQueen", "TrophyFader" };
        public static readonly string[] BossSummoningItems = { "TrophyDeer", "AncientSeed", "WitheredBone", "DragonEgg", "GoblinTotem", "DvergrKeyFragment", "BellFragment" };

        public static Dictionary<string, string> ItemToPower = new Dictionary<string, string>
        {
            {"TrophyEikthyr", "GP_Eikthyr"},
            {"TrophyTheElder", "GP_TheElder"},
            {"TrophyBonemass", "GP_Bonemass"},
            {"TrophyDragonQueen", "GP_Moder"},
            {"TrophyGoblinKing", "GP_Yagluth"},
            {"TrophySeekerQueen", "GP_Queen"},
            {"TrophyFader", "GP_Fader"},

            {"TrophyDeer", "GP_Eikthyr"},
            {"AncientSeed", "GP_TheElder"},
            {"WitheredBone", "GP_Bonemass"},
            {"DragonEgg", "GP_Moder"},
            {"GoblinTotem", "GP_Yagluth"},
            {"DvergrKeyFragment", "GP_Queen"},
            {"BellFragment", "GP_Fader"}
        };

        public static Dictionary<string, string> PowerTranslations = new Dictionary<string, string>
        {
            {"GP_Eikthyr", "Eikthyr"},
            {"GP_TheElder", "The Elder"},
            {"GP_Bonemass", "Bonemass"},
            {"GP_Moder", "Moder"},
            {"GP_Yagluth", "Yagluth"},
            {"GP_Queen", "The Queen"},
            {"GP_Fader", "Fader"}
        };

        private void Awake()
        {
            SetupTrophies();
            harmony.PatchAll();
            Logger.LogInfo($"{ModName} {ModVersion} loaded successfully!");
        }

        private void SetupTrophies()
        {
            int orderMax = 10000;

            var defaultXp = new Dictionary<string, float>
            {
                {"TrophyBoar", 4f}, {"TrophyDeer", 8f}, {"TrophyNeck", 10f}, {"TrophySerpent", 150f}, {"TrophyEikthyr", 100f},
                {"TrophyGreydwarf", 5f}, {"TrophyGreydwarfBrute", 25f}, {"TrophyGreydwarfShaman", 15f}, {"TrophySkeletonPoison", 25f}, {"TrophySkeleton", 10f}, {"TrophyGhost", 20f}, {"TrophyFrostTroll", 30f}, {"TrophyBjorn", 40f}, {"TrophySkeletonHildir", 100f}, {"TrophyTheElder", 200f},
                {"TrophyAbomination", 150f}, {"TrophyBlob", 20f}, {"TrophyDraugr", 20f}, {"TrophyDraugrElite", 30f}, {"TrophyLeech", 25f}, {"TrophySurtling", 25f}, {"TrophyWraith", 30f}, {"TrophyKvastur", 60f}, {"TrophyBonemass", 300f},
                {"TrophyCultist", 50f}, {"TrophyHatchling", 40f}, {"TrophyFenring", 40f}, {"TrophySGolem", 50f}, {"TrophyUlv", 50f}, {"TrophyWolf", 35f}, {"TrophyCultist_Hildir", 300f}, {"TrophyDragonQueen", 400f},
                {"TrophyDeathsquito", 80f}, {"TrophyGoblin", 50f}, {"TrophyGoblinBrute", 70f}, {"TrophyGoblinShaman", 50f}, {"TrophyGrowth", 50f}, {"TrophyLox", 100f}, {"TrophyBjornUndead", 150f}, {"TrophyGoblinBruteBrosShaman", 400f}, {"TrophyGoblinBruteBrosBrute", 400f}, {"TrophyGoblinKing", 500f},
                {"TrophyDvergr", 300f}, {"TrophyHare", 50f}, {"TrophyGjall", 100f}, {"TrophySeeker", 80f}, {"TrophySeekerBrute", 110f}, {"TrophyTick", 70f}, {"TrophySeekerQueen", 600f},
                {"TrophyAsksvin", 120f}, {"TrophyBonemawSerpent", 250f}, {"TrophyFallenValkyrie", 200f}, {"TrophyCharredArcher", 110f}, {"TrophyMorgen", 200f}, {"TrophyVolture", 80f}, {"TrophyCharredMage", 120f}, {"TrophyCharredMelee", 300f}, {"TrophyFader", 700f},
                {"AncientSeed", 25f}, {"WitheredBone", 30f},{"DragonEgg", 100f}, {"GoblinTotem", 70f}, {"DvergrKeyFragment", 100f}, {"BellFragment", 120f}
            };

            var secondaryDurations = new Dictionary<string, float>
            {
                {"TrophyDeer", 120f}, {"AncientSeed", 120f}, {"WitheredBone", 120f},
                {"DragonEgg", 300f}, {"GoblinTotem", 120f}, {"DvergrKeyFragment", 120f}, {"BellFragment", 120f}
            };

            var categories = new Dictionary<string, string[]>
            {               
                { "Meadows", new[] { "TrophyBoar", "TrophyNeck", "TrophySerpent" } },
                { "BlackForest", new[] { "TrophyGreydwarf", "TrophyGreydwarfBrute", "TrophyGreydwarfShaman", "TrophySkeletonPoison", "TrophySkeleton", "TrophyGhost", "TrophyFrostTroll", "TrophyBjorn", "TrophySkeletonHildir" } },
                { "Swamp", new[] { "TrophyAbomination", "TrophyBlob", "TrophyDraugr", "TrophyDraugrElite", "TrophyLeech", "TrophySurtling", "TrophyWraith", "TrophyKvastur" } },
                { "Mountains", new[] { "TrophyCultist", "TrophyHatchling", "TrophyFenring", "TrophySGolem", "TrophyUlv", "TrophyWolf", "TrophyCultist_Hildir" } },
                { "Plains", new[] { "TrophyDeathsquito", "TrophyGoblin", "TrophyGoblinBrute", "TrophyGoblinShaman", "TrophyGrowth", "TrophyLox", "TrophyBjornUndead", "TrophyGoblinBruteBrosShaman", "TrophyGoblinBruteBrosBrute" } },
                { "Mistlands", new[] { "TrophyDvergr", "TrophyHare", "TrophyGjall", "TrophySeeker", "TrophySeekerBrute", "TrophyTick" } },
                { "Ashlands", new[] { "TrophyAsksvin", "TrophyBonemawSerpent", "TrophyFallenValkyrie", "TrophyCharredArcher", "TrophyMorgen", "TrophyVolture", "TrophyCharredMage", "TrophyCharredMelee" } },
                { "Bosses", BossItems },
                { "Boss Summoning", BossSummoningItems }
            };

            foreach (var cat in categories)
            {
                if (cat.Key == "Bosses")
                {
                    EnableBossCategory = Config.Bind("Bosses", "Enable Bosses Category", true, new ConfigDescription("Enable or disable the entire Bosses category.", null, new ConfigurationManagerAttributes { Order = orderMax-- }));
                }
                else if (cat.Key == "Boss Summoning")
                {
                    EnableBossSummoningCategory = Config.Bind("Boss Summoning", "Enable Boss Summoning Category", true, new ConfigDescription("Enable or disable the entire Boss Summoning category.", null, new ConfigurationManagerAttributes { Order = orderMax-- }));
                }

                foreach (var item in cat.Value)
                {
                    float defaultXpVal = defaultXp.ContainsKey(item) ? defaultXp[item] : 1f;
                    bool hasDuration = (cat.Key == "Bosses" || secondaryDurations.ContainsKey(item));
                    
                    ConfigEntry<float> pts;
                    ConfigEntry<SkillChoice> skill;
                    ConfigEntry<float> duration = null;

                    if (hasDuration)
                    {
                        float defaultDuration = cat.Key == "Bosses" ? 300f : secondaryDurations[item];
                        
                        duration = Config.Bind(cat.Key, item + "_Duration", defaultDuration, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = orderMax-- }));
                        pts = Config.Bind(cat.Key, item + "_Points", defaultXpVal, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = orderMax-- }));
                        skill = Config.Bind(cat.Key, item + "_Skill", SkillChoice.Random, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = orderMax-- }));
                    }
                    else
                    {
                        pts = Config.Bind(cat.Key, item + "_Points", defaultXpVal, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = orderMax-- }));
                        skill = Config.Bind(cat.Key, item + "_Skill", SkillChoice.Random, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = orderMax-- }));
                    }

                    TrophyConfigs[item] = new TrophySettings { Points = pts, Skill = skill, Duration = duration };
                }
            }
        }

        public static Skills.SkillType GetSkillType(SkillChoice choice)
        {
            if (choice == SkillChoice.Random)
            {
                var skills = Enum.GetValues(typeof(Skills.SkillType)).Cast<Skills.SkillType>().Where(s => s > Skills.SkillType.None && s < Skills.SkillType.All).ToArray();
                return skills[UnityEngine.Random.Range(0, skills.Length)];
            }
            return Enum.TryParse(choice.ToString(), out Skills.SkillType s) ? s : Skills.SkillType.Swords;
        }
    }

    [HarmonyPatch(typeof(ObjectDB), "Awake")]
    public static class MakeConsumablePatch
    {
        static void Postfix(ObjectDB __instance)
        {
            foreach (var kvp in TrophyEaterPlugin.TrophyConfigs)
            {
                var itemName = kvp.Key;
                
                if (TrophyEaterPlugin.BossItems.Contains(itemName) && !TrophyEaterPlugin.EnableBossCategory.Value) continue;
                if (TrophyEaterPlugin.BossSummoningItems.Contains(itemName) && !TrophyEaterPlugin.EnableBossSummoningCategory.Value) continue;

                var cfg = kvp.Value;

                bool hasPoints = cfg.Points.Value > 0;
                bool hasDuration = cfg.Duration != null && cfg.Duration.Value > 0;

                if (!hasPoints && !hasDuration) continue;

                var prefab = __instance.GetItemPrefab(itemName);
                if (prefab?.TryGetComponent<ItemDrop>(out var id) == true)
                {
                    id.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
                    id.m_itemData.m_shared.m_food = 0;
                }
            }
        }
    }

    [HarmonyPatch]
    public static class TooltipPatch
    {
        [HarmonyTargetMethod]
        static System.Reflection.MethodBase TargetMethod() => typeof(ItemDrop.ItemData).GetMethods().First(m => m.Name == "GetTooltip" && m.GetParameters().Length >= 1);

        static void Postfix(ItemDrop.ItemData __instance, ref string __result)
        {
            if (__instance?.m_dropPrefab == null || !TrophyEaterPlugin.TrophyConfigs.ContainsKey(__instance.m_dropPrefab.name)) return;
            
            string prefabName = __instance.m_dropPrefab.name;
            
            if (TrophyEaterPlugin.BossItems.Contains(prefabName) && !TrophyEaterPlugin.EnableBossCategory.Value) return;
            if (TrophyEaterPlugin.BossSummoningItems.Contains(prefabName) && !TrophyEaterPlugin.EnableBossSummoningCategory.Value) return;

            var cfg = TrophyEaterPlugin.TrophyConfigs[prefabName];
            
            if (cfg.Points.Value > 0)
            {
                __result += $"\nAdds <color=orange>{cfg.Points.Value}</color> points to <color=orange>{cfg.Skill.Value}</color> skill.";
            }

            if (cfg.Duration != null && cfg.Duration.Value > 0)
            {
                if (TrophyEaterPlugin.ItemToPower.TryGetValue(prefabName, out string seName))
                {
                    string powerName = TrophyEaterPlugin.PowerTranslations.ContainsKey(seName) ? TrophyEaterPlugin.PowerTranslations[seName] : seName;
                    __result += $"\n Grants <color=orange>{powerName}</color> power for <color=orange>{cfg.Duration.Value}</color> sec.";
                }
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeItem))]
    public static class ConsumePatch
    {
        static void Postfix(Player __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!__result || item?.m_dropPrefab == null || !TrophyEaterPlugin.TrophyConfigs.ContainsKey(item.m_dropPrefab.name)) return;

            string prefabName = item.m_dropPrefab.name;
            
            if (TrophyEaterPlugin.BossItems.Contains(prefabName) && !TrophyEaterPlugin.EnableBossCategory.Value) return;
            if (TrophyEaterPlugin.BossSummoningItems.Contains(prefabName) && !TrophyEaterPlugin.EnableBossSummoningCategory.Value) return;

            var cfg = TrophyEaterPlugin.TrophyConfigs[prefabName];
            
            if (cfg.Points.Value > 0)
            {
                var type = TrophyEaterPlugin.GetSkillType(cfg.Skill.Value);
                __instance.RaiseSkill(type, cfg.Points.Value);
                __instance.Message(MessageHud.MessageType.Center, $"You increase {cfg.Points.Value} skill points to {type}");
            }

            if (cfg.Duration != null && cfg.Duration.Value > 0)
            {
                if (TrophyEaterPlugin.ItemToPower.TryGetValue(prefabName, out string seName))
                {
                    var effect = ObjectDB.instance.m_StatusEffects.FirstOrDefault(se => se.name == seName);
                    if (effect != null)
                    {
                        var cloned = UnityEngine.Object.Instantiate(effect);
                        cloned.m_ttl = cfg.Duration.Value;
                        __instance.GetSEMan().AddStatusEffect(cloned, true);
                    }
                }
            }
        }
    }
}