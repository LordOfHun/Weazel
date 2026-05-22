using RimWorld;

namespace WeazelStat
{
    [DefOf]
    public static class WeazelStatDefOf
    {
        public static StatDef Weazel_MassCarryCapacity;

        static WeazelStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WeazelStatDefOf));
        }
    }
}