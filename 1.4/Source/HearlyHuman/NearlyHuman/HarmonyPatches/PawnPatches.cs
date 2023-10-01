using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NearlyHuman.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnStyleItemChooser), "WantsToUseStyle")]
    public static class PawnStyleItemChooserPatch
    {
        public static bool Prefix(ref bool __result, Pawn pawn, StyleItemDef styleItemDef, TattooType? tattooType = null)
        {
            if (pawn.def != NearlyHumanDefOf.NH_Packkin || styleItemDef != NearlyHumanDefOf.NH_PackkinBeard) return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateSkills")]
    public static class GenerateSkillsPatch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.def != NearlyHumanDefOf.NH_Packkin) return;
            SkillRecord skillRecord = pawn.skills.GetSkill(SkillDefOf.Melee);
            skillRecord.Level = Mathf.Min(20, skillRecord.Level + 4);
        }
    }

    [HarmonyPatch(typeof(BodyPartRecord), "Label", MethodType.Getter)]
    public static class BodyPartRecord_Label_Patch
    {
        public static Pawn curPawn;

        public static void Postfix(BodyPartRecord __instance, ref string __result)
        {
            if (curPawn == null) return;
            if (curPawn.def == NearlyHumanDefOf.NH_Fulcrum)
                __result = "NH.Robo".Translate() + __result;
        }
    }

    [HarmonyPatch(typeof(BodyPartRecord), "LabelCap", MethodType.Getter)]
    public static class BodyPartRecord_LabelCap_Patch
    {
        public static void Postfix(BodyPartRecord __instance, ref string __result)
        {
            if (BodyPartRecord_Label_Patch.curPawn == null) return;
            if (BodyPartRecord_Label_Patch.curPawn.def == NearlyHumanDefOf.NH_Fulcrum)
                __result = ("NH.Robo".Translate() + __result.UncapitalizeFirst()).CapitalizeFirst();
        }
    }


    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    public static class Pawn_HealthTracker_HealthTick_Patch
    {
        public static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            if (___pawn.Dead) return;

            if (___pawn.def != NearlyHumanDefOf.NH_Stargazer) return;
            for (var i = 0; i < 3; i++)
                if (___pawn.RaceProps.IsFlesh && ___pawn.IsHashIntervalTick(600) && (___pawn.needs.food == null || !___pawn.needs.food.Starving))
                {
                    var flag2 = false;
                    if (__instance.hediffSet.HasNaturallyHealingInjury())
                    {
                        var num3 = 8f;
                        if (___pawn.GetPosture() != 0)
                        {
                            num3 += 4f;
                            Building_Bed building_Bed = ___pawn.CurrentBed();
                            if (building_Bed != null) num3 += building_Bed.def.building.bed_healPerDay;
                        }

                        foreach (Hediff hediff3 in __instance.hediffSet.hediffs)
                        {
                            HediffStage curStage = hediff3.CurStage;
                            if (curStage != null && curStage.naturalHealingFactor != -1f) num3 *= curStage.naturalHealingFactor;
                        }

                        List<Hediff_Injury> healableHediffs = new();
                        __instance.hediffSet.GetHediffs(ref healableHediffs, injury => injury.CanHealNaturally());
                        healableHediffs.RandomElement().Heal(num3 * ___pawn.HealthScale * 0.01f * ___pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                        flag2 = true;
                    }

                    if (__instance.hediffSet.HasTendedAndHealingInjury() && ___pawn.needs.food is not { Starving: true })
                    {
                        List<Hediff_Injury> healableHediffs = new();
                        __instance.hediffSet.GetHediffs(ref healableHediffs, injury => injury.CanHealNaturally());
                        Hediff_Injury hediff_Injury = healableHediffs.RandomElement();
                        var tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
                        var num4 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
                        hediff_Injury.Heal(8f * num4 * ___pawn.HealthScale * 0.01f * ___pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                        flag2 = true;
                    }

                    if (flag2 && !__instance.HasHediffsNeedingTendByPlayer() && !HealthAIUtility.ShouldSeekMedicalRest(___pawn) &&
                        !__instance.hediffSet.HasTendedAndHealingInjury() && PawnUtility.ShouldSendNotificationAbout(___pawn))
                        Messages.Message("MessageFullyHealed".Translate(___pawn.LabelCap, ___pawn), ___pawn, MessageTypeDefOf.PositiveEvent);
                }
        }
    }
}
