using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("PreApplyDamage")]
    public class Thing_PreApplyDamage_WeaponGradeCheck
    {
        public static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Thing __instance)
        {
            if (__instance == null) return;
            if (__instance is Pawn) return;
            float damAmount = dinfo.Amount;
            float mul = 1f;
            ItemGrade grade = ItemGrade.Mortal;
            if (dinfo.Instigator != null)
            {
                if (dinfo.Instigator is Pawn attacker)
                {
                    if (attacker.RaceProps.Humanlike)
                    {
                        CompItemGrade itemGrade = attacker?.equipment?.Primary?.TryGetComp<CompItemGrade>();
                        if (itemGrade != null)
                        {
                            switch (itemGrade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    break;
                                case ItemGrade.Ordinary:
                                    mul = 1.1f;
                                    grade = ItemGrade.Ordinary;
                                    break;
                                case ItemGrade.Earth:
                                    mul = 1.25f;
                                    grade = ItemGrade.Earth;
                                    break;
                                case ItemGrade.Heaven:
                                    mul = 1.5f;
                                    grade = ItemGrade.Heaven;
                                    break;
                                case ItemGrade.Mysterious:
                                    mul = 1.75f;
                                    grade = ItemGrade.Mysterious;
                                    break;
                                case ItemGrade.Divine:
                                    mul = 2.00f;
                                    grade = ItemGrade.Divine;
                                    break;
                                case ItemGrade.Emperor:
                                    mul = 2.25f;
                                    grade = ItemGrade.Emperor;
                                    break;
                                case ItemGrade.Dao:
                                    mul = 2.5f;
                                    grade = ItemGrade.Dao;
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    if (dinfo.Instigator is Building_TurretGun turret)
                    {
                        CompItemGrade itemGrade = turret.gun.TryGetComp<CompItemGrade>();
                        if (itemGrade != null)
                        {
                            switch (itemGrade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    break;
                                case ItemGrade.Ordinary:
                                    mul = 1.1f;
                                    grade = ItemGrade.Ordinary;
                                    break;
                                case ItemGrade.Earth:
                                    mul = 1.25f;
                                    grade = ItemGrade.Earth;
                                    break;
                                case ItemGrade.Heaven:
                                    mul = 1.5f;
                                    grade = ItemGrade.Heaven;
                                    break;
                                case ItemGrade.Mysterious:
                                    mul = 1.75f;
                                    grade = ItemGrade.Mysterious;
                                    break;
                                case ItemGrade.Divine:
                                    mul = 2.00f;
                                    grade = ItemGrade.Divine;
                                    break;
                                case ItemGrade.Emperor:
                                    mul = 2.25f;
                                    grade = ItemGrade.Emperor;
                                    break;
                                case ItemGrade.Dao:
                                    mul = 2.5f;
                                    grade = ItemGrade.Dao;
                                    break;
                            }
                        }
                    }
                }
            }
            dinfo.SetAmount(damAmount * mul);
            if (CultivatorOfTheRimMod.settings.isShowDamageModifierGrade)
            {
                if (grade > ItemGrade.Mortal)
                {
                    if (__instance.Spawned)
                    {
                        if (Find.CameraDriver.InViewOf(__instance) && __instance.Map != null)
                        {
                            MoteMaker.ThrowText(__instance.DrawPos, __instance.MapHeld, $"x{mul}. {grade.ToString()}");
                        }
                    }
                }
            }
        }
    }
}
