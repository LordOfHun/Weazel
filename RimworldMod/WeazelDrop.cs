using Verse;

namespace WeazelDrop
{
    public class HediffCompProperties_DropItem : HediffCompProperties
    {
        public int tickInterval = 60000;
        public ThingDef thingDef;
        public int count = 15;

        public HediffCompProperties_DropItem()
        {this.compClass = typeof(HediffComp_DropItem);}
    }

    public class HediffComp_DropItem : HediffComp
    {
        private int ticks;

        public HediffCompProperties_DropItem Props => 
            (HediffCompProperties_DropItem)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticks++;

            if (ticks >= Props.tickInterval)
            {
                ticks = 0;

                Pawn pawn = this.Pawn;
                if (pawn.Map == null) return;

                Thing thing = ThingMaker.MakeThing(Props.thingDef);
                thing.stackCount = Props.count;

                GenPlace.TryPlaceThing(
                    thing,
                    pawn.Position,
                    pawn.Map,
                    ThingPlaceMode.Near
                );
            }
        }
    }
}