
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;
using System;
using AsteroidMineralScanner.Properties;

namespace AsteroidMineralScanner
{
    public class GenStep_Asteroid_AsteroidMineralScanner : GenStep
    {
        public override int SeedPart => 1929282;
        public ThingDef targetThingDef;

        /*
        public class MineableCountConfig
        {
            public ThingDef mineable;

            public IntRange countRange;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "mineable", xmlRoot.Name);
                countRange = IntRange.FromString(xmlRoot.FirstChild.Value);
            }
        }*/

        //public IntRange countRange = new IntRange(20, 30);

        // A = pi*r^2
        // r = sqrt(A)/sqrt(pi)

        public int asteroidOreCount => AMSMod.settings.asteroidOreCount; // desired ore area
        public float oreRadius => Math.Min((float)(Math.Sqrt((double)asteroidOreCount)/Math.Sqrt(Math.PI) + 3),41);
        public float floorRadius => Math.Min(oreRadius + 15, 56);
        //public float floorRadius => (float)((Math.Sqrt((double)asteroidOreCount) / Math.Sqrt(Math.PI)) + 15);
        public int asteroidFloorCount => (int)(Math.PI* floorRadius* floorRadius); // desired floor area





        //public static List<MineableCountConfig> mineableCounts;
        //private ModuleBase innerNoise;
        //protected virtual float Radius => 0.06f; // 0.224f base .. 0.04 is too big 0.02 seems ok.. was 0.06 for a long time

        public override void Generate(Map map, GenStepParams parms)
        {
            if (ModLister.CheckOdyssey("Asteroid"))
            {
                //GenerateAsteroidElevation(map, parms);
                Log.Message($"asteroidOreCount = {asteroidOreCount}");
                Log.Message($"oreRadius = {oreRadius}");
                Log.Message($"floorRadius = {floorRadius}");
                Log.Message($"asteroidFloorCount = {asteroidFloorCount}");
                Log.Message($"map.Size = {map.Size}");
                Log.Message($"asteroidFloorCount = {asteroidFloorCount}");


                targetThingDef = ((SpaceMapParent)map.ParentHolder).preciousResource;

                SpawnAsteroid(map, parms);
                //SpawnOres(map, parms);
                map.OrbitalDebris = OrbitalDebrisDefOf.Asteroid;
            }

        }

        private void SpawnAsteroid(Map map, GenStepParams parms)
        {
            //targetThingDef = Current.Game.GetComponent<AsteroidScannerGameComponent>().targetThingDef;

            IntVec3 mapCenter = map.Center;

            using (map.pathing.DisableIncrementalScope())
            {
                /*
                var intVec3List = GridShapeMaker.IrregularLump(mapCenter, map, asteroidFloorCount);

                foreach (IntVec3 cell in intVec3List)
                {
                    map.terrainGrid.SetTerrain(cell, ThingDefOf.Vacstone.building.naturalTerrain);
                }*/

                int generatedFloor = 0;
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(mapCenter, floorRadius, true))
                {
                    if (generatedFloor < asteroidFloorCount)
                    {
                        map.terrainGrid.SetTerrain(cell, ThingDefOf.Vacstone.building.naturalTerrain);
                        generatedFloor++;
                    }
                }

                /*              foreach (IntVec3 allCell in map.AllCells)
                                {
                                    float num = MapGenerator.Elevation[allCell];

                                    if (num > 0.5) // was 0.5f
                                    {
                                        map.terrainGrid.SetTerrain(allCell, ThingDefOf.Vacstone.building.naturalTerrain);
                                    }


                                }*/

                List<IntVec3> cellsInCircle = new List<IntVec3>();
                //int forcedLumpSize = countRange.RandomInRange;
                int generatedOre = 0;

                foreach (IntVec3 cell in GenRadial.RadialCellsAround(mapCenter, oreRadius, true))
                {   
                    if (generatedOre < asteroidOreCount)
                    {
                        GenSpawn.Spawn(targetThingDef, cell, map);
                        generatedOre++;
                        //Log.Message($"Spawning Ore # {generatedOre}\t thing = {targetThingDef}");
                    }
                    else
                    {
                        GenSpawn.Spawn(ThingDefOf.Vacstone, cell, map);
                        //Log.Message($"Spawning Vacstone");
                    }
                }

                HashSet<IntVec3> mainIsland = new HashSet<IntVec3>();
                map.floodFiller.FloodFill(map.Center, (IntVec3 x) => x.GetTerrain(map) != TerrainDefOf.Space, delegate (IntVec3 x)
                {
                    mainIsland.Add(x);
                });
                foreach (IntVec3 allCell2 in map.AllCells)
                {
                    if (mainIsland.Contains(allCell2))
                    {
                        continue;
                    }
                    map.terrainGrid.SetTerrain(allCell2, TerrainDefOf.Space);
                    map.roofGrid.SetRoof(allCell2, null);
                    foreach (Thing item in allCell2.GetThingList(map).ToList())
                    {
                        item.Destroy();
                    }
                }
            }
        }

