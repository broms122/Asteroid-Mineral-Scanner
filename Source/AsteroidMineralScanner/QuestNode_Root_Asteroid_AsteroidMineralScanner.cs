using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static RimWorld.QuestGen.QuestNode_GetSitePartDefsByTagsAndFaction;
using System.IO;

namespace AsteroidMineralScanner
{
    public class QuestNode_Root_Asteroid_AsteroidMineralScanner : QuestNode
    {
        public PlanetLayerDef layerDef;
        public WorldObjectDef worldObjectDef;



        //public List<ThingDef> mineables;
        public ThingDef targetMineableBlock; // MineableGold
        public ThingDef targetMineableThingDef; //Gold

        private static readonly int TimeoutDays = 30;

        protected override bool TestRunInt(Slate slate)
        {
            PlanetTile tile;
            return TryFindSiteTile(slate, out tile);
        }

        protected override void RunInt()
        {
            if (ModsConfig.OdysseyActive)
            {
                Quest quest = QuestGen.quest;
                Slate slate = QuestGen.slate;
                TryFindSiteTile(slate, out var tile);
                //slate.Set("targetMineableBlock", targetMineableBlock);
                SpaceMapParent spaceMapParent = (SpaceMapParent)WorldObjectMaker.MakeWorldObject(worldObjectDef);
                spaceMapParent.Tile = tile;

                //ThingDef thingDef = targetMineableBlock;
                slate.Set("resource", targetMineableThingDef.label);
                spaceMapParent.nameInt = "AsteroidName".Translate(targetMineableThingDef.label.Named("RESOURCE"));
                spaceMapParent.preciousResource = targetMineableBlock;

                slate.Set("worldObject", spaceMapParent);
                quest.SpawnWorldObject(spaceMapParent);
                string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("worldObject.MapRemoved");
                int delayTicks = TimeoutDays * 60000;
                quest.WorldObjectTimeout(spaceMapParent, delayTicks);
                quest.Delay(delayTicks, delegate
                {
                    QuestGen_End.End(quest, QuestEndOutcome.Fail);
                });
                quest.End(QuestEndOutcome.Success, 0, null, inSignal);
            }
        }

        private bool TryFindSiteTile(Slate slate, out PlanetTile tile)
        {
            tile = PlanetTile.Invalid;
            targetMineableBlock = slate.Get<ThingDef>("targetMineableBlock");
            targetMineableThingDef = slate.Get<ThingDef>("targetMineableThingDef");



            PlanetTile tile2;
            PlanetTile origin = (TileFinder.TryFindRandomPlayerTile(out tile2, allowCaravans: false, null, canBeSpace: true) ? tile2 : new PlanetTile(0, Find.WorldGrid.Surface));
            if (!Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(origin, layerDef, out var layer))
            {
                return false;
            }
            FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(origin, 1f, 3f);
            return layer.FastTileFinder.Query(query).TryRandomElement(out tile);
        }

    }

}
