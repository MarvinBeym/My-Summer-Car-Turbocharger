using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Shopping;
using MscModApi.Tools;

using System;
using System.Collections.Generic;
using System.Linq;
using MscModApi.PaintingSystem;
using MscModApi.Parts.ReplacePart;
using SatsumaTurboCharger.part;
using SatsumaTurboCharger.turbo;
using Tools.gui;
using UnityEngine;

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
         *  add screws to original turboBigHood.
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
		public override string Version => "2.2.1";
		public override bool UseAssetsFolder => true;

		public GuiDebug guiDebug;

		//Kits
		private Kit turboBig_kit;
		private Kit turboSmall_kit;
		private Kit manifoldWeber_kit;
		private Kit manifoldTwinCarb_kit;

		//Saves
#if DEBUG
		public Dictionary<string, float> partsWearSave;
#endif

		public Dictionary<string, float> boostSave;

		//saveFiles
		private const string boostSaveFile = "boost_saveFile.json";

#if DEBUG
		private const string wearSaveFile = "wear_saveFile.json";
#endif

		//Exhaust smoke handling
		private GameObject exhaustFromMuffler;
		private GameObject exhaustFromHeaders;
		private GameObject exhaustFromPipe;
		private GameObject exhaustFromEngine;
		private Vector3 originalExhaustPipePosition;
		private Quaternion originalExhaustPipeRotation;
		private Transform originalExhaustPipeParent;


		//Inspection
		private PlayMakerFSM inspectionPlayMakerFsm;
		private FsmEvent inspectionFailedEvent;

		protected bool carStarted = false;

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

#if DEBUG
		public Settings partsWearSetting = new Settings("partsWearSetting", "Use parts wear system", true);
