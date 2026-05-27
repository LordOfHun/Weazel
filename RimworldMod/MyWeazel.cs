using HarmonyLib;
using Verse;
using System.Reflection;
using AlienRace;
using RimWorld;
using WeazelStat;

namespace MyWeazelMod // 본인의 네임스페이스 (중요하지 않지만 형식을 맞추는 게 좋음)
{
    // [StaticConstructorOnStartup] 이 어노테이션이 있어야 게임 로딩 시 자동으로 실행됩니다.
    [StaticConstructorOnStartup]
    public static class ModStartup
    {
        static ModStartup()
        {
            // 하모니 인스턴스 생성 (본인의 고유 ID를 넣으세요)
            var harmony = new Harmony("com.weazel.hideears.patch");
            
            // 현재 어셈블리(DLL) 내의 모든 [HarmonyPatch] 클래스를 찾아 적용합니다.
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // --- 기존에 작성하신 패치 클래스는 이 아래에 그대로 두시면 됩니다 ---
    [HarmonyPatch(typeof(AlienPartGenerator.BodyAddon), "CanDrawAddon", new Type[] { typeof(Verse.Pawn) })]
    public static class Patch_HideEars_CanDraw
    {
        // Postfix를 사용하여 원래 결과가 true이더라도 헬멧을 썼다면 false로 덮어씌웁니다.
        static void Postfix(AlienPartGenerator.BodyAddon __instance, Pawn pawn, ref bool __result)
        {
            // 1. 이미 다른 이유로 안 그려진다면(false) 굳이 체크할 필요 없음
            if (!__result) return;

            // 2. 기본 널 체크 및 종족 확인
            if (pawn == null || pawn.def?.defName != "Weazel")
                return;

            if (__instance.path == null || !__instance.path.Contains("Weazellike/Ear/Ears"))
                return;
                

            // 4. 헬멧 체크 로직
            if (pawn.apparel?.WornApparel != null)
            {
                var worn = pawn.apparel.WornApparel;
                for (int i = 0; i < worn.Count; i++)
                {
                    var app = worn[i].def?.apparel;
                    if (app?.bodyPartGroups != null)
                    {
                        if (app.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead))
                        {
                            // 헬멧을 썼으므로 결과를 false로 강제 변경
                            __result = false;
                            return;
                        }
                    }
                }
            }
        }
    }
    

    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new Type[] {typeof(Verse.Thing), typeof(Verse.Pawn), typeof(string), typeof(bool)},
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public static class Patch_PowerWeapon
    {
        static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            //1. 이미 다른 이유로 장착불가라면
            if (!__result) return;

            //2. 해당무기가 위젤무기라면
            if (thing.def.weaponTags != null && thing.def.weaponTags.Contains("WeazelWeapon") && pawn.def.defName != "Weazel")
            {
                __result = false;
                cantReason = "다음 종족만 착용 가능합니다. 'Weazel'";
                return;
                
            } else if (thing.def.weaponTags != null && thing.def.weaponTags.Contains("Weazel_PowerWeapon"))
            {
                __result = false;
                cantReason = "해당 장비가 필요합니다. 'Weazel_PowerArmor'";

                if (pawn.apparel?.WornApparel != null)
                {
                    var worn = pawn.apparel.WornApparel;
                    for (int i = 0; i < worn.Count; i++)
                    {
                        var app = worn[i].def?.apparel;
                        if (app != null && app.tags != null)
                        {
                            if (app.tags.Contains("Weazel_PowerArmor"))
                            {
                                __result = true;
                                cantReason = "";
                                break;
                            }
                        }
                    }
                }
            }
            
            if (thing.def.apparel != null && thing.def.apparel.tags.Contains("Weazel_Apparel") && pawn.def.defName != "Weazel")
            {
                __result = false;
                cantReason = "다음 종족만 착용 가능합니다. 'Weazel'";
                return;
            }
            else
            {
                __result = true;
                cantReason = "";
            }
        }
    }

    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    public static class Patch_Weazel_CapacityMass
    {
        [HarmonyPostfix]
        static void Postfix(Pawn p, ref float __result)
        {
            if(p == null) return;

            // 1. 장비에 붙은 커스텀 스탯(무게 추가) 합산
            // StatDefOf.Mass를 쓰지 않고 전용 StatDef를 하나 만듭니다.
            float bonus = p.GetStatValue(WeazelStatDefOf.Weazel_MassCarryCapacity);

            __result += bonus;
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
    public static class Patch_ApparelScoreGain_WeazelRestriction
    {
        public static bool Prefix(Pawn pawn, Apparel ap, ref float __result)
        {
            // 1. 옷 태그에 Weazel_Apparel이 있는지 확인
            bool isWeazelApparel = ap.def.apparel.tags?.Contains("Weazel_Apparel") ?? false;
            
            // 2. 폰이 위제트가 아닌지 확인
            bool isNotWeazel = pawn.def.defName != "Weazel";

            // 3. 위제트 전용 옷인데 일반인이 입으려 한다면?
            if (isWeazelApparel && isNotWeazel)
            {
                __result = -1000f; // 이득이 전혀 없다고 판단하게 함
                return false;      // 원본 코드 실행 안 함
            }

            // 반대의 경우(위제트인데 위제트 옷이 아닌 것을 못 입게 하려면)도 여기에 추가 가능
            
            return true; // 일반적인 상황에선 원본 코드 실행
        }
    }
}

