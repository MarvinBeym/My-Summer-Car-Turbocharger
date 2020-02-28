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
         *  Better calculation of turbocharger boost
         *  hole in hood for intercooler
         *  try and make parts stronger -> can detect when damage happens
         *  check for race carb installed
         *  check for air fuel mixture and decrease/increase boost
         *  prevent inspection with turbo installed
         *  add blowoff valve as model which then can be configured using mouse wheel
         *  NOT POSSIBLE: change parent object (manifold should be mounted on racing carburator) and other 
         *  EXTERNAL MOD: ABS, ESP, TCS AWD -> maybe as external mod with their own models
         *  make n2o usable
         *  -------------------------------------------------
         */

        /*  Changelog v1.5.2:
         *  -------------------------------------------------
         *  fixed Part name not displaying correctly
         *  fixed Throttle Key having to be "W" or else boost sound/display would not work
         *  -------------------------------------------------
         *  
         *  Known bugs
         *  -------------------------------------------------
         *  Use Key still has to be "F" for Painting
         *  
         */

        //Material[] tttt = Resources.FindObjectsOfTypeAll<Material>(); //Get all Materials of game
        //Component sprayCanPlayMakerFSM = tests.Value.GetComponentInChildren<PlayMakerFSM>(); // get PlayMaker Component from GameObject
        public override string ID => "SatsumaTurboCharger"; //Your mod ID (unique)
        public override string Name => "DonnerTech-Racing Turbocharger"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.5.2"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;

        GameObject satsuma;
        Drivetrain satsumaDriveTrain;

        AssetBundle assets;
        GameObject turbocharger = new GameObject();
        GameObject turbocharger_hood = new GameObject();
        GameObject turbocharger_manifold = new GameObject();
        GameObject turbocharger_boost_tube = new GameObject();
        GameObject turbocharger_exhaust_inlet_tube = new GameObject();
        GameObject turbocharger_exhaust_outlet_tube = new GameObject();
        GameObject turbocharger_boost_gauge = new GameObject();
        GameObject turbocharger_intercooler = new GameObject();
        GameObject turbocharger_intercooler_manifold_tube = new GameObject();
        GameObject boostGauge;
        TextMesh boostGaugeTextMesh;

        Trigger turbocharger_hoodTrigger;
        Trigger turbochargerTrigger;
        Trigger turbocharger_manifoldTrigger;
        Trigger turbocharger_boostTubeTrigger;
        Trigger turbocharger_exhaustInletTubeTrigger;
        Trigger turbocharger_exhaustOutletTubeTrigger;
        Trigger turbocharger_boostGaugeTrigger;
        Trigger turbocharger_intercoolerTrigger;
        Trigger turbocharger_intercoolerManifoldTubeTrigger;

        GameObject satsumaRaceCarb;
        FsmState n2oBottle;
        FsmFloat n2oBottlePSI;
        private CarController carController;
        private bool hoodMaterialSet = false;
        private bool hoodMaterialCloneSet = false;
        private GameObject elect;
        private PlayMakerFSM power;
        private MeshRenderer[] sprayCansMeshRenders;
        Material regularCarPaintMaterial;
        MeshRenderer turbocharger_hood_renderer;
        Color pickedUPsprayCanColor;

        ModAudio turbocharger_loop = new ModAudio();
        ModAudio turbocharger_blowoff = new ModAudio();
        ModAudio engine_backfire = new ModAudio();
        Settings displayTurboChargerValues = new Settings("toggleTurboChargerValuesButton", "Enable/Disable", SwitchTurboChargerGui);
        Settings toggleBackFire = new Settings("toggleBackfireButton", "Enable/Disable", ToggleBackFire);
        Settings toggleAWD = new Settings("toggleAWD", "Enable All Wheel Drive - Set BEFORE first entering the car or it wont work!", true);

        //Car values
        private float engineRPM = 0;
        private float enginePowerMultiplier;
        private float enginePowerCurrent;
        private float engineMaxPower;
        private FsmFloat _enginePowerMultiplier;
        private FsmFloat _carSpeed;
        private Keybind throttleKey = new Keybind("throttle", "Throttle Key", KeyCode.W);
        private float turboChargerBar = 0;
        private float newTurboChargerBar = 0;
        private float turboChargerRPM = 0;

        private string hoodColorFilePath;
        private Color loadedHoodColor;
        private bool isItemInHand;
        //Turbocharger audio sounds
        private AudioSource turboLoop;
        private AudioSource turboBlowOffShot;
        private AudioSource engineBackfire;

        private bool electricityOn = false;

        //Turbocharger parts
        private TurboChargerHoodPart turboChargerHoodPart;
        private TurboChargerPart turboChargerPart;
        private TurboChargerManifoldPart turboChargerManifoldPart;
        private TurboChargerBoostTubePart turboChargerBoostTubePart;
        private TurboChargerExhaustInletTubePart turboChargerExhaustInletTubePart;
        private TurboChargerExhaustOutletTubePart turboChargerExhaustOutletTubePart;
        private TurboChargerBoostGaugePart turboChargerBoostGaugePart;
        private TurboChargerIntercoolerPart turboChargerIntercoolerPart;
        private TurboChargerIntercoolerManifoldTubePart turboChargerIntercoolerManifoldTubePart;

        private bool gearIncreaseModMessageSend = false;
        private static bool displayTurboChargerValuesOnGui = false;
        private static bool backFireEnabled = false;
        private bool allPartsInstalled = false;
        private int currentGear = 0;
        private bool newGameStarted = false;
        /* Gear: R = 0
         * Gear: N = 1
         * Gear: 1 = 2
         * Gear: 2 = 3
         * Gear: 3 = 4
         * Gear: 4 = 5
         */
        private bool turbocharger_blowoffShotAllowed = false;
        private float timeSinceLastBlowOff;
        private bool engineBackfiring = false;

        private float[] newGearRatio = new float[]
        {
         // feel free to add more gears here :)
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
        private bool raceCarbInstalled;

        internal static bool useThrottleButton
        {
            get
            {
                return cInput.GetKey("Throttle");
            }
        }


        private const string turbocharger_SaveFile = "turbocharger_partSave.txt";
        private const string turbocharger_hood_SaveFile = "turbocharger_hood_partSave.txt";
        private const string turbocharger_boost_tube_SaveFile = "turbocharger_boost_tube_partSave.txt";
        private const string turbocharger_exhaust_inlet_tube_SaveFile = "turbocharger_exhaust_inlet_tube_partSave.txt";
        private const string turbocharger_exhaust_outlet_tube_SaveFile = "turbocharger_exhaust_outlet_tube_partSave.txt";
        private const string turbocharger_manifold_SaveFile = "turbocharger_manifold_partSave.txt";
        private const string turbocharger_boost_gauge_SaveFile = "turbocharger_boost_gauge_partSave.txt";
        private const string turbocharger_intercooler_SaveFile = "turbocharger_intercooler_partSave.txt";
        private const string turbocharger_intercooler__manifold_tube_SaveFile = "turbocharger_intercooler__manifold_tube_SaveFile.txt";



        private PartSaveInfo loadSaveData(string saveFile)
        {
            // Written, 12.10.2018

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
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_hood_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_boost_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_exhaust_inlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_exhaust_outlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_boost_gauge_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler__manifold_tube_SaveFile);
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

                elect = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                power = PlayMakerFSM.FindFsmOnGameObject(elect, "Power");


                satsuma = GameObject.Find("SATSUMA(557kg, 248)");
                satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
                satsumaRaceCarb = GameObject.Find("racing carburators(Clone)");
                n2oBottle = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmStates[4];
                n2oBottlePSI = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmVariables.FloatVariables[4];
                

                
                satsumaDriveTrain.revLimiter = true;
                carController = satsuma.GetComponent<CarController>();
                Axles test = satsuma.GetComponent<Axles>();
                currentGear = satsumaDriveTrain.gear;


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

                


                //Load Turbocharger Asset from Mod Asset Folder

                assets = LoadAssets.LoadBundle(this, "turbochargermod.unity3d");
                turbocharger = assets.LoadAsset("turbocharger.prefab") as GameObject;
                turbocharger_hood = assets.LoadAsset("turbocharger_hood.prefab") as GameObject;
                turbocharger_manifold = assets.LoadAsset("turbocharger_boost_manifold.prefab") as GameObject;
                turbocharger_boost_tube = assets.LoadAsset("turbocharger_boost_tube.prefab") as GameObject;
                turbocharger_exhaust_inlet_tube = assets.LoadAsset("turbocharger_exhaust_inlet_tube.prefab") as GameObject;
                turbocharger_exhaust_outlet_tube = assets.LoadAsset("turbocharger_exhaust_outlet_tube.prefab") as GameObject;
                turbocharger_boost_gauge = assets.LoadAsset("turbocharger_boost_gauge.prefab") as GameObject;
                turbocharger_intercooler = assets.LoadAsset("turbocharger_intercooler.prefab") as GameObject;
                turbocharger_intercooler_manifold_tube = assets.LoadAsset("turbocharger_tube_intercooler_manifold.prefab") as GameObject;


                turbocharger.name = "Turbocharger";
                turbocharger_hood.name = "Turbocharger Hood";
                turbocharger_manifold.name = "Turbocharger Manifold";
                turbocharger_boost_tube.name = "Turbocharger Boost Tube";
                turbocharger_exhaust_inlet_tube.name = "Turbocharger Exhaust Inlet Tube";
                turbocharger_exhaust_outlet_tube.name = "Turbocharger Exhaust Outlet Tube";
                turbocharger_boost_gauge.name = "Turbocharger Boost Gauge";
                turbocharger_intercooler.name = "Turbocharger Intercooler";
                turbocharger_intercooler_manifold_tube.name = "Turbocharger Intercooler-Manifold Tube";

                turbocharger_hoodTrigger = new Trigger("TurbochargerHoodTrigger", satsuma, new Vector3(0.0f, 0.241f, 1.68f), new Quaternion(0, 0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f), false);
                turbochargerTrigger = new Trigger("TurbochargerTrigger", satsuma, new Vector3(0.272f, 0.405f, 1.25f), new Quaternion(0, 0, 0, 0), new Vector3(0.2025909f, 0.2292653f, 0.2594887f), false);
                turbocharger_manifoldTrigger = new Trigger("TurbochargerManifoldTrigger", satsuma, new Vector3(0f, 0.2185f, 1.01f), new Quaternion(0, 0, 0, 0), new Vector3(0.4110837f, 0.09178066f, 0.1377684f), false);
                turbocharger_boostTubeTrigger = new Trigger("TurbochargerBoostTubeTrigger", satsuma, new Vector3(0.147f, 0.242f, 1.425f), new Quaternion(0, 0, 0, 0), new Vector3(0.2325249f, 0.1078328f, 0.1718995f), false);
                turbocharger_exhaustInletTubeTrigger = new Trigger("TurbochargerExhaustInletTubeTrigger", satsuma, new Vector3(0.2195f, 0.05f, 1.167f), new Quaternion(0, 0, 0, 0), new Vector3(0.2947979f, 0.380095f, 0.0698716f), false);
                turbocharger_exhaustOutletTubeTrigger = new Trigger("TurbochargerExhaustOutletTubeTrigger", satsuma, new Vector3(0.272f, 0.405f, 1.3f), new Quaternion(0, 0, 0, 0), new Vector3(0.2696412f, 0.6248907f, 0.1637728f), false);
                turbocharger_boostGaugeTrigger = new Trigger("TurbochargerBoostGaugeTrigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.5f, -0.04f, 0.125f), new Quaternion(0, 0, 0, 0), new Vector3(0.04952335f, 0.0636534f, 0.04000306f), false);
                turbocharger_intercoolerTrigger = new Trigger("TurbochargerIntercoolerTrigger", satsuma, new Vector3(-0.098f, 0.305f, 1.295f), new Quaternion(0, 0, 0, 0), new Vector3(0.3751739f, 0.1643179f, 0.3941967f), false);
                turbocharger_intercoolerManifoldTubeTrigger = new Trigger("TurbochargerIntercoolerManifoldTubeTrigger", satsuma, new Vector3(-0.245f, 0.282f, 1.214f), new Quaternion(0, 0, 0, 0), new Vector3(0.1552045f, 0.1442022f, 0.500939f), false);
                

                LoadSetAllParts(newGameStarted);



                assets.Unload(false);

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
            }
        }



        public override void ModSettings()
        {
            // All settings should be created here. 
            // DO NOT put anything else here that settings.
            Settings.AddButton(this, displayTurboChargerValues, "DEBUG TurboCharger GUI");
            Settings.AddButton(this, toggleBackFire, "Toggle Exhaust Backfire");
            Settings.AddHeader(this, "", Color.clear);
            Settings.AddCheckBox(this, toggleAWD);
            Settings.AddHeader(this, "", Color.clear);
        }


        public override void OnSave()
        {
            try
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerPart.getSaveInfo(), turbocharger_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerHoodPart.getSaveInfo(), turbocharger_hood_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerManifoldPart.getSaveInfo(), turbocharger_manifold_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerBoostTubePart.getSaveInfo(), turbocharger_boost_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerExhaustInletTubePart.getSaveInfo(), turbocharger_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerExhaustOutletTubePart.getSaveInfo(), turbocharger_exhaust_outlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerBoostGaugePart.getSaveInfo(), turbocharger_boost_gauge_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerIntercoolerPart.getSaveInfo(), turbocharger_intercooler_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, this.turboChargerIntercoolerManifoldTubePart.getSaveInfo(), turbocharger_intercooler__manifold_tube_SaveFile);

                WriteTurbochargerHoodColorSave(false);
            }
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[TurboChargerMod]</b> - an error occured while attempting to save part info.. see: " + ex.ToString());
            }

        }

        public override void OnGUI()
        {
            /*
            GUI.Label(new Rect(20, 20, 200, 100), "Gang R:  " + satsumaDriveTrain.gearRatios[0]);
            GUI.Label(new Rect(20, 40, 200, 100), "Gang 0:  " + satsumaDriveTrain.gearRatios[1]);
            GUI.Label(new Rect(20, 60, 200, 100), "Gang 1:  " + satsumaDriveTrain.gearRatios[2]);
            GUI.Label(new Rect(20, 80, 200, 100), "Gang 2:  " + satsumaDriveTrain.gearRatios[3]);
            GUI.Label(new Rect(20, 100, 200, 100), "Gang 3:  " + satsumaDriveTrain.gearRatios[4]);
            GUI.Label(new Rect(20, 120, 200, 100), "Gang 4:  " + satsumaDriveTrain.gearRatios[5]);
            GUI.Label(new Rect(20, 140, 200, 100), "Gang 5:  " + satsumaDriveTrain.gearRatios[6]);
            GUI.Label(new Rect(20, 160, 200, 100), "Gang 6:  " + satsumaDriveTrain.gearRatios[7]);
            GUI.Label(new Rect(20, 180, 200, 100), "final gear: " + (satsumaDriveTrain.lastGearRatio));
            GUI.Label(new Rect(20, 200, 200, 100), "km/h: " + ((int) _carSpeed.Value));
            */


            // Draw unity OnGUI() here
            // GUI.Label(new Rect(20, 20, 200, 100), "Friction Torque: " + satsumaDriveTrain.frictionTorque);
            // GUI.Label(new Rect(20, 40, 200, 100), "current Gear: " + currentGear);

            //General Data

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
                GUI.Label(new Rect(20, 200, 200, 100), "N2o active: " + n2oBottle.Active);
                GUI.Label(new Rect(20, 220, 200, 100), "Electricity on: " + electricityOn);
                GUI.Label(new Rect(20, 240, 200, 100), "------------------------------------");

                
            }


        }


        public override void FixedUpdate()
        {
            

        }



        public override void Update()
        {
            if (ModLoader.CheckSteam() != false)
            {
                ModConsole.Print(useThrottleButton);
                electricityOn = power.FsmVariables.FindFsmBool("ElectricsOK").Value;
                SetTurbochargerHoodColorMaterial();
                DetectPaintingHood();
                if (satsumaRaceCarb != null && satsumaRaceCarb.transform.parent.parent.name != null)
                {
                    raceCarbInstalled = true;
                }
                else
                    raceCarbInstalled = false;



                if (GameObject.Find("Turbocharger Boost Gauge(Clone)") != null)
                {
                    //Part is installed GameObject changes to Clone Clone
                    boostGauge = GameObject.Find("Turbocharger Boost Gauge(Clone)");
                    boostGaugeTextMesh = boostGauge.GetComponentInChildren<TextMesh>();
                }
                else
                    boostGauge = null;

                if ((bool)toggleAWD.GetValue() == true && satsumaDriveTrain.transmission == Drivetrain.Transmissions.FWD)
                {
                    satsumaDriveTrain.transmission = Drivetrain.Transmissions.AWD;
                }
                else if ((bool)toggleAWD.GetValue() == false && satsumaDriveTrain.transmission == Drivetrain.Transmissions.AWD)
                {
                    satsumaDriveTrain.transmission = Drivetrain.Transmissions.FWD;
                }


                if (gearIncreaseModMessageSend == false)
                {
                    DisplayGearRatioIncreaseInConsole();
                    satsumaDriveTrain.gearRatios = newGearRatio;

                    satsumaDriveTrain.maxRPM = 8400f;
                    gearIncreaseModMessageSend = true;
                }



                if (turboChargerPart.installed && turboChargerManifoldPart.installed
                    && turboChargerBoostTubePart.installed
                    && turboChargerExhaustInletTubePart.installed
                    && turboChargerExhaustOutletTubePart.installed
                    && turboChargerIntercoolerPart.installed
                    && turboChargerIntercoolerManifoldTubePart.installed
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
                                                       //satsumaDriveTrain.maxPower = 120f; //increases max power engine can produce/use/gets translated into actual "CAR GO FORWARD FASTER THAN YOUR MAX FAST"
                                                       //turboChargerRPM = engineRPM * 16f; //calculate the turbocharger rpm


                    if (timeSinceLastBlowOff >= 0.3f)
                    {
                        CalculateAndSetEnginePowerTurbo();
                    }
                    else
                    {
                        newTurboChargerBar = 0.86f;
                        enginePowerMultiplier = 0.86f;
                    }



                    //Continous Loop of turbo sound -> if not already exists it will be created and played
                    if (turboLoop == null)
                    {
                        CreateTurboLoop();
                    }
                    else if (turboLoop.isPlaying == false)
                        turboLoop.Play();

                    //Set Volume and Pitch based on engineRPM -> to make sound go ssssSuuuuuUUUUUUU
                    turboLoop.volume = engineRPM * 0.00005f;
                    turboLoop.pitch = engineRPM * 0.00018f;

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
                        SetBoostGaugeText(0.86f, false);
                        TriggerBlowOff();
                    }




                    if (backFireEnabled == true)
                    {
                        TriggerBackFire();
                    }
                }
            }
        }











        private static void SwitchTurboChargerGui()
        {
            displayTurboChargerValuesOnGui = !displayTurboChargerValuesOnGui;
        }

        private static void ToggleBackFire()
        {
            backFireEnabled = !backFireEnabled;
        }

        private void LoadSetAllParts(bool newGameStarted)
        {
            if (newGameStarted == true)
            {
                turboChargerHoodPart = new TurboChargerHoodPart(
                    null,
                    turbocharger_hood,
                    satsuma,
                    turbocharger_hoodTrigger,
                    new Vector3(0.0f, 0.241f, 1.68f),
                    new Quaternion(0, 180, 0, 0)
                );
                loadedHoodColor = new Color(0.800f, 0.800f, 0.800f, 1.000f);


                turboChargerPart = new TurboChargerPart(
                    null,
                    turbocharger,
                    satsuma,
                    turbochargerTrigger,
                    new Vector3(0.272f, 0.405f, 1.25f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 0, 0)
                    }
                );
                turboChargerManifoldPart = new TurboChargerManifoldPart(
                    null,
                    turbocharger_manifold,
                    satsuma,
                    turbocharger_manifoldTrigger,
                    new Vector3(0f, 0.2185f, 1.01f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(-20, 180, 0)
                    }
                );
                turboChargerBoostTubePart = new TurboChargerBoostTubePart(
                    null,
                    turbocharger_boost_tube,
                    satsuma,
                    turbocharger_boostTubeTrigger,
                    new Vector3(0.147f, 0.242f, 1.425f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 180, 0)
                    }
                );
                turboChargerExhaustInletTubePart = new TurboChargerExhaustInletTubePart(
                    null,
                    turbocharger_exhaust_inlet_tube,
                    satsuma,
                    turbocharger_exhaustInletTubeTrigger,
                    new Vector3(0.2195f, 0.05f, 1.167f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 180, 0)
                    }
                );
                turboChargerExhaustOutletTubePart = new TurboChargerExhaustOutletTubePart(
                    null,
                    turbocharger_exhaust_outlet_tube,
                    satsuma,
                    turbocharger_exhaustOutletTubeTrigger,
                    new Vector3(0.272f, 0.405f, 1.3f),
                    new Quaternion(0f, 180, 0, 0)
                );
                turboChargerBoostGaugePart = new TurboChargerBoostGaugePart(
                    null,
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
                   null,
                   turbocharger_intercooler,
                   satsuma,
                   turbocharger_intercoolerTrigger,
                   new Vector3(-0.098f, 0.305f, 1.295f),
                   new Quaternion
                   {
                       eulerAngles = new Vector3(0, 180, 0)
                   }
               );
                turboChargerIntercoolerManifoldTubePart = new TurboChargerIntercoolerManifoldTubePart(
                    null,
                    turbocharger_intercooler_manifold_tube,
                    satsuma,
                    turbocharger_intercoolerManifoldTubeTrigger,
                    new Vector3(-0.245f, 0.282f, 1.214f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0.5f, 180, 0)
                    }
                );
            }
            else
            {
                turboChargerHoodPart = new TurboChargerHoodPart(
                    this.loadSaveData(turbocharger_hood_SaveFile),
                    turbocharger_hood,
                    satsuma,
                    turbocharger_hoodTrigger,
                    new Vector3(0.0f, 0.241f, 1.68f),
                    new Quaternion(0, 180, 0, 0)
                );
                LoadTurbochargerHoodColorSave();

                turboChargerPart = new TurboChargerPart(
                    this.loadSaveData(turbocharger_SaveFile),
                    turbocharger,
                    satsuma,
                    turbochargerTrigger,
                    new Vector3(0.272f, 0.405f, 1.25f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 0, 0)
                    }
                );
                turboChargerManifoldPart = new TurboChargerManifoldPart(
                    this.loadSaveData(turbocharger_manifold_SaveFile),
                    turbocharger_manifold,
                    satsuma,
                    turbocharger_manifoldTrigger,
                    new Vector3(0f, 0.2185f, 1.01f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(-20, 180, 0)
                    }
                );
                turboChargerBoostTubePart = new TurboChargerBoostTubePart(
                    this.loadSaveData(turbocharger_boost_tube_SaveFile),
                    turbocharger_boost_tube,
                    satsuma,
                    turbocharger_boostTubeTrigger,
                    new Vector3(0.147f, 0.242f, 1.425f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 180, 0)
                    }
                );
                turboChargerExhaustInletTubePart = new TurboChargerExhaustInletTubePart(
                    this.loadSaveData(turbocharger_exhaust_inlet_tube_SaveFile),
                    turbocharger_exhaust_inlet_tube,
                    satsuma,
                    turbocharger_exhaustInletTubeTrigger,
                    new Vector3(0.2195f, 0.05f, 1.167f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 180, 0)
                    }
                );
                turboChargerExhaustOutletTubePart = new TurboChargerExhaustOutletTubePart(
                    this.loadSaveData(turbocharger_exhaust_outlet_tube_SaveFile),
                    turbocharger_exhaust_outlet_tube,
                    satsuma,
                    turbocharger_exhaustOutletTubeTrigger,
                    new Vector3(0.272f, 0.405f, 1.3f),
                    new Quaternion(0f, 180, 0, 0)
                );
                turboChargerBoostGaugePart = new TurboChargerBoostGaugePart(
                    this.loadSaveData(turbocharger_boost_gauge_SaveFile),
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
                   this.loadSaveData(turbocharger_intercooler_SaveFile),
                   turbocharger_intercooler,
                   satsuma,
                   turbocharger_intercoolerTrigger,
                   new Vector3(-0.098f, 0.305f, 1.295f),
                   new Quaternion
                   {
                       eulerAngles = new Vector3(0, 180, 0)
                   }
               );
                turboChargerIntercoolerManifoldTubePart = new TurboChargerIntercoolerManifoldTubePart(
                    this.loadSaveData(turbocharger_intercooler__manifold_tube_SaveFile),
                    turbocharger_intercooler_manifold_tube,
                    satsuma,
                    turbocharger_intercoolerManifoldTubeTrigger,
                    new Vector3(-0.245f, 0.282f, 1.214f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0.5f, 180, 0)
                    }
                );
            }
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
            GameObject turbocharger_hood_gameObject = GameObject.Find("Turbocharger Hood");
            if (turbocharger_hood_gameObject == null)
            {
                turbocharger_hood_renderer = GameObject.Find("Turbocharger Hood(Clone)").GetComponentInChildren<MeshRenderer>();
                if (hoodMaterialCloneSet == false)
                {
                    turbocharger_hood_renderer.material = regularCarPaintMaterial;
                    turbocharger_hood_renderer.material.color = loadedHoodColor;
                    hoodMaterialCloneSet = true;
                }
                turbocharger_hood_renderer.material.color = pickedUPsprayCanColor;
            }
            else
            {
                turbocharger_hood_renderer = GameObject.Find("Turbocharger Hood").GetComponentInChildren<MeshRenderer>();
                if (hoodMaterialSet == false)
                {
                    turbocharger_hood_renderer.material = regularCarPaintMaterial;
                    turbocharger_hood_renderer.material.color = loadedHoodColor;
                    hoodMaterialSet = true;
                }
                turbocharger_hood_renderer.material.color = pickedUPsprayCanColor;
            }
        }

        private void SetBoostGaugeText(float valueToDisplay, bool positive)
        {
            if (GameObject.Find("Turbocharger Boost Gauge(Clone)") != null)
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
            turboBlowOffShot.volume = 0.20f;
            timeSinceLastBlowOff = 0;
            turbocharger_blowoffShotAllowed = false;
        }

        private void CalculateAndSetEnginePowerTurbo()
        {
            if(electricityOn == true)
            {
                newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 3000, 100)) * 10;
                //turboChargerBar = ((turboChargerRPM / 16f) / 10f) * 0.004f;
                if (engineRPM >= 6200f)
                {
                    newTurboChargerBar = (newTurboChargerBar - (engineRPM - 6200) / 4500);
                }

                if (satsumaDriveTrain.gear == 1)
                {
                    satsumaDriveTrain.maxRPM = 7000f;
                    _enginePowerMultiplier.Value = 0.92f;
                    SetBoostGaugeText(0.92f, false);
                }
                else
                {
                    satsumaDriveTrain.canStall = false;
                    satsumaDriveTrain.maxRPM = 8400f;

                    if (newTurboChargerBar > 0f)
                    {
                        if (newTurboChargerBar > 1.65f)
                        {
                            newTurboChargerBar = 1.65f;
                        }
                        if (n2oBottle.Active)
                        {
                            _enginePowerMultiplier.Value = (0.92f + (newTurboChargerBar) * 2) + (n2oBottlePSI.Value / 3200f); //Not working
                        }
                        else
                            _enginePowerMultiplier.Value = (0.92f + (newTurboChargerBar) * 2);

                        SetBoostGaugeText(newTurboChargerBar, true);
                    }
                    else
                    {
                        SetBoostGaugeText(0.92f, false);
                        _enginePowerMultiplier.Value = 0.92f;
                    }
                }
            }
            else
            {
                SetBoostGaugeText(0.00f, false);
                enginePowerMultiplier = 0;

            }
                
           
        }

        private void TriggerBackFire()
        {
            if (satsumaDriveTrain.revLimiterTriggered)
            {

                if (engineBackfire == null)
                {
                    CreateBackfire();
                }
                if (engineBackfiring == false)
                {
                    engine_backfire.Play();
                    engineBackfiring = true;
                }
                engineBackfire.volume = 2.5f;
                engineBackfire.pitch = 1f;
            }
            else
            {
                if (engineBackfiring == true)
                {
                    engineBackfiring = false;
                    engine_backfire.Stop();
                }

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
        public void CreateBackfire()
        {
            engineBackfire = satsuma.AddComponent<AudioSource>();
            engine_backfire.audioSource = engineBackfire;
            engine_backfire.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "engine_backfire.wav"), true, false);
        }

        private void DetectPaintingHood()
        {
            FsmGameObject itemInHand = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("ItemPivot");
            if (itemInHand.Value.GetComponentInChildren<MeshRenderer>() != null)
            {

                if (itemInHand.Value.GetComponentInChildren<MeshRenderer>().name == "spray can(itemx)" && Input.GetKey(KeyCode.F))
                {
                    sprayCansMeshRenders = itemInHand.Value.GetComponentsInChildren<MeshRenderer>();
                    isItemInHand = true;
                }

            }
            else
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);

                if (hit.collider)
                {

                    if (hit.collider.transform.name == "Turbocharger Hood" || hit.collider.transform.name == "Turbocharger Hood(Clone)")
                    {
                        if (Input.GetMouseButton(0) && isItemInHand == true)
                        {

                            pickedUPsprayCanColor = sprayCansMeshRenders[1].material.color;
                            turbocharger_hood_renderer.material.color = pickedUPsprayCanColor;
                        }
                    }

                }
            }
        }

        private void DisplayGearRatioIncreaseInConsole()
        {
            ModConsole.Print("--> Changing gear ratios to make car usable with turbocharger");
            ModConsole.Print("1: " + newGearRatio[2]);
            ModConsole.Print("2: " + newGearRatio[3]);
            ModConsole.Print("3: " + newGearRatio[4]);
            ModConsole.Print("4: " + newGearRatio[5]);
            ModConsole.Print("5: " + newGearRatio[6]);
            ModConsole.Print("6: " + newGearRatio[7]);

        }
    }
}