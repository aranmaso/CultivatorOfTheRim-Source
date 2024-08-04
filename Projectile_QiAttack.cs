using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class Projectile_QiAttack : Projectile
    {
        //public override int DamageAmount => Mathf.RoundToInt(def.projectile.GetDamageAmount(weaponDamageMultiplier) * launcher.GetStatValue(CTR_DefOf.TechniqueEfficiency));

        public override int DamageAmount
        {
            get
            {
                if(Launcher is Pawn pawnLauncher)
                {
                    if(Cultivation_Utility.HaveCultivation(pawnLauncher))
                    {
                        return Mathf.RoundToInt(def.projectile.GetDamageAmount(weaponDamageMultiplier) * launcher.GetStatValue(CTR_DefOf.TechniqueEfficiency));
                    }
                    else
                    {
                        return 0;
                    }
                }
                return base.DamageAmount;
            }
        }

    }
}
