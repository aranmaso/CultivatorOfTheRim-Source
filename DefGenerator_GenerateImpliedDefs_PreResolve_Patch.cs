using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class CTR_SuperStartPatch : Mod
    {
        public CTR_SuperStartPatch(ModContentPack content) : base(content)
        {
            new Harmony("FarmerJoe.CultivatorOfTheRim").PatchAll();
        }
    }


    [HarmonyPatch(typeof(DefGenerator))]
    [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
    public class DefGenerator_GenerateImpliedDefs_PreResolve_Patch
    {
        private static void Postfix()
        {
            Log.Message("it work?");
            foreach (var item in DefDatabase<ThingDef>.AllDefsListForReading.Where(InjectQiType))
            {
                if (item.category == ThingCategory.Item)
                {
                    if (item.modContentPack != null && item.modContentPack?.PackageId == "aranmaho.xianxia") continue;

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(item.description);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    string tags = null;
                    if (item.IsStuff)
                    {
                        if (!item.stuffProps.categories.NullOrEmpty())
                        {
                            bool hasQiTag = false;
                            if (item.stuffProps.categories.Any(ShouldHaveQiTags))
                            {
                                hasQiTag = true;
                            }
                            if (hasQiTag)
                            {
                                foreach (var category in item.stuffProps.categories)
                                {
                                    if (category == CTR_DefOf.Metallic)
                                    {
                                        if (item.tradeTags == null)
                                        {
                                            item.tradeTags = new List<string>();
                                        }
                                        item.tradeTags.Add("Metal_Qi");
                                        tags += "Metal_Qi".Colorize(Color.yellow);
                                        tags += ",";
                                    }
                                    if (category == CTR_DefOf.Stony)
                                    {
                                        if (item.tradeTags == null)
                                        {
                                            item.tradeTags = new List<string>();
                                        }
                                        item.tradeTags.Add("Earth_Qi");
                                        tags += "Earth_Qi".Colorize(new Color(0.54f, 0.27f, 0.07f));
                                        tags += ",";
                                    }
                                    if (category == CTR_DefOf.Woody)
                                    {
                                        if (item.tradeTags == null)
                                        {
                                            item.tradeTags = new List<string>();
                                        }
                                        item.tradeTags.Add("Wood_Qi");
                                        tags += "Wood_Qi".Colorize(Color.green);
                                        tags += ",";
                                    }
                                }
                            }
                        }
                    }
                    if (!item.thingCategories.NullOrEmpty() && item.thingCategories.Contains(ThingCategoryDefOf.StoneChunks))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Earth_Qi");
                        tags += "Earth_Qi".Colorize(new Color(0.54f, 0.27f, 0.07f));
                        tags += ",";
                    }
                    if (item.thingClass == typeof(Medicine) || item.thingClass.IsSubclassOf(typeof(Medicine)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Healing_Item");
                        tags += "Healing_Item".Colorize(Color.green);
                        tags += ",";
                    }
                    IEnumerable<string> fire = new List<string>()
                    {
                        "fire","hot","heat","star","sun","flame"
                    };
                    if (fire.Any(x => item.label.ToLower().Contains(x)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Fire_Qi");
                        tags += "Fire_Qi".Colorize(Color.red);
                        tags += ",";
                    }

                    IEnumerable<string> icy = new List<string>()
                    {
                        "ice","frozen","freeze","cold","frigid","water","icy","frost"
                    };
                    if (icy.Any(x => item.label.ToLower().Contains(x)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Water_Qi");
                        tags += "Water_Qi".Colorize(Color.blue);
                        tags += ",";
                    }
                    IEnumerable<string> yinCold = new List<string>()
                    {
                        "death","soul"
                    };
                    if (yinCold.Any(x => item.label.ToLower().Contains(x)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Cold_Qi");
                        item.tradeTags.Add("Yin_Qi");
                        tags += "Cold_Qi".Colorize(Color.cyan);
                        tags += ",";
                        tags += "Yin_Qi".Colorize(Color.cyan);
                        tags += ",";
                    }
                    IEnumerable<string> celestial = new List<string>()
                    {
                        "celestial","star","divine","holy"
                    };
                    if (celestial.Any(x => item.label.ToLower().Contains(x)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Immortal_Qi");
                        tags += "Immortal_Qi".Colorize(Color.cyan);
                        tags += ",";
                    }
                    if (item.label.ToLower().Contains("existence"))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Qi_Source");
                        item.tradeTags.Add("Qi_Source_Tier6");
                        item.tradeTags.Add("Unlimited_Source");
                        item.tradeTags.Add("Neutral_Qi");
                        item.tradeTags.Add("Cold_Qi");
                        item.tradeTags.Add("Yin_Qi");
                        item.tradeTags.Add("Yang_Qi");
                        item.tradeTags.Add("Fire_Qi");
                        item.tradeTags.Add("Water_Qi");
                        item.tradeTags.Add("Wood_Qi");
                        item.tradeTags.Add("Earth_Qi");
                        item.tradeTags.Add("Metal_Qi");
                        item.tradeTags.Add("Pure_Qi");
                        item.tradeTags.Add("Saint_Qi");
                        item.tradeTags.Add("Heavenly_Qi");
                        item.tradeTags.Add("Immortal_Qi");
                        foreach (var tradeTags in item.tradeTags)
                        {
                            tags += tradeTags;
                            tags += ",";
                            tags += "\n";
                        }

                    }
                    if (tags != null)
                    {
                        stringBuilder.AppendLine("Tags: " + tags);
                        item.description = stringBuilder.ToString().TrimEndNewlines();
                    }
                }
                else
                {
                    continue;
                }
            }
        }
        private static bool ShouldHaveQiTags(StuffCategoryDef stuffDef)
        {
            if (stuffDef == CTR_DefOf.Metallic) return true;
            if (stuffDef == CTR_DefOf.Woody) return true;
            if (stuffDef == CTR_DefOf.Stony) return true;
            return false;
        }
        private static bool InjectQiType(ThingDef def)
        {
            if (def.category == ThingCategory.Item)
            {
                return true;
            }
            return false;
        }        
    }
}