#endif
		public static SettingsCheckBox debugGuiSetting;
		public static SettingsCheckBox rotateTurbineSetting;
		public static SettingsCheckBox backfireEffectSetting;
		public static SettingsSliderInt turboVolumeSetting;
		public static SettingsSliderInt blowoffVolumeSetting;
		public static SettingsSliderInt backfireVolumeSetting;


		internal static PartBaseInfo partBaseInfo;

		//
		//ModApi Parts
		//
		//Big Turbo
		public TurboBig turboBig; //Done
		public TurboBigIntercoolerTube turboBigIntercoolerTube; //Done
		public TurboBigExhaustInletTube turboBigExhaustInletTube; //Tone
		public TurboBigExhaustOutletTube turboBigExhaustOutletTube; //Done
		public TurboBigBlowoffValve turboBigBlowoffValve; //Done
		public TurboBigExhaustOutletStraight turboBigExhaustOutletStraight; //Done
		public TurboBigHood turboBigHood; //Done

		public ExhaustHeader exhaustHeader; //Done

		//Small Turbo
		public TurboSmall turboSmall;
		public TurboSmallIntercoolerTube turboSmallIntercoolerTube;
		public TurboSmallExhaustInletTube turboSmallExhaustInletTube;
		public TurboSmallExhaustOutletTube turboSmallExhaustOutletTube;
		public TurboSmallAirfilter turboSmallAirfilter;

		//Other Parts
		public ManifoldWeber manifoldWeber; //Done
		public ManifoldTwinCarb manifoldTwinCarb; //Done
		public BoostGauge boostGauge; //Done
		public Intercooler intercooler; //Done
		public IntercoolerManifoldWeberTube intercoolerManifoldWeberTube; //Done
		public IntercoolerManifoldTwinCarbTube intercoolerManifoldTwinCarbTube; //Done

		//Game parts
		public GamePart weberCarb;
		public GamePart twinCarb;
		public GamePart steelHeaders;
		public GamePart headers;
		public GamePart racingExhaustPipe;
		public GamePart racingExhaustMuffler;
		public GamePart exhaustPipe;
		public GamePart hood;
		public GamePart fiberglassHood;
		public GamePart dashboard;
		public GamePart cylinderHead;

		internal static List<Part> partsList = new List<Part>();

		//Wear
		//private Wear racingTurboWear;
		//private Wear intercoolerWear;
		//private Wear gtTurboWear;
		//private Wear gtTurboAirfilterWear;


		//ECU-Mod Communication
		private bool ecuModInstalled = false;

		public AssetBundle assetsBundle;

		/*
		public static SettingsSlider boostBase;
		public static SettingsSliderInt boostStartingRpm;
		public static SettingsSlider boostMin;
		public static SettingsSlider boostSettingSteps;
		public static SettingsSlider minSettableBoost;
		public static SettingsSlider boostSteepness;
		public static SettingsSliderInt boostStartingRpmOffset;
		public static SettingsSlider blowoffDelay;
		public static SettingsSlider blowoffTriggerBoost;
		public static SettingsSlider backfireThreshold;
		public static SettingsSliderInt backfireRandomRange;
		public static SettingsSlider rpmMultiplier;
		public static SettingsSlider extraPowerMultiplicator;
		public static SettingsSlider soundboostMinVolume;
		public static SettingsSlider soundboostMaxVolume;
		public static SettingsSlider soundboostPitchMultiplicator;
		public static SettingsSlider backfireDelay;
*/

		public override void OnNewGame()
		{
			MscModApi.MscModApi.NewGameCleanUp(this);
			TurboPart.Save(this, boostSaveFile, new TurboPart[0]);
			//SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(this, null, modsShop_saveFile);
			//SaveLoad.SerializeSaveFile<BoostSave>(this, null, boost_saveFile);
			//SaveLoad.SerializeSaveFile<Dictionary<string, float>>(this, null, wearSaveFile);
		}

		public override void OnLoad()
		{
			ModConsole.Print(Name + $" [v{Version} started loading");
			Logger.InitLogger(this);

			ecuModInstalled = ModLoader.IsModPresent("DonnerTech_ECU_Mod");

			guiDebug = new GuiDebug(Screen.width - 310, 50, 300, "TURBO MOD DEBUG", new[]
			{
				new GuiDebugElement("DEBUG"),
#if DEBUG 
				new GuiDebugElement("Wear"),
#endif
			});

			assetsBundle = Helper.LoadAssetBundle(this, "turbochargermod.unity3d");
			TurboPart.LoadAssets(assetsBundle);
			partBaseInfo = new PartBaseInfo(this, assetsBundle, partsList);

			exhaustFromMuffler = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");
			exhaustFromHeaders = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromHeaders");
			exhaustFromPipe = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");
			exhaustFromEngine = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");

			originalExhaustPipeParent = exhaustFromPipe.transform.parent;
			originalExhaustPipePosition = exhaustFromPipe.transform.localPosition;
			originalExhaustPipeRotation = exhaustFromPipe.transform.localRotation;

			weberCarb = new GamePart("Database/DatabaseOrders/Racing Carburators");
			twinCarb = new GamePart("Database/DatabaseOrders/Twin Carburators");
			steelHeaders = new GamePart("Database/DatabaseOrders/Steel Headers");
			headers = new GamePart("Database/DatabaseMotor/Headers");
			racingExhaustPipe = new GamePart("Database/DatabaseOrders/Racing Exhaust");
			racingExhaustMuffler = new GamePart("Database/DatabaseOrders/Racing Muffler");
			exhaustPipe = new GamePart("Database/DatabaseMechanics/ExhaustPipe");
			hood = new GamePart("Database/DatabaseBody/Hood");
			fiberglassHood = new GamePart("Database/DatabaseOrders/Fiberglass Hood");
			dashboard = new GamePart("Database/DatabaseMechanics/Dashboard");
			cylinderHead = new GamePart("Database/DatabaseMotor/Cylinderhead");

			try
			{
#if DEBUG
				partsWearSave = Helper.LoadSaveOrReturnNew<Dictionary<string, float>>(this, wearSaveFile);
#endif
				boostSave = Helper.LoadSaveOrReturnNew<Dictionary<string, float>>(this, boostSaveFile);
			}
			catch (Exception ex)
			{
				Logger.New("Error while trying to deserialize save file", "Please check paths to save files", ex);
			}

			manifoldWeber = new ManifoldWeber(weberCarb);
			manifoldTwinCarb = new ManifoldTwinCarb(twinCarb);
			boostGauge = new BoostGauge(dashboard);
			intercooler = new Intercooler();
			intercoolerManifoldWeberTube = new IntercoolerManifoldWeberTube(manifoldWeber);
			intercoolerManifoldTwinCarbTube = new IntercoolerManifoldTwinCarbTube(manifoldTwinCarb);
			exhaustHeader = new ExhaustHeader(cylinderHead);

			turboBigExhaustInletTube = new TurboBigExhaustInletTube(exhaustHeader);
			turboBigIntercoolerTube = new TurboBigIntercoolerTube(intercooler);
			turboBigBlowoffValve = new TurboBigBlowoffValve(turboBigIntercoolerTube);
			turboBigHood = new TurboBigHood();

			turboBig = new TurboBig(
				this,
				boostGauge,
				turboBigBlowoffValve,
				turboBigExhaustInletTube,
				boostSave
				);
			turboBigExhaustOutletTube = new TurboBigExhaustOutletTube(turboBig);
			turboBigExhaustOutletStraight = new TurboBigExhaustOutletStraight(turboBig);

			turboBig.audioHandler.Add("backfire", turboBigExhaustOutletStraight, "backFire_once.wav", PartEvent.Type.InstallOnCar);
			turboBig.DefineBackfire(turboBigExhaustOutletStraight, turboBig.audioHandler.Get("backfire"));
			turboBig.backFireLogic.fireFXPosition = new Vector3(0.0185f, 0.073f, 0.0217f);
			turboBig.backFireLogic.fireFxRotation = new Vector3(-75, 0, 0);

			turboSmallExhaustInletTube = new TurboSmallExhaustInletTube(exhaustHeader);
			turboSmall = new TurboSmall(
				this,
				boostGauge,
				turboSmallExhaustInletTube,
				boostSave
				);
			turboSmallIntercoolerTube = new TurboSmallIntercoolerTube(intercooler);
			turboSmallAirfilter = new TurboSmallAirfilter(turboSmall);

			turboSmallExhaustOutletTube = new TurboSmallExhaustOutletTube(turboSmall);

			TurboLogicRequiredParts turboBigRequiredParts = new TurboLogicRequiredParts();
			turboBigRequiredParts.Add(turboBig);
			turboBigRequiredParts.Add(turboBigExhaustInletTube);
			turboBigRequiredParts.Add(turboBigExhaustOutletTube, turboBigExhaustOutletStraight);
			turboBigRequiredParts.Add(turboBigBlowoffValve);
			turboBigRequiredParts.Add(turboBigIntercoolerTube);
			turboBigRequiredParts.Add(exhaustHeader);
			turboBigRequiredParts.Add(manifoldWeber, manifoldTwinCarb);
			turboBigRequiredParts.Add(intercoolerManifoldWeberTube, intercoolerManifoldTwinCarbTube);
			turboBigRequiredParts.Add(intercooler);
			turboBig.DefineRequiredParts(turboBigRequiredParts);

			TurboLogicRequiredParts turboSmallRequiredParts = new TurboLogicRequiredParts();
			turboSmallRequiredParts.Add(turboSmall);
			turboSmallRequiredParts.Add(turboSmallExhaustInletTube);
			turboSmallRequiredParts.Add(turboSmallExhaustOutletTube);
			turboSmallRequiredParts.Add(turboSmallIntercoolerTube);
			turboSmallRequiredParts.Add(exhaustHeader);
			turboSmallRequiredParts.Add(manifoldWeber, manifoldTwinCarb);
			turboSmallRequiredParts.Add(intercoolerManifoldWeberTube, intercoolerManifoldTwinCarbTube);
			turboSmallRequiredParts.Add(intercooler);
			turboSmall.DefineRequiredParts(turboSmallRequiredParts);

			turboBig_kit = new Kit(
				"Racing Turbocharger Kit",
				new Part[]
				{
					turboBig,
					turboBigIntercoolerTube,
					turboBigExhaustInletTube,
					turboBigExhaustOutletTube,
				});
			turboSmall_kit = new Kit(
				"GT Turbocharger Kit",
				new Part[]
				{
					turboSmall,
					turboSmallExhaustInletTube,
					turboSmallExhaustOutletTube,
					turboSmallIntercoolerTube,
				});

			manifoldWeber_kit = new Kit(
				"Weber Kit",
				new Part[]
				{
					manifoldWeber,
					intercoolerManifoldWeberTube
				});
			manifoldTwinCarb_kit = new Kit(
				"TwinCarb Kit",
				new Part[]
				{
					manifoldTwinCarb,
					intercoolerManifoldTwinCarbTube
				});



			var shopBaseInfo = new ShopBaseInfo(this, assetsBundle);

			var shopSpawnLocation = Shop.SpawnLocation.Fleetari.Counter;

			Shop.Add(shopBaseInfo, Shop.ShopLocation.Fleetari, new[]
			{
				new ShopItem("Racing Turbocharger Kit", 8100, shopSpawnLocation, turboBig_kit),
				new ShopItem("GT Turbocharger Kit", 4150, shopSpawnLocation, turboSmall_kit),
				new ShopItem("Racing Turbocharger Straight Exhaust", 1000, shopSpawnLocation, turboBigExhaustOutletStraight),
				new ShopItem("Racing Turbocharger Blowoff Valve", 1350, shopSpawnLocation, turboBigBlowoffValve),
				new ShopItem("Racing Turbocharger Hood", 1800, shopSpawnLocation, turboBigHood),
				new ShopItem("GT Turbocharger Airfilter", 800, shopSpawnLocation, turboSmallAirfilter),
				new ShopItem("TwinCarb Manifold Kit", 1950, shopSpawnLocation, manifoldTwinCarb_kit),
				new ShopItem("Weber Manifold Kit", 2250, shopSpawnLocation, manifoldWeber_kit),
				new ShopItem("Intercooler", 3000, shopSpawnLocation, intercooler),
				new ShopItem("Boost Gauge", 180, shopSpawnLocation, boostGauge),
				new ShopItem("Turbocharger Exhaust Header", 2100, shopSpawnLocation, exhaustHeader),
			});

			SetupPartInstallBlocking();

			//Temporary => Adapt to fit gt turbo style

			/*
             * Requires reimplementation
             */
			/*
            List<WearCondition> wearConditions = new List<WearCondition>
            {
                new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new.."),
                new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess.."),
                new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                new WearCondition(15, WearCondition.Check.LessThan, 0, "Well.. I think it's fucked"),
            };

            racingTurboWear = new Wear(this, "racingTurbo", turboBig, wearConditions,
                0.003f, 0.5f, partsWearSave, 4000, "repair_turbocharger_big_ProductImage.png", 100);

            intercoolerWear = new Wear(this, "intercooler", intercooler, wearConditions,
                0.005f, 0.5f, partsWearSave, 1500, "repairIntercooler_ProductImage.png", 100);

            gtTurboWear = new Wear(this, "gtTurbo", turboSmall, wearConditions,
                0.003f, 0.5f, partsWearSave, 2500, "repair_turbocharger_small_ProductImage.png", 100);
            */
			//Todo:
			/* manipulator should reduce the value substracted from the turbo boost
             * but without making the last value which should make the boos 0 be also multiplied by 0 making it the base boost of 2
             * 
             * 
            */
			/*
 * Requires reimplementation
 */
			/*
            float[] multiplicatorManipulator = new float[]
            {
                0.5f,
                0.4f,
                0.2f,
                0.1f,
                0,
            };

            gtTurboAirfilterWear = new Wear(this, "gtTurboAirfilter", turboSmallAirfilter, wearConditions,
                0.0045f, 0.5f, partsWearSave, 400, "repair_turbocharger_smallAirfilter_ProductImage.png", 100, multiplicatorManipulator);
            */

			/*
 * Requires reimplementation
 */

			/*
 * Requires reimplementation
 */
			/*
            racingTurbo.wears = new Wear[]
            {
                racingTurboWear,
                intercoolerWear,
            };
            */

			SetupInspectionPrevention();
			assetsBundle.Unload(false);

			ModConsole.Print(Name + $" [v{Version} finished loading");
		}

		private void SetupInspectionPrevention()
		{
			GameObject inspectionProcess = Cache.Find("InspectionProcess");
			inspectionPlayMakerFsm = inspectionProcess.FindFsm("Inspect");

			foreach (FsmEvent fsmEvent in inspectionPlayMakerFsm.FsmEvents)
			{
				if (fsmEvent.Name == "FAIL")
				{
					inspectionFailedEvent = fsmEvent;
					break;
				}
			}

			FsmHook.FsmInject(inspectionProcess, "Results", () =>
			{
				if (partsList.Any(part => part.installed))
				{
					PlayMakerFSM.BroadcastEvent(inspectionFailedEvent);
				}
			});
		}

		public override void ModSettings()
		{
			Settings.AddHeader(this, "DEBUG");
			debugGuiSetting = Settings.AddCheckBox(this, "debugGuiSetting", "Show DEBUG GUI");
			Settings.AddButton(this, "resetPos", "Reset Part positions (uninstalled)", PosReset);
			Settings.AddHeader(this, "Settings");

			rotateTurbineSetting = Settings.AddCheckBox(this, "rotateTurbineSetting", "Allow turbo turbine rotation");
			backfireEffectSetting = Settings.AddCheckBox(this, "backfireEffectSetting", "Allow backfire effect for turbo");

			Settings.AddHeader(this, "Volume", Color.clear);
			turboVolumeSetting =
				Settings.AddSlider(this, "turboVolumeSetting", "Turbo Sound Volume (loop)", 0, 200, 100);
			blowoffVolumeSetting =
				Settings.AddSlider(this, "blowoffVolumeSetting", "Blowoff Sound Volume", 0, 200, 100);
			backfireVolumeSetting =
				Settings.AddSlider(this, "backfireVolumeSetting ", "Backfire Sound Volume", 0, 200, 100);

#if DEBUG
			Settings.AddCheckBox(this, partsWearSetting);
#endif
			TransmissionHandler.SetupSettings(this);
			GearRatiosHandler.SetupSettings(this);

			/*
			Settings.AddHeader(this, "", Color.clear);
			boostBase = Settings.AddSlider(this,"boostBaseSetting", "Boost Base", 0, 2f, 0.8f);
			boostStartingRpm = Settings.AddSlider(this,"boostStartingRpmSetting", "Boost Starting Rpm", 0, 7000, 2400);
			boostStartingRpmOffset = Settings.AddSlider(this, "boostStartingRpmOffsetSetting", "Boost Starting Rpm Offset", 1000, 5000, 1000);
			boostMin = Settings.AddSlider(this,"boostMinSetting", "Boost Min", -0.2f, 1f, -0.04f);
			minSettableBoost = Settings.AddSlider(this,"minSettableBoostSetting", "Mit Settable Boost", 0.3f, 1.8f, 0.4f);
			boostSteepness = Settings.AddSlider(this,"boostSteepnessSetting", "Boost Steepness", 0.8f, 2f, 1f);
			blowoffDelay = Settings.AddSlider(this,"blowoffDelaySetting", "Blowoff Delay", 0.1f, 1.4f, 0.8f);
			blowoffTriggerBoost = Settings.AddSlider(this,"blowoffTriggerBoostSetting", "Blowoff Trigger Boost", 0.1f, 1f, 0.75f);
			backfireThreshold = Settings.AddSlider(this,"backfireThresholdSetting", "Backfire RPM Threshold", 1000, 6000, 5500f);
			backfireRandomRange = Settings.AddSlider(this,"backfireRandomRangeSetting", "Backfire Random Range", 1, 60, 20);
			rpmMultiplier = Settings.AddSlider(this,"rpmMultiplierSetting", "RPM Multiplier", 1, 30, 10f);
			extraPowerMultiplicator = Settings.AddSlider(this,"extraPowerMultiplicatorSetting", "Extra Power Multiplicator", 0.2f, 3f, 1.5f);
			boostSettingSteps = Settings.AddSlider(this, "boostSettingStepsSetting", "Boost Setting Steps", 0.01f, 0.2f, 0.05f);
			soundboostMinVolume = Settings.AddSlider(this,"soundboostMinVolumeSetting", "Soundboost Min Volume", 0.005f, 0.3f, 0.03f);
			soundboostMaxVolume = Settings.AddSlider(this,"soundboostMaxVolumeSetting", "Soundboost Max Volume", 0.01f, 0.5f, 0.08f);
			soundboostPitchMultiplicator = Settings.AddSlider(this,"soundboostPitchMultiplicatorSetting", "Soundboost Pitch Multiplicator", 0.5f, 8f, 5f);
			backfireDelay = Settings.AddSlider(this, "backfireDelaySetting", "Delay between a backfire trigger", 0.001f, 0.5f, 0.1f, null, 4);
			*/
		}

		public override void OnSave()
		{
			TurboPart.Save(this, boostSaveFile, new TurboPart[]
			{
				turboBig,
				turboSmall,
			});
		}

		public override void OnGUI()
		{
			if (debugGuiSetting.GetValue())
			{
				TurboPart turboInstalled = null;
				if (turboBig.installed)
				{
					turboInstalled = turboBig;
				}
				else if (turboSmall.installed)
				{
					turboInstalled = turboSmall;
				}

				guiDebug.Handle(new GuiDebugInfo[]
				{
					new GuiDebugInfo("DEBUG", "Engine RPM", ((int)CarH.drivetrain.rpm).ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Turbo pressure (bar)", turboInstalled == null ? "NOT INSTALLED" : turboInstalled.boost.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Turbo rpm", turboInstalled == null ? "NOT INSTALLED" : turboInstalled.rpm.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Turbo boost set", turboInstalled == null ? "NOT INSTALLED" : turboInstalled.setBoost.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Power multiplier", CarH.drivetrain.powerMultiplier.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "KM/H", ((int)CarH.drivetrain.differentialSpeed).ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Torque", CarH.drivetrain.torque.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "HP", CarH.drivetrain.currentPower.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Clutch Max Torque", CarH.drivetrain.clutchMaxTorque.ToStringOrEmpty()),
					new GuiDebugInfo("DEBUG", "Clutch Torque Multiplier",
						CarH.drivetrain.clutchTorqueMultiplier.ToStringOrEmpty()),
				});
			}
		}

		public override void Update()
		{
			TransmissionHandler.Handle();
			GearRatiosHandler.Handle();

			HandleExhaustSystem();
		}

		private void SetupPartInstallBlocking()
		{
			//Block different exhausts for bigTurbo
			turboBigExhaustOutletStraight.BlockOtherPartInstallOnEvent(PartEvent.Type.Install, turboBigExhaustOutletTube);
			turboBigExhaustOutletTube.BlockOtherPartInstallOnEvent(PartEvent.Type.Install, turboBigExhaustOutletStraight);

			//Allow only one inlet to be installed to race exhaust
			turboBigExhaustInletTube.BlockOtherPartInstallOnEvent(PartEvent.Type.Install, turboSmallExhaustInletTube);
			turboSmallExhaustInletTube.BlockOtherPartInstallOnEvent(PartEvent.Type.Install, turboBigExhaustInletTube);

			//Allow only one intercooler tube to be installed to intercooler
			turboBigIntercoolerTube.BlockOtherPartInstallOnEvent(PartEvent.Type.Install, turboSmallIntercoolerTube);
			turboSmallIntercoolerTube.BlockOtherPartInstallOnEvent(PartEvent.Type.Install, turboBigIntercoolerTube);

			exhaustHeader.BlockOtherPartInstallOnEvent(PartEvent.Type.InstallOnCar, new[]
			{
				steelHeaders,
				headers,
				exhaustPipe
			});

			exhaustPipe.BlockOtherPartInstallOnEvent(PartEvent.Type.InstallOnCar, exhaustHeader);
			steelHeaders.BlockOtherPartInstallOnEvent(PartEvent.Type.InstallOnCar, exhaustHeader);
			headers.BlockOtherPartInstallOnEvent(PartEvent.Type.InstallOnCar, exhaustHeader);

			turboBigHood.BlockOtherPartInstallOnEvent(PartEvent.Type.InstallOnCar, new[]
			{
				hood,
				fiberglassHood,
			});

			hood.BlockOtherPartInstallOnEvent(PartEvent.Type.InstallOnCar, turboBigHood);
			fiberglassHood.BlockOtherPartInstallOnEvent(PartEvent.Type.InstallOnCar, turboBigHood);

			weberCarb.AddEventListener(PartEvent.Time.Post, PartEvent.Type.InstallOnCar, () =>
			{
				turboBig.conditionStorage.UpdateCondition("weberCarb", true);
			});

			weberCarb.AddEventListener(PartEvent.Time.Post, PartEvent.Type.UninstallFromCar, () =>
			{
				turboBig.conditionStorage.UpdateCondition("weberCarb", false);
			});

			twinCarb.AddEventListener(PartEvent.Time.Post, PartEvent.Type.InstallOnCar, () =>
			{
				turboBig.conditionStorage.UpdateCondition("twinCarb", true);
			});

			twinCarb.AddEventListener(PartEvent.Time.Post, PartEvent.Type.UninstallFromCar, () =>
			{
				turboBig.conditionStorage.UpdateCondition("twinCarb", false);
			});
		}

		private void HandleExhaustSystem()
		{
			if (CarH.running)
			{
				//Would benefit from a more "event" driven setup to avoid continuously checking installed state & SetActive() calls
				carStarted = true;

				if (
					steelHeaders.installed
					|| headers.installed
					|| turboBigExhaustOutletTube.installed
					|| turboBigExhaustOutletStraight.installed
					|| turboSmallExhaustOutletTube.installed
				) {
					if (turboBigExhaustOutletStraight.installed)
					{
						exhaustFromPipe.transform.parent = turboBigExhaustOutletStraight.gameObject.transform;
						exhaustFromPipe.transform.localPosition = turboBig.backFireLogic.fireFXPosition;
						exhaustFromPipe.transform.localRotation = Quaternion.Euler(turboBig.backFireLogic.fireFxRotation);

						exhaustFromEngine.SetActive(false);
						exhaustFromHeaders.SetActive(false);
						exhaustFromPipe.SetActive(true);
						exhaustFromMuffler.SetActive(false);
						return;
					}
					else
					{
						exhaustFromPipe.transform.parent = originalExhaustPipeParent;
						exhaustFromPipe.transform.localPosition = originalExhaustPipePosition;
						exhaustFromPipe.transform.localRotation = originalExhaustPipeRotation;
					}

					if (racingExhaustMuffler.installed)
					{
						exhaustFromEngine.SetActive(false);
						exhaustFromHeaders.SetActive(false);
						exhaustFromPipe.SetActive(false);
						exhaustFromMuffler.SetActive(true);
						return;
					}

					if (racingExhaustPipe.installed || exhaustPipe.installed)
					{
						exhaustFromEngine.SetActive(false);
						exhaustFromHeaders.SetActive(false);
						exhaustFromPipe.SetActive(true);
						exhaustFromMuffler.SetActive(false);
						return;
					}

					exhaustFromEngine.SetActive(false);
					exhaustFromHeaders.SetActive(true);
					exhaustFromPipe.SetActive(false);
					exhaustFromMuffler.SetActive(false);
				}
				else
				{
					exhaustFromPipe.transform.parent = originalExhaustPipeParent;
					exhaustFromPipe.transform.localPosition = originalExhaustPipePosition;
					exhaustFromPipe.transform.localRotation = originalExhaustPipeRotation;

					exhaustFromEngine.SetActive(true);
					exhaustFromHeaders.SetActive(false);
					exhaustFromPipe.SetActive(false);
					exhaustFromMuffler.SetActive(false);
				}
			}

			if (!CarH.running && carStarted)
			{
				carStarted = false;

				exhaustFromPipe.transform.parent = originalExhaustPipeParent;
				exhaustFromPipe.transform.localPosition = originalExhaustPipePosition;
				exhaustFromPipe.transform.localRotation = originalExhaustPipeRotation;

				exhaustFromEngine.SetActive(false);
				exhaustFromHeaders.SetActive(false);
				exhaustFromPipe.SetActive(false);
				exhaustFromMuffler.SetActive(false);
			}
		}

		private void PosReset()
		{
			try
			{
				partsList.ForEach(delegate (Part part) { part.ResetToDefault(); });
			}
			catch (Exception ex)
			{
				Logger.New("Resetting positions failed", ex);
			}
		}
	}
}