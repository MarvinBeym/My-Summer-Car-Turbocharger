using HutongGames.PlayMaker;
using ModApi.Attachable;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class SatsumaTurboCharger : Mod
    {
        /* Todo:
         *  -------------------------------------------------
         *  DONE: (has to be changed before first entering car/starting up ) toggle awd/rwd/fwd
         *  DONE: (no sound is played in 5th and 6th ) fix six gear mod not working
         *  
         *  DONE: :Check for steam
         *  save settings in file
         *  DONE: Intercooler
         *  DONE: (make pretty) gauge
         *  DONE: limited boost to 1.65bar
         *  DONE: turbocharger_intercooler__manifold_tube -> see throuth object
         *  DONE: Reset partSaves when starting new game
         *  DONE: prevent turboboost in Neutral
         *  DONE: Better calculation of turbocharger boost
         *           *  DONE: add position reset button
         *  backfire flames ?
         *  Draw power grapth/turbo grapth ?
         *  add ModsShop
         *  check for race carb installed
         *  model for other carb
         *  check for air fuel mixture and decrease/increase boost
         *  prevent inspection with turbo installed
         *  add blowoff valve as model which then can be configured using mouse wheel to increase pressure
         *  make n2o usable
         *  increase fuel consumption when turbo is used/installed
         *  more grip ?
         *  -------------------------------------------------
         */

        /*  -> next one Changelog v1.8:
         *  -------------------------------------------------
         *  added twincarb Manfifold and tube
         *  added check for twinCarb and raceCarb (weber) installed
         *  reduced trigger area of all parts.
         *  renamed parts that attach to racecarb (the one with 4 outlets) to xyz Weber (because it's a Weber Carburetor
         *  made some more parts paintable (both manifolds text, both turbocharger turbine wheels, Intercooler)
         *  changed how part painting is detected (now I can add every part to be painting system)
         *  changed how part paint is applied (now I can add every part to the painting system)
         *  changed how part paint is saved (recommend deleting all saveFiles)
         *  
         *  -------------------------------------------------
         *  
         *  Known bugs
         *  -------------------------------------------------
         *  Use Key still has to be "F" for Painting
         */

        //Material[] tttt = Resources.FindObjectsOfTypeAll<Material>(); //Get all Materials of game
        //Component sprayCanPlayMakerFSM = tests.Value.GetComponentInChildren<PlayMakerFSM>(); // get PlayMaker Component from GameObject
        public override string ID => "SatsumaTurboCharger"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing Turbocharger"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.8"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;

        private Vector3 turbocharger_big_installLocation = new Vector3(-0.275f, -0.062f, 0.288f);
        private Vector3 turbocharger_big_intercooler_tube_installLocation = new Vector3(0.32f, -0.044f, 1.52f);
        private Vector3 turbocharger_big_exhaust_inlet_tube_installLocation = new Vector3(-0.262f, -0.1705f, -0.072f);
        private Vector3 turbocharger_big_exhaust_outlet_tube_installLocation = new Vector3(-0.274f, -0.0125f, 0.288f);
        private Vector3 turbocharger_small_installLocation = new Vector3(-0.254f, -0.162f, 0.01f);
        private Vector3 turbocharger_small_intercooler_tube_installLocation = new Vector3(0.32f, -0.04f, 1.522f);
        private Vector3 turbocharger_small_exhaust_inlet_tube_installLocation = new Vector3(-0.136f, -0.217f, -0.137f);
        private Vector3 turbocharger_small_exhaust_outlet_tube_installLocation = new Vector3(-0.184f, -0.272f, -0.15f);

        private Vector3 turbocharger_hood_installLocation = new Vector3(0.0f, 0.241f, 1.68f);
        private Vector3 turbocharger_manifold_weber_installLocation = new Vector3(0f, -0.3f, 0.1f);
        private Vector3 turbocharger_manifold_twinCarb_installLocation = new Vector3(0f, -0.265f, 0.005f);
        private Vector3 turbocharger_boost_gauge_installLocation = new Vector3(0.5f, -0.04f, 0.125f);
        private Vector3 turbocharger_intercooler_installLocation = new Vector3(0.0f, -0.162f, 1.686f);
        private Vector3 turbocharger_intercooler_manifold_tube_weber_installLocation = new Vector3(-0.33f, -0.047f, 1.445f);
        private Vector3 turbocharger_intercooler_manifold_tube_twinCarb_installLocation = new Vector3(-0.33f, -0.047f, 1.445f);



        private GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private bool cracked = false;
        private string[] crackedMSCLoaderHashes = { "4e5af1f010743d8f48e74ea7472fed0e153bfd48", "9db4a94cede70acefb91a3862ee99f06e1987d15", "cdc72e09bb7dbc1e67e7dd84a394d6f8bad5c38c" };
        private string computedSHA1;
        private AssetBundle assets;
        private static GameObject turbocharger_big = new GameObject();
        private static GameObject turbocharger_big_intercooler_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_outlet_tube = new GameObject();

        private static GameObject turbocharger_small = new GameObject();
        private static GameObject turbocharger_small_intercooler_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_outlet_tube = new GameObject();

        private static GameObject turbocharger_hood = new GameObject();
        private static GameObject turbocharger_manifold_weber = new GameObject();
        private static GameObject turbocharger_manifold_twinCarb = new GameObject();
        private static GameObject turbocharger_boost_gauge = new GameObject();
        private static GameObject turbocharger_intercooler = new GameObject();
        private static GameObject turbocharger_intercooler_manifold_tube_weber = new GameObject();
        private static GameObject turbocharger_intercooler_manifold_tube_twinCarb = new GameObject();
        private static GameObject boostGauge;
        private TextMesh boostGaugeTextMesh;

        private Trigger turbocharger_big_Trigger;
        private Trigger turbocharger_big_intercoolerTubeTrigger;
        private Trigger turbocharger_big_exhaustInletTubeTrigger;
        private Trigger turbocharger_big_exhaustOutletTubeTrigger;
        
        private Trigger turbocharger_small_Trigger;
        private Trigger turbocharger_small_intercoolerTubeTrigger;
        private Trigger turbocharger_small_exhaustInletTubeTrigger;
        private Trigger turbocharger_small_exhaustOutletTubeTrigger;


        private Trigger turbocharger_manifoldWeberTrigger;
        private Trigger turbocharger_manifoldTwinCarbTrigger;
        private Trigger turbocharger_hoodTrigger;
        private Trigger turbocharger_boostGaugeTrigger;
        private Trigger turbocharger_intercoolerTrigger;
        private Trigger turbocharger_intercoolerManifoldTubeWeberTrigger;
        private Trigger turbocharger_intercoolerManifoldTubeTwinCarbTrigger;

        private GameObject satsumaRaceCarb;
        //private FsmState n2oBottle;
        //private FsmFloat n2oBottlePSI;
        private CarController carController;
        private GameObject elect;
        private PlayMakerFSM power;
        private MeshRenderer[] sprayCansMeshRenders;
        private Material regularCarPaintMaterial;
        private MeshRenderer turbocharger_hood_renderer;


        private ModAudio turbocharger_loop = new ModAudio();
        private ModAudio turbocharger_blowoff = new ModAudio();

        private Settings displayTurboChargerValues = new Settings("toggleTurboChargerValuesButton", "Enable/Disable", SwitchTurboChargerGui);
        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", new Action(SatsumaTurboCharger.PosReset));

        //Car values
        private float engineRPM = 0;
        private float enginePowerMultiplier;
        private float enginePowerCurrent;
        private FsmFloat _enginePowerMultiplier;
        private float newTurboChargerBar = 0;

        private Color loadedHoodColor;
        private bool isItemInHand;
        //Turbocharger audio sounds
        private AudioSource turboLoop;
        private AudioSource turboBlowOffShot;
        private bool weberCarb_inst = false;
        private bool electricityOn = false;

        //Turbocharger parts
        private static TurboChargerBigPart turboChargerBigPart;
        private static TurboChargerBigIntercoolerTubePart turboChargerBigIntercoolerTubePart;
        private static TurboChargerBigExhaustInletTubePart turboChargerBigExhaustInletTubePart;
        private static TurboChargerBigExhaustOutletTubePart turboChargerBigExhaustOutletTubePart;

        private static TurboChargerSmallPart turboChargerSmallPart;
        private static TurboChargerSmallIntercoolerTubePart turboChargerSmallIntercoolerTubePart;
        private static TurboChargerSmallExhaustInletTubePart turboChargerSmallExhaustInletTubePart;
        private static TurboChargerSmallExhaustOutletTubePart turboChargerSmallExhaustOutletTubePart;

        private static TurboChargerHoodPart turboChargerHoodPart;
        private static TurboChargerManifoldWeberPart turboChargerManifoldWeberPart;
        private static TurboChargerManifoldTwinCarbPart turboChargerManifoldTwinCarbPart;
        private static TurboChargerBoostGaugePart turboChargerBoostGaugePart;
        private static TurboChargerIntercoolerPart turboChargerIntercoolerPart;
        private static TurboChargerIntercoolerManifoldTubeWeberPart turboChargerIntercoolerManifoldTubeWeberPart;
        private static TurboChargerIntercoolerManifoldTubeTwinCarbPart turboChargerIntercoolerManifoldTubeTwinCarbPart;

        private static bool displayTurboChargerValuesOnGui = false;
        private bool allPartsInstalled = false;
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
        private GameObject weberCarb;
        private GameObject twinCarb;
        private bool twinCarb_inst = false;
        private Color pickedUPsprayCanColor;

        private Color hoodColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerBigColor = new Color(0.800f, 0.800f, 0.800f);
        private Color intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
        private Color[] ingameSprayColors = new Color[13];
        private Color[] modSprayColors = new Color[13];
        private Color turbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
        private string partsColorSave;


        //private bool raceCarbInstalled;

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

        private const string turbocharger_small_SaveFile = "turbocharger_small_partSave.txt";
        private const string turbocharger_small_intercooler_tube_SaveFile = "turbocharger_small_intercooler_tube_partSave.txt";
        private const string turbocharger_small_exhaust_inlet_tube_SaveFile = "turbocharger_small_exhaust_inlet_tube_partSave.txt";
        private const string turbocharger_small_exhaust_outlet_tube_SaveFile = "turbocharger_small_exhaust_outlet_tube_partSave.txt";

        private const string turbocharger_hood_SaveFile = "turbocharger_hood_partSave.txt";
        
        private const string turbocharger_manifold_weber_SaveFile = "turbocharger_manifold_weber_partSave.txt";
        private const string turbocharger_manifold_twinCarb_SaveFile = "turbocharger_manifold_twinCarb_partSave.txt";
        private const string turbocharger_boost_gauge_SaveFile = "turbocharger_boost_gauge_partSave.txt";
        private const string turbocharger_intercooler_SaveFile = "turbocharger_intercooler_partSave.txt";
        private const string turbocharger_intercooler_manifold_tube_weber_SaveFile = "turbocharger_intercooler_manifold_tube_weber_partSave.txt";
        private const string turbocharger_intercooler_manifold_tube_twinCarb_SaveFile = "turbocharger_intercooler_manifold_tube_twinCarb_partSave.txt";

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

        public override void OnNewGame()
        {
            // Called once, when starting a New Game, you can reset your saves here
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_intercooler_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_exhaust_inlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_exhaust_outlet_tube_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_intercooler_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_exhaust_inlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_exhaust_outlet_tube_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_hood_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_weber_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_twinCarb_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_boost_gauge_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_manifold_tube_weber_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);
            WritePartsColorSave(true);
        }



        public override void OnLoad()
        {

            /*
            ingameSprayColors[0] = new Color(0.961f, 0.961f, 0.961f, 1f);
            ingameSprayColors[1] = new Color(0.965f, 0.510f, 0.122f, 1f);
            ingameSprayColors[2] = new Color(0.824f, 0.176f, 0.204f, 1f);
            ingameSprayColors[3] = new Color(0.000f, 0.494f, 0.773f, 1f);
            ingameSprayColors[4] = new Color(0.380f, 0.247f, 0.208f, 1f);
            ingameSprayColors[5] = new Color(0.114f, 0.110f, 0.129f, 1f);
            ingameSprayColors[6] = new Color(1.000f, 0.831f, 0.004f, 1f);
            ingameSprayColors[7] = new Color(0.000f, 0.447f, 0.212f, 1f);
            ingameSprayColors[8] = new Color(0.490f, 0.816f, 0.851f, 1f);
            ingameSprayColors[9] = new Color(0.569f, 0.569f, 0.569f, 1f);
            ingameSprayColors[10] = new Color(0.953f, 0.529f, 0.722f, 1f);
            ingameSprayColors[11] = new Color(0.149f, 0.310f, 0.318f, 1f);
            ingameSprayColors[12] = new Color(0.125f, 0.125f, 0.125f, 1f);
            
            ingameSprayColors[0] = new Color(245 / 255, 245 / 255, 245 / 255, 1f);
            ingameSprayColors[1] = new Color(246 / 255, 130 / 255, 31 / 255, 1f);
            ingameSprayColors[2] = new Color(210 / 255, 45 / 255, 52 / 255, 1f);
            ingameSprayColors[3] = new Color(126 / 255, 197 / 255, 197 / 255, 1f);
            ingameSprayColors[4] = new Color(97 / 255, 63 / 255, 53 / 255, 1f);
            ingameSprayColors[5] = new Color(29 / 255, 28 / 255, 33 / 255, 1f);
            ingameSprayColors[6] = new Color(255 / 255, 212 / 255, 1 / 255, 1f);
            ingameSprayColors[7] = new Color(0 / 255, 114 / 255, 54 / 255, 1f);
            ingameSprayColors[8] = new Color(125 / 255, 208 / 255, 217 / 255, 1f);
            ingameSprayColors[9] = new Color(145 / 255, 145 / 255, 145 / 255, 1f);
            ingameSprayColors[10] = new Color(243 / 255, 135 / 255, 184 / 255, 1f);
            ingameSprayColors[11] = new Color(38 / 255, 79 / 255, 81 / 255, 1f);
            ingameSprayColors[12] = new Color(29 / 255, 28 / 255, 33 / 255, 1f);
            */

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


            ModConsole.Print("DonnerTechRacing Turbocharger Mod [ v" + this.Version + "]" + " started loaded");

            string gamePath = Directory.GetCurrentDirectory();
            string mscLoaderDLLPath = Path.Combine(gamePath, "mysummercar_Data\\Managed\\MSCLoader.dll");
            //string modAssetsFolderPath = ModLoader.GetModAssetsFolder(this);
            //string mscLoaderDLLPath = modAssetsFolderPath.Replace("Mods\\Assets\\SatsumaTurboCharger", "mysummercar_Data\\Managed\\MSCLoader.dll");
            if (File.Exists(mscLoaderDLLPath))
            {
                computedSHA1 = CalculateSHA1(mscLoaderDLLPath);
            }
            else
            {
                computedSHA1 = "none";
            }
            



            //Component sprayCanPlayMakerFSM = tests.Value.GetComponentInChildren<PlayMakerFSM>(); // get PlayMaker Component from GameObject
            for(int i = 0; i < crackedMSCLoaderHashes.Length; i++)
            {
                if(computedSHA1 == crackedMSCLoaderHashes[i] == true)
                {
                    cracked = true;
                    break;
                }
            }
            if (ModLoader.CheckSteam() == false || cracked)
            {
                if (ModLoader.CheckSteam() == false)
                {
                    ModConsole.Warning("You are not running a legit version of 'My Summer Car' from Steam");
                    ModConsole.Warning("Please support the developer of the game!");
                    ModConsole.Warning("This mod will not work if your version of the game is not legit");
                    ModConsole.Warning("Other cause: you started the .exe instead of through steam");
                    ModUI.ShowMessage(
                        "You are running a version of 'My Summer Car' without Steam.\n" +
                        "Either it is a pirated copy of the game or you started the .exe of the game.\n" +
                        "Please buy the game and support Developing.\n\n" + "You had enough time to test the game.",
                        "Illegal copy of Game Detected - Mod was disabled");
                }
                if (cracked)
                {
                    ModConsole.Warning("You are running a modified version of the 'MSC ModLoader");
                    ModConsole.Warning("This version might add dangerous stuff which could potentially delete files on your pc or do something else");
                    ModConsole.Warning("Please use the original version of the ModLoader made by @piotrulos");
                    ModUI.ShowMessage(
                        "You are running a modified version of the 'MSC ModLoader.\n" +
                        "This version might add dangerous stuff which could potentially delete files on your pc or do something else.\n" +
                        "Please use the original version made by @piotrulos.",
                        "DANGEROUS/MODIFIED version of ModLoader found - Mod will disable!");
                }
            }
            else
            {
                //"S:\\Spiele\\Steam\\steamapps\\common\\My Summer Car\\Mods\\Assets\\SatsumaTurboCharger"
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
                
                if(errorDetected == false)
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
                        satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
                    }
                    catch
                    {
                        ModConsole.Error("Could not find Satsuma, Drivetrain");
                    }



                    //n2oBottle = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmStates[4];
                    //n2oBottlePSI = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmVariables.FloatVariables[4];
                    weberCarb = GameObject.Find("racing carburators(Clone)");
                    twinCarb = GameObject.Find("twin carburators(Clone)");
                    originalGearRatios = satsumaDriveTrain.gearRatios;
                    satsumaDriveTrain.revLimiter = true;
                    carController = satsuma.GetComponent<CarController>();
                    //Axles test = satsuma.GetComponent<Axles>();
                    //currentGear = satsumaDriveTrain.gear;

                    //satsumaDriveTrain.gearRatios = newGearRatio;
                    

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

                    SatsumaTurboCharger.turbocharger_big = (assets.LoadAsset("turbocharger_big.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_big_intercooler_tube = (assets.LoadAsset("turbocharger_big_intercooler_tube.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_big_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_inlet_tube.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_big_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_outlet_tube.prefab") as GameObject);

                    SatsumaTurboCharger.turbocharger_small = (assets.LoadAsset("turbocharger_small.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_small_intercooler_tube = (assets.LoadAsset("turbocharger_small_intercooler_tube.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_small_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_inlet_tube.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_small_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_outlet_tube.prefab") as GameObject);

                    SatsumaTurboCharger.turbocharger_hood = (assets.LoadAsset("turbocharger_hood.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_manifold_weber = (assets.LoadAsset("turbocharger_manifold_weber.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_manifold_twinCarb = (assets.LoadAsset("turbocharger_manifold_twinCarb.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_intercooler = (assets.LoadAsset("turbocharger_intercooler.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_weber = (assets.LoadAsset("turbocharger_tube_intercooler_manifold_weber.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_twinCarb = (assets.LoadAsset("turbocharger_tube_intercooler_manifold_twinCarb.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_boost_gauge = (assets.LoadAsset("turbocharger_boost_gauge.prefab") as GameObject);

                    SatsumaTurboCharger.turbocharger_big.name = "Turbocharger Big";
                    SatsumaTurboCharger.turbocharger_big_intercooler_tube.name = "Turbocharger Big Intercooler Tube";
                    SatsumaTurboCharger.turbocharger_big_exhaust_inlet_tube.name = "Turbocharger Big Exhaust Inlet Tube";
                    SatsumaTurboCharger.turbocharger_big_exhaust_outlet_tube.name = "Turbocharger Big Exhaust Outlet Tube";

                    SatsumaTurboCharger.turbocharger_small.name = "Turbocharger Small";
                    SatsumaTurboCharger.turbocharger_small_intercooler_tube.name = "Turbocharger Small Intercooler Tube";
                    SatsumaTurboCharger.turbocharger_small_exhaust_inlet_tube.name = "Turbocharger Small Exhaust Inlet Tube";
                    SatsumaTurboCharger.turbocharger_small_exhaust_outlet_tube.name = "Turbocharger Small Exhaust Outlet Tube";

                    SatsumaTurboCharger.turbocharger_hood.name = "Turbocharger Hood";
                    SatsumaTurboCharger.turbocharger_manifold_weber.name = "Turbocharger Manifold Weber";
                    SatsumaTurboCharger.turbocharger_manifold_twinCarb.name = "Turbocharger Manifold TwinCarb";
                    SatsumaTurboCharger.turbocharger_intercooler.name = "Turbocharger Intercooler";
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_weber.name = "Turbocharger Intercooler-Manifold Tube Weber";
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_twinCarb.name = "Turbocharger Intercooler-Manifold Tube TwinCarb";
                    SatsumaTurboCharger.turbocharger_boost_gauge.name = "Turbocharger Boost Gauge";

                    SatsumaTurboCharger.turbocharger_big.tag = "PART";
                    SatsumaTurboCharger.turbocharger_big_intercooler_tube.tag = "PART";
                    SatsumaTurboCharger.turbocharger_big_exhaust_inlet_tube.tag = "PART";
                    SatsumaTurboCharger.turbocharger_big_exhaust_outlet_tube.tag = "PART";

                    SatsumaTurboCharger.turbocharger_small.tag = "PART";
                    SatsumaTurboCharger.turbocharger_small_intercooler_tube.tag = "PART";
                    SatsumaTurboCharger.turbocharger_small_exhaust_inlet_tube.tag = "PART";
                    SatsumaTurboCharger.turbocharger_small_exhaust_outlet_tube.tag = "PART";

                    SatsumaTurboCharger.turbocharger_hood.tag = "PART";
                    SatsumaTurboCharger.turbocharger_manifold_weber.tag = "PART";
                    SatsumaTurboCharger.turbocharger_manifold_twinCarb.tag = "PART";
                    SatsumaTurboCharger.turbocharger_intercooler.tag = "PART";
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_weber.tag = "PART";
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_twinCarb.tag = "PART";
                    SatsumaTurboCharger.turbocharger_boost_gauge.tag = "PART";

                    SatsumaTurboCharger.turbocharger_big.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_big_intercooler_tube.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_big_exhaust_inlet_tube.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_big_exhaust_outlet_tube.layer = LayerMask.NameToLayer("Parts");

                    SatsumaTurboCharger.turbocharger_small.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_small_intercooler_tube.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_small_exhaust_inlet_tube.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_small_exhaust_outlet_tube.layer = LayerMask.NameToLayer("Parts");

                    SatsumaTurboCharger.turbocharger_hood.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_manifold_weber.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_manifold_twinCarb.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_intercooler.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_weber.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_twinCarb.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_boost_gauge.layer = LayerMask.NameToLayer("Parts");

                    GameObject originalCylinerHead = GameObject.Find("cylinder head(Clone)");

                    turbocharger_big_Trigger = new Trigger("TurbochargerBigTrigger", originalCylinerHead, turbocharger_big_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_big_intercoolerTubeTrigger = new Trigger("TurbochargerBigIntercoolerTubeTrigger", satsuma, turbocharger_big_intercooler_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_big_exhaustInletTubeTrigger = new Trigger("TurbochargerBigExhaustInletTubeTrigger", originalCylinerHead, turbocharger_big_exhaust_inlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_big_exhaustOutletTubeTrigger = new Trigger("TurbochargerBigExhaustOutletTubeTrigger", originalCylinerHead, turbocharger_small_exhaust_outlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);

                    turbocharger_small_Trigger = new Trigger("TurbochargerSmallTrigger", originalCylinerHead, turbocharger_small_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_small_intercoolerTubeTrigger = new Trigger("TurbochargerSmallIntercoolerTubeTrigger", satsuma, turbocharger_small_intercooler_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_small_exhaustInletTubeTrigger = new Trigger("TurbochargerSmallExhaustInletTubeTrigger", originalCylinerHead, turbocharger_small_exhaust_inlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_small_exhaustOutletTubeTrigger = new Trigger("TurbochargerSmallExhaustOutletTubeTrigger", originalCylinerHead, turbocharger_small_exhaust_outlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    
                    turbocharger_hoodTrigger = new Trigger("TurbochargerHoodTrigger", satsuma, turbocharger_hood_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_manifoldWeberTrigger = new Trigger("TurbochargerManifoldWeberTrigger", originalCylinerHead, turbocharger_manifold_weber_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_manifoldTwinCarbTrigger = new Trigger("TurbochargerManifoldTwinCarbTrigger", originalCylinerHead, turbocharger_manifold_twinCarb_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);

                    turbocharger_boostGaugeTrigger = new Trigger("TurbochargerBoostGaugeTrigger", GameObject.Find("dashboard(Clone)"), turbocharger_boost_gauge_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_intercoolerTrigger = new Trigger("TurbochargerIntercoolerTrigger", satsuma, turbocharger_intercooler_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.2f, 0.2f), false);
                    turbocharger_intercoolerManifoldTubeWeberTrigger = new Trigger("TurbochargerIntercoolerManifoldTubeWeberTrigger", satsuma, turbocharger_intercooler_manifold_tube_weber_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
                    turbocharger_intercoolerManifoldTubeTwinCarbTrigger = new Trigger("TurbochargerIntercoolerManifoldTubeTwinCarbTrigger", satsuma, turbocharger_intercooler_manifold_tube_twinCarb_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);

                    LoadSetAllParts();

                    assets.Unload(false);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_big);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_big_intercooler_tube);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_big_exhaust_inlet_tube);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_big_exhaust_outlet_tube);

                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_small);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_small_intercooler_tube);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_small_exhaust_inlet_tube);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_small_exhaust_outlet_tube);

                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_hood);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_manifold_weber);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_manifold_twinCarb);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_intercooler);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_weber);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_intercooler_manifold_tube_twinCarb);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_boost_gauge);


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

                    ModConsole.Print("DonnerTechRacing Turbocharger Mod [ v" + this.Version + "]" + " loaded");
                }
                
            }
        }

        public override void ModSettings()
        {
            Settings.AddButton(this, displayTurboChargerValues, "DEBUG TurboCharger GUI");

            Settings.AddButton(this, resetPosSetting, "reset part location");
        }


        public override void OnSave()
        {
            try
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerBigPart.getSaveInfo(), turbocharger_big_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerBigIntercoolerTubePart.getSaveInfo(), turbocharger_big_intercooler_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerBigExhaustInletTubePart.getSaveInfo(), turbocharger_big_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerBigExhaustOutletTubePart.getSaveInfo(), turbocharger_big_exhaust_outlet_tube_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerSmallPart.getSaveInfo(), turbocharger_small_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerSmallIntercoolerTubePart.getSaveInfo(), turbocharger_small_intercooler_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerSmallExhaustInletTubePart.getSaveInfo(), turbocharger_small_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerSmallExhaustOutletTubePart.getSaveInfo(), turbocharger_small_exhaust_outlet_tube_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerHoodPart.getSaveInfo(), turbocharger_hood_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerManifoldWeberPart.getSaveInfo(), turbocharger_manifold_weber_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerManifoldTwinCarbPart.getSaveInfo(), turbocharger_manifold_twinCarb_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerBoostGaugePart.getSaveInfo(), turbocharger_boost_gauge_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerIntercoolerPart.getSaveInfo(), turbocharger_intercooler_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerIntercoolerManifoldTubeWeberPart.getSaveInfo(), turbocharger_intercooler_manifold_tube_weber_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerIntercoolerManifoldTubeTwinCarbPart.getSaveInfo(), turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);
                WritePartsColorSave(false);
            }
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[TurboChargerMod]</b> - an error occured while attempting to save part info.. see: " + ex.ToString());
            }

        }

        public override void OnGUI()
        {
            if (displayTurboChargerValuesOnGui == true)
            {
                GUI.Label(new Rect(20, 20, 200, 100), "------------------------------------");
                GUI.Label(new Rect(20, 40, 200, 100), "Engine RPM: " + ((int)engineRPM).ToString());
                // GUI.Label(new Rect(20, 60, 220, 100), "Turbo Charger RPM: " + ((int)turboChargerRPM).ToString());
                GUI.Label(new Rect(20, 60, 200, 100), "Turbo Charger bar: " + newTurboChargerBar.ToString("n3"));
                GUI.Label(new Rect(20, 80, 200, 100), "Power Current: " + ((int)enginePowerCurrent).ToString());
                GUI.Label(new Rect(20, 100, 200, 100), "Power Multiplier: " + enginePowerMultiplier.ToString("n2"));
                GUI.Label(new Rect(20, 120, 200, 100), "km/h: " + ((int)satsumaDriveTrain.differentialSpeed));
                GUI.Label(new Rect(20, 140, 200, 100), "Torque: " + satsumaDriveTrain.torque);
                GUI.Label(new Rect(20, 160, 200, 100), "Clutch Max Torque: " + satsumaDriveTrain.clutchMaxTorque);
                GUI.Label(new Rect(20, 180, 200, 100), "Clutch Torque Multiplier: " + satsumaDriveTrain.clutchTorqueMultiplier);
                //GUI.Label(new Rect(20, 200, 200, 100), "N2o active: " + n2oBottle.Active);
                GUI.Label(new Rect(20, 200, 200, 100), "Electricity on: " + electricityOn);
                GUI.Label(new Rect(20, 220, 200, 100), "------------------------------------");


            }
        }
        public override void Update()
        {
            if (ModLoader.CheckSteam() == true && cracked == false && errorDetected == false)
            {

                electricityOn = power.FsmVariables.FindFsmBool("ElectricsOK").Value;
                SetPartsColorMaterial();
                DetectPaintingPart();
                CheckPartsInstalledTrigger();
                /*
                if (satsumaRaceCarb != null && satsumaRaceCarb.transform.parent.parent.name != null)
                {
                    raceCarbInstalled = true;
                }
                else
                    raceCarbInstalled = false;
                */


                if (turboChargerBoostGaugePart.installed)
                {
                    //Part is installed GameObject changes to Clone
                    boostGauge = turboChargerBoostGaugePart.rigidPart;
                    boostGaugeTextMesh = boostGauge.GetComponentInChildren<TextMesh>();
                }
                else
                    boostGauge = null;

                /*
                if ((bool)toggleAWD.GetValue() == true && satsumaDriveTrain.transmission == Drivetrain.Transmissions.FWD)
                {
                    
                }
                else if ((bool)toggleAWD.GetValue() == false && satsumaDriveTrain.transmission == Drivetrain.Transmissions.AWD)
                {
                    satsumaDriveTrain.transmission = Drivetrain.Transmissions.FWD;
                }
                */

                if (
                    (
                    (turboChargerBigPart.installed && turboChargerBigIntercoolerTubePart.installed && turboChargerBigExhaustInletTubePart.installed && turboChargerBigExhaustOutletTubePart.installed)
                    ||
                    (turboChargerSmallPart.installed && turboChargerSmallIntercoolerTubePart.installed && turboChargerSmallExhaustInletTubePart.installed && turboChargerSmallExhaustOutletTubePart.installed)
                    )
                    &&
                    (
                    (turboChargerManifoldWeberPart.installed || turboChargerManifoldTwinCarbPart.installed)
                    && turboChargerIntercoolerPart.installed
                    && (turboChargerIntercoolerManifoldTubeWeberPart.installed || turboChargerIntercoolerManifoldTubeTwinCarbPart.installed)
                    )
                    )
                    allPartsInstalled = true;
                else
                {
                    if (allPartsInstalled == true)
                    {
                        allPartsInstalled = false;
                        turbocharger_loop.Stop();
                        turbocharger_blowoff.Stop();
                    }

                }


                if (allPartsInstalled == true)
                {

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
                        if (turboChargerBigPart.installed)
                        {
                            newTurboChargerBar = 0.86f;
                            enginePowerMultiplier = 0.86f;
                        }
                        else if (turboChargerSmallPart.installed)
                        {
                            newTurboChargerBar = 0.92f;
                            enginePowerMultiplier = 0.92f;
                        }

                    }



                    //Continous Loop of turbo sound -> if not already exists it will be created and played
                    if (turboLoop == null)
                    {
                        CreateTurboLoop();
                    }
                    else if (turboLoop.isPlaying == false)
                        turboLoop.Play();

                    //Set Volume and Pitch based on engineRPM -> to make sound go ssssSuuuuuUUUUUUU
                    if (turboChargerBigPart.installed)
                    {
                        turboLoop.volume = engineRPM * 0.00005f;
                        turboLoop.pitch = engineRPM * 0.00018f;
                    }
                    else if (turboChargerSmallPart.installed)
                    {
                        turboLoop.volume = engineRPM * 0.00003f;
                        turboLoop.pitch = engineRPM * 0.00012f;
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
                        if (turboChargerBigPart.installed)
                        {
                            SetBoostGaugeText(0.86f, false);
                        }
                        else if (turboChargerSmallPart.installed)
                        {
                            SetBoostGaugeText(0.92f, false);
                        }
                        //SetBoostGaugeText(0.86f, false);
                        TriggerBlowOff();
                    }



                    /*
                    if (backFireEnabled == true)
                    {
                        TriggerBackFire();
                    }
                    */
                }
            }
        }

        private void CheckPartsInstalledTrigger()
        {
            if (turboChargerBigPart.installed)
            {
                turbocharger_big_intercoolerTubeTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_big_intercoolerTubeTrigger.triggerGameObject.SetActive(false);
            }

            if (turboChargerSmallPart.installed)
            {
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(false);
            }



            if (!turboChargerBigExhaustInletTubePart.installed || !turboChargerBigExhaustOutletTubePart.installed || !turboChargerBigIntercoolerTubePart.installed)
            {
                turbocharger_big_Trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_big_Trigger.triggerGameObject.SetActive(true);
            }

            if (!turboChargerSmallExhaustInletTubePart.installed || !turboChargerSmallExhaustOutletTubePart.installed || !turboChargerSmallIntercoolerTubePart.installed)
            {
                turbocharger_small_Trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_Trigger.triggerGameObject.SetActive(true);
            }


            if (turboChargerBigExhaustInletTubePart.installed || turboChargerBigExhaustOutletTubePart.installed || turboChargerBigIntercoolerTubePart.installed)
            {

                turbocharger_small_exhaustInletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_small_exhaustOutletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(false);
            }
            else
            {

                turbocharger_small_exhaustInletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_small_exhaustOutletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_small_intercoolerTubeTrigger.triggerGameObject.SetActive(true);
            }

            if (turboChargerSmallExhaustInletTubePart.installed || turboChargerSmallExhaustOutletTubePart.installed || turboChargerSmallIntercoolerTubePart.installed)
            {
                turbocharger_big_exhaustInletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_big_exhaustOutletTubeTrigger.triggerGameObject.SetActive(false);
                turbocharger_big_intercoolerTubeTrigger.triggerGameObject.SetActive(false);
            }
            else
            {

                turbocharger_big_exhaustInletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_big_exhaustOutletTubeTrigger.triggerGameObject.SetActive(true);
                turbocharger_big_intercoolerTubeTrigger.triggerGameObject.SetActive(true);
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

            if (weberCarb_inst)
            {
                turbocharger_manifoldWeberTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_manifoldWeberTrigger.triggerGameObject.SetActive(false);
            }

            if (twinCarb_inst)
            {
                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_manifoldTwinCarbTrigger.triggerGameObject.SetActive(false);
            }


            if (turboChargerManifoldWeberPart.installed)
            {
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_intercoolerManifoldTubeWeberTrigger.triggerGameObject.SetActive(false);
            }

            if (turboChargerManifoldTwinCarbPart.installed)
            {
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_intercoolerManifoldTubeTwinCarbTrigger.triggerGameObject.SetActive(false);
            }




            if ((turboChargerManifoldWeberPart.installed || turboChargerManifoldTwinCarbPart.installed) && (turboChargerIntercoolerManifoldTubeWeberPart.installed || turboChargerIntercoolerManifoldTubeTwinCarbPart.installed))
            {
                turbocharger_intercoolerTrigger.triggerGameObject.SetActive(true);

            }
            else
            {
                turbocharger_intercoolerTrigger.triggerGameObject.SetActive(false);
            }

        }


        private static void SwitchTurboChargerGui()
        {
            displayTurboChargerValuesOnGui = !displayTurboChargerValuesOnGui;
        }

        private void LoadSetAllParts()
        {
            PartSaveInfo turbocharger_big_SaveInfo = null;
            PartSaveInfo turbocharger_big_intercooler_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_exhaust_inlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_exhaust_outlet_tube_SaveInfo = null;

            PartSaveInfo turbocharger_small_SaveInfo = null;
            PartSaveInfo turbocharger_small_intercooler_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_exhaust_inlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_exhaust_outlet_tube_SaveInfo = null;

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

            turbocharger_small_SaveInfo = this.loadSaveData(turbocharger_small_SaveFile);
            turbocharger_small_intercooler_tube_SaveInfo = this.loadSaveData(turbocharger_small_intercooler_tube_SaveFile);
            turbocharger_small_exhaust_inlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_inlet_tube_SaveFile);
            turbocharger_small_exhaust_outlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_outlet_tube_SaveFile);


            turbocharger_hood_SaveInfo = this.loadSaveData(turbocharger_hood_SaveFile);
            turbocharger_manifold_weber_SaveInfo = this.loadSaveData(turbocharger_manifold_weber_SaveFile);
            turbocharger_manifold_twinCarb_SaveInfo = this.loadSaveData(turbocharger_manifold_twinCarb_SaveFile);
            turbocharger_boost_gauge_SaveInfo = this.loadSaveData(turbocharger_boost_gauge_SaveFile);
            turbocharger_intercooler_SaveInfo = this.loadSaveData(turbocharger_intercooler_SaveFile);
            turbocharger_intercooler_manifold_tube_weber_SaveInfo = this.loadSaveData(turbocharger_intercooler_manifold_tube_weber_SaveFile);
            turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo = this.loadSaveData(turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);
            GameObject originalCylinerHead = GameObject.Find("cylinder head(Clone)");
            satsumaRaceCarb = GameObject.Find("racing carburators(Clone)");

            turboChargerBigPart = new TurboChargerBigPart(
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
            turboChargerBigIntercoolerTubePart = new TurboChargerBigIntercoolerTubePart(
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
            turboChargerBigExhaustInletTubePart = new TurboChargerBigExhaustInletTubePart(
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
            turboChargerBigExhaustOutletTubePart = new TurboChargerBigExhaustOutletTubePart(
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

            turboChargerSmallPart = new TurboChargerSmallPart(
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
            turboChargerSmallIntercoolerTubePart = new TurboChargerSmallIntercoolerTubePart(
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
            turboChargerSmallExhaustInletTubePart = new TurboChargerSmallExhaustInletTubePart(
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
            turboChargerSmallExhaustOutletTubePart = new TurboChargerSmallExhaustOutletTubePart(
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


            turboChargerHoodPart = new TurboChargerHoodPart(
                turbocharger_hood_SaveInfo,
                turbocharger_hood,
                satsuma,
                turbocharger_hoodTrigger,
                turbocharger_hood_installLocation,
                new Quaternion(0, 180, 0, 0)
            );


            turboChargerManifoldWeberPart = new TurboChargerManifoldWeberPart(
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

            turboChargerManifoldTwinCarbPart = new TurboChargerManifoldTwinCarbPart(
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


            turboChargerBoostGaugePart = new TurboChargerBoostGaugePart(
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
            turboChargerIntercoolerPart = new TurboChargerIntercoolerPart(
               turbocharger_intercooler_SaveInfo,
               turbocharger_intercooler,
               satsuma,
               turbocharger_intercoolerTrigger,
               turbocharger_intercooler_installLocation,
               new Quaternion
               {
                   eulerAngles = new Vector3(15, 0, 0)
               }
               //old: 0.03f, -0.015f, 0.142f
           );
            turboChargerIntercoolerManifoldTubeWeberPart = new TurboChargerIntercoolerManifoldTubeWeberPart(
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
            turboChargerIntercoolerManifoldTubeTwinCarbPart = new TurboChargerIntercoolerManifoldTubeTwinCarbPart(
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

            LoadPartsColorSave();
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
                            //pickedUPsprayCanColor = loadedHoodColor;
                        }
                    }
                }
                xmlReader.Close();
            }
            else
            {
                loadedHoodColor = new Color(0.800f, 0.800f, 0.800f, 1.000f);
                pickedUPsprayCanColor = loadedHoodColor;
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
                WriteXMLColorSaveElement(xmlWriter, "weber-color", turbochargerManifoldWeberColor);
                WriteXMLColorSaveElement(xmlWriter, "twincarb-color", turbochargerManifoldTwinCarbColor);
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

        private void SetPartsColorMaterial()
        {
            
            try
            {
                turbocharger_hood_renderer = turboChargerHoodPart.rigidPart.GetComponentInChildren<MeshRenderer>();
                if(turbocharger_hood_renderer == null)
                {
                    turbocharger_hood_renderer = turboChargerHoodPart.activePart.GetComponentInChildren<MeshRenderer>();
                    
                }
                if (turbocharger_hood_renderer.material.name != "CAR_PAINT_REGULAR (Instance)")
                {
                    turbocharger_hood_renderer.material = regularCarPaintMaterial;
                }
                SetPartMaterialColor(turboChargerHoodPart, hoodColor);
                SetPartMaterialColor(turboChargerBigPart, turbochargerBigColor);
                SetPartMaterialColor(turboChargerSmallPart, turbochargerSmallColor);
                SetPartMaterialColor(turboChargerIntercoolerPart, intercoolerColor);
                SetPartMaterialColor(turboChargerManifoldWeberPart, turbochargerManifoldWeberColor);
                SetPartMaterialColor(turboChargerManifoldTwinCarbPart, turbochargerManifoldTwinCarbColor);
            }
            catch
            {

            }
        }

        private void SetPartMaterialColor(Part part, Color colorToPaint)
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

        private void SetBoostGaugeText(float valueToDisplay, bool positive)
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

        private void TriggerBlowOff()
        {
            if (turboBlowOffShot == null)
            {
                CreateTurboBlowoff();

            }
            turbocharger_blowoff.Play();
            if (turboChargerBigPart.installed)
            {
                turboBlowOffShot.volume = 0.20f;
            }
            else if (turboChargerSmallPart.installed)
            {
                turboBlowOffShot.volume = 0.12f;
            }
            timeSinceLastBlowOff = 0;
            turbocharger_blowoffShotAllowed = false;
        }

        private void CalculateAndSetEnginePowerTurbo()
        {
            if(electricityOn == true && allPartsInstalled)
            {
                if (turboChargerBigPart.installed)
                {
                    newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 3000, 100)) * 11;
                    if (engineRPM >= 6200f)
                    {
                        newTurboChargerBar = (newTurboChargerBar - (engineRPM - 6200) / 4500);
                    }
                    if (newTurboChargerBar > 0f)
                    {
                        if (newTurboChargerBar > 1.75f)
                        {
                            newTurboChargerBar = 1.75f;
                        }
                        /*if (n2oBottle.Active)
                        {
                            _enginePowerMultiplier.Value = (0.92f + (newTurboChargerBar) * 2) + (n2oBottlePSI.Value / 3200f); //Not working
                        }
                        else
                        */
                        _enginePowerMultiplier.Value = (0.92f + (newTurboChargerBar) * 2);

                        SetBoostGaugeText(newTurboChargerBar, true);
                    }
                    else
                    {
                        SetBoostGaugeText(0.92f, false);
                        _enginePowerMultiplier.Value = 0.92f;
                    }
                }
                else if (turboChargerSmallPart.installed)
                {
                    newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 1600, 10)) * 2;
                    if (engineRPM >= 6200f)
                    {
                        newTurboChargerBar = (newTurboChargerBar - (engineRPM - 6200) / 4700);
                    }
                    if (newTurboChargerBar > 0f)
                    {
                        if (newTurboChargerBar > 1.12f)
                        {
                            newTurboChargerBar = 1.12f;
                        }
                        /*if (n2oBottle.Active)
                        {
                            _enginePowerMultiplier.Value = (0.92f + (newTurboChargerBar) * 2) + (n2oBottlePSI.Value / 3200f); //Not working
                        }
                        else
                        */
                        _enginePowerMultiplier.Value = (0.96f + (newTurboChargerBar) * 1.4f);

                        SetBoostGaugeText(newTurboChargerBar, true);
                    }
                    else
                    {
                        SetBoostGaugeText(0.92f, false);
                        _enginePowerMultiplier.Value = 0.96f;
                    }
                }
            }
            else
            {
                SetBoostGaugeText(0.00f, false);
                enginePowerMultiplier = 0;

            }
                
           
        }

        public void CreateTurboLoop()
        {
            //Creates the TurboLoop loading the file "turbocharger_loop.wav" from the Asset folder of the mod
            //And setting it to loop

            turboLoop = satsuma.AddComponent<AudioSource>();
            turbocharger_loop.audioSource = turboLoop;
            turbocharger_loop.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "turbocharger_loop.wav"), true, false);

            turboLoop.rolloffMode = AudioRolloffMode.Custom;
            turboLoop.minDistance = 1;
            turboLoop.maxDistance = 10;
            turboLoop.spatialBlend = 1;
            turboLoop.loop = true;
            turbocharger_loop.Play();

        }
        public void CreateTurboBlowoff()
        {
            //Creates the TurboBlowoff  loading the file "turbocharger_blowoff.wav" from the Asset folder of the mod

            turboBlowOffShot = satsuma.AddComponent<AudioSource>();
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
                if(isItemInHand)
                {
                    if (Camera.main != null)
                    {
                        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        Physics.Raycast(ray, out hit);

                        if (hit.collider)
                        {
                            if (hit.collider.transform.name == "Turbocharger Hood" || hit.collider.transform.name == "Turbocharger Hood(Clone)" || hit.collider.transform.name == "Turbocharger Intercooler" || hit.collider.transform.name == "Turbocharger Intercooler(Clone)" || hit.collider.transform.name == "Turbocharger Big" || hit.collider.transform.name == "Turbocharger Big(Clone)" || hit.collider.transform.name == "Turbocharger Small" || hit.collider.transform.name == "Turbocharger Small(Clone)" || hit.collider.transform.name == "Turbocharger Manifold Weber" || hit.collider.transform.name == "Turbocharger Manifold Weber(Clone)" || hit.collider.transform.name == "Turbocharger Manifold TwinCarb" || hit.collider.transform.name == "Turbocharger Manifold TwinCarb(Clone)")
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
                                    if(arrIndex < 0 || arrIndex > 12)
                                    {
                                        arrIndex = 0;
                                    }
                                    
                                   
                                    if (hit.collider.transform.name == "Turbocharger Hood" || hit.collider.transform.name == "Turbocharger Hood(Clone)")
                                    {
                                        hoodColor = pickedUPsprayCanColor;
                                    }
                                    if(hit.collider.transform.name == "Turbocharger Intercooler" || hit.collider.transform.name == "Turbocharger Intercooler(Clone)")
                                    {
                                        intercoolerColor = modSprayColors[arrIndex];
                                    }

                                    if(hit.collider.transform.name == "Turbocharger Big" || hit.collider.transform.name == "Turbocharger Big(Clone)")
                                    {
                                        turbochargerBigColor = modSprayColors[arrIndex];
                                    }

                                    if(hit.collider.transform.name == "Turbocharger Small" || hit.collider.transform.name == "Turbocharger Small(Clone)")
                                    {
                                        turbochargerSmallColor = modSprayColors[arrIndex];
                                    }

                                    if(hit.collider.transform.name == "Turbocharger Manifold Weber" || hit.collider.transform.name == "Turbocharger Manifold Weber(Clone)")
                                    {
                                        turbochargerManifoldWeberColor = modSprayColors[arrIndex];
                                    }

                                    if (hit.collider.transform.name == "Turbocharger Manifold TwinCarb" || hit.collider.transform.name == "Turbocharger Manifold TwinCarb(Clone)")
                                    {
                                        turbochargerManifoldTwinCarbColor = modSprayColors[arrIndex];
                                    }
                                    MeshRenderer partRenderer = hit.collider.GetComponentInChildren<MeshRenderer>();
                                    for(int i = 0; i < partRenderer.materials.Length; i++)
                                    {
                                        if(partRenderer.materials[i].name == "Red_Acent (Instance)" || partRenderer.materials[i].name == "CAR_PAINT_REGULAR (Instance)")
                                        {
                                            partRenderer.materials[i].SetColor("_Color", modSprayColors[arrIndex]);
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
        }

        private static string CalculateSHA1(string filename)
        {
            using (var sha1 = SHA1.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}