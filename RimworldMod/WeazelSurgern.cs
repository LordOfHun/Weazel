using RimWorld;
using Verse;

namespace AddWeazelSurgern
{
    public class Recipe_ExtractEssence : Recipe_Surgery
    {
        // 수술 가능 여부 체크
        public override bool AvailableOnNow(Thing thing, BodyPartRecord? part = null)
        {
            if (thing is not Pawn pawn) return false;

            if (pawn.def.defName == "Weazel") return false;
            
            // 아기는 추출 불가 (Translate 설정 필요 또는 직접 문자열 입력)
            if (pawn.DevelopmentalStage.Baby()) return false;

            return base.AvailableOnNow(thing, part);
        }

        // 수술 실행 로직
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            // 0. 의사(billDoer)가 위즐 종족인지 확인
            if (billDoer == null || billDoer.def.defName != "Weazel")
            {
                // 의사가 위즐이 아니면 수술 중단 및 메시지 출력
                Messages.Message("이 수술은 위즐 종족만 수행할 수 있습니다.", MessageTypeDefOf.RejectInput);
                
                // 수술 재료를 돌려주고 수술 중단 (실패 처리와 비슷함)
                return;
            }

            // 1. 대상 검증 (포로 또는 노예만 가능)
            if (!pawn.IsPrisoner && !pawn.IsSlave)
            {
                Messages.Message("대상자가 포로나 노예가 아니거나, 종족이 위젤입니다.", MessageTypeDefOf.RejectInput);
                return;
            }

            // 2. 헤디프(상태이상) 정의 및 중복 체크
            // XML에 정의된 이름과 정확히 일치해야 합니다.
            HediffDef essenceHediff = HediffDef.Named("EssenceReplicating"); 
            if (essenceHediff == null) return;

            // 3. 수치 감소 (허기 및 휴식)
            if (pawn.needs != null)
            {
                if (pawn.needs.food != null) pawn.needs.food.CurLevelPercentage -= 0.5f;
                if (pawn.needs.rest != null) pawn.needs.rest.CurLevelPercentage -= 0.5f;
            }

            // 4. '추출됨' 디버프 부여
            pawn.health.AddHediff(essenceHediff);

            // 5. 결과물(에센스) 생성 및 배치
            ThingDef essenceDef = ThingDef.Named("Weazel_Essence");
            if (essenceDef != null)
            {
                Thing essence = ThingMaker.MakeThing(essenceDef);
                essence.stackCount = 60;
                GenPlace.TryPlaceThing(essence, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
            }

            // 6. 렌더링 강제 갱신 (장비 안 보임 현상 방지)
            // 수술 직후 RenderTree를 리셋하여 그래픽 오류를 해결합니다.
            pawn.Drawer.renderer.SetAllGraphicsDirty();

            // 8. 수술 완료 보고 (중복 사망하지 않았을 때만)
             
        }
    }
}