using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class Hediff_DynamicStage : HediffWithComps
    {
        public HediffStage cachedCurStage;
        public override HediffStage CurStage
        {
            get
            {
                if (cachedCurStage == null)
                {
                    cachedCurStage = new HediffStage();
                }
                return cachedCurStage;
            }
        }
        public HediffStageData hediffStageData;

        public string CurStageLabel = "";

        public bool refreshChecked = false;

        /*public Dictionary<StatDef, float> statOffsetsCached = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> statFactorsCached = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef,float> capModFactorCached = new Dictionary<PawnCapacityDef, float>();

        public Dictionary<PawnCapacityDef,float> capModOffsetCached = new Dictionary<PawnCapacityDef, float>();*/
        
        public int rebirthCount = 0;
        public override void ExposeData()
        {
            base.ExposeData();
            /*Scribe_Collections.Look(ref statOffsetsCached, "statOffsetsCached", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref statFactorsCached, "statFactorsCached", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref capModFactorCached, "capModFactorCached", LookMode.Def,LookMode.Value);
            Scribe_Collections.Look(ref capModOffsetCached, "capModOffsetCached", LookMode.Def,LookMode.Value);*/
            Scribe_Values.Look(ref rebirthCount, "rebirthCount");
            Scribe_Deep.Look(ref hediffStageData, "hediffStageData");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                RefreshStage();
            }
        }
        public override string LabelInBrackets => base.LabelInBrackets + $"x{rebirthCount}";
        public override void PostMake()
        {
            base.PostMake();
            if (hediffStageData == null)
            {
                hediffStageData = new HediffStageData();
            }
            cachedCurStage = new HediffStage();
        }
        public override bool Visible
        {
            get
            {
                if (cachedCurStage != null)
                {
                    return cachedCurStage.becomeVisible;
                }
                return true;
            }
        }
        public void AddOrEditToStat(StatModifier statMod, bool addToOffset, bool addToFactor, bool negativeBenefit)
        {
            if (addToOffset)
            {
                if (cachedCurStage.statOffsets.NullOrEmpty()) cachedCurStage.statOffsets = new List<StatModifier>();
                if (cachedCurStage.statOffsets.Any(x => x.stat == statMod.stat))
                {
                    EditStatModValue(statMod.stat, statMod.value, addToOffset, addToFactor);
                }
                else
                {
                    AddStatModToStage(statMod, addToOffset, addToFactor);
                }
            }
            if (addToFactor)
            {
                if (cachedCurStage.statFactors.NullOrEmpty()) cachedCurStage.statFactors = new List<StatModifier>();
                if (cachedCurStage.statFactors.Any(x => x.stat == statMod.stat))
                {
                    EditStatModValue(statMod.stat, statMod.value, addToOffset, addToFactor,negativeBenefitFactor: negativeBenefit);
                }
                else
                {
                    AddStatModToStage(statMod, addToOffset, addToFactor,negativeBenefitFactor: negativeBenefit);
                }
            }
        }

        public void AddOrEditToCapMod(PawnCapacityModifier capMod)
        {
            if(cachedCurStage.capMods.NullOrEmpty()) cachedCurStage.capMods = new List<PawnCapacityModifier>();
            if (cachedCurStage.capMods.Any(x => x.capacity == capMod.capacity))
            {
                EditCapModValue(capMod);
            }
            else
            {
                AddCapModToStage(capMod);
            }
        }

        public void AddCapModToStage(PawnCapacityModifier capMod)
        {
            if (cachedCurStage.capMods.NullOrEmpty()) cachedCurStage.capMods = new List<PawnCapacityModifier>();
            if (capMod.postFactor < 1) capMod.postFactor += 1f;
            cachedCurStage.capMods.Add(capMod);
            /*capModFactorCached.Add(capMod.capacity,capMod.postFactor);
            capModOffsetCached.Add(capMod.capacity,capMod.offset);*/
            hediffStageData.capModsFactor.Add(capMod.capacity,capMod.postFactor);
            hediffStageData.capModsOffset.Add(capMod.capacity,capMod.offset);
        }

        public void EditCapModValue(PawnCapacityModifier capMod, bool isAdditive = true)
        {
            if (cachedCurStage.capMods.NullOrEmpty()) return;
            PawnCapacityModifier capacity = cachedCurStage.capMods.FirstOrDefault(x => x.capacity == capMod.capacity);            
            if (capMod.postFactor > 0)
            {
                float postFac = 0f;
                if (isAdditive)
                {
                    postFac += capMod.postFactor;
                }
                else
                {
                    postFac = capMod.postFactor;
                }                
                capacity.postFactor += postFac;
                //capModFactorCached[capacity.capacity] = capacity.postFactor;
                hediffStageData.capModsFactor[capacity.capacity] = capacity.postFactor;
            }
            if (capMod.offset > 0)
            {
                float offset = capacity.offset;
                if (isAdditive)
                {
                   offset += capMod.offset;
                }
                else
                {
                    offset = capMod.offset;
                }
                capacity.offset = offset;
                //capModOffsetCached[capacity.capacity] = capacity.offset;
                hediffStageData.capModsOffset[capacity.capacity] = capacity.offset;
            }

        }
        public void AddStatModToStage(StatModifier statMod, bool addToOffset, bool addToFactor, bool negativeBenefitFactor = false)
        {
            if (addToOffset)
            {
                if (cachedCurStage.statOffsets.NullOrEmpty()) cachedCurStage.statOffsets = new List<StatModifier>();
                cachedCurStage.statOffsets.Add(statMod);
                //statOffsetsCached.Add(statMod.stat,statMod.value);
                hediffStageData.statOffsets.Add(statMod.stat,statMod.value);
            }
            if (addToFactor)
            {
                if (cachedCurStage.statFactors.NullOrEmpty()) cachedCurStage.statFactors = new List<StatModifier>();
                if (negativeBenefitFactor)
                {
                    if(statMod.value < 0) statMod.value *= -1f;
                    statMod.value = 1f - statMod.value;
                }
                if (!negativeBenefitFactor && statMod.value < 1) statMod.value += 1f;
                cachedCurStage.statFactors.Add(statMod);
                //statFactorsCached.Add(statMod.stat,statMod.value);  
                hediffStageData.statFactors.Add(statMod.stat,statMod.value);
            }
        }

        public void RemoveStatModFromStage(StatModifier statMod, bool Offset, bool Factor)
        {
            if (Offset)
            {
                if (!cachedCurStage.statOffsets.NullOrEmpty())
                {
                    cachedCurStage.statOffsets.Remove(statMod);
                    //statOffsetsCached.Remove(statMod.stat);
                    hediffStageData.statOffsets.Remove(statMod.stat);
                }
            }
            if (Factor)
            {
                if (!cachedCurStage.statFactors.NullOrEmpty())
                {
                    cachedCurStage.statFactors.Remove(statMod);
                    //statFactorsCached.Remove(statMod.stat);
                }
            }
        }

        public void EditStatModValue(StatDef statDef, float value, bool offset, bool factor, bool isAdditive = true, bool negativeBenefitFactor = false)
        {
            if (offset)
            {
                if (cachedCurStage.statOffsets.NullOrEmpty()) return;
                float finalValue = cachedCurStage.statOffsets.FirstOrDefault(x => x.stat == statDef).value;
                if (isAdditive)
                {
                    finalValue += value;
                }
                else
                {
                     finalValue = value;
                }
                cachedCurStage.statOffsets.FirstOrDefault(x => x.stat == statDef).value = finalValue;
                //statOffsetsCached[statDef] = finalValue;
                hediffStageData.statOffsets[statDef] = finalValue;
            }
            if (factor)
            {
                if (cachedCurStage.statFactors.NullOrEmpty()) return;
                float finalValue = cachedCurStage.statFactors.FirstOrDefault(x => x.stat == statDef).value;
                if (isAdditive)
                {
                    finalValue += value;
                }
                else
                {
                    finalValue = value;
                }
                if (!negativeBenefitFactor && finalValue < 1) finalValue += 1f;
                cachedCurStage.statFactors.FirstOrDefault(x => x.stat == statDef).value = finalValue;
                //statFactorsCached[statDef] = finalValue;
                hediffStageData.statFactors[statDef] = finalValue;
            }
        }
        public void RefreshStage()
        {
            try
            {
                CurStage.label = CurStageLabel;                
                if (!hediffStageData.capModsFactor.NullOrEmpty())
                {
                    if (CurStage.capMods.NullOrEmpty())
                    {
                        CurStage.capMods = new List<PawnCapacityModifier>();
                    }
                    foreach (var item in hediffStageData.capModsFactor)
                    {
                        if (CurStage.capMods.FirstOrDefault(x => x.capacity == item.Key) != null)
                        {
                            CurStage.capMods.FirstOrDefault(x => x.capacity == item.Key).postFactor = item.Value;
                        }
                        else
                        {
                            PawnCapacityModifier capMod = new PawnCapacityModifier();
                            capMod.capacity = item.Key;
                            capMod.postFactor = item.Value;
                            CurStage.capMods.Add(capMod);
                        }
                    }
                }
                if (!hediffStageData.capModsOffset.NullOrEmpty())
                {
                    if (CurStage.capMods.NullOrEmpty())
                    {
                        CurStage.capMods = new List<PawnCapacityModifier>();
                    }
                    foreach (var item in hediffStageData.capModsOffset)
                    {
                        if (CurStage.capMods.FirstOrDefault(x => x.capacity == item.Key) != null)
                        {
                            CurStage.capMods.FirstOrDefault(x => x.capacity == item.Key).offset = item.Value;
                        }
                        else
                        {
                            PawnCapacityModifier capMod = new PawnCapacityModifier();
                            capMod.capacity = item.Key;
                            capMod.offset = item.Value;
                            CurStage.capMods.Add(capMod);
                        }
                    }
                }
                if (!hediffStageData.statOffsets.NullOrEmpty())
                {
                    if (CurStage.statOffsets.NullOrEmpty())
                    {
                        CurStage.statOffsets = new List<StatModifier>();
                    }
                    if (CurStage.statOffsets.NotNullOrEmpty())
                    {
                        CurStage.statOffsets.Clear();
                    }
                    foreach (var item in hediffStageData.statOffsets)
                    {
                        StatModifier statModifier = new StatModifier();
                        statModifier.stat = item.Key;
                        statModifier.value = item.Value;
                        CurStage.statOffsets.Add(statModifier);
                    }
                }
                if (!hediffStageData.statFactors.NullOrEmpty())
                {
                    if (CurStage.statFactors.NullOrEmpty())
                    {
                        CurStage.statFactors = new List<StatModifier>();
                    }
                    if (CurStage.statFactors.NotNullOrEmpty())
                    {
                        CurStage.statFactors.Clear();
                    }
                    foreach (var item in hediffStageData.statFactors)
                    {
                        StatModifier statModifier = new StatModifier();
                        statModifier.stat = item.Key;
                        statModifier.value = item.Value;
                        CurStage.statFactors.Add(statModifier);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Error in Refreshing stage for {pawn.LabelShort}. {ex}");
            }
        }
    }
}
