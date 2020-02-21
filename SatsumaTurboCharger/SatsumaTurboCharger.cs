using HutongGames.PlayMaker;
using ModApi.Attachable;
using MSCLoader;
using System.IO;
using System.Xml;
using UnityEngine;
namespace SatsumaTurboCharger
{
    public class SatsumaTurboCharger : Mod
    {
        /* Todo:
         * Looks not Possible: more grip
         * DONE: (has to be changed before first entering car/starting up ) toggle awd/rwd/fwd
         * DONE: (no sound is played in 5th and 6th ) fix six gear mod not working
         * make changing turbocharger boost 
         * DONE :Check for steam
         * DONE: Intercooler
         * DONE: (make pretty) gauge
         *  backfire flames
         *  antilag ?
         *  !!!!!!!!add ModsShop
         *  FIX: turbocharger_intercooler__manifold_tube -> see throuth object
         *  Reset partSaves when starting new game
         */

        public override string ID => "SatsumaTurboCharger"; //Your mod ID (unique)
        public override string Name => "SatsumaTurboCharger"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.3.1"; //Version

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
        TextMesh boostGaugeTextMesh;

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
        private float turboChargerRPM = 0;

        //Turbocharger audio sounds
        private AudioSource turboLoop;
        private AudioSource turboBlowOffShot;
        private AudioSource maxTurboLoop;
        private AudioSource engineBackfire;

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
        0.7f,   // 5th
        0.6f    // 6th
        };

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
            newGameStarted = true;
            ModConsole.Print("Test");
        }



        public override void OnLoad()
        {
            if(ModLoader.CheckSteam() == false)
            {
                ModConsole.Warning("You are not running a legit version of 'My Summer Car' from Steam");
                ModConsole.Warning("Please support the developer of the game!");
                ModConsole.Warning("This mod will not work if your version of the game is not legit");
                ModConsole.Warning("Other cause: you started the .exe instead of through steam");
                throw new System.ArgumentException("Game is not legit!!!");
            }
            else
            {
                satsuma = GameObject.Find("SATSUMA(557kg, 248)");
                satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();


                currentGear = satsumaDriveTrain.gear;



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



                turbocharger = GameObject.Instantiate(turbocharger);
                turbocharger_hood = GameObject.Instantiate(turbocharger_hood);
                turbocharger_manifold = GameObject.Instantiate(turbocharger_manifold);
                turbocharger_boost_tube = GameObject.Instantiate(turbocharger_boost_tube);
                turbocharger_exhaust_inlet_tube = GameObject.Instantiate(turbocharger_exhaust_inlet_tube);
                turbocharger_exhaust_outlet_tube = GameObject.Instantiate(turbocharger_exhaust_outlet_tube);
                turbocharger_boost_gauge = GameObject.Instantiate(turbocharger_boost_gauge);

                turbocharger.name = "Turbocharger";
                turbocharger_hood.name = "Turbocharger Hood";
                turbocharger_manifold.name = "Turbocharger Manifold";
                turbocharger_boost_tube.name = "Turbocharger Boost Tube";
                turbocharger_exhaust_inlet_tube.name = "Turbocharger Exhaust Inlet Tube";
                turbocharger_exhaust_outlet_tube.name = "Turbocharger Exhaust Outlet Tube";
                turbocharger_boost_gauge.name = "Turbocharger Boost Gauge";
                turbocharger_intercooler.name = "Turbocharger Intercooler";
                turbocharger_intercooler_manifold_tube.name = "Turbocharger Intercooler-Manifold Tube";

                Trigger turbocharger_hoodTrigger = new Trigger("TurbochargerHoodTrigger", satsuma, new Vector3(0.0f, 0.241f, 1.68f), new Quaternion(0, 0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f), false);
                Trigger turbochargerTrigger = new Trigger("TurbochargerTrigger", satsuma, new Vector3(0.272f, 0.405f, 1.25f), new Quaternion(0, 0, 0, 0), new Vector3(0.2025909f, 0.2292653f, 0.2594887f), false);
                Trigger turbocharger_manifoldTrigger = new Trigger("TurbochargerManifoldTrigger", satsuma, new Vector3(0f, 0.2185f, 1.01f), new Quaternion(0, 0, 0, 0), new Vector3(0.4110837f, 0.09178066f, 0.1377684f), false);
                Trigger turbocharger_boostTubeTrigger = new Trigger("TurbochargerBoostTubeTrigger", satsuma, new Vector3(0.147f, 0.242f, 1.425f), new Quaternion(0, 0, 0, 0), new Vector3(0.2325249f, 0.1078328f, 0.1718995f), false);
                Trigger turbocharger_exhaustInletTubeTrigger = new Trigger("TurbochargerExhaustInletTubeTrigger", satsuma, new Vector3(0.2195f, 0.05f, 1.167f), new Quaternion(0, 0, 0, 0), new Vector3(0.2947979f, 0.380095f, 0.0698716f), false);
                Trigger turbocharger_exhaustOutletTubeTrigger = new Trigger("TurbochargerExhaustOutletTubeTrigger", satsuma, new Vector3(0.272f, 0.405f, 1.3f), new Quaternion(0, 0, 0, 0), new Vector3(0.2696412f, 0.6248907f, 0.1637728f), false);
                Trigger turbocharger_boostGaugeTrigger = new Trigger("TurbochargerBoostGaugeTrigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.5f, -0.04f, 0.125f), new Quaternion(0, 0, 0, 0), new Vector3(0.04952335f, 0.0636534f, 0.04000306f), false);
                Trigger turbocharger_intercoolerTrigger = new Trigger("TurbochargerIntercoolerTrigger", satsuma, new Vector3(-0.098f, 0.305f, 1.295f), new Quaternion(0, 0, 0, 0), new Vector3(0.3751739f, 0.1643179f, 0.3941967f), false);
                Trigger turbocharger_intercoolerManifoldTubeTrigger = new Trigger("TurbochargerIntercoolerManifoldTubeTrigger", satsuma, new Vector3(-0.305f, 0.288f, 1.211f), new Quaternion(0, 0, 0, 0), new Vector3(0.1552045f, 0.1442022f, 0.500939f), false);

                if(newGameStarted == true)
                {
                    turboChargerHoodPart = new TurboChargerHoodPart(
                        null,
                        turbocharger_hood,
                        satsuma,
                        turbocharger_hoodTrigger,
                        new Vector3(0.0f, 0.241f, 1.68f),
                        new Quaternion(0, 180, 0, 0)
                    );
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
                        new Vector3(-0.305f, 0.288f, 1.211f),
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
                        new Vector3(-0.305f, 0.288f, 1.211f),
                        new Quaternion
                        {
                            eulerAngles = new Vector3(0.5f, 180, 0)
                        }
                    );
                }
                

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
                    }
                }
            }

        }



        public override void ModSettings()
        {
            // All settings should be created here. 
            // DO NOT put anything else here that settings.
            Settings.AddButton(this, displayTurboChargerValues, "TurboCharger GUI");
            Settings.AddButton(this, toggleBackFire, "Toggle Exhaust Backfire");
            Settings.AddHeader(this, "", Color.clear);
            Settings.AddCheckBox(this, toggleAWD);
            Settings.AddHeader(this, "", Color.clear);
        }

        private static void SwitchTurboChargerGui()
        {
            displayTurboChargerValuesOnGui = !displayTurboChargerValuesOnGui;
        }

        private static void ToggleBackFire()
        {
            backFireEnabled = !backFireEnabled;
        }

        public override void OnSave()
        {
            try
            {
                SaveLoad.SerializeSaveFile(this, this.turboChargerPart.getSaveInfo(), turbocharger_SaveFile);
                SaveLoad.SerializeSaveFile(this, this.turboChargerHoodPart.getSaveInfo(), turbocharger_hood_SaveFile);
                SaveLoad.SerializeSaveFile(this, this.turboChargerManifoldPart.getSaveInfo(), turbocharger_manifold_SaveFile);
                SaveLoad.SerializeSaveFile(this, this.turboChargerBoostTubePart.getSaveInfo(), turbocharger_boost_tube_SaveFile);
                SaveLoad.SerializeSaveFile(this, this.turboChargerExhaustInletTubePart.getSaveInfo(), turbocharger_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile(this, this.turboChargerExhaustOutletTubePart.getSaveInfo(), turbocharger_exhaust_outlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile(this, this.turboChargerBoostGaugePart.getSaveInfo(), turbocharger_boost_gauge_SaveFile);

                SaveLoad.SerializeSaveFile(this, this.turboChargerIntercoolerPart.getSaveInfo(), turbocharger_intercooler_SaveFile);
                SaveLoad.SerializeSaveFile(this, this.turboChargerIntercoolerManifoldTubePart.getSaveInfo(), turbocharger_intercooler__manifold_tube_SaveFile);
            }
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[TurboChargerMod]</b> - an error occured while attempting to save part info.. see: " + ex.ToString());
            }
            
            // Called once, when save and quit
            // Serialize your save file here.
            //SaveLoad.SerializeSaveFile(this, this.turboChargerPart.getSaveInfo(), fileName);
            //SaveLoad.SerializeSaveFile(this, this.turboChargerManifoldPart.getSaveInfo(), fileName);
            //SaveLoad.SerializeSaveFile(this, this.turboChargerBoostTubePart.getSaveInfo(), fileName);
            //SaveLoad.SerializeSaveFile(this, this.turboChargerExhaustInletTubePart.getSaveInfo(), fileName);
           // SaveLoad.SerializeSaveFile(this, this.turboChargerExhaustOutletTubePart.getSaveInfo(), fileName);

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
                GUI.Label(new Rect(20, 60, 220, 100), "Turbo Charger RPM: " + ((int)turboChargerRPM).ToString());
                GUI.Label(new Rect(20, 80, 200, 100), "Turbo Charger bar: " + turboChargerBar.ToString("n3"));
                GUI.Label(new Rect(20, 100, 200, 100), "Power Current: " + ((int)enginePowerCurrent).ToString());
                GUI.Label(new Rect(20, 120, 200, 100), "Power Multiplier: " + enginePowerMultiplier.ToString("n2"));
                GUI.Label(new Rect(20, 140, 200, 100), "Final Gear: " + (satsumaDriveTrain.finalDriveRatio));
                GUI.Label(new Rect(20, 160, 200, 100), "km/h: " + ((int)_carSpeed.Value));
                GUI.Label(new Rect(20, 180, 200, 100), "------------------------------------");
            }
            
            
            
        }
        public override void Update()
        {
            GameObject boostGauge;
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
                ModConsole.Print("--> Changing gear ratios to make car usable with turbocharger");
                ModConsole.Print("1: 3.4");
                ModConsole.Print("2: 1.4");
                ModConsole.Print("3: 1.1");
                ModConsole.Print("4: 0.9");
                ModConsole.Print("5: 0.8");
                ModConsole.Print("6: 0.6");
                ModConsole.Print("Final Gear: 3.5");
                satsumaDriveTrain.finalDriveRatio = 3.5f;
                satsumaDriveTrain.gearRatios = newGearRatio;

                gearIncreaseModMessageSend = true;
                satsumaDriveTrain.maxRPM = 8400f;
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
                satsumaDriveTrain.maxPower = 120f; //increases max power engine can produce/use/gets translated into actual "CAR GO FORWARD FASTER THAN YOUR MAX FAST"
                enginePowerMultiplier = satsumaDriveTrain.powerMultiplier; //load the powerMultiplier of the car into a variable to be used later
                enginePowerCurrent = satsumaDriveTrain.currentPower; //load the current power generated of the car into a variable to be used later
                engineMaxPower = satsumaDriveTrain.maxPower; //load the max Power allowed into a variable to be used later
                engineRPM = satsumaDriveTrain.rpm; //load the engine rpm of the car into a variable to be used later
                turboChargerRPM = engineRPM * 16f; //calculate the turbocharger rpm
                


                //If Engine is above 1500rpm the turbocharger "engages" and increases the power multiplier
                //Example power multiplier calculation:
                /* engineRPM = 2200
                 * turboChargerRPM: 2200rpm * 16f = 35.200rpm <-> 6000rpm * 16f = 96.000rpm
                 * turboChargerBar: (turboChargerRPM / 16f) / 10f * 0.002f
                 * -> 35.200rpm / 16f = 2200    //Get Back to engineRPM
                 * 2200 / 10f = 220             //Decrease number 
                 * 220 * 0.002f = 0.44          //calculate good working multiplier number
                 * powerMultiplier = 1 + 0.44 = 1.44 (so 0.44 more Power at 2200rpm) (6000rpm = 1.2 = 2.2 multiplier)
                 * 
                 */


                if(timeSinceLastBlowOff >= 0.3f)
                {
                    if (engineRPM > 1500)
                    {

                        turboChargerBar = ((turboChargerRPM / 16f) / 10f) * 0.004f;
                        if (turboChargerBar > 3.10f)
                        {
                            turboChargerBar = 3.10f;
                        }
                        _enginePowerMultiplier.Value = (1 + turboChargerBar);
                    }
                    else
                    {
                        //Reset PowerMultiplier back to 1 if engineRPM is below 1900rpm -> so no boost produced
                        _enginePowerMultiplier.Value = 1;
                    }

                    if(GameObject.Find("Turbocharger Boost Gauge(Clone)") != null)
                    {
                        if (turboChargerBar > 1)
                        {
                            boostGaugeTextMesh.text = (turboChargerBar).ToString("0.00");
                        }
                        else
                            boostGaugeTextMesh.text = "1.00";
                    }

                }



                //Continous Loop of turbo sound -> if not already exists it will be created and played
                if (turboLoop == null)
                {
                    CreateTurboLoop();
                }
                else if(turboLoop.isPlaying == false)
                    turboLoop.Play();

                //Set Volume and Pitch based on engineRPM -> to make sound go ssssSuuuuuUUUUUUU
                turboLoop.volume = engineRPM * 0.00005f;
                turboLoop.pitch = engineRPM * 0.00018f;

                //If throttleKey is Pressed (W) and engineRPM is above 4000rpm AND one second has past since last blowoff sound
                //set turbocharger_blowoffShotAllowed to true -> a blowoff sound can be fired
                if (throttleKey.IsPressed() && engineRPM > 4000 && timeSinceLastBlowOff > 1)
                {
                    turbocharger_blowoffShotAllowed = true;

                }

                //If ThrottleKey is NOT Pressed (W) and blowoff sound is allowed -> create blowoff sound if not already exists and set turbocharger_blowoffShotAllowed back to false
                if (!throttleKey.IsPressed() && turbocharger_blowoffShotAllowed == true)
                {
                    if (turboBlowOffShot == null)
                    {
                        CreateTurboBlowoff();
                    }
                    turbocharger_blowoff.Play();
                    if(GameObject.Find("Turbocharger Boost Gauge(Clone)") != null)
                    {
                        boostGaugeTextMesh.text = 0.65f.ToString("0.00");
                    }
                    
                    turboBlowOffShot.volume = 0.3f;
                    timeSinceLastBlowOff = 0;
                    turbocharger_blowoffShotAllowed = false;

                }


                //Blowoff Sound when shifting gears above 4000rpm
                if (currentGear != satsumaDriveTrain.gear && engineRPM > 4000)
                {
                    currentGear = satsumaDriveTrain.gear;
                    if (turboBlowOffShot == null)
                    {
                        CreateTurboBlowoff();

                    }
                    turbocharger_blowoff.Play();
                    turboBlowOffShot.volume = 0.3f;
                    timeSinceLastBlowOff = 0;
                    turbocharger_blowoffShotAllowed = false;
                }

                if (backFireEnabled == true)
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
            turboLoop.maxDistance = 50;
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
        }
        public void CreateBackfire()
        {
            engineBackfire = satsuma.AddComponent<AudioSource>();
            engine_backfire.audioSource = engineBackfire;
            engine_backfire.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "engine_backfire.wav"), true, false);
        }

    }
}
