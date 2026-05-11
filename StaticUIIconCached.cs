using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim;

[StaticConstructorOnStartup]
public static class StaticUIIconCached
{
    public static Dictionary<string,Texture2D> iconCache = new Dictionary<string, Texture2D>();
    
    
}