using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using System.Runtime;
using Verse.Noise;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AsteroidMineralScanner.Properties;
using static Verse.ArenaUtility.ArenaResult;

namespace AsteroidMineralScanner
{
    [DefOf]
    public static class AMSDefOf
    {
        public static ThingDef AsteroidMineralScanner;
    }

    public class DefEditAMSMod : GameComponent
    {
        public DefEditAMSMod(Game game) { }
        public override void LoadedGame()
        {
            AMSMod.ApplySettingsNow();
        }
        public override void StartedNewGame()
        {
            AMSMod.ApplySettingsNow();
        }
    }

    [StaticConstructorOnStartup]
    public class AMSMod : Mod
    {

        public static AMSModSettings settings = null;
        public AMSMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AMSModSettings>();
        }

        public override string SettingsCategory() => "Asteroid Mineral Scanner";

        private int scanFindMtbDays;
        private int scanFindGuaranteedDays;
        private bool canBeUsedUnderRoof;



        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);


            // Add a new button to set settings to their defaults.
            if (listingStandard.ButtonText("Set Defaults"))
            {
                SetDefault();
                ApplySettingsNow();
            }

            var buffer1 = settings.scanFindMtbDays.ToString();
            var buffer2 = settings.scanFindGuaranteedDays.ToString();


            settings.scanFindMtbDays = (int)listingStandard.SliderLabeled("scanFindMtbDays".Translate() + ": " + settings.scanFindMtbDays, settings.scanFindMtbDays, 0, 20);
            listingStandard.TextFieldNumericLabeled("", ref settings.scanFindMtbDays, ref buffer1);
            listingStandard.Gap(24f);

            settings.scanFindGuaranteedDays = (int)listingStandard.SliderLabeled("scanFindGuaranteedDays".Translate() + ": " + settings.scanFindGuaranteedDays, settings.scanFindGuaranteedDays, 0, 20);
            listingStandard.TextFieldNumericLabeled("", ref settings.scanFindGuaranteedDays, ref buffer2);
            listingStandard.Gap(24f);

            //settings.canBeUsedUnderRoof = 
            //listingStandard.CheckboxLabeled("BromsAMS_canBeUsedUnderRoof".Translate(), ref settings.canBeUsedUnderRoof);
            listingStandard.CheckboxLabeled("canBeUsedUnderRoof".Translate(), ref settings.canBeUsedUnderRoof, 0);
            
            listingStandard.Gap(24f);



            /*
            listingStandard.Label(new TaggedString("BromsVCE_eachTicks".Translate() + ": " + settings.eachTicks), -1, "15000 is 6 hours, 60000 is 1 day");
            settings.eachTicks = (int)listingStandard.Slider(settings.eachTicks, 0, 60000);
            listingStandard.Gap(12f);

            listingStandard.Label(new TaggedString("BromsVCE_thingCount".Translate() + ": " + settings.thingCount), -1);
            settings.thingCount = (int)listingStandard.Slider(settings.thingCount, 0, 1000);
            listingStandard.Gap(12f);*/



            if (listingStandard.ButtonText("Apply now"))
            {
                ApplySettingsNow();
            }



            listingStandard.End();
            base.DoSettingsWindowContents(inRect);

        }

        private void SetDefault()
        {
            settings.scanFindMtbDays = AMSModSettings.scanFindMtbDays_default;
            settings.scanFindGuaranteedDays = AMSModSettings.scanFindGuaranteedDays_default;
            settings.canBeUsedUnderRoof = AMSModSettings.canBeUsedUnderRoof_default;
        }

        public static void ApplySettingsNow()
        {
            var scanner = AMSDefOf.AsteroidMineralScanner;
            ThingDef AMS = DefDatabase<ThingDef>.GetNamed("AsteroidMineralScanner");

            CompProperties_AsteroidMineralScanner asteroidMineralScanner = scanner.GetCompProperties<CompProperties_AsteroidMineralScanner>();
            if (asteroidMineralScanner != null)
            {
                asteroidMineralScanner.scanFindMtbDays = settings.scanFindMtbDays;
                asteroidMineralScanner.scanFindGuaranteedDays = settings.scanFindGuaranteedDays;
            }

            if (settings.canBeUsedUnderRoof == true)
            {
                AMS.canBeUsedUnderRoof = true;
                //AMS.placeWorkers.RemoveAll(x => x == typeof(PlaceWorker_NotUnderRoof));
                //AMS.placeWorkers.Remove(typeof(PlaceWorker_NotUnderRoof));
            } else {
                AMS.canBeUsedUnderRoof = false;
                /*
                if (!AMS.placeWorkers.Contains(typeof(PlaceWorker_NotUnderRoof)))
                    {
                        AMS.placeWorkers.Add(typeof(PlaceWorker_NotUnderRoof));
                    }*/
            }
        }

    }

    public class AMSModSettings : ModSettings
    {
        public AMSModSettings settings;


        public int scanFindMtbDays = scanFindMtbDays_default;
        public int scanFindGuaranteedDays = scanFindGuaranteedDays_default;
        public bool canBeUsedUnderRoof = canBeUsedUnderRoof_default;

        public const int scanFindMtbDays_default = 4;
        public const int scanFindGuaranteedDays_default = 8;
        public const bool canBeUsedUnderRoof_default = true;

        public override void ExposeData()
        {

            base.ExposeData();
            Scribe_Values.Look(ref scanFindMtbDays, "scanFindMtbDays", scanFindMtbDays_default);
            Scribe_Values.Look(ref scanFindGuaranteedDays, "scanFindGuaranteedDays", scanFindGuaranteedDays_default); 
            Scribe_Values.Look(ref canBeUsedUnderRoof, "canBeUsedUnderRoof", canBeUsedUnderRoof_default); 
        }

    }


}
