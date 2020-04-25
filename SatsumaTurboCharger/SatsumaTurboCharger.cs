using HutongGames.PlayMaker;
using ModApi;
using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger
{

    public class SatsumaTurboCharger : Mod
    {
        /* Todo:
         *  -------------------------------------------------
         *  backfire flames ?
         *  Draw power grapth/turbo grapth ?

         *  check for air fuel mixture and decrease/increase boost
         *  prevent inspection with turbo installed
         *  make n2o usable
         *  increase fuel consumption when turbo is used/installed
         *  more grip ?
         *  -------------------------------------------------
         */

        /* Changelog v2.0 first
         * Added exhaust header 
         * changed exhaust pipes to fit with the new header
         * changed models for racing (big) turbo to have connection plates for screws
         * changed positions of parts slightly
         * added screws to all big turbo parts
         * added screws to intercooler manidold tubes
         * added screws to manifolds
         * added clamps to make tubes connected to each other more realistic looking
         */

        /* Changelog v2.0 second  
         * Added mods communication with DonnerTechRacing ecus mod
         * if you have the ecu mod installed and enable ALS (Antilag) the turbo will go to max boost and you can let of the throttle, drive around a corner and let it go again to have max boost
         * changed big turbo boost calculation (will now increase slower so that the car won't just instantly loose grip)
         */

        /* Changelog v2.0 third 
         * changed the multiplier used when the turbocharger value is applied.
         * Big turbo: from 2.0 -> 1.5f
         * Small turbo: from 1.4 -> 1.5f
         * Changed the boost gauge text to be the pressure of the turbo itself. 
         * Big turbo has a pressure of -0.10, meaning the car when idle or below turbo engage speed has less power (powerMultiplier = 0.9 instead of 1)
         * Small turbo has a pressure of -0.04, meaning the car when idle or below turbo engage speed has less power (powerMultiplier = 0.96 instead of 1)
         * Added parts wear to big turbo, small turbo, intercooler and airfilter.
         * Big turbo wears faster than small turbo
         * Airfilter reduces wear on small turbo based on it's own wear
         * If intercooler is damaged it will reduce the max pressure possible
         * Parts wear status can be checked by removing the part and aiming at it. It will show a text roughly telling the current wear status
         * Added Debug information for parts wear status
         * If the wear is below 15 (15%) there is a chance of 400:1 (0.25%) that the part will disassemble itself. The part can be reinstalled but the chance stays.
         * If the wear is at 0 (0%) the part will always disassemble when installed.
         * Added grinding sound when turbos wear is below 25%
         * Added repair "Products" to the ModsShop for big turbo, small turbo, intercooler, airfilter
         * ModsShop might not display anything after buying something. Just wait a while and it will display again (something going on with ModsShop no turbo mod)
         * Adjusted Prices for "REPAIR" to be lower than the original buy price
         * Added workaround for ModsShop purchased logic so "REPAIR" products can be bought multiple times
         * Parts have to be removed from the car and placed on the Desk where the ModsShop sign is. Otherwise the parts won't be repaired (money is refunded)
         * Changed started/finished loading ModConsole output.
         * ModsShop save information changed. I might have to delete the modsShop bought save file.
         * Added screws/clamps to small turbo, small turbo intercooler tube and small turbo exhaust inlet tube
         * 
         */

        /*
         *  
         *  
         *  
         *  -------------------------------------------------
         *  Known bugs:
         *  
         *  
         *  Use Key still has to be "F" for Painting
         */

        //Material[] tttt = Resources.FindObjectsOfTypeAll<Material>(); //Get all Materials of game
        //Component sprayCanPlayMakerFSM = tests.Value.GetComponentInChildren<PlayMakerFSM>(); // get PlayMaker Component from GameObject
        public override string ID => "SatsumaTurboCharger"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing Turbocharger"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "2.0"; //Version

        private bool ecu_mod_installed = false;
        public override bool UseAssetsFolder => true;

        private static PartBuySave partBuySave;
        private static OthersSave othersSave;
        private PartsWearSave partsWearSave;
        private Vector3 turbocharger_big_installLocation = new Vector3(-0.2705f, -0.064f, 0.273f);                              //Cylinder Head
        private Vector3 turbocharger_big_intercooler_tube_installLocation = new Vector3(0.318f, -0.041f, 1.52f);                //Satsuma       //Done
        private Vector3 turbocharger_big_exhaust_inlet_tube_installLocation = new Vector3(-0.179f, -0.1506f, -0.037f);          //Cylinder Head
        private Vector3 turbocharger_big_exhaust_outlet_tube_installLocation = new Vector3(-0.217f, -0.229f, -0.059f);          //Cylinder Head
        private Vector3 turbocharger_big_blowoff_valve_installLocation = new Vector3(0.3165f, 0.254f, 1.334f);                  //Satsuma       //Done

        private Vector3 turbocharger_exhaust_header_installLocation = new Vector3(-0.005f, -0.089f, -0.064f);               //Cylinder Head

        private Vector3 turbocharger_small_installLocation = new Vector3(-0.25f, -0.1665f, 0.0001f);                              //Cylinder Head
        private Vector3 turbocharger_small_manifold_twinCarb_tube_installLocation = new Vector3(-0.188f, -0.23f, 0.14f);        //Cylinder Head
        private Vector3 turbocharger_small_intercooler_tube_installLocation = new Vector3(0.316f, -0.041f, 1.518f);              //Satsuma
        private Vector3 turbocharger_small_exhaust_inlet_tube_installLocation = new Vector3(-0.0918f, -0.1774f, -0.094f);         //Cylinder Head
        private Vector3 turbocharger_small_exhaust_outlet_tube_installLocation = new Vector3(-0.1825f, -0.267f, -0.145f);         //Cylinder Head
        private Vector3 turbocharger_small_airfilter_installLocation = new Vector3(-0.25f, -0.04f, 0.0001f);                     //Cylinder Head

        private Vector3 turbocharger_hood_installLocation = new Vector3(0.0f, 0.241f, 1.68f);                                   //Satsuma
        private Vector3 turbocharger_manifold_weber_installLocation = new Vector3(0f, -0.3f, 0.1f);                             //Cylinder Head
        private Vector3 turbocharger_manifold_twinCarb_installLocation = new Vector3(-0.007f, -0.265f, 0.006f);                 //Cylinder Head
        private Vector3 turbocharger_boost_gauge_installLocation = new Vector3(0.5f, -0.04f, 0.125f);                           //Dashboard
        private Vector3 turbocharger_intercooler_installLocation = new Vector3(0.0f, -0.162f, 1.682f);                          //Satsuma
        private Vector3 turbocharger_intercooler_manifold_tube_weber_installLocation = new Vector3(-0.34f, -0.047f, 1.445f);    //Satsuma
        private Vector3 turbocharger_intercooler_manifold_tube_twinCarb_installLocation = new Vector3(-0.332f, -0.047f, 1.445f); //Satsuma

        private GameObject ecu_mod_SmartEngineModule;

        private ModsShop.ProductDetails repair_turbocharger_big_Product;
        private ModsShop.ProductDetails repair_turbocharger_small_Product;
        private ModsShop.ProductDetails repair_turbocharger_small_airfilter_Product;
        private ModsShop.ProductDetails repair_intercooler_Product;

        public static Vector3 turbocharger_big_spawnLocation = new Vector3(1558.366f, 5f, 742.5068f);
        public static Vector3 turbocharger_big_intercooler_tube_spawnLocation = new Vector3(1556.846f, 5f, 741.4836f);
        public static Vector3 turbocharger_big_exhaust_inlet_tube_spawnLocation = new Vector3(1557.866f, 5.5f, 741.9728f);
        public static Vector3 turbocharger_big_exhaust_outlet_tube_spawnLocation = new Vector3(1557.352f, 5f, 741.7303f);
        public static Vector3 turbocharger_big_blowoff_valve_spawnLocation = new Vector3(1555.136f, 5.8f, 737.2324f);

        public static Vector3 turbocharger_exhaust_header_spawnLocation = new Vector3(1555.136f, 5.8f, 737.2324f);

        public static Vector3 turbocharger_small_spawnLocation = new Vector3(1457.509f, -1.8f, 716.0f);
        public static Vector3 turbocharger_small_exhaust_inlet_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 715.5f);
        public static Vector3 turbocharger_small_exhaust_outlet_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 715.0f);
        public static Vector3 turbocharger_small_manifold_twinCarb_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 714.5f);

        public static Vector3 turbocharger_small_airfilter_spawnLocation = new Vector3(1555.174f, 5.8f, 736.9866f);

        public static Vector3 turbocharger_small_intercooler_tube_spawnLocation = new Vector3(1554.144f, 5f, 738.733f);

        public static Vector3 turbocharger_hood_spawnLocation = new Vector3(1559.46f, 5f, 742.296f);
        public static Vector3 turbocharger_manifold_weber_spawnLocation = new Vector3(1555.18f, 5.8f, 737.8821f);
        public static Vector3 turbocharger_manifold_twinCarb_spawnLocation = new Vector3(1555.07f, 5.8f, 737.6261f);
        public static Vector3 turbocharger_boost_gauge_spawnLocation = new Vector3(1555.383f, 5.8f, 737.058f);
        public static Vector3 turbocharger_intercooler_spawnLocation = new Vector3(1555.382f, 5.8f, 737.3588f);
        public static Vector3 turbocharger_intercooler_manifold_tube_weber_spawnLocation = new Vector3(1554.56f, 5f, 737.2017f);
        public static Vector3 turbocharger_intercooler_manifold_tube_twinCarb_spawnLocation = new Vector3(1554.339f, 5.5f, 737.913f);

        private RaycastHit hit;
        private GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private static bool useDefaultColors = false;

        private AssetBundle assets;

        private static GameObject turbocharger_big = new GameObject();
        private static GameObject turbocharger_big_intercooler_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_outlet_tube = new GameObject();
        private static GameObject turbocharger_big_blowoff_valve = new GameObject();

        private static GameObject turbocharger_exhaust_header = new GameObject();

        private static GameObject turbocharger_small = new GameObject();
        private static GameObject turbocharger_small_intercooler_tube = new GameObject();
        private static GameObject turbocharger_small_manifold_twinCarb_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_outlet_tube = new GameObject();
        private static GameObject turbocharger_small_airfilter = new GameObject();

        private static GameObject turbocharger_hood = new GameObject();
        private static GameObject turbocharger_manifold_weber = new GameObject();
        private static GameObject turbocharger_manifold_twinCarb = new GameObject();
        private static GameObject turbocharger_boost_gauge = new GameObject();
        private static GameObject turbocharger_intercooler = new GameObject();
        private static GameObject turbocharger_intercooler_manifold_tube_weber = new GameObject();
        private static GameObject turbocharger_intercooler_manifold_tube_twinCarb = new GameObject();
        private static GameObject boostGauge;
        private TextMesh boostGaugeTextMesh;
        private ParticleSystem small_turbo_fire_fx;

        private GameObject exhaustEngine;
        private GameObject exhaustPipeRace;
        private GameObject exhaustRaceMuffler;

        private Trigger turbocharger_big_Trigger;
        private Trigger turbocharger_big_intercoolerTubeTrigger;
        private Trigger turbocharger_big_exhaustInletTubeTrigger;
        private Trigger turbocharger_big_exhaustOutletTubeTrigger;
        private Trigger turbocharger_big_blowoffValveTrigger;

        private Trigger turbocharger_exhaust_header_Trigger;

        private Trigger turbocharger_small_Trigger;
        private Trigger turbocharger_small_intercoolerTubeTrigger;
        private Trigger turbocharger_small_manifold_twinCarb_tube_Trigger;
        private Trigger turbocharger_small_exhaustInletTubeTrigger;
        private Trigger turbocharger_small_exhaustOutletTubeTrigger;
        private Trigger turbocharger_small_airfilter_Trigger;

        private Trigger turbocharger_manifoldWeberTrigger;
        private Trigger turbocharger_manifoldTwinCarbTrigger;
        private Trigger turbocharger_hoodTrigger;
        private Trigger turbocharger_boostGaugeTrigger;
        private Trigger turbocharger_intercoolerTrigger;
        private Trigger turbocharger_intercoolerManifoldTubeWeberTrigger;
        private Trigger turbocharger_intercoolerManifoldTubeTwinCarbTrigger;

        //private FsmState n2oBottle;
        //private FsmFloat n2oBottlePSI;
        //private CarController satsumaCarController;
        //private Axles satsumaAxles;
        private GameObject elect;
        private PlayMakerFSM power;
        private MeshRenderer[] sprayCansMeshRenders;
        private Material regularCarPaintMaterial;
        private MeshRenderer turbocharger_hood_renderer;

        private ModAudio turbocharger_loop_big = new ModAudio();
        private ModAudio turbocharger_loop_small = new ModAudio();
        private ModAudio turbocharger_blowoff = new ModAudio();
        private ModAudio turbocharger_grinding_loop = new ModAudio();

        private static bool partsWearDEBUG = false;
        private static bool turboValuesDEBUG = false;
        private Settings DEBUG_parts_wear = new Settings("debugPartsWear", "Enable/Disable", SwitchPartsWearDEBUG);
        private Settings DEBUG_turbo_values = new Settings("debugTurboValues", "Enable/Disable", SwitchTurboChargerValuesDEBUG);
        private Settings useDefaultColorsSetting = new Settings("useDefaultColors", "Use default game colors for painting", false, new Action(SatsumaTurboCharger.ToggleUseDefaultColors));
        private Settings resetPosSetting = new Settings("resetPos", "Reset", new Action(SatsumaTurboCharger.PosReset));
        //Car values
        private float engineRPM = 0;
        private float enginePowerMultiplier;
        private float enginePowerCurrent;
        private FsmFloat _enginePowerMultiplier;
        private float newTurboChargerBar = 0;
        private bool isItemInHand;
        //Turbocharger audio sounds
        private AudioSource turboLoopBig;
        private AudioSource turboGrindingLoop;
        private AudioSource turboLoopSmall;
        private AudioSource turboBlowOffShot;

        private bool weberCarb_inst = false;
        private bool twinCarb_inst = false;
        private bool racingExhaustPipe_inst = false;
        private bool racingExhaustMuffler_inst = false;
        private FsmFloat racingExhaustPipeTighness;

        private bool electricityOn = false;


        private GameObject clampModelToUse;


        //Turbocharger parts
        private static Racing_Turbocharger_Part turboChargerBigPart;
        private static Racing_Intercooler_Tube_Part turboChargerBigIntercoolerTubePart;
        private static Racing_Exhaust_Inlet_Tube_Part turboChargerBigExhaustInletTubePart;
        private static Racing_Exhaust_Outlet_Tube_Part turboChargerBigExhaustOutletTubePart;
        private static Racing_Blowoff_Valve_Part turboChargerBigBlowoffValvePart;

        private static Exhaust_Header_Part turbocharger_exhaust_header_part;

        private static GT_Turbocharger_Part turboChargerSmallPart;
        private static Intercooler_Tube_Part turboChargerSmallIntercoolerTubePart;
        private static Exhaust_Inlet_Tube_Part turboChargerSmallExhaustInletTubePart;
        private static Exhaust_Outlet_Tube_Part turboChargerSmallExhaustOutletTubePart;
        private static GT_Airfilter_Part turbocharger_small_airfilter_part;
        private static Manifold_TwinCarb_Tube_Part turbocharger_small_manifold_twinCarb_tube_part;

        private static Racing_Hood_Part turboChargerHoodPart;
        
        private static Manifold_Weber_Part turboChargerManifoldWeberPart;
        private static Manifold_TwinCarb_Part turboChargerManifoldTwinCarbPart;
        private static Boost_Gauge_Part turboChargerBoostGaugePart;
        private static Intercooler_Part turboChargerIntercoolerPart;
        private static Intercooler_Manifold_Tube_Weber_Part turboChargerIntercoolerManifoldTubeWeberPart;
        private static Intercooler_Manifold_Tube_TwinCarb_Part turboChargerIntercoolerManifoldTubeTwinCarbPart;

        //ScrewableAPI
        public static ScrewablePart turbocharger_big_intercooler_tube_screwable;
        public static ScrewablePart turbocharger_big_screwable;
        public static ScrewablePart turbocharger_big_exhaust_inlet_tube_screwable;
        public static ScrewablePart turbocharger_big_exhaust_outlet_tube_screwable;

        public static ScrewablePart turbocharger_small_intercooler_tube_screwable;
        public static ScrewablePart turbocharger_small_screwable;
        public static ScrewablePart turbocharger_small_exhaust_inlet_tube_screwable;
        public static ScrewablePart turbocharger_small_exhaust_outlet_tube_screwable;
        public static ScrewablePart turbocharger_small_manifold_twinCarb_tube_screwable;

        public static ScrewablePart turbocharger_intercooler_manifold_weberCarb_tube_screwable;
        public static ScrewablePart turbocharger_intercooler_manifold_twinCarb_tube_screwable;

        public static ScrewablePart turbocharger_exhaust_header_screwable;

        public static ScrewablePart turbocharger_manifold_weberCarb_screwable;
        public static ScrewablePart turbocharger_manifold_twinCarb_screwable;

        private ModsShop.ShopItem shop;

        private int currentGear = 0;
        private bool errorDetected = false;
        /* Gear: R = 0
         * Gear: N = 1
         * Gear: 1 = 2
         * Gear: 2 = 3
         * Gear: 3 = 4
         * Gear: 4 = 5
         */
        private bool turbocharger_blowoffShotAllowed = false;
        private float timeSinceLastBlowOff;

        private float timer_wear_turbocharger_big;
        private float timer_wear_turbocharger_small;
        private float timer_wear_airfilter;
        private float timer_wear_intercooler;

        private static float[] originalGearRatios;
        private static float[] newGearRatio = new float[]
        {
            -4.093f, // reverse
            0f,      // neutral
            3.4f,  // 1st
            1.8f,  // 2nd
            1.4f,  // 3rd
            1.0f,   // 4th
            0.8f,   // 5th
            0.65f    // 6th
        };

        private FsmFloat _engineTorqueRpm;
        private FsmFloat _engineFriction;
        private FsmFloat _engineTorque;
        private FsmState n2oState;
        private FsmState no_n2oState;


        private GameObject N2O;
        private PlayMakerFSM n2oPlayMaker;
        private GameObject weberCarb;
        private GameObject twinCarb;
        private GameObject racingExhaustPipe;
        private GameObject racingExhaustMuffler;
        private GameObject originalCylinerHead;

        private Color pickedUPsprayCanColor;

        private Color hoodColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerBigColor = new Color(0.800f, 0.800f, 0.800f);
        private Color intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerBigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);

        private Color originalTurbocchargerBigColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalIntercoolerColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerBigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
        private Color original_turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
        private Color[] modSprayColors = new Color[13];

        private string partsColorSave;
        private bool allBigPartsInstalled = false;
        private bool allSmallPartsInstalled = false;
        private bool allOtherPartsInstalled = false;

        //private bool raceCarbInstalled;
        private Random randDestroyValue;
        internal static bool useThrottleButton
        {
            get
            {
                return cInput.GetKey("Throttle");
            }
        }

        private const string turbocharger_big_SaveFile = "turbocharger_big_partSave.txt";
        private const string turbocharger_big_intercooler_tube_SaveFile = "turbocharger_big_intercooler_tube_partSave.txt";
        private const string turbocharger_big_exhaust_inlet_tube_SaveFile = "turbocharger_big_exhaust_inlet_tube_partSave.txt";
        private const string turbocharger_big_exhaust_outlet_tube_SaveFile = "turbocharger_big_exhaust_outlet_tube_partSave.txt";
        private const string turbocharger_big_blowoff_valve_SaveFile = "turbocharger_big_blowoff_valve_partSave.txt";

        private const string turbocharger_exhaust_header_SaveFile = "turbocharger_exhaust_header_partSave.txt";


        private const string turbocharger_small_SaveFile = "turbocharger_small_partSave.txt";
        private const string turbocharger_small_intercooler_tube_SaveFile = "turbocharger_small_intercooler_tube_partSave.txt";
        private const string turbocharger_small_exhaust_inlet_tube_SaveFile = "turbocharger_small_exhaust_inlet_tube_partSave.txt";
        private const string turbocharger_small_exhaust_outlet_tube_SaveFile = "turbocharger_small_exhaust_outlet_tube_partSave.txt";
        private const string turbocharger_small_manifold_twinCarb_tube_SaveFile = "turbocharger_small_manifold_twinCarb_tube_partSave.txt";
        private const string turbocharger_small_airfilter_SaveFile = "turbocharger_small_airfilter_partSave.txt";

        private const string turbocharger_hood_SaveFile = "turbocharger_hood_partSave.txt";
        
        private const string turbocharger_manifold_weber_SaveFile = "turbocharger_manifold_weber_partSave.txt";
        private const string turbocharger_manifold_twinCarb_SaveFile = "turbocharger_manifold_twinCarb_partSave.txt";
        private const string turbocharger_boost_gauge_SaveFile = "turbocharger_boost_gauge_partSave.txt";
        private const string turbocharger_intercooler_SaveFile = "turbocharger_intercooler_partSave.txt";
        private const string turbocharger_intercooler_manifold_tube_weber_SaveFile = "turbocharger_intercooler_manifold_tube_weber_partSave.txt";
        private const string turbocharger_intercooler_manifold_tube_twinCarb_SaveFile = "turbocharger_intercooler_manifold_tube_twinCarb_partSave.txt";

        private const string turbocharger_mod_ModsShop_SaveFile = "turbocharger_mod_ModsShop_SaveFile.txt";
        private const string turbocharger_mod_others_SaveFile = "turbocharger_mod_Others_SaveFile.txt";
        private const string turbocharger_mod_wear_SaveFile = "turbocharger_mod_wear_SaveFile.txt";

        private const string boltSaveTest = "boltSaveTest.txt";

        private PartSaveInfo loadSaveData(string saveFile)
        {
            try
            {
                return SaveLoad.DeserializeSaveFile<PartSaveInfo>(this, saveFile);
            }
            catch (System.NullReferenceException)
            {
                // no save file exists.. //loading default save data.
                return null;
            }
        }
        private static void SwitchPartsWearDEBUG()
        {
            partsWearDEBUG = !partsWearDEBUG;
        }

        public override void OnNewGame()
        {
            // Called once, when starting a New Game, you can reset your saves here
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_intercooler_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_exhaust_inlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_exhaust_outlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_blowoff_valve_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_exhaust_header_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_intercooler_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_exhaust_inlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_exhaust_outlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_manifold_twinCarb_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_airfilter_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_hood_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_weber_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_twinCarb_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_boost_gauge_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_manifold_tube_weber_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);

            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, turbocharger_mod_ModsShop_SaveFile);
            SaveLoad.SerializeSaveFile<OthersSave>(this, null, turbocharger_mod_others_SaveFile);
            SaveLoad.SerializeSaveFile<PartsWearSave>(this, null, turbocharger_mod_wear_SaveFile);
            WritePartsColorSave(true);
        }



        public override void OnLoad()
        {
            ModConsole.Print("DonnerTechRacing Turbocharger Mod [v" + this.Version + "]" + " started loading");
            ecu_mod_installed = ModLoader.IsModPresent("DonnerTech_ECU_Mod");
            randDestroyValue = new Random();

            modSprayColors[0] = new Color(205f / 255, 205f / 255, 205f / 255, 1f);    // white
            modSprayColors[1] = new Color(40f / 255, 40f / 255, 40f / 255, 1f);       // black
            modSprayColors[2] = new Color(205f / 255, 0f / 255, 0f / 255, 1f);        // red
            modSprayColors[3] = new Color(0f / 255, 0f / 255, 220f / 255, 1f);        // blue
            modSprayColors[4] = new Color(130f / 255, 60f / 255, 0f / 255, 1f);       // brown
            modSprayColors[5] = new Color(250f / 255, 105f / 255, 0f / 255, 1f);      // orange
            modSprayColors[6] = new Color(190f / 255, 190f / 255, 0f / 255, 1f);      // yellow
            modSprayColors[7] = new Color(0f / 255, 120f / 255, 0f / 255, 1f);        // green
            modSprayColors[8] = new Color(0f / 255, 170f / 255, 210f / 255, 1f);      // lightblue
            modSprayColors[9] = new Color(130f / 255, 130f / 255, 130f / 255, 1f);    // grey
            modSprayColors[10] = new Color(220f / 255, 55f / 255, 220f / 255, 1f);     // pink
            modSprayColors[11] = new Color(0f / 255, 0f / 255, 220f / 255, 1f);        // turquoise
            modSprayColors[12] = new Color(40f / 255, 40f / 255, 40f / 255, 1f);       // mattblack


            
            try
            {
                string modapiPath = ModLoader.GetModAssetsFolder(this);
                modapiPath = modapiPath.Replace("Assets\\SatsumaTurboCharger", "References\\modapi_v0130-alpha.dll");
                if (File.Exists(modapiPath))
                {
                    ModUI.ShowMessage(
                        "ModAPI v130 detected!! \n" +
                        "This is a modified version of the original modapi (made by @tommojphillips)\n" +
                        "The modified version was made by the maker of the Supercharger mod (@Spysi)\n" +
                        "He has now removed this version from this mod and uses the same as me\n" +
                        "This file should still be deleted as it will still cause problems for any other mod\n" +
                        "Please remove this file and restart the game."
                        , "modapi v130 detected!!!!!");
                    errorDetected = true;
                }
            }
            catch
            {
                ModConsole.Error("Could not check modapiv130 status, please check your paths");
            }

            if (errorDetected == false)
            {
                try
                {
                    elect = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                }
                catch
                {
                    ModConsole.Print("Could not find Electricity of Car");
                }
                try
                {
                    power = PlayMakerFSM.FindFsmOnGameObject(elect, "Power");
                }
                catch
                {
                    ModConsole.Print("Could not find Power of Car");
                }
                try
                {
                    satsuma = GameObject.Find("SATSUMA(557kg, 248)");
                    /*
                    N2O = satsuma.transform.Find("CarSimulation/Engine/N2O").gameObject;
                    n2oPlayMaker = N2O.GetComponent<PlayMakerFSM>();
                    for(int i = 0; i < n2oPlayMaker.FsmStates.Length; i++)
                    {
                        if(n2oPlayMaker.FsmStates[i].Name == "Boost")
                        {
                            n2oState = n2oPlayMaker.FsmStates[i];
                        }
                        else if(n2oPlayMaker.FsmStates[i].Name == "No NOS")
                        {
                            no_n2oState = n2oPlayMaker.FsmStates[i];
                        }
                    }
                    */
                    satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
                    //satsumaCarController = satsuma.GetComponent<CarController>();
                    //satsumaAxles = satsuma.GetComponent<Axles>();
                }
                catch
                {
                    ModConsole.Error("Could not find Satsuma, Drivetrain");
                }

                try
                {
                    exhaustEngine = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");
                    exhaustPipeRace = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");
                    exhaustRaceMuffler = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");
                    exhaustRaceMuffler.transform.localPosition = new Vector3(-0.4f, -0.1f, -1.7f);
                }
                catch
                {

                }


                //n2oBottle = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmStates[4];
                //n2oBottlePSI = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmVariables.FloatVariables[4];
                weberCarb = GameObject.Find("racing carburators(Clone)");
                twinCarb = GameObject.Find("twin carburators(Clone)");
                racingExhaustPipe = GameObject.Find("racing exhaust(Clone)");
                racingExhaustMuffler = GameObject.Find("racing muffler(Clone)");
                originalCylinerHead = GameObject.Find("cylinder head(Clone)");

                

                originalGearRatios = satsumaDriveTrain.gearRatios;
                satsumaDriveTrain.revLimiter = true;

                try
                {
                    partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, turbocharger_mod_ModsShop_SaveFile);
                    othersSave = SaveLoad.DeserializeSaveFile<OthersSave>(this, turbocharger_mod_others_SaveFile);
                    partsWearSave = SaveLoad.DeserializeSaveFile<PartsWearSave>(this, turbocharger_mod_wear_SaveFile);
                    if (othersSave == null || othersSave.turbocharger_big_max_boost <= 0 || othersSave.turbocharger_big_max_boost_limit <= 0 || othersSave.turbocharger_small_max_boost <= 0 || othersSave.turbocharger_small_max_boost_limit <= 0)
                    {
                        othersSave = new OthersSave
                        {
                            turbocharger_big_max_boost = 1.55f,
                            turbocharger_big_max_boost_limit = 2f,
                            turbocharger_small_max_boost = 1f,
                            turbocharger_small_max_boost_limit = 1f
                        };
                    }
                    if(partsWearSave == null)
                    {
                        partsWearSave = new PartsWearSave
                        {
                            turbocharger_big_wear = 15.5f,
                            turbocharger_small_wear = 100f,
                            intercooler_wear = 100f,
                            airfilter_wear = 100f
                        };
                    }
                }
                catch
                {

                }
                satsumaDriveTrain.clutchTorqueMultiplier = 10f;
                partsColorSave = ModLoader.GetModConfigFolder(this) + "\\turbocharger_parts_ColorSave.xml";
                Material[] materialCollecion = Resources.FindObjectsOfTypeAll<Material>();
                foreach (Material material in materialCollecion)
                {
                    if (material.name == "CAR_PAINT_REGULAR")
                    {
                        regularCarPaintMaterial = material;
                        break;
                    }

                }
                try
                {
                    assets = LoadAssets.LoadBundle(this, "turbochargermod.unity3d");
                }
                catch
                {
                    ModConsole.Error("turbochargermod.unity3d could not be loaded \n check Mods/Assets/SatsumaTurboCharger/turbochargermod.unity3d");
                    errorDetected = true;
                }

                clampModelToUse = (assets.LoadAsset("Tube_Clamp.prefab") as GameObject);

                turbocharger_big = (assets.LoadAsset("turbocharger_big.prefab") as GameObject);
                turbocharger_big_intercooler_tube = (assets.LoadAsset("turbocharger_big_intercooler_tube.prefab") as GameObject);
                turbocharger_big_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_inlet_tube.prefab") as GameObject);
                turbocharger_big_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_outlet_tube.prefab") as GameObject);
                turbocharger_big_blowoff_valve = (assets.LoadAsset("turbocharger_big_blowoff_valve.prefab") as GameObject);
                turbocharger_exhaust_header = (assets.LoadAsset("turbocharger_big_exhaust_header.prefab") as GameObject);

                turbocharger_small = (assets.LoadAsset("turbocharger_small.prefab") as GameObject);

                turbocharger_small_intercooler_tube = (assets.LoadAsset("turbocharger_small_intercooler_tube.prefab") as GameObject);
                turbocharger_small_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_inlet_tube.prefab") as GameObject);
                turbocharger_small_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_outlet_tube.prefab") as GameObject);
                turbocharger_small_manifold_twinCarb_tube = (assets.LoadAsset("turbocharger_small_tube_manifold_twinCarb.prefab") as GameObject);
                turbocharger_small_airfilter = (assets.LoadAsset("turbocharger_small_airfilter.prefab") as GameObject);

                turbocharger_hood = (assets.LoadAsset("turbocharger_hood.prefab") as GameObject);
                turbocharger_manifold_weber = (assets.LoadAsset("turbocharger_manifold_weber.prefab") as GameObject);
                turbocharger_manifold_twinCarb = (assets.LoadAsset("turbocharger_manifold_twinCarb.prefab") as GameObject);
                turbocharger_intercooler = (assets.LoadAsset("turbocharger_intercooler.prefab") as GameObject);
                turbocharger_intercooler_manifold_tube_weber = (assets.LoadAsset("turbocharger_tube_intercooler_manifold_weber.prefab") as GameObject);
                turbocharger_intercooler_manifold_tube_twinCarb = (assets.LoadAsset("turbocharger_tube_intercooler_manifold_twinCarb.prefab") as GameObject);
                turbocharger_boost_gauge = (assets.LoadAsset("turbocharger_boost_gauge.prefab") as GameObject);

                
                AddPartsNames();
                AddPartsTrigger(originalCylinerHead);
                AddParts();

                SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, "test.txt");


                turbocharger_big_exhaust_inlet_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerBigExhaustInletTubePart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.202f, -0.1f, -0.01f),
                        new Vector3(0.145f, -0.1f, -0.018f),
                        new Vector3(-0.12f, 0.19f, -0.005f),
                        new Vector3(-0.2f, 0.19f, -0.005f),
                    },
                    new Vector3[]
                    {
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(90, 0, 0),
                        new Vector3(90, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_screw2");
                turbocharger_big_exhaust_outlet_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerBigExhaustOutletTubePart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(-0.0655f, 0.372f, -0.0425f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0, -90, 0),   //Bolt1
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_screw2");

                turbocharger_small_screwable = new ScrewablePart(screwListSave, this, turboChargerSmallPart.rigidPart,
                     new Vector3[]
                    {
                        new Vector3(0.0715f, -0.024f, 0.044f),
                        new Vector3(-0.0528f, 0.068f, -0.034f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180f, 0f, 0f),
                        new Vector3(0f, -90f, 0f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_small_intercooler_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerSmallIntercoolerTubePart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.034f, -0.13f, -0.1638f),
                    },
                    new Vector3[]
                    {
                         new Vector3(180f, 0f, 0f),
                    },
                    new Vector3[]
                    {
                         new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                //
                turbocharger_small_exhaust_inlet_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerSmallExhaustInletTubePart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.114f, -0.044f, -0.035f),
                        new Vector3(0.06f, -0.044f, -0.044f),
                    },
                    new Vector3[]
                    {
                        new Vector3(-90f, 0f, 0f),
                        new Vector3(-90f, 0f, 0f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_screw2");
                turbocharger_small_exhaust_outlet_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerSmallExhaustOutletTubePart.rigidPart,
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    }, 10, "screwable_screw2");
                turbocharger_small_manifold_twinCarb_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_small_manifold_twinCarb_tube_part.rigidPart,
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    }, 10, "screwable_screw2");

                turbocharger_exhaust_header_screwable = new ScrewablePart(screwListSave, this, turbocharger_exhaust_header_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.169f, 0.076f, -0.022f),   //Bolt1
                        new Vector3(0.13f, 0.0296f, -0.022f),   //Bolt2
                        new Vector3(-0.003f, 0.08f, -0.022f),   //Bolt3
                        new Vector3(-0.137f, 0.0296f, -0.022f),   //Bolt4
                        new Vector3(-0.174f, 0.076f, -0.022f)  //Bolt5
                    },
                    new Vector3[]
                    {
                        new Vector3(0, 0, 0),   //Bolt1
                        new Vector3(0, 0, 0),   //Bolt2
                        new Vector3(0, 0, 0),   //Bolt3
                        new Vector3(0, 0, 0),   //Bolt4
                        new Vector3(0, 0, 0)  //Bolt5
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 8, "screwable_nut");

                turbocharger_big_intercooler_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerBigIntercoolerTubePart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.153f, 0.324f, 0.1835f),
                        new Vector3(0.0779f, 0.324f, 0.1835f),
                        new Vector3(0.031f, -0.13f, -0.1632f),
                    },
                    new Vector3[]
                    {
                        new Vector3(90, 0, 0),
                        new Vector3(90, 0, 0),
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_big_screwable = new ScrewablePart(screwListSave, this, turboChargerBigPart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.105f, -0.098f, -0.082f),
                        new Vector3(0.03f, -0.098f, -0.082f),
                        new Vector3(-0.0288f, -0.098f, 0.0817f),
                        new Vector3(-0.1085f, -0.098f, 0.0817f),
                    },
                    new Vector3[]
                    {
                        new Vector3(-90, 0, 0),   //Bolt1
                        new Vector3(-90, 0, 0),   //Bolt2
                        new Vector3(-90, 0, 0),   //Bolt3
                        new Vector3(-90, 0, 0),   //Bolt4
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_nut");

                turbocharger_intercooler_manifold_weberCarb_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerIntercoolerManifoldTubeWeberPart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(-0.0473f, -0.1205f, -0.241f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_intercooler_manifold_twinCarb_tube_screwable = new ScrewablePart(screwListSave, this, turboChargerIntercoolerManifoldTubeTwinCarbPart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(-0.0425f, -0.1205f, -0.241f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");

                turbocharger_manifold_weberCarb_screwable = new ScrewablePart(screwListSave, this, turboChargerManifoldWeberPart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.2f, 0.03f, -0.009f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_manifold_twinCarb_screwable = new ScrewablePart(screwListSave, this, turboChargerManifoldTwinCarbPart.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.0105f, 0.105f, 0.03f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0, 90, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.5f, 0.5f, 0.5f),
                    }, 10, "screwable_screw2");

                turbocharger_big_exhaust_outlet_tube_screwable.AddClampModel(new Vector3(-0.055f, 0.334f, -0.0425f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                turbocharger_big_intercooler_tube_screwable.AddClampModel(new Vector3(0.031f, -0.154f, -0.1545f), new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
                turbocharger_intercooler_manifold_weberCarb_tube_screwable.AddClampModel(new Vector3(-0.047f, -0.1465f, -0.232f), new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
                turbocharger_intercooler_manifold_twinCarb_tube_screwable.AddClampModel(new Vector3(-0.042f, -0.1465f, -0.232f), new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
                turbocharger_manifold_weberCarb_screwable.AddClampModel(new Vector3(0.2f, -0.002f, 0.001f), new Vector3(0, 90, 0), new Vector3(0.82f, 0.82f, 0.82f));
                turbocharger_manifold_twinCarb_screwable.AddClampModel(new Vector3(0f, 0.105f, 0f), new Vector3(90, 0, 0), new Vector3(0.8f, 0.8f, 0.8f));

                
                turbocharger_small_intercooler_tube_screwable.AddClampModel(new Vector3(0.034f, -0.154f, -0.1548f), new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));

                turbocharger_small_screwable.AddClampModel(new Vector3(0.0715f, -0.043f, 0.052f), new Vector3(0, 90, 0), new Vector3(0.5f, 0.5f, 0.5f));
                turbocharger_small_screwable.AddClampModel(new Vector3(-0.044f, 0.068f, -0.0525f), new Vector3(90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
                /*
                
                turbocharger_small_exhaust_inlet_tube_screwable.AddClampModel(new Vector3(0f, 0f, 0f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                turbocharger_small_exhaust_outlet_tube_screwable.AddClampModel(new Vector3(0f, 0f, 0f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                turbocharger_small_manifold_twinCarb_tube_screwable.AddClampModel(new Vector3(0f, 0f, 0f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                */

                if (ecu_mod_installed)
                {
                    ecu_mod_SmartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
                }

                assets.Unload(false);
                UnityEngine.Object.Destroy(turbocharger_big);
                UnityEngine.Object.Destroy(turbocharger_big_intercooler_tube);
                UnityEngine.Object.Destroy(turbocharger_big_exhaust_inlet_tube);
                UnityEngine.Object.Destroy(turbocharger_big_exhaust_outlet_tube);
                UnityEngine.Object.Destroy(turbocharger_big_blowoff_valve);
                UnityEngine.Object.Destroy(turbocharger_exhaust_header);

                UnityEngine.Object.Destroy(turbocharger_small);
                UnityEngine.Object.Destroy(turbocharger_small_intercooler_tube);
                UnityEngine.Object.Destroy(turbocharger_small_exhaust_inlet_tube);
                UnityEngine.Object.Destroy(turbocharger_small_exhaust_outlet_tube);
                UnityEngine.Object.Destroy(turbocharger_small_manifold_twinCarb_tube);
                UnityEngine.Object.Destroy(turbocharger_small_airfilter);

                UnityEngine.Object.Destroy(turbocharger_hood);
                UnityEngine.Object.Destroy(turbocharger_manifold_weber);
                UnityEngine.Object.Destroy(turbocharger_manifold_twinCarb);
                UnityEngine.Object.Destroy(turbocharger_intercooler);
                UnityEngine.Object.Destroy(turbocharger_intercooler_manifold_tube_weber);
                UnityEngine.Object.Destroy(turbocharger_intercooler_manifold_tube_twinCarb);
                UnityEngine.Object.Destroy(turbocharger_boost_gauge);

                //Get PlayMaker Variables
                foreach (var playMakerFloatVar in PlayMakerGlobals.Instance.Variables.FloatVariables)
                {
                    switch (playMakerFloatVar.Name)
                    {
                        case "EnginePowerMultiplier":
                            _enginePowerMultiplier = playMakerFloatVar;
                            break;
                        case "EngineTorque":
                            _engineTorque = playMakerFloatVar;
                            break;
                        case "EngineTorqueRPM":
                            _engineTorqueRpm = playMakerFloatVar;
                            break;
                        case "EngineFriction":
                            _engineFriction = playMakerFloatVar;
                            break;
                    }
                }
                ModConsole.Print("DonnerTechRacing Turbocharger Mod [v" + this.Version + "]" + " finished loading");
            }
            else
            {
                ModConsole.Error("<b>[SatsumaTurbocharger]</b> - an error occured while trying to load mods");
            }

        }


        public override void ModSettings()
        {
            Settings.AddHeader(this, "DEBUG");
            Settings.AddButton(this, DEBUG_parts_wear, "DEBUG parts wear");
            Settings.AddButton(this, DEBUG_turbo_values, "DEBUG TurboCharger GUI");
            Settings.AddText(this, "");
            Settings.AddHeader(this, "Settings");
            Settings.AddCheckBox(this, useDefaultColorsSetting);
            Settings.AddButton(this, resetPosSetting, "reset part location");
        }

        public override void OnSave()
        {
            try
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerBigPart.getSaveInfo(), turbocharger_big_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerBigIntercoolerTubePart.getSaveInfo(), turbocharger_big_intercooler_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerBigExhaustInletTubePart.getSaveInfo(), turbocharger_big_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerBigExhaustOutletTubePart.getSaveInfo(), turbocharger_big_exhaust_outlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerBigBlowoffValvePart.getSaveInfo(), turbocharger_big_blowoff_valve_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_exhaust_header_part.getSaveInfo(), turbocharger_exhaust_header_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerSmallPart.getSaveInfo(), turbocharger_small_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerSmallIntercoolerTubePart.getSaveInfo(), turbocharger_small_intercooler_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerSmallExhaustInletTubePart.getSaveInfo(), turbocharger_small_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerSmallExhaustOutletTubePart.getSaveInfo(), turbocharger_small_exhaust_outlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_manifold_twinCarb_tube_part.getSaveInfo(), turbocharger_small_manifold_twinCarb_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_airfilter_part.getSaveInfo(), turbocharger_small_airfilter_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerHoodPart.getSaveInfo(), turbocharger_hood_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerManifoldWeberPart.getSaveInfo(), turbocharger_manifold_weber_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerManifoldTwinCarbPart.getSaveInfo(), turbocharger_manifold_twinCarb_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerBoostGaugePart.getSaveInfo(), turbocharger_boost_gauge_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerIntercoolerPart.getSaveInfo(), turbocharger_intercooler_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerIntercoolerManifoldTubeWeberPart.getSaveInfo(), turbocharger_intercooler_manifold_tube_weber_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turboChargerIntercoolerManifoldTubeTwinCarbPart.getSaveInfo(), turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);
                SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, turbocharger_mod_ModsShop_SaveFile);
                SaveLoad.SerializeSaveFile<OthersSave>(this, othersSave, turbocharger_mod_others_SaveFile);
                SaveLoad.SerializeSaveFile<PartsWearSave>(this, partsWearSave, turbocharger_mod_wear_SaveFile);


                ScrewablePart.SaveScrews(this, new ScrewablePart[]
                {
                    turbocharger_exhaust_header_screwable,
                    turbocharger_big_exhaust_inlet_tube_screwable,
                    turbocharger_big_exhaust_outlet_tube_screwable,
                    turbocharger_big_intercooler_tube_screwable,
                    turbocharger_big_screwable,
                    turbocharger_manifold_weberCarb_screwable,
                    turbocharger_manifold_twinCarb_screwable,
                    turbocharger_intercooler_manifold_weberCarb_tube_screwable,
                    turbocharger_intercooler_manifold_twinCarb_tube_screwable,
                }, "test.txt");

                WritePartsColorSave(false);
            }
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[SatsumaTurbocharger]</b> - an error occured while attempting to save part info.. see: " + ex.ToString());
            }
        }

        public override void OnGUI()
        {
            if (partsWearDEBUG)
            {
                GUI.Label(new Rect(20, 20, 200, 100), "Wear information:");
                GUI.Label(new Rect(20, 40, 200, 100), "Big turbo wear:   " + partsWearSave.turbocharger_big_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 60, 200, 100), "Small turbo wear: " + partsWearSave.turbocharger_small_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 80, 200, 100), "Intercooler wear: " + partsWearSave.intercooler_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 100, 200, 100), "Airfilter wear:   " + partsWearSave.airfilter_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 120, 200, 100), "-----------------------------------");
            }

            if (turboValuesDEBUG == true)
            {
                GUI.Label(new Rect(20, 140, 200, 100), "------------------------------------");
                GUI.Label(new Rect(20, 160, 200, 100), "Engine RPM: " + ((int)engineRPM).ToString());
                GUI.Label(new Rect(20, 180, 200, 100), "Turbo Charger bar: " + newTurboChargerBar.ToString("n3"));
                GUI.Label(new Rect(20, 200, 200, 100), "Power Current: " + ((int)enginePowerCurrent).ToString());
                GUI.Label(new Rect(20, 220, 200, 100), "Power Multiplier: " + _enginePowerMultiplier.Value.ToString("n2"));
                GUI.Label(new Rect(20, 240, 200, 100), "km/h: " + ((int)satsumaDriveTrain.differentialSpeed));
                GUI.Label(new Rect(20, 260, 200, 100), "Torque: " + satsumaDriveTrain.torque);
                GUI.Label(new Rect(20, 280, 200, 100), "Clutch Max Torque: " + satsumaDriveTrain.clutchMaxTorque);
                GUI.Label(new Rect(20, 300, 200, 100), "Clutch Torque Multiplier: " + satsumaDriveTrain.clutchTorqueMultiplier);
                //GUI.Label(new Rect(20, 200, 200, 100), "N2o active: " + n2oBottle.Active);
                GUI.Label(new Rect(20, 320, 200, 100), "Electricity on: " + electricityOn);
                GUI.Label(new Rect(20, 340, 200, 100), "------------------------------------");
            }
        }
        public override void Update()
        {
            if (turboChargerSmallPart.installed && small_turbo_fire_fx == null)
            {
                small_turbo_fire_fx = turboChargerSmallPart.rigidPart.GetComponentInChildren<ParticleSystem>();
                small_turbo_fire_fx.startSpeed = 5;
                small_turbo_fire_fx.startLifetime = 0.1f;
                small_turbo_fire_fx.playbackSpeed = 5;
            }
            

            //ModsShop purchashed workaround
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            foreach (ModsShop.ShopItems shopItem in shopItems)
            {
                if(shopItem.details.productName == "REPAIR Racing Turbocharger")
                {
                    shopItem.purchashed = false;
                }
                else if (shopItem.details.productName == "REPAIR GT Turbocharger")
                {
                    shopItem.purchashed = false;
                }
                else if(shopItem.details.productName == "REPAIR GT Turbo Airfilter")
                {
                    shopItem.purchashed = false;
                }
                else if(shopItem.details.productName == "REPAIR Intercooler")
                {
                    shopItem.purchashed = false;
                }
            }


            electricityOn = power.FsmVariables.FindFsmBool("ElectricsOK").Value;
            AddPartsColorMaterial();
            DetectPaintingPart();
            
            turbocharger_exhaust_header_screwable.DetectScrewing();
            turbocharger_big_exhaust_inlet_tube_screwable.DetectScrewing();
            turbocharger_big_exhaust_outlet_tube_screwable.DetectScrewing();
            turbocharger_big_intercooler_tube_screwable.DetectScrewing();
            turbocharger_big_screwable.DetectScrewing();

            turbocharger_small_intercooler_tube_screwable.DetectScrewing();
            turbocharger_small_screwable.DetectScrewing();
            turbocharger_small_exhaust_inlet_tube_screwable.DetectScrewing();
            turbocharger_small_exhaust_outlet_tube_screwable.DetectScrewing();
            turbocharger_small_manifold_twinCarb_tube_screwable.DetectScrewing();

            turbocharger_manifold_weberCarb_screwable.DetectScrewing();
            turbocharger_manifold_twinCarb_screwable.DetectScrewing();
            turbocharger_intercooler_manifold_weberCarb_tube_screwable.DetectScrewing();
            turbocharger_intercooler_manifold_twinCarb_tube_screwable.DetectScrewing();

            CheckPartsInstalledTrigger();
            if (turboChargerBoostGaugePart.installed)
            {
                boostGauge = turboChargerBoostGaugePart.rigidPart;
                boostGaugeTextMesh = boostGauge.GetComponentInChildren<TextMesh>();
            }
            else
                boostGauge = null;
            
            if (!turboChargerBigBlowoffValvePart.installed)
            {
                if (turboLoopBig != null && turboLoopBig.isPlaying)
                {
                    turbocharger_loop_big.Stop();
                }
                if (turboGrindingLoop != null && turboGrindingLoop.isPlaying)
                {
                    turbocharger_grinding_loop.Stop();
                }
                if (turboBlowOffShot != null && turboBlowOffShot.isPlaying)
                {
                    turbocharger_blowoff.Stop();
                }
            }

            if(turboChargerBigPart.installed && turboChargerBigIntercoolerTubePart.installed && turboChargerBigExhaustInletTubePart.installed && turboChargerBigExhaustOutletTubePart.installed && turboChargerBigBlowoffValvePart.installed)
            {
                allBigPartsInstalled = true;
            }
            else
            {
                allBigPartsInstalled = false;
            }
            if(turboChargerSmallPart.installed && (turboChargerSmallIntercoolerTubePart.installed || turbocharger_small_manifold_twinCarb_tube_part.installed) && turboChargerSmallExhaustInletTubePart.installed && turboChargerSmallExhaustOutletTubePart.installed)
            {
                allSmallPartsInstalled = true;
            }
            else
            {
                allSmallPartsInstalled = false;
            }

            if (
                (turboChargerManifoldWeberPart.installed || turboChargerManifoldTwinCarbPart.installed) && ((turboChargerIntercoolerPart.installed && (turboChargerIntercoolerManifoldTubeWeberPart.installed || turboChargerIntercoolerManifoldTubeTwinCarbPart.installed)) || turbocharger_small_manifold_twinCarb_tube_part.installed))
            {
                allOtherPartsInstalled = true;
            }
            else
            {
                allOtherPartsInstalled = false;
            }

            if((!allBigPartsInstalled && !allSmallPartsInstalled) || !allOtherPartsInstalled)
            {
                if (turboGrindingLoop != null && turboGrindingLoop.isPlaying)
                {
                    turbocharger_grinding_loop.Stop();
                }
            }

            if (allBigPartsInstalled == false || allOtherPartsInstalled == false)
            {
                if(turboLoopBig != null && turboLoopBig.isPlaying)
                {
                    turbocharger_loop_big.Stop();
                }
                
                if(turboBlowOffShot != null && turboBlowOffShot.isPlaying)
                {
                    turbocharger_blowoff.Stop();
                }
            }

            if(turboGrindingLoop != null && turboGrindingLoop.isPlaying && satsumaDriveTrain.rpm <= 200)
            {
                turbocharger_grinding_loop.Stop();
            }

            if (allSmallPartsInstalled == false || allOtherPartsInstalled == false)
            {
                if(turboLoopSmall != null && turboLoopSmall.isPlaying)
                {
                    turbocharger_loop_small.Stop();
                }
            }

            if ((allBigPartsInstalled || allSmallPartsInstalled) && allOtherPartsInstalled)
            {
                if (turboChargerBigPart.installed)
                {
                    timer_wear_turbocharger_big += Time.deltaTime;
                }
                else if (turboChargerSmallPart.installed)
                {
                    timer_wear_turbocharger_small += Time.deltaTime;
                }
                if (turbocharger_small_airfilter_part.installed)
                {
                    timer_wear_airfilter += Time.deltaTime;
                }
                if (turboChargerIntercoolerPart.installed)
                {
                    timer_wear_intercooler += Time.deltaTime;
                }

                timeSinceLastBlowOff += Time.deltaTime; //timer for checking if turbocharger blowoff sound is allowed to trigger

                enginePowerMultiplier = satsumaDriveTrain.powerMultiplier; //load the powerMultiplier of the car into a variable to be used later
                enginePowerCurrent = satsumaDriveTrain.currentPower; //load the current power generated of the car into a variable to be used later
                engineRPM = satsumaDriveTrain.rpm; //load the engine rpm of the car into a variable to be used later      


                if (timeSinceLastBlowOff >= 0.3f)
                {
                    CalculateAndSetEnginePowerTurbo();
                }
                else
                {
                    if (allBigPartsInstalled)
                    {
                        newTurboChargerBar = 0.90f;
                        enginePowerMultiplier = 0.90f;
                    }
                    else if (allSmallPartsInstalled)
                    {
                        newTurboChargerBar = 0.96f;
                        enginePowerMultiplier = 0.96f;
                    }
                }

                

                //Continous Loop of turbo sound -> if not already exists it will be created and played
                if (allBigPartsInstalled)
                {
                    if (turboLoopBig == null)
                    {
                        CreateTurboLoopBig();
                    }
                    else if (turboLoopBig.isPlaying == false)
                        turboLoopBig.Play();
                }
                else if (allSmallPartsInstalled)
                {
                    if (turboLoopSmall == null)
                    {
                        CreateTurboLoopSmall();
                    }
                    else if (turboLoopSmall.isPlaying == false)
                        turboLoopSmall.Play();
                }

                //Set Volume and Pitch based on engineRPM -> to make sound go ssssSuuuuuUUUUUUU
                if (allBigPartsInstalled)
                {
                    if (turboLoopBig != null)
                    {
                        turboLoopBig.volume = engineRPM * 0.00005f;
                        turboLoopBig.pitch = engineRPM * 0.00018f;
                    }

                }
                else if (allSmallPartsInstalled && turbocharger_small_airfilter_part.installed)
                {
                    if (turboLoopSmall != null)
                    {
                        turboLoopSmall.volume = engineRPM * 0.00003f;
                        turboLoopSmall.pitch = (engineRPM - 500) * 0.0003f;
                    }
                }
                else if (allSmallPartsInstalled)
                {
                    if (turboLoopSmall != null)
                    {
                        turboLoopSmall.volume = engineRPM * 0.000026f;
                        turboLoopSmall.pitch = (engineRPM - 500) * 0.00045f;
                    }
                }


                //If throttleKey is Pressed (W) and engineRPM is above 4000rpm AND one second has past since last blowoff sound
                //set turbocharger_blowoffShotAllowed to true -> a blowoff sound can be fired
                if (useThrottleButton && engineRPM > 4000 && timeSinceLastBlowOff > 1)
                {
                    turbocharger_blowoffShotAllowed = true;
                }

                // || (currentGear != satsumaDriveTrain.gear && engineRPM > 4000 && turbocharger_blowoffShotAllowed == true)
                //If ThrottleKey is NOT Pressed (W) and blowoff sound is allowed -> create blowoff sound if not already exists and set turbocharger_blowoffShotAllowed back to false
                if ((!useThrottleButton && turbocharger_blowoffShotAllowed == true))
                {
                    currentGear = satsumaDriveTrain.gear;
                    if (allBigPartsInstalled)
                    {
                        SetBoostGaugeText(0.10f, false);
                    }
                    else if (allSmallPartsInstalled)
                    {
                        SetBoostGaugeText(0.04f, false);
                    }
                    TriggerBlowOff();
                }
            }

            CheckPartsForDamage();
        }

        private void CheckPartsForDamage()
        {
            if (Camera.main != null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
                {
                    GameObject gameObjectHit;
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null)
                    {
                        if (hit.collider)
                        {
                            if (gameObjectHit.name == "Racing Turbocharger" || gameObjectHit.name == "Racing Turbocharger(Clone)")
                            {
                                if (!turboChargerBigPart.installed)
                                {
                                    if (partsWearSave.turbocharger_big_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.turbocharger_big_wear >= 50f)
                                        ModClient.guiInteraction = "Not sure if this thing should wobble that much";
                                    else if (partsWearSave.turbocharger_big_wear >= 25f)
                                        ModClient.guiInteraction = "Nope... This should not wobble that much";
                                    else if (partsWearSave.turbocharger_big_wear >= 15f)
                                        ModClient.guiInteraction = "I could pull it out. If I wanted to...";
                                    else if (partsWearSave.turbocharger_big_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "GT Turbocharger" || gameObjectHit.name == "GT Turbocharger(Clone)")
                            {
                                if (!turboChargerSmallPart.installed)
                                {
                                    if (partsWearSave.turbocharger_small_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.turbocharger_small_wear >= 50f)
                                        ModClient.guiInteraction = "Not sure if this thing should wobble that much";
                                    else if (partsWearSave.turbocharger_small_wear >= 25f)
                                        ModClient.guiInteraction = "Nope... This should not wobble that much";
                                    else if (partsWearSave.turbocharger_small_wear >= 15f)
                                        ModClient.guiInteraction = "I could pull it out. If I wanted to...";
                                    else if (partsWearSave.turbocharger_small_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "GT Turbocharger Airfilter" || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)")
                            {
                                if(!turbocharger_small_airfilter_part.installed){
                                    if (partsWearSave.airfilter_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.airfilter_wear >= 50f)
                                        ModClient.guiInteraction = "I can see some small holes in the fabric...";
                                    else if (partsWearSave.airfilter_wear >= 25f)
                                        ModClient.guiInteraction = "With those large holes I should get more power right?";
                                    else if (partsWearSave.airfilter_wear >= 15f)
                                        ModClient.guiInteraction = "I can nearly fit my hand into the holes";
                                    else if (partsWearSave.airfilter_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "Intercooler" || gameObjectHit.name == "Intercooler(Clone)")
                            {
                                if(!turboChargerIntercoolerPart.installed){
                                    if (partsWearSave.intercooler_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.intercooler_wear >= 50f)
                                        ModClient.guiInteraction = "Some scratches and little damage. Should be fine I guess...";
                                    else if (partsWearSave.intercooler_wear >= 25f)
                                        ModClient.guiInteraction = "I can hear air escaping more than before";
                                    else if (partsWearSave.intercooler_wear >= 15f)
                                        ModClient.guiInteraction = "It sounds like a leaf blower from those holes";
                                    else if (partsWearSave.intercooler_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckPartsWear()
        {      
            if (turboChargerBigPart.installed)
            {
                if (partsWearSave.turbocharger_big_wear <= 0f)
                {
                    turboChargerBigPart.removePart();
                }
                else if (partsWearSave.turbocharger_big_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(400);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        turboChargerBigPart.removePart();
                    }
                }
            }
            else if (turboChargerSmallPart.installed)
            {
                if (partsWearSave.turbocharger_small_wear <= 0f)
                {
                    turboChargerSmallPart.removePart();
                }
                else if (partsWearSave.turbocharger_small_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(400);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        turboChargerSmallPart.removePart();
                    }
                }
            }

            if (turbocharger_small_airfilter_part.installed)
            {
                if (partsWearSave.airfilter_wear <= 0f)
                {
                    turbocharger_small_airfilter_part.removePart();
                }
                else if (partsWearSave.airfilter_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(400);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        turbocharger_small_airfilter_part.removePart();
                    }
                }
            }
            if (turboChargerIntercoolerPart.installed)
            {
                if (partsWearSave.intercooler_wear <= 0f)
                {
                    turboChargerIntercoolerPart.removePart();
                }
                else if (partsWearSave.intercooler_wear <= 15f)
                {
                    
                    int randVal = randDestroyValue.Next(400);
                    if(randVal == 1)
                    {
                        //Part should destroy
                        turboChargerIntercoolerPart.removePart();
                    }
                }
            }
            
            
            
            
        }

        private void CheckPartsInstalledTrigger()
        {
            if (turboChargerBigPart.installed || turboChargerBigIntercoolerTubePart.installed || turboChargerBigExhaustInletTubePart.installed || turboChargerBigExhaustOutletTubePart.installed || turboChargerBigBlowoffValvePart.installed)
            {
                turbocharger_small_Trigger.triggerGameObject.SetActive(false);
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_small_exhaustInletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_small_exhaustOutletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_small_manifold_twinCarb_tube_Trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_Trigger.triggerGameObject.SetActive(true);
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_small_exhaustInletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_small_exhaustOutletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_small_manifold_twinCarb_tube_Trigger.triggerGameObject.SetActive(true);
            }

            if (turboChargerSmallPart.installed || (turboChargerSmallIntercoolerTubePart.installed || turbocharger_small_manifold_twinCarb_tube_part.installed) || turboChargerSmallExhaustInletTubePart.installed || turboChargerSmallExhaustOutletTubePart.installed)
            {
                turbocharger_big_Trigger.triggerGameObject.SetActive(false);
                turbocharger_big_intercoolerTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_big_exhaustInletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_big_exhaustOutletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_big_blowoffValveTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_big_Trigger.triggerGameObject.SetActive(true);
                turbocharger_big_intercoolerTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_big_exhaustInletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_big_exhaustOutletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_big_blowoffValveTrigger.triggerGameObject.SetActive(true);
            }

            if (turboChargerSmallIntercoolerTubePart.installed)
            {
                turbocharger_small_manifold_twinCarb_tube_Trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_manifold_twinCarb_tube_Trigger.triggerGameObject.SetActive(true);
            }

            if (turbocharger_small_manifold_twinCarb_tube_part.installed)
            {
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(false);

                turbocharger_intercoolerTrigger.triggerGameObject.SetActive(false);
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(false);
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(true);

                turbocharger_intercoolerTrigger.triggerGameObject.SetActive(true);
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(true);
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(true);
            }

            try
            {
                weberCarb_inst = weberCarb.transform.parent.transform.parent.name == "cylinder head(Clone)";
            }
            catch
            {
                try
                {
                    weberCarb_inst = GameObject.Find("racing carburators(Clone)").transform.parent.transform.parent.name == "cylinder head(Clone)";
                }
                catch { }
            }
            try
            {
                twinCarb_inst = twinCarb.transform.parent.transform.parent.name == "cylinder head(Clone)";
            }
            catch
            {
                try
                {
                    twinCarb_inst = GameObject.Find("twin carburators(Clone)").transform.parent.transform.parent.name == "cylinder head(Clone)";
                }
                catch { }
            }
            try
            {
                racingExhaustPipe_inst = racingExhaustPipe.transform.parent.name == "pivot_exhaust pipe";
            }
            catch
            {
                try
                {
                    racingExhaustPipe_inst = GameObject.Find("racing exhaust(Clone)").transform.parent.transform.parent.name == "pivot_exhaust pipe";
                }
                catch { }
            }

            try
            {
                racingExhaustPipeTighness = racingExhaustPipe.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Tightness");
            }
            catch
            {

            }

            try
            {
                racingExhaustMuffler_inst = racingExhaustMuffler.transform.parent.name == "pivot_exhaust_muffler";
            }
            catch
            {
                try
                {
                    racingExhaustMuffler_inst = GameObject.Find("racing muffler(Clone)").transform.parent.transform.parent.name == "pivot_exhaust_muffler";
                }
                catch { }
            }


            if (weberCarb_inst)
            {

                turbocharger_manifoldWeberTrigger.triggerGameObject.SetActive(true);
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(true);

                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(false);
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_manifoldWeberTrigger.triggerGameObject.SetActive(false);
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(false);

                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(true);
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(true);

            }

            if (twinCarb_inst)
            {
                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(true);
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(true);

                turbocharger_manifoldWeberTrigger.triggerGameObject.SetActive(false);
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(false);
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(false);

                turbocharger_manifoldWeberTrigger.triggerGameObject.SetActive(true);
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(true);
            }

            if (racingExhaustPipe_inst)
            {
                turbocharger_big_exhaustOutletTubeTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_big_exhaustOutletTubeTrigger.triggerGameObject.SetActive(false);
            }

            //ModConsole.Print(racingExhaustPipe_inst + " | " + racingExhaustMuffler_inst + " | " + turbocharger_big_exhaust_outlet_tube_screwable.partFixed);
            if(racingExhaustPipe_inst && racingExhaustMuffler_inst && turbocharger_exhaust_header_screwable.partFixed && turbocharger_big_exhaust_inlet_tube_screwable.partFixed && turbocharger_big_exhaust_outlet_tube_screwable.partFixed && racingExhaustPipeTighness.Value >= 24)
            {
                exhaustEngine.SetActive(false);
                exhaustPipeRace.SetActive(false);
                exhaustRaceMuffler.SetActive(true);
            }
            else
            {
                if(!turbocharger_exhaust_header_screwable.partFixed || !turbocharger_big_exhaust_inlet_tube_screwable.partFixed || !turbocharger_big_exhaust_outlet_tube_screwable.partFixed)
                {
                    exhaustEngine.SetActive(true);
                    exhaustPipeRace.SetActive(false);
                    exhaustRaceMuffler.SetActive(false);
                }
                else
                {
                    if (racingExhaustPipe_inst && !racingExhaustMuffler_inst)
                    {
                        exhaustEngine.SetActive(false);
                        exhaustPipeRace.SetActive(true);
                        exhaustRaceMuffler.SetActive(false);
                    }
                    else if (!racingExhaustPipe_inst && !racingExhaustMuffler_inst)
                    {
                        exhaustEngine.SetActive(true);
                        exhaustPipeRace.SetActive(false);
                        exhaustRaceMuffler.SetActive(false);
                    }
                }
                
            }

            if(turboChargerIntercoolerPart.installed || (turboChargerIntercoolerManifoldTubeWeberPart.installed || turboChargerIntercoolerManifoldTubeTwinCarbPart.installed) || turboChargerManifoldWeberPart.installed){
                turbocharger_small_manifold_twinCarb_tube_Trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_manifold_twinCarb_tube_Trigger.triggerGameObject.SetActive(true);
            }
        }

        private static void SwitchTurboChargerValuesDEBUG()
        {
            turboValuesDEBUG = !turboValuesDEBUG;
        }

        private void AddParts()
        {
            PartSaveInfo turbocharger_big_SaveInfo = null;
            PartSaveInfo turbocharger_big_intercooler_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_exhaust_inlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_exhaust_outlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_blowoff_valve_SaveInfo = null;
            PartSaveInfo turbocharger_exhaust_header_SaveInfo = null;

            PartSaveInfo turbocharger_small_SaveInfo = null;
            PartSaveInfo turbocharger_small_intercooler_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_exhaust_inlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_exhaust_outlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_manifold_twinCarb_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_airfilter_SaveInfo = null;

            PartSaveInfo turbocharger_hood_SaveInfo = null;
            PartSaveInfo turbocharger_manifold_weber_SaveInfo = null;
            PartSaveInfo turbocharger_manifold_twinCarb_SaveInfo = null;
            PartSaveInfo turbocharger_boost_gauge_SaveInfo = null;
            PartSaveInfo turbocharger_intercooler_SaveInfo = null;
            PartSaveInfo turbocharger_intercooler_manifold_tube_weber_SaveInfo = null;
            PartSaveInfo turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo = null;

            turbocharger_big_SaveInfo = this.loadSaveData(turbocharger_big_SaveFile);
            turbocharger_big_intercooler_tube_SaveInfo = this.loadSaveData(turbocharger_big_intercooler_tube_SaveFile);
            turbocharger_big_exhaust_inlet_tube_SaveInfo = this.loadSaveData(turbocharger_big_exhaust_inlet_tube_SaveFile);
            turbocharger_big_exhaust_outlet_tube_SaveInfo = this.loadSaveData(turbocharger_big_exhaust_outlet_tube_SaveFile);
            turbocharger_big_blowoff_valve_SaveInfo = this.loadSaveData(turbocharger_big_blowoff_valve_SaveFile);

            turbocharger_exhaust_header_SaveInfo = this.loadSaveData(turbocharger_exhaust_header_SaveFile);

            turbocharger_small_SaveInfo = this.loadSaveData(turbocharger_small_SaveFile);
            turbocharger_small_intercooler_tube_SaveInfo = this.loadSaveData(turbocharger_small_intercooler_tube_SaveFile);
            turbocharger_small_exhaust_inlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_inlet_tube_SaveFile);
            turbocharger_small_exhaust_outlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_outlet_tube_SaveFile);
            turbocharger_small_manifold_twinCarb_tube_SaveInfo = this.loadSaveData(turbocharger_small_manifold_twinCarb_tube_SaveFile);
            turbocharger_small_airfilter_SaveInfo = this.loadSaveData(turbocharger_small_airfilter_SaveFile);

            turbocharger_hood_SaveInfo = this.loadSaveData(turbocharger_hood_SaveFile);
            turbocharger_manifold_weber_SaveInfo = this.loadSaveData(turbocharger_manifold_weber_SaveFile);
            turbocharger_manifold_twinCarb_SaveInfo = this.loadSaveData(turbocharger_manifold_twinCarb_SaveFile);
            turbocharger_boost_gauge_SaveInfo = this.loadSaveData(turbocharger_boost_gauge_SaveFile);
            turbocharger_intercooler_SaveInfo = this.loadSaveData(turbocharger_intercooler_SaveFile);
            turbocharger_intercooler_manifold_tube_weber_SaveInfo = this.loadSaveData(turbocharger_intercooler_manifold_tube_weber_SaveFile);
            turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo = this.loadSaveData(turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);

            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, turbocharger_mod_others_SaveFile);

            try
            {
                partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, turbocharger_mod_ModsShop_SaveFile);
            }
            catch
            {

            }
            if (partBuySave == null)
            {
                partBuySave = new PartBuySave
                {
                    bought_turbocharger_big_kit = false,
                    bought_turbocharger_exhaust_header = false,
                    bought_turbocharger_small_intercooler_tube = false,
                    bought_turbocharger_small_airfilter = false,
                    bought_turbocharger_big_blowoff_valve = false,
                    bought_turbocharger_manifold_twinCarb_kit = false,
                    bought_turbocharger_manifold_weber_kit = false,
                    bought_turbocharger_hood = false,
                    bought_turbocharger_intercooler = false,
                    bought_turbocharger_boost_gauge = false
                };
            }
            if (!partBuySave.bought_turbocharger_big_kit)
            {
                turbocharger_big_SaveInfo = null;
                turbocharger_big_intercooler_tube_SaveInfo = null;
                turbocharger_big_exhaust_inlet_tube_SaveInfo = null;
                turbocharger_big_exhaust_outlet_tube_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_exhaust_header)
            {
                turbocharger_exhaust_header_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_small_intercooler_tube)
            {
                turbocharger_small_intercooler_tube_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_small_airfilter)
            {
                turbocharger_small_airfilter_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_big_blowoff_valve)
            {
                turbocharger_big_blowoff_valve_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_manifold_weber_kit)
            {
                turbocharger_manifold_weber_SaveInfo = null;
                turbocharger_intercooler_manifold_tube_weber_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_manifold_twinCarb_kit)
            {
                turbocharger_manifold_twinCarb_SaveInfo = null;
                turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_hood)
            {
                turbocharger_hood_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_intercooler)
            {
                turbocharger_intercooler_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_boost_gauge)
            {
                turbocharger_boost_gauge_SaveInfo = null;
            }


            turboChargerBigPart = new Racing_Turbocharger_Part(
                turbocharger_big_SaveInfo,
                turbocharger_big,
                originalCylinerHead,
                turbocharger_big_Trigger,
                turbocharger_big_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );

            turbocharger_exhaust_header_part = new Exhaust_Header_Part(
                turbocharger_exhaust_header_SaveInfo,
                turbocharger_exhaust_header,
                originalCylinerHead,
                turbocharger_exhaust_header_Trigger,
                turbocharger_exhaust_header_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );

            turboChargerBigIntercoolerTubePart = new Racing_Intercooler_Tube_Part(
                turbocharger_big_intercooler_tube_SaveInfo,
                turbocharger_big_intercooler_tube,
                satsuma, 
                turbocharger_big_intercoolerTubeTrigger,
                turbocharger_big_intercooler_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
                // -0.195f, 0.071f, 0.145f old

            );
            turboChargerBigExhaustInletTubePart = new Racing_Exhaust_Inlet_Tube_Part(
                turbocharger_big_exhaust_inlet_tube_SaveInfo,
                turbocharger_big_exhaust_inlet_tube,
                originalCylinerHead,
                turbocharger_big_exhaustInletTubeTrigger,
                turbocharger_big_exhaust_inlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turboChargerBigExhaustOutletTubePart = new Racing_Exhaust_Outlet_Tube_Part(
                turbocharger_big_exhaust_outlet_tube_SaveInfo,
                turbocharger_big_exhaust_outlet_tube,
                originalCylinerHead,
                turbocharger_big_exhaustOutletTubeTrigger,
                turbocharger_big_exhaust_outlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turboChargerBigBlowoffValvePart = new Racing_Blowoff_Valve_Part(
                turbocharger_big_blowoff_valve_SaveInfo,
                turbocharger_big_blowoff_valve,
                satsuma,
                turbocharger_big_blowoffValveTrigger,
                turbocharger_big_blowoff_valve_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 35)
                }
            );

            turboChargerSmallPart = new GT_Turbocharger_Part(
                turbocharger_small_SaveInfo,
                turbocharger_small,
                originalCylinerHead,
                turbocharger_small_Trigger,
                turbocharger_small_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turboChargerSmallIntercoolerTubePart = new Intercooler_Tube_Part(
                turbocharger_small_intercooler_tube_SaveInfo,
                turbocharger_small_intercooler_tube,
                satsuma,
                turbocharger_small_intercoolerTubeTrigger,
                turbocharger_small_intercooler_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );
            turbocharger_small_manifold_twinCarb_tube_part = new Manifold_TwinCarb_Tube_Part(
                turbocharger_small_manifold_twinCarb_tube_SaveInfo,
                turbocharger_small_manifold_twinCarb_tube,
                originalCylinerHead,
                turbocharger_small_manifold_twinCarb_tube_Trigger,
                turbocharger_small_manifold_twinCarb_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_small_airfilter_part = new GT_Airfilter_Part(
                turbocharger_small_airfilter_SaveInfo,
                turbocharger_small_airfilter,
                originalCylinerHead,
                turbocharger_small_airfilter_Trigger,
                turbocharger_small_airfilter_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );


            turboChargerSmallExhaustInletTubePart = new Exhaust_Inlet_Tube_Part(
                turbocharger_small_exhaust_inlet_tube_SaveInfo,
                turbocharger_small_exhaust_inlet_tube,
                originalCylinerHead,
                turbocharger_small_exhaustInletTubeTrigger,
                turbocharger_small_exhaust_inlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turboChargerSmallExhaustOutletTubePart = new Exhaust_Outlet_Tube_Part(
                turbocharger_small_exhaust_outlet_tube_SaveInfo,
                turbocharger_small_exhaust_outlet_tube,
                originalCylinerHead,
                turbocharger_small_exhaustOutletTubeTrigger,
                turbocharger_small_exhaust_outlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );


            turboChargerHoodPart = new Racing_Hood_Part(
                turbocharger_hood_SaveInfo,
                turbocharger_hood,
                satsuma,
                turbocharger_hoodTrigger,
                turbocharger_hood_installLocation,
                new Quaternion(0, 180, 0, 0)
            );


            turboChargerManifoldWeberPart = new Manifold_Weber_Part(
                turbocharger_manifold_weber_SaveInfo,
                turbocharger_manifold_weber,
                originalCylinerHead,
                turbocharger_manifoldWeberTrigger,
                turbocharger_manifold_weber_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(60, 0, 0)
                }
            );

            turboChargerManifoldTwinCarbPart = new Manifold_TwinCarb_Part(
                turbocharger_manifold_twinCarb_SaveInfo,
                turbocharger_manifold_twinCarb,
                originalCylinerHead,
                turbocharger_manifoldTwinCarbTrigger,
                turbocharger_manifold_twinCarb_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );


            turboChargerBoostGaugePart = new Boost_Gauge_Part(
                turbocharger_boost_gauge_SaveInfo,
                turbocharger_boost_gauge,
                GameObject.Find("dashboard(Clone)"),
                turbocharger_boostGaugeTrigger,
                turbocharger_boost_gauge_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turboChargerIntercoolerPart = new Intercooler_Part(
               turbocharger_intercooler_SaveInfo,
               turbocharger_intercooler,
               satsuma,
               turbocharger_intercoolerTrigger,
               turbocharger_intercooler_installLocation,
               new Quaternion
               {
                   eulerAngles = new Vector3(-15, 180, 0)
               }
               //old: 0.03f, -0.015f, 0.142f
           );
            turboChargerIntercoolerManifoldTubeWeberPart = new Intercooler_Manifold_Tube_Weber_Part(
                turbocharger_intercooler_manifold_tube_weber_SaveInfo,
                turbocharger_intercooler_manifold_tube_weber,
                satsuma,
                turbocharger_intercoolerManifoldTubeWeberTrigger,
                turbocharger_intercooler_manifold_tube_weber_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );
            turboChargerIntercoolerManifoldTubeTwinCarbPart = new Intercooler_Manifold_Tube_TwinCarb_Part(
                turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo,
                turbocharger_intercooler_manifold_tube_twinCarb,
                satsuma,
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger,
                turbocharger_intercooler_manifold_tube_twinCarb_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );

            //addClampModelToParent();
            

            LoadPartsColorSave();
            SetModsShop();
        }

        private void addClampModelToParent()
        {
            Transform[] parentTransforms = new Transform[]
            {
                turboChargerBigExhaustOutletTubePart.rigidPart.transform,
                turboChargerBigIntercoolerTubePart.rigidPart.transform,
                turboChargerIntercoolerManifoldTubeWeberPart.rigidPart.transform,
                turboChargerIntercoolerManifoldTubeTwinCarbPart.rigidPart.transform,
                turboChargerManifoldWeberPart.rigidPart.transform,
                turboChargerManifoldTwinCarbPart.rigidPart.transform,

            };

            Vector3[] positions = new Vector3[]
            {
                new Vector3(-0.055f, 0.334f, -0.0425f), // Big Exhaust Outlet Tube
                new Vector3(0.031f, -0.154f, -0.1545f), // Big Intercooler Tube
                new Vector3(-0.047f, -0.1465f, -0.232f), //Intercooler Manifold Weber Tube
                new Vector3(-0.042f, -0.1465f, -0.232f), //Intercooler Manifold TwinCarb Tube
                new Vector3(0.2f, -0.002f, 0.001f), //Manifold Weber
                new Vector3(0f, 0.105f, 0f), //Manifold TwinCarb
            };
            Vector3[] rotations = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 90, 0),
                new Vector3(0, 90, 0),
                new Vector3(0, 90, 0),
                new Vector3(0, 90, 0),
                new Vector3(90, 0, 0),
            };
            Vector3[] scales = new Vector3[]
            {
                new Vector3(1, 1, 1),
                new Vector3(0.62f, 0.62f, 0.62f),
                new Vector3(0.68f, 0.68f, 0.68f),
                new Vector3(0.68f, 0.68f, 0.68f),
                new Vector3(0.82f, 0.82f, 0.82f),
                new Vector3(0.8f, 0.8f, 0.8f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject clamp = GameObject.Instantiate(clampModelToUse);
                clamp.name = "CLAMP" + i;
                clamp.transform.SetParent(parentTransforms[i]);
                clamp.transform.localPosition = positions[i];
                clamp.transform.localScale = scales[i];
                clamp.transform.localRotation = new Quaternion { eulerAngles = rotations[i] };
            }
            
        }

        private void SetModsShop()
        {
            if (GameObject.Find("Shop for mods") != null)
            {
                shop = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();

                //Create product
                repair_turbocharger_big_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR Racing Turbocharger",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Repair_ProductImage.png"),
                    productPrice = 4000
                };
                repair_turbocharger_small_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR GT Turbocharger",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Repair_ProductImage.png"),
                    productPrice = 2500
                };
                shop.Add(this, repair_turbocharger_small_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerSmall, null);

                repair_turbocharger_small_airfilter_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR GT Turbo Airfilter",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Airfilter_Repair_ProductImage.png"),
                    productPrice = 400
                };
                repair_intercooler_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR Intercooler",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Intercooler_Repair_ProductImage.png"),
                    productPrice = 1500
                };


                ModsShop.ProductDetails turbocharger_big_kit_Product = new ModsShop.ProductDetails
                {
                    productName = "Racing Turbocharger Kit",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Kit_ProductImage.png"),
                    productPrice = 8100
                };
                if (!partBuySave.bought_turbocharger_big_kit)
                {
                    shop.Add(this, turbocharger_big_kit_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerBigKit, null);
                    turboChargerBigPart.activePart.SetActive(false);
                    turboChargerBigIntercoolerTubePart.activePart.SetActive(false);
                    turboChargerBigExhaustInletTubePart.activePart.SetActive(false);
                    turboChargerBigExhaustOutletTubePart.activePart.SetActive(false);
                    turbochargerBigColor = new Color(0.800f, 0.800f, 0.800f);
                    originalTurbocchargerBigColor = new Color(0.800f, 0.800f, 0.800f);

                }
                else
                {
                    shop.Add(this, repair_turbocharger_big_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerBig, null);
                }
                ModsShop.ProductDetails turbocharger_exhaust_header_Product = new ModsShop.ProductDetails
                {
                    productName = "Turbocharger Exhaust Header",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Kit_ProductImage.png"),
                    productPrice = 2100
                };
                if (!partBuySave.bought_turbocharger_exhaust_header)
                {
                    shop.Add(this, turbocharger_exhaust_header_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerExhaustHeader, null);
                    turbocharger_exhaust_header_part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails turbocharger_big_blowoff_valve_Product = new ModsShop.ProductDetails
                {
                    productName = "Racing Turbocharger Blowoff Valve",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Blowoff_Valve_ProductImage.png"),
                    productPrice = 1350
                };
                if (!partBuySave.bought_turbocharger_big_blowoff_valve)
                {
                    shop.Add(this, turbocharger_big_blowoff_valve_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerBigBlowoffValve, null);
                    turboChargerBigBlowoffValvePart.activePart.SetActive(false);
                    turbochargerBigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
                    originalTurbochargerBigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
                }

                ModsShop.ProductDetails turbocharger_small_intercoolerTube_Product = new ModsShop.ProductDetails
                {
                    productName = "GT Turbocharger Intercooler Tube",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Tube_Intercooler_ProductImage.png"),
                    productPrice = 500
                };
                if (!partBuySave.bought_turbocharger_small_intercooler_tube)
                {
                    shop.Add(this, turbocharger_small_intercoolerTube_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerSmallIntercoolerTube, null);
                    turboChargerSmallIntercoolerTubePart.activePart.SetActive(false);
                }
                ModsShop.ProductDetails turbocharger_small_airfilter_Product = new ModsShop.ProductDetails
                {
                    productName = "GT Turbocharger Airfilter",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Airfilter_ProductImage.png"),
                    productPrice = 800
                };
                if (!partBuySave.bought_turbocharger_small_airfilter)
                {
                    shop.Add(this, turbocharger_small_airfilter_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerSmallAirfilter, null);
                    turbocharger_small_airfilter_part.activePart.SetActive(false);
                    turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                    original_turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                }
                else
                {
                    shop.Add(this, repair_turbocharger_small_airfilter_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerSmallAirfilter, null);
                }

                ModsShop.ProductDetails turbocharger_manifold_twinCarb_kit_Product = new ModsShop.ProductDetails
                {
                    productName = "TwinCarb Manifold Kit",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("TwinCarb_Manifold_Kit_ProductImage.png"),
                    productPrice = 1950
                };
                if (!partBuySave.bought_turbocharger_manifold_twinCarb_kit)
                {
                    shop.Add(this, turbocharger_manifold_twinCarb_kit_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerManifoldTwinCarbKit, null);
                    turboChargerManifoldTwinCarbPart.activePart.SetActive(false);
                    turboChargerIntercoolerManifoldTubeTwinCarbPart.activePart.SetActive(false);
                    turbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
                    originalTurbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
                }
                ModsShop.ProductDetails turbocharger_manifold_weber_kit_Product = new ModsShop.ProductDetails
                {
                    productName = "Weber Manifold Kit",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Weber_Manifold_Kit_ProductImage.png"),
                    productPrice = 2250
                };
                if (!partBuySave.bought_turbocharger_manifold_weber_kit)
                {
                    shop.Add(this, turbocharger_manifold_weber_kit_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerManifoldbWeberKit, null);
                    turboChargerManifoldWeberPart.activePart.SetActive(false);
                    turboChargerIntercoolerManifoldTubeWeberPart.activePart.SetActive(false);
                    turbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
                    originalTurbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
                }
                ModsShop.ProductDetails turbocharger_hood_Product = new ModsShop.ProductDetails
                {
                    productName = "Racing Turbocharger Hood",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Turbocharger_Hood_ProductImage.png"),
                    productPrice = 1800
                };
                if (!partBuySave.bought_turbocharger_hood)
                {
                    shop.Add(this, turbocharger_hood_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerHood, null);
                    turboChargerHoodPart.activePart.SetActive(false);
                    hoodColor = new Color(0.800f, 0.800f, 0.800f);
                }
                ModsShop.ProductDetails turbocharger_intercooler_Product = new ModsShop.ProductDetails
                {
                    productName = "Intercooler",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Turbocharger_Intercooler_ProductImage.png"),
                    productPrice = 3500
                };
                if (!partBuySave.bought_turbocharger_intercooler)
                {
                    shop.Add(this, turbocharger_intercooler_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerIntercooler, null);
                    turboChargerIntercoolerPart.activePart.SetActive(false);
                    intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                    originalIntercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                }
                else
                {
                    shop.Add(this, repair_intercooler_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeIntercooler, null);
                }
                ModsShop.ProductDetails turbocharger_boost_gauge_Product = new ModsShop.ProductDetails
                {
                    productName = "Boost Gauge",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Turbocharger_Boost_Gauge_ProductImage.png"),
                    productPrice = 180
                };
                if (!partBuySave.bought_turbocharger_boost_gauge)
                {
                    shop.Add(this, turbocharger_boost_gauge_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerBoostGauge, null);
                    turboChargerBoostGaugePart.activePart.SetActive(false);
                }
            }
            else
            {
                ModUI.ShowMessage(
               "You need to install ModsShop by piotrulos for this mod\n" +
               "Please close the game and install the mod\n" +
               "There should have been a ModsShop.dll and unity3d file (inside Assets) inside the archive of this mod",
               "Installation of ModsShop (by piotrulos) needed");
            }
        }
        public void RepairPurchaseMadeTurbochargerBig(ModsShop.PurchaseInfo item)
        {
            if (CheckCloseToPosition(turboChargerBigPart.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                partsWearSave.turbocharger_big_wear = 100;
                turbochargerBigColor = new Color(0.800f, 0.800f, 0.800f);
                originalTurbocchargerBigColor = new Color(0.800f, 0.800f, 0.800f);
                turboChargerBigPart.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turboChargerBigPart.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_turbocharger_big_Product.productPrice;
            }
        }
        public void RepairPurchaseMadeTurbochargerSmall(PurchaseInfo purchaseInfo)
        {
            if (CheckCloseToPosition(turboChargerSmallPart.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                partsWearSave.turbocharger_small_wear = 100;
                turbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
                originalTurbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
                turboChargerSmallPart.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turboChargerSmallPart.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_turbocharger_small_Product.productPrice;
            }
        }
        public void RepairPurchaseMadeTurbochargerSmallAirfilter(PurchaseInfo purchaseInfo)
        {
            if (CheckCloseToPosition(turbocharger_small_airfilter_part.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                partsWearSave.airfilter_wear = 100;
                turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                original_turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                turbocharger_small_airfilter_part.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turbocharger_small_airfilter_part.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_turbocharger_small_airfilter_Product.productPrice;
            }
        }
        public void RepairPurchaseMadeIntercooler(PurchaseInfo purchaseInfo)
        {
            if (CheckCloseToPosition(turboChargerIntercoolerPart.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                if (turboChargerIntercoolerPart.installed)
                {
                    turboChargerIntercoolerPart.removePart();
                }
                partsWearSave.intercooler_wear = 100;
                intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                originalIntercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                turboChargerIntercoolerPart.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turboChargerIntercoolerPart.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_intercooler_Product.productPrice;
            }
        }

        public void PurchaseMadeTurbochargerBigKit(ModsShop.PurchaseInfo item)
        {
            partsWearSave.turbocharger_big_wear = 100;
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            int counter = 0;
            foreach(ModsShop.ShopItems shopItem in shopItems)
            {
                if(shopItem.details.productName == "REPAIR Racing Turbocharger")
                {
                    counter++;
                }
            }
            if(counter == 0)
            {
                shop.Add(this, repair_turbocharger_big_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerBig, null);
            }
            turboChargerBigPart.activePart.transform.position = new Vector3(1558.366f, 5f, 742.5068f);
            turboChargerBigIntercoolerTubePart.activePart.transform.position = new Vector3(1556.846f, 5f, 741.4836f);
            turboChargerBigExhaustInletTubePart.activePart.transform.position = new Vector3(1557.866f, 5f, 741.9728f);
            turboChargerBigExhaustOutletTubePart.activePart.transform.position = new Vector3(1557.352f, 5f, 741.7303f);


            turboChargerBigPart.activePart.SetActive(true);
            turboChargerBigIntercoolerTubePart.activePart.SetActive(true);
            turboChargerBigExhaustInletTubePart.activePart.SetActive(true);
            turboChargerBigExhaustOutletTubePart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_big_kit = true;
        }
        public void PurchaseMadeTurbochargerExhaustHeader(ModsShop.PurchaseInfo item)
        {
            turbocharger_exhaust_header_part.activePart.transform.position = new Vector3(1555.136f, 5.8f, 737.2324f); //CHANGE
            turbocharger_exhaust_header_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_exhaust_header = true;
        }
        public void PurchaseMadeTurbochargerBigBlowoffValve(ModsShop.PurchaseInfo item)
        {
            turboChargerBigBlowoffValvePart.activePart.transform.position = new Vector3(1555.136f, 5.8f, 737.2324f);

            turboChargerBigBlowoffValvePart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_big_blowoff_valve = true;
        }
        public void PurchaseMadeTurbochargerSmallIntercoolerTube(ModsShop.PurchaseInfo item)
        {
            turboChargerSmallIntercoolerTubePart.activePart.transform.position = new Vector3(1554.144f, 5f, 738.733f);
            turboChargerSmallIntercoolerTubePart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_small_intercooler_tube = true;
        }
        public void PurchaseMadeTurbochargerSmallAirfilter(ModsShop.PurchaseInfo item)
        {
            partsWearSave.airfilter_wear = 100;
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            int counter = 0;
            foreach (ModsShop.ShopItems shopItem in shopItems)
            {
                if (shopItem.details.productName == "REPAIR GT Turbo Airfilter")
                {
                    counter++;
                }
            }
            if (counter == 0)
            {
                shop.Add(this, repair_turbocharger_small_airfilter_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerSmallAirfilter, null);
            }
            turbocharger_small_airfilter_part.activePart.transform.position = new Vector3(1555.174f, 5.8f, 736.9866f);
            turbocharger_small_airfilter_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_small_airfilter = true;
        }

        public void PurchaseMadeTurbochargerManifoldTwinCarbKit(ModsShop.PurchaseInfo item)
        {
            turboChargerManifoldTwinCarbPart.activePart.transform.position = new Vector3(1555.07f, 5.8f, 737.6261f);
            turboChargerIntercoolerManifoldTubeTwinCarbPart.activePart.transform.position = new Vector3(1554.339f, 5.5f, 737.913f);

            turboChargerManifoldTwinCarbPart.activePart.SetActive(true);
            turboChargerIntercoolerManifoldTubeTwinCarbPart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_manifold_twinCarb_kit = true;
        }
        public void PurchaseMadeTurbochargerManifoldbWeberKit(ModsShop.PurchaseInfo item)
        {
            turboChargerManifoldWeberPart.activePart.transform.position = new Vector3(1555.18f, 5.8f, 737.8821f);
            turboChargerIntercoolerManifoldTubeWeberPart.activePart.transform.position = new Vector3(1554.56f, 5f, 737.2017f);

            turboChargerManifoldWeberPart.activePart.SetActive(true);
            turboChargerIntercoolerManifoldTubeWeberPart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_manifold_weber_kit = true;

        }
        public void PurchaseMadeTurbochargerHood(ModsShop.PurchaseInfo item)
        {
            turboChargerHoodPart.activePart.transform.position = new Vector3(1559.46f, 5f, 742.296f);

            turboChargerHoodPart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_hood = true;
            
        }
        public void PurchaseMadeTurbochargerIntercooler(ModsShop.PurchaseInfo item)
        {
            partsWearSave.intercooler_wear = 100;
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            int counter = 0;
            foreach (ModsShop.ShopItems shopItem in shopItems)
            {
                if (shopItem.details.productName == "REPAIR Intercooler")
                {
                    counter++;
                }
            }
            if (counter == 0)
            {
                shop.Add(this, repair_intercooler_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeIntercooler, null);
            }
            turboChargerIntercoolerPart.activePart.transform.position = new Vector3(1555.382f, 5.8f, 737.3588f);

            turboChargerIntercoolerPart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_intercooler = true;
        }
        public void PurchaseMadeTurbochargerBoostGauge(ModsShop.PurchaseInfo item)
        {
            turboChargerBoostGaugePart.activePart.transform.position = new Vector3(1555.383f, 5.8f, 737.058f);

            turboChargerBoostGaugePart.activePart.SetActive(true);
            partBuySave.bought_turbocharger_boost_gauge = true;
        }

        private void LoadPartsColorSave()
        {
            if (File.Exists(partsColorSave))
            {
                XmlReader xmlReader = XmlReader.Create(partsColorSave);
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element))
                    {
                        if (xmlReader.HasAttributes)
                        {
                            string r = xmlReader.GetAttribute("r");
                            string g = xmlReader.GetAttribute("g");
                            string b = xmlReader.GetAttribute("b");
                            float rFloat = 0f;
                            float gFloat = 0f;
                            float bFloat = 0f;

                            float.TryParse(r, out rFloat);
                            float.TryParse(g, out gFloat);
                            float.TryParse(b, out bFloat);
                            if (xmlReader.Name == "hood-color")
                            {
                                hoodColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "intercooler-color")
                            {
                                intercoolerColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "turbochargerBig-color")
                            {
                                turbochargerBigColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "turbochargerSmall-color")
                            {
                                turbochargerSmallColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "weber-color")
                            {
                                turbochargerManifoldWeberColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "twincarb-color")
                            {
                                turbochargerManifoldTwinCarbColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "blowoffValve-color")
                            {
                                turbochargerBigBlowoffValveColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if(xmlReader.Name == "turbocharger_small_airfilter-color")
                            {
                                turbocharger_small_airfilter_color = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_intercooler-color")
                            {
                                originalIntercoolerColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_turbochargerBig-color")
                            {
                                originalTurbocchargerBigColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_turbochargerSmall-color")
                            {
                                originalTurbochargerSmallColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_weber-color")
                            {
                                originalTurbochargerManifoldWeberColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_twincarb-color")
                            {
                                originalTurbochargerManifoldTwinCarbColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if(xmlReader.Name == "original_blowoffValve-color")
                            {
                                originalTurbochargerBigBlowoffValveColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_turbocharger_small_airfilter-color")
                            {
                                turbocharger_small_airfilter_color = new Color(rFloat, gFloat, bFloat);
                            }
                        }
                    }
                }
                xmlReader.Close();
            }
        }

        private void WritePartsColorSave(bool newGame)
        {
            partsColorSave = ModLoader.GetModConfigFolder(this) + "\\turbocharger_parts_ColorSave.xml";
            XmlWriter xmlWriter = XmlWriter.Create(partsColorSave);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("color-save");
            if (newGame == false)
            {
                WriteXMLColorSaveElement(xmlWriter, "hood-color", hoodColor);
                WriteXMLColorSaveElement(xmlWriter, "intercooler-color", intercoolerColor);
                WriteXMLColorSaveElement(xmlWriter, "turbochargerBig-color", turbochargerBigColor);
                WriteXMLColorSaveElement(xmlWriter, "turbochargerSmall-color", turbochargerSmallColor);
                WriteXMLColorSaveElement(xmlWriter, "turbocharger_small_airfilter-color", turbocharger_small_airfilter_color);

                WriteXMLColorSaveElement(xmlWriter, "weber-color", turbochargerManifoldWeberColor);
                WriteXMLColorSaveElement(xmlWriter, "twincarb-color", turbochargerManifoldTwinCarbColor);
                WriteXMLColorSaveElement(xmlWriter, "blowoffValve-color", turbochargerBigBlowoffValveColor);

                WriteXMLColorSaveElement(xmlWriter, "original_intercooler-color", originalIntercoolerColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbochargerBig-color", originalTurbocchargerBigColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbochargerSmall-color", originalTurbochargerSmallColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_small_airfilter-color", original_turbocharger_small_airfilter_color);

                WriteXMLColorSaveElement(xmlWriter, "original_weber-color", originalTurbochargerManifoldWeberColor);
                WriteXMLColorSaveElement(xmlWriter, "original_twincarb-color", originalTurbochargerManifoldTwinCarbColor);
                WriteXMLColorSaveElement(xmlWriter, "original_blowoffValve-color", originalTurbochargerBigBlowoffValveColor);
            }

            else
            {
                Color defaultColor = new Color(0.800f, 0.800f, 0.800f);
                WriteXMLColorSaveElement(xmlWriter, "hood-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "intercooler-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "turbochargerBig-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "turbochargerSmall-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "weber-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "twincarb-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "blowoffValve-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "turbocharger_small_airfilter-color", defaultColor);

                WriteXMLColorSaveElement(xmlWriter, "original_intercooler-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbochargerBig-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbochargerSmall-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_small_airfilter-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_weber-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_twincarb-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_blowoffValve-color", defaultColor);
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        private void WriteXMLColorSaveElement(XmlWriter xmlWriter, string elementName, Color colorToSave)
        {
            xmlWriter.WriteStartElement(elementName);
            xmlWriter.WriteAttributeString("r", colorToSave.r.ToString());
            xmlWriter.WriteAttributeString("g", colorToSave.g.ToString());
            xmlWriter.WriteAttributeString("b", colorToSave.b.ToString());
            xmlWriter.WriteEndElement();
        }

        private void AddPartsColorMaterial()
        {

            try
            {
                turbocharger_hood_renderer = turboChargerHoodPart.rigidPart.GetComponentInChildren<MeshRenderer>();
                if (turbocharger_hood_renderer == null)
                {
                    turbocharger_hood_renderer = turboChargerHoodPart.activePart.GetComponentInChildren<MeshRenderer>();

                }
                if (turbocharger_hood_renderer != null)
                {
                    if (turbocharger_hood_renderer.material.name != "CAR_PAINT_REGULAR (Instance)")
                    {
                        turbocharger_hood_renderer.material = regularCarPaintMaterial;
                    }
                }
            }
            catch
            {

            }
            try
            {
                if (useDefaultColors)
                {
                    SetPartMaterialColor(turboChargerHoodPart, hoodColor);
                    SetPartMaterialColor(turboChargerBigPart, originalTurbocchargerBigColor);
                    SetPartMaterialColor(turboChargerSmallPart, originalTurbochargerSmallColor);
                    SetPartMaterialColor(turboChargerIntercoolerPart, originalIntercoolerColor);
                    SetPartMaterialColor(turboChargerManifoldWeberPart, originalTurbochargerManifoldWeberColor);
                    SetPartMaterialColor(turboChargerManifoldTwinCarbPart, originalTurbochargerManifoldTwinCarbColor);
                    SetPartMaterialColor(turboChargerBigBlowoffValvePart, originalTurbochargerBigBlowoffValveColor);
                    SetPartMaterialColor(turbocharger_small_airfilter_part, original_turbocharger_small_airfilter_color);
                }
                else
                {
                    SetPartMaterialColor(turboChargerHoodPart, hoodColor);
                    SetPartMaterialColor(turboChargerBigPart, turbochargerBigColor);
                    SetPartMaterialColor(turboChargerSmallPart, turbochargerSmallColor);
                    SetPartMaterialColor(turboChargerIntercoolerPart, intercoolerColor);
                    SetPartMaterialColor(turboChargerManifoldWeberPart, turbochargerManifoldWeberColor);
                    SetPartMaterialColor(turboChargerManifoldTwinCarbPart, turbochargerManifoldTwinCarbColor);
                    SetPartMaterialColor(turboChargerBigBlowoffValvePart, turbochargerBigBlowoffValveColor);
                    SetPartMaterialColor(turbocharger_small_airfilter_part, turbocharger_small_airfilter_color);
                }

            }
            catch
            {

            }
        }

        private void SetPartMaterialColor(Part part, Color colorToPaint)
        {
            try
            {
                MeshRenderer meshRenderer = part.rigidPart.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = part.activePart.GetComponentInChildren<MeshRenderer>();
                }
                if (meshRenderer.material.color != colorToPaint)
                {
                    for (int i = 0; i < meshRenderer.materials.Length; i++)
                    {
                        if (meshRenderer.materials[i].name == "Red_Acent (Instance)" || meshRenderer.materials[i].name == "CAR_PAINT_REGULAR (Instance)")
                        {
                            meshRenderer.materials[i].SetColor("_Color", colorToPaint);
                        }
                    }
                }
            }
            catch
            {
        
            }

        }

        private void SetBoostGaugeText(float valueToDisplay, bool positive)
        {
            try
            {
                if (turboChargerBoostGaugePart.installed)
                {
                    if (electricityOn == true)
                    {
                        if (positive == true)
                        {
                            boostGaugeTextMesh.text = valueToDisplay.ToString("0.00");
                        }
                        else
                            boostGaugeTextMesh.text = "-" + valueToDisplay.ToString(".00");
                    }
                    else
                    {
                        boostGaugeTextMesh.text = "";

                    }
                }
            }
            catch
            {

            }
            
        }

        private void TriggerBlowOff()
        {
            if(turboChargerBigPart.installed && turboChargerBigBlowoffValvePart.installed)
            {
                if (turboBlowOffShot == null)
                {
                    CreateTurboBlowoff();
                }
                turbocharger_blowoff.Play();
            }


            if (turboChargerBigPart.installed && turboBlowOffShot != null && turboBlowOffShot.volume != 0.20f)
            {
                turboBlowOffShot.volume = 0.20f;
            }
            
            timeSinceLastBlowOff = 0;
            turbocharger_blowoffShotAllowed = false;
        }

        private void CalculateAndSetEnginePowerTurbo()
        {
            if (turboChargerBigPart.installed && turboChargerBigBlowoffValvePart.installed)
            {
                if (twinCarb_inst)
                {
                    othersSave.turbocharger_big_max_boost_limit = (2.2f + 0.05f);
                }
                else if (weberCarb_inst)
                {
                    othersSave.turbocharger_big_max_boost_limit = (2.2f + 0.30f);
                }
                if (othersSave.turbocharger_big_max_boost >= othersSave.turbocharger_big_max_boost_limit)
                {
                    othersSave.turbocharger_big_max_boost = othersSave.turbocharger_big_max_boost_limit;
                }
            }
            else if (turboChargerSmallPart.installed && !turboChargerBigBlowoffValvePart.installed)
            {
                if (twinCarb_inst && turboChargerIntercoolerPart.installed && turbocharger_small_airfilter_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.15f + 0.19f);
                }
                else if (twinCarb_inst && turboChargerIntercoolerPart.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.15f);
                }
                else if (twinCarb_inst && turbocharger_small_airfilter_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.00f + 0.11f);
                }
                else if (weberCarb_inst && turboChargerIntercoolerPart.installed && turbocharger_small_airfilter_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.11f + 0.15f + 0.19f);
                }
                else if (weberCarb_inst && turboChargerIntercoolerPart.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.11f + 0.15f);
                }
                else
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f);
                }
                if (othersSave.turbocharger_small_max_boost >= othersSave.turbocharger_small_max_boost_limit)
                {
                    othersSave.turbocharger_small_max_boost = othersSave.turbocharger_small_max_boost_limit;
                }
            }

            if (electricityOn == true)
            {
                if (turboChargerBigPart.installed && turboChargerBigBlowoffValvePart.installed)
                {
                    if (ecu_mod_installed)
                    {
                        if (ecu_mod_SmartEngineModule == null)
                        {
                            ecu_mod_SmartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
                        }
                        if(ecu_mod_SmartEngineModule != null)
                        {
                            Component ecu_mod_ModCommunication = ecu_mod_SmartEngineModule.GetComponent("ModCommunication");
                            Type compType = ecu_mod_ModCommunication.GetType();
                            FieldInfo alsEnabledInfo = compType.GetField("alsEnabled");
                            bool alsEnabled = (bool)alsEnabledInfo.GetValue(ecu_mod_ModCommunication);
                            if (alsEnabled)
                            {
                                newTurboChargerBar = Convert.ToSingle(Math.Log(8000 / 2800, 100)) * 10f;
                            }
                            else
                            {
                                newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 2800, 100)) * 10f;
                            }
                        }
                        else
                        {
                            newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 2800, 100)) * 10f;
                        }
                    }
                    else
                    {
                        newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 2800, 100)) * 10f;
                    }


                    if (turboChargerBigPart.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.turbocharger_big_wear <= 0)
                            {
                                partsWearSave.turbocharger_big_wear = 0;
                            }
                            else if (timer_wear_turbocharger_big >= 0.5f)
                            {
                                timer_wear_turbocharger_big = 0;
                                partsWearSave.turbocharger_big_wear -= (newTurboChargerBar * 0.025f);
                            }
                            if(partsWearSave.turbocharger_big_wear < 25f)
                            {
                                if (!turboGrindingLoop.isPlaying)
                                {
                                    turboGrindingLoop.Play();
                                }
                                turboGrindingLoop.volume = engineRPM * 0.00008f;
                                turboGrindingLoop.pitch = engineRPM * 0.00012f;
                            }

                        }
                        else
                        {
                            timer_wear_turbocharger_big = 0;
                        }
                    }
                    else if (turboChargerSmallPart.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.turbocharger_small_wear <= 0)
                            {
                                partsWearSave.turbocharger_small_wear = 0;
                            }
                            else if (timer_wear_turbocharger_small >= 0.5f)
                            {
                                timer_wear_turbocharger_small = 0;
                                if (turbocharger_small_airfilter_part.installed)
                                {
                                    partsWearSave.turbocharger_small_wear -= (newTurboChargerBar * ((0.025f + 0.035f) - (partsWearSave.airfilter_wear / 10000)));
                                }
                                else
                                {
                                    partsWearSave.turbocharger_small_wear -= (newTurboChargerBar * 0.025f);
                                }
                                if (partsWearSave.turbocharger_big_wear < 25f)
                                {
                                    if (!turboGrindingLoop.isPlaying)
                                    {
                                        turboGrindingLoop.Play();
                                    }
                                    turboGrindingLoop.volume = engineRPM * 0.00008f;
                                    turboGrindingLoop.pitch = engineRPM * 0.00012f;
                                }
                            }
                        }
                        else
                        {
                            timer_wear_turbocharger_small = 0;
                        }
                    }
                    if (turbocharger_small_airfilter_part.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.airfilter_wear <= 0)
                            {
                                partsWearSave.airfilter_wear = 0;
                            }
                            else if (timer_wear_airfilter >= 0.5f)
                            {
                                timer_wear_airfilter = 0;
                                partsWearSave.airfilter_wear -= (newTurboChargerBar * 0.035f);
                            }
                        }
                        else
                        {
                            timer_wear_airfilter = 0;
                        }
                    }
                    if (turboChargerIntercoolerPart.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.intercooler_wear <= 0)
                            {
                                partsWearSave.intercooler_wear = 0;
                            }
                            else if (timer_wear_intercooler >= 0.5f)
                            {
                                timer_wear_intercooler = 0;
                                partsWearSave.intercooler_wear -= (newTurboChargerBar * 0.035f);
                            }
                        }
                        else
                        {
                            timer_wear_intercooler = 0;
                        }
                    }

                    


                    if (newTurboChargerBar > 0f)
                    {
                        if (newTurboChargerBar > othersSave.turbocharger_big_max_boost)
                        {
                            newTurboChargerBar = othersSave.turbocharger_big_max_boost;
                        }
                        

                        if (partsWearSave.intercooler_wear >= 75f)
                        {
                            
                        }
                        else if (partsWearSave.intercooler_wear >= 50f)
                        {
                            newTurboChargerBar /= 1.2f;
                        }
                        else if (partsWearSave.intercooler_wear >= 25f)
                        {
                            newTurboChargerBar /= 1.4f;
                        }
                        else if (partsWearSave.intercooler_wear >= 15f)
                        {
                            newTurboChargerBar /= 1.8f;
                        }
                        else if (partsWearSave.intercooler_wear < 15f)
                        {
                            newTurboChargerBar = 0;
                        }

                        SetBoostGaugeText(newTurboChargerBar, true);
                        _enginePowerMultiplier.Value = (0.90f + (newTurboChargerBar * 1.5f));
                    }
                    else
                    {
                        SetBoostGaugeText(0.10f, false);
                        _enginePowerMultiplier.Value = 0.90f;
                    }
                }
                else if (turboChargerSmallPart.installed && !turboChargerBigBlowoffValvePart.installed)
                {
                    if (ecu_mod_installed)
                    {
                        if (ecu_mod_SmartEngineModule == null)
                        {
                            ecu_mod_SmartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
                        }
                        if (ecu_mod_SmartEngineModule != null)
                        {
                            Component ecu_mod_ModCommunication = ecu_mod_SmartEngineModule.GetComponent("ModCommunication");
                            Type compType = ecu_mod_ModCommunication.GetType();
                            FieldInfo alsEnabledInfo = compType.GetField("alsEnabled");
                            bool alsEnabled = (bool)alsEnabledInfo.GetValue(ecu_mod_ModCommunication);
                            if (alsEnabled)
                            {
                                newTurboChargerBar = Convert.ToSingle(Math.Log(8000 / 1600, 10)) * 2.2f;
                            }
                            else
                            {
                                newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 1600, 10)) * 2.2f;
                            }
                        }
                        else
                        {
                            newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 1600, 10)) * 2.2f;
                        }
                    }
                    else
                    {
                        newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 1600, 10)) * 2.2f;
                    }

                    

                    if (newTurboChargerBar > 0f)
                    {
                        if (newTurboChargerBar > othersSave.turbocharger_small_max_boost)
                        {
                            newTurboChargerBar = othersSave.turbocharger_small_max_boost;
                        }
                        

                        if (partsWearSave.intercooler_wear >= 75f)
                        {

                        }
                        else if (partsWearSave.intercooler_wear >= 50f)
                        {
                            newTurboChargerBar /= 1.2f;
                        }
                        else if (partsWearSave.intercooler_wear >= 25f)
                        {
                            newTurboChargerBar /= 1.4f;
                        }
                        else if (partsWearSave.intercooler_wear >= 15f)
                        {
                            newTurboChargerBar /= 1.8f;
                        }
                        else if (partsWearSave.intercooler_wear < 15f)
                        {
                            newTurboChargerBar = 0;
                        }

                        SetBoostGaugeText(newTurboChargerBar, true);
                        _enginePowerMultiplier.Value = (0.96f + (newTurboChargerBar * 1.5f));
                    }
                    else
                    {
                        SetBoostGaugeText(0.04f, false);
                        _enginePowerMultiplier.Value = 0.96f;
                    }

                    /*
                    if (small_turbo_fire_fx != null)
                    {
                        if (newTurboChargerBar >= othersSave.turbocharger_small_max_boost - 0.1f)
                        {
                            if (!small_turbo_fire_fx.isPlaying)
                            {
                                small_turbo_fire_fx.Play();
                            }
                        }
                        else
                        {
                            small_turbo_fire_fx.Stop();
                        }
                    }
                    */
                }
                if(satsumaDriveTrain.rpm >= 400)
                {
                    CheckPartsWear();
                }
            }
            else
            {
                timer_wear_turbocharger_big = 0;
                timer_wear_turbocharger_small = 0;
                timer_wear_airfilter = 0;
                timer_wear_intercooler = 0;

                SetBoostGaugeText(0.00f, false);
                enginePowerMultiplier = 0;

            }
                
           
        }

        private void CreateTurboGrindingLoop()
        {
            turboGrindingLoop = turboChargerBigPart.rigidPart.AddComponent<AudioSource>();
            turbocharger_grinding_loop.audioSource = turboGrindingLoop;
            turbocharger_grinding_loop.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "grinding sound.wav"), true, false);

            turboGrindingLoop.rolloffMode = AudioRolloffMode.Linear;
            turboGrindingLoop.minDistance = 1;
            turboGrindingLoop.maxDistance = 10;
            turboGrindingLoop.spatialBlend = 0.6f;
            turboGrindingLoop.loop = true;
        }

        public void CreateTurboLoopBig()
        {
            if (turboGrindingLoop == null)
            {
                CreateTurboGrindingLoop();
            }
            //Creates the TurboLoop loading the file "turbocharger_loop.wav" from the Asset folder of the mod
            //And setting it to loop

            turboLoopBig = turboChargerBigPart.rigidPart.AddComponent<AudioSource>();
            turbocharger_loop_big.audioSource = turboLoopBig;
            turbocharger_loop_big.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "turbocharger_loop.wav"), true, false);

            

            turboLoopBig.rolloffMode = AudioRolloffMode.Custom;
            turboLoopBig.minDistance = 1;
            turboLoopBig.maxDistance = 10;
            turboLoopBig.spatialBlend = 1;
            turboLoopBig.loop = true;
            turbocharger_loop_big.Play();

        }
        public void CreateTurboLoopSmall()
        {
            turboLoopSmall = turboChargerSmallPart.rigidPart.AddComponent<AudioSource>();
            turbocharger_loop_small.audioSource = turboLoopSmall;
            turbocharger_loop_small.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "turbocharger_loop.wav"), true, false);

            turboLoopSmall.rolloffMode = AudioRolloffMode.Custom;
            turboLoopSmall.minDistance = 1;
            turboLoopSmall.maxDistance = 10;
            turboLoopSmall.spatialBlend = 1;
            turboLoopSmall.loop = true;
            turbocharger_loop_small.Play();
        }

        public void CreateTurboBlowoff()
        {
            //Creates the TurboBlowoff  loading the file "turbocharger_blowoff.wav" from the Asset folder of the mod

            turboBlowOffShot = turboChargerBigBlowoffValvePart.rigidPart.AddComponent<AudioSource>();
            turbocharger_blowoff.audioSource = turboBlowOffShot;

            turbocharger_blowoff.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "turbocharger_blowoff.wav"), true, false);
            turboBlowOffShot.minDistance = 1;
            turboBlowOffShot.maxDistance = 10;
            turboBlowOffShot.spatialBlend = 1;
        }

        private void DetectPaintingPart()
        {
            try
            {
                FsmGameObject itemInHand = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("ItemPivot");
                if (itemInHand.Value.GetComponentInChildren<MeshRenderer>() != null)
                {

                    if (itemInHand.Value.GetComponentInChildren<MeshRenderer>().name == "spray can(itemx)" && Input.GetKey(KeyCode.F))
                    {
                        sprayCansMeshRenders = itemInHand.Value.GetComponentsInChildren<MeshRenderer>();
                        isItemInHand = true;
                    }
                    else if(itemInHand.Value.GetComponentInChildren<MeshRenderer>().name == "spray can(itemx)" && isItemInHand == true)
                    {
                        isItemInHand = false;
                    }

                }
                if (Camera.main != null)
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
                    {
                        GameObject gameObjectHit;
                        gameObjectHit = hit.collider?.gameObject;
                        if (gameObjectHit != null)
                        {
                            if (hit.collider)
                            {
                                if (isItemInHand)
                                {
                                    if (
                                     gameObjectHit.name == "Racing Turbocharger Hood"
                                     || gameObjectHit.name == "Racing Turbocharger Hood(Clone)"
                                     || gameObjectHit.name == "Intercooler"
                                     || gameObjectHit.name == "Intercooler(Clone)"
                                     || gameObjectHit.name == "Racing Turbocharger"
                                     || gameObjectHit.name == "Racing Turbocharger(Clone)"
                                     || gameObjectHit.name == "GT Turbocharger"
                                     || gameObjectHit.name == "GT Turbocharger(Clone)"
                                     || gameObjectHit.name == "Weber Manifold"
                                     || gameObjectHit.name == "Weber Manifold(Clone)"
                                     || gameObjectHit.name == "TwinCarb Manifold"
                                     || gameObjectHit.name == "TwinCarb Manifold(Clone)"
                                     || gameObjectHit.name == "Racing Turbocharger Blowoff Valve"
                                     || gameObjectHit.name == "Racing Turbocharger Blowoff Valve(Clone)"
                                     || gameObjectHit.name == "GT Turbocharger Airfilter"
                                     || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)"
                                     )
                                    {
                                        if (Input.GetMouseButton(0))
                                        {
                                            pickedUPsprayCanColor = sprayCansMeshRenders[1].material.color;
                                            string test2 = sprayCansMeshRenders[1].material.name;
                                            int colorID = 0;
                                            if (sprayCansMeshRenders[1].material.name == "colormatte01 (Instance)")
                                            {
                                                colorID = 13;
                                            }
                                            else
                                            {
                                                colorID = Convert.ToInt32(sprayCansMeshRenders[1].material.name.Replace("color", "").Replace(" (Instance)", ""));
                                            }
                                            int arrIndex = colorID - 1;
                                            if (arrIndex < 0 || arrIndex > 12)
                                            {
                                                arrIndex = 0;
                                            }


                                            if (gameObjectHit.name == "Racing Turbocharger Hood" || gameObjectHit.name == "Racing Turbocharger Hood(Clone)")
                                            {
                                                hoodColor = pickedUPsprayCanColor;

                                            }
                                            if (gameObjectHit.name == "Intercooler" || gameObjectHit.name == "Intercooler(Clone)")
                                            {
                                                intercoolerColor = modSprayColors[arrIndex];
                                                originalIntercoolerColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "Racing Turbocharger" || gameObjectHit.name == "Racing Turbocharger(Clone)")
                                            {
                                                turbochargerBigColor = modSprayColors[arrIndex];
                                                originalTurbocchargerBigColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "GT Turbocharger" || gameObjectHit.name == "GT Turbocharger(Clone)")
                                            {
                                                turbochargerSmallColor = modSprayColors[arrIndex];
                                                originalTurbochargerSmallColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "Weber Manifold" || gameObjectHit.name == "Weber Manifold(Clone)")
                                            {
                                                turbochargerManifoldWeberColor = modSprayColors[arrIndex];
                                                originalTurbochargerManifoldWeberColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "TwinCarb Manifold" || gameObjectHit.name == "TwinCarb Manifold(Clone)")
                                            {
                                                turbochargerManifoldTwinCarbColor = modSprayColors[arrIndex];
                                                originalTurbochargerManifoldTwinCarbColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "Racing Turbocharger Blowoff Valve" || gameObjectHit.name == "Racing Turbocharger Blowoff Valve(Clone)")
                                            {
                                                turbochargerBigBlowoffValveColor = modSprayColors[arrIndex];
                                                originalTurbochargerBigBlowoffValveColor = pickedUPsprayCanColor;
                                            }
                                            if(gameObjectHit.name == "GT Turbocharger Airfilter" || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)")
                                            {
                                                turbocharger_small_airfilter_color = modSprayColors[arrIndex];
                                                original_turbocharger_small_airfilter_color = pickedUPsprayCanColor;
                                            }

                                            MeshRenderer partRenderer = hit.collider.GetComponentInChildren<MeshRenderer>();
                                            for (int i = 0; i < partRenderer.materials.Length; i++)
                                            {
                                                if (partRenderer.materials[i].name == "Red_Acent (Instance)" || partRenderer.materials[i].name == "CAR_PAINT_REGULAR (Instance)")
                                                {
                                                    if (useDefaultColors != true)
                                                    {
                                                        partRenderer.materials[i].SetColor("_Color", modSprayColors[arrIndex]);
                                                    }

                                                }
                                            }

                                        }
                                    }
                                }
                                else if (gameObjectHit.name == "Racing Turbocharger Blowoff Valve" || gameObjectHit.name == "Racing Turbocharger Blowoff Valve(Clone)" ||gameObjectHit.name == "Racing Turbocharger" || gameObjectHit.name == "Racing Turbocharger(Clone)"){
                                    if (turboChargerBigBlowoffValvePart.installed)
                                    {
                                        ModClient.guiInteract("Increase/Decrease Max Boost: " + othersSave.turbocharger_big_max_boost.ToString("0.00"));
                                        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                                        {
                                            if (othersSave.turbocharger_big_max_boost < othersSave.turbocharger_big_max_boost_limit)
                                            {
                                                othersSave.turbocharger_big_max_boost += 0.05f;
                                            }
                                        }
                                        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                                        {
                                            if ((othersSave.turbocharger_big_max_boost - 0.05f) > 1.6f)
                                            {
                                                othersSave.turbocharger_big_max_boost -= 0.05f;
                                            }
                                        }
                                    }
                                }
                                else if(gameObjectHit.name == "GT Turbocharger" || gameObjectHit.name == "GT Turbocharger(Clone)")
                                {
                                    if (turboChargerSmallPart.installed)
                                    {
                                        ModClient.guiInteract("Increase/Decrease Max Boost: " + othersSave.turbocharger_small_max_boost.ToString("0.00"));
                                        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                                        {
                                            if (othersSave.turbocharger_small_max_boost < othersSave.turbocharger_small_max_boost_limit)
                                            {
                                                othersSave.turbocharger_small_max_boost += 0.01f;
                                            }
                                        }
                                        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                                        {
                                            if ((othersSave.turbocharger_small_max_boost - 0.01f) > 1f)
                                            {
                                                othersSave.turbocharger_small_max_boost -= 0.01f;
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }

                    }
                }
            }
            catch
            {

            }
            
        }

        private static void ToggleUseDefaultColors()
        {
            useDefaultColors = !useDefaultColors;
        }

        private static void PosReset()
        {
            if (!turboChargerBigPart.installed)
            {
                turboChargerBigPart.activePart.transform.position = turboChargerBigPart.defaultPartSaveInfo.position;
            }

            if (!turboChargerBigIntercoolerTubePart.installed)
            {
                turboChargerBigIntercoolerTubePart.activePart.transform.position = turboChargerBigIntercoolerTubePart.defaultPartSaveInfo.position;
            }

            if (!turboChargerBigExhaustInletTubePart.installed)
            {
                turboChargerBigExhaustInletTubePart.activePart.transform.position = turboChargerBigExhaustInletTubePart.defaultPartSaveInfo.position;
            }

            if (!turboChargerBigExhaustOutletTubePart.installed)
            {
                turboChargerBigExhaustOutletTubePart.activePart.transform.position = turboChargerBigExhaustOutletTubePart.defaultPartSaveInfo.position;
            }

            if (!turboChargerBigBlowoffValvePart.installed)
            {
                turboChargerBigBlowoffValvePart.activePart.transform.position = turboChargerBigBlowoffValvePart.defaultPartSaveInfo.position;
            }

            if (!turboChargerSmallPart.installed)
            {
                turboChargerSmallPart.activePart.transform.position = turboChargerSmallPart.defaultPartSaveInfo.position;
            }

            if (!turboChargerSmallIntercoolerTubePart.installed)
            {
                turboChargerSmallIntercoolerTubePart.activePart.transform.position = turboChargerSmallIntercoolerTubePart.defaultPartSaveInfo.position;
            }

            if (!turboChargerSmallExhaustInletTubePart.installed)
            {
                turboChargerSmallExhaustInletTubePart.activePart.transform.position = turboChargerSmallExhaustInletTubePart.defaultPartSaveInfo.position;
            }

            if (!turboChargerSmallExhaustOutletTubePart.installed)
            {
                turboChargerSmallExhaustOutletTubePart.activePart.transform.position = turboChargerSmallExhaustOutletTubePart.defaultPartSaveInfo.position;
            }


            if (!turboChargerHoodPart.installed)
            {
                turboChargerHoodPart.activePart.transform.position = turboChargerHoodPart.defaultPartSaveInfo.position;
            }

            if (!turboChargerManifoldWeberPart.installed)
            {
                turboChargerManifoldWeberPart.activePart.transform.position = turboChargerManifoldWeberPart.defaultPartSaveInfo.position;
            }

            if (!turboChargerManifoldTwinCarbPart.installed)
            {
                turboChargerManifoldTwinCarbPart.activePart.transform.position = turboChargerManifoldTwinCarbPart.defaultPartSaveInfo.position;
            }

            if (!turboChargerBoostGaugePart.installed)
            {
                turboChargerBoostGaugePart.activePart.transform.position = turboChargerBoostGaugePart.defaultPartSaveInfo.position;
            }
            if (!turboChargerIntercoolerPart.installed)
            {
                turboChargerIntercoolerPart.activePart.transform.position = turboChargerIntercoolerPart.defaultPartSaveInfo.position;
            }

            if (!turboChargerIntercoolerManifoldTubeWeberPart.installed)
            {
                turboChargerIntercoolerManifoldTubeWeberPart.activePart.transform.position = turboChargerIntercoolerManifoldTubeWeberPart.defaultPartSaveInfo.position;
            }

            if (!turboChargerIntercoolerManifoldTubeTwinCarbPart.installed)
            {
                turboChargerIntercoolerManifoldTubeTwinCarbPart.activePart.transform.position = turboChargerIntercoolerManifoldTubeTwinCarbPart.defaultPartSaveInfo.position;
            }

            if (!turbocharger_small_airfilter_part.installed)
            {
                turbocharger_small_airfilter_part.activePart.transform.position = turbocharger_small_airfilter_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_small_manifold_twinCarb_tube_part.installed)
            {
                turbocharger_small_manifold_twinCarb_tube_part.activePart.transform.position = turbocharger_small_manifold_twinCarb_tube_part.defaultPartSaveInfo.position;
            }
        }

        private void AddPartsNames()
        {
            //Big Turbocharger
            turbocharger_big.name = "Racing Turbocharger";
            turbocharger_big_intercooler_tube.name = "Racing Turbocharger Intercooler Tube";
            turbocharger_big_exhaust_inlet_tube.name = "Racing Turbocharger Exhaust Inlet Tube";
            turbocharger_big_exhaust_outlet_tube.name = "Racing Turbocharger Exhaust Outlet Tube";
            turbocharger_big_blowoff_valve.name = "Racing Turbocharger Blowoff Valve";
            turbocharger_exhaust_header.name = "Turbocharger Exhaust Header";

            //Small Turbocharger
            turbocharger_small.name = "GT Turbocharger";
            turbocharger_small_intercooler_tube.name = "GT Turbocharger Intercooler Tube";
            turbocharger_small_exhaust_inlet_tube.name = "GT Turbocharger Exhaust Inlet Tube";
            turbocharger_small_exhaust_outlet_tube.name = "GT Turbocharger Exhaust Outlet Tube";
            turbocharger_small_manifold_twinCarb_tube.name = "GT Turbocharger Manifold TwinCarb Tube";
            turbocharger_small_airfilter.name = "GT Turbocharger Airfilter";

            //Other Turbo parts
            turbocharger_hood.name = "Racing Turbocharger Hood";
            turbocharger_manifold_weber.name = "Weber Manifold";
            turbocharger_manifold_twinCarb.name = "TwinCarb Manifold";
            turbocharger_intercooler.name = "Intercooler";
            turbocharger_intercooler_manifold_tube_weber.name = "Weber Intercooler-Manifold Tube";
            turbocharger_intercooler_manifold_tube_twinCarb.name = "TwinCarb Intercooler-Manifold Tube";
            turbocharger_boost_gauge.name = "Boost Gauge";
        }

        private void AddPartsTrigger(GameObject originalCylinerHead)
        {
            turbocharger_big_Trigger = new Trigger("TurbochargerBigTrigger", originalCylinerHead, turbocharger_big_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_big_intercoolerTubeTrigger = new Trigger("TurbochargerBigIntercoolerTubeTrigger", satsuma, turbocharger_big_intercooler_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_big_exhaustInletTubeTrigger = new Trigger("TurbochargerBigExhaustInletTubeTrigger", originalCylinerHead, turbocharger_big_exhaust_inlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_big_exhaustOutletTubeTrigger = new Trigger("TurbochargerBigExhaustOutletTubeTrigger", originalCylinerHead, turbocharger_small_exhaust_outlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_big_blowoffValveTrigger = new Trigger("TurbochargerBigBlowoffValveTrigger", satsuma, turbocharger_big_blowoff_valve_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);

            turbocharger_exhaust_header_Trigger = new Trigger("turbocharger_big_exhaust_header_Trigger", originalCylinerHead, turbocharger_exhaust_header_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);

            turbocharger_small_Trigger = new Trigger("TurbochargerSmallTrigger", originalCylinerHead, turbocharger_small_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_small_intercoolerTubeTrigger = new Trigger("TurbochargerSmallIntercoolerTubeTrigger", satsuma, turbocharger_small_intercooler_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_small_exhaustInletTubeTrigger = new Trigger("TurbochargerSmallExhaustInletTubeTrigger", originalCylinerHead, turbocharger_small_exhaust_inlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_small_exhaustOutletTubeTrigger = new Trigger("TurbochargerSmallExhaustOutletTubeTrigger", originalCylinerHead, turbocharger_small_exhaust_outlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_small_manifold_twinCarb_tube_Trigger = new Trigger("turbocharger_small_manifold_twinCarb_tube_Trigger", originalCylinerHead, turbocharger_small_manifold_twinCarb_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_small_airfilter_Trigger = new Trigger("turbocharger_small_manifold_twinCarb_tube_Trigger", originalCylinerHead, turbocharger_small_airfilter_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);

            turbocharger_hoodTrigger = new Trigger("TurbochargerHoodTrigger", satsuma, turbocharger_hood_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_manifoldWeberTrigger = new Trigger("TurbochargerManifoldWeberTrigger", originalCylinerHead, turbocharger_manifold_weber_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_manifoldTwinCarbTrigger = new Trigger("TurbochargerManifoldTwinCarbTrigger", originalCylinerHead, turbocharger_manifold_twinCarb_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);

            turbocharger_boostGaugeTrigger = new Trigger("TurbochargerBoostGaugeTrigger", GameObject.Find("dashboard(Clone)"), turbocharger_boost_gauge_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_intercoolerTrigger = new Trigger("TurbochargerIntercoolerTrigger", satsuma, turbocharger_intercooler_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_intercoolerManifoldTubeWeberTrigger = new Trigger("TurbochargerIntercoolerManifoldTubeWeberTrigger", satsuma, turbocharger_intercooler_manifold_tube_weber_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);
            turbocharger_intercoolerManifoldTubeTwinCarbTrigger = new Trigger("TurbochargerIntercoolerManifoldTubeTwinCarbTrigger", satsuma, turbocharger_intercooler_manifold_tube_twinCarb_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), false);

        }

        private bool CheckCloseToPosition(Vector3 positionOfPartTocheck, Vector3 position, float minimumDistance)
        {
            if (Vector3.Distance(positionOfPartTocheck, position) <= minimumDistance)
                return true;
            else
                return false;
        }
    }
}