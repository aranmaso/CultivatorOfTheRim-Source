using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_ArmorGradeCheck
    {
        private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {
            if (__instance == null)
            {
                return;
            }
            if (!__instance.RaceProps.Humanlike)
            {
                return;
            }
            float damAmount = dinfo.Amount;
            //Log.Message("base damage: " + damAmount);
            float mul = 1f;
            float buildingMul = 1f;
            if (!__instance.apparel.WornApparel.EnumerableNullOrEmpty())
            {
                //Log.Message("pawn have cloth");
                foreach (var item in __instance?.apparel?.WornApparel)
                {
                    if (item == null) continue;
                    //Log.Message(item.Label + " not null");
                    CompItemGrade apparelGrade = item?.TryGetComp<CompItemGrade>();
                    if(apparelGrade != null)
                    {
                        //Log.Message(item.Label + " has grade");
                        if (CultivatorOfTheRimMod.settings.isArmorGradeStackMultiplicatively)
                        {
                            switch (apparelGrade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    break;
                                case ItemGrade.Ordinary:
                                    //Log.Message(item.Label + " is Ordinary");
                                    mul *= 0.95f;
                                    break;
                                case ItemGrade.Earth:
                                    //Log.Message(item.Label + " is Earth");
                                    mul *= 0.85f;
                                    break;
                                case ItemGrade.Heaven:
                                    //Log.Message(item.Label + " is Heaven");
                                    mul *= 0.825f;
                                    break;
                                case ItemGrade.Mysterious:
                                    //Log.Message(item.Label + " is Mysterious");
                                    mul *= 0.80f;
                                    break;
                                case ItemGrade.Divine:
                                    //Log.Message(item.Label + " is Divine");
                                    mul *= 0.75f;
                                    break;
                                case ItemGrade.Emperor:
                                    //Log.Message(item.Label + " is Emperor");
                                    mul *= 0.725f;
                                    break;
                                case ItemGrade.Dao:
                                    //Log.Message(item.Label + " is Dao");
                                    mul *= 0.5f;
                                    break;
                            }
                        }     
                        else
                        {
                            switch (apparelGrade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    break;
                                case ItemGrade.Ordinary:
                                    if (0.8f < mul) mul = 0.8f;
                                    break;
                                case ItemGrade.Earth:
                                    if (0.5f < mul) mul = 0.5f;
                                    break;
                                case ItemGrade.Heaven:
                                    if (0.5f < mul) mul = 0.5f;
                                    break;
                                case ItemGrade.Mysterious:
                                    if (0.5f < mul) mul = 0.5f;
                                    break;
                                case ItemGrade.Divine:
                                    if (0.25f < mul) mul = 0.25f;
                                    break;
                                case ItemGrade.Emperor:
                                    if (0.25f < mul) mul = 0.25f;
                                    break;
                                case ItemGrade.Dao:
                                    if (0.1f < mul) mul = 0.1f;
                                    break;
                            }
                        }                        
                    }                    
                }
            }
            //Log.Message("total armor reduction: " + mul);
            if (dinfo.Instigator != null)
            {
                if (dinfo.Instigator is Pawn attacker)
                {
                    if (attacker.RaceProps.Humanlike)
                    {
                        CompItemGrade attackerWeaponGrade = attacker?.equipment?.Primary?.TryGetComp<CompItemGrade>();
                        if (attackerWeaponGrade != null)
                        {
                            switch (attackerWeaponGrade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    break;
                                case ItemGrade.Ordinary:
                                    damAmount *= 1.1f;
                                    break;
                                case ItemGrade.Earth:
                                    damAmount *= 1.25f;
                                    break;
                                case ItemGrade.Heaven:
                                    damAmount *= 1.5f;
                                    break;
                                case ItemGrade.Mysterious:
                                    damAmount *= 1.75f;
                                    break;
                                case ItemGrade.Divine:
                                    damAmount *= 2.00f;
                                    break;
                                case ItemGrade.Emperor:
                                    damAmount *= 2.25f;
                                    break;
                                case ItemGrade.Dao:
                                    damAmount *= 2.5f;
                                    break;
                            }
                        }
                    }
                }
                if (dinfo.Instigator is Building_Turret turret || dinfo.Instigator is Building building)
                {
                    if (!dinfo.Instigator.def.tradeTags.NullOrEmpty() && dinfo.Instigator.def.tradeTags.Contains("Cultivator_Building"))
                    {
                        buildingMul = Rand.Range(0.8f, 0.5f);
                    }
                    else
                    {
                        buildingMul = 0.5f;
                    }
                }
            }
            /*if(Prefs.DevMode)
            {
                Log.Message("final formular: " + damAmount + "*" + mul + "*" + buildingMul + " = " + damAmount * mul * buildingMul);
            }*/
            dinfo.SetAmount(damAmount * mul * buildingMul);
        }
    }
}
