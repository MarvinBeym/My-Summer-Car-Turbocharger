using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger
{
    [Obsolete("Replaced by generic Turbo_Logic class", true)]
    public class Racing_Turbocharger_Logic : MonoBehaviour
    {
        private SatsumaTurboCharger mod;
        
        //Turbo mod
        private GameObject turboBig_turbine;


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
        private PlayMakerFSM turboBigFSM;
        private FsmFloat turboBig_rpm;
        private FsmFloat turboBig_pressure;
        private FsmFloat turboBig_max_boost;
        private FsmFloat turboBig_wear;
        private FsmFloat turboBig_exhaust_temp;
        private FsmFloat turboBig_intake_temp;
        private FsmBool turboBig_allInstalled;

        //Installed Objects
        private FsmBool weberCarb_inst;
        private FsmBool twinCarb_inst;

        //Time Comparer
        private float timeSinceLastBlowOff;
        private float timer_delay_turboBig;

        //Turbo delay
        private float turbocharger_delay;


        //Wear Logic
        private Random randDestroyValue;
        private float timer_wear_turboBig;
        private float timer_wear_intercooler;
        private float timer_backfire;

        void Start()
        {
            ecu_mod_installed = ModLoader.IsModPresent("SatsumaTurboCharger");

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


            turboBigFSM = this.gameObject.AddComponent<PlayMakerFSM>();
            turboBigFSM.FsmName = "SatsumaTurboCharger_Values";

            turboBig_rpm = new FsmFloat("Rpm");
            turboBig_pressure = new FsmFloat("Pressure");
            turboBig_max_boost = new FsmFloat("Max boost");
            turboBig_wear = new FsmFloat("Wear");
            turboBig_exhaust_temp = new FsmFloat("Exhaust temperature");
            turboBig_intake_temp = new FsmFloat("Intake temperature");
            turboBig_allInstalled = new FsmBool("All installed");

            turboBigFSM.FsmVariables.FloatVariables = new FsmFloat[]
            {
                turboBig_rpm,
                turboBig_pressure,
                turboBig_max_boost,
                turboBig_wear,
                turboBig_exhaust_temp,
                turboBig_intake_temp
            };
            turboBigFSM.FsmVariables.BoolVariables = new FsmBool[]
            {
                turboBig_allInstalled
            };
        }

        public void CreateTurboBlowoff()
        {
            //Creates the TurboBlowoff  loading the file "turbocharger_blowoff.wav" from the Asset folder of the mod

            turboBlowOffShot = this.gameObject.AddComponent<AudioSource>();
            turbocharger_blowoff.audioSource = turboBlowOffShot;

            turbocharger_blowoff.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "turbocharger_blowoff.wav"), true, false);
            turboBlowOffShot.minDistance = 1;
            turboBlowOffShot.maxDistance = 10;
            turboBlowOffShot.spatialBlend = 1;
        }

        private void CreateTurboLoopBig()
        {
            turboLoopBig = this.gameObject.AddComponent<AudioSource>();
            turbocharger_loop_big.audioSource = turboLoopBig;
            turbocharger_loop_big.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "turbocharger_loop.wav"), true, false);



            turboLoopBig.rolloffMode = AudioRolloffMode.Custom;
            turboLoopBig.minDistance = 1;
            turboLoopBig.maxDistance = 10;
            turboLoopBig.spatialBlend = 1;
            turboLoopBig.loop = true;
        }

        void Update()
        {
            bool allBigInstalled = mod.AllBigInstalled();
            bool allSmallInstalled = mod.AllSmallInstalled();
            bool allOtherInstalled = mod.AllOtherInstalled();
            if (ModLoader.IsModPresent("SatsumaTurboCharger"))
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

            if (hasPower && allOtherInstalled && allBigInstalled && !allSmallInstalled)
            {
                if (!turboLoopBig.isPlaying)
                {
                    turboLoopBig.Play();
                }
                turboLoopBig.volume = satsumaDriveTrain.rpm * 0.00005f;
                turboLoopBig.pitch = satsumaDriveTrain.rpm * 0.00018f;

                timer_wear_turboBig += Time.deltaTime;
                timer_wear_intercooler += Time.deltaTime;
                timeSinceLastBlowOff += Time.deltaTime;
                RotateTurbineWheel();
                
                if (timeSinceLastBlowOff >= 0.8f)
                {
                    calculated_boost = CalculateTurboBoost();
                    calculated_boost = HandleTurboDelay(calculated_boost, 0.1f, 0.4f);
                    if (calculated_boost > 0f)
                    {
                        if ((bool)mod.partsWearSetting.Value) { calculated_boost = HandleWear(calculated_boost); }
                        HandleBackfire();

                        mod.SetBoostGaugeText(calculated_boost.ToString("0.00"));
                        powerMultiplier.Value = (0.90f + (calculated_boost * 1.5f));
                    }
                    else
                    {
                        timer_wear_intercooler = 0;
                        timer_wear_turboBig = 0;
                        calculated_boost = -0.10f;
                        powerMultiplier.Value = 1f - 0.10f;
                        mod.SetBoostGaugeText(0.10f.ToString("0.00"));
                    }

                    
                }

                if(timeSinceLastBlowOff < 0.8f || turbocharger_delay <= 0)
                {
                    calculated_boost = -0.10f;
                    powerMultiplier.Value = 1f - 0.10f;
                    mod.SetBoostGaugeText(0.10f.ToString("0.00"));
                }

                if (satsumaDriveTrain.rpm >= 400)
                {
                    if ((bool)mod.partsWearSetting.Value) CheckPartsWear();

                }

                if (useThrottleButton && satsumaDriveTrain.rpm > 4000 && timeSinceLastBlowOff > 1)
                {
                    turbocharger_blowoffShotAllowed = true;
                }


                if ((!useThrottleButton && turbocharger_blowoffShotAllowed == true) && calculated_boost >= 0.6f)
                {
                    mod.SetBoostGaugeText(0.10f.ToString("0.00"));
                    TriggerBlowoff();
                }

                turboBig_max_boost.Value = mod.boostSave.turboBig_max_boost;
                turboBig_exhaust_temp.Value = 0f;
                turboBig_intake_temp.Value = 0f;
                turboBig_rpm.Value = CalculateRpm(calculated_boost);
                turboBig_pressure.Value = calculated_boost;
                turboBig_wear.Value = mod.partsWearSave.turboBig_wear;
                turboBig_allInstalled.Value = true;
                
            }
            else if (!allSmallInstalled)
            {
                turboBig_max_boost.Value = mod.boostSave.turboBig_max_boost;
                turboBig_exhaust_temp.Value = 0f;
                turboBig_intake_temp.Value = 0f;
                turboBig_rpm.Value = 0;
                turboBig_pressure.Value = 0;
                turboBig_allInstalled.Value = false;

                turbocharger_loop_big.Stop();
                turbocharger_blowoff.Stop();
                turbocharger_grinding_loop.Stop();
                mod.SetBoostGaugeText("ERR");


            }
            else
            {
                turboBig_max_boost.Value = mod.boostSave.turboBig_max_boost;
                turboBig_exhaust_temp.Value = 0f;
                turboBig_intake_temp.Value = 0f;
                turboBig_rpm.Value = 0;
                turboBig_pressure.Value = 0;
                turboBig_allInstalled.Value = false;

                turbocharger_loop_big.Stop();
                turbocharger_blowoff.Stop();
                turbocharger_grinding_loop.Stop();
            }
        }

        private float HandleTurboDelay(float calculated_boost, float delay_comparer, float delayAdder)
        {
            timer_delay_turboBig += Time.deltaTime;
            if (useThrottleButton || throttleUsed)
            {
                if (timer_delay_turboBig >= delay_comparer)
                {
                    timer_delay_turboBig = 0;
                    turbocharger_delay += delayAdder;
                    if (turbocharger_delay >= 1)
                        turbocharger_delay = 1;
                }
            }
            else if(!useThrottleButton && !throttleUsed)
            {
                if (timer_delay_turboBig >= delay_comparer)
                {
                    timer_delay_turboBig = 0;
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
            if (mod.partsWearSave.turboBig_wear <= 0f)
            {
                mod.turboBig_part.removePart();
            }
            else if (mod.partsWearSave.turboBig_wear <= 15f)
            {

                int randVal = randDestroyValue.Next(100);
                if (randVal == 1)
                {
                    //Part should destroy
                    mod.turboBig_part.removePart();
                }
            }

            if (mod.partsWearSave.intercooler_wear <= 0f)
            {
                mod.intercooler_part.removePart();
            }
            else if (mod.partsWearSave.intercooler_wear <= 15f)
            {

                int randVal = randDestroyValue.Next(100);
                if (randVal == 1)
                {
                    //Part should destroy
                    mod.intercooler_part.removePart();
                }
            }
        }

        private float CalculateTurboBoost()
        {
            if (twinCarb_inst.Value)
            {
                mod.boostSave.turboBig_max_boost_limit = (2.2f + 0.05f);
            }
            else if (weberCarb_inst.Value)
            {
                mod.boostSave.turboBig_max_boost_limit = (2.2f + 0.30f);
            }
            if (mod.boostSave.turboBig_max_boost >= mod.boostSave.turboBig_max_boost_limit)
            {
                mod.boostSave.turboBig_max_boost = mod.boostSave.turboBig_max_boost_limit;
            }

            if (ecu_mod_installed && smart_engine_module_alsModuleEnabled != null && smart_engine_module_alsModuleEnabled.Value && satsumaDriveTrain.rpm >= satsumaDriveTrain.maxRPM)
            {
                calculated_boost = Convert.ToSingle(Math.Log(10000 / 4000, 100)) * 19f;
            }
            else
            {
                calculated_boost = Convert.ToSingle(Math.Log(satsumaDriveTrain.rpm / 4000, 100)) * 19f;
            }

            if (calculated_boost > mod.boostSave.turboBig_max_boost)
            {
                calculated_boost = mod.boostSave.turboBig_max_boost;
            }

            return calculated_boost;
        }

        private float HandleWear(float boost)
        {
            float newCalculated_boost = boost;
            if (mod.partsWearSave.turboBig_wear <= 0)
            {
                mod.partsWearSave.turboBig_wear = 0;
            }
            else if (timer_wear_turboBig >= 0.5f)
            {
                timer_wear_turboBig = 0;
                mod.partsWearSave.turboBig_wear -= (newCalculated_boost * 0.003f);
            }
            if (mod.partsWearSave.turboBig_wear < 25f)
            {
                if (!turboGrindingLoop.isPlaying)
                {
                    turboGrindingLoop.Play();
                }
                turboGrindingLoop.volume = satsumaDriveTrain.rpm * 0.00008f;
                turboGrindingLoop.pitch = satsumaDriveTrain.rpm * 0.00012f;
            }

            if (mod.partsWearSave.intercooler_wear <= 0)
            {
                mod.partsWearSave.intercooler_wear = 0;
            }
            else if (timer_wear_intercooler >= 0.5f)
            {
                timer_wear_intercooler = 0;
                mod.partsWearSave.intercooler_wear -= (newCalculated_boost * 0.005f);
            }
            
            if (mod.partsWearSave.intercooler_wear >= 75)
            {
            }
            else if (mod.partsWearSave.intercooler_wear >= 50f)
            {
                newCalculated_boost /= 1.2f;
            }
            else if (mod.partsWearSave.intercooler_wear >= 25f)
            {
                newCalculated_boost /= 1.4f;
            }
            else if (mod.partsWearSave.intercooler_wear >= 15f)
            {
                newCalculated_boost /= 1.8f;
            }
            else if (mod.partsWearSave.intercooler_wear < 15f)
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
            //mod.turboBig_exhaust_outlet_straight_logic.TriggerBackfire();
        }

        private void CreateTurboGrindingLoop()
        {
            turboGrindingLoop = this.gameObject.AddComponent<AudioSource>();
            turbocharger_grinding_loop.audioSource = turboGrindingLoop;
            turbocharger_grinding_loop.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "grinding sound.wav"), true, false);

            turboGrindingLoop.rolloffMode = AudioRolloffMode.Linear;
            turboGrindingLoop.minDistance = 1;
            turboGrindingLoop.maxDistance = 10;
            turboGrindingLoop.spatialBlend = 0.6f;
            turboGrindingLoop.loop = true;
        }

        private void RotateTurbineWheel()
        {
            if (turboBig_turbine == null)
            {
                turboBig_turbine = GameObject.Find("TurboCharger_Big_Compressor_Turbine");
            }
            if (turboBig_turbine != null)
            {
                turboBig_turbine.transform.Rotate(0, 0, (satsumaDriveTrain.rpm / 500));
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

        public void Init(SatsumaTurboCharger mod)
        {
            this.mod = mod;
        }
    }
}