using HutongGames.PlayMaker;
using ModApi.Attachable;
using ModApi;
using MSCLoader;
using System;
using System.Globalization;
using System.IO;
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
         *  DONE: Intercooler
         *  DONE: (make pretty) gauge
         *  DONE: limited boost to 1.65bar
         *  DONE: turbocharger_intercooler__manifold_tube -> see throuth object
         *  DONE: Reset partSaves when starting new game
         *  DONE: prevent turboboost in Neutral
         *  backfire flames ?
         *  Draw power grapth/turbo grapth ?
         *  antilag
         *  add ModsShop
         *  make changing turbocharger boost 
         *  DONE: Better calculation of turbocharger boost
         *  hole in hood for intercooler
         *  try and make parts stronger -> can detect when damage happens
         *  check for race carb installed
         *  check for air fuel mixture and decrease/increase boost
         *  prevent inspection with turbo installed
         *  add blowoff valve as model which then can be configured using mouse wheel
         *  EXTERNAL MOD: ABS, ESP, TCS AWD -> maybe as external mod with their own models
         *  make n2o usable
         *  DONE: add position reset button
         *  increase fuel consumption when turbo is used/installed
         *  -------------------------------------------------
         */

        /*  -> next one Changelog v1.7:
         *  -------------------------------------------------
         *  changed text displayed on modapi v130 detection as @Spysi changed his mod and removed modapi v130
         *  changed parent of manifold,turbo (small, big) and exhausts parts to be the cylinder head. (parts will now be installed to the engine itself making it wobble with the engine and when engine is removed parts stay)
         *  changed installation trigger position to center of engine
         *  changed installation trigger area size to space inside engine bay
         *  changed multiple parts install locations
         *  changed turbocharger intercooler model
         *  changed turbocharger intercooler position to be outside at the front bumper
         *  changed connection tubes to connect new intercooler
         *  added Six Gears enable/disable to ModSettings (not recommended, not well driveable
         *  changed Mod Settings order
         *  removed backfire
         *  removed backfire toogle button in Mod Settings
         *  added ModConsole output of mod version loaded.
         *  -------------------------------------------------
         *  
         *  Known bugs
         *  -------------------------------------------------
         *  Use Key still has to be "F" for Painting
         *  Error pops up when starting new game (won't produce problems it just can't reset the Hood color).
         *  tube that goes from turbo to intercooler can always be installed
         *  
         */

        //Material[] tttt = Resources.FindObjectsOfTypeAll<Material>(); //Get all Materials of game
        //Component sprayCanPlayMakerFSM = tests.Value.GetComponentInChildren<PlayMakerFSM>(); // get PlayMaker Component from GameObject
        public override string ID => "SatsumaTurboCharger"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing Turbocharger"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.7"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;

        private GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;

        private AssetBundle assets;
        private static GameObject turbocharger_big = new GameObject();
        private static GameObject turbocharger_big_intercooler_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_outlet_tube = new GameObject();

        private static GameObject turbocharger_small = new GameObject();
        private static GameObject turbocharger_small_intercooler_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_outlet_tube = new GameObject();

        private static bool sixGearsEnabled = false;
        private static GameObject turbocharger_hood = new GameObject();
        private static GameObject turbocharger_manifold = new GameObject();
        private static GameObject turbocharger_boost_gauge = new GameObject();
        private static GameObject turbocharger_intercooler = new GameObject();
        private static GameObject turbocharger_intercooler_manifold_tube = new GameObject();
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


        private Trigger turbocharger_manifoldTrigger;
        private Trigger turbocharger_hoodTrigger;
        private Trigger turbocharger_boostGaugeTrigger;
        private Trigger turbocharger_intercoolerTrigger;
        private Trigger turbocharger_intercoolerManifoldTubeTrigger;

        private GameObject satsumaRaceCarb;
        //private FsmState n2oBottle;
        //private FsmFloat n2oBottlePSI;
        private CarController carController;
        private GameObject elect;
        private PlayMakerFSM power;
        private MeshRenderer[] sprayCansMeshRenders;
        private Material regularCarPaintMaterial;
        private MeshRenderer turbocharger_hood_renderer;
        private Color pickedUPsprayCanColor;

        private ModAudio turbocharger_loop = new ModAudio();
        private ModAudio turbocharger_blowoff = new ModAudio();

        private Settings displayTurboChargerValues = new Settings("toggleTurboChargerValuesButton", "Enable/Disable", SwitchTurboChargerGui);
        //private Settings toggleAWD = new Settings("toggleAWD", "Enable All Wheel Drive - Set BEFORE first entering the car or it wont work!", false);
        private Settings toggleAWD = new Settings("toggleAWD", "Toggle All Wheel Drive - Set before first entering car", false, ToggleAWD);
        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", new Action(SatsumaTurboCharger.PosReset));
        private Settings toggleSixGears = new Settings("toggleSixGears", "Enable/Disable", new Action(SatsumaTurboCharger.ToggleSixGears));


        //Car values
        private float engineRPM = 0;
        private float enginePowerMultiplier;
        private float enginePowerCurrent;
        private float engineMaxPower;
        private FsmFloat _enginePowerMultiplier;
        private FsmFloat _carSpeed;
        private Keybind throttleKey = new Keybind("throttle", "Throttle Key", KeyCode.W);
        private float newTurboChargerBar = 0;

        private string hoodColorFilePath;
        private Color loadedHoodColor;
        private bool isItemInHand;
        //Turbocharger audio sounds
        private AudioSource turboLoop;
        private AudioSource turboBlowOffShot;

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
        private static TurboChargerManifoldPart turboChargerManifoldPart;
        private static TurboChargerBoostGaugePart turboChargerBoostGaugePart;
        private static TurboChargerIntercoolerPart turboChargerIntercoolerPart;
        private static TurboChargerIntercoolerManifoldTubePart turboChargerIntercoolerManifoldTubePart;

        private static bool displayTurboChargerValuesOnGui = false;
        private static bool backFireEnabled = false;
        private bool allPartsInstalled = false;
        private int currentGear = 0;
        private bool newGameStarted = false;
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
        
        private const string turbocharger_manifold_SaveFile = "turbocharger_manifold_partSave.txt";
        private const string turbocharger_boost_gauge_SaveFile = "turbocharger_boost_gauge_partSave.txt";
        private const string turbocharger_intercooler_SaveFile = "turbocharger_intercooler_partSave.txt";
        private const string turbocharger_intercooler_manifold_tube_SaveFile = "turbocharger_intercooler_manifold_tube_partSave.txt";

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
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_boost_gauge_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_manifold_tube_SaveFile);
            WriteTurbochargerHoodColorSave(true);
        }



        public override void OnLoad()
        {
            
            //Component sprayCanPlayMakerFSM = tests.Value.GetComponentInChildren<PlayMakerFSM>(); // get PlayMaker Component from GameObject

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

                    satsumaRaceCarb = GameObject.Find("racing carburators(Clone)");

                    //n2oBottle = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmStates[4];
                    //n2oBottlePSI = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmVariables.FloatVariables[4];

                    originalGearRatios = satsumaDriveTrain.gearRatios;
                    satsumaDriveTrain.revLimiter = true;
                    carController = satsuma.GetComponent<CarController>();
                    //Axles test = satsuma.GetComponent<Axles>();
                    //currentGear = satsumaDriveTrain.gear;

                    //satsumaDriveTrain.gearRatios = newGearRatio;
                    

                    satsumaDriveTrain.clutchTorqueMultiplier = 10f;
                    hoodColorFilePath = ModLoader.GetModConfigFolder(this) + "\\turbocharger_hood_turbocharger_hood_ColorSave.xml";
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
                    SatsumaTurboCharger.turbocharger_big_intercooler_tube = (assets.LoadAsset("turbocharger_big_intercooler_tube_v2.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_big_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_inlet_tube.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_big_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_outlet_tube.prefab") as GameObject);

                    SatsumaTurboCharger.turbocharger_small = (assets.LoadAsset("turbocharger_small.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_small_intercooler_tube = (assets.LoadAsset("turbocharger_small_intercooler_tube_v2.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_small_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_inlet_tube.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_small_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_outlet_tube.prefab") as GameObject);

                    SatsumaTurboCharger.turbocharger_hood = (assets.LoadAsset("turbocharger_hood.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_manifold = (assets.LoadAsset("turbocharger_boost_manifold.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_intercooler = (assets.LoadAsset("turbocharger_intercooler_v2.prefab") as GameObject);
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube = (assets.LoadAsset("turbocharger_tube_intercooler_manifold_v2.prefab") as GameObject);
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
                    SatsumaTurboCharger.turbocharger_manifold.name = "Turbocharger Manifold";
                    SatsumaTurboCharger.turbocharger_intercooler.name = "Turbocharger Intercooler";
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube.name = "Turbocharger Intercooler-Manifold Tube";
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
                    SatsumaTurboCharger.turbocharger_manifold.tag = "PART";
                    SatsumaTurboCharger.turbocharger_intercooler.tag = "PART";
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube.tag = "PART";
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
                    SatsumaTurboCharger.turbocharger_manifold.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_intercooler.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_intercooler_manifold_tube.layer = LayerMask.NameToLayer("Parts");
                    SatsumaTurboCharger.turbocharger_boost_gauge.layer = LayerMask.NameToLayer("Parts");

                    GameObject originalCylinerHead = GameObject.Find("cylinder head(Clone)");

                    turbocharger_big_Trigger = new Trigger("TurbochargerBigTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_big_intercoolerTubeTrigger = new Trigger("TurbochargerBigIntercoolerTubeTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_big_exhaustInletTubeTrigger = new Trigger("TurbochargerBigExhaustInletTubeTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_big_exhaustOutletTubeTrigger = new Trigger("TurbochargerBigExhaustOutletTubeTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);

                    turbocharger_small_Trigger = new Trigger("TurbochargerSmallTrigger", originalCylinerHead, new Vector3(0.272f, 0.405f, 1.25f), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_small_intercoolerTubeTrigger = new Trigger("TurbochargerSmallIntercoolerTubeTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_small_exhaustInletTubeTrigger = new Trigger("TurbochargerSmallExhaustInletTubeTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_small_exhaustOutletTubeTrigger = new Trigger("TurbochargerSmallExhaustOutletTubeTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);

                    turbocharger_hoodTrigger = new Trigger("TurbochargerHoodTrigger", satsuma, new Vector3(0.0f, 0.241f, 1.68f), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_manifoldTrigger = new Trigger("TurbochargerManifoldTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_boostGaugeTrigger = new Trigger("TurbochargerBoostGaugeTrigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.5f, -0.04f, 0.125f), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_intercoolerTrigger = new Trigger("TurbochargerIntercoolerTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);
                    turbocharger_intercoolerManifoldTubeTrigger = new Trigger("TurbochargerIntercoolerManifoldTubeTrigger", originalCylinerHead, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.8f, 0.8f, 0.7f), false);

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
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_manifold);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_intercooler);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_intercooler_manifold_tube);
                    UnityEngine.Object.Destroy(SatsumaTurboCharger.turbocharger_boost_gauge);


                    foreach (var playMakerFloatVar in PlayMakerGlobals.Instance.Variables.FloatVariables)
                    {
                        switch (playMakerFloatVar.Name)
                        {
                            case "EnginePowerMultiplier":
                                _enginePowerMultiplier = playMakerFloatVar;
                                break;
                            case "SpeedKMH":
                                _carSpeed = playMakerFloatVar;
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

                    ModConsole.Print("DonnerTechRacing Turbocharger Mod [ v" + this.Version + "]" + "loaded");
                }
                
            }
        }

        public override void ModSettings()
        {
            // All settings should be created here. 
            // DO NOT put anything else here that settings.
            Settings.AddButton(this, displayTurboChargerValues, "DEBUG TurboCharger GUI");
            Settings.AddButton(this, toggleSixGears, "enable Six Gears");
            Settings.AddHeader(this, "", Color.clear);
            Settings.AddButton(this, toggleAWD);
            Settings.AddHeader(this, "", Color.clear);
            Settings.AddButton(this, resetPosSetting, "reset part location");
            Settings.AddHeader(this, "", Color.clear);

            Settings.AddText(this, "New Gear ratios + 5th & 6th gear\n" +
                "1.Gear: " + newGearRatio[2] + "\n" + 
                "2.Gear: " + newGearRatio[3] + "\n" +
                "3.Gear: " + newGearRatio[4] + "\n" +
                "4.Gear: " + newGearRatio[5] + "\n" +
                "5.Gear: " + newGearRatio[6] + "\n" +
                "6.Gear: " + newGearRatio[7]
                );
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
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerManifoldPart.getSaveInfo(), turbocharger_manifold_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerBoostGaugePart.getSaveInfo(), turbocharger_boost_gauge_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerIntercoolerPart.getSaveInfo(), turbocharger_intercooler_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, SatsumaTurboCharger.turboChargerIntercoolerManifoldTubePart.getSaveInfo(), turbocharger_intercooler_manifold_tube_SaveFile);

                // Needs fixing -> change to using selfwritten Object
                WriteTurbochargerHoodColorSave(false);
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
            if (ModLoader.CheckSteam() == true && errorDetected == false)
            {

                electricityOn = power.FsmVariables.FindFsmBool("ElectricsOK").Value;
                SetTurbochargerHoodColorMaterial();
                DetectPaintingHood();
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
                    turboChargerManifoldPart.installed
                    && turboChargerIntercoolerPart.installed
                    && turboChargerIntercoolerManifoldTubePart.installed
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
                    engineMaxPower = satsumaDriveTrain.maxPower; //load the max Power allowed into a variable to be used later
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



            if(!turboChargerBigExhaustInletTubePart.installed || !turboChargerBigExhaustOutletTubePart.installed || !turboChargerBigIntercoolerTubePart.installed)
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


            if (turboChargerManifoldPart.installed)
            {
                turbocharger_intercoolerManifoldTubeTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_intercoolerManifoldTubeTrigger.triggerGameObject.SetActive(false);
            }


            if(turboChargerManifoldPart.installed && turboChargerIntercoolerManifoldTubePart.installed)
            {
                turbocharger_intercoolerTrigger.triggerGameObject.SetActive(true);

            }
            else
            {
                turbocharger_intercoolerTrigger.triggerGameObject.SetActive(false);
            }
            Component[] t = GameObject.Find("fiberglass hood(Clone)").GetComponentsInParent<Component>();
            /*if(GameObject.Find("fiberglass hood(Clone)"). != null)
            {
                //fiberglass hood installed
                SatsumaTurboCharger.turboChargerIntercoolerPart.rigidPart.GetComponent<BoxCollider>().enabled = false;
                SatsumaTurboCharger.turboChargerIntercoolerManifoldTubePart.rigidPart.GetComponent<BoxCollider>().enabled = false;
                SatsumaTurboCharger.turboChargerIntercoolerManifoldTubePart.rigidPart.GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                SatsumaTurboCharger.turboChargerIntercoolerPart.rigidPart.GetComponent<BoxCollider>().enabled = true;
                SatsumaTurboCharger.turboChargerIntercoolerManifoldTubePart.rigidPart.GetComponent<BoxCollider>().enabled = true;
                SatsumaTurboCharger.turboChargerIntercoolerManifoldTubePart.rigidPart.GetComponent<BoxCollider>().enabled = true;
            }
            */



        }


        private static void SwitchTurboChargerGui()
        {
            displayTurboChargerValuesOnGui = !displayTurboChargerValuesOnGui;
        }

        private static void ToggleBackFire()
        {
            backFireEnabled = !backFireEnabled;
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
            PartSaveInfo turbocharger_manifold_SaveInfo = null;
            PartSaveInfo turbocharger_boost_gauge_SaveInfo = null;
            PartSaveInfo turbocharger_intercooler_SaveInfo = null;
            PartSaveInfo turbocharger__intercooler_manifold_tube_SaveInfo = null;

            turbocharger_big_SaveInfo = this.loadSaveData(turbocharger_big_SaveFile);
            turbocharger_big_intercooler_tube_SaveInfo = this.loadSaveData(turbocharger_big_intercooler_tube_SaveFile);
            turbocharger_big_exhaust_inlet_tube_SaveInfo = this.loadSaveData(turbocharger_big_exhaust_inlet_tube_SaveFile);
            turbocharger_big_exhaust_outlet_tube_SaveInfo = this.loadSaveData(turbocharger_big_exhaust_outlet_tube_SaveFile);

            turbocharger_small_SaveInfo = this.loadSaveData(turbocharger_small_SaveFile);
            turbocharger_small_intercooler_tube_SaveInfo = this.loadSaveData(turbocharger_small_intercooler_tube_SaveFile);
            turbocharger_small_exhaust_inlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_inlet_tube_SaveFile);
            turbocharger_small_exhaust_outlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_outlet_tube_SaveFile);


            turbocharger_hood_SaveInfo = this.loadSaveData(turbocharger_hood_SaveFile);
            turbocharger_manifold_SaveInfo = this.loadSaveData(turbocharger_manifold_SaveFile);
            turbocharger_boost_gauge_SaveInfo = this.loadSaveData(turbocharger_boost_gauge_SaveFile);
            turbocharger_intercooler_SaveInfo = this.loadSaveData(turbocharger_intercooler_SaveFile);
            turbocharger__intercooler_manifold_tube_SaveInfo = this.loadSaveData(turbocharger_intercooler_manifold_tube_SaveFile);

            GameObject originalCylinerHead = GameObject.Find("cylinder head(Clone)");


            turboChargerBigPart = new TurboChargerBigPart(
                turbocharger_big_SaveInfo,
                turbocharger_big,
                originalCylinerHead,
                turbocharger_big_Trigger,
                new Vector3(-0.275f, -0.062f, 0.288f),
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
                new Vector3(0.32f, -0.044f, 1.52f),
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
                new Vector3(-0.262f, -0.1705f, -0.072f),
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
                new Vector3(-0.274f, -0.0125f, 0.288f),
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
                new Vector3(-0.254f, -0.162f, 0.01f),
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
                new Vector3(0.32f, -0.04f, 1.522f),
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
                new Vector3(-0.136f, -0.217f, -0.137f),
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
                new Vector3(-0.184f, -0.272f, -0.15f),
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
                new Vector3(0.0f, 0.241f, 1.68f),
                new Quaternion(0, 180, 0, 0)
            );
            LoadTurbochargerHoodColorSave();

            turboChargerManifoldPart = new TurboChargerManifoldPart(
                turbocharger_manifold_SaveInfo,
                turbocharger_manifold,
                originalCylinerHead,
                turbocharger_manifoldTrigger,
                new Vector3(0f, -0.3f, 0.1f),
                new Quaternion
                {
                    eulerAngles = new Vector3(60, 0, 0)
                }
            );
            
            turboChargerBoostGaugePart = new TurboChargerBoostGaugePart(
                turbocharger_boost_gauge_SaveInfo,
                turbocharger_boost_gauge,
                GameObject.Find("dashboard(Clone)"),
                turbocharger_boostGaugeTrigger,
                new Vector3(0.5f, -0.04f, 0.125f),
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
               new Vector3(0.0f, -0.162f, 1.686f),
               new Quaternion
               {
                   eulerAngles = new Vector3(15, 0, 0)
               }
               //old: 0.03f, -0.015f, 0.142f
           );
            turboChargerIntercoolerManifoldTubePart = new TurboChargerIntercoolerManifoldTubePart(
                turbocharger__intercooler_manifold_tube_SaveInfo,
                turbocharger_intercooler_manifold_tube,
                satsuma,
                turbocharger_intercoolerManifoldTubeTrigger,
                new Vector3(-0.33f, -0.047f, 1.445f),
                new Quaternion
                {
                    eulerAngles = new Vector3(-50, 180, 0)
                }
            );
        }

        private void LoadTurbochargerHoodColorSave()
        {
            if (File.Exists(hoodColorFilePath))
            {
                XmlReader xmlReader = XmlReader.Create(hoodColorFilePath);
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "color"))
                    {
                        if (xmlReader.HasAttributes)
                        {

                            string r = xmlReader.GetAttribute("r");
                            string g = xmlReader.GetAttribute("g");
                            string b = xmlReader.GetAttribute("b");
                            string a = xmlReader.GetAttribute("a");


                            float rFloat = 5f;
                            float gFloat = 5f;
                            float bFloat = 5f;
                            float aFloat = 5f;

                            float.TryParse(r, out rFloat);
                            float.TryParse(g, out gFloat);
                            float.TryParse(b, out bFloat);
                            float.TryParse(a, out aFloat);
                            loadedHoodColor = new Color(rFloat, gFloat, bFloat, aFloat);
                            pickedUPsprayCanColor = loadedHoodColor;
                        }
                        else
                        {
                            loadedHoodColor = new Color(0.800f, 0.800f, 0.800f, 1.000f);
                            pickedUPsprayCanColor = loadedHoodColor;
                        }
                    }
                }
            }
            else
            {
                loadedHoodColor = new Color(0.800f, 0.800f, 0.800f, 1.000f);
                pickedUPsprayCanColor = loadedHoodColor;
            }
        }

        private void WriteTurbochargerHoodColorSave(bool newGame)
        {
            XmlWriter xmlWriter = XmlWriter.Create(hoodColorFilePath);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("color");
            if(newGame != true)
            {
                if (turbocharger_hood_renderer.material.color.Equals(regularCarPaintMaterial.color))
                {
                    xmlWriter.WriteAttributeString("r", 0.800f.ToString());
                    xmlWriter.WriteAttributeString("g", 0.800f.ToString());
                    xmlWriter.WriteAttributeString("b", 0.800f.ToString());
                    xmlWriter.WriteAttributeString("a", 1.000f.ToString());
                }
                else
                {
                    xmlWriter.WriteAttributeString("r", (turbocharger_hood_renderer.material.color.r).ToString());
                    xmlWriter.WriteAttributeString("g", (turbocharger_hood_renderer.material.color.g).ToString());
                    xmlWriter.WriteAttributeString("b", (turbocharger_hood_renderer.material.color.b).ToString());
                    xmlWriter.WriteAttributeString("a", (turbocharger_hood_renderer.material.color.a).ToString());
                }
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
            else
            {
                xmlWriter.WriteAttributeString("r", 0.800f.ToString());
                xmlWriter.WriteAttributeString("g", 0.800f.ToString());
                xmlWriter.WriteAttributeString("b", 0.800f.ToString());
                xmlWriter.WriteAttributeString("a", 1.000f.ToString());
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
            
        }

        private void SetTurbochargerHoodColorMaterial()
        {
            try
            {
                turbocharger_hood_renderer =  turboChargerHoodPart.rigidPart.GetComponentInChildren<MeshRenderer>();
                if(turbocharger_hood_renderer == null)
                {
                    turbocharger_hood_renderer = turboChargerHoodPart.activePart.GetComponentInChildren<MeshRenderer>();
                    if(turbocharger_hood_renderer.material.name != "CAR_PAINT_REGULAR (Instance)")
                    {
                        turbocharger_hood_renderer.material = regularCarPaintMaterial;
                    }
                    if(turbocharger_hood_renderer.material.color != pickedUPsprayCanColor)
                    {
                        turbocharger_hood_renderer.material.color = pickedUPsprayCanColor;
                    }
                }
                else
                {
                    if (turbocharger_hood_renderer.material.name != "CAR_PAINT_REGULAR (Instance)")
                    {
                        turbocharger_hood_renderer.material = regularCarPaintMaterial;
                    }

                    
                    if (turbocharger_hood_renderer.material.color != pickedUPsprayCanColor)
                    {
                        turbocharger_hood_renderer.material.SetColor("_Color", pickedUPsprayCanColor);
                    }

                }
            }
            catch
            {

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

        private static void ToggleAWD()
        {
            if(satsumaDriveTrain.transmission == Drivetrain.Transmissions.AWD)
            {
                satsumaDriveTrain.transmission = Drivetrain.Transmissions.FWD;
            }
            else if(satsumaDriveTrain.transmission == Drivetrain.Transmissions.FWD)
            {
                satsumaDriveTrain.transmission = Drivetrain.Transmissions.AWD;
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


        private void DetectPaintingHood()
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
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    Physics.Raycast(ray, out hit);

                    if (hit.collider)
                    {
                        if (hit.collider.transform.name == "Turbocharger Hood" || hit.collider.transform.name == "Turbocharger Hood(Clone)")
                        {
                            if (Input.GetMouseButton(0))
                            {
                                pickedUPsprayCanColor = sprayCansMeshRenders[1].material.color;
                                turbocharger_hood_renderer.material.color = pickedUPsprayCanColor;
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
            if (!turboChargerManifoldPart.installed)
            {
                turboChargerManifoldPart.activePart.transform.position = turboChargerManifoldPart.defaultPartSaveInfo.position;
            }
            if (!turboChargerBoostGaugePart.installed)
            {
                turboChargerBoostGaugePart.activePart.transform.position = turboChargerBoostGaugePart.defaultPartSaveInfo.position;
            }
            if (!turboChargerIntercoolerPart.installed)
            {

                turboChargerIntercoolerPart.activePart.transform.position = turboChargerIntercoolerPart.defaultPartSaveInfo.position;
            }
            if (!turboChargerIntercoolerManifoldTubePart.installed)
            {
                turboChargerIntercoolerManifoldTubePart.activePart.transform.position = turboChargerIntercoolerManifoldTubePart.defaultPartSaveInfo.position;
            }
        }

        private static void ToggleSixGears()
        {
            if(satsumaDriveTrain.gearRatios == SatsumaTurboCharger.newGearRatio)
            {
                sixGearsEnabled = false;
                satsumaDriveTrain.gearRatios = SatsumaTurboCharger.originalGearRatios;
            }
            else
            {
                sixGearsEnabled = true;
                satsumaDriveTrain.gearRatios = SatsumaTurboCharger.newGearRatio;
            }
        }
    }
}