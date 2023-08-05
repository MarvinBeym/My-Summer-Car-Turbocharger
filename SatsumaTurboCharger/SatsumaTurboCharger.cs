using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Shopping;
using MscModApi.Tools;
using SatsumaTurboCharger.turbo;
using System;
using System.Collections.Generic;
using System.Linq;
using MscModApi.PaintingSystem;
using SatsumaTurboCharger.part;
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
        public override string Version => "2.2";
        public override bool UseAssetsFolder => true;
        
        public GuiDebug guiDebug;

        //Kits
        private Kit turboBig_kit;
        private Kit turboSmall_kit;
        private Kit manifoldWeber_kit;
        private Kit manifoldTwinCarb_kit;

        //Saves
        public Dictionary<string, float> partsWearSave;

        public Dictionary<string, float> boostSave;

        //Files
        private const string boost_saveFile = "turbocharger_mod_boost_SaveFile.txt";
        private const string wear_saveFile = "wear_saveFile.json";

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

        public Settings rotateTurbineSetting =
            new Settings("rotateTurbineSetting", "Allow turbo turbine rotation", false);

        public Settings backfireEffectSetting =
            new Settings("backfireEffectSetting", "Allow backfire effect for turbo", false);

        private Settings useCustomGearRatios =
            new Settings("useCustomGearRatios", "Use custom gear ratios", false, Helper.WorkAroundAction);

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

        internal static List<Part> partsList = new List<Part>();
        public List<Part> bigPartsList;
        public List<Part> smallPartsList;
        public List<Part> otherPartsList;
        
        //Wear
        //private Wear racingTurboWear;
        //private Wear intercoolerWear;
        //private Wear gtTurboWear;
        //private Wear gtTurboAirfilterWear;


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

        public override void OnNewGame()
        {
            MscModApi.MscModApi.NewGameCleanUp(this);
            //SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(this, null, modsShop_saveFile);
            //SaveLoad.SerializeSaveFile<BoostSave>(this, null, boost_saveFile);
            //SaveLoad.SerializeSaveFile<Dictionary<string, float>>(this, null, wear_saveFile);
        }

        public override void OnLoad()
        {
            ModConsole.Print(Name + $" [v{Version} started loading");
            Logger.InitLogger(this);

            ecuModInstalled = ModLoader.IsModPresent("DonnerTech_ECU_Mod");

            guiDebug = new GuiDebug(Screen.width - 310, 50, 300, "ECU MOD DEBUG", new[]
            {
                new GuiDebugElement("DEBUG"),
                new GuiDebugElement("Wear"),
            });

            resetPosSetting.DoAction = PosReset;
            useCustomGearRatios.DoAction = delegate
            {
                if ((bool)useCustomGearRatios.Value)
                {
                    CarH.drivetrain.gearRatios = newGearRatios;
                }
                else
                {
                    CarH.drivetrain.gearRatios = originalGearRatios;
                }
            };
            assetsBundle = Helper.LoadAssetBundle(this, "turbochargermod.unity3d");
            partBaseInfo = new PartBaseInfo(this, assetsBundle, partsList);

            
            
            try
            {
                CarH.drivetrain.clutchTorqueMultiplier = 10f;

                exhaustFromEngine = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");
                exhaustFromPipe = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");
                exhaustFromMuffler = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");

                originalExhaustPipeRaceParent = exhaustFromPipe.transform.parent;
                originalExhaustPipeRacePosition = new Vector3(exhaustFromPipe.transform.localPosition.x,
                    exhaustFromPipe.transform.localPosition.y, exhaustFromPipe.transform.localPosition.z);
                originalExhaustPipeRaceRotation = new Quaternion(exhaustFromPipe.transform.localRotation.x,
                    exhaustFromPipe.transform.localRotation.y, exhaustFromPipe.transform.localRotation.z,
                    exhaustFromPipe.transform.localRotation.w);

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
                partsWearSave = Helper.LoadSaveOrReturnNew<Dictionary<string, float>>(this, wear_saveFile);
                boostSave = Helper.LoadSaveOrReturnNew<Dictionary<string, float>>(this, boost_saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to deserialize save file", "Please check paths to save files", ex);
            }

            manifoldWeber = new ManifoldWeber();
            manifoldTwinCarb = new ManifoldTwinCarb();
            boostGauge = new BoostGauge();
            intercooler = new Intercooler();
            intercoolerManifoldWeberTube = new IntercoolerManifoldWeberTube(manifoldWeber);
            intercoolerManifoldTwinCarbTube = new IntercoolerManifoldTwinCarbTube(manifoldTwinCarb);
            exhaustHeader = new ExhaustHeader();
            turboBigExhaustInletTube = new TurboBigExhaustInletTube(exhaustHeader);
            turboBigExhaustOutletTube = new TurboBigExhaustOutletTube();
            turboBigIntercoolerTube = new TurboBigIntercoolerTube(intercooler);
            turboBigBlowoffValve = new TurboBigBlowoffValve(turboBigIntercoolerTube);

            turboBig = new TurboBig(this, boostSave, turboBigExhaustInletTube, turboBigBlowoffValve,turboBigExhaustOutletStraight);
            turboBigExhaustOutletStraight = new TurboBigExhaustOutletStraight(turboBig);
            turboBig.AddBackfire(turboBigExhaustOutletStraight);
            turboBigHood = new TurboBigHood();

            turboSmall = new TurboSmall(this, boostSave);

            turboSmallIntercoolerTube = new TurboSmallIntercoolerTube();

            turboSmallAirfilter = new TurboSmallAirfilter();


            turboSmallExhaustInletTube = new TurboSmallExhaustInletTube();

            turboSmallExhaustOutletTube = new TurboSmallExhaustOutletTube();

            bigPartsList = new List<Part>
            {
                turboBig,
                turboBigIntercoolerTube,
                turboBigExhaustInletTube,
                turboBigExhaustOutletTube,
                turboBigBlowoffValve,
                turboBigExhaustOutletStraight,
            };

            smallPartsList = new List<Part>
            {
                turboSmall,
                turboSmallIntercoolerTube,
                turboSmallExhaustInletTube,
                turboSmallExhaustOutletTube,
                turboSmallAirfilter,
            };

            otherPartsList = new List<Part>
            {
                manifoldWeber,
                manifoldTwinCarb,
                boostGauge,
                intercooler,
                intercoolerManifoldWeberTube,
                intercoolerManifoldTwinCarbTube,
                turboBigHood,
                exhaustHeader,
            };

            partsList.AddRange(bigPartsList);
            partsList.AddRange(smallPartsList);
            partsList.AddRange(otherPartsList);

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
                new ShopItem("Intercooler", 3500, shopSpawnLocation, intercooler),
                new ShopItem("Boost Gauge", 180, shopSpawnLocation, boostGauge),
                new ShopItem("Turbocharger Exhaust Header", 2100, shopSpawnLocation, exhaustHeader),
            });

            //Temporary => Adapt to fit gt turbo style

            /*
             * Requires reimplementation
             */
            /*
            List<WearCondition> wearConditions = new List<WearCondition>
            {
                new WearCondition(75, WearCondition.Check.MoreThan, 1, "Looks brand new..."),
                new WearCondition(50, WearCondition.Check.MoreThan, 1.1f, "Some scratches and a bit of damage. Should be fine I guess..."),
                new WearCondition(25, WearCondition.Check.MoreThan, 1.3f, "I can hear air escaping more than before"),
                new WearCondition(15, WearCondition.Check.MoreThan, 1.5f, "It sounds like a leaf blower"),
                new WearCondition(15, WearCondition.Check.LessThan, 0, "Well... I think it's fucked"),
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
                turboBig.turbo,
                turboSmall.turbo,
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
                    new GuiDebugInfo("DEBUG", "Racing Turbo bar", turboBig.turbo.boost.ToStringOrEmpty()),
                    new GuiDebugInfo("DEBUG", "GT Turbo bar", turboSmall.turbo.boost.ToStringOrEmpty()),
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

            turboBig.turbo.Handle(allBig, allSmall, allOther);
            turboBig.turbo.UpdateCondition("weberCarb", weberCarb_inst.Value);
            turboBig.turbo.UpdateCondition("twinCarb", twinCarb_inst.Value);

            //Todo
            /*
            if (!turboBig.turbo.CheckAllRequiredInstalled() && !turboSmall.turbo.CheckAllRequiredInstalled() && CarH.hasPower)
            {
                //boostGaugeLogic.SetDigitalText("ERR");
            }
            else if (CarH.hasPower && boostGaugeLogic.gaugeMode != GaugeMode.Digital)
            {
                boostGaugeLogic.SetDigitalText("");
            }*/

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
                turboSmall.partTrigger.triggerGameObject.SetActive(false);
                turboSmallIntercoolerTube.partTrigger.triggerGameObject.SetActive(false);
                turboSmallExhaustInletTube.partTrigger.triggerGameObject.SetActive(false);
                turboSmallExhaustOutletTube.partTrigger.triggerGameObject.SetActive(false);
                turboSmallAirfilter.partTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turboSmall.partTrigger.triggerGameObject.SetActive(true);
                turboSmallIntercoolerTube.partTrigger.triggerGameObject.SetActive(true);
                turboSmallExhaustInletTube.partTrigger.triggerGameObject.SetActive(true);
                turboSmallExhaustOutletTube.partTrigger.triggerGameObject.SetActive(true);
                turboSmallAirfilter.partTrigger.triggerGameObject.SetActive(true);
            }

            if (anySmall)
            {
                turboBig.partTrigger.triggerGameObject.SetActive(false);
                turboBigIntercoolerTube.partTrigger.triggerGameObject.SetActive(false);
                turboBigExhaustInletTube.partTrigger.triggerGameObject.SetActive(false);
                turboBigExhaustOutletTube.partTrigger.triggerGameObject.SetActive(false);
                turboBigBlowoffValve.partTrigger.triggerGameObject.SetActive(false);
                turboBigExhaustOutletStraight.partTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turboBig.partTrigger.triggerGameObject.SetActive(true);
                turboBigIntercoolerTube.partTrigger.triggerGameObject.SetActive(true);
                turboBigExhaustInletTube.partTrigger.triggerGameObject.SetActive(true);
                turboBigExhaustOutletTube.partTrigger.triggerGameObject.SetActive(true);
                turboBigBlowoffValve.partTrigger.triggerGameObject.SetActive(true);
                turboBigExhaustOutletStraight.partTrigger.triggerGameObject.SetActive(true);
            }

            if (weberCarb_inst.Value)
            {
                intercoolerManifoldTwinCarbTube.partTrigger.triggerGameObject.SetActive(false);
                manifoldTwinCarb.partTrigger.triggerGameObject.SetActive(false);

                if (intercoolerManifoldTwinCarbTube.installed) { intercoolerManifoldTwinCarbTube.removePart(); }
                if (manifoldTwinCarb.installed) { manifoldTwinCarb.removePart(); }

            }
            else
            {
                intercoolerManifoldTwinCarbTube.partTrigger.triggerGameObject.SetActive(true);
                manifoldTwinCarb.partTrigger.triggerGameObject.SetActive(true);
            }

            if (twinCarb_inst.Value)
            {
                manifoldWeber.partTrigger.triggerGameObject.SetActive(false);
                intercoolerManifoldWeberTube.partTrigger.triggerGameObject.SetActive(false);

                if (manifoldWeber.installed) { manifoldWeber.removePart(); }
                if (intercoolerManifoldWeberTube.installed) { intercoolerManifoldWeberTube.removePart(); }
            }
            else
            {
                manifoldWeber.partTrigger.triggerGameObject.SetActive(true);
                intercoolerManifoldWeberTube.partTrigger.triggerGameObject.SetActive(true);
            }


            if (!turboBig.installed && turboBigExhaustOutletStraight.installed)
            {
                turboBigExhaustOutletStraight.removePart();
            }
            if (!turboBigIntercoolerTube.installed && !turboBigExhaustOutletTube.installed && !turboBigExhaustInletTube.installed && turboBig.installed)
            {
                turboBig.removePart();

                if (turboBigExhaustOutletStraight.installed) { turboBigExhaustOutletStraight.removePart(); }
            }
            if (!turboBigIntercoolerTube.installed && turboBigBlowoffValve.installed)
            {
                turboBigBlowoffValve.removePart();
            }

            if (!turboSmall.installed)
            {
                if (turboSmallAirfilter.installed)
                    turboSmallAirfilter.removePart();
            }

            if (!turboSmallIntercoolerTube.installed && !turboSmallExhaustInletTube.installed && !turboSmallExhaustOutletTube.installed && turboSmall.installed)
            {
                turboSmall.removePart();
                if (turboSmallAirfilter.installed)
                    turboSmallAirfilter.removePart();
            }

            if (turboBigExhaustOutletStraight.installed) { turboBigExhaustOutletTube.partTrigger.triggerGameObject.SetActive(false); }
            else { turboBigExhaustOutletTube.partTrigger.triggerGameObject.SetActive(true); }

            if (turboBigExhaustOutletTube.installed) { turboBigExhaustOutletStraight.partTrigger.triggerGameObject.SetActive(false); }
            else { turboBigExhaustOutletStraight.partTrigger.triggerGameObject.SetActive(true); }
            */
        }

        private void HandleExhaustSystem()
        {
            bool allBig = AllBigInstalled();
            bool allSmall = AllSmallInstalled();
            bool allOther = AllOtherInstalled();

            if (steelHeaders_inst.Value || headers_inst.Value)
            {
                if (exhaustHeader.IsInstalled())
                {
                    exhaustHeader.Uninstall();
                }
            }

            if (CarH.running)
            {
                if (turboBigExhaustOutletStraight.IsInstalled() && allBig && allOther)
                {
                    exhaustFromEngine.SetActive(false);
                    exhaustFromPipe.SetActive(true);

                    exhaustFromPipe.transform.parent = turboBigExhaustOutletStraight.gameObject.transform;
                    exhaustFromPipe.transform.localPosition = turboBig.turbo.backfire_Logic.GetFireFXPos();
                    exhaustFromPipe.transform.localRotation = turboBig.turbo.backfire_Logic.GetFireFXRot();
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
                Logger.New("Resetting positions failed", ex);
            }
        }

        public bool AllBigInstalled(bool ignoreScrewable = false)
        {
            return turboBig.IsFixed(ignoreScrewable) &&
                   turboBigIntercoolerTube.IsFixed(ignoreScrewable) &&
                   turboBigExhaustInletTube.IsFixed(ignoreScrewable) &&
                   (turboBigExhaustOutletTube.IsFixed(ignoreScrewable) ||
                    turboBigExhaustOutletStraight.IsFixed(ignoreScrewable)) &&
                   turboBigBlowoffValve.IsFixed(ignoreScrewable);
        }

        public bool AllSmallInstalled(bool ignoreScrewable = false)
        {
            return turboSmall.IsFixed(ignoreScrewable) &&
                   turboSmallIntercoolerTube.IsFixed(ignoreScrewable) &&
                   turboSmallExhaustInletTube.IsFixed(ignoreScrewable) &&
                   turboSmallExhaustOutletTube.IsFixed(ignoreScrewable);
        }

        public bool AllOtherInstalled(bool ignoreScrewable = false)
        {
            return (manifoldWeber.IsFixed(ignoreScrewable) || manifoldTwinCarb.IsFixed(ignoreScrewable)) &&
                   (
                       (intercooler.IsFixed(ignoreScrewable) &&
                        (intercoolerManifoldWeberTube.IsFixed(ignoreScrewable) ||
                         intercoolerManifoldTwinCarbTube.IsFixed(ignoreScrewable))
                       ) &&
                       exhaustHeader.IsFixed(ignoreScrewable));
        }

        public bool AnyBigInstalled(bool ignoreScrewable = false)
        {
            return turboBig.IsFixed(ignoreScrewable) ||
                   turboBigIntercoolerTube.IsFixed(ignoreScrewable) ||
                   turboBigExhaustInletTube.IsFixed(ignoreScrewable) ||
                   turboBigExhaustOutletTube.IsFixed(ignoreScrewable) ||
                   turboBigBlowoffValve.IsFixed(ignoreScrewable) ||
                   turboBigExhaustOutletStraight.IsFixed(ignoreScrewable);
        }

        public bool AnySmallInstalled(bool ignoreScrewable = false)
        {
            return turboSmall.IsFixed(ignoreScrewable) ||
                   turboSmallIntercoolerTube.IsFixed(ignoreScrewable) ||
                   turboSmallExhaustInletTube.IsFixed(ignoreScrewable) ||
                   turboSmallExhaustOutletTube.IsFixed(ignoreScrewable) ||
                   turboSmallAirfilter.IsFixed(ignoreScrewable);
        }
    }
}