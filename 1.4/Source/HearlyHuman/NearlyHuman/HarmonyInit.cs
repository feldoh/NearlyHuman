using HarmonyLib;
using Verse;

namespace NearlyHuman
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        static Core()
        {
            new Harmony("NearlyHuman.Mod").PatchAll();
        }
    }
}
