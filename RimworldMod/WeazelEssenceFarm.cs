using RimWorld;
using Verse;
using Verse.AI;

namespace WeazelEssenceFarm
{
    public class EssenceFarm : Verse.DefModExtension
    {
        public bool hideOnWeazelPawns = true;
    }

    public class WeazelJobGiver : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
		{
			if (!ModsConfig.BiotechActive) return 0f;
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Wait_Combat) return 0f;
            if (pawn.mindState.enemyTarget != null) return 0f;
			if (pawn.def.defName != "Weazel") return 0f;
			return 4.5f;
		}

        [DefOf] // 림월드 엔진에게 이 클래스가 Def 목록임을 알림
        public static class MyJobDefOf
        {
            // XML의 <defName>과 변수명이 '정확히' 일치해야 자동으로 연결됩니다.
            public static JobDef? EssenceExtract; 

            static MyJobDefOf()
            {
                // 게임 시작 시 XML 데이터를 이 변수들에 채워넣는 로직
                DefOfHelper.EnsureInitializedInCtor(typeof(MyJobDefOf));
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
		{
			if (!ModsConfig.BiotechActive) return null;

			if (pawn.def.defName != "Weazel")
            {
                return null;
            }
                
			if (pawn.def.defName == "Weazel")
			{
				Pawn prisoner = this.GetPrisoner(pawn);
				if (prisoner != null) return JobMaker.MakeJob(MyJobDefOf.EssenceExtract, prisoner);
			}

			return null;
		}

		public static AcceptanceReport CanFeedOnPrisoner(Pawn weazel, Pawn prisoner)
		{
            HediffDef essenceHediff = HediffDef.Named("EssenceReplicating"); 
            if (essenceHediff == null) return false;

			if (prisoner.health.hediffSet.HasHediff(essenceHediff)) return "대상의 에센스가 부족하여 추출시 사망 가능성이 있습니다.";
			
            if (!prisoner.IsPrisonerOfColony ||
                !prisoner.guest.PrisonerIsSecure ||
                prisoner.guest.IsInteractionDisabled(JobDriver_EssenceExtract.MyPrisonerInteractionModeDefOf.EssenceFarm) ||
                prisoner.IsForbidden(weazel) ||
                !weazel.CanReserveAndReach(prisoner, PathEndMode.OnCell, weazel.NormalMaxDanger(), 1, -1, null, false) ||
                prisoner.InAggroMentalState)
			{
				return false;
			}

			return true;
		}

        private Pawn GetPrisoner(Pawn pawn)
		{
			return (Pawn)GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.mapPawns.AllPawnsSpawned,
                PathEndMode.OnCell, TraverseParms.For(pawn, Danger.None, TraverseMode.ByPawn, false, false, false, true), 9999f, delegate(Thing t)
			{
				Pawn? pawn2 = t as Pawn;
				return pawn2 != null && WeazelJobGiver.CanFeedOnPrisoner(pawn, pawn2).Accepted;
			}, null, false);
		}
    }

    public class JobDriver_EssenceExtract : JobDriver
    {
        [DefOf] // 림월드 엔진에게 이 클래스가 Def 목록임을 알림
        public static class MyPrisonerInteractionModeDefOf
        {
            // XML의 <defName>과 변수명이 '정확히' 일치해야 자동으로 연결됩니다.
            public static PrisonerInteractionModeDef? EssenceFarm; 

            static MyPrisonerInteractionModeDefOf()
            {
                // 게임 시작 시 XML 데이터를 이 변수들에 채워넣는 로직
                DefOfHelper.EnsureInitializedInCtor(typeof(MyPrisonerInteractionModeDefOf));
            }
        }

        [DefOf]
        public static class MyRecipeDefOf
        {
            public static RecipeDef? ExtractEssenceRecipe;

            static MyRecipeDefOf()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(MyRecipeDefOf));
            }
        }

        protected Pawn Prisoner
		{
			get
			{
				return (Pawn)this.job.targetA.Thing;
			}
		}

        public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed, false);
		}

        // Token: 0x06007F2B RID: 32555 RVA: 0x002641A9 File Offset: 0x002623A9
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => !this.Prisoner.IsPrisonerOfColony || !this.Prisoner.guest.PrisonerIsSecure || this.Prisoner.InAggroMentalState || this.Prisoner.guest.IsInteractionDisabled(MyPrisonerInteractionModeDefOf.EssenceFarm));
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Prisoner, MyPrisonerInteractionModeDefOf.EssenceFarm);
			yield return Toils_General.WaitWith(TargetIndex.A, 120, true, false, false, TargetIndex.None, PathEndMode.Touch).PlaySustainerOrSound(SoundDefOf.Bloodfeed_Cast, 1f);
			yield return Toils_General.Do(delegate
			{
				RecipeDef? recipe = MyRecipeDefOf.ExtractEssenceRecipe;

                if (recipe != null)
                {
                    recipe.Worker.ApplyOnPawn(this.Prisoner, null, this.pawn, new List<Thing>(), null);
                }
			});
			yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
			yield break;
		}
    }
}