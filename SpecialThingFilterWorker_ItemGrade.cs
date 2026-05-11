using Verse;

namespace CultivatorOfTheRim;

public class SpecialThingFilterWorker_ItemGrade : SpecialThingFilterWorker
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade > ItemGrade.Mortal;
    }

    public override bool CanEverMatch(ThingDef def)
    {
        if (def.HasComp<CompItemGrade>())
        {
            return true;
        }
        return false;
    }
}

public class SpecialThingFilterWorker_OrdinaryGrade : SpecialThingFilterWorker_ItemGrade
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade == ItemGrade.Ordinary;
    }
}

public class SpecialThingFilterWorker_EarthGrade : SpecialThingFilterWorker_ItemGrade
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade == ItemGrade.Earth;
    }
}

public class SpecialThingFilterWorker_HeavenGrade : SpecialThingFilterWorker_ItemGrade
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade == ItemGrade.Heaven;
    }
}

public class SpecialThingFilterWorker_MysteriousGrade : SpecialThingFilterWorker_ItemGrade
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade == ItemGrade.Mysterious;
    }
}

public class SpecialThingFilterWorker_DivineGrade : SpecialThingFilterWorker_ItemGrade
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade == ItemGrade.Divine;
    }
}

public class SpecialThingFilterWorker_EmperorGrade : SpecialThingFilterWorker_ItemGrade
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade == ItemGrade.Emperor;
    }
}

public class SpecialThingFilterWorker_DaoGrade : SpecialThingFilterWorker_ItemGrade
{
    public override bool Matches(Thing t)
    {
        return t.TryGetGrade(out var grade) && grade == ItemGrade.Dao;
    }
}