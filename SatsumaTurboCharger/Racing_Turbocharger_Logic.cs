using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger
{
    public class Racing_Turbocharger_Logic : MonoBehaviour
    {
        private SatsumaTurboCharger donnerTech_turbocharger_mod;

        //Turbo mod
        private GameObject turbocharger_big_turbine;
        private float calculated_boost = 0;
        private OthersSave othersSave;
        private PartsWearSave partsWearSave;
        private ParticleSystem fire_fx_big_turbo_exhaust_straight;
        private bool canBackfire = false;
        private FsmFloat powerMultiplier;
        private bool turbocharger_blowoffShotAllowed = false;

        //ECU mod communication
        private bool ecu_mod_installed = false;
        private GameObject ecu_mod_SmartEngineModule;

        //Car
        private GameObject satsuma;
        private Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        private FsmString playerCurrentVehicle;
        private PlayMakerFSM carElectricsPower;

        //Audio
        private ModAudio turbocharger_loop_big = new ModAudio();
        private ModAudio turbocharger_blowoff = new ModAudio();
        private ModAudio turbocharger_grinding_loop = new ModAudio();

        private AudioSource turboLoopBig;
        private AudioSource turboGrindingLoop;
        private AudioSource turboBlowOffShot;
        private AudioSource backfire_fx_big_turbo_exhaust_straight;


        //Part Tightness
        private FsmFloat racingExhaustPipeTightness;
        private FsmFloat exhaustPipeTightness;

        private PlayMakerFSM turbocharger_bigFSM;
        private FsmFloat turbocharger_big_rpm;
        private FsmFloat turbocharger_big_pressure;
        private FsmFloat turbocharger_big_max_boost;
        private FsmFloat turbocharger_big_wear;
        private FsmFloat turbocharger_big_exhaust_temp;
        private FsmFloat turbocharger_big_intake_temp;
        private FsmBool turbocharger_big_allInstalled;

        private GameObject weberCarb;
        private GameObject twinCarb;
        private GameObject racingExhaustPipe;
        private GameObject steelHeaders;
        private GameObject racingExhaustMuffler;

        private GameObject exhaustPipe;
        private GameObject headers;
        private GameObject exhaustMuffler;

        private GameObject originalCylinerHead;
        
        private GameObject exhaustRaceMuffler;
        private GameObject exhaustEngine;
        private GameObject exhaustPipeRace;
        private Transform originalExhaustPipeRaceTransform;



        //Installed Objects
        private bool weberCarb_inst = false;
        private bool twinCarb_inst = false;

        //Racing exhaust system
        private bool steelHeaders_inst = false;
        private bool racingExhaustPipe_inst = false;
        private bool racingExhaustMuffler_inst = false;

        //Stock exhaust system1
        private bool headers_inst = false;
        private bool exhaustPipe_inst = false;
        private bool exhaustMuffler_inst = false;


        //Time Comparer
        private float timeSinceLastBlowOff;

        //Wear Logic
        private Random randDestroyValue;
        private float timer_wear_turbocharger_big;
        private float timer_wear_intercooler;
        private float timer_backfire;






        // Use this for initialization
        void Start()
        {
            System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
            Mod[] modsArr = mods.ToArray();
            foreach (Mod mod in modsArr)
            {
                if (mod.ID == "SatsumaTurboCharger")
                {
                    donnerTech_turbocharger_mod = (SatsumaTurboCharger) mod;
                    break;
                }
            }
            ecu_mod_installed = ModLoader.IsModPresent("DonnerTech_ECU_Mod");

            othersSave = donnerTech_turbocharger_mod.GetOthersSave();
            partsWearSave = donnerTech_turbocharger_mod.GetPartsWearSave();

            CreateTurboGrindingLoop();
            CreateTurboLoopBig();
            CreateTurboBlowoff();

            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();
            satsumaDriveTrain.clutchTorqueMultiplier = 10f;

            foreach (var playMakerFloatVar in PlayMakerGlobals.Instance.Variables.FloatVariables)
            {
                switch (playMakerFloatVar.Name)
                {
                    case "EnginePowerMultiplier":
                        {
                            powerMultiplier = playMakerFloatVar;
                            break;
                        }
                }
            }
            

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");

            GameObject electrics = GameObject.Find("CarSimulation/Car/Electrics");
            PlayMakerFSM electricsFSM = electrics.GetComponent<PlayMakerFSM>();


            turbocharger_bigFSM = this.gameObject.AddComponent<PlayMakerFSM>();
            turbocharger_bigFSM.FsmName = "SatsumaTurboCharger_Values";

            turbocharger_big_rpm = new FsmFloat("Rpm");
            turbocharger_big_pressure = new FsmFloat("Pressure");
            turbocharger_big_max_boost = new FsmFloat("Max boost");
            turbocharger_big_wear = new FsmFloat("Wear");
            turbocharger_big_exhaust_temp = new FsmFloat("Exhaust temperature");
            turbocharger_big_intake_temp = new FsmFloat("Intake temperature");
            turbocharger_big_allInstalled = new FsmBool("All installed");

            turbocharger_bigFSM.FsmVariables.FloatVariables = new FsmFloat[]
            {
                        turbocharger_big_rpm,
                        turbocharger_big_pressure,
                        turbocharger_big_max_boost,
                        turbocharger_big_wear,
                        turbocharger_big_exhaust_temp,
                        turbocharger_big_intake_temp
            };
            turbocharger_bigFSM.FsmVariables.BoolVariables = new FsmBool[]
            {
                        turbocharger_big_allInstalled
            };

            weberCarb = GameObject.Find("racing carburators(Clone)");
            twinCarb = GameObject.Find("twin carburators(Clone)");

            steelHeaders = GameObject.Find("headers(Clone)");
            racingExhaustPipe = GameObject.Find("racing exhaust(Clone)");
            racingExhaustMuffler = GameObject.Find("racing muffler(Clone)");

            headers = GameObject.Find("steel headers(Clone)");
            exhaustPipe = GameObject.Find("exhaust pipe(Clone)");
            exhaustMuffler = GameObject.Find("exhaust muffler(Clone)");

            
        }

        public void CreateTurboBlowoff()
        {
            //Creates the TurboBlowoff  loading the file "turbocharger_blowoff.wav" from the Asset folder of the mod

            turboBlowOffShot = this.gameObject.AddComponent<AudioSource>();
            turbocharger_blowoff.audioSource = turboBlowOffShot;

            turbocharger_blowoff.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_turbocharger_mod), "turbocharger_blowoff.wav"), true, false);
            turboBlowOffShot.minDistance = 1;
            turboBlowOffShot.maxDistance = 10;
            turboBlowOffShot.spatialBlend = 1;
        }

        private void CreateTurboLoopBig()
        {
            turboLoopBig = this.gameObject.AddComponent<AudioSource>();
            turbocharger_loop_big.audioSource = turboLoopBig;
            turbocharger_loop_big.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_turbocharger_mod), "turbocharger_loop.wav"), true, false);



            turboLoopBig.rolloffMode = AudioRolloffMode.Custom;
            turboLoopBig.minDistance = 1;
            turboLoopBig.maxDistance = 10;
            turboLoopBig.spatialBlend = 1;
            turboLoopBig.loop = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (hasPower && donnerTech_turbocharger_mod.GetAllOtherPartsInstalledScrewed() && donnerTech_turbocharger_mod.GetAllBigPartsInstalledScrewed() && !donnerTech_turbocharger_mod.GetAllSmallPartsInstalledScrewed())
            {
                if (!turboLoopBig.isPlaying)
                {
                    turboLoopBig.Play();
                }
                turboLoopBig.volume = satsumaDriveTrain.rpm * 0.00005f;
                turboLoopBig.pitch = satsumaDriveTrain.rpm * 0.00018f;

                HandlePartsInstalled();
                timer_wear_turbocharger_big += Time.deltaTime;
                timer_wear_intercooler += Time.deltaTime;
                timeSinceLastBlowOff += Time.deltaTime;
                RotateTurbineWheel();

                if (timeSinceLastBlowOff >= 0.3f)
                {
                    calculated_boost = CalculateTurboBoost();

                    if (calculated_boost >= 0.2f)
                    {
                        calculated_boost = HandleWear(calculated_boost);
                        HandleBackfire();

                        donnerTech_turbocharger_mod.SetBoostGaugeText(calculated_boost, true);
                        powerMultiplier.Value = (0.90f + (calculated_boost * 1.5f));
                    }
                    else
                    {
                        timer_wear_intercooler = 0;
                        timer_wear_turbocharger_big = 0;
                    }

                    
                }
                else
                {
                    calculated_boost = 0.90f;
                    powerMultiplier.Value = 0.90f;
                    donnerTech_turbocharger_mod.SetBoostGaugeText(calculated_boost, false);
                }

                if (satsumaDriveTrain.rpm >= 400)
                {
                    CheckPartsWear();
                }

                if (useThrottleButton && satsumaDriveTrain.rpm > 4000 && timeSinceLastBlowOff > 1)
                {
                    turbocharger_blowoffShotAllowed = true;
                }


                if ((!useThrottleButton && turbocharger_blowoffShotAllowed == true) && !GetALSModuleEnabled())
                {
                    donnerTech_turbocharger_mod.SetBoostGaugeText(0.10f, false);
                    TriggerBlowoff();
                }


                
                turbocharger_big_max_boost.Value = othersSave.turbocharger_big_max_boost;
                turbocharger_big_exhaust_temp.Value = 0f;
                turbocharger_big_intake_temp.Value = 0f;
                turbocharger_big_rpm.Value = 20000f;
                turbocharger_big_pressure.Value = calculated_boost;
                turbocharger_big_wear.Value = partsWearSave.turbocharger_big_wear;
                turbocharger_big_allInstalled.Value = true;
                
            }
            else if (!donnerTech_turbocharger_mod.GetAllSmallPartsInstalledScrewed())
            {
                turbocharger_loop_big.Stop();
                turbocharger_blowoff.Stop();
                turbocharger_grinding_loop.Stop();
                donnerTech_turbocharger_mod.SetBoostGaugeText("ERR");


            }
            else
            {
                turbocharger_loop_big.Stop();
                turbocharger_blowoff.Stop();
                turbocharger_grinding_loop.Stop();
            }
        }

        private void TriggerBlowoff()
        {
            turbocharger_blowoffShotAllowed = false;
            timeSinceLastBlowOff = 0;
            turbocharger_blowoff.Play();
            turboBlowOffShot.volume = 0.20f;
            
            
        }

        private void CheckPartsWear()
        {
            if (partsWearSave.turbocharger_big_wear <= 0f)
            {
                donnerTech_turbocharger_mod.removePartBigTurbo();
            }
            else if (partsWearSave.turbocharger_big_wear <= 15f)
            {

                int randVal = randDestroyValue.Next(400);
                if (randVal == 1)
                {
                    //Part should destroy
                    donnerTech_turbocharger_mod.removePartBigTurbo();
                }
            }

            if (partsWearSave.intercooler_wear <= 0f)
            {
                donnerTech_turbocharger_mod.removePartIntercooler();
            }
            else if (partsWearSave.intercooler_wear <= 15f)
            {

                int randVal = randDestroyValue.Next(400);
                if (randVal == 1)
                {
                    //Part should destroy
                    donnerTech_turbocharger_mod.removePartIntercooler();
                }
            }
        }

        private void HandlePartsInstalled()
        {
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
                steelHeaders_inst = steelHeaders.transform.parent.name == "pivot_headers";
            }
            catch
            {
                try
                {
                    steelHeaders_inst = GameObject.Find("steel headers(Clone)").transform.parent.transform.parent.name == "pivot_exhaust pipe";
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
                racingExhaustPipeTightness = racingExhaustPipe.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Tightness");
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


            try
            {
                headers_inst = headers.transform.parent.name == "pivot_headers";
            }
            catch
            {
                try
                {
                    headers_inst = GameObject.Find("headers(Clone)").transform.parent.transform.parent.name == "pivot_exhaust pipe";
                }
                catch { }
            }
            try
            {
                exhaustPipe_inst = exhaustPipe.transform.parent.name == "pivot_exhaust pipe";
            }
            catch
            {
                try
                {
                    exhaustPipe_inst = GameObject.Find("exhaust pipe(Clone)").transform.parent.transform.parent.name == "pivot_exhaust pipe";
                }
                catch { }
            }

            try
            {
                exhaustPipeTightness = exhaustPipe.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Tightness");
            }
            catch
            {

            }

            try
            {
                exhaustMuffler_inst = exhaustMuffler.transform.parent.name == "pivot_exhaust_muffler";
            }
            catch
            {
                try
                {
                    exhaustMuffler_inst = GameObject.Find("exhaust muffler(Clone)").transform.parent.transform.parent.name == "pivot_exhaust_muffler";
                }
                catch { }
            }
        }

        private float CalculateTurboBoost()
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

            if (ecu_mod_installed && GetALSModuleEnabled())
            {
                calculated_boost = Convert.ToSingle(Math.Log(satsumaDriveTrain.maxRPM / 2800, 100)) * 19f;
            }
            else
            {
                calculated_boost = Convert.ToSingle(Math.Log(satsumaDriveTrain.rpm / 4000, 100)) * 19f;
            }

            if (calculated_boost > othersSave.turbocharger_big_max_boost)
            {
                calculated_boost = othersSave.turbocharger_big_max_boost;
            }

            return calculated_boost;
        }

        private float HandleWear(float boost)
        {
            float newCalculated_boost = boost;
            if (partsWearSave.turbocharger_big_wear <= 0)
            {
                partsWearSave.turbocharger_big_wear = 0;
            }
            else if (timer_wear_turbocharger_big >= 0.5f)
            {
                timer_wear_turbocharger_big = 0;
                partsWearSave.turbocharger_big_wear -= (newCalculated_boost * 0.025f);
            }
            if (partsWearSave.turbocharger_big_wear < 25f)
            {
                if (!turboGrindingLoop.isPlaying)
                {
                    turboGrindingLoop.Play();
                }
                turboGrindingLoop.volume = satsumaDriveTrain.rpm * 0.00008f;
                turboGrindingLoop.pitch = satsumaDriveTrain.rpm * 0.00012f;
            }

            if (partsWearSave.intercooler_wear <= 0)
            {
                partsWearSave.intercooler_wear = 0;
            }
            else if (timer_wear_intercooler >= 0.5f)
            {
                timer_wear_intercooler = 0;
                partsWearSave.intercooler_wear -= (newCalculated_boost * 0.035f);
            }

            if (partsWearSave.intercooler_wear >= 75)
            {
            }
            else if (partsWearSave.intercooler_wear >= 50f)
            {
                newCalculated_boost /= 1.2f;
            }
            else if (partsWearSave.intercooler_wear >= 25f)
            {
                newCalculated_boost /= 1.4f;
            }
            else if (partsWearSave.intercooler_wear >= 15f)
            {
                newCalculated_boost /= 1.8f;
            }
            else if (partsWearSave.intercooler_wear < 15f)
            {
                newCalculated_boost = 0;
            }
            return newCalculated_boost;
        }

        private void HandleBackfire()
        {
            timer_backfire += Time.deltaTime;
            if(timer_backfire >= 0.1f)
            {
                if (satsumaDriveTrain.rpm >= 4000 && !useThrottleButton)
                {
                    canBackfire = true;
                    if (ecu_mod_installed && GetALSModuleEnabled())
                    {
                        Random randomShouldBackfire = new Random();
                        if (randomShouldBackfire.Next(3) == 1)
                        {
                            timer_backfire = 0;
                            TriggerBackfire();
                        }

                    }
                    else if (canBackfire)
                    {
                        Random randomShouldBackfire = new Random();
                        if (randomShouldBackfire.Next(20) == 1)
                        {
                            timer_backfire = 0;
                            TriggerBackfire();
                        }
                    }
                }
            }
            if (satsumaDriveTrain.rpm <= 3500)
            {
                canBackfire = false;
            }
        }

        private void TriggerBackfire()
        {
            donnerTech_turbocharger_mod.turbocharger_big_exhaust_outlet_straight_logic.TriggerBackfire();
        }

        private void CreateTurboGrindingLoop()
        {
            turboGrindingLoop = this.gameObject.AddComponent<AudioSource>();
            turbocharger_grinding_loop.audioSource = turboGrindingLoop;
            turbocharger_grinding_loop.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_turbocharger_mod), "grinding sound.wav"), true, false);

            turboGrindingLoop.rolloffMode = AudioRolloffMode.Linear;
            turboGrindingLoop.minDistance = 1;
            turboGrindingLoop.maxDistance = 10;
            turboGrindingLoop.spatialBlend = 0.6f;
            turboGrindingLoop.loop = true;
        }


        private bool GetALSModuleEnabled()
        {
            bool ecu_mod_alsEnabled = false;
            if (ecu_mod_SmartEngineModule == null)
            {
                ecu_mod_SmartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
            }
            if (ecu_mod_SmartEngineModule != null)
            {
                Component ecu_mod_ModCommunication = ecu_mod_SmartEngineModule.GetComponent("ModCommunication");
                Type compType = ecu_mod_ModCommunication.GetType();
                FieldInfo alsEnabledInfo = compType.GetField("alsEnabled");
                ecu_mod_alsEnabled = (bool) alsEnabledInfo.GetValue(ecu_mod_ModCommunication);
            }
            if (satsumaDriveTrain.rpm < 4000 && ecu_mod_alsEnabled)
            {
                ecu_mod_alsEnabled = false;
            }
            return ecu_mod_alsEnabled;
        }

        private void RotateTurbineWheel()
        {
            if (turbocharger_big_turbine == null)
            {
                turbocharger_big_turbine = GameObject.Find("TurboCharger_Big_Compressor_Turbine");
            }
            if (turbocharger_big_turbine != null)
            {
                turbocharger_big_turbine.transform.Rotate(0, 0, (satsumaDriveTrain.rpm / 500));
            }
        }

        private bool hasPower
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
        internal static bool useButtonDown
        {
            get
            {
                return cInput.GetKeyDown("Use");
            }
        }
        internal static bool useThrottleButton
        {
            get
            {
                return cInput.GetKey("Throttle");
            }
        }
    }
}