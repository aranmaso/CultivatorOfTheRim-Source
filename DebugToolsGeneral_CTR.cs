using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace CultivatorOfTheRim;

public static class DebugToolsGeneral_CTR
{
    
    [DebugAction("Cultivator Of The Rim", "Increase Grade", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
    private static void IncreaseGrade()
    {
        try
        {
            foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
            {
                if (item is Apparel)
                {
                    CompItemGrade grade = item.TryGetComp<CompItemGrade>();
                    if (grade != null)
                    {
                        if(grade.Grade == ItemGrade.Dao) continue;
                        int num = (int)grade.Grade;
                        grade.SetGrade((ItemGrade)num + 1);
                    }
                }
                    
            }
                
        }
        catch (Exception ex)
        {
                
        }
    }
    [DebugAction("Cultivator Of The Rim", "Decrease Grade", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
    private static void DecreaseGrade()
    {
        try
        {
            foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
            {
                if (item is Apparel)
                {
                    CompItemGrade grade = item.TryGetComp<CompItemGrade>();
                    if (grade != null)
                    {
                        if(grade.Grade == ItemGrade.Mortal) continue;
                        int num = (int)grade.Grade;
                        grade.SetGrade((ItemGrade)num - 1);
                    }
                }
                    
            }
                
        }
        catch (Exception ex)
        {
                
        }
    }
}