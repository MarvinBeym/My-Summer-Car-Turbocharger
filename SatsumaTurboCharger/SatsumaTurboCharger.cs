using HutongGames.PlayMaker;
using ModApi;
using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using SatsumaTurboCharger.gui;
using SatsumaTurboCharger.old_file_checker;
using SatsumaTurboCharger.painting_system;
using SatsumaTurboCharger.parts;
using SatsumaTurboCharger.shop;
using SatsumaTurboCharger.turbo;
using SatsumaTurboCharger.wear;
using ScrewablePartAPI.V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using static SatsumaTurboCharger.parts.AdvPart;
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
         * Added clear error message when mod can't load the AssetBundle
         * Added logger
         * Fixed some errors that could happen sometimes on launch
         * Improved guiDebug/replaced with reworked one using GUILayout
         * Added back/fixed six gear + ratio replacer
         * Hood can now be opened/closed
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

        public override string ID => "SatsumaTurboCharger";
        public override string Name => "DonnerTechRacing Turbocharger";
        public override string Author => "DonnerPlays";
        public override string Version => "2.2";
        public override bool UseAssetsFolder => true;

        public SaveFileRenamer saveFileRenamer;
        public GuiDebug guiDebug;

        //Kits
        private Kit turboBig_kit;
        private Kit turboSmall_kit;
        private Kit manifoldWeber_kit;
        private Kit manifoldTwinCarb_kit;



        //Saves
        public Dictionary<string, SaveableColor> partsColorSave;
        public Dictionary<string, float> partsWearSave;
        public Dictionary<string, bool> partsBuySave;
        public Dictionary<string, float> boostSave;
        //Files
        private const string logger_saveFile = "turbo_mod.log";
        private const string modsShop_saveFile = "mod_shop_saveFile.json";
        private const string boost_saveFile = "turbocharger_mod_boost_SaveFile.txt";
        private const string wear_saveFile = "wear_saveFile.json";

        private const string screwableV2_saveFile = "screwableV2_saveFile.json";
        private const string color_saveFile = "color_saveFile.json";

        //
        //Install & Spawn Locations
        //

        //Big Turbo
        private Vector3 turboBig_installLocation = new Vector3(-0.067799f, 0.291159f, -0.07805f);                           //DONE                       
        private Vector3 turboBig_intercooler_tube_installLocation = new Vector3(-0.346378f, 0.201461f, 0.25025f);           //DONE
        private Vector3 turboBig_exhaust_inlet_tube_installLocation = new Vector3(-0.234201f, -0.020159f, 0.09884f);        //DONE
        private Vector3 turboBig_exhaust_outlet_tube_installLocation = new Vector3(-0.307337f, 1.044137f, 0.456764f);       //DONE
        private Vector3 turboBig_blowoff_valve_installLocation = new Vector3(0.026583f, 0.139111f, 0.15561f);               //DONE             
        private Vector3 turboBig_exhaust_outlet_straight_installLocation = new Vector3(-0.0185f, 0.0385f, 0.159f);          //DONE          
        private Vector3 turboBig_hood_installLocation = new Vector3(0, 0.2408085f, 1.68f);                                  //DONE                   

        //Small Turbo
        private Vector3 turboSmall_installLocation = new Vector3(-0.25f, -0.1665f, 0.0001f);
        private Vector3 turboSmall_manifold_twinCarb_tube_installLocation = new Vector3(-0.188f, -0.23f, 0.14f);
        private Vector3 turboSmall_intercooler_tube_installLocation = new Vector3(0.316f, -0.041f, 1.518f);
        private Vector3 turboSmall_exhaust_inlet_tube_installLocation = new Vector3(-0.0918f, -0.1774f, -0.094f);
        private Vector3 turboSmall_exhaust_outlet_tube_installLocation = new Vector3(-0.1825f, -0.267f, -0.145f);
        private Vector3 turboSmall_airfilter_installLocation = new Vector3(-0.25f, -0.04f, 0.0001f);

        //Other Parts        
        private Vector3 manifold_weber_installLocation = new Vector3(-0.009812f, -0.11697f, 0.030088f);                     //DONE
        private Vector3 manifold_twinCarb_installLocation = new Vector3(-0.007944f, -0.13087f, -0.0316f);                   //DONE   
        private Vector3 boost_gauge_installLocation = new Vector3(0.5f, -0.04f, 0.125f);                                        
        private Vector3 intercooler_installLocation = new Vector3(0.0f, -0.162f, 1.6775f);                                  //DONE
        private Vector3 intercooler_manifold_weber_tube_installLocation = new Vector3(0.362165f, -0.139072f, -0.23436f);    //DONE             
        private Vector3 intercooler_manifold_twinCarb_tube_installLocation = new Vector3(0.325294f, -0.180286f, -0.29813f); //DONE              
        private Vector3 exhaust_header_installLocation = new Vector3(-0.005f, -0.089f, -0.068809f);                         //DONE                            

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

        //Engine Values
        //private FsmState n2oBottle;
        //private FsmFloat n2oBottlePSI;
        //private CarController satsumaCarController;
        //private Axles satsumaAxles;

        //NOS
        //private FsmState n2oState;
        //private FsmState no_n2oState;
        //private GameObject N2O;
        //private PlayMakerFSM n2oPlayMaker;


        //Mod Settings

        private Settings debugGuiSetting = new Settings("debugGuiSetting", "Show DEBUG GUI", false);
        private Settings resetPosSetting = new Settings("resetPos", "Reset", Helper.WorkAroundAction);
        public Settings partsWearSetting = new Settings("partsWearSetting", "Use parts wear system", true);
        public Settings rotateTurbineSetting = new Settings("rotateTurbineSetting", "Allow turbo turbine rotation", false);
        public Settings backfireEffectSetting = new Settings("backfireEffectSetting", "Allow backfire effect for turbo", false);
        private Settings useCustomGearRatios = new Settings("useCustomGearRatios", "Use custom gear ratios", false, Helper.WorkAroundAction);

        //
        //ModApi Parts
        //
        //Big Turbo
        public AdvPart turboBig_part;
        public AdvPart turboBig_intercooler_tube_part;
        public AdvPart turboBig_exhaust_inlet_tube_part;
        public AdvPart turboBig_exhaust_outlet_tube_part;
        public AdvPart turboBig_blowoff_valve_part;
        public AdvPart turboBig_exhaust_outlet_straight_part;

        public AdvPart turboBig_hood_part;
        public AdvPart exhaust_header_part;

        //Small Turbo
        public AdvPart turboSmall_part;
        public AdvPart turboSmall_intercooler_tube_part;
        public AdvPart turboSmall_exhaust_inlet_tube_part;
        public AdvPart turboSmall_exhaust_outlet_tube_part;
        public AdvPart turboSmall_airfilter_part;
        public AdvPart turboSmall_manifold_twinCarb_tube_part;

        //Other Parts
        public AdvPart manifold_weber_part;
        public AdvPart manifold_twinCarb_part;
        public AdvPart boost_gauge_part;
        public AdvPart intercooler_part;
        public AdvPart intercooler_manifold_weber_tube_part;
        public AdvPart intercooler_manifold_twinCarb_tube_part;

        public List<AdvPart> partsList = new List<AdvPart>();
        public List<AdvPart> bigPartsList;
        public List<AdvPart> smallPartsList;
        public List<AdvPart> otherPartsList;

        //Turbo
        private Turbo racingTurbo;
        private Turbo gtTurbo;

        //Wear
        private Wear racingTurboWear;
        private Wear intercoolerWear;
        private Wear gtTurboWear;
        private Wear gtTurboAirfilterWear;

        //PaintSystem
        private PaintSystem turboBigPaintSystem;
        private PaintSystem turboBigHoodPaintSystem;
        private PaintSystem intercoolerPaintSystem;
        private PaintSystem turboSmallAirfilterPaintSystem;
        //Logic
        private GT_Turbocharger_Logic turboSmall_logic;



        //ECU-Mod Communication
        private bool ecuModInstalled = false;

        //Everything Else
        private float[] originalGearRatios = new float[]
        {
            -4.093f,
            0f,
            3.673f,
            2.217f,
            1.448f,
            1f,
        };
        private float[] newGearRatios = new float[]
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
        public AssetBundle assetsBundle;
        private TextMesh boostGaugeTextMesh;

        public override void OnNewGame()
        {
            partsList.ForEach(delegate (AdvPart part)
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, part.saveFile);
            });
            SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(this, null, modsShop_saveFile);
            SaveLoad.SerializeSaveFile<BoostSave>(this, null, boost_saveFile);
            SaveLoad.SerializeSaveFile<Dictionary<string, float>>(this, null, wear_saveFile);
        }

        public override void OnLoad()
        {
            ModConsole.Print(this.Name + $" [v{this.Version} | Screwable v{ScrewablePartV2.version}] started loading");
            Logger.InitLogger(this, logger_saveFile, 100);

            ecuModInstalled = ModLoader.IsModPresent("DonnerTech_ECU_Mod");
            saveFileRenamer = new SaveFileRenamer(this, 900);

            guiDebug = new GuiDebug(Screen.width - 310, 50, 300, "ECU MOD DEBUG", new GuiDebugElement[] {
                new GuiDebugElement("DEBUG"),
                new GuiDebugElement("Wear"),
            });

            resetPosSetting.DoAction = PosReset;
            useCustomGearRatios.DoAction = new Action(delegate ()
            {
                if ((bool)useCustomGearRatios.Value)
                {
                    satsumaDriveTrain.gearRatios = newGearRatios;
                }
                else
                {
                    satsumaDriveTrain.gearRatios = originalGearRatios;
                }
            });
            assetsBundle = Helper.LoadAssetBundle(this, "turbochargermod.unity3d");

            try
            {
                elect = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                power = PlayMakerFSM.FindFsmOnGameObject(elect, "Power");
                satsuma = GameObject.Find("SATSUMA(557kg, 248)");
                satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
                satsumaDriveTrain.clutchTorqueMultiplier = 10f;

                exhaustFromEngine = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");
                exhaustFromPipe = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");
                exhaustFromMuffler = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");

                originalExhaustPipeRaceParent = exhaustFromPipe.transform.parent;
                originalExhaustPipeRacePosition = new Vector3(exhaustFromPipe.transform.localPosition.x, exhaustFromPipe.transform.localPosition.y, exhaustFromPipe.transform.localPosition.z);
                originalExhaustPipeRaceRotation = new Quaternion(exhaustFromPipe.transform.localRotation.x, exhaustFromPipe.transform.localRotation.y, exhaustFromPipe.transform.localRotation.z, exhaustFromPipe.transform.localRotation.w);

                weberCarb = Helper.GetGameObjectFromFsm(GameObject.Find("Racing Carburators"));
                twinCarb = Helper.GetGameObjectFromFsm(GameObject.Find("Twin Carburators"));

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
                Logger.New("Error while trying to load required game objects/values", "Gameobject.Find has failed to return the desired object", ex);
            }

            

            try
            {
                partsColorSave = Helper.LoadSaveOrReturnNew<Dictionary<string, SaveableColor>>(this, color_saveFile);
                partsWearSave = Helper.LoadSaveOrReturnNew<Dictionary<string, float>>(this, wear_saveFile);
                partsBuySave = Helper.LoadSaveOrReturnNew<Dictionary<string, bool>>(this, modsShop_saveFile);
                boostSave = Helper.LoadSaveOrReturnNew<Dictionary<string, float>>(this, boost_saveFile);
            }
            catch(Exception ex)
            {
                Logger.New("Error while trying to deserialize save file", "Please check paths to save files", ex);
            }

            AdvPartBaseInfo advPartBaseInfo = new AdvPartBaseInfo
            {
                assetBundle = assetsBundle,
                mod = this,
                partsBuySave = partsBuySave
            };

            manifold_weber_part = new AdvPart(advPartBaseInfo,
                "manifold_weber", "Weber Manifold", weberCarb, "",
                manifold_weber_installLocation, new Vector3(90, 0, 0),
                true, "manifold_weber_kit");

            manifold_twinCarb_part = new AdvPart(advPartBaseInfo,
                "manifold_twinCarb", "TwinCarb Manifold", twinCarb, "",
                manifold_twinCarb_installLocation, new Vector3(90, 0, 0),
                true);

            boost_gauge_part = new AdvPart(advPartBaseInfo,
                "boost_gauge", "Boost Gauge", GameObject.Find("dashboard(Clone)"), "",
                boost_gauge_installLocation, new Vector3(90, 0, 0),
                true);

            boostGaugeTextMesh = boost_gauge_part.rigidPart.GetComponentInChildren<TextMesh>();

            intercooler_part = new AdvPart(advPartBaseInfo,
                "intercooler", "Intercooler", satsuma, "",
                intercooler_installLocation, new Vector3(0, 180, 0),
                true);

            intercooler_manifold_weber_tube_part = new AdvPart(advPartBaseInfo,
                "intercooler_manifold_weber_tube", "Weber Intercooler-Manifold Tube", manifold_weber_part.part, "",
                intercooler_manifold_weber_tube_installLocation, new Vector3(0, 0, 0), 
                true, "manifold_weber_kit");

            intercooler_manifold_twinCarb_tube_part = new AdvPart(advPartBaseInfo,
                "intercooler_manifold_twinCarb_tube", "TwinCarb Intercooler-Manifold Tube", manifold_twinCarb_part.part, "",
                intercooler_manifold_twinCarb_tube_installLocation, new Vector3(0, 0, 0), 
                true, "manifold_twinCarb_kit");

            exhaust_header_part = new AdvPart(advPartBaseInfo,
                "exhaust_header", "Turbo Exhaust Header", originalCylinerHead, "",
                exhaust_header_installLocation, new Vector3(90, 0, 0),
                true);

            turboBig_exhaust_inlet_tube_part = new AdvPart(advPartBaseInfo,
                "turboBig_exhaust_inlet_tube", "Racing Turbo Exhaust Inlet Tube", exhaust_header_part.part, "",
                turboBig_exhaust_inlet_tube_installLocation, new Vector3(0, 0, 0),
                true, "turboBig_kit");

            turboBig_exhaust_outlet_tube_part = new AdvPart(advPartBaseInfo,
                "turboBig_exhaust_outlet_tube", "Racing Turbo Exhaust Outlet Tube", GameObject.Find("racing exhaust(Clone)"), "",
                turboBig_exhaust_outlet_tube_installLocation, new Vector3(90, 0, 0),
                true, "turboBig_kit");


            turboBig_part = new AdvPart(advPartBaseInfo,
                "turboBig", "Racing Turbo", turboBig_exhaust_inlet_tube_part.part, "",
                turboBig_installLocation, new Vector3(0, 0, 0), 
                true, "turboBig_kit");

            turboBig_exhaust_outlet_straight_part = new AdvPart(advPartBaseInfo,
                "turboBig_exhaust_outlet_straight", "Racing Turbo Exhaust Straight", turboBig_part.part, "",
                turboBig_exhaust_outlet_straight_installLocation, new Vector3(0, 0, 0),
                true);

            turboBig_intercooler_tube_part = new AdvPart(advPartBaseInfo,
                "turboBig_intercooler_tube", "Racing Turbo Intercooler Tube", intercooler_part.part, "",
                turboBig_intercooler_tube_installLocation, new Vector3(0, 0, 0),
                true, "turboBig_kit");

            turboBig_blowoff_valve_part = new AdvPart(advPartBaseInfo,
                "turboBig_blowoff_valve", "Racing Turbo Blowoff Valve", turboBig_intercooler_tube_part.part, "",
                turboBig_blowoff_valve_installLocation, new Vector3(0, 0, 45),
                true);

            turboBig_hood_part = new AdvPart(advPartBaseInfo,
                "turboBig_hood", "Racing Turbo Hood", satsuma, "",
                turboBig_hood_installLocation, new Vector3(0, 180, 0),
                true);

            Hood_Logic hoodLogic = turboBig_hood_part.rigidPart.AddComponent<Hood_Logic>();
            turboBig_hood_part.AddOnAssembleAction(hoodLogic.OnTurboHoodAssemble);
            turboBig_hood_part.AddOnDisassembleAction(hoodLogic.OnTurboHoodDisassemble);
            hoodLogic.Init(turboBig_hood_part);

       
            
        turboSmall_part = new AdvPart(advPartBaseInfo,
                "turboSmall", "GT Turbo", originalCylinerHead, "",
                turboSmall_installLocation, new Vector3(90, 0, 0),
                true, "turboSmall_kit");
            turboSmall_logic = turboSmall_part.rigidPart.AddComponent<GT_Turbocharger_Logic>();
            turboSmall_logic.Init(this);

            turboSmall_intercooler_tube_part = new AdvPart(advPartBaseInfo,
                "turboSmall_intercooler_tube", "GT Turbo Intercooler Tube", satsuma, "",
                turboSmall_intercooler_tube_installLocation, new Vector3(0, 180, 0),
                true, "turboSmall_kit");

            turboSmall_manifold_twinCarb_tube_part = new AdvPart(advPartBaseInfo,
                "turboSmall_manifold_twinCarb_tube", "GT Turbo Manifold TwinCarb Tube", originalCylinerHead, "",
                turboSmall_manifold_twinCarb_tube_installLocation, new Vector3(90, 0, 0),
                true);

            turboSmall_airfilter_part = new AdvPart(advPartBaseInfo,
                "turboSmall_airfilter", "GT Turbo Airfilter", originalCylinerHead, "",
                turboSmall_airfilter_installLocation, new Vector3(90, 0, 0),
                true);

            turboSmall_exhaust_inlet_tube_part = new AdvPart(advPartBaseInfo,
                "turboSmall_exhaust_inlet_tube", "GT Turbo Exhaust Inlet Tube", originalCylinerHead, "",
                turboSmall_exhaust_inlet_tube_installLocation, new Vector3(90, 0, 0),
                true, "turboSmall_kit");

            turboSmall_exhaust_outlet_tube_part = new AdvPart(advPartBaseInfo,
                "turboSmall_exhaust_outlet_tube", "GT Turbo Exhaust Outlet Tube", originalCylinerHead, "",
                turboSmall_exhaust_outlet_tube_installLocation, new Vector3(90, 0, 0),
                true, "turboSmall_kit");

            bigPartsList = new List<AdvPart>
            {
                turboBig_part,
                turboBig_intercooler_tube_part,
                turboBig_exhaust_inlet_tube_part,
                turboBig_exhaust_outlet_tube_part,
                turboBig_blowoff_valve_part,
                turboBig_exhaust_outlet_straight_part,
            };

            smallPartsList = new List<AdvPart>
            {
                turboSmall_part,
                turboSmall_intercooler_tube_part,
                turboSmall_exhaust_inlet_tube_part,
                turboSmall_exhaust_outlet_tube_part,
                turboSmall_airfilter_part,
                turboSmall_manifold_twinCarb_tube_part,
            };

            otherPartsList = new List<AdvPart>
            {
                manifold_weber_part,
                manifold_twinCarb_part,
                boost_gauge_part,
                intercooler_part,
                intercooler_manifold_weber_tube_part,
                intercooler_manifold_twinCarb_tube_part,
                turboBig_hood_part,
                exhaust_header_part,
            };

            partsList.AddRange(bigPartsList);
            partsList.AddRange(smallPartsList);
            partsList.AddRange(otherPartsList);

            SetupScrewable();

            GameObject turboBig_kitBox = GameObject.Instantiate((assetsBundle.LoadAsset("turboBig_box.prefab") as GameObject));
            GameObject turboSmall_kitBox = GameObject.Instantiate((assetsBundle.LoadAsset("turboBig_box.prefab") as GameObject)); //TEMP CHANGE!!!! prefab
            GameObject manifoldWeber_kitBox = GameObject.Instantiate((assetsBundle.LoadAsset("turboBig_box.prefab") as GameObject));
            GameObject manifoldTwinCarb_kitBox = GameObject.Instantiate((assetsBundle.LoadAsset("turboBig_box.prefab") as GameObject));

            Helper.SetObjectNameTagLayer(turboBig_kitBox, "Racing Turbocharger Kit(Clone)");
            Helper.SetObjectNameTagLayer(turboSmall_kitBox, "GT Turbocharger Kit(Clone)");
            Helper.SetObjectNameTagLayer(manifoldWeber_kitBox, "Weber Kit(Clone)");
            Helper.SetObjectNameTagLayer(manifoldTwinCarb_kitBox, "TwinCarb Kit(Clone)");

            turboBig_kit = new Kit(this, turboBig_kitBox,
                new AdvPart[]{
                    turboBig_part,
                    turboBig_intercooler_tube_part,
                    turboBig_exhaust_inlet_tube_part,
                    turboBig_exhaust_outlet_tube_part,
                });
            turboSmall_kit = new Kit(this, turboSmall_kitBox,
                new AdvPart[]{
                    turboSmall_part,
                    turboSmall_exhaust_inlet_tube_part,
                    turboSmall_exhaust_outlet_tube_part,
                    turboSmall_intercooler_tube_part,
                });

            manifoldWeber_kit = new Kit(this, manifoldWeber_kitBox,
                new AdvPart[]{
                    manifold_weber_part,
                    intercooler_manifold_weber_tube_part
                });
            manifoldTwinCarb_kit = new Kit(this, manifoldTwinCarb_kitBox,
                new AdvPart[]
                {
                    manifold_twinCarb_part,
                    intercooler_manifold_twinCarb_tube_part
                });

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

            racingTurbo = new Turbo(this, turboBig_part, boostSave, "turbocharger_loop.wav", "grinding sound.wav", "turbocharger_blowoff.wav",
                new bool[]
                {
                    true, 
                    false,
                    true
                }, racingTurboConfig, turboBig_blowoff_valve_part.rigidPart.transform.FindChild("blowoff_valve").gameObject);

            racingTurbo.turbine = turboBig_part.rigidPart.transform.FindChild("compressor_turbine").gameObject;
            racingTurbo.backfire_Logic = turboBig_exhaust_outlet_straight_part.rigidPart.AddComponent<Backfire_Logic>();

            racingTurboWear = new Wear(this, "racingTurbo", turboBig_part, new List<WearCondition>
                {
                    new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                    new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                    new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                    new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                    new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
                }, 0.003f, 0.5f, partsWearSave, 4000, "repair_turbocharger_big_ProductImage.png", 100);
            /*intercoolerWear = new Wear(this, "intercooler", intercooler_part, new List<WearCondition>
                {
                    new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                    new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                    new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                    new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                    new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
                }, 0.005f, 0.5f, partsWearSave, 1500, "repair_intercooler_ProductImage.png", 100);*/

            gtTurboWear = new Wear(this, "gtTurbo", turboSmall_part, new List<WearCondition>
                {
                    new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                    new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                    new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                    new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                    new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
                }, 0.003f, 0.5f, partsWearSave, 2500, "repair_turbocharger_small_ProductImage.png", 100);

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



            turboBigPaintSystem = new PaintSystem(partsColorSave, turboBig_part, new Color(0.8f, 0.8f, 0.8f), new string[] { "TurboCharger_Big_Compressor_Turbine", "TurboCharger_Big_Exhaust_Turbine" });
            turboBigHoodPaintSystem = new PaintSystem(partsColorSave, turboBig_hood_part, new Color(0.8f, 0.8f, 0.8f));
            //intercoolerPaintSystem = new PaintSystem(partsColorSave, intercooler_part, new Color(0.8f, 0.8f, 0.8f));
            //turboSmallAirfilterPaintSystem = new PaintSystem(partsColorSave, turboSmall_airfilter_part, new Color(0.8f, 0.8f, 0.8f));
            //-> Issue with component. Paintable components have to be gameobjects on their own
            //-> Another way would be that paintability is defined by a material name on the object (like "paintable_material")


            racingTurbo.wears = new Wear[]
            {
                racingTurboWear,
                intercoolerWear,
            };

            Dictionary<string, Condition> racingTurbo_conditions = new Dictionary<string, Condition>();
            racingTurbo_conditions["weberCarb"] = new Condition("weberCarb", 0.5f);
            racingTurbo_conditions["twinCarb"] = new Condition("twinCarb", 0.2f);
            racingTurbo.SetConditions(racingTurbo_conditions);

            gtTurbo = new Turbo(this, turboSmall_part, boostSave, "turbocharger_loop.wav", "grinding sound.wav", null,
                new bool[]
                {
                    false,
                    true,
                    true
                }, gtTurboConfig, turboSmall_part.rigidPart.transform.FindChild("blowoff_valve").gameObject);

            SetupInspectionPrevention();
            assetsBundle.Unload(false);
            ModConsole.Print(this.Name + $" [v{this.Version} | Screwable v{ScrewablePartV2.version}] finished loading");         
        }
        
        private void SetupScrewable()
        {
            string screwSavePath = Path.Combine(ModLoader.GetModConfigFolder(this), screwableV2_saveFile);
            ScrewablePartV2BaseInfo baseInfo = new ScrewablePartV2BaseInfo(screwSavePath, true);
            //Big turbo

            Helper.ScrewablePartV2Simple(baseInfo, turboBig_intercooler_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.0655f, -0.225f, -0.274f), new Vector3(-90, 0, 0), 0.4f, 5),
               });
            Helper.ScrewablePartV2Simple(baseInfo, turboBig_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.069f, -0.09f, -0.093f), new Vector3(0, -90, 0), 0.5f, 8),
                });
            Helper.ScrewablePartV2Simple(baseInfo, turboBig_exhaust_inlet_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.205f, -0.055f, -0.05f), new Vector3(-90, 0, 0), 0.6f, 7),
                    new ScrewV2(new Vector3(0.262f, -0.055f, -0.05f), new Vector3(-90, 0, 0), 0.6f, 7),
                });
            Helper.ScrewablePartV2Simple(baseInfo, turboBig_exhaust_outlet_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0f, 0.206f, -0.055f), new Vector3(0, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(-0.042f, 0.164f, -0.055f), new Vector3(0, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(0f, 0.122f, -0.055f), new Vector3(0, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(0.041f, 0.164f, -0.055f), new Vector3(0, 0, 0), 0.6f, 6),

                });

            Helper.ScrewablePartV2Simple(baseInfo, turboBig_exhaust_outlet_straight_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.6f, 6),
                });

            Helper.ScrewablePartV2Simple(baseInfo, turboBig_blowoff_valve_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.016f, -0.04f, 0.008f), new Vector3(0, 0, 0), 0.3f, 5),
                });

            Helper.ScrewablePartV2Simple(baseInfo, turboBig_hood_part,
                new ScrewV2[]
                {
                    new ScrewV2(new Vector3(0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(-0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0), 0.6f, 6),
                    new ScrewV2(new Vector3(-0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0), 0.6f, 6),
                });


            //Small turbo
            Helper.ScrewablePartV2Simple(baseInfo, turboSmall_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.0715f, -0.024f, 0.044f), new Vector3(180f, 0f, 0f), 0.4f, 10),
                });
            Helper.ScrewablePartV2Simple(baseInfo, turboSmall_intercooler_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.034f, -0.13f, -0.1638f), new Vector3(180f, 0f, 0f), 0.4f, 10),
                    new ScrewV2(new Vector3(0.014f, 0.24f, 0.332f), new Vector3(0f, -90f, 0f), 0.4f, 10),
                });
            Helper.ScrewablePartV2Simple(baseInfo, turboSmall_exhaust_inlet_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.114f, -0.044f, -0.035f), new Vector3(-90f, 0f, 0f), 0.7f, 10),
                    new ScrewV2(new Vector3(0.06f, -0.044f, -0.044f), new Vector3(-90f, 0f, 0f), 0.7f, 10),
                });
            Helper.ScrewablePartV2Simple(baseInfo, turboSmall_exhaust_outlet_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(-0.078f, 0.1708f, -0.0235f), new Vector3(0, -90, 0), 0.5f, 10),
                });
            Helper.ScrewablePartV2Simple(baseInfo, turboSmall_manifold_twinCarb_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(-0.097f, -0.07f, -0.135f), new Vector3(0, 90, 0), 0.4f, 10),
                });

            Helper.ScrewablePartV2Simple(baseInfo, turboSmall_airfilter_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.0095f, 0.025f, 0.0488f), new Vector3(0, 90, 0), 0.4f, 10),
                });

            //Other parts
            Helper.ScrewablePartV2Simple(baseInfo, exhaust_header_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(0.169f, 0.076f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewV2.Type.Nut),
                    new ScrewV2(new Vector3(0.13f, 0.0296f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewV2.Type.Nut),
                    new ScrewV2(new Vector3(-0.003f, 0.08f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewV2.Type.Nut),
                    new ScrewV2(new Vector3(-0.137f, 0.0296f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewV2.Type.Nut),
                    new ScrewV2(new Vector3(-0.174f, 0.076f, -0.022f), new Vector3(0, 0, 0), 0.7f, 8, ScrewV2.Type.Nut),
                });
            Helper.ScrewablePartV2Simple(baseInfo, intercooler_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(-0.245f, 0.081f, 0.039f), new Vector3(180, 0, 0), 0.6f, 10),
                    new ScrewV2(new Vector3(0.265f, 0.081f, 0.039f), new Vector3(180, 0, 0), 0.6f, 10),
                });
            Helper.ScrewablePartV2Simple(baseInfo, intercooler_manifold_weber_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(-0.0675f, -0.243f, -0.479f), new Vector3(-90, 0, 0), 0.4f, 5),
                    new ScrewV2(new Vector3(0-0.152f, 0.254f, 0.213f), new Vector3(0, 0, 0), 0.4f, 5),
                });
            Helper.ScrewablePartV2Simple(baseInfo, intercooler_manifold_twinCarb_tube_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(-0.0425f, -0.1205f, -0.241f), new Vector3(180, 0, 0), 0.4f, 10),
                });

            Helper.ScrewablePartV2Simple(baseInfo, manifold_weber_part,
                new ScrewV2[]{
                    new ScrewV2(new Vector3(-0.09f, 0, 0), new Vector3(0, 0, 0), 0.5f, 5),
                    new ScrewV2(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.5f, 5),
                    new ScrewV2(new Vector3(0.09f, 0, 0), new Vector3(0, 0, 0), 0.5f, 5),
                });
            Helper.ScrewablePartV2Simple(baseInfo, manifold_twinCarb_part,
                new ScrewV2[] {
                    new ScrewV2(new Vector3(-0.003f, 0.105f, 0.0305f), new Vector3(0, 90, 0), 0.5f, 10),
                });

            //Clamps
            turboBig_part.screwablePart.AddClampModel(new Vector3(0.079f, -0.1f, -0.06f), new Vector3(-90, 0, 0), new Vector3(0.85f, 0.85f, 0.85f));

            turboBig_blowoff_valve_part.screwablePart.AddClampModel(new Vector3(0f, -0.04f, 0f), new Vector3(90, 90, 0), new Vector3(0.43f, 0.43f, 0.43f));

            turboBig_intercooler_tube_part.screwablePart.AddClampModel(new Vector3(0.056f, -0.235f, -0.2475f), new Vector3(0, 90, -90), new Vector3(0.68f, 0.68f, 0.68f));

            turboSmall_airfilter_part.screwablePart.AddClampModel(new Vector3(0f, 0f, 0.049f), new Vector3(0, 0, 0), new Vector3(0.65f, 0.65f, 0.65f));
            turboSmall_intercooler_tube_part.screwablePart.AddClampModel(new Vector3(0.034f, -0.154f, -0.1548f), new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
            turboSmall_intercooler_tube_part.screwablePart.AddClampModel(new Vector3(0.0225f, 0.24f, 0.313f), new Vector3(90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
            turboSmall_part.screwablePart.AddClampModel(new Vector3(0.0715f, -0.043f, 0.052f), new Vector3(0, 90, 0), new Vector3(0.5f, 0.5f, 0.5f));
            turboSmall_exhaust_outlet_tube_part.screwablePart.AddClampModel(new Vector3(-0.068f, 0.1445f, -0.0235f), new Vector3(0, 0, 0), new Vector3(0.67f, 0.67f, 0.67f));
            turboSmall_manifold_twinCarb_tube_part.screwablePart.AddClampModel(new Vector3(-0.106f, -0.07f, -0.116f), new Vector3(-90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));

            intercooler_manifold_weber_tube_part.screwablePart.AddClampModel(new Vector3(-0.053f, -0.2475f, -0.362f), new Vector3(0, 90, -90), new Vector3(0.65f, 0.65f, 0.65f));
            intercooler_manifold_weber_tube_part.screwablePart.AddClampModel(new Vector3(-0.133f, 0.232f, 0.2955f), new Vector3(0, 90, 0), new Vector3(0.65f, 0.65f, 0.65f));
            
            intercooler_manifold_twinCarb_tube_part.screwablePart.AddClampModel(new Vector3(-0.042f, -0.1465f, -0.232f), new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
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
            Settings.AddButton(this, resetPosSetting, "Reset uninstalled part location");
            Settings.AddHeader(this, "Settings");
            Settings.AddCheckBox(this, rotateTurbineSetting);
            Settings.AddCheckBox(this, backfireEffectSetting);
            Settings.AddCheckBox(this, partsWearSetting);
            Settings.AddCheckBox(this, useCustomGearRatios);
            Settings.AddHeader(this, "", Color.clear);
            Settings.AddText(this, "New Gear ratios\n" +
                "1.Gear: " + newGearRatios[2] + "\n" +
                "2.Gear: " + newGearRatios[3] + "\n" +
                "3.Gear: " + newGearRatios[4] + "\n" +
                "4.Gear: " + newGearRatios[5] + "\n" +
                "5.Gear: " + newGearRatios[6] + "\n" +
                "6.Gear: " + newGearRatios[7]
                );
        }
        public override void OnSave()
        {
            turboBig_kit.CheckUnpackedOnSave();
            turboSmall_kit.CheckUnpackedOnSave();
            manifoldWeber_kit.CheckUnpackedOnSave();
            manifoldTwinCarb_kit.CheckUnpackedOnSave();

            try
            {
                partsList.ForEach(delegate (AdvPart part)
                {
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, part.getSaveInfo(), part.saveFile);
                });
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save part", "", ex);
            }

            partsColorSave = turboBigPaintSystem.GetColor(partsColorSave);
            partsColorSave = turboBigHoodPaintSystem.GetColor(partsColorSave);
            //partsColorSave = intercoolerPaintSystem.GetColor(partsColorSave);
            //partsColorSave = turboSmallAirfilterPaintSystem.GetColor(partsColorSave);
            SaveLoad.SerializeSaveFile<Dictionary<string, SaveableColor>>(this, partsColorSave, color_saveFile);


            try
            {
                foreach(AdvPart part in partsList)
                {
                    partsBuySave = part.GetBought(partsBuySave);
                }

                SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(this, partsBuySave, modsShop_saveFile);

                boostSave = racingTurbo.GetBoost(boostSave);
                boostSave = gtTurbo.GetBoost(boostSave);

                SaveLoad.SerializeSaveFile<Dictionary<string, float>>(this, boostSave, boost_saveFile);

                partsWearSave = racingTurboWear.GetWear(partsWearSave);
                partsWearSave = intercoolerWear.GetWear(partsWearSave);

                SaveLoad.SerializeSaveFile<Dictionary<string, float>>(this, partsWearSave, wear_saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save save file", "", ex);
            }

            
            try
            {
                ScrewablePartV2.SaveScrews(this, Helper.GetScrewablePartsArrayFromPartsList(partsList), screwableV2_saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save screws ", $"save file: {screwableV2_saveFile}", ex);
            }
        }
        public override void OnGUI()
        {
            saveFileRenamer.GuiHandler();
            if((bool)debugGuiSetting.Value)
            {
                guiDebug.Handle(new GuiDebugInfo[] {
                new GuiDebugInfo("Wear", "Racing Turbo", racingTurboWear.ToStringOrEmpty()),
                new GuiDebugInfo("Wear", "GT Turbo", gtTurboWear.ToStringOrEmpty()),
                new GuiDebugInfo("Wear", "Intercooler", intercoolerWear.ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "Engine RPM", ((int) satsumaDriveTrain.rpm).ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "Racing Turbo bar", racingTurbo.boost.ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "GT Turbo bar", gtTurbo.boost.ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "Power multiplier", racingTurbo.powerMultiplier.Value.ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "KM/H", ((int) satsumaDriveTrain.differentialSpeed).ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "Torque", satsumaDriveTrain.torque.ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "Clutch Max Torque", satsumaDriveTrain.clutchMaxTorque.ToStringOrEmpty()),
                new GuiDebugInfo("DEBUG", "Clutch Torque Multiplier", satsumaDriveTrain.clutchTorqueMultiplier.ToStringOrEmpty()),
            });
            }

        }

        public bool turboErr = false;
        public override void Update()
        {
            bool allBig = AllBigInstalled();
            bool allSmall = AllSmallInstalled();
            bool allOther = AllOtherInstalled();

            racingTurbo.Handle(allBig, allSmall, allOther);
            racingTurbo.UpdateCondition("weberCarb", weberCarb_inst.Value);
            racingTurbo.UpdateCondition("twinCarb", twinCarb_inst.Value);

            //Todo
            if (!racingTurbo.CheckAllRequiredInstalled() && !gtTurbo.CheckAllRequiredInstalled())
            {
                if (hasPower)
                {
                    turboErr = true;
                    SetBoostGaugeText("ERR");
                }
                else
                {
                    turboErr = false;
                    SetBoostGaugeText("");
                }
            }
            else if (turboErr)
            {
                turboErr = false;
                SetBoostGaugeText("");
            }
            HandleExhaustSystem();
            //HandlePartsTrigger();
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
                intercooler_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(false);
                manifold_twinCarb_part.partTrigger.triggerGameObject.SetActive(false);
                turboSmall_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(false);

                if (intercooler_manifold_twinCarb_tube_part.installed) { intercooler_manifold_twinCarb_tube_part.removePart(); }
                if (manifold_twinCarb_part.installed) { manifold_twinCarb_part.removePart(); }
                if (turboSmall_manifold_twinCarb_tube_part.installed) { turboSmall_manifold_twinCarb_tube_part.removePart(); }

            }
            else
            {
                intercooler_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(true);
                manifold_twinCarb_part.partTrigger.triggerGameObject.SetActive(true);
                turboSmall_manifold_twinCarb_tube_part.partTrigger.triggerGameObject.SetActive(true);
            }

            if (twinCarb_inst.Value)
            {
                manifold_weber_part.partTrigger.triggerGameObject.SetActive(false);
                intercooler_manifold_weber_tube_part.partTrigger.triggerGameObject.SetActive(false);

                if (manifold_weber_part.installed) { manifold_weber_part.removePart(); }
                if (intercooler_manifold_weber_tube_part.installed) { intercooler_manifold_weber_tube_part.removePart(); }
            }
            else
            {
                manifold_weber_part.partTrigger.triggerGameObject.SetActive(true);
                intercooler_manifold_weber_tube_part.partTrigger.triggerGameObject.SetActive(true);
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

            if (engineRunning)
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

        private void SetModsShop()
        {
            if (GameObject.Find("Shop for mods") != null)
            {
                modsShop = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();
                List<ProductInformation> shopItems = new List<ProductInformation>
                {
                    new ProductInformation(turboBig_kit, "Racing Turbocharger Kit", 8100, null),
                    new ProductInformation(turboSmall_kit, "GT Turbocharger Kit", 4150, null),
                    new ProductInformation(turboBig_exhaust_outlet_straight_part, "Racing Turbocharger Straight Exhaust", 1000, null),
                    new ProductInformation(turboBig_blowoff_valve_part, "Racing Turbocharger Blowoff Valve", 1350, null),
                    new ProductInformation(turboBig_hood_part, "Racing Turbocharger Hood", 1800, null),
                    new ProductInformation(turboSmall_manifold_twinCarb_tube_part, "GT Turbocharger TwinCarb Tube", 300, null),
                    new ProductInformation(turboSmall_airfilter_part, "GT Turbocharger Airfilter", 800, null),
                    new ProductInformation(manifoldTwinCarb_kit, "TwinCarb Manifold Kit", 1950, null),
                    new ProductInformation(manifoldWeber_kit, "Weber Manifold Kit", 2250, null),
                    new ProductInformation(intercooler_part, "Intercooler", 3500, null),
                    new ProductInformation(boost_gauge_part, "Boost Gauge", 180, null),
                    new ProductInformation(exhaust_header_part, "Turbocharger Exhaust Header", 2100, null),
                };
                Shop shop = new Shop(this, modsShop, assetsBundle, shopItems);
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

        public void SetBoostGaugeText(string valueToDisplay)
        {
            if (!boost_gauge_part.InstalledScrewed() )
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

        private void PosReset()
        {
            try
            {
                foreach (AdvPart advPart in partsList)
                {
                    if (!advPart.installed)
                    {
                        advPart.activePart.transform.position = advPart.defaultPartSaveInfo.position;
                    }
                }
            }
            catch(Exception ex)
            {
                ModConsole.Error(ex.Message);
            }
            
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
                    (intercooler_manifold_weber_tube_part.InstalledScrewed(ignoreScrewable) || intercooler_manifold_twinCarb_tube_part.InstalledScrewed(ignoreScrewable))
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