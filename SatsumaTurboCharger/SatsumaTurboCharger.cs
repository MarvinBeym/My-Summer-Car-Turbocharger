﻿using HutongGames.PlayMaker;
using ModApi;
using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using SatsumaTurboCharger.gui;
using SatsumaTurboCharger.old_file_checker;
using SatsumaTurboCharger.shop;
using SatsumaTurboCharger.turbo;
using SatsumaTurboCharger.wear;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger
{

    public class SatsumaTurboCharger : Mod
    {
        /* Todo:
         *  -------------------------------------------------
         *  Draw power grapth/turbo grapth ?

         *  check for air fuel mixture and decrease/increase boost
         *  make n2o usable
         *  increase fuel consumption when turbo is used/installed
         *  more grip ?
         *  -------------------------------------------------
         */

        /* Changelog v2.2
         * Changed blowoff valve sound to old flutter sound.
         * Code optimization
         * Loading time improvements
         * Code rework
         * Parts renamed to replace "Turbocharger" with shorter form "Turbo"
         * Part names have changed DELETE the screw save file!!!!
         * Changed turbo logic
         * Changed turbo boost curve
         * Improved turbo logic
         * Made racing turbo more aggressive (Like a lot. It will kill you)
         * Player now has to be in the car for the turbo logic to work (sound and turbine rotation is still played)
         * Reworked parts wear
         * Reworked debug gui
         * Reworked turbo logic
         * Reworked blowoff logic
         * Reworked backfire logic
         * Added old file renaming tool
         * Added copying of old save files to folder "AUTO_BACKUP" before changing name
         * 
         */

        /*FIX
         * ScrewablePartAPI always adds .txt to end of screw save.
         * -> Remove that 
         * Make parts paintable by Fleetar (metallic & normal)
         * cruise control not working -> Radex video
         *  Prevent turbo from working if any stock exhaust part is installed.
         *  Change PaintingSystem into MonoBehaviour
         *  if cruise control is off and a speed was set it should enable cruise control and make it reach the originally set speed
         *  if cruise control is off and on button is pressed it should set the speed to the current car speed
         *  Do dynotest with big and small turbo and adjust max boost possible to make it more realistic
         *  
         *  add screws to original turboBig_hood.
         *  ScrewableAPI: screws hitbox different than original (make larger)
         *  check/make sure parts are only installable if parts connecting them are

         *  make fire of exhaust light up house?????
         *  ScrewablePartAPI: screwdriver works on 6mm bolts
         *  Add debug information that parts are not all installed/screwed
         *  Add debug information how many parts are missing
         *  -------------------------------------------------
         *  Known bugs:
         *  
         *  
         *  Use Key still has to be "F" for Painting
         */

        public override string ID => "SatsumaTurboCharger"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing Turbocharger"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "2.1.7"; //Version
        public override bool UseAssetsFolder => true;

        public SaveFileRenamer saveFileRenamer;
        public Logger logger;
        public GuiDebug guiDebug;

        //Kits
        private Kit turboBig_kit;
        private Kit manifoldWeber_kit;
        private Kit manifoldTwinCarb_kit;



        //Saves
        public PartBuySave partBuySave { get; set; }
        public BoostSave boostSave { get; set; }
        public PartsWearSave partsWearSave { get; set; }

        //
        //Install & Spawn Locations
        //

        //Big Turbo
        private Vector3 turboBig_installLocation = new Vector3(-0.2705f, -0.064f, 0.273f);                              //Cylinder Head
        private Vector3 turboBig_intercooler_tube_installLocation = new Vector3(0.318f, -0.041f, 1.52f);                //Satsuma       //Done

        private Vector3 turboBig_exhaust_inlet_tube_installLocation = new Vector3(-0.179f, -0.1506f, -0.037f);          //Cylinder Head
        private Vector3 turboBig_exhaust_outlet_tube_installLocation = new Vector3(-0.217f, -0.229f, -0.059f);          //Cylinder Head
        private Vector3 turboBig_blowoff_valve_installLocation = new Vector3(0.315f, 0.258f, 1.332f);                  //Satsuma       //Done
        private Vector3 turboBig_exhaust_outlet_straight_installLocation = new Vector3(-0.316f, -0.2f, 0.307f);

        public static Vector3 turboBig_spawnLocation = new Vector3(1558.366f, 5f, 742.5068f);
        public static Vector3 turboBig_intercooler_tube_spawnLocation = new Vector3(1556.846f, 5f, 741.4836f);
        public static Vector3 turboBig_exhaust_inlet_tube_spawnLocation = new Vector3(1557.866f, 5.5f, 741.9728f);
        public static Vector3 turboBig_exhaust_outlet_tube_spawnLocation = new Vector3(1557.352f, 5f, 741.7303f);

 

        public static Vector3 turboBig_blowoff_valve_spawnLocation = new Vector3(1555.136f, 5.8f, 737.2324f);
        public static Vector3 turboBig_exhaust_outlet_straight_spawnLocation = new Vector3(1555.136f, 5.8f, 737.2324f);

        //Small Turbo
        private Vector3 turboSmall_installLocation = new Vector3(-0.25f, -0.1665f, 0.0001f);                              //Cylinder Head
        private Vector3 turboSmall_manifold_twinCarb_tube_installLocation = new Vector3(-0.188f, -0.23f, 0.14f);        //Cylinder Head
        private Vector3 turboSmall_intercooler_tube_installLocation = new Vector3(0.316f, -0.041f, 1.518f);              //Satsuma
        private Vector3 turboSmall_exhaust_inlet_tube_installLocation = new Vector3(-0.0918f, -0.1774f, -0.094f);         //Cylinder Head
        private Vector3 turboSmall_exhaust_outlet_tube_installLocation = new Vector3(-0.1825f, -0.267f, -0.145f);         //Cylinder Head
        private Vector3 turboSmall_airfilter_installLocation = new Vector3(-0.25f, -0.04f, 0.0001f);                     //Cylinder Head

        public static Vector3 turboSmall_spawnLocation = new Vector3(1457.509f, -1.8f, 716.0f);
        public static Vector3 turboSmall_exhaust_inlet_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 715.5f);
        public static Vector3 turboSmall_exhaust_outlet_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 715.0f);
        public static Vector3 turboSmall_manifold_twinCarb_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 714.5f);
        public static Vector3 turboSmall_airfilter_spawnLocation = new Vector3(1555.174f, 5.8f, 736.9866f);
        public static Vector3 turboSmall_intercooler_tube_spawnLocation = new Vector3(1554.144f, 5f, 738.733f);

        //Other Parts
        private Vector3 turboBig_hood_installLocation = new Vector3(0.0f, 0.241f, 1.68f);                                   //Satsuma

        private Vector3 manifold_weber_installLocation = new Vector3(0f, -0.3f, 0.1f);                             //Cylinder Head

        private Vector3 manifold_twinCarb_installLocation = new Vector3(0.0075f, -0.265f, 0.006f);                 //Cylinder Head
        private Vector3 boost_gauge_installLocation = new Vector3(0.5f, -0.04f, 0.125f);                           //Dashboard
        private Vector3 intercooler_installLocation = new Vector3(0.0f, -0.162f, 1.6775f);                          //Satsuma
        private Vector3 intercooler_manifold_tube_weber_installLocation = new Vector3(-0.34f, -0.047f, 1.445f);    //Satsuma
        private Vector3 intercooler_manifold_tube_twinCarb_installLocation = new Vector3(-0.332f, -0.047f, 1.445f); //Satsuma
        private Vector3 exhaust_header_installLocation = new Vector3(-0.005f, -0.089f, -0.064f);               //Cylinder Head

        public static Vector3 exhaust_header_spawnLocation = new Vector3(1555.136f, 5.8f, 737.2324f);
        public static Vector3 turboBig_hood_spawnLocation = new Vector3(1559.46f, 5f, 742.296f);
        public static Vector3 manifold_weber_spawnLocation = new Vector3(1555.18f, 5.8f, 737.8821f);
        public static Vector3 manifold_twinCarb_spawnLocation = new Vector3(1555.07f, 5.8f, 737.6261f);
        public static Vector3 boost_gauge_spawnLocation = new Vector3(1555.383f, 5.8f, 737.058f);
        public static Vector3 intercooler_spawnLocation = new Vector3(1555.382f, 5.8f, 737.3588f);
        public static Vector3 intercooler_manifold_tube_weber_spawnLocation = new Vector3(1554.56f, 5f, 737.2017f);
        public static Vector3 intercooler_manifold_tube_twinCarb_spawnLocation = new Vector3(1554.339f, 5.5f, 737.913f);

        //Mods Shop
        private ShopItem modsShop;

        //
        //Game Objects
        //
        //Parts
        private GameObject satsuma;
        private GameObject weberCarb;
        private GameObject twinCarb;
        private GameObject racingExhaustPipe;
        private GameObject steelHeaders;
        private GameObject racingExhaustMuffler;

        private GameObject exhaustPipe;
        private GameObject headers;
        private GameObject exhaustMuffler;

        private GameObject originalCylinerHead;
        private GameObject exhaustFromMuffler;
        private GameObject exhaustFromEngine;
        private GameObject exhaustFromPipe;
        private Vector3 originalExhaustPipeRacePosition;
        private Quaternion originalExhaustPipeRaceRotation;
        private Transform originalExhaustPipeRaceParent;
        private static Drivetrain satsumaDriveTrain;
        private PlayMakerFSM carElectricsPower;
        //Power/Electricity
        private GameObject elect;
        private PlayMakerFSM power;

        //Inspection
        private PlayMakerFSM inspectionPlayMakerFsm;
        private FsmEvent inspectionFailedEvent;

        //Parts installed
        private FsmBool weberCarb_inst;
        private FsmBool twinCarb_inst;
        private FsmBool steelHeaders_inst;
        private FsmBool racingExhaustPipe_inst;
        private FsmBool racingExhaustMuffler_inst;
        private FsmBool headers_inst;
        private FsmBool exhaustPipe_inst;
        private FsmBool exhaustMuffler_inst;
        private FsmBool exhaustMufflerDualTip_inst;

        //Part Tightness
        private FsmFloat racingExhaustPipeTightness;
        private FsmFloat exhaustPipeTightness;
        //Engine Values
        //private FsmState n2oBottle;
        //private FsmFloat n2oBottlePSI;
        //private CarController satsumaCarController;
        //private Axles satsumaAxles;

        //NOS
        private FsmState n2oState;
        private FsmState no_n2oState;
        private GameObject N2O;
        private PlayMakerFSM n2oPlayMaker;

        //Painting System
        private MeshRenderer[] sprayCansMeshRenders;
        private Material regularCarPaintMaterial;
        private MeshRenderer turboBig_hood_renderer;

        //Audio

        private ModAudio turbocharger_loop_small = new ModAudio();
        private AudioSource turboLoopSmall;

        private ModAudio turbocharger_loop_big = new ModAudio();
        private ModAudio turbocharger_blowoff = new ModAudio();
        private ModAudio turbocharger_grinding_loop = new ModAudio();


        private AudioSource turboLoopBig;
        private AudioSource turboGrindingLoop;
        private AudioSource turboBlowOffShot;
        private AudioSource backfire_fx_big_turbo_exhaust_straight;

        //Mod Settings
        private static bool partsWearDEBUG = false;
        private static bool turboValuesDEBUG = false;
        private static bool useDefaultColors = false;

        private Settings debugGuiSetting = new Settings("debugGuiSetting", "Show DEBUG GUI", false);

        private Settings useDefaultColorsSetting = new Settings("useDefaultColors", "Use default game colors for painting", false, new Action(ToggleUseDefaultColors));
        private Settings resetPosSetting = new Settings("resetPos", "Reset", Helper.WorkAroundAction);
        private static Settings toggleNewGearRatios = new Settings("toggleNewGearRatios", "Enable/Disable New Gear Ratios", false, new Action(ToggleNewGearRatios));

        public Settings partsWearSetting = new Settings("partsWearSetting", "Use parts wear system", true);
        public Settings rotateTurbineSetting = new Settings("rotateTurbineSetting", "Allow turbo turbine rotation", false);
        public Settings backfireEffectSetting = new Settings("backfireEffectSetting", "Allow backfire effect for turbo", false);
        //Car values
        private float engineRPM = 0;
        private float enginePowerMultiplier;
        private float enginePowerCurrent;
        private FsmFloat _enginePowerMultiplier;
        private float newTurboChargerBar = 0;
        private bool isItemInHand;

        //
        //ModApi Parts
        //
        //Big Turbo
        public SimplePart turboBig_part;
        public SimplePart turboBig_intercooler_tube_part;
        public SimplePart turboBig_exhaust_inlet_tube_part;
        public SimplePart turboBig_exhaust_outlet_tube_part;
        public SimplePart turboBig_blowoff_valve_part;
        public SimplePart turboBig_exhaust_outlet_straight_part;

        public SimplePart turboBig_hood_part;
        public SimplePart exhaust_header_part;

        //Small Turbo
        public SimplePart turboSmall_part;
        public SimplePart turboSmall_intercooler_tube_part;
        public SimplePart turboSmall_exhaust_inlet_tube_part;
        public SimplePart turboSmall_exhaust_outlet_tube_part;
        public SimplePart turboSmall_airfilter_part;
        public SimplePart turboSmall_manifold_twinCarb_tube_part;

        //Other Parts
        public SimplePart manifold_weber_part;
        public SimplePart manifold_twinCarb_part;
        public SimplePart boost_gauge_part;
        public SimplePart intercooler_part;
        public SimplePart intercooler_manifold_tube_weber_part;
        public SimplePart intercooler_manifold_tube_twinCarb_part;

        public List<SimplePart> partsList = new List<SimplePart>();
        public List<SimplePart> bigPartsList;
        public List<SimplePart> smallPartsList;
        public List<SimplePart> otherPartsList;


        private Turbo racingTurbo;
        private Turbo gtTurbo;

        private Wear racingTurboWear;
        private Wear intercoolerWear;
        private Wear gtTurboWear;
        private Wear gtTurboAirfilterWear;

        //Logic
        private GT_Turbocharger_Logic turboSmall_logic;

        //Part Coloring
        private Color pickedUPsprayCanColor;

        private Color turboBig_hoodColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turboBigColor = new Color(0.800f, 0.800f, 0.800f);
        private Color intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turboBigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turboSmall_airfilter_color = new Color(0.800f, 0.800f, 0.800f);

        private Color originalTurbocchargerBigColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalIntercoolerColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalturboBigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
        private Color original_turboSmall_airfilter_color = new Color(0.800f, 0.800f, 0.800f);

        private Color[] modSprayColors = new Color[13];
        public static bool colorHasToChange = false;

        //
        //Save File Locations
        //
        private const string logger_saveFile = "turbo_mod.log";
        //Big Turbo
        private const string turboBig_SaveFile = "turboBig_saveFile.json";
        private const string turboBig_intercooler_tube_SaveFile = "turboBig_intercooler_tube_saveFile.json";
        private const string turboBig_exhaust_inlet_tube_SaveFile = "turboBig_exhaustInlet_tube_saveFile.json";
        private const string turboBig_exhaust_outlet_tube_SaveFile = "turboBig_exhaustOutlet_tube_saveFile.json";
        private const string turboBig_blowoff_valve_SaveFile = "turboBig_blowoffValve_saveFile.json";
        private const string turboBig_exhaust_outlet_straight_SaveFile = "turboBig_exhaustOutlet_straight_saveFile.json";
        private const string turboBig_hood_SaveFile = "turboBig_hood_saveFile.json";

        //Small Turbo
        private const string turboSmall_SaveFile = "turboSmall_saveFile.json";
        private const string turboSmall_intercooler_tube_SaveFile = "turboSmall_intercooler_tube_saveFile.json";
        private const string turboSmall_exhaust_inlet_tube_SaveFile = "turboSmall_exhaustInlet_tube_saveFile.json";
        private const string turboSmall_exhaust_outlet_tube_SaveFile = "turboSmall_exhaustOutlet_tube_saveFile.json";
        private const string turboSmall_manifold_twinCarb_tube_SaveFile = "turboSmall_manifoldTwinCarb_tube_saveFile.json";
        private const string turboSmall_airfilter_SaveFile = "turboSmall_airfilter_saveFile.json";

        //Other Parts
        private const string exhaust_header_SaveFile = "exhaustHeader_saveFile.json";
        private const string manifold_weber_SaveFile = "manifoldWeber_saveFile.json";
        private const string manifold_twinCarb_SaveFile = "manifoldTwinCarb_saveFile.json";
        private const string boost_gauge_SaveFile = "boostGauge_saveFile.json";
        private const string intercooler_SaveFile = "intercooler_saveFile.json";
        private const string intercooler_manifold_tube_weber_SaveFile = "intercooler_manifoldWeber_tube_saveFile.json";
        private const string intercooler_manifold_tube_twinCarb_SaveFile = "intercooler_manifoldTwinCarb_tube_saveFile.json";

        //Other Saves
        private const string modsShop_saveFile = "mod_shop_saveFile.json";
        private const string boost_saveFile = "turbocharger_mod_boost_SaveFile.txt";
        private const string wear_saveFile = "wear_saveFile.json";
        private const string screwable_saveFile = "screwable_saveFile.json";
        
        
        
        
        private string partsColorSave_SaveFile;
        //ECU-Mod Communication
        private bool ecu_mod_installed = false;
        private GameObject ecu_mod_SmartEngineModule;
        private bool ecu_mod_alsEnabled = false;

        //Everything Else
        private int currentGear = 0;
        private bool errorDetected = false;
        private static float[] originalGearRatios;
        private static float[] newGearRatio = new float[]
        {
            -4.093f, // reverse
            0f,      // neutral
            3.4f,  // 1st
            1.8f,  // 2nd
            1.4f,  // 3rd
            0.8f   // 4th
        };
        /* Gear: R = 0
         * Gear: N = 1
         * Gear: 1 = 2
         * Gear: 2 = 3
         * Gear: 3 = 4
         * Gear: 4 = 5
         */
        private RaycastHit hit;
        public AssetBundle assetsBundle;
        private AssetBundle screwableAssetsBundle;
        private TextMesh boostGaugeTextMesh;
        private static bool newGearRatiosEnabled;
        GameObject turboBig_turbine;

        private static void SwitchPartsWearDEBUG()
        {
            partsWearDEBUG = !partsWearDEBUG;
        }

        public override void OnNewGame()
        {
            partsList.ForEach(delegate (SimplePart part)
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, part.saveFile);
            });
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, modsShop_saveFile);
            SaveLoad.SerializeSaveFile<BoostSave>(this, null, boost_saveFile);
            SaveLoad.SerializeSaveFile<PartsWearSave>(this, null, wear_saveFile);
        }



        public override void OnLoad()
        {
            ModConsole.Print("DonnerTechRacing Turbocharger Mod [v" + this.Version + " | Screwable: v" + ScrewablePart.apiVersion + "]" + " started loading");
            saveFileRenamer = new SaveFileRenamer(this, 900);
            guiDebug = new GuiDebug(Screen.width - 260, 10, 250, "TURBO MOD DEBUG", new List<GuiButtonElement>()
            {
                new GuiButtonElement("DEBUG"),
                new GuiButtonElement("Wear"),
            });

            logger = new Logger(this, logger_saveFile, 100);
            if (!ModLoader.CheckSteam())
            {
                ModUI.ShowMessage("Cunt", "CUNT");
                ModConsole.Print("Cunt detected");
            }
            resetPosSetting.DoAction = PosReset;
            ecu_mod_installed = ModLoader.IsModPresent("SatsumaTurboCharger");

            //PaintSystem
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
                assetsBundle = LoadAssets.LoadBundle(this, "turbochargermod.unity3d");
            } catch(Exception ex)
            {
                logger.New("Assets file could not be loaded", "Check that the file turbochargermod.unity3d exists in the mods assets folder", ex);
            }
            try
            {
                screwableAssetsBundle = LoadAssets.LoadBundle(this, "screwableapi.unity3d");
            } catch(Exception ex)
            {
                logger.New("Assets file could not be loaded", "Check that the file screwableapi.unity3d exists in the mods assets folder", ex);
            }
            

            try
            {
                elect = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                power = PlayMakerFSM.FindFsmOnGameObject(elect, "Power");
                satsuma = GameObject.Find("SATSUMA(557kg, 248)");
                satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
                originalGearRatios = satsumaDriveTrain.gearRatios;
                satsumaDriveTrain.clutchTorqueMultiplier = 10f;

                exhaustFromEngine = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");
                exhaustFromPipe = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");
                exhaustFromMuffler = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");

                originalExhaustPipeRaceParent = exhaustFromPipe.transform.parent;
                originalExhaustPipeRacePosition = new Vector3(exhaustFromPipe.transform.localPosition.x, exhaustFromPipe.transform.localPosition.y, exhaustFromPipe.transform.localPosition.z);
                originalExhaustPipeRaceRotation = new Quaternion(exhaustFromPipe.transform.localRotation.x, exhaustFromPipe.transform.localRotation.y, exhaustFromPipe.transform.localRotation.z, exhaustFromPipe.transform.localRotation.w);

                weberCarb = GameObject.Find("racing carburators(Clone)");
                twinCarb = GameObject.Find("twin carburators(Clone)");

                weberCarb_inst = GameObject.Find("Racing Carburators").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
                twinCarb_inst = GameObject.Find("Twin Carburators").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
                steelHeaders_inst = GameObject.Find("Steel Headers").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
                racingExhaustPipe_inst = GameObject.Find("Racing Exhaust").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
                racingExhaustMuffler_inst = GameObject.Find("Racing Muffler").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");

                headers_inst = GameObject.Find("Headers").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
                exhaustPipe_inst = GameObject.Find("ExhaustPipe").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
                exhaustMuffler_inst = GameObject.Find("ExhaustMuffler").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
                exhaustMufflerDualTip_inst = GameObject.Find("ExhaustDualTip").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");

                steelHeaders = GameObject.Find("headers(Clone)");
                racingExhaustPipe = GameObject.Find("racing exhaust(Clone)");
                racingExhaustMuffler = GameObject.Find("racing muffler(Clone)");

                headers = GameObject.Find("steel headers(Clone)");
                exhaustPipe = GameObject.Find("exhaust pipe(Clone)");
                exhaustMuffler = GameObject.Find("exhaust muffler(Clone)");

                originalCylinerHead = GameObject.Find("cylinder head(Clone)");
            } 
            catch(Exception ex)
            {
                logger.New("Error while trying to load required game objects/values", "Gameobject.Find has failed to return the desired object", ex);
            }

            try
            {
                partBuySave = Helper.LoadSaveOrReturnNew<PartBuySave>(this, modsShop_saveFile);
                boostSave = Helper.LoadSaveOrReturnNew<BoostSave>(this, boost_saveFile);
                partsWearSave = Helper.LoadSaveOrReturnNew<PartsWearSave>(this, wear_saveFile);
            }
            catch(Exception ex)
            {
                logger.New("Error while trying to deserialize save file", "Please check paths to save files", ex);
            }


            //PaintSystem
            partsColorSave_SaveFile = ModLoader.GetModConfigFolder(this) + "\\turbocharger_parts_ColorSave.xml";
            Material[] materialCollecion = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material material in materialCollecion)
            {
                if (material.name == "CAR_PAINT_REGULAR")
                {
                    regularCarPaintMaterial = material;
                    break;
                }

            }

            turboBig_part = new SimplePart(
                SimplePart.LoadData(this, turboBig_SaveFile, partBuySave.bought_turboBig_kit),
                Helper.LoadPartAndSetName(assetsBundle, "turboBig.prefab", "Racing Turbo"),
                originalCylinerHead,
                turboBig_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );


            turboBig_exhaust_outlet_straight_part = new SimplePart(
                SimplePart.LoadData(this, turboBig_exhaust_outlet_straight_SaveFile, partBuySave.bought_turboBig_exhaust_outlet_straight),
                Helper.LoadPartAndSetName(assetsBundle, "turboBig_exhaust_outlet_straight.prefab", "Racing Turbo Exhaust Straight"),
                originalCylinerHead,
                turboBig_exhaust_outlet_straight_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            

            exhaust_header_part = new SimplePart(
                SimplePart.LoadData(this, exhaust_header_SaveFile, partBuySave.bought_exhaust_header),
                Helper.LoadPartAndSetName(assetsBundle, "exhaust_header.prefab", "Turbo Exhaust Header"),
                originalCylinerHead,
                exhaust_header_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            turboBig_intercooler_tube_part = new SimplePart(
                SimplePart.LoadData(this, turboBig_intercooler_tube_SaveFile, partBuySave.bought_turboBig_kit),
                Helper.LoadPartAndSetName(assetsBundle, "turboBig_intercooler_tube.prefab", "Racing Turbo Intercooler Tube"),
                satsuma,
                turboBig_intercooler_tube_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
            turboBig_exhaust_inlet_tube_part = new SimplePart(
                SimplePart.LoadData(this, turboBig_exhaust_inlet_tube_SaveFile, partBuySave.bought_turboBig_kit),
                Helper.LoadPartAndSetName(assetsBundle, "turboBig_exhaust_inlet_tube.prefab", "Racing Turbo Exhaust Inlet Tube"),
                originalCylinerHead,
                turboBig_exhaust_inlet_tube_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            turboBig_exhaust_outlet_tube_part = new SimplePart(
                SimplePart.LoadData(this, turboBig_exhaust_outlet_tube_SaveFile, partBuySave.bought_turboBig_kit),
                Helper.LoadPartAndSetName(assetsBundle, "turboBig_exhaust_outlet_tube.prefab", "Racing Turbo Exhaust Outlet Tube"),
                originalCylinerHead,
                turboBig_exhaust_outlet_tube_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            turboBig_blowoff_valve_part = new SimplePart(
                SimplePart.LoadData(this, turboBig_blowoff_valve_SaveFile, partBuySave.bought_turboBig_blowoff_valve),
                Helper.LoadPartAndSetName(assetsBundle, "turboBig_blowoff_valve.prefab", "Racing Turbo Blowoff Valve"),
                satsuma,
                turboBig_blowoff_valve_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );

            turboSmall_part = new SimplePart(
                SimplePart.LoadData(this, turboSmall_SaveFile, true),
                Helper.LoadPartAndSetName(assetsBundle, "turboSmall.prefab", "GT Turbo"),
                originalCylinerHead,
                turboSmall_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            turboSmall_logic = turboSmall_part.rigidPart.AddComponent<GT_Turbocharger_Logic>();

            turboSmall_logic.Init(this);

            turboSmall_intercooler_tube_part = new SimplePart(
                SimplePart.LoadData(this, turboSmall_intercooler_tube_SaveFile, partBuySave.bought_turboSmall_intercooler_tube),
                Helper.LoadPartAndSetName(assetsBundle, "turboSmall_intercooler_tube.prefab", "GT Turbo Intercooler Tube"),
                satsuma,
                turboSmall_intercooler_tube_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
            turboSmall_manifold_twinCarb_tube_part = new SimplePart(
                SimplePart.LoadData(this, turboSmall_manifold_twinCarb_tube_SaveFile, true),
                Helper.LoadPartAndSetName(assetsBundle, "turboSmall_manifold_twinCarb_tube.prefab", "GT Turbo Manifold TwinCarb Tube"),
                originalCylinerHead,
                turboSmall_manifold_twinCarb_tube_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            turboSmall_airfilter_part = new SimplePart(
                SimplePart.LoadData(this, turboSmall_airfilter_SaveFile, partBuySave.bought_turboSmall_airfilter),
                Helper.LoadPartAndSetName(assetsBundle, "turboSmall_airfilter.prefab", "GT Turbo Airfilter"),
                originalCylinerHead,
                turboSmall_airfilter_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            turboSmall_exhaust_inlet_tube_part = new SimplePart(
                SimplePart.LoadData(this, turboSmall_exhaust_inlet_tube_SaveFile, true),
                Helper.LoadPartAndSetName(assetsBundle, "turboSmall_exhaust_inlet_tube.prefab", "GT Turbo Exhaust Inlet Tube"),
                originalCylinerHead,
                turboSmall_exhaust_inlet_tube_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            turboSmall_exhaust_outlet_tube_part = new SimplePart(
                SimplePart.LoadData(this, turboSmall_exhaust_outlet_tube_SaveFile, true),
                Helper.LoadPartAndSetName(assetsBundle, "turboSmall_exhaust_outlet_tube.prefab", "GT Turbo Exhaust Outlet Tube"),
                originalCylinerHead,
                turboSmall_exhaust_outlet_tube_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            turboBig_hood_part = new SimplePart(
                SimplePart.LoadData(this, turboBig_hood_SaveFile, partBuySave.bought_turboBig_hood),
                Helper.LoadPartAndSetName(assetsBundle, "turboBig_hood.prefab", "Racing Turbo Hood"),
                satsuma,
                turboBig_hood_installLocation,
                new Quaternion(0, 180, 0, 0)
            );
            
            manifold_weber_part = new SimplePart(
                SimplePart.LoadData(this, manifold_weber_SaveFile, partBuySave.bought_manifold_weber_kit),
                Helper.LoadPartAndSetName(assetsBundle, "manifold_weber.prefab", "Weber Manifold"),
                originalCylinerHead,
                manifold_weber_installLocation,
                new Quaternion { eulerAngles = new Vector3(80, 0, 0) }
            );

            manifold_twinCarb_part = new SimplePart(
                SimplePart.LoadData(this, manifold_twinCarb_SaveFile, partBuySave.bought_manifold_twinCarb_kit),
                Helper.LoadPartAndSetName(assetsBundle, "manifold_twinCarb.prefab", "TwinCarb Manifold"),
                originalCylinerHead,
                manifold_twinCarb_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            boost_gauge_part = new SimplePart(
                SimplePart.LoadData(this, boost_gauge_SaveFile, partBuySave.bought_boost_gauge),
                Helper.LoadPartAndSetName(assetsBundle, "boost_gauge.prefab", "Boost Gauge"),
                GameObject.Find("dashboard(Clone)"),
                boost_gauge_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            boostGaugeTextMesh = boost_gauge_part.rigidPart.GetComponentInChildren<TextMesh>();

            intercooler_part = new SimplePart(
               SimplePart.LoadData(this, intercooler_SaveFile, partBuySave.bought_intercooler),
               Helper.LoadPartAndSetName(assetsBundle, "intercooler.prefab", "Intercooler"),
               satsuma,
               intercooler_installLocation,
               new Quaternion { eulerAngles = new Vector3(-5, 180, 0) }
           );
            intercooler_manifold_tube_weber_part = new SimplePart(
                SimplePart.LoadData(this, intercooler_manifold_tube_weber_SaveFile, partBuySave.bought_manifold_weber_kit),
                Helper.LoadPartAndSetName(assetsBundle, "intercooler_manifold_tube_weber.prefab", "Weber Intercooler-Manifold Tube"),
                satsuma,
                intercooler_manifold_tube_weber_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
            intercooler_manifold_tube_twinCarb_part = new SimplePart(
                SimplePart.LoadData(this, intercooler_manifold_tube_twinCarb_SaveFile, partBuySave.bought_manifold_twinCarb_kit),
                Helper.LoadPartAndSetName(assetsBundle, "intercooler_manifold_tube_twinCarb.prefab", "TwinCarb Intercooler-Manifold Tube"),
                satsuma,
                intercooler_manifold_tube_twinCarb_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );

            bigPartsList = new List<SimplePart>
            {
                turboBig_part,
                turboBig_intercooler_tube_part,
                turboBig_exhaust_inlet_tube_part,
                turboBig_exhaust_outlet_tube_part,
                turboBig_blowoff_valve_part,
                turboBig_exhaust_outlet_straight_part,
            };

            smallPartsList = new List<SimplePart>
            {
                turboSmall_part,
                turboSmall_intercooler_tube_part,
                turboSmall_exhaust_inlet_tube_part,
                turboSmall_exhaust_outlet_tube_part,
                turboSmall_airfilter_part,
                turboSmall_manifold_twinCarb_tube_part,
            };

            otherPartsList = new List<SimplePart>
            {
                manifold_weber_part,
                manifold_twinCarb_part,
                boost_gauge_part,
                intercooler_part,
                intercooler_manifold_tube_weber_part,
                intercooler_manifold_tube_twinCarb_part,
                turboBig_hood_part,
                exhaust_header_part,
            };

            partsList.AddRange(bigPartsList);
            partsList.AddRange(smallPartsList);
            partsList.AddRange(otherPartsList);

            SetupScrewable();

            GameObject turboBig_kitBox = GameObject.Instantiate((assetsBundle.LoadAsset("turboBig_box.prefab") as GameObject));
            GameObject manifoldWeber_kitBox = GameObject.Instantiate((assetsBundle.LoadAsset("turboBig_box.prefab") as GameObject));
            GameObject manifoldTwinCarb_kitBox = GameObject.Instantiate((assetsBundle.LoadAsset("turboBig_box.prefab") as GameObject));

            Helper.SetObjectNameTagLayer(turboBig_kitBox, "Racing Turbocharger Kit(Clone)");
            Helper.SetObjectNameTagLayer(manifoldWeber_kitBox, "Weber Kit(Clone)");
            Helper.SetObjectNameTagLayer(manifoldTwinCarb_kitBox, "TwinCarb Kit(Clone)");

            turboBig_kit = new Kit(this, turboBig_kitBox,
                new SimplePart[]{
                    turboBig_part,
                    turboBig_intercooler_tube_part,
                    turboBig_exhaust_inlet_tube_part,
                    turboBig_exhaust_outlet_tube_part,
                }, partBuySave.bought_turboBig_kit);

            manifoldWeber_kit = new Kit(this, manifoldWeber_kitBox,
                new SimplePart[]{
                    manifold_weber_part,
                    intercooler_manifold_tube_weber_part
                }, partBuySave.bought_manifold_weber_kit);

            manifoldTwinCarb_kit = new Kit(this, manifoldTwinCarb_kitBox,
                new SimplePart[]
                {
                    manifold_twinCarb_part,
                    intercooler_manifold_tube_twinCarb_part
                }, partBuySave.bought_manifold_twinCarb_kit);

            SetModsShop();

            Configuration racingTurboConfig = new Configuration
            {
                boostBase = 2f,
                boostStartingRpm = 4000,
                boostMin = -0.10f,
                minSettableBoost = 1.65f,
                boostIncreasement = 800,
                blowoffDelay = 0.8f,
                blowoffTriggerBoost = 0.6f,
                backfireThreshold = 4000,
                backfireRandomRange = 20,
                rpmMultiplier = 14,
                extraPowerMultiplicator = 1.5f,
                soundBoostMaxVolume = 0.3f,
                soundBoostIncreasement = 4000,
                soundBoostPitchMultiplicator = 4,
                boostSettingSteps = 0.05f,
            };

            //Temporary
            Configuration gtTurboConfig = new Configuration
            {
                boostBase = 2f,
                boostStartingRpm = 4000,
                boostMin = -0.10f,
                minSettableBoost = 1.65f,
                boostIncreasement = 1000,
                blowoffDelay = 0.8f,
                blowoffTriggerBoost = 0.6f,
                rpmMultiplier = 14,
                extraPowerMultiplicator = 1.5f,
                soundBoostMaxVolume = 0.3f,
                soundBoostIncreasement = 4000,
                soundBoostPitchMultiplicator = 4,
                boostSettingSteps = 0.01f,
            };

            racingTurbo = new Turbo(this, turboBig_part, "turbocharger_loop.wav", "grinding sound.wav", "turbocharger_blowoff.wav",
                new bool[]
                {
                    true, 
                    false,
                    true
                }, racingTurboConfig, turboBig_blowoff_valve_part.rigidPart.transform.FindChild("blowoff_valve").gameObject);

            racingTurbo.turbine = turboBig_part.rigidPart.transform.FindChild("TurboCharger_Big_Compressor_Turbine").gameObject;
            racingTurbo.backfire_Logic = turboBig_exhaust_outlet_straight_part.rigidPart.AddComponent<Backfire_Logic>();


            Dictionary<string, float> partsWear = Helper.LoadSaveOrReturnNew<Dictionary<string, float>>(this, wear_saveFile);

            racingTurboWear = new Wear(this, "racingTurbo", turboBig_part, new List<WearCondition>
                {
                    new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                    new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                    new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                    new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                    new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
                }, 0.003f, 0.5f, partsWear, 4000, "repair_turbocharger_big_ProductImage.png", 100);
            intercoolerWear = new Wear(this, "intercooler", intercooler_part, new List<WearCondition>
                {
                    new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                    new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                    new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                    new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                    new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
                }, 0.005f, 0.5f, partsWear, 1500, "repair_intercooler_ProductImage.png", 100);

            gtTurboWear = new Wear(this, "gtTurbo", turboSmall_part, new List<WearCondition>
                {
                    new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                    new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                    new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                    new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                    new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
                }, 0.003f, 0.5f, partsWear, 2500, "repair_turbocharger_small_ProductImage.png", 100);

            //Will currently not work as it should only be a wearReductionMultiplier reducer new Wear class?
            /*
            gtTurboAirfilterWear = new Wear(this, "gtTurboAirfilter", turboSmall_airfilter_part, new List<WearCondition>
                {
                    new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                    new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                    new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                    new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                    new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
                }, 0.0045f, 0.5f, partsWear, 400, "repair_turbocharger_small_airfilter_ProductImage.png", 100);
            */



            racingTurbo.wears = new Wear[]
            {
                racingTurboWear,
                intercoolerWear,
            };

            Dictionary<string, Condition> racingTurbo_conditions = new Dictionary<string, Condition>();
            racingTurbo_conditions["weberCarb"] = new Condition("weberCarb", 0.5f);
            racingTurbo_conditions["twinCarb"] = new Condition("twinCarb", 0.2f);
            racingTurbo.SetConditions(racingTurbo_conditions);

            gtTurbo = new Turbo(this, turboSmall_part, "turbocharger_loop.wav", "grinding sound.wav", null,
                new bool[]
                {
                    false,
                    true,
                    true
                }, gtTurboConfig, turboSmall_part.rigidPart.transform.FindChild("Turbocharger_Small_Wastegate").gameObject);

            if (ecu_mod_installed)
            {
                ecu_mod_SmartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
            }

            SetupInspectionPrevention();
            assetsBundle.Unload(false);
            screwableAssetsBundle.Unload(false);
            ModConsole.Print("DonnerTechRacing Turbocharger Mod [v" + this.Version + "]" + " finished loading");            
        }

        

        private void SetupScrewable()
        {
            SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, screwable_saveFile);
            //Big turbo
            turboBig_intercooler_tube_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboBig_intercooler_tube_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.153f, 0.324f, 0.1835f), new Vector3(90, 0, 0), 0.7f, 10),
                    new Screw(new Vector3(0.0779f, 0.324f, 0.1835f), new Vector3(90, 0, 0), 0.7f, 10),
                    new Screw(new Vector3(0.031f, -0.13f, -0.1632f), new Vector3(180, 0, 0), 0.4f, 10),
               });
            turboBig_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboBig_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.105f, -0.098f, -0.082f), new Vector3(-90, 0, 0), 0.7f, 10, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(0.03f, -0.098f, -0.082f), new Vector3(-90, 0, 0), 0.7f, 10, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.0288f, -0.098f, 0.0817f), new Vector3(-90, 0, 0), 0.7f, 10, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.1085f, -0.098f, 0.0817f), new Vector3(-90, 0, 0), 0.7f, 10, ScrewablePart.ScrewType.Nut),
                });
            turboBig_exhaust_inlet_tube_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboBig_exhaust_inlet_tube_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.202f, -0.1f, -0.01f), new Vector3(-90, 0, 0), 0.7f, 10),
                    new Screw(new Vector3(0.145f, -0.1f, -0.018f), new Vector3(-90, 0, 0), 0.7f, 10),
                    new Screw(new Vector3(-0.12f, 0.19f, -0.005f), new Vector3(90, 0, 0), 0.7f, 10),
                    new Screw(new Vector3(-0.2f, 0.19f, -0.005f), new Vector3(90, 0, 0), 0.7f, 10),
                });
            turboBig_exhaust_outlet_tube_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboBig_exhaust_outlet_tube_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.0655f, 0.372f, -0.0425f), new Vector3(0, -90, 0), 0.7f, 10),
                });

            turboBig_exhaust_outlet_straight_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboBig_exhaust_outlet_straight_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0342f, 0.004f, -0.023f), new Vector3(0, -90, 0), 0.7f, 10),
                });

            turboBig_blowoff_valve_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboBig_blowoff_valve_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0475f, -0.031f, 0.01f), new Vector3(0, 0, 0), 0.3f, 5),
                });

            //Small turbo
            turboSmall_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboSmall_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0715f, -0.024f, 0.044f), new Vector3(180f, 0f, 0f), 0.4f, 10),
                });
            turboSmall_intercooler_tube_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboSmall_intercooler_tube_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.034f, -0.13f, -0.1638f), new Vector3(180f, 0f, 0f), 0.4f, 10),
                    new Screw(new Vector3(0.014f, 0.24f, 0.332f), new Vector3(0f, -90f, 0f), 0.4f, 10),
                });
            turboSmall_exhaust_inlet_tube_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboSmall_exhaust_inlet_tube_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.114f, -0.044f, -0.035f), new Vector3(-90f, 0f, 0f), 0.7f, 10),
                    new Screw(new Vector3(0.06f, -0.044f, -0.044f), new Vector3(-90f, 0f, 0f), 0.7f, 10),
                });
            turboSmall_exhaust_outlet_tube_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboSmall_exhaust_outlet_tube_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.078f, 0.1708f, -0.0235f), new Vector3(0, -90, 0), 0.5f, 10),
                });
            turboSmall_manifold_twinCarb_tube_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboSmall_manifold_twinCarb_tube_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.097f, -0.07f, -0.135f), new Vector3(0, 90, 0), 0.4f, 10),
                });

            turboSmall_airfilter_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, turboSmall_airfilter_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0095f, 0.025f, 0.0488f), new Vector3(0, 90, 0), 0.4f, 10),
                });

            //Other parts
            exhaust_header_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, exhaust_header_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.169f, 0.076f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(0.13f, 0.0296f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.003f, 0.08f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.137f, 0.0296f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.174f, 0.076f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewablePart.ScrewType.Nut),
                });
            intercooler_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, intercooler_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.2215f, 0.081f, 0.039f), new Vector3(180, 0, 0), 0.6f, 10),
                    new Screw(new Vector3(0.239f, 0.081f, 0.039f), new Vector3(180, 0, 0), 0.6f, 10),
                });
            intercooler_manifold_tube_weber_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, intercooler_manifold_tube_weber_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.0473f, -0.1205f, -0.241f), new Vector3(180, 0, 0), 0.4f, 10),
                });
            intercooler_manifold_tube_twinCarb_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, intercooler_manifold_tube_twinCarb_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.0425f, -0.1205f, -0.241f), new Vector3(180, 0, 0), 0.4f, 10),
                });
            manifold_weber_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, manifold_weber_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.2f, 0.03f, -0.009f), new Vector3(180, 0, 0), 0.4f, 10),
                });
            manifold_twinCarb_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, manifold_twinCarb_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.003f, 0.105f, 0.0305f), new Vector3(0, 90, 0), 0.5f, 10),
                });



            //Clamps
            turboBig_blowoff_valve_part.screwablePart.AddClampModel(new Vector3(0.035f, -0.04f, 0.0005f), new Vector3(55, 90, 0), new Vector3(0.41f, 0.41f, 0.41f));
            turboBig_exhaust_outlet_straight_part.screwablePart.AddClampModel(new Vector3(0.045f, -0.034f, -0.023f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            turboBig_exhaust_outlet_tube_part.screwablePart.AddClampModel(new Vector3(-0.055f, 0.334f, -0.0425f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            turboBig_intercooler_tube_part.screwablePart.AddClampModel(new Vector3(0.031f, -0.154f, -0.1545f), new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));

            turboSmall_airfilter_part.screwablePart.AddClampModel(new Vector3(0f, 0f, 0.049f), new Vector3(0, 0, 0), new Vector3(0.65f, 0.65f, 0.65f));
            turboSmall_intercooler_tube_part.screwablePart.AddClampModel(new Vector3(0.034f, -0.154f, -0.1548f), new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
            turboSmall_intercooler_tube_part.screwablePart.AddClampModel(new Vector3(0.0225f, 0.24f, 0.313f), new Vector3(90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
            turboSmall_part.screwablePart.AddClampModel(new Vector3(0.0715f, -0.043f, 0.052f), new Vector3(0, 90, 0), new Vector3(0.5f, 0.5f, 0.5f));
            turboSmall_exhaust_outlet_tube_part.screwablePart.AddClampModel(new Vector3(-0.068f, 0.1445f, -0.0235f), new Vector3(0, 0, 0), new Vector3(0.67f, 0.67f, 0.67f));
            turboSmall_manifold_twinCarb_tube_part.screwablePart.AddClampModel(new Vector3(-0.106f, -0.07f, -0.116f), new Vector3(-90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));

            intercooler_manifold_tube_weber_part.screwablePart.AddClampModel(new Vector3(-0.047f, -0.1465f, -0.232f), new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
            intercooler_manifold_tube_twinCarb_part.screwablePart.AddClampModel(new Vector3(-0.042f, -0.1465f, -0.232f), new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
            manifold_weber_part.screwablePart.AddClampModel(new Vector3(0.2f, -0.002f, 0.001f), new Vector3(0, 90, 0), new Vector3(0.82f, 0.82f, 0.82f));
            manifold_twinCarb_part.screwablePart.AddClampModel(new Vector3(-0.013f, 0.105f, 0f), new Vector3(90, 0, 0), new Vector3(0.8f, 0.8f, 0.8f));
        }
        private void SetupInspectionPrevention()
        {
            GameObject inspectionProcess = GameObject.Find("InspectionProcess");
            inspectionPlayMakerFsm = inspectionProcess.GetComponents<PlayMakerFSM>()[0];
            foreach (PlayMakerFSM playMakerFSM in inspectionProcess.GetComponents<PlayMakerFSM>())
            {
                if (playMakerFSM.FsmName == "Inspect")
                {
                    inspectionPlayMakerFsm = playMakerFSM;
                    break;
                }
            }
            foreach (FsmEvent fsmEvent in inspectionPlayMakerFsm.FsmEvents)
            {
                if (fsmEvent.Name == "FAIL")
                {
                    inspectionFailedEvent = fsmEvent;
                    break;
                }
            }

            FsmHook.FsmInject(inspectionProcess, "Results", InspectionResults);
        }
        public void InspectionResults()
        {
            if(partsList.Any(part => part.installed))
            {
                PlayMakerFSM.BroadcastEvent(inspectionFailedEvent);
            }
        }
        public override void ModSettings()
        {
            Settings.AddHeader(this, "DEBUG");
            Settings.AddCheckBox(this, debugGuiSetting);
            Settings.AddHeader(this, "Settings");
            Settings.AddCheckBox(this, rotateTurbineSetting);
            Settings.AddCheckBox(this, backfireEffectSetting);
            Settings.AddCheckBox(this, partsWearSetting);

            Settings.AddCheckBox(this, toggleNewGearRatios);
            Settings.AddCheckBox(this, useDefaultColorsSetting);
            Settings.AddButton(this, resetPosSetting, "reset part location");
            Settings.AddHeader(this, "", Color.clear);
            Settings.AddText(this, "New Gear ratios\n" +
                "1.Gear: " + newGearRatio[2] + "\n" +
                "2.Gear: " + newGearRatio[3] + "\n" +
                "3.Gear: " + newGearRatio[4] + "\n" +
                "4.Gear: " + newGearRatio[5]
                );
        }
        public override void OnSave()
        {
            turboBig_kit.CheckUnpackedOnSave(partBuySave.bought_turboBig_kit);
            manifoldWeber_kit.CheckUnpackedOnSave(partBuySave.bought_manifold_weber_kit);
            manifoldTwinCarb_kit.CheckUnpackedOnSave(partBuySave.bought_manifold_twinCarb_kit);

            try
            {
                partsList.ForEach(delegate (SimplePart part)
                {
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, part.getSaveInfo(), part.saveFile);
                });
            }
            catch (Exception ex)
            {
                logger.New("Error while trying to save part", "", ex);
            }

            try
            {
                SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, modsShop_saveFile);
                SaveLoad.SerializeSaveFile<BoostSave>(this, boostSave, boost_saveFile);
                SaveLoad.SerializeSaveFile<PartsWearSave>(this, partsWearSave, wear_saveFile);

                Dictionary<string, float> partsWear = new Dictionary<string, float>();
                partsWear = racingTurboWear.GetWear(partsWear);
                partsWear = intercoolerWear.GetWear(partsWear);

                SaveLoad.SerializeSaveFile<Dictionary<string, float>>(this, partsWear, wear_saveFile);
            }
            catch (System.Exception ex)
            {
                logger.New("Error while trying to save save file", "", ex);
            }


            try
            {
                ScrewablePart.SaveScrews(this, Helper.GetScrewablePartsArrayFromPartsList(partsList), screwable_saveFile);
            }
            catch (System.Exception ex)
            {
                logger.New("Error while trying to save screws ", $"save file: {screwable_saveFile}", ex);
            }
        }

        public override void OnGUI()
        {
            saveFileRenamer.GuiHandler();
            if((bool)debugGuiSetting.Value)
            {
                guiDebug.Handle(new List<GuiInfo> {
                new GuiInfo("Wear", "Racing Turbo", racingTurboWear.wear.ToString("000.00000")),
                new GuiInfo("Wear", "Intercooler", intercoolerWear.wear.ToString("000.00000")),
                new GuiInfo("DEBUG", "Engine RPM", ((int)engineRPM).ToString()),
                new GuiInfo("DEBUG", "Racing Turbo bar", racingTurbo.boost.ToString()),
                new GuiInfo("DEBUG", "GT Turbo bar", gtTurbo.boost.ToString()),
                new GuiInfo("DEBUG", "Power multiplier", racingTurbo.powerMultiplier.ToString()),
                new GuiInfo("DEBUG", "KM/H", ((int)satsumaDriveTrain.differentialSpeed).ToString()),
                new GuiInfo("DEBUG", "Torque", satsumaDriveTrain.torque.ToString()),
                new GuiInfo("DEBUG", "Clutch Max Torque", satsumaDriveTrain.clutchMaxTorque.ToString()),
                new GuiInfo("DEBUG", "Clutch Torque Multiplier", satsumaDriveTrain.clutchTorqueMultiplier.ToString()),
            });
            }

        }
        public override void Update()
        {
            bool allBig = AllBigInstalled();
            bool allSmall = AllSmallInstalled();
            bool allOther = AllOtherInstalled();
            racingTurbo.Handle(allBig, allSmall, allOther);
            racingTurbo.UpdateCondition("weberCarb", weberCarb_inst.Value);
            racingTurbo.UpdateCondition("twinCarb", twinCarb_inst.Value);

            AddPartsColorMaterial();
            //DetectPaintingPart();
            DetectChangingBoost();
            HandleExhaustSystem();
            //CheckPartsForDamage();
            HandlePartsTrigger();
        }

        private void HandlePartsTrigger()
        {
            bool anyBig = AnyBigInstalled(true);
            bool anySmall = AnySmallInstalled(true);
            if (anyBig)
            {
                turboSmall_part.partTrigger.triggerGameObject.SetActive(false);
                turboSmall_intercooler_tube_part.partTrigger.triggerGameObject.SetActive(false);
                turboSmall_exhaust_inlet_tube_part.partTrigger.triggerGameObject.SetActive(false);
                turboSmall_exhaust_outlet_tube_part.partTrigger.triggerGameObject.SetActive(false);
                turboSmall_airfilter_part.partTrigger.triggerGameObject.SetActive(false);
                turboSmall_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turboSmall_part.partTrigger.triggerGameObject.SetActive(true);
                turboSmall_intercooler_tube_part.partTrigger.triggerGameObject.SetActive(true);
                turboSmall_exhaust_inlet_tube_part.partTrigger.triggerGameObject.SetActive(true);
                turboSmall_exhaust_outlet_tube_part.partTrigger.triggerGameObject.SetActive(true);
                turboSmall_airfilter_part.partTrigger.triggerGameObject.SetActive(true);
                turboSmall_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(true);
            }

            if (anySmall)
            {
                turboBig_part.partTrigger.triggerGameObject.SetActive(false);
                turboBig_intercooler_tube_part.partTrigger.triggerGameObject.SetActive(false);
                turboBig_exhaust_inlet_tube_part.partTrigger.triggerGameObject.SetActive(false);
                turboBig_exhaust_outlet_tube_part.partTrigger.triggerGameObject.SetActive(false);
                turboBig_blowoff_valve_part.partTrigger.triggerGameObject.SetActive(false);
                turboBig_exhaust_outlet_straight_part.partTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turboBig_part.partTrigger.triggerGameObject.SetActive(true);
                turboBig_intercooler_tube_part.partTrigger.triggerGameObject.SetActive(true);
                turboBig_exhaust_inlet_tube_part.partTrigger.triggerGameObject.SetActive(true);
                turboBig_exhaust_outlet_tube_part.partTrigger.triggerGameObject.SetActive(true);
                turboBig_blowoff_valve_part.partTrigger.triggerGameObject.SetActive(true);
                turboBig_exhaust_outlet_straight_part.partTrigger.triggerGameObject.SetActive(true);
            }

            if (weberCarb_inst.Value)
            {
                intercooler_manifold_tube_twinCarb_part.partTrigger.triggerGameObject.SetActive(false);
                manifold_twinCarb_part.partTrigger.triggerGameObject.SetActive(false);
                turboSmall_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(false);

                if (intercooler_manifold_tube_twinCarb_part.installed) { intercooler_manifold_tube_twinCarb_part.removePart(); }
                if (manifold_twinCarb_part.installed) { manifold_twinCarb_part.removePart(); }
                if (turboSmall_manifold_twinCarb_tube_part.installed) { turboSmall_manifold_twinCarb_tube_part.removePart(); }

            }
            else
            {
                intercooler_manifold_tube_twinCarb_part.partTrigger.triggerGameObject.SetActive(true);
                manifold_twinCarb_part.partTrigger.triggerGameObject.SetActive(true);
                turboSmall_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(true);
            }

            if (twinCarb_inst.Value)
            {
                manifold_weber_part.partTrigger.triggerGameObject.SetActive(false);
                intercooler_manifold_tube_weber_part.partTrigger.triggerGameObject.SetActive(false);

                if (manifold_weber_part.installed) { manifold_weber_part.removePart(); }
                if (intercooler_manifold_tube_weber_part.installed) { intercooler_manifold_tube_weber_part.removePart(); }
            }
            else
            {
                manifold_weber_part.partTrigger.triggerGameObject.SetActive(true);
                intercooler_manifold_tube_weber_part.partTrigger.triggerGameObject.SetActive(true);
            }


            if (!turboBig_part.installed && turboBig_exhaust_outlet_straight_part.installed)
            {
                turboBig_exhaust_outlet_straight_part.removePart();
            }
            if(!turboBig_intercooler_tube_part.installed && !turboBig_exhaust_outlet_tube_part.installed && !turboBig_exhaust_inlet_tube_part.installed && turboBig_part.installed)
            {
                turboBig_part.removePart();

                if (turboBig_exhaust_outlet_straight_part.installed) { turboBig_exhaust_outlet_straight_part.removePart(); }
            }
            if(!turboBig_intercooler_tube_part.installed && turboBig_blowoff_valve_part.installed)
            {
                turboBig_blowoff_valve_part.removePart();
            }

            if (!turboSmall_part.installed)
            {
                if (turboSmall_airfilter_part.installed)
                    turboSmall_airfilter_part.removePart();
            }

            if(!turboSmall_intercooler_tube_part.installed && !turboSmall_exhaust_inlet_tube_part.installed && !turboSmall_exhaust_outlet_tube_part.installed && !turboSmall_manifold_twinCarb_tube_part.installed && turboSmall_part.installed)
            {
                turboSmall_part.removePart();
                if (turboSmall_airfilter_part.installed)
                    turboSmall_airfilter_part.removePart();
            }

            if (turboBig_exhaust_outlet_straight_part.installed) { turboBig_exhaust_outlet_tube_part.partTrigger.triggerGameObject.SetActive(false);}
            else { turboBig_exhaust_outlet_tube_part.partTrigger.triggerGameObject.SetActive(true); }

            if(turboBig_exhaust_outlet_tube_part.installed) { turboBig_exhaust_outlet_straight_part.partTrigger.triggerGameObject.SetActive(false); }
            else { turboBig_exhaust_outlet_straight_part.partTrigger.triggerGameObject.SetActive(true); }

        }
        private void HandleExhaustSystem()
        {
            bool allBig = AllBigInstalled();
            bool allSmall = AllSmallInstalled();
            bool allOther = AllOtherInstalled();

            if (steelHeaders_inst.Value || headers_inst.Value)
            {
                if (exhaust_header_part.installed)
                {
                    exhaust_header_part.removePart();
                }
            }

            if (satsumaDriveTrain.rpm >= 200)
            {
                if (turboBig_exhaust_outlet_straight_part.installed && allBig && allOther)
                {
                    exhaustFromEngine.SetActive(false);
                    exhaustFromPipe.SetActive(true);

                    exhaustFromPipe.transform.parent = turboBig_exhaust_outlet_straight_part.rigidPart.transform;
                    exhaustFromPipe.transform.localPosition = racingTurbo.backfire_Logic.GetFireFXPos();
                    exhaustFromPipe.transform.localRotation = racingTurbo.backfire_Logic.GetFireFXRot();
                    exhaustFromMuffler.SetActive(false);
                }
                else
                {
                    exhaustFromPipe.transform.parent = originalExhaustPipeRaceParent;
                    exhaustFromPipe.transform.localPosition = originalExhaustPipeRacePosition;
                    exhaustFromPipe.transform.localRotation = originalExhaustPipeRaceRotation;

                    if ((allBig || allSmall) && allOther)
                    {
                        if(racingExhaustPipe_inst.Value && racingExhaustMuffler_inst.Value)
                        {
                            exhaustFromEngine.SetActive(false);
                            exhaustFromPipe.SetActive(false);
                            exhaustFromMuffler.SetActive(true);
                        }
                        else if(racingExhaustPipe_inst.Value)
                        {
                            exhaustFromEngine.SetActive(false);
                            exhaustFromPipe.SetActive(true);
                            exhaustFromMuffler.SetActive(false);
                        }
                        else
                        {
                            exhaustFromEngine.SetActive(true);
                            exhaustFromPipe.SetActive(false);
                            exhaustFromMuffler.SetActive(false);
                        }
                    }
                    else
                    {
                        if((steelHeaders_inst.Value && racingExhaustPipe_inst.Value && racingExhaustMuffler_inst.Value) || (headers_inst.Value && exhaustPipe_inst.Value && (exhaustMuffler_inst.Value || exhaustMufflerDualTip_inst.Value)))
                        {
                            exhaustFromEngine.SetActive(false);
                            exhaustFromPipe.SetActive(false);
                            exhaustFromMuffler.SetActive(true);
                        }
                        else if((steelHeaders_inst.Value && racingExhaustPipe_inst.Value) || (headers_inst.Value && exhaustPipe_inst.Value))
                        {
                            exhaustFromEngine.SetActive(false);
                            exhaustFromPipe.SetActive(true);
                            exhaustFromMuffler.SetActive(false);
                        }
                        else
                        {
                            exhaustFromEngine.SetActive(true);
                            exhaustFromPipe.SetActive(false);
                            exhaustFromMuffler.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                exhaustFromEngine.SetActive(false);
                exhaustFromPipe.SetActive(false);
                exhaustFromMuffler.SetActive(false);
            }

        }


        [Obsolete("Don't use!", true)]
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
                                if (!turboBig_part.installed)
                                {
                                    if (partsWearSave.turboBig_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.turboBig_wear >= 50f)
                                        ModClient.guiInteraction = "Not sure if this thing should wobble that much";
                                    else if (partsWearSave.turboBig_wear >= 25f)
                                        ModClient.guiInteraction = "Nope... This should not wobble that much";
                                    else if (partsWearSave.turboBig_wear >= 15f)
                                        ModClient.guiInteraction = "I could pull it out. If I wanted to...";
                                    else if (partsWearSave.turboBig_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "GT Turbocharger" || gameObjectHit.name == "GT Turbocharger(Clone)")
                            {
                                if (!turboSmall_part.installed)
                                {
                                    if (partsWearSave.turboSmall_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.turboSmall_wear >= 50f)
                                        ModClient.guiInteraction = "Not sure if this thing should wobble that much";
                                    else if (partsWearSave.turboSmall_wear >= 25f)
                                        ModClient.guiInteraction = "Nope... This should not wobble that much";
                                    else if (partsWearSave.turboSmall_wear >= 15f)
                                        ModClient.guiInteraction = "I could pull it out. If I wanted to...";
                                    else if (partsWearSave.turboSmall_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "GT Turbocharger Airfilter" || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)")
                            {
                                if(!turboSmall_airfilter_part.installed){
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
                                if(!intercooler_part.installed){
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

        private void SetModsShop()
        {
            if (GameObject.Find("Shop for mods") != null)
            {
                modsShop = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();
                List<ProductInformation> shopItems = new List<ProductInformation>();

                shopItems.Add(new ProductInformation(turboBig_kit.kitBox, "Racing Turbocharger Kit", 8100, null, partBuySave.bought_turboBig_kit));
                shopItems.Add(new ProductInformation(turboBig_exhaust_outlet_straight_part.activePart, "Racing Turbocharger Straight Exhaust", 1000, null, partBuySave.bought_turboBig_exhaust_outlet_straight));
                shopItems.Add(new ProductInformation(turboBig_blowoff_valve_part.activePart, "Racing Turbocharger Blowoff Valve", 1350, null, partBuySave.bought_turboBig_blowoff_valve));
                shopItems.Add(new ProductInformation(turboBig_hood_part.activePart, "Racing Turbocharger Hood", 1800, null, partBuySave.bought_turboBig_hood));

                shopItems.Add(new ProductInformation(turboSmall_intercooler_tube_part.activePart, "GT Turbocharger Intercooler Tube", 500, null, partBuySave.bought_turboSmall_intercooler_tube));
                shopItems.Add(new ProductInformation(turboSmall_airfilter_part.activePart, "GT Turbocharger Airfilter", 800, null, partBuySave.bought_turboSmall_airfilter));
                

                shopItems.Add(new ProductInformation(manifoldTwinCarb_kit.kitBox, "TwinCarb Manifold Kit", 1950, null, partBuySave.bought_manifold_twinCarb_kit));
                shopItems.Add(new ProductInformation(manifoldWeber_kit.kitBox, "Weber Manifold Kit", 2250, null, partBuySave.bought_manifold_weber_kit));

                shopItems.Add(new ProductInformation(intercooler_part.activePart, "Intercooler", 3500, null, partBuySave.bought_intercooler));
                shopItems.Add(new ProductInformation(boost_gauge_part.activePart, "Boost Gauge", 180, null, partBuySave.bought_boost_gauge));
                shopItems.Add(new ProductInformation(exhaust_header_part.activePart, "Turbocharger Exhaust Header", 2100, null, partBuySave.bought_exhaust_header));

                Shop shop = new Shop(this, modsShop, assetsBundle, partBuySave, shopItems);
                shop.SetupShopItems();
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

        private void LoadPartsColorSave()
        {
            try
            {
                if (File.Exists(partsColorSave_SaveFile))
                {
                    XmlReader xmlReader = XmlReader.Create(partsColorSave_SaveFile);
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
                                if (xmlReader.Name == "turboBig_hood-color")
                                {
                                    turboBig_hoodColor = new Color(rFloat, gFloat, bFloat);
                                }
                                else if (xmlReader.Name == "intercooler-color")
                                {
                                    intercoolerColor = new Color(rFloat, gFloat, bFloat);
                                }
                                else if (xmlReader.Name == "turbocharger_big-color")
                                {
                                    turboBigColor = new Color(rFloat, gFloat, bFloat);
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
                                    turboBigBlowoffValveColor = new Color(rFloat, gFloat, bFloat);
                                }
                                else if (xmlReader.Name == "turbocharger_small_airfilter-color")
                                {
                                    turboSmall_airfilter_color = new Color(rFloat, gFloat, bFloat);
                                }
                                else if (xmlReader.Name == "original_intercooler-color")
                                {
                                    originalIntercoolerColor = new Color(rFloat, gFloat, bFloat);
                                }
                                else if (xmlReader.Name == "original_turbocharger_big-color")
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
                                else if (xmlReader.Name == "original_blowoffValve-color")
                                {
                                    originalturboBigBlowoffValveColor = new Color(rFloat, gFloat, bFloat);
                                }
                                else if (xmlReader.Name == "original_turbocharger_small_airfilter-color")
                                {
                                    turboSmall_airfilter_color = new Color(rFloat, gFloat, bFloat);
                                }
                            }
                        }
                    }
                    xmlReader.Close();
                }
            }
            catch (Exception ex)
            {
            }
            
        }

        private void WritePartsColorSave(bool newGame)
        {
            try
            {
                partsColorSave_SaveFile = ModLoader.GetModConfigFolder(this) + "\\turbocharger_parts_ColorSave.xml";
                XmlWriter xmlWriter = XmlWriter.Create(partsColorSave_SaveFile);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("color-save");
                if (newGame == false)
                {
                    WriteXMLColorSaveElement(xmlWriter, "turboBig_hood-color", turboBig_hoodColor);
                    WriteXMLColorSaveElement(xmlWriter, "intercooler-color", intercoolerColor);
                    WriteXMLColorSaveElement(xmlWriter, "turbocharger_big-color", turboBigColor);
                    WriteXMLColorSaveElement(xmlWriter, "turbochargerSmall-color", turbochargerSmallColor);
                    WriteXMLColorSaveElement(xmlWriter, "turbocharger_small_airfilter-color", turboSmall_airfilter_color);

                    WriteXMLColorSaveElement(xmlWriter, "weber-color", turbochargerManifoldWeberColor);
                    WriteXMLColorSaveElement(xmlWriter, "twincarb-color", turbochargerManifoldTwinCarbColor);
                    WriteXMLColorSaveElement(xmlWriter, "blowoffValve-color", turboBigBlowoffValveColor);

                    WriteXMLColorSaveElement(xmlWriter, "original_intercooler-color", originalIntercoolerColor);
                    WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_big-color", originalTurbocchargerBigColor);
                    WriteXMLColorSaveElement(xmlWriter, "original_turbochargerSmall-color", originalTurbochargerSmallColor);
                    WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_small_airfilter-color", original_turboSmall_airfilter_color);

                    WriteXMLColorSaveElement(xmlWriter, "original_weber-color", originalTurbochargerManifoldWeberColor);
                    WriteXMLColorSaveElement(xmlWriter, "original_twincarb-color", originalTurbochargerManifoldTwinCarbColor);
                    WriteXMLColorSaveElement(xmlWriter, "original_blowoffValve-color", originalturboBigBlowoffValveColor);
                }

                else
                {
                    Color defaultColor = new Color(0.800f, 0.800f, 0.800f);
                    WriteXMLColorSaveElement(xmlWriter, "turboBig_hood-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "intercooler-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "turbocharger_big-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "turbochargerSmall-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "weber-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "twincarb-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "blowoffValve-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "turbocharger_small_airfilter-color", defaultColor);

                    WriteXMLColorSaveElement(xmlWriter, "original_intercooler-color", defaultColor);
                    WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_big-color", defaultColor);
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
            catch (Exception ex)
            {
            }
            
        }

        private void WriteXMLColorSaveElement(XmlWriter xmlWriter, string elementName, Color colorToSave)
        {
            try
            {
                xmlWriter.WriteStartElement(elementName);
                xmlWriter.WriteAttributeString("r", colorToSave.r.ToString());
                xmlWriter.WriteAttributeString("g", colorToSave.g.ToString());
                xmlWriter.WriteAttributeString("b", colorToSave.b.ToString());
                xmlWriter.WriteEndElement();
            }
            catch(Exception ex)
            {
            }
        }

        private void AddPartsColorMaterial()
        {
            if (colorHasToChange)
            {
                colorHasToChange = false;
                try
                {
                    turboBig_hood_renderer = turboBig_hood_part.rigidPart.GetComponentInChildren<MeshRenderer>();
                    if (turboBig_hood_renderer == null)
                    {
                        turboBig_hood_renderer = turboBig_hood_part.activePart.GetComponentInChildren<MeshRenderer>();

                    }
                    if (turboBig_hood_renderer != null)
                    {
                        if (turboBig_hood_renderer.material.name != "CAR_PAINT_REGULAR (Instance)")
                        {
                            turboBig_hood_renderer.material = regularCarPaintMaterial;
                        }
                    }
                }
                catch(Exception ex)
                {
                }
                try
                {
                    if (useDefaultColors)
                    {
                        SetPartMaterialColor(turboBig_hood_part, turboBig_hoodColor);
                        SetPartMaterialColor(turboBig_part, originalTurbocchargerBigColor);
                        SetPartMaterialColor(turboSmall_part, originalTurbochargerSmallColor);
                        SetPartMaterialColor(intercooler_part, originalIntercoolerColor);
                        SetPartMaterialColor(manifold_weber_part, originalTurbochargerManifoldWeberColor);
                        SetPartMaterialColor(manifold_twinCarb_part, originalTurbochargerManifoldTwinCarbColor);
                        SetPartMaterialColor(turboBig_blowoff_valve_part, originalturboBigBlowoffValveColor);
                        SetPartMaterialColor(turboSmall_airfilter_part, original_turboSmall_airfilter_color);
                    }
                    else
                    {
                        SetPartMaterialColor(turboBig_hood_part, turboBig_hoodColor);
                        SetPartMaterialColor(turboBig_part, turboBigColor);
                        SetPartMaterialColor(turboSmall_part, turbochargerSmallColor);
                        SetPartMaterialColor(intercooler_part, intercoolerColor);
                        SetPartMaterialColor(manifold_weber_part, turbochargerManifoldWeberColor);
                        SetPartMaterialColor(manifold_twinCarb_part, turbochargerManifoldTwinCarbColor);
                        SetPartMaterialColor(turboBig_blowoff_valve_part, turboBigBlowoffValveColor);
                        SetPartMaterialColor(turboSmall_airfilter_part, turboSmall_airfilter_color);
                    }
                }
                catch(Exception ex)
                {
                }
            }
        }

        private void SetPartMaterialColor(Part part, Color colorToPaint)
        {
            try
            {
                MeshRenderer meshRenderer;
                if (part == turboBig_part)
                {
                    meshRenderer = GameObject.Find("TurboCharger_Big_Compressor_Turbine").GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer.material.name == "Red_Acent (Instance)" || meshRenderer.material.name == "CAR_PAINT_REGULAR (Instance)")
                    {
                        if (useDefaultColors != true)
                        {
                            meshRenderer.material.SetColor("_Color", colorToPaint);
                        }

                    }
                    meshRenderer = GameObject.Find("TurboCharger_Big_Exhaust_Turbine").GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer.material.name == "Red_Acent (Instance)" || meshRenderer.material.name == "CAR_PAINT_REGULAR (Instance)")
                    {
                        if (useDefaultColors != true)
                        {
                            meshRenderer.material.SetColor("_Color", colorToPaint);
                        }
                    }
                }
                else if (part == turboSmall_part)
                {
                    try
                    {
                        meshRenderer = GameObject.Find("Turbocharger_Small_Wastegate").GetComponentInChildren<MeshRenderer>();
                        if (meshRenderer.material.name == "Red_Acent (Instance)" || meshRenderer.material.name == "CAR_PAINT_REGULAR (Instance)")
                        {
                            if (useDefaultColors != true)
                            {
                                meshRenderer.material.SetColor("_Color", colorToPaint);
                            }

                        }
                        meshRenderer = GameObject.Find("Turbocharger_Small_Exhaust_Turbine").GetComponentInChildren<MeshRenderer>();
                        if (meshRenderer.material.name == "Red_Acent (Instance)" || meshRenderer.material.name == "CAR_PAINT_REGULAR (Instance)")
                        {
                            if (useDefaultColors != true)
                            {
                                meshRenderer.material.SetColor("_Color", colorToPaint);
                            }

                        }
                        meshRenderer = GameObject.Find("Turbocharger_Small_Compressor_Turbine").GetComponentInChildren<MeshRenderer>();
                        if (meshRenderer.material.name == "Red_Acent (Instance)" || meshRenderer.material.name == "CAR_PAINT_REGULAR (Instance)")
                        {
                            if (useDefaultColors != true)
                            {
                                meshRenderer.material.SetColor("_Color", colorToPaint);
                            }

                        }
                    }
                    catch
                    {

                    }

                }
                else
                {
                    meshRenderer = part.rigidPart.GetComponentInChildren<MeshRenderer>();
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
            }
            catch(Exception ex)
            {
            }
            
        }
        public void SetBoostGaugeText(string valueToDisplay)
        {
            if (!boost_gauge_part.InstalledScrewed())
            {
                return;
            }
            if (!hasPower)
            {
                boostGaugeTextMesh.text = "";
                return;
            }

            boostGaugeTextMesh.text = valueToDisplay;
        }

        private void DetectChangingBoost()
        {
            return;
            try
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
                                if (gameObjectHit.name == "blowoff_valve" || gameObjectHit.name == "blowoff_valve(Clone)")
                                {
                                    if (turboBig_blowoff_valve_part.InstalledScrewed())
                                    {
                                        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                                        {
                                            if (boostSave.turboBig_max_boost < boostSave.turboBig_max_boost_limit)
                                            {
                                                boostSave.turboBig_max_boost += 0.05f;
                                                if (boostSave.turboBig_max_boost >= boostSave.turboBig_max_boost_limit)
                                                {
                                                    boostSave.turboBig_max_boost = boostSave.turboBig_max_boost_limit;
                                                }
                                            }
                                        }
                                        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                                        {
                                            if (boostSave.turboBig_max_boost > 1.55f)
                                            {
                                                boostSave.turboBig_max_boost -= 0.05f;
                                                if (boostSave.turboBig_max_boost <= 1.55f)
                                                {
                                                    boostSave.turboBig_max_boost = 1.55f;
                                                }
                                            }
                                        }
                                        ModClient.guiInteract("Increase/Decrease Max Boost: " + boostSave.turboBig_max_boost.ToString("0.00"));
                                    }
                                }
                                else if (gameObjectHit.name == "Turbocharger_Small_Wastegate" || gameObjectHit.name == "Turbocharger_Small_Wastegate(Clone)")
                                {
                                    if (turboSmall_airfilter_part.InstalledScrewed())
                                    {
                                        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                                        {
                                            if (boostSave.turboSmall_max_boost < boostSave.turboSmall_max_boost_limit)
                                            {
                                                boostSave.turboSmall_max_boost += 0.01f;
                                                if (boostSave.turboSmall_max_boost >= boostSave.turboSmall_max_boost_limit)
                                                {
                                                    boostSave.turboSmall_max_boost = boostSave.turboSmall_max_boost_limit;
                                                }
                                            }
                                        }
                                        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                                        {

                                            if (boostSave.turboSmall_max_boost > 0.8f)
                                            {
                                                boostSave.turboSmall_max_boost -= 0.01f;
                                                if (boostSave.turboSmall_max_boost <= 0.8f)
                                                {
                                                    boostSave.turboSmall_max_boost = 0.8f;
                                                }
                                            }
                                        }
                                        ModClient.guiInteract("Increase/Decrease Max Boost: " + boostSave.turboSmall_max_boost.ToString("0.00"));
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
                                     gameObjectHit.name == "Racing Turbocharger Hood(Clone)"
                                     || gameObjectHit.name == "Intercooler(Clone)"
                                     || gameObjectHit.name == "Racing Turbocharger(Clone)"
                                     || gameObjectHit.name == "GT Turbocharger(Clone)"
                                     || gameObjectHit.name == "Weber Manifold(Clone)"
                                     || gameObjectHit.name == "TwinCarb Manifold(Clone)"
                                     || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)"
                                     || gameObjectHit.name == "Racing Turbocharger Blowoff Valve(Clone)"
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
                                                turboBig_hoodColor = pickedUPsprayCanColor;

                                            }
                                            if (gameObjectHit.name == "Intercooler" || gameObjectHit.name == "Intercooler(Clone)")
                                            {
                                                intercoolerColor = modSprayColors[arrIndex];
                                                originalIntercoolerColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "Racing Turbocharger" || gameObjectHit.name == "Racing Turbocharger(Clone)")
                                            {
                                                turboBigColor = modSprayColors[arrIndex];
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
                                                turboBigBlowoffValveColor = modSprayColors[arrIndex];
                                                originalturboBigBlowoffValveColor = pickedUPsprayCanColor;
                                            }
                                            if(gameObjectHit.name == "GT Turbocharger Airfilter" || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)")
                                            {
                                                turboSmall_airfilter_color = modSprayColors[arrIndex];
                                                original_turboSmall_airfilter_color = pickedUPsprayCanColor;
                                            }
                                            MeshRenderer partRenderer;
                                            if (gameObjectHit.name == "Racing Turbocharger" || gameObjectHit.name == "Racing Turbocharger(Clone)")
                                            {
                                                if (turboBig_turbine == null)
                                                {
                                                    turboBig_turbine = GameObject.Find("TurboCharger_Big_Compressor_Turbine");
                                                }
                                                if (turboBig_turbine != null)
                                                {
                                                    partRenderer = turboBig_turbine.GetComponentInChildren<MeshRenderer>();
                                                }
                                                else
                                                {
                                                    partRenderer = null;
                                                }
                                               
                                            }

                                            colorHasToChange = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                
            }
            
        }

        private static void ToggleUseDefaultColors()
        {
            useDefaultColors = !useDefaultColors;
        }

        private static void ToggleNewGearRatios()
        {
            if (toggleNewGearRatios.Value is bool value)
            {
                if (value == true)
                {
                    newGearRatiosEnabled = true;
                    satsumaDriveTrain.gearRatios = newGearRatio;
                }
                else if (value == false)
                {
                    newGearRatiosEnabled = false;
                    satsumaDriveTrain.gearRatios = originalGearRatios;
                }
            }
        }

        private void PosReset()
        {
            try
            {
                foreach (Part part in partsList)
                {
                    if (!part.installed)
                    {
                        part.activePart.transform.position = part.defaultPartSaveInfo.position;
                    }
                }
            }
            catch(Exception ex)
            {
                ModConsole.Error(ex.Message);
            }
            
        }

        public float GetTurboChargerBoost()
        {
            return newTurboChargerBar;
        }

        public bool AllBigInstalled(bool ignoreScrewable = false)
        {
                return turboBig_part.InstalledScrewed(ignoreScrewable) &&
                    turboBig_intercooler_tube_part.InstalledScrewed(ignoreScrewable) &&
                    turboBig_exhaust_inlet_tube_part.InstalledScrewed(ignoreScrewable) &&
                    (turboBig_exhaust_outlet_tube_part.InstalledScrewed(ignoreScrewable) || turboBig_exhaust_outlet_straight_part.InstalledScrewed(ignoreScrewable)) &&
                    turboBig_blowoff_valve_part.InstalledScrewed(ignoreScrewable);
        }
        public bool AllSmallInstalled(bool ignoreScrewable = false)
        {
                return turboSmall_part.InstalledScrewed(ignoreScrewable) &&
                    (turboSmall_intercooler_tube_part.InstalledScrewed(ignoreScrewable) || turboSmall_manifold_twinCarb_tube_part.InstalledScrewed(ignoreScrewable)) &&
                    turboSmall_exhaust_inlet_tube_part.InstalledScrewed(ignoreScrewable) &&
                    turboSmall_exhaust_outlet_tube_part.InstalledScrewed(ignoreScrewable);
        }
        public bool AllOtherInstalled(bool ignoreScrewable = false)
        {
                return (manifold_weber_part.InstalledScrewed(ignoreScrewable) || manifold_twinCarb_part.InstalledScrewed(ignoreScrewable)) &&
                    (
                    (intercooler_part.InstalledScrewed(ignoreScrewable) &&
                    (intercooler_manifold_tube_weber_part.InstalledScrewed(ignoreScrewable) || intercooler_manifold_tube_twinCarb_part.InstalledScrewed(ignoreScrewable))
                    ) ||
                    turboSmall_manifold_twinCarb_tube_part.InstalledScrewed(ignoreScrewable)) &&
                    exhaust_header_part.InstalledScrewed(ignoreScrewable);
        }

        public bool AnyBigInstalled(bool ignoreScrewable = false)
        {
                return turboBig_part.InstalledScrewed(ignoreScrewable) ||
                    turboBig_intercooler_tube_part.InstalledScrewed(ignoreScrewable) ||
                    turboBig_exhaust_inlet_tube_part.InstalledScrewed(ignoreScrewable) ||
                    turboBig_exhaust_outlet_tube_part.InstalledScrewed(ignoreScrewable) ||
                    turboBig_blowoff_valve_part.InstalledScrewed(ignoreScrewable) ||
                    turboBig_exhaust_outlet_straight_part.InstalledScrewed(ignoreScrewable);
        }
        public bool AnySmallInstalled(bool ignoreScrewable = false)
        {
            return turboSmall_part.InstalledScrewed(ignoreScrewable) ||
                turboSmall_intercooler_tube_part.InstalledScrewed(ignoreScrewable) ||
                turboSmall_exhaust_inlet_tube_part.InstalledScrewed(ignoreScrewable) ||
                turboSmall_exhaust_outlet_tube_part.InstalledScrewed(ignoreScrewable) ||
                turboSmall_airfilter_part.InstalledScrewed(ignoreScrewable) ||
                turboSmall_manifold_twinCarb_tube_part.InstalledScrewed(ignoreScrewable);
        }

        public bool hasPower
        {
            get
            {
                if (carElectricsPower == null)
                {
                    GameObject carElectrics = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                    carElectricsPower = PlayMakerFSM.FindFsmOnGameObject(carElectrics, "Power");
                    return carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK").Value;
                }
                else
                {
                    return carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK").Value;
                }
            }
        }

        public bool engineRunning
        {
            get
            {
                return satsumaDriveTrain.rpm > 0;
            }
        }
    }
}