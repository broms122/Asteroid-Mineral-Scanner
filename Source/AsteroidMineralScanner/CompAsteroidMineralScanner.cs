using System.Collections.Generic;
using AsteroidMineralScanner.Properties;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace AsteroidMineralScanner
{
    public class CompAsteroidMineralScanner : CompScanner
    {
        public ThingDef targetMineableBlock;
        //public static ThingDef targetMineableThingDef => targetMineableBlock.building.mineableThing; //Gold
        public List<ThingDef> ResourceLumpNodeDefThingsList = new List<ThingDef>();
        public override AcceptanceReport CanUseNow
        {
            get
            {

                if (!parent.Spawned)
                {
                    return false;
                }
                if (powerComp != null && !powerComp.PowerOn)
                {
                    return false;
                }

                if (forbiddable != null && forbiddable.Forbidden)
                {
                    return false;
                }

                if (AMSMod.settings.canBeUsedUnderRoof == false)
                {
                    return "CannotUseScannerRoofed".Translate();
                }

                return parent.Faction == Faction.OfPlayer;
            }
        }

        public new CompProperties_AsteroidMineralScanner Props => props as CompProperties_AsteroidMineralScanner;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref targetMineableBlock, "targetMineableBlock");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && targetMineableBlock == null)
            {
                SetDefaultTargetMineral();
            }

            //Current.Game.GetComponent<AsteroidScannerGameComponent>().targetThingDef = targetMineableBlock;
            //Log.Message($"targetMineableBlock = {targetMineableBlock}");

        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            SetDefaultTargetMineral();
        }

        private void SetDefaultTargetMineral()
        {
            targetMineableBlock = ThingDefOf.MineableGold;
        }

        public void GetUndegroundNodeDefThings()
        {
            foreach (ThingDef thingdef in DefDatabase<ThingDef>.AllDefsListForReading.FindAll((ThingDef t) => t.thingClass == typeof(Mineable) && t.building.veinMineable && t.building.mineableThing != null))
                {
                    ResourceLumpNodeDefThingsList.Add(thingdef);
            }
        }

        protected override void DoFind(Pawn worker)
        {
            Slate slate = new Slate();
            slate.Set("map", parent.Map);
            slate.Set("targetMineableBlock", targetMineableBlock);
            slate.Set("targetMineableThingDef", targetMineableBlock.building.mineableThing);
            slate.Set("worker", worker);

                //Current.Game.GetComponent<AsteroidScannerGameComponent>().targetThingDef = targetMineableBlock;
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_Asteroid_AsteroidMineralScanner, slate);
                Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item2 in base.CompGetGizmosExtra())
            {
                yield return item2;
            }
            if (parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }
            ThingDef mineableThing = targetMineableBlock.building.mineableThing;
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandSelectMineralToScanFor".Translate() + ": " + mineableThing.LabelCap;
            command_Action.defaultDesc = "CommandSelectMineralToScanForDesc".Translate();
            command_Action.icon = mineableThing.uiIcon;
            command_Action.iconAngle = mineableThing.uiIconAngle;
            command_Action.iconOffset = mineableThing.uiIconOffset;
            command_Action.action = delegate
            {



                if (ResourceLumpNodeDefThingsList.NullOrEmpty())
                {
                    GetUndegroundNodeDefThings();
                }

                /*
                List<ThingDef> mineables = ((GenStep_PreciousLump)GenStepDefOf.PreciousLump.genStep).mineables;
                Log.Message("Mineables below");
                mineables.ForEach(def => Log.Message(def.ToString()));
                Log.Message("\n");
                ResourceLumpNodeDefThingsList.ForEach(def => Log.Message(def.ToString()));
                Log.Message("\n");
                */

                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (ThingDef item3 in ResourceLumpNodeDefThingsList)
                {
                    ThingDef localD = item3;
                    FloatMenuOption item = new FloatMenuOption(localD.building.mineableThing.LabelCap, delegate
                    {
                        foreach (object selectedObject in Find.Selector.SelectedObjects)
                        {
                            if (selectedObject is Thing thing)
                            {
                                    targetMineableBlock = localD;
                                    //Current.Game.GetComponent<AsteroidScannerGameComponent>().targetThingDef = localD;
                            }
                        }
                    }, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localD.building.mineableThing));
                    list.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            };
            yield return command_Action;
        }
    }
}
