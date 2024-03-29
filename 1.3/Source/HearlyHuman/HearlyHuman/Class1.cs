﻿using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static RimWorld.FleshTypeDef;

namespace HearlyHuman
{
	[DefOf]
	public static class NearlyHumanDefOf
	{
		public static ThingDef NH_Packkin;
		public static BeardDef NH_PackkinBeard;
        public static ThingDef NH_Waterborne;
        public static ThingDef NH_Fulcrum;
        public static ThingDef NH_Stargazer;
    }
    [StaticConstructorOnStartup]
    public static class Core
    {
        static Core()
        {
            new Harmony("HearlyHuman.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(PawnStyleItemChooser), "WantsToUseStyle")]
    public static class PawnStyleItemChooserPatch
    {
        public static bool Prefix(ref bool __result, Pawn pawn, StyleItemDef styleItemDef, TattooType? tattooType = null)
        {
			if (pawn.def == NearlyHumanDefOf.NH_Packkin && styleItemDef == NearlyHumanDefOf.NH_PackkinBeard)
            {
				__result = true;
				return false;
            }
			return true;
		}
	}

    [HarmonyPatch(typeof(PawnGenerator), "GenerateSkills")]
    public static class GenerateSkillsPatch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.def == NearlyHumanDefOf.NH_Packkin)
            {
                var skillRecord = pawn.skills.GetSkill(SkillDefOf.Melee);
                skillRecord.Level = Mathf.Min(20, skillRecord.Level + 4);
            }
        }
    }

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

    [HarmonyPatch(typeof(BodyPartRecord), "Label", MethodType.Getter)]
    public static class BodyPartRecord_Label_Patch
    {
        public static Pawn curPawn;
        public static void Postfix(BodyPartRecord __instance, ref string __result)
        {
            if (curPawn != null)
            {
                if (curPawn.def == NearlyHumanDefOf.NH_Fulcrum)
                {
                    __result = "NH.Robo".Translate() + __result;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(BodyPartRecord), "LabelCap", MethodType.Getter)]
    public static class BodyPartRecord_LabelCap_Patch
    {
        public static void Postfix(BodyPartRecord __instance, ref string __result)
        {
            if (BodyPartRecord_Label_Patch.curPawn != null)
            {
                if (BodyPartRecord_Label_Patch.curPawn.def == NearlyHumanDefOf.NH_Fulcrum)
                {
                    __result = ("NH.Robo".Translate() + __result.UncapitalizeFirst()).CapitalizeFirst();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch("CostToMoveIntoCell")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(IntVec3) })]
    public static class Pawn_PathFollower_CostToMoveIntoCell_Patch
    {
        public static void Postfix(Pawn pawn, IntVec3 c, ref int __result)
        {
            if (pawn.Map != null && pawn.def == NearlyHumanDefOf.NH_Waterborne)
            {
                TerrainDef terrainDef = pawn.Map.terrainGrid.TerrainAt(c);
                if (terrainDef != null && terrainDef.IsWater)
                {
                    __result /= 6;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.FindPath), new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode), typeof(PathFinderCostTuning) })]
    public static class PathFinder_FindPath_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var found = false;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (!found && codes[i].opcode == OpCodes.Stloc_S && codes[i].operand is LocalBuilder lb && lb.LocalIndex == 53)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 12);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 43);
                    yield return new CodeInstruction(OpCodes.Ldelem_Ref);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 46);
                    yield return new CodeInstruction(OpCodes.Call, typeof(PathFinder_FindPath_Patch).GetMethod(nameof(PathFinder_FindPath_Patch.PathCostChangeIfNeeded)));
                    yield return new CodeInstruction(OpCodes.Stloc_S, 46);
                }
            }
            if (!found)
            {
                Log.Error("PathFinder.FindPath Transpiler failed. The code won't work.");
            }
        }

        static public int PathCostChangeIfNeeded(Pawn pawn, TerrainDef def, int cost)
        {
            if (pawn.def == NearlyHumanDefOf.NH_Waterborne)
            {
                if (def != null && def.IsWater)
                {
                    cost /= 3;
                }
            }
            return cost;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    public static class Pawn_HealthTracker_HealthTick_Patch
    {
        public static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            var pawn = ___pawn;
            if (pawn.Dead)
            {
                return;
            }
            if (pawn.def == NearlyHumanDefOf.NH_Stargazer)
            {
                for (var i = 0; i < 3; i++)
                {
                    if (pawn.RaceProps.IsFlesh && pawn.IsHashIntervalTick(600) && (pawn.needs.food == null || !pawn.needs.food.Starving))
                    {
                        bool flag2 = false;
                        if (__instance.hediffSet.HasNaturallyHealingInjury())
                        {
                            float num3 = 8f;
                            if (pawn.GetPosture() != 0)
                            {
                                num3 += 4f;
                                Building_Bed building_Bed = pawn.CurrentBed();
                                if (building_Bed != null)
                                {
                                    num3 += building_Bed.def.building.bed_healPerDay;
                                }
                            }
                            foreach (Hediff hediff3 in __instance.hediffSet.hediffs)
                            {
                                HediffStage curStage = hediff3.CurStage;
                                if (curStage != null && curStage.naturalHealingFactor != -1f)
                                {
                                    num3 *= curStage.naturalHealingFactor;
                                }
                            }
                            (from x in __instance.hediffSet.GetHediffs<Hediff_Injury>()
                             where x.CanHealNaturally()
                             select x).RandomElement().Heal(num3 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                            flag2 = true;
                        }
                        if (__instance.hediffSet.HasTendedAndHealingInjury() && (pawn.needs.food == null || !pawn.needs.food.Starving))
                        {
                            Hediff_Injury hediff_Injury = (from x in __instance.hediffSet.GetHediffs<Hediff_Injury>()
                                                           where x.CanHealFromTending()
                                                           select x).RandomElement();
                            float tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
                            float num4 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
                            hediff_Injury.Heal(8f * num4 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                            flag2 = true;
                        }
                        if (flag2 && !__instance.HasHediffsNeedingTendByPlayer() && !HealthAIUtility.ShouldSeekMedicalRest(pawn) && !__instance.hediffSet.HasTendedAndHealingInjury() && PawnUtility.ShouldSendNotificationAbout(pawn))
                        {
                            Messages.Message("MessageFullyHealed".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }
        }
    }
}
