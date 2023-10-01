using HarmonyLib;
using RimWorld;
using Verse;

namespace NearlyHuman.HarmonyPatches
{
    [HarmonyPatch(typeof(Bill_Medical), "Label", MethodType.Getter)]
    public static class Bill_Medical_Label_Patch
    {
        public static void Prefix(Bill_Medical __instance)
        {
            BodyPartRecord_Label_Patch.curPawn = __instance.GiverPawn;
        }

        public static void Postfix()
        {
            BodyPartRecord_Label_Patch.curPawn = null;
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "GenerateSurgeryOption")]
    public static class HealthCardUtility_GenerateSurgeryOption_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            BodyPartRecord_Label_Patch.curPawn = pawn;
        }

        public static void Postfix()
        {
            BodyPartRecord_Label_Patch.curPawn = null;
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "DrawHediffListing")]
    public static class HealthCardUtility_DrawHediffListing_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            BodyPartRecord_Label_Patch.curPawn = pawn;
        }

        public static void Postfix()
        {
            BodyPartRecord_Label_Patch.curPawn = null;
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "DrawPawnHealthCard")]
    public static class HealthCardUtility_DrawPawnHealthCard_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            BodyPartRecord_Label_Patch.curPawn = pawn;
        }

        public static void Postfix()
        {
            BodyPartRecord_Label_Patch.curPawn = null;
        }
    }

}
