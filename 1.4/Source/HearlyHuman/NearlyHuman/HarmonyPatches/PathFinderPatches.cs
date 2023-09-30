using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace NearlyHuman.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch("CostToMoveIntoCell")]
    [HarmonyPatch(new[] { typeof(Pawn), typeof(IntVec3) })]
    public static class Pawn_PathFollower_CostToMoveIntoCell_Patch
    {
        public static void Postfix(Pawn pawn, IntVec3 c, ref int __result)
        {
            if (pawn.Map != null && pawn.def == NearlyHumanDefOf.NH_Waterborne)
            {
                TerrainDef terrainDef = pawn.Map.terrainGrid.TerrainAt(c);
                if (terrainDef != null && terrainDef.IsWater) __result /= 6;
            }
        }
    }

    [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.FindPath), typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode),
        typeof(PathFinderCostTuning))]
    public static class PathFinder_FindPath_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var found = false;
            foreach (CodeInstruction t in codes)
            {
                yield return t;
                if (found || t.opcode != OpCodes.Stloc_S || !(t.operand is LocalBuilder lb) || lb.LocalIndex != 53) continue;
                found = true;
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_S, 12);
                yield return new CodeInstruction(OpCodes.Ldloc_S, 43);
                yield return new CodeInstruction(OpCodes.Ldelem_Ref);
                yield return new CodeInstruction(OpCodes.Ldloc_S, 46);
                yield return new CodeInstruction(OpCodes.Call, typeof(PathFinder_FindPath_Patch).GetMethod(nameof(PathCostChangeIfNeeded)));
                yield return new CodeInstruction(OpCodes.Stloc_S, 46);
            }

            if (!found) Log.Error("PathFinder.FindPath Transpiler failed. The code won't work.");
        }

        public static int PathCostChangeIfNeeded(Pawn pawn, TerrainDef def, int cost)
        {
            if (pawn.def != NearlyHumanDefOf.NH_Waterborne) return cost;
            if (def != null && def.IsWater)
                cost /= 3;

            return cost;
        }
    }
}