        /*        private void GenerateAsteroidElevation(Map map, GenStepParams parms)
                {
                    innerNoise = ConfigureNoise(map, parms);
                    foreach (IntVec3 allCell in map.AllCells)
                    {
                        MapGenerator.Elevation[allCell] = innerNoise.GetValue(allCell);
                        //Log.Message($"test debug error log for GenStep_Asteroid_AsteroidMineralScanner - innerNoise = {innerNoise.GetValue(allCell)}");
                    }
                }

                protected virtual ModuleBase ConfigureNoise(Map map, GenStepParams parms)
                {
                    ModuleBase input = new DistFromPoint((float)map.Size.x * Radius);
                    input = new ScaleBias(-1.0, 1.0, input);
                    input = new Scale(0.6499999761581421, 1.0, 1.0, input);
                    input = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, input);
                    input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
                    NoiseDebugUI.StoreNoiseRender(input, "Base asteroid shape");
                    input = new Blend(new Perlin(0.006000000052154064, 2.0, 2.0, 3, Rand.Int, QualityMode.Medium), input, new Const(0.800000011920929));
                    input = new Blend(new Perlin(0.05000000074505806, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium), input, new Const(0.8500000238418579));
                    input = new Power(input, new Const(0.20000000298023224));
                    NoiseDebugUI.StoreNoiseRender(input, "Asteroid");
                    return input;
                }
        */
        /*        private void SpawnOres(Map map, GenStepParams parms)
                {

                    targetThingDef = ((SpaceMapParent)map.ParentHolder).preciousResource;
                    //mineableCounts.ForEach(count => { Log.Message($"mineableCounts({count.mineable})"); });

                    int forcedLumpSize = countRange.min;
                    GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
                    genStep_ScatterLumpsMineable.count = forcedLumpSize;
                    genStep_ScatterLumpsMineable.forcedDefToScatter = targetThingDef;
                    genStep_ScatterLumpsMineable.forcedLumpSize = forcedLumpSize;
                    genStep_ScatterLumpsMineable.Generate(map, parms);
                }


                private void GenerateCaveElevation(Map map, GenStepParams parms)
                {
                    Perlin directionNoise = new Perlin(0.0020000000949949026, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
                    MapGenFloatGrid elevation = MapGenerator.Elevation;
                    BoolGrid visited = new BoolGrid(map);
                    List<IntVec3> group = new List<IntVec3>();
                    MapGenCavesUtility.CaveGenParms @default = MapGenCavesUtility.CaveGenParms.Default;
                    @default.widthOffsetPerCell = 0.015f;
                    @default.minTunnelWidth = 0.5f;
                    @default.branchChance = 0.05f;
                    @default.maxOpenTunnelsPerRockGroup = 2;
                    @default.maxClosedTunnelsPerRockGroup = 2;
                    @default.minTunnelWidth = 0.25f;
                    @default.branchChance = 0.05f;
                    @default.openTunnelsPer10k = 4f;
                    @default.tunnelsWidthPerRockCount = new SimpleCurve
                {
                    new CurvePoint(100f, 1f),
                    new CurvePoint(300f, 1.5f),
                    new CurvePoint(3000f, 1.9f)
                };
                    MapGenCavesUtility.GenerateCaves(map, visited, group, directionNoise, @default, Rock);
                    bool Rock(IntVec3 cell)
                    {
                        return IsRock(cell, elevation, map);
                    }
                }

                private bool IsRock(IntVec3 c, MapGenFloatGrid elevation, Map map)
                {
                    if (c.InBounds(map))
                    {
                        return elevation[c] > 0.7f;
                    }
                    */
    }
}
