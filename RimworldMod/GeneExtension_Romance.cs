using Verse;
using RimWorld;
using HarmonyLib;

namespace GeneExtension_Romance
{
    public class GeneExtension_Romance : DefModExtension
    {
        public float lovinGeneRomanceChanceFactor = 1f;
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
    public static class Patch_SecondaryRomanceChanceFactor
    {
        public static void Postfix(Pawn otherPawn, ref float __result, Pawn ___pawn)
        {
            if (!ModsConfig.BiotechActive) return;
            if (otherPawn == null) return;
            if (___pawn.genes == null || otherPawn.genes == null) return;

            float bonus = 1f;

            var genes = ___pawn.genes.GenesListForReading;

            for (int i = 0; i < genes.Count; i++)
            {
                var gene = genes[i];
                var ext = gene.def.GetModExtension<GeneExtension_Romance>();

                if (ext == null || ext.lovinGeneRomanceChanceFactor == 1f)
                    continue;

                if (otherPawn.genes.HasActiveGene(gene.def))
                {
                    Log.Message("대상 존재");
                    bonus *= ext.lovinGeneRomanceChanceFactor;
                }
            }

            __result *= bonus;
        }
    }
}