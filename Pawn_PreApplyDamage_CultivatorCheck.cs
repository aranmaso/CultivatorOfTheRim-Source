using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_CultivatorCheck
    {
        private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {
            if (__instance.RaceProps.IsMechanoid)
            {
                return;
            }
            Hediff pawnCultivation = Cultivation_Utility.FindCultivationLevel(__instance);
            HediffExtension_ByPassHediff modExtension = pawnCultivation?.def?.GetModExtension<HediffExtension_ByPassHediff>();
            if (modExtension == null || pawnCultivation == null)
            {
                return;
            }
            if(!modExtension.byPassHediff.NullOrEmpty() && modExtension.byPassHediff.Any(x => __instance.health.hediffSet.HasHediff(x)))
            {
                return;
            }
            if (dinfo.Instigator == null)
            {
                return;
            }
            if (pawnCultivation == null)
            {
                return;
            }
            if (dinfo.Instigator is Pawn attacker)
            {
                if (!Cultivation_Utility.HaveCultivation(attacker))
                {
                    if(CultivatorOfTheRimMod.settings.isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal)
                    {
                        if (IsSaintRealmOrAbove(pawnCultivation))
                        {
                            dinfo.SetAmount(0);
                            return;
                        }
                        else if (IsCoreShapingOrAbove(pawnCultivation))
                        {
                            if (attacker.RaceProps.Humanlike)
                            {
                                if (attacker?.equipment?.Primary != null)
                                {
                                    ItemGrade weaponGrade = attacker.equipment.Primary.TryGetComp<CompItemGrade>().Grade;
                                    if (weaponGrade >= ItemGrade.Earth)
                                    {
                                        switch (weaponGrade)
                                        {
                                            case ItemGrade.Mortal:
                                                return;
                                            case ItemGrade.Ordinary:
                                                dinfo.SetAmount(dinfo.Amount * 0.1f);
                                                return;
                                            case ItemGrade.Earth:
                                                dinfo.SetAmount(dinfo.Amount * 0.2f);
                                                return;
                                            case ItemGrade.Heaven:
                                                dinfo.SetAmount(dinfo.Amount * 0.25f);
                                                return;
                                            case ItemGrade.Mysterious:
                                                dinfo.SetAmount(dinfo.Amount * 0.5f);
                                                return;
                                            case ItemGrade.Divine:
                                                dinfo.SetAmount(dinfo.Amount * 0.5f);
                                                return;
                                            case ItemGrade.Emperor:
                                                dinfo.SetAmount(dinfo.Amount * 0.5f);
                                                return;
                                            case ItemGrade.Dao:
                                                dinfo.SetAmount(dinfo.Amount * 0.8f);
                                                return;
                                        }
                                    }
                                    else
                                    {
                                        dinfo.SetAmount(dinfo.Amount * 0.01f);
                                    }
                                }
                            }
                            else
                            {
                                dinfo.SetAmount(dinfo.Amount * 0.01f);
                            }

                        }
                    }
                    
                    
                }
                else
                {
                    Hediff h1 = pawnCultivation;
                    Hediff h2 = Cultivation_Utility.FindCultivationLevel(attacker);
                    if(h1.def == h2.def)
                    {
                        if(h1.Severity > h2.Severity)
                        {
                            dinfo.SetAmount(dinfo.Amount *0.75f);
                        }
                    }
                    else
                    {
                        HediffDef h3 = Cultivation_Utility.GetHighestCultivationHediff(h1.def, h2.def);
                        if (h1.def == h3 && IsCoreShapingOrAbove(h1))
                        {
                            int realmDiff = Cultivation_Utility.GetRealmDifferent(h1.def, h2.def);
                            if (realmDiff > 3)
                            {
                                dinfo.SetAmount(0);
                            }
                            else
                            {
                                dinfo.SetAmount(dinfo.Amount / realmDiff);
                            }
                        }
                    }                    
                }
            }
            /*else if ((dinfo.Instigator.def.thingClass.IsSubclassOf(typeof(Building_Turret))))
            {
                if (dinfo.Instigator.def.tradeTags.Contains("Cultivator_Building"))
                {
                    dinfo.SetAmount(dinfo.Amount * Rand.Range(0.25f, 0.5f));
                }
                else
                {
                    if (IsSaintRealmOrAbove(pawnCultivation))
                    {
                        dinfo.SetAmount(0);
                    }
                    else if (IsCoreShapingOrAbove(pawnCultivation))
                    {
                        dinfo.SetAmount(dinfo.Amount * 0.01f);
                    }
                }
            }*/
            else if(dinfo.Instigator is Building building_Turret)
            {
                if (!building_Turret.def.tradeTags.NullOrEmpty() && building_Turret.def.tradeTags.Contains("Cultivator_Building"))
                {
                    dinfo.SetAmount(dinfo.Amount * Rand.Range(0.25f, 0.5f));
                }
            }
            else
            {
                if (IsSaintRealmOrAbove(pawnCultivation))
                {
                    dinfo.SetAmount(0);
                }
                else if (IsCoreShapingOrAbove(pawnCultivation))
                {
                    dinfo.SetAmount(dinfo.Amount * 0.01f);
                }
            }
        }

        private static bool IsCoreShapingOrAbove(Hediff h)
        {
            if(h.def.tags.Contains("CTR_CoreShapingOrAbove"))
            {
                return true;
            }
            return false;
        }
        private static bool IsSaintRealmOrAbove(Hediff h)
        {
            if(h.def.tags.Contains("CTR_SaintRealmOrAbove"))
            {
                return true;
            }
            return false;
        }
    }
    /*[HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_ArmorGradeCheck
    {
        private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {
            if(__instance == null)
            {
                return;
            }
            if (__instance.RaceProps.IsMechanoid || __instance.RaceProps.Animal)
            {
                return;
            }
            Hediff pawnCultivation = Cultivation_Utility.FindCultivationLevel(__instance);
            if (dinfo.Instigator is Pawn attacker)
            {
                BodyPartRecord hitPart = dinfo.HitPart;
                IEnumerable<Apparel> wornApparel = __instance?.apparel?.WornApparel;
                if (!wornApparel.EnumerableNullOrEmpty())
                {
                    foreach (var item in wornApparel)
                    {
                        if (hitPart == null) continue;

                        if(!item.def.apparel.CoversBodyPart(hitPart))
                        {
                            continue;
                        }

                        if (item != null && item.def.apparel.CoversBodyPart(hitPart))
                        {
                            CompItemGrade apparelGrade = item?.TryGetComp<CompItemGrade>();
                            if (apparelGrade == null)
                            {
                                return;
                            }
                            if (attacker.RaceProps.IsMechanoid || attacker.RaceProps.Animal || attacker.RaceProps.IsAnomalyEntity)
                            {
                                switch (apparelGrade.Grade)
                                {
                                    case ItemGrade.Mortal:
                                        return;
                                    case ItemGrade.Ordinary:
                                        dinfo.SetAmount(dinfo.Amount * 0.8f);
                                        return;
                                    case ItemGrade.Earth:
                                        dinfo.SetAmount(dinfo.Amount * 0.5f);
                                        return;
                                    case ItemGrade.Heaven:
                                        dinfo.SetAmount(dinfo.Amount * 0.5f);
                                        return;
                                    case ItemGrade.Mysterious:
                                        dinfo.SetAmount(dinfo.Amount * 0.5f);
                                        return;
                                    case ItemGrade.Divine:
                                        dinfo.SetAmount(dinfo.Amount * 0.25f);
                                        return;
                                    case ItemGrade.Emperor:
                                        dinfo.SetAmount(dinfo.Amount * 0.25f);
                                        return;
                                    case ItemGrade.Dao:
                                        dinfo.SetAmount(dinfo.Amount * 0.10f);
                                        return;
                                }
                            }
                            if (attacker?.equipment?.Primary == null)
                            {
                                switch (apparelGrade.Grade)
                                {
                                    case ItemGrade.Mortal:
                                        return;
                                    case ItemGrade.Ordinary:
                                        dinfo.SetAmount(dinfo.Amount * 0.8f);
                                        return;
                                    case ItemGrade.Earth:
                                        dinfo.SetAmount(dinfo.Amount * 0.5f);
                                        return;
                                    case ItemGrade.Heaven:
                                        dinfo.SetAmount(dinfo.Amount * 0.5f);
                                        return;
                                    case ItemGrade.Mysterious:
                                        dinfo.SetAmount(dinfo.Amount * 0.5f);
                                        return;
                                    case ItemGrade.Divine:
                                        dinfo.SetAmount(dinfo.Amount * 0.25f);
                                        return;
                                    case ItemGrade.Emperor:
                                        dinfo.SetAmount(dinfo.Amount * 0.25f);
                                        return;
                                    case ItemGrade.Dao:
                                        dinfo.SetAmount(dinfo.Amount * 0.10f);
                                        return;
                                }
                            }
                            if (!Cultivation_Utility.HaveCultivation(attacker))
                            {
                                CompItemGrade attackerWeaponGrade = attacker?.equipment?.Primary?.TryGetComp<CompItemGrade>();
                                if (attackerWeaponGrade.Grade < apparelGrade.Grade)
                                {
                                    switch (apparelGrade.Grade)
                                    {
                                        case ItemGrade.Mortal:
                                            return;
                                        case ItemGrade.Ordinary:
                                            dinfo.SetAmount(dinfo.Amount * 0.8f);
                                            return;
                                        case ItemGrade.Earth:
                                            dinfo.SetAmount(dinfo.Amount * 0.5f);
                                            return;
                                        case ItemGrade.Heaven:
                                            dinfo.SetAmount(dinfo.Amount * 0.5f);
                                            return;
                                        case ItemGrade.Mysterious:
                                            dinfo.SetAmount(dinfo.Amount * 0.5f);
                                            return;
                                        case ItemGrade.Divine:
                                            dinfo.SetAmount(dinfo.Amount * 0.25f);
                                            return;
                                        case ItemGrade.Emperor:
                                            dinfo.SetAmount(dinfo.Amount * 0.25f);
                                            return;
                                        case ItemGrade.Dao:
                                            dinfo.SetAmount(dinfo.Amount * 0.10f);
                                            return;
                                    }
                                }
                                else
                                {
                                    switch (attackerWeaponGrade.Grade)
                                    {
                                        case ItemGrade.Mortal:
                                            return;
                                        case ItemGrade.Ordinary:
                                            dinfo.SetAmount(dinfo.Amount * 1.1f);
                                            return;
                                        case ItemGrade.Earth:
                                            dinfo.SetAmount(dinfo.Amount * 1.25f);
                                            return;
                                        case ItemGrade.Heaven:
                                            dinfo.SetAmount(dinfo.Amount * 1.5f);
                                            return;
                                        case ItemGrade.Mysterious:
                                            dinfo.SetAmount(dinfo.Amount * 1.75f);
                                            return;
                                        case ItemGrade.Divine:
                                            dinfo.SetAmount(dinfo.Amount * 2.00f);
                                            return;
                                        case ItemGrade.Emperor:
                                            dinfo.SetAmount(dinfo.Amount * 2.25f);
                                            return;
                                        case ItemGrade.Dao:
                                            dinfo.SetAmount(dinfo.Amount * 2.50f);
                                            return;
                                    }
                                }

                            }
                            if (Cultivation_Utility.HaveCultivation(attacker))
                            {
                                CompItemGrade attackerWeaponGrade = attacker?.equipment?.Primary?.TryGetComp<CompItemGrade>();
                                if(attackerWeaponGrade != null)
                                {
                                    if (attackerWeaponGrade.Grade < apparelGrade.Grade)
                                    {
                                        switch (apparelGrade.Grade)
                                        {
                                            case ItemGrade.Mortal:
                                                return;
                                            case ItemGrade.Ordinary:
                                                dinfo.SetAmount(dinfo.Amount * 0.95f);
                                                return;
                                            case ItemGrade.Earth:
                                                dinfo.SetAmount(dinfo.Amount * 0.8f);
                                                return;
                                            case ItemGrade.Heaven:
                                                dinfo.SetAmount(dinfo.Amount * 0.8f);
                                                return;
                                            case ItemGrade.Mysterious:
                                                dinfo.SetAmount(dinfo.Amount * 0.8f);
                                                return;
                                            case ItemGrade.Divine:
                                                dinfo.SetAmount(dinfo.Amount * 0.75f);
                                                return;
                                            case ItemGrade.Emperor:
                                                dinfo.SetAmount(dinfo.Amount * 0.75f);
                                                return;
                                            case ItemGrade.Dao:
                                                dinfo.SetAmount(dinfo.Amount * 0.5f);
                                                return;
                                        }
                                    }
                                    else
                                    {
                                        if (attackerWeaponGrade != null)
                                        {
                                            switch (attackerWeaponGrade.Grade)
                                            {
                                                case ItemGrade.Mortal:
                                                    return;
                                                case ItemGrade.Ordinary:
                                                    dinfo.SetAmount(dinfo.Amount * 1.25f);
                                                    return;
                                                case ItemGrade.Earth:
                                                    dinfo.SetAmount(dinfo.Amount * 1.5f);
                                                    return;
                                                case ItemGrade.Heaven:
                                                    dinfo.SetAmount(dinfo.Amount * 1.75f);
                                                    return;
                                                case ItemGrade.Mysterious:
                                                    dinfo.SetAmount(dinfo.Amount * 2f);
                                                    return;
                                                case ItemGrade.Divine:
                                                    dinfo.SetAmount(dinfo.Amount * 2.50f);
                                                    return;
                                                case ItemGrade.Emperor:
                                                    dinfo.SetAmount(dinfo.Amount * 3f);
                                                    return;
                                                case ItemGrade.Dao:
                                                    dinfo.SetAmount(dinfo.Amount * 3.5f);
                                                    return;
                                            }
                                        }
                                    }
                                }
                                
                            }

                        }                        
                    }
                }
                else
                {
                    if (attacker.RaceProps.IsMechanoid || attacker.RaceProps.Animal || attacker.RaceProps.IsAnomalyEntity)
                    {
                        return;
                    }
                    if (dinfo.HitPart != null)
                    {
                        if (!Cultivation_Utility.HaveCultivation(attacker))
                        {
                            CompItemGrade attackerWeaponGrade = attacker?.equipment?.Primary?.TryGetComp<CompItemGrade>();
                            if (attackerWeaponGrade != null)
                            {
                                switch (attackerWeaponGrade.Grade)
                                {
                                    case ItemGrade.Mortal:
                                        return;
                                    case ItemGrade.Ordinary:
                                        dinfo.SetAmount(dinfo.Amount * 1.1f);
                                        return;
                                    case ItemGrade.Earth:
                                        dinfo.SetAmount(dinfo.Amount * 1.25f);
                                        return;
                                    case ItemGrade.Heaven:
                                        dinfo.SetAmount(dinfo.Amount * 1.5f);
                                        return;
                                    case ItemGrade.Mysterious:
                                        dinfo.SetAmount(dinfo.Amount * 1.75f);
                                        return;
                                    case ItemGrade.Divine:
                                        dinfo.SetAmount(dinfo.Amount * 2.00f);
                                        return;
                                    case ItemGrade.Emperor:
                                        dinfo.SetAmount(dinfo.Amount * 2.25f);
                                        return;
                                    case ItemGrade.Dao:
                                        dinfo.SetAmount(dinfo.Amount * 2.50f);
                                        return;
                                }
                            }
                        }
                        if (Cultivation_Utility.HaveCultivation(attacker))
                        {
                            CompItemGrade attackerWeaponGrade = attacker?.equipment?.Primary?.TryGetComp<CompItemGrade>();
                            if (attackerWeaponGrade != null)
                            {
                                switch (attackerWeaponGrade.Grade)
                                {
                                    case ItemGrade.Mortal:
                                        return;
                                    case ItemGrade.Ordinary:
                                        dinfo.SetAmount(dinfo.Amount * 1.25f);
                                        return;
                                    case ItemGrade.Earth:
                                        dinfo.SetAmount(dinfo.Amount * 1.5f);
                                        return;
                                    case ItemGrade.Heaven:
                                        dinfo.SetAmount(dinfo.Amount * 1.75f);
                                        return;
                                    case ItemGrade.Mysterious:
                                        dinfo.SetAmount(dinfo.Amount * 2f);
                                        return;
                                    case ItemGrade.Divine:
                                        dinfo.SetAmount(dinfo.Amount * 2.50f);
                                        return;
                                    case ItemGrade.Emperor:
                                        dinfo.SetAmount(dinfo.Amount * 3f);
                                        return;
                                    case ItemGrade.Dao:
                                        dinfo.SetAmount(dinfo.Amount * 3.5f);
                                        return;
                                }
                            }
                        }
                    }
                }

            }
            *//*else if ((dinfo.Instigator is Building_Turret building_Turret)
            {
                if (dinfo.Instigator.def.tradeTags.Contains("Cultivator_Building"))
                {
                    dinfo.SetAmount(dinfo.Amount * Rand.Range(0.25f, 0.5f));
                }
                else
                {
                    IEnumerable<Apparel> wornApparel = __instance?.apparel?.WornApparel;
                    if (wornApparel.EnumerableNullOrEmpty())
                    {
                        return;
                    }
                    foreach (var item in wornApparel)
                    {   
                        if (item == null)
                        {
                            continue;
                        }
                        if (dinfo.HitPart != null && item.def.apparel.CoversBodyPart(dinfo.HitPart))
                        {
                            CompItemGrade apparelGrade = item?.TryGetComp<CompItemGrade>();
                            if (apparelGrade == null)
                            {
                                return;
                            }
                            switch (apparelGrade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    return;
                                case ItemGrade.Ordinary:
                                    dinfo.SetAmount(dinfo.Amount * 0.8f);
                                    return;
                                case ItemGrade.Earth:
                                    dinfo.SetAmount(dinfo.Amount * 0.5f);
                                    return;
                                case ItemGrade.Heaven:
                                    dinfo.SetAmount(dinfo.Amount * 0.5f);
                                    return;
                                case ItemGrade.Mysterious:
                                    dinfo.SetAmount(dinfo.Amount * 0.5f);
                                    return;
                                case ItemGrade.Divine:
                                    dinfo.SetAmount(dinfo.Amount * 0.25f);
                                    return;
                                case ItemGrade.Emperor:
                                    dinfo.SetAmount(dinfo.Amount * 0.25f);
                                    return;
                                case ItemGrade.Dao:
                                    dinfo.SetAmount(dinfo.Amount * 0.10f);
                                    return;
                            }
                        }

                    }
                }

            }*//*
            else if (dinfo.Instigator is Building building_Turret)
            {
                if (!building_Turret.def.tradeTags.NullOrEmpty() && building_Turret.def.tradeTags.Contains("Cultivator_Building"))
                    {
                    dinfo.SetAmount(dinfo.Amount * Rand.Range(0.25f, 0.5f));
                }
                else
                {
                    if(!__instance.RaceProps.Humanlike)
                    {
                        return;
                    }
                    IEnumerable<Apparel> wornApparel = __instance?.apparel?.WornApparel;
                    if (wornApparel.EnumerableNullOrEmpty())
                    {
                        return;
                    }
                    foreach (var item in wornApparel)
                    {
                        if (item == null)
                        {
                            continue;
                        }
                        if (dinfo.HitPart != null && item.def.apparel.CoversBodyPart(dinfo.HitPart))
                        {
                            CompItemGrade apparelGrade = item?.TryGetComp<CompItemGrade>();
                            if (apparelGrade == null)
                            {
                                return;
                            }
                            switch (apparelGrade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    return;
                                case ItemGrade.Ordinary:
                                    dinfo.SetAmount(dinfo.Amount * 0.8f);
                                    return;
                                case ItemGrade.Earth:
                                    dinfo.SetAmount(dinfo.Amount * 0.5f);
                                    return;
                                case ItemGrade.Heaven:
                                    dinfo.SetAmount(dinfo.Amount * 0.5f);
                                    return;
                                case ItemGrade.Mysterious:
                                    dinfo.SetAmount(dinfo.Amount * 0.5f);
                                    return;
                                case ItemGrade.Divine:
                                    dinfo.SetAmount(dinfo.Amount * 0.25f);
                                    return;
                                case ItemGrade.Emperor:
                                    dinfo.SetAmount(dinfo.Amount * 0.25f);
                                    return;
                                case ItemGrade.Dao:
                                    dinfo.SetAmount(dinfo.Amount * 0.10f);
                                    return;
                            }
                        }

                    }
                }
            }
            else
            {
                if (!__instance.RaceProps.Humanlike)
                {
                    return;
                }
                IEnumerable<Apparel> wornApparel = __instance?.apparel.WornApparel;
                if(wornApparel.EnumerableNullOrEmpty())
                {
                    return;
                }
                foreach(var item in  wornApparel)
                {
                    if(item == null)
                    { 
                        continue; 
                    }
                    if(dinfo.HitPart != null && item.def.apparel.CoversBodyPart(dinfo.HitPart))
                    {
                        CompItemGrade apparelGrade = item?.TryGetComp<CompItemGrade>();
                        if (apparelGrade == null)
                        {
                            return;
                        }
                        switch (apparelGrade.Grade)
                        {
                            case ItemGrade.Mortal:
                                return;
                            case ItemGrade.Ordinary:
                                dinfo.SetAmount(dinfo.Amount * 0.8f);
                                return;
                            case ItemGrade.Earth:
                                dinfo.SetAmount(dinfo.Amount * 0.5f);
                                return;
                            case ItemGrade.Heaven:
                                dinfo.SetAmount(dinfo.Amount * 0.5f);
                                return;
                            case ItemGrade.Mysterious:
                                dinfo.SetAmount(dinfo.Amount * 0.5f);
                                return;
                            case ItemGrade.Divine:
                                dinfo.SetAmount(dinfo.Amount * 0.25f);
                                return;
                            case ItemGrade.Emperor:
                                dinfo.SetAmount(dinfo.Amount * 0.25f);
                                return;
                            case ItemGrade.Dao:
                                dinfo.SetAmount(dinfo.Amount * 0.10f);
                                return;
                        }
                    }
                }
            }
        }
        private static bool IsCoreShapingOrAbove(Hediff h)
        {
            if (h.def.tags.Contains("CTR_CoreShapingOrAbove"))
            {
                return true;
            }
            return false;
        }
        private static bool IsSaintRealmOrAbove(Hediff h)
        {
            if (h.def.tags.Contains("CTR_SaintRealmOrAbove"))
            {
                return true;
            }
            return false;
        }
    }*/
}
