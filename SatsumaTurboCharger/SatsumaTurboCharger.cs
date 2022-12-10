using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Shopping;
using MscModApi.Tools;
using SatsumaTurboCharger.gui;
using SatsumaTurboCharger.turbo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Tools;
using Tools.gui;
using UnityEngine;
using static SatsumaTurboCharger.BoostGauge_Logic;

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
         * Massive code refactoring
         * Code improvement
         * Code improvement
         * Loading time improvement
         * Performance improvement
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

		public GuiDebug guiDebug;

        //Kits
        private Kit turboBig_kit;
        private Kit turboSmall_kit;
        private Kit manifoldWeber_kit;
        private Kit manifoldTwinCarb_kit;

        //Logics
        public BoostGauge_Logic boostGaugeLogic;

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
		private Vector3
			turboBig_installLocation = new Vector3(-0.300799f, 0.271159f, 0.01994999f); //DONE                       

		private Vector3 turboBig_intercooler_tube_installLocation = new Vector3(-0.346378f, 0.201461f, 0.25025f); //DONE

		private Vector3 turboBig_exhaust_inlet_tube_installLocation =
			new Vector3(-0.234201f, -0.020159f, 0.09884f); //DONE

		private Vector3 turboBig_exhaust_outlet_tube_installLocation =
			new Vector3(-0.307337f, 1.044137f, 0.456764f); //DONE

		private Vector3
			turboBig_blowoff_valve_installLocation = new Vector3(0.026583f, 0.139111f, 0.15561f); //DONE             

		private Vector3
			turboBig_exhaust_outlet_straight_installLocation = new Vector3(-0.0185f, 0.0385f, 0.159f); //DONE          

		private Vector3 turboBig_hood_installLocation = new Vector3(0, 0.2408085f, 1.68f); //DONE                   

        //Small Turbo
        private Vector3 turboSmall_installLocation = new Vector3(-0.25f, -0.1665f, 0.0001f);
        private Vector3 turboSmall_manifold_twinCarb_tube_installLocation = new Vector3(-0.188f, -0.23f, 0.14f);
        private Vector3 turboSmall_intercooler_tube_installLocation = new Vector3(0.316f, -0.041f, 1.518f);
        private Vector3 turboSmall_exhaust_inlet_tube_installLocation = new Vector3(-0.0918f, -0.1774f, -0.094f);
        private Vector3 turboSmall_exhaust_outlet_tube_installLocation = new Vector3(-0.1825f, -0.267f, -0.145f);
        private Vector3 turboSmall_airfilter_installLocation = new Vector3(-0.25f, -0.04f, 0.0001f);

		//Other Parts        
		private Vector3 manifold_weber_installLocation = new Vector3(-0.009812f, -0.12237f, 0.031888f); //DONE
		private Vector3 manifold_twinCarb_installLocation = new Vector3(-0.007944f, -0.13087f, -0.0316f); //DONE   
		private Vector3 boost_gauge_installLocation = new Vector3(0.4f, -0.019f, 0.152f); //DONE 
		private Vector3 intercooler_installLocation = new Vector3(0.0f, -0.162f, 1.6775f); //DONE

		private Vector3 intercooler_manifold_weber_tube_installLocation =
			new Vector3(0.365165f, -0.139772f, -0.23436f); //DONE             

		private Vector3 intercooler_manifold_twinCarb_tube_installLocation =
			new Vector3(0.325294f, -0.180286f, -0.29813f); //DONE              

		private Vector3
			exhaust_header_installLocation =
				new Vector3(-0.005f, -0.089f, -0.068809f); //DONE                            

        //
        //Game Objects
        //
        //Parts
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
		
		internal PartBaseInfo partBaseInfo;

		//
		//ModApi Parts
		//
		//Big Turbo
		public Part turboBig_part;
		public Part turboBig_intercooler_tube_part;
		public Part turboBig_exhaust_inlet_tube_part;
		public Part turboBig_exhaust_outlet_tube_part;
		public Part turboBig_blowoff_valve_part;
		public Part turboBig_exhaust_outlet_straight_part;

		public Part turboBig_hood_part;
		public Part exhaust_header_part;

		//Small Turbo
		public Part turboSmall_part;
		public Part turboSmall_intercooler_tube_part;
		public Part turboSmall_exhaust_inlet_tube_part;
		public Part turboSmall_exhaust_outlet_tube_part;
		public Part turboSmall_airfilter_part;
		public Part turboSmall_manifold_twinCarb_tube_part;

		//Other Parts
		public Part manifold_weber_part;
		public Part manifold_twinCarb_part;
		public Part boost_gauge_part;
		public Part intercooler_part;
		public Part intercooler_manifold_weber_tube_part;
		public Part intercooler_manifold_twinCarb_tube_part;

		internal static List<Part> partsList = new List<Part>();
		public List<Part> bigPartsList;
		public List<Part> smallPartsList;
		public List<Part> otherPartsList;

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
			0f, // neutral
			3.4f, // 1st
			1.8f, // 2nd
			1.4f, // 3rd
			1.0f, // 4th
			0.8f, // 5th
			0.65f // 6th
		};

		public AssetBundle assetsBundle;
		private TextMesh boostGaugeTextMesh;

		public override void OnNewGame()
		{
			MscModApi.MscModApi.NewGameCleanUp(this);
			//SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(this, null, modsShop_saveFile);
			//SaveLoad.SerializeSaveFile<BoostSave>(this, null, boost_saveFile);
			//SaveLoad.SerializeSaveFile<Dictionary<string, float>>(this, null, wear_saveFile);
		}

		public override void OnLoad()
		{
			ModConsole.Print(this.Name + $" [v{this.Version} started loading");

            ecuModInstalled = ModLoader.IsModPresent("DonnerTech_ECU_Mod");
            saveFileRenamer = new SaveFileRenamer(this, 900);

			guiDebug = new GuiDebug(Screen.width - 310, 50, 300, "ECU MOD DEBUG", new GuiDebugElement[]
			{
				new GuiDebugElement("DEBUG"),
				new GuiDebugElement("Wear"),
			});

            resetPosSetting.DoAction = PosReset;
            useCustomGearRatios.DoAction = new Action(delegate ()
            {
                if ((bool)useCustomGearRatios.Value)
                {
                    CarH.drivetrain.gearRatios = newGearRatios;
                }
                else
                {
                    CarH.drivetrain.gearRatios = originalGearRatios;
                }
            });
            assetsBundle = Helper.LoadAssetBundle(this, "turbochargermod.unity3d");

            try
            {
                CarH.drivetrain.clutchTorqueMultiplier = 10f;

				exhaustFromEngine = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");
				exhaustFromPipe = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");
				exhaustFromMuffler = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");

                originalExhaustPipeRaceParent = exhaustFromPipe.transform.parent;
                originalExhaustPipeRacePosition = new Vector3(exhaustFromPipe.transform.localPosition.x, exhaustFromPipe.transform.localPosition.y, exhaustFromPipe.transform.localPosition.z);
                originalExhaustPipeRaceRotation = new Quaternion(exhaustFromPipe.transform.localRotation.x, exhaustFromPipe.transform.localRotation.y, exhaustFromPipe.transform.localRotation.z, exhaustFromPipe.transform.localRotation.w);

				weberCarb = Helper.GetGameObjectFromFsm(Cache.Find("Racing Carburators"));
				twinCarb = Helper.GetGameObjectFromFsm(Cache.Find("Twin Carburators"));

				weberCarb_inst = Cache.Find("Racing Carburators").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");
				twinCarb_inst = Cache.Find("Twin Carburators").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");
				steelHeaders_inst = Cache.Find("Steel Headers").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");
				racingExhaustPipe_inst = Cache.Find("Racing Exhaust").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");
				racingExhaustMuffler_inst = Cache.Find("Racing Muffler").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");

				headers_inst = Cache.Find("Headers").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
				exhaustPipe_inst = Cache.Find("ExhaustPipe").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");
				exhaustMuffler_inst = Cache.Find("ExhaustMuffler").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");
				exhaustMufflerDualTip_inst = Cache.Find("ExhaustDualTip").GetComponent<PlayMakerFSM>().FsmVariables
					.FindFsmBool("Installed");

				steelHeaders = Cache.Find("headers(Clone)");
				racingExhaustPipe = Cache.Find("racing exhaust(Clone)");
				racingExhaustMuffler = Cache.Find("racing muffler(Clone)");

				headers = Cache.Find("steel headers(Clone)");
				exhaustPipe = Cache.Find("exhaust pipe(Clone)");
				exhaustMuffler = Cache.Find("exhaust muffler(Clone)");

				originalCylinerHead = Cache.Find("cylinder head(Clone)");
			}
			catch (Exception ex)
			{
				Logger.New("Error while trying to load required game objects/values",
					"Gameobject.Find has failed to return the desired object", ex);
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

			partBaseInfo = new PartBaseInfo(this, assetsBundle, partsList);
			manifold_weber_part = new Part("manifold_weber", "Weber Manifold", weberCarb,
				manifold_weber_installLocation, new Vector3(90, 0, 0), partBaseInfo);
			manifold_weber_part.AddClampModel(new Vector3(0.1275f, -0.0061f, -0.02f), new Vector3(-20, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			manifold_weber_part.AddClampModel(new Vector3(0.051f, -0.0061f, -0.02f), new Vector3(-20, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			manifold_weber_part.AddClampModel(new Vector3(-0.047f, -0.0061f, -0.02f), new Vector3(-20, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			manifold_weber_part.AddClampModel(new Vector3(-0.1235f, -0.0061f, -0.02f), new Vector3(-20, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			manifold_weber_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0.1275f, -0.0061f, -0.02f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0.1275f, -0.0061f, -0.02f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0.1275f, -0.0061f, -0.02f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0.1275f, -0.0061f, -0.02f), new Vector3(0, 0, 0))
			}, 0.5f);

			manifold_twinCarb_part = new Part("manifold_twinCarb", "TwinCarb Manifold", twinCarb,
				manifold_twinCarb_installLocation, new Vector3(90, 0, 0), partBaseInfo);
			manifold_twinCarb_part.AddClampModel(new Vector3(-0.013f, 0.105f, 0f), new Vector3(90, 0, 0),
				new Vector3(0.8f, 0.8f, 0.8f));
			manifold_twinCarb_part.AddScrew(new Screw(new Vector3(-0.003f, 0.105f, 0.0305f), new Vector3(0, 90, 0),
				Screw.Type.Normal, 0.5f));

			boost_gauge_part = new Part("boost_gauge", "Boost Gauge", Cache.Find("dashboard(Clone)"),
				boost_gauge_installLocation, new Vector3(90, 0, 0), partBaseInfo);
			boost_gauge_part.AddScrew(new Screw(new Vector3(0f, -0.0405f, 0.003f), new Vector3(-90, 0, 0),
				Screw.Type.Normal, 0.4f));

			boostGaugeLogic = boost_gauge_part.AddWhenInstalledBehaviour<BoostGauge_Logic>();
			boostGaugeLogic.Init(boost_gauge_part);

			intercooler_part = new Part("intercooler", "Intercooler", CarH.satsuma, intercooler_installLocation,
				new Vector3(0, 180, 0), partBaseInfo);
			intercooler_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(-0.245f, 0.081f, 0.039f), new Vector3(180, 0, 0)),
				new Screw(new Vector3(0.265f, 0.081f, 0.039f), new Vector3(180, 0, 0)),
			}, 0.6f);

			intercooler_manifold_weber_tube_part = new Part(
				"intercooler_manifold_weber_tube",
				"Weber Intercooler-Manifold Tube",
				manifold_weber_part,
				intercooler_manifold_weber_tube_installLocation,
				new Vector3(0, 0, 0),
				partBaseInfo
			);
			intercooler_manifold_weber_tube_part.AddClampModel(new Vector3(-0.053f, -0.2475f, -0.362f),
				new Vector3(0, 90, -90), new Vector3(0.65f, 0.65f, 0.65f));
			intercooler_manifold_weber_tube_part.AddClampModel(new Vector3(0.143f, 0.232f, 0.2955f),
				new Vector3(0, 90, 0), new Vector3(0.65f, 0.65f, 0.65f));
			intercooler_manifold_weber_tube_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(-0.0675f, -0.243f, -0.479f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0 - 0.152f, 0.254f, 0.213f), new Vector3(0, 0, 0)),
			}, 0.4f);

			intercooler_manifold_twinCarb_tube_part = new Part(
				"intercooler_manifold_twinCarb_tube",
				"TwinCarb Intercooler-Manifold Tube",
				manifold_twinCarb_part,
				intercooler_manifold_twinCarb_tube_installLocation,
				new Vector3(0, 0, 0),
				partBaseInfo
			);
			intercooler_manifold_twinCarb_tube_part.AddClampModel(new Vector3(-0.042f, -0.1465f, -0.232f),
				new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
			intercooler_manifold_twinCarb_tube_part.AddScrew(new Screw(new Vector3(-0.0425f, -0.1205f, -0.241f),
				new Vector3(180, 0, 0), Screw.Type.Normal, 0.4f));

			exhaust_header_part = new Part(
				"exhaust_header",
				"Turbo Exhaust Header",
				originalCylinerHead,
				exhaust_header_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			exhaust_header_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0.169f, 0.076f, -0.022f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(0.13f, 0.0296f, -0.022f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.003f, 0.08f, -0.022f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.137f, 0.0296f, -0.022f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.174f, 0.076f, -0.022f), new Vector3(0, 0, 0), Screw.Type.Nut),
			}, 0.7f);

			turboBig_exhaust_inlet_tube_part = new Part(
				"turboBig_exhaust_inlet_tube",
				"Racing Turbo Exhaust Inlet Tube",
				originalCylinerHead,
				exhaust_header_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			turboBig_exhaust_inlet_tube_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0.205f, -0.055f, -0.05f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.262f, -0.055f, -0.05f), new Vector3(-90, 0, 0)),
			}, 0.6f);

			turboBig_exhaust_outlet_tube_part = new Part(
				"turboBig_exhaust_outlet_tube",
				"Racing Turbo Exhaust Outlet Tube",
				Cache.Find("racing exhaust(Clone)"),
				turboBig_exhaust_outlet_tube_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			turboBig_exhaust_outlet_tube_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0f, 0.206f, -0.055f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(-0.042f, 0.164f, -0.055f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0f, 0.122f, -0.055f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0.041f, 0.164f, -0.055f), new Vector3(0, 0, 0)),
			}, 0.6f);

			turboBig_part = new Part(
				"turboBig",
				"Racing Turbo",
				turboBig_exhaust_inlet_tube_part,
				turboBig_installLocation,
				new Vector3(0, 0, 0),
				partBaseInfo
			);
			turboBig_part.AddClampModel(new Vector3(0.078f, -0.09f, -0.06f), new Vector3(-90, 0, 0),
				new Vector3(0.85f, 0.85f, 0.85f));
			turboBig_part.AddScrew(new Screw(new Vector3(0.069f, -0.09f, -0.093f), new Vector3(0, -90, 0),
				Screw.Type.Normal, 0.5f));


			turboBig_exhaust_outlet_straight_part = new Part(
				"turboBig_exhaust_outlet_straight",
				"Racing Turbo Exhaust Straight",
				turboBig_part,
				turboBig_exhaust_outlet_straight_installLocation,
				new Vector3(0, 0, 0),
				partBaseInfo
			);
			turboBig_exhaust_outlet_straight_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0, 0, 0), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0, 0, 0), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0, 0, 0), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0, 0, 0), new Vector3(0, 0, 0))
			}, 0.6f);

			turboBig_intercooler_tube_part = new Part(
				"turboBig_intercooler_tube",
				"Racing Turbo Intercooler Tube",
				intercooler_part,
				turboBig_intercooler_tube_installLocation,
				new Vector3(0, 0, 0),
				partBaseInfo
			);
			turboBig_intercooler_tube_part.AddClampModel(new Vector3(0.065f, -0.235f, -0.2475f),
				new Vector3(0, 90, -90), new Vector3(0.68f, 0.68f, 0.68f));
			turboBig_intercooler_tube_part.AddScrew(new Screw(new Vector3(0.0655f, -0.225f, -0.274f),
				new Vector3(-90, 0, 0), Screw.Type.Normal, 0.4f));

			turboBig_blowoff_valve_part = new Part(
				"turboBig_blowoff_valve",
				"Racing Turbo Blowoff Valve",
				turboBig_intercooler_tube_part,
				turboBig_blowoff_valve_installLocation,
				new Vector3(0, 0, 45),
				partBaseInfo
			);
			turboBig_blowoff_valve_part.AddClampModel(new Vector3(0f, -0.04f, 0f), new Vector3(90, 90, 0),
				new Vector3(0.43f, 0.43f, 0.43f));
			turboBig_blowoff_valve_part.AddScrew(new Screw(new Vector3(0.016f, -0.04f, 0.008f), new Vector3(0, 0, 0),
				Screw.Type.Normal, 0.3f));

			turboBig_hood_part = new Part(
				"turboBig_hood",
				"Racing Turbo Hood",
				CarH.satsuma,
				turboBig_hood_installLocation,
				new Vector3(0, 180, 0),
				partBaseInfo
			);
			turboBig_hood_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0)),
				new Screw(new Vector3(0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0)),
				new Screw(new Vector3(-0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0)),
				new Screw(new Vector3(-0.4075f, 0.01f, 0.02f), new Vector3(90, 0, 0)),
			}, 0.6f);

			//Hood_Logic hoodLogic = turboBig_hood_part.AddComponent<Hood_Logic>();
			//hoodLogic.Init(turboBig_hood_part);

			turboSmall_part = new Part(
				"turboSmall",
				"GT Turbo",
				originalCylinerHead,
				turboSmall_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			turboSmall_part.AddClampModel(new Vector3(0.0715f, -0.043f, 0.052f), new Vector3(0, 90, 0),
				new Vector3(0.5f, 0.5f, 0.5f));
			turboSmall_part.AddScrew(new Screw(new Vector3(0.0715f, -0.024f, 0.044f), new Vector3(180f, 0f, 0f),
				Screw.Type.Normal, 0.4f));

			turboSmall_intercooler_tube_part = new Part(
				"turboSmall_intercooler_tube",
				"GT Turbo Intercooler Tube",
				CarH.satsuma,
				turboSmall_intercooler_tube_installLocation,
				new Vector3(0, 180, 0),
				partBaseInfo
			);
			turboSmall_intercooler_tube_part.AddClampModel(new Vector3(0.034f, -0.154f, -0.1548f),
				new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
			turboSmall_intercooler_tube_part.AddClampModel(new Vector3(0.0225f, 0.24f, 0.313f), new Vector3(90, 0, 0),
				new Vector3(0.5f, 0.5f, 0.5f));
			turboSmall_intercooler_tube_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0.034f, -0.13f, -0.1638f), new Vector3(180f, 0f, 0f)),
				new Screw(new Vector3(0.014f, 0.24f, 0.332f), new Vector3(0f, -90f, 0f)),
			}, 0.4f);

			turboSmall_manifold_twinCarb_tube_part = new Part(
				"turboSmall_manifold_twinCarb_tube",
				"GT Turbo Manifold TwinCarb Tube",
				originalCylinerHead,
				turboSmall_manifold_twinCarb_tube_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			turboSmall_manifold_twinCarb_tube_part.AddClampModel(new Vector3(-0.106f, -0.07f, -0.116f),
				new Vector3(-90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
			turboSmall_manifold_twinCarb_tube_part.AddScrew(new Screw(new Vector3(-0.097f, -0.07f, -0.135f),
				new Vector3(0, 90, 0), Screw.Type.Normal, 0.4f));

			turboSmall_airfilter_part = new Part(
				"turboSmall_airfilter",
				"GT Turbo Airfilter",
				originalCylinerHead,
				turboSmall_airfilter_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			turboSmall_airfilter_part.AddClampModel(new Vector3(0f, 0f, 0.049f), new Vector3(0, 0, 0),
				new Vector3(0.65f, 0.65f, 0.65f));
			turboSmall_airfilter_part.AddScrew(new Screw(new Vector3(0.0095f, 0.025f, 0.0488f), new Vector3(0, 90, 0),
				Screw.Type.Normal, 0.4f));

			turboSmall_exhaust_inlet_tube_part = new Part(
				"turboSmall_exhaust_inlet_tube",
				"GT Turbo Exhaust Inlet Tube",
				originalCylinerHead,
				turboSmall_exhaust_inlet_tube_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			turboSmall_exhaust_inlet_tube_part.AddScrews(new Screw[]
			{
				new Screw(new Vector3(0.114f, -0.044f, -0.035f), new Vector3(-90f, 0f, 0f)),
				new Screw(new Vector3(0.06f, -0.044f, -0.044f), new Vector3(-90f, 0f, 0f)),
			}, 0.7f);

			turboSmall_exhaust_outlet_tube_part = new Part(
				"turboSmall_exhaust_outlet_tube",
				"GT Turbo Exhaust Outlet Tube",
				originalCylinerHead,
				turboSmall_exhaust_outlet_tube_installLocation,
				new Vector3(90, 0, 0),
				partBaseInfo
			);
			turboSmall_exhaust_outlet_tube_part.AddClampModel(new Vector3(-0.068f, 0.1445f, -0.0235f),
				new Vector3(0, 0, 0), new Vector3(0.67f, 0.67f, 0.67f));
			turboSmall_exhaust_outlet_tube_part.AddScrew(new Screw(new Vector3(-0.078f, 0.1708f, -0.0235f),
				new Vector3(0, -90, 0), Screw.Type.Normal, 0.5f));

			bigPartsList = new List<Part>
			{
				turboBig_part,
				turboBig_intercooler_tube_part,
				turboBig_exhaust_inlet_tube_part,
				turboBig_exhaust_outlet_tube_part,
				turboBig_blowoff_valve_part,
				turboBig_exhaust_outlet_straight_part,
			};

			smallPartsList = new List<Part>
			{
				turboSmall_part,
				turboSmall_intercooler_tube_part,
				turboSmall_exhaust_inlet_tube_part,
				turboSmall_exhaust_outlet_tube_part,
				turboSmall_airfilter_part,
				turboSmall_manifold_twinCarb_tube_part,
			};

			otherPartsList = new List<Part>
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

			turboBig_kit = new Kit(
				"Racing Turbocharger Kit",
				new Part[]
				{
					turboBig_part,
					turboBig_intercooler_tube_part,
					turboBig_exhaust_inlet_tube_part,
					turboBig_exhaust_outlet_tube_part,
				});
			turboSmall_kit = new Kit(
				"GT Turbocharger Kit",
				new Part[]
				{
					turboSmall_part,
					turboSmall_exhaust_inlet_tube_part,
					turboSmall_exhaust_outlet_tube_part,
					turboSmall_intercooler_tube_part,
				});

			manifoldWeber_kit = new Kit(
				"Weber Kit",
				new Part[]
				{
					manifold_weber_part,
					intercooler_manifold_weber_tube_part
				});
			manifoldTwinCarb_kit = new Kit(
				"TwinCarb Kit",
				new Part[]
				{
					manifold_twinCarb_part,
					intercooler_manifold_twinCarb_tube_part
				});

			var shopBaseInfo = new ShopBaseInfo(this, assetsBundle);

			var shopSpawnLocation = Shop.SpawnLocation.Fleetari.Counter;


			Shop.Add(shopBaseInfo, Shop.ShopLocation.Fleetari, new ShopItem[]
			{
				new ShopItem("Racing Turbocharger Kit", 8100, shopSpawnLocation, turboBig_kit),
				new ShopItem("GT Turbocharger Kit", 4150, shopSpawnLocation, turboSmall_kit),
				new ShopItem("Racing Turbocharger Straight Exhaust", 1000, shopSpawnLocation,
					turboBig_exhaust_outlet_straight_part),
				new ShopItem("Racing Turbocharger Blowoff Valve", 1350, shopSpawnLocation, turboBig_blowoff_valve_part),
				new ShopItem("Racing Turbocharger Hood", 1800, shopSpawnLocation, turboBig_hood_part),
				new ShopItem("GT Turbocharger TwinCarb Tube", 300, shopSpawnLocation,
					turboSmall_manifold_twinCarb_tube_part),
				new ShopItem("GT Turbocharger Airfilter", 800, shopSpawnLocation, turboSmall_airfilter_part),
				new ShopItem("TwinCarb Manifold Kit", 1950, shopSpawnLocation, manifoldTwinCarb_kit),
				new ShopItem("Weber Manifold Kit", 2250, shopSpawnLocation, manifoldWeber_kit),
				new ShopItem("Intercooler", 3500, shopSpawnLocation, intercooler_part),
				new ShopItem("Boost Gauge", 180, shopSpawnLocation, boost_gauge_part),
				new ShopItem("Turbocharger Exhaust Header", 2100, shopSpawnLocation, exhaust_header_part),
			});

			TurboConfiguration racingTurboConfig = new TurboConfiguration
			{
				boostBase = 2f,
				boostStartingRpm = 3000,
				boostStartingRpmOffset = 1600,
				boostMin = -0.10f,
				minSettableBoost = 1.65f,
				boostSteepness = 1.5f,
				blowoffDelay = 0.8f,
				blowoffTriggerBoost = 0.6f,
				backfireThreshold = 4000,
				backfireRandomRange = 20,
				rpmMultiplier = 14,
				extraPowerMultiplicator = 1.5f,
				boostSettingSteps = 0.05f,
				soundboostMinVolume = 0.1f,
				soundboostMaxVolume = 0.35f,
				soundboostPitchMultiplicator = 4
			};

            //Temporary
            TurboConfiguration gtTurboConfig = new TurboConfiguration
            {
                boostBase = 2f,
                boostStartingRpm = 3500,
                boostStartingRpmOffset = 2200,
                boostMin = -0.10f,
                minSettableBoost = 1.65f,
                boostSteepness = 2.2f,
                blowoffDelay = 0.8f,
                blowoffTriggerBoost = 0.6f,
                backfireThreshold = 4000,
                backfireRandomRange = 20,
                rpmMultiplier = 14,
                extraPowerMultiplicator = 1.5f,
                boostSettingSteps = 0.05f,
                soundboostMinVolume = 0.05f,
                soundboostMaxVolume = 0.3f,
                soundboostPitchMultiplicator = 4,
            };

			racingTurbo = new Turbo(this, turboBig_part, boostSave, "turbocharger_loop.wav", "grinding sound.wav",
				"turbocharger_blowoff.wav",
				new bool[]
				{
					true,
					false,
					true
				}, racingTurboConfig,
				turboBig_blowoff_valve_part.gameObject.transform.FindChild("blowoff_valve").gameObject);

			racingTurbo.turbine = turboBig_part.gameObject.transform.FindChild("compressor_turbine").gameObject;
			racingTurbo.backfire_Logic =
				turboBig_exhaust_outlet_straight_part.gameObject.AddComponent<Backfire_Logic>();

            List<WearCondition> wearConditions = new List<WearCondition>
            {
                new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
            };

            racingTurboWear = new Wear(this, "racingTurbo", turboBig_part, wearConditions, 
                0.003f, 0.5f, partsWearSave, 4000, "repair_turbocharger_big_ProductImage.png", 100);

            intercoolerWear = new Wear(this, "intercooler", intercooler_part, wearConditions, 
                0.005f, 0.5f, partsWearSave, 1500, "repair_intercooler_ProductImage.png", 100);

            gtTurboWear = new Wear(this, "gtTurbo", turboSmall_part, wearConditions, 
                0.003f, 0.5f, partsWearSave, 2500, "repair_turbocharger_small_ProductImage.png", 100);

            //Todo:
            /* manipulator should reduce the value substracted from the turbo boost
             * but without making the last value which should make the boos 0 be also multiplied by 0 making it the base boost of 2
             * 
             * 
            */

            float[] multiplicatorManipulator = new float[]
            {
                0.5f,
                0.4f,
                0.2f,
                0.1f,
                0,
            };

            gtTurboAirfilterWear = new Wear(this, "gtTurboAirfilter", turboSmall_airfilter_part, wearConditions, 
                0.0045f, 0.5f, partsWearSave, 400, "repair_turbocharger_small_airfilter_ProductImage.png", 100, multiplicatorManipulator);



            turboBigPaintSystem = new PaintSystem(partsColorSave, turboBig_part, new Color(0.8f, 0.8f, 0.8f));
            turboBigHoodPaintSystem = new PaintSystem(partsColorSave, turboBig_hood_part, new Color(0.8f, 0.8f, 0.8f), true);
            intercoolerPaintSystem = new PaintSystem(partsColorSave, intercooler_part, new Color(0.8f, 0.8f, 0.8f), false, "intercooler__");
            intercoolerPaintSystem.SetMetal(0.8f, 0.5f);



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
				}, gtTurboConfig, turboSmall_part.gameObject.transform.FindChild("blowoff_valve").gameObject);

            SetupInspectionPrevention();
            assetsBundle.Unload(false);
            //boost_gauge_part;


			ModConsole.Print(this.Name + $" [v{this.Version} finished loading");
		}

		private void SetupInspectionPrevention()
		{
			GameObject inspectionProcess = Cache.Find("InspectionProcess");
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
			if (partsList.Any(part => part.IsInstalled()))
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
			/*
			PaintSystem.Save(this, color_saveFile, new PaintSystem[]
			{
				turboBigPaintSystem,
				turboBigHoodPaintSystem,
				intercoolerPaintSystem,
                //turboSmallAirfilterPaintSystem,
            });
			*/

			//Shop.Save(this, modsShop_saveFile, partsList.ToArray());

			//AdvPart.Save(this, modsShop_saveFile, partsList.ToArray());

            Turbo.Save(this, boost_saveFile, new Turbo[]
            {
                    racingTurbo,
                    gtTurbo,
            });

			/*
			Wear.Save(this, wear_saveFile, new Wear[]
			{
				racingTurboWear,
				intercoolerWear,
                //gtTurboWear,
            });
			*/
		}

		public override void OnGUI()
		{
			if ((bool)debugGuiSetting.Value)
			{
				guiDebug.Handle(new GuiDebugInfo[]
				{
					new GuiDebugInfo("DEBUG", "Engine RPM", ((int)CarH.drivetrain.rpm).ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Racing Turbo bar", racingTurbo.boost.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "GT Turbo bar", gtTurbo.boost.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Power multiplier", CarH.drivetrain.powerMultiplier.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "KM/H", ((int)CarH.drivetrain.differentialSpeed).ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Torque", CarH.drivetrain.torque.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Clutch Max Torque", CarH.drivetrain.clutchMaxTorque.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Clutch Torque Multiplier",
						CarH.drivetrain.clutchTorqueMultiplier.ToStringOrEmpty()),
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

			//Todo
			if (!racingTurbo.CheckAllRequiredInstalled() && !gtTurbo.CheckAllRequiredInstalled() && CarH.hasPower)
			{
				boostGaugeLogic.SetDigitalText("ERR");
			}
			else if (CarH.hasPower && boostGaugeLogic.gaugeMode != GaugeMode.Digital)
			{
				boostGaugeLogic.SetDigitalText("");
			}

			HandleExhaustSystem();
			//HandlePartsTrigger();
		}

		private void HandlePartsTrigger()
		{
			/* replace with part.Add...Action implementation
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

			if (turboBig_exhaust_outlet_tube_part.installed) { turboBig_exhaust_outlet_straight_part.partTrigger.triggerGameObject.SetActive(false); }
			else { turboBig_exhaust_outlet_straight_part.partTrigger.triggerGameObject.SetActive(true); }
			*/
		}

		private void HandleExhaustSystem()
		{
			bool allBig = AllBigInstalled();
			bool allSmall = AllSmallInstalled();
			bool allOther = AllOtherInstalled();

			if (steelHeaders_inst.Value || headers_inst.Value)
			{
				if (exhaust_header_part.IsInstalled())
				{
					exhaust_header_part.Uninstall();
				}
			}

			if (CarH.running)
			{
				if (turboBig_exhaust_outlet_straight_part.IsInstalled() && allBig && allOther)
				{
					exhaustFromEngine.SetActive(false);
					exhaustFromPipe.SetActive(true);

					exhaustFromPipe.transform.parent = turboBig_exhaust_outlet_straight_part.gameObject.transform;
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
						if (racingExhaustPipe_inst.Value && racingExhaustMuffler_inst.Value)
						{
							exhaustFromEngine.SetActive(false);
							exhaustFromPipe.SetActive(false);
							exhaustFromMuffler.SetActive(true);
						}
						else if (racingExhaustPipe_inst.Value)
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
						if ((steelHeaders_inst.Value && racingExhaustPipe_inst.Value &&
						     racingExhaustMuffler_inst.Value) || (headers_inst.Value && exhaustPipe_inst.Value &&
						                                          (exhaustMuffler_inst.Value ||
						                                           exhaustMufflerDualTip_inst.Value)))
						{
							exhaustFromEngine.SetActive(false);
							exhaustFromPipe.SetActive(false);
							exhaustFromMuffler.SetActive(true);
						}
						else if ((steelHeaders_inst.Value && racingExhaustPipe_inst.Value) ||
						         (headers_inst.Value && exhaustPipe_inst.Value))
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

		private void PosReset()
		{
			try
			{
				partsList.ForEach(delegate(Part part) { part.ResetToDefault(); });
			}
			catch (Exception ex)
			{
				ModConsole.Error(ex.Message);
			}
		}

		public bool AllBigInstalled(bool ignoreScrewable = false)
		{
			return turboBig_part.IsFixed(ignoreScrewable) &&
			       turboBig_intercooler_tube_part.IsFixed(ignoreScrewable) &&
			       turboBig_exhaust_inlet_tube_part.IsFixed(ignoreScrewable) &&
			       (turboBig_exhaust_outlet_tube_part.IsFixed(ignoreScrewable) ||
			        turboBig_exhaust_outlet_straight_part.IsFixed(ignoreScrewable)) &&
			       turboBig_blowoff_valve_part.IsFixed(ignoreScrewable);
		}

		public bool AllSmallInstalled(bool ignoreScrewable = false)
		{
			return turboSmall_part.IsFixed(ignoreScrewable) &&
			       (turboSmall_intercooler_tube_part.IsFixed(ignoreScrewable) ||
			        turboSmall_manifold_twinCarb_tube_part.IsFixed(ignoreScrewable)) &&
			       turboSmall_exhaust_inlet_tube_part.IsFixed(ignoreScrewable) &&
			       turboSmall_exhaust_outlet_tube_part.IsFixed(ignoreScrewable);
		}

		public bool AllOtherInstalled(bool ignoreScrewable = false)
		{
			return (manifold_weber_part.IsFixed(ignoreScrewable) || manifold_twinCarb_part.IsFixed(ignoreScrewable)) &&
			       (
				       (intercooler_part.IsFixed(ignoreScrewable) &&
				        (intercooler_manifold_weber_tube_part.IsFixed(ignoreScrewable) ||
				         intercooler_manifold_twinCarb_tube_part.IsFixed(ignoreScrewable))
				       ) ||
				       turboSmall_manifold_twinCarb_tube_part.IsFixed(ignoreScrewable)) &&
			       exhaust_header_part.IsFixed(ignoreScrewable);
		}

		public bool AnyBigInstalled(bool ignoreScrewable = false)
		{
			return turboBig_part.IsFixed(ignoreScrewable) ||
			       turboBig_intercooler_tube_part.IsFixed(ignoreScrewable) ||
			       turboBig_exhaust_inlet_tube_part.IsFixed(ignoreScrewable) ||
			       turboBig_exhaust_outlet_tube_part.IsFixed(ignoreScrewable) ||
			       turboBig_blowoff_valve_part.IsFixed(ignoreScrewable) ||
			       turboBig_exhaust_outlet_straight_part.IsFixed(ignoreScrewable);
		}

		public bool AnySmallInstalled(bool ignoreScrewable = false)
		{
			return turboSmall_part.IsFixed(ignoreScrewable) ||
			       turboSmall_intercooler_tube_part.IsFixed(ignoreScrewable) ||
			       turboSmall_exhaust_inlet_tube_part.IsFixed(ignoreScrewable) ||
			       turboSmall_exhaust_outlet_tube_part.IsFixed(ignoreScrewable) ||
			       turboSmall_airfilter_part.IsFixed(ignoreScrewable) ||
			       turboSmall_manifold_twinCarb_tube_part.IsFixed(ignoreScrewable);
		}
	}
}