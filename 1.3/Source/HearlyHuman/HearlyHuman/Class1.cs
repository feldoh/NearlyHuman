using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace HearlyHuman
{
	[DefOf]
	public static class NearlyHumanDefOf
	{
		public static ThingDef NH_Packkin;
		public static BeardDef NH_PackkinBeard;

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
}
