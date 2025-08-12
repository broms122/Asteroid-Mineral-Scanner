using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld.Planet;

namespace AsteroidMineralScanner
{
    public class AsteroidScannerGameComponent : GameComponent
    {
        public ThingDef targetThingDef;

        public AsteroidScannerGameComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref targetThingDef, "targetThingDef");
        }
    }
}
