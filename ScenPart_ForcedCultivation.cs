using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class ScenPart_ForcedCultivation : ScenPart_PawnModifier
    {
        private Dictionary<HediffDef, float> hediffs = new Dictionary<HediffDef, float>();

        private Dictionary<int, string> realmRange = new Dictionary<int, string>()
        {
            { 1, "Mortal Transforming"},
            { 4, "Qi Cultivating"},
            { 13, "Heaven Ascending"},
            { 18, "Domain Ascending" }
        };

        private Dictionary<int, string> realmRangeMax = new Dictionary<int, string>()
        {
            { 3, "Mortal Transforming"},
            { 12, "Qi Cultivating"},
            { 17, "Heaven Ascending"},
            { 19, "Domain Ascending" }
        };

        private IntRange stageRange;

        public int minRange;

        public int maxRange;

        private string selectedRangeString;
        private string selectedRangeString2;
        public bool isAffectingAnimal;
        public bool isAffectingMech;
        public bool isOnlyAffectAnimal;
        public bool isOnlyAffectMech;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f + 200f);
            Rect rect = new Rect(scenPartRect.x, scenPartRect.y + 5f, scenPartRect.width, scenPartRect.height / 6f - 20f);
            Rect rect2 = new Rect(scenPartRect.x, scenPartRect.y + rect.height + 5f, scenPartRect.width, rect.height);
            if (Widgets.ButtonText(rect, "min realm: " + realmRange[minRange]))
            {
                FloatMenuUtility.MakeMenu(realmRange.Keys, (int x) => realmRange[x], (int x) => delegate
                {
                    minRange = x;
                    //GetPossibleHediff(x, selectedRangeString, isMaxList:false, isMinList:true);
                    //Log.Message("num = " + x);
                    selectedRangeString = realmRange[x];
                    UpdateHediffs();
                });
            }
            if (Widgets.ButtonText(rect2, "max realm: " + realmRangeMax[maxRange]))
            {
                FloatMenuUtility.MakeMenu(realmRangeMax.Keys, (int x) => realmRangeMax[x], (int x) => delegate
                {
                    maxRange = x;
                    //GetPossibleHediff(x, selectedRangeString2, isMaxList:true, isMinList:false);
                    //Log.Message("num = " + x);
                    selectedRangeString2 = realmRangeMax[x];
                    UpdateHediffs();
                });
            }
            Rect rect3 = new Rect(scenPartRect.x, scenPartRect.y + 60f, scenPartRect.width, 40f);
            Widgets.IntRange(rect3, listing.CurHeight.GetHashCode(), ref stageRange, 1, 10, "ConfigurableRealmStage");
            Rect rect4 = new Rect(scenPartRect.x, rect3.y + 25f, scenPartRect.width, scenPartRect.height / 4f);
            Widgets.CheckboxLabeled(rect4, "affect animal", ref isAffectingAnimal);
            Rect rect5 = new Rect(scenPartRect.x, rect4.y + 25f, scenPartRect.width, scenPartRect.height / 4f);
            Widgets.CheckboxLabeled(rect5, "only affect animal", ref isOnlyAffectAnimal);
            Rect rect6 = new Rect(scenPartRect.x, rect5.y + 25f, scenPartRect.width, scenPartRect.height / 4f);
            Widgets.CheckboxLabeled(rect6, "affect mech", ref isAffectingMech);
            Rect rect7 = new Rect(scenPartRect.x, rect6.y + 25f, scenPartRect.width, scenPartRect.height / 4f);
            Widgets.CheckboxLabeled(rect7, "only affect mech", ref isOnlyAffectMech);
            Rect rect8 = new Rect(scenPartRect.x, rect7.y + 35f, scenPartRect.width, scenPartRect.height / 4f);
            DoPawnModifierEditInterface(rect8.BottomPartPixels(ScenPart.RowHeight * 2f));
        }
        private IEnumerable<int> PossibleRealm()
        {
            return realmRange.Keys;
        }

        public void UpdateHediffs()
        {
            hediffs.Clear();
            foreach (var item in Cultivation_Utility.realmListRanking)
            {
                if (item.Key >= minRange && item.Key <= maxRange)
                {
                    hediffs.SetOrAdd(item.Value, Cultivation_Utility.RealmList[item.Value]);
                }
            }
            /*foreach (var item in minList)
            {
                if (hediffs.Contains(item))
                {
                    continue;
                }
                hediffs.SetOrAdd(item.Key, item.Value);
            }
            foreach (var item in maxList)
            {
                if (hediffs.Contains(item))
                {
                    continue;
                }
                hediffs.SetOrAdd(item.Key, item.Value);
            }*/
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hediffs, "hediffs", keyLookMode: LookMode.Def, valueLookMode: LookMode.Value);
            Scribe_Values.Look(ref selectedRangeString, "selectedRangeString", null);
            Scribe_Values.Look(ref selectedRangeString2, "selectedRangeString2", null);
            Scribe_Values.Look(ref stageRange, "stageRange");
            Scribe_Values.Look(ref isAffectingAnimal, "isAffectingAnimal", false);
            Scribe_Values.Look(ref isOnlyAffectAnimal, "isOnlyAffectAnimal", false);
            Scribe_Values.Look(ref isAffectingMech, "isAffectingMech", false);
            Scribe_Values.Look(ref isOnlyAffectMech, "isOnlyAffectMech", false);
            Scribe_Values.Look(ref minRange, "minRange", 0);
            Scribe_Values.Look(ref maxRange, "maxRange", 0);
        }

        public override string Summary(Scenario scen)
        {
            string animalText = null;
            if (isAffectingAnimal)
            {
                animalText = ",animal";
            }
            string mechText = null;
            if (isAffectingMech)
            {
                mechText = ",mech";
            }
            string text = "ScenPart_PawnsHaveCultivation".Translate(context.ToStringHuman(), animalText, mechText, chance.ToStringPercent(), selectedRangeString, selectedRangeString2).CapitalizeFirst();

            /*if(isAffectingAnimal)
            {
                text = "ScenPart_PawnsHaveCultivationAnimal".Translate(context.ToString(), chance.ToStringPercent(), selectedRangeString, selectedRangeString2).CapitalizeFirst();
            }
            if(isOnlyAffectAnimal)
            {
                text = "ScenPart_PawnsHaveCultivationAnimalOnly".Translate(context.ToString(), chance.ToStringPercent(), selectedRangeString, selectedRangeString2).CapitalizeFirst();
            }*/
            return text;
        }

        public override void Randomize()
        {
            base.Randomize();
            /*if(hediffs.NullOrEmpty())
            {
                hediffs = Cultivation_Utility.RealmListMortalTransforming;
            } */
            minRange = 1;

            maxRange = 12;

            selectedRangeString = realmRange[minRange];
            selectedRangeString2 = realmRangeMax[maxRange];
            stageRange.min = 1;
            stageRange.max = 9;
            isAffectingAnimal = false;
            isOnlyAffectAnimal = false;
            UpdateHediffs();
        }

        public override void Notify_NewPawnGenerating(Pawn pawn, PawnGenerationContext context)
        {
            if (!isAffectingAnimal && (pawn.RaceProps.Animal || pawn.RaceProps.IsAnomalyEntity))
            {
                return;
            }            
            if(isOnlyAffectAnimal && !(pawn.RaceProps.Animal || pawn.RaceProps.IsAnomalyEntity))
            {
                return;
            }
            if(!isAffectingMech && pawn.RaceProps.IsMechanoid)
            {
                return;
            }
            if(isOnlyAffectMech && !pawn.RaceProps.IsMechanoid)
            {
                return;
            }
            if (this.context.Includes(context) && (!hideOffMap || context != PawnGenerationContext.PlayerStarter) && Rand.Chance(chance))
            {
                ModifyNewPawn(pawn);
            }
        }

        public override bool TryMerge(ScenPart other)
        {
            if (other is ScenPart_ForcedCultivation scenPart_ForcedCultivation && hediffs == scenPart_ForcedCultivation.hediffs)
            {                
                chance = GenMath.ChanceEitherHappens(chance, scenPart_ForcedCultivation.chance);
                return true;
            }
            return false;
        }

        public override bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
        {
            if (!base.AllowPlayerStartingPawn(pawn, tryingToRedress, req))
            {
                return false;
            }
            return true;
        }
        protected override void ModifyNewPawn(Pawn p)
        {
            AddHediff(p);
        }

        protected override void ModifyHideOffMapStartingPawnPostMapGenerate(Pawn p)
        {
            AddHediff(p);
        }
        private void AddHediff(Pawn p)
        {            
            Hediff hediff = HediffMaker.MakeHediff(hediffs.RandomElementByWeight(x => x.Value).Key, p);
            Hediff hediff2 = HediffMaker.MakeHediff(CTR_DefOf.CTR_BreakthroughCounter,p);
            int selectedStage = stageRange.RandomInRange;
            if (selectedStage > hediff.def.stages.Count())
            {
                selectedStage = hediff.def.stages.Count();
            }
            float sev = 0f;
            sev = hediff.def.stages[selectedStage - 1].minSeverity;
            hediff.Severity = sev;
            p.health.AddHediff(hediff);
            p.health.AddHediff(hediff2);
        }


        public override int GetHashCode()
        {
            int num = base.GetHashCode();
            if(hediffs != null)
            {
                num ^= hediffs.GetHashCode();
            }
            num ^= minRange.GetHashCode();
            return num;
        }
    }

}
