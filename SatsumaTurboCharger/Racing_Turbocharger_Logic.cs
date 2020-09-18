using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.IO;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger
{
    public class Racing_Turbocharger_Logic : MonoBehaviour
    {
        private SatsumaTurboCharger donnerTech_turbocharger_mod;
        
        //Turbo mod
        private GameObject turbocharger_big_turbine;

        private BoostSave boostSave;
        private PartsWearSave partsWearSave;

        private float calculated_boost = 0;

        private bool canBackfire = false;
        private FsmFloat powerMultiplier;
        private bool turbocharger_blowoffShotAllowed = false;

        //ECU Mod
        private bool ecu_mod_installed = false;
        private PlayMakerFSM smart_engine_moduleFSM;
        private FsmBool smart_engine_module_allInstalled;
        private FsmBool smart_engine_module_alsModuleEnabled;
        private FsmBool smart_engine_module_step2RevLimiterModuleEnabled;

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

        //Big turbo playmakerFSM
        private PlayMakerFSM turbocharger_bigFSM;
        private FsmFloat turbocharger_big_rpm;
        private FsmFloat turbocharger_big_pressure;
        private FsmFloat turbocharger_big_max_boost;
        private FsmFloat turbocharger_big_wear;
        private FsmFloat turbocharger_big_exhaust_temp;
        private FsmFloat turbocharger_big_intake_temp;
        private FsmBool turbocharger_big_allInstalled;

        //Installed Objects
        private FsmBool weberCarb_inst;
        private FsmBool twinCarb_inst;

        //Time Comparer
        private float timeSinceLastBlowOff;
        private float timer_delay_turbocharger_big;

        //Turbo delay
        private float turbocharger_delay;


        //Wear Logic
        private Random randDestroyValue;
        private float timer_wear_turbocharger_big;
        private float timer_wear_intercooler;
        private float timer_backfire;

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

            boostSave = donnerTech_turbocharger_mod.GetBoostSave();
            partsWearSave = donnerTech_turbocharger_mod.GetPartsWearSave();

            CreateTurboGrindingLoop();
            CreateTurboLoopBig();
            CreateTurboBlowoff();

            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();
            satsumaDriveTrain.clutchTorqueMultiplier = 10f;

            weberCarb_inst = GameObject.Find("Racing Carburators").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
            twinCarb_inst = GameObject.Find("Twin Carburators").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");

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

        void Update()
        {
            if (ModLoader.IsModPresent("DonnerTech_ECU_Mod"))
            {
                if (smart_engine_moduleFSM == null)
                {
                    try
                    {
                        GameObject smartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
                        if (smartEngineModule != null)
                        {
                            smart_engine_moduleFSM = smartEngineModule.GetComponent<PlayMakerFSM>();
                            if (smart_engine_moduleFSM != null)
                            {
                                smart_engine_module_allInstalled = smart_engine_moduleFSM.FsmVariables.FindFsmBool("All installed");
                                smart_engine_module_alsModuleEnabled = smart_engine_moduleFSM.FsmVariables.FindFsmBool("ALS Enabled");
                                smart_engine_module_step2RevLimiterModuleEnabled = smart_engine_moduleFSM.FsmVariables.FindFsmBool("Step2RevLimiter Enabled");
                            }
                        }

                    }
                    catch
                    {

                    }
                }
            }

            if (hasPower && donnerTech_turbocharger_mod.GetAllOtherPartsInstalledScrewed() && donnerTech_turbocharger_mod.GetAllBigPartsInstalledScrewed() && !donnerTech_turbocharger_mod.GetAllSmallPartsInstalledScrewed())
            {
                if (!turboLoopBig.isPlaying)
                {
                    turboLoopBig.Play();
                }
                turboLoopBig.volume = satsumaDriveTrain.rpm * 0.00005f;
                turboLoopBig.pitch = satsumaDriveTrain.rpm * 0.00018f;

                timer_wear_turbocharger_big += Time.deltaTime;
                timer_wear_intercooler += Time.deltaTime;
                timeSinceLastBlowOff += Time.deltaTime;
                RotateTurbineWheel();

                if (timeSinceLastBlowOff >= 0.8f)
                {
                    calculated_boost = CalculateTurboBoost();
                    calculated_boost = HandleTurboDelay(calculated_boost, 0.1f, 0.4f);
                    if (calculated_boost > 0f)
                    {
                        if (SatsumaTurboCharger.GetPartsWearEnabled())
                        {
                            calculated_boost = HandleWear(calculated_boost);
                        }
                        HandleBackfire();

                        donnerTech_turbocharger_mod.SetBoostGaugeText(calculated_boost, true);
                        powerMultiplier.Value = (0.90f + (calculated_boost * 1.5f));
                    }
                    else
                    {
                        timer_wear_intercooler = 0;
                        timer_wear_turbocharger_big = 0;
                        calculated_boost = -0.10f;
                        powerMultiplier.Value = 1f - 0.10f;
                        donnerTech_turbocharger_mod.SetBoostGaugeText(0.10f, false);
                    }

                    
                }

                if(timeSinceLastBlowOff < 0.8f || turbocharger_delay <= 0)
                {
                    calculated_boost = -0.10f;
                    powerMultiplier.Value = 1f - 0.10f;
                    donnerTech_turbocharger_mod.SetBoostGaugeText(0.10f, false);
                }

                if (satsumaDriveTrain.rpm >= 400)
                {
                    if (SatsumaTurboCharger.GetPartsWearEnabled())
                    {
                        CheckPartsWear();
                    }
                    
                }

                if (useThrottleButton && satsumaDriveTrain.rpm > 4000 && timeSinceLastBlowOff > 1)
                {
                    turbocharger_blowoffShotAllowed = true;
                }


                if ((!useThrottleButton && turbocharger_blowoffShotAllowed == true) && calculated_boost >= 0.6f)
                {
                    donnerTech_turbocharger_mod.SetBoostGaugeText(0.10f, false);
                    TriggerBlowoff();
                }

                turbocharger_big_max_boost.Value = boostSave.turbocharger_big_max_boost;
                turbocharger_big_exhaust_temp.Value = 0f;
                turbocharger_big_intake_temp.Value = 0f;
                turbocharger_big_rpm.Value = CalculateRpm(calculated_boost);
                turbocharger_big_pressure.Value = calculated_boost;
                turbocharger_big_wear.Value = partsWearSave.turbocharger_big_wear;
                turbocharger_big_allInstalled.Value = true;
                
            }
            else if (!donnerTech_turbocharger_mod.GetAllSmallPartsInstalledScrewed())
            {
                turbocharger_big_max_boost.Value = boostSave.turbocharger_big_max_boost;
                turbocharger_big_exhaust_temp.Value = 0f;
                turbocharger_big_intake_temp.Value = 0f;
                turbocharger_big_rpm.Value = 0;
                turbocharger_big_pressure.Value = 0;
                turbocharger_big_allInstalled.Value = false;

                turbocharger_loop_big.Stop();
                turbocharger_blowoff.Stop();
                turbocharger_grinding_loop.Stop();
                donnerTech_turbocharger_mod.SetBoostGaugeText("ERR");


            }
            else
            {
                turbocharger_big_max_boost.Value = boostSave.turbocharger_big_max_boost;
                turbocharger_big_exhaust_temp.Value = 0f;
                turbocharger_big_intake_temp.Value = 0f;
                turbocharger_big_rpm.Value = 0;
                turbocharger_big_pressure.Value = 0;
                turbocharger_big_allInstalled.Value = false;

                turbocharger_loop_big.Stop();
                turbocharger_blowoff.Stop();
                turbocharger_grinding_loop.Stop();
            }
        }

        private float HandleTurboDelay(float calculated_boost, float delay_comparer, float delayAdder)
        {
            timer_delay_turbocharger_big += Time.deltaTime;
            if (useThrottleButton || throttleUsed)
            {
                if (timer_delay_turbocharger_big >= delay_comparer)
                {
                    timer_delay_turbocharger_big = 0;
                    turbocharger_delay += delayAdder;
                    if (turbocharger_delay >= 1)
                        turbocharger_delay = 1;
                }
            }
            else if(!useThrottleButton && !throttleUsed)
            {
                if (timer_delay_turbocharger_big >= delay_comparer)
                {
                    timer_delay_turbocharger_big = 0;
                    turbocharger_delay -= (0.1f / 4);
                    if (turbocharger_delay <= 0)
                        turbocharger_delay = 0;
                }
            }

            return calculated_boost * turbocharger_delay;
        }

        private float CalculateRpm(float calculated_boost)
        {
            float new_calculated_rpm = calculated_boost * 48000f;
            if (new_calculated_rpm <= 0)
            {
                new_calculated_rpm = 0;
            }
            return new_calculated_rpm;
        }

        private void TriggerBlowoff()
        {
            turbocharger_blowoffShotAllowed = false;
            timeSinceLastBlowOff = 0;
            turbocharger_blowoff.Play();
            float pitch = Mathf.Clamp(calculated_boost, 0.8f, 1.2f);
            turbocharger_blowoff.audioSource.pitch = pitch;
        }

        private void CheckPartsWear()
        {
            if (partsWearSave.turbocharger_big_wear <= 0f)
            {
                donnerTech_turbocharger_mod.removePartBigTurbo();
            }
            else if (partsWearSave.turbocharger_big_wear <= 15f)
            {

                int randVal = randDestroyValue.Next(100);
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

                int randVal = randDestroyValue.Next(100);
                if (randVal == 1)
                {
                    //Part should destroy
                    donnerTech_turbocharger_mod.removePartIntercooler();
                }
            }
        }

        private float CalculateTurboBoost()
        {
            if (twinCarb_inst.Value)
            {
                boostSave.turbocharger_big_max_boost_limit = (2.2f + 0.05f);
            }
            else if (weberCarb_inst.Value)
            {
                boostSave.turbocharger_big_max_boost_limit = (2.2f + 0.30f);
            }
            if (boostSave.turbocharger_big_max_boost >= boostSave.turbocharger_big_max_boost_limit)
            {
                boostSave.turbocharger_big_max_boost = boostSave.turbocharger_big_max_boost_limit;
            }

            if (ecu_mod_installed && smart_engine_module_alsModuleEnabled != null && smart_engine_module_alsModuleEnabled.Value && satsumaDriveTrain.rpm >= satsumaDriveTrain.maxRPM)
            {
                calculated_boost = Convert.ToSingle(Math.Log(10000 / 4000, 100)) * 19f;
            }
            else
            {
                calculated_boost = Convert.ToSingle(Math.Log(satsumaDriveTrain.rpm / 4000, 100)) * 19f;
            }

            if (calculated_boost > boostSave.turbocharger_big_max_boost)
            {
                calculated_boost = boostSave.turbocharger_big_max_boost;
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
                partsWearSave.turbocharger_big_wear -= (newCalculated_boost * 0.003f);
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
                partsWearSave.intercooler_wear -= (newCalculated_boost * 0.005f);
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

            if (smart_engine_module_alsModuleEnabled != null && smart_engine_module_alsModuleEnabled.Value)
            {
                if (satsumaDriveTrain.rpm >= 4000 && useThrottleButton && satsumaDriveTrain.revLimiterTriggered)
                {
                    timer_backfire = 0;
                    TriggerBackfire();
                }
            }
            else if (timer_backfire >= 0.1f)
            {
                if (satsumaDriveTrain.rpm >= 4000 && !useThrottleButton)
                {
                    if (canBackfire)
                    {
                        Random randomShouldBackfire = new Random();
                        if (randomShouldBackfire.Next(20) == 1)
                        {
                            timer_backfire = 0;
                            TriggerBackfire();
                            canBackfire = false;
                        }
                    }
                }
            }


            if (useThrottleButton)
            {
                canBackfire = true;
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
        internal bool useButtonDown
        {
            get
            {
                return cInput.GetKeyDown("Use");
            }
        }
        internal bool useThrottleButton
        {
            get
            {
                return cInput.GetKey("Throttle");
            }
        }
        internal bool throttleUsed
        {
            get
            {
                return (satsumaDriveTrain.idlethrottle > 0f);
            }
        }
    }
}