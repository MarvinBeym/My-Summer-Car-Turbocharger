using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.IO;
using System.Reflection;
using Tools;
using UnityEngine;
using Random = System.Random;


namespace SatsumaTurboCharger
{
    public class GT_Turbocharger_Logic : MonoBehaviour
    {
        private SatsumaTurboCharger mod;

        //Boost calculation
        private float calculated_boost = 0;
        private FsmFloat powerMultiplier;
        private bool turbocharger_blowoffShotAllowed = false;

        //ECU mod communication
        private bool ecu_mod_installed = false;
        private PlayMakerFSM smart_engine_moduleFSM;
        private FsmBool smart_engine_module_allInstalled;
        private FsmBool smart_engine_module_alsModuleEnabled;
        private FsmBool smart_engine_module_step2RevLimiterModuleEnabled;

        private FsmString playerCurrentVehicle;

        //Audio
        private ModAudio turbocharger_loop_small = new ModAudio();
        private ModAudio turbocharger_grinding_loop = new ModAudio();

        private AudioSource turboLoopSmall;
        private AudioSource turboGrindingLoop;

        //Small turbo playmakerFSM
        private PlayMakerFSM turboSmallFSM;
        private FsmFloat turboSmall_rpm;
        private FsmFloat turboSmall_pressure;
        private FsmFloat turboSmall_max_boost;
        private FsmFloat turboSmall_wear;
        private FsmFloat turboSmall_exhaust_temp;
        private FsmFloat turboSmall_intake_temp;
        private FsmBool turboSmall_allInstalled;

        //Installed Objects
        private FsmBool weberCarb_inst;
        private FsmBool twinCarb_inst;

        //Time Comparer
        private float timeSinceLastBlowOff;
        private float timer_delay_turboSmall;

        //Turbo delay
        private float turbocharger_delay;

        //Wear Logic
        private Random randDestroyValue;
        private float timer_wear_turboSmall;
        private float timer_wear_intercooler;
        private float timer_wear_airfilter;

        void Start(){ 

            ecu_mod_installed = ModLoader.IsModPresent("SatsumaTurboCharger");

            CreateTurboGrindingLoop();
            CreateTurboLoopSmall();

            Tools.CarH.drivetrain.clutchTorqueMultiplier = 10f;

            weberCarb_inst = Game.Find("Racing Carburators").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");
            twinCarb_inst = Game.Find("Twin Carburators").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Installed");

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


            turboSmallFSM = this.gameObject.AddComponent<PlayMakerFSM>();
            turboSmallFSM.FsmName = "SatsumaTurboCharger_Values";

            turboSmall_rpm = new FsmFloat("Rpm");
            turboSmall_pressure = new FsmFloat("Pressure");
            turboSmall_max_boost = new FsmFloat("Max boost");
            turboSmall_wear = new FsmFloat("Wear");
            turboSmall_exhaust_temp = new FsmFloat("Exhaust temperature");
            turboSmall_intake_temp = new FsmFloat("Intake temperature");
            turboSmall_allInstalled = new FsmBool("All installed");

            turboSmallFSM.FsmVariables.FloatVariables = new FsmFloat[]
            {
                        turboSmall_rpm,
                        turboSmall_pressure,
                        turboSmall_max_boost,
                        turboSmall_wear,
                        turboSmall_exhaust_temp,
                        turboSmall_intake_temp
            };
            turboSmallFSM.FsmVariables.BoolVariables = new FsmBool[]
            {
                        turboSmall_allInstalled
            };
        }

        // Update is called once per frame
        void Update(){
            /*
            bool allBigInstalled = mod.AllBigInstalled();
            bool allSmallInstalled = mod.AllSmallInstalled();
            bool allOtherInstalled = mod.AllOtherInstalled();

            if (ecu_mod_installed)
            {
                if (smart_engine_moduleFSM == null)
                {
                    try
                    {
                        GameObject smartEngineModule = Game.Find("Smart Engine ECU(Clone)");
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

            if (hasPower && mod.turboSmall_airfilter_part.installed && !mod.turboSmall_airfilter_part.screwablePart.partFixed)
            {
                turboSmall_max_boost.Value = mod.boostSave.turboBig_max_boost;
                turboSmall_exhaust_temp.Value = 0f;
                turboSmall_intake_temp.Value = 0f;
                turboSmall_rpm.Value = 0;
                turboSmall_pressure.Value = 0;
                turboSmall_allInstalled.Value = false;

                turbocharger_loop_small.Stop();
                turbocharger_grinding_loop.Stop();
                mod.SetBoostGaugeText("ERR");
            }
            else if (hasPower && allOtherInstalled && allSmallInstalled && !allBigInstalled)
            {
                if (!turboLoopSmall.isPlaying)
                {
                    turboLoopSmall.Play();
                }
                if (mod.turboSmall_airfilter_part.InstalledScrewed())
                {
                    turboLoopSmall.volume = satsumaDriveTrain.rpm * 0.00003f;
                    turboLoopSmall.pitch = (satsumaDriveTrain.rpm - 500) * 0.0003f;
                }
                else
                {
                    turboLoopSmall.volume = satsumaDriveTrain.rpm * 0.000026f;
                    turboLoopSmall.pitch = (satsumaDriveTrain.rpm - 500) * 0.00045f;
                }

                timer_wear_turboSmall += Time.deltaTime;
                timer_wear_intercooler += Time.deltaTime;
                timer_wear_airfilter += Time.deltaTime;
                timeSinceLastBlowOff += Time.deltaTime;


                if (timeSinceLastBlowOff >= 0.2f)
                {
                    calculated_boost = CalculateTurboBoost();
                    calculated_boost = HandleTurboDelay(calculated_boost, 0.1f, 0.4f);

                    if (calculated_boost > 0f)
                    {
                        if ((bool)mod.partsWearSetting.Value) { calculated_boost = HandleWear(calculated_boost); }
                        mod.SetBoostGaugeText(calculated_boost.ToString("0.00"));
                        powerMultiplier.Value = (0.90f + (calculated_boost * 1.5f));
                    }
                    else
                    {
                        timer_wear_turboSmall = 0;
                        timer_wear_intercooler = 0;
                        timer_wear_airfilter = 0;
                        calculated_boost = -0.04f;
                        powerMultiplier.Value = 1f - 0.04f;
                        mod.SetBoostGaugeText(0.04f.ToString("0.00"));
                    }


                }

                if(timeSinceLastBlowOff < 0.2f || turbocharger_delay <= 0)
                {
                    calculated_boost = -0.04f;
                    powerMultiplier.Value = 1f - 0.04f;
                    mod.SetBoostGaugeText(0.04f.ToString("0.00"));
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

                    if (ecu_mod_installed && smart_engine_module_alsModuleEnabled != null)
                    {
                        mod.SetBoostGaugeText(0.10f.ToString("0.00"));
                        TriggerBlowoff();
                    }
                }
            }
            else if (!allBigInstalled)
            {
                turboSmall_max_boost.Value = mod.boostSave.turboBig_max_boost;
                turboSmall_exhaust_temp.Value = 0f;
                turboSmall_intake_temp.Value = 0f;
                turboSmall_rpm.Value = 0;
                turboSmall_pressure.Value = 0;
                turboSmall_allInstalled.Value = false;

                turbocharger_loop_small.Stop();
                turbocharger_grinding_loop.Stop();
                mod.SetBoostGaugeText("ERR");


            }
            else
            {
                turboSmall_max_boost.Value = mod.boostSave.turboBig_max_boost;
                turboSmall_exhaust_temp.Value = 0f;
                turboSmall_intake_temp.Value = 0f;
                turboSmall_rpm.Value = 0;
                turboSmall_pressure.Value = 0;
                turboSmall_allInstalled.Value = false;

                turbocharger_loop_small.Stop();
                turbocharger_grinding_loop.Stop();
            }
            */
        }

        private float HandleTurboDelay(float calculated_boost, float delay_comparer, float delayAdder){ 
            timer_delay_turboSmall += Time.deltaTime;
            if (useThrottleButton || throttleUsed)
            {
                if (timer_delay_turboSmall >= delay_comparer)
                {
                    timer_delay_turboSmall = 0;
                    turbocharger_delay += delayAdder;
                    if (turbocharger_delay >= 1)
                        turbocharger_delay = 1;
                }
            }
            else if (!useThrottleButton && !throttleUsed)
            {
                if (timer_delay_turboSmall >= delay_comparer)
                {
                    timer_delay_turboSmall = 0;
                    turbocharger_delay -= (0.1f / 4);
                    if (turbocharger_delay <= 0)
                        turbocharger_delay = 0;
                }
            }

            return calculated_boost * turbocharger_delay;
        }

        private void TriggerBlowoff(){ 
            turbocharger_blowoffShotAllowed = false;
            timeSinceLastBlowOff = 0;
        }

        private void CheckPartsWear(){ 
            /*
            if (mod.partsWearSave.turboSmall_wear <= 0f)
            {
                mod.turboSmall_part.removePart();
            }
            else if (mod.partsWearSave.turboSmall_wear <= 15f)
            {

                int randVal = randDestroyValue.Next(100);
                if (randVal == 1)
                {
                    //Part should destroy
                    mod.turboSmall_part.removePart();
                }
            }
            if (mod.turboSmall_airfilter_part.InstalledScrewed())
            {
                if (mod.partsWearSave.airfilter_wear <= 0f)
                {
                    mod.turboSmall_airfilter_part.removePart();
                }
                else if (mod.partsWearSave.airfilter_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(100);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        mod.turboSmall_airfilter_part.removePart();
                    }
                }
            }

            if (mod.turboSmall_airfilter_part.InstalledScrewed())
            {
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
            */
        }

        private float CalculateRpm(float calculated_boost){ 
            float new_calculated_rpm = calculated_boost * 48000f;
            if(new_calculated_rpm <= 0)
            {
                new_calculated_rpm = 0;
            }
            return new_calculated_rpm;
        }

        private float HandleWear(float boost){ 
            float newCalculated_boost = boost;
            /*
            if (mod.partsWearSave.turboSmall_wear <= 0)
            {
                mod.partsWearSave.turboSmall_wear = 0;
            }
            else if (timer_wear_turboSmall >= 0.5f)
            {
                timer_wear_turboSmall = 0;
                if (mod.turboSmall_airfilter_part.InstalledScrewed())
                {
                    mod.partsWearSave.turboSmall_wear -= (newCalculated_boost * (0.003f - (mod.partsWearSave.airfilter_wear / 60000)));
                }
                else
                {
                    mod.partsWearSave.turboSmall_wear -= (newCalculated_boost * 0.003f);
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
            }

            if (mod.turboSmall_airfilter_part.InstalledScrewed())
            {
                if (newCalculated_boost >= 0.2f)
                {
                    if (mod.partsWearSave.airfilter_wear <= 0)
                    {
                        mod.partsWearSave.airfilter_wear = 0;
                    }
                    else if (timer_wear_airfilter >= 0.5f)
                    {
                        timer_wear_airfilter = 0;
                        mod.partsWearSave.airfilter_wear -= (newCalculated_boost * 0.0045f);
                    }
                }
                else
                {
                    timer_wear_airfilter = 0;
                }
            }


            if (mod.turboSmall_airfilter_part.InstalledScrewed())
            {
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
            }
            */
            return newCalculated_boost;
        }

        private float CalculateTurboBoost(){
            /*
            bool intercooler_installedScrewed = mod.intercooler_part.InstalledScrewed();
            if (twinCarb_inst.Value && intercooler_installedScrewed)
            {
                mod.boostSave.turboSmall_max_boost_limit = (0.95f + 0.05f + 0.15f + 0.19f);
            }
            else if (twinCarb_inst.Value && intercooler_installedScrewed)
            {
                mod.boostSave.turboSmall_max_boost_limit = (0.95f + 0.05f + 0.15f);
            }
            else if (twinCarb_inst.Value && intercooler_installedScrewed)
            {
                mod.boostSave.turboSmall_max_boost_limit = (0.95f + 0.05f + 0.00f + 0.11f);
            }
            else if (weberCarb_inst.Value && intercooler_installedScrewed && mod.turboSmall_airfilter_part.InstalledScrewed())
            {
                mod.boostSave.turboSmall_max_boost_limit = (0.95f + 0.11f + 0.15f + 0.19f);
            }
            else if (weberCarb_inst.Value && intercooler_installedScrewed)
            {
                mod.boostSave.turboSmall_max_boost_limit = (0.95f + 0.11f + 0.15f);
            }
            else
            {
                mod.boostSave.turboSmall_max_boost_limit = (0.95f + 0.05f);
            }
            if (mod.boostSave.turboSmall_max_boost >= mod.boostSave.turboSmall_max_boost_limit)
            {
                mod.boostSave.turboSmall_max_boost = mod.boostSave.turboSmall_max_boost_limit;
            }

            if (ecu_mod_installed && ecu_mod_installed && smart_engine_module_alsModuleEnabled != null && smart_engine_module_alsModuleEnabled.Value && satsumaDriveTrain.revLimiterTriggered)
            {
                calculated_boost = Convert.ToSingle(Math.Log(10000 / 1600, 10)) * 2.2f;
            }
            else
            {
                calculated_boost = Convert.ToSingle(Math.Log(satsumaDriveTrain.rpm / 1600, 10)) * 2.2f;
            }

            if (calculated_boost > mod.boostSave.turboSmall_max_boost)
            {
                calculated_boost = mod.boostSave.turboSmall_max_boost;
            }
            */
            return calculated_boost;
            
        }

        private void CreateTurboGrindingLoop(){ 
            turboGrindingLoop = this.gameObject.AddComponent<AudioSource>();
            turbocharger_grinding_loop.audioSource = turboGrindingLoop;
            turbocharger_grinding_loop.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "grinding sound.wav"), true, false);

            turboGrindingLoop.rolloffMode = AudioRolloffMode.Linear;
            turboGrindingLoop.minDistance = 1;
            turboGrindingLoop.maxDistance = 10;
            turboGrindingLoop.spatialBlend = 0.6f;
            turboGrindingLoop.loop = true;
        }

        private void CreateTurboLoopSmall(){ 
            turboLoopSmall = this.gameObject.AddComponent<AudioSource>();
            turbocharger_loop_small.audioSource = turboLoopSmall;
            turbocharger_loop_small.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "turbocharger_loop.wav"), true, false);

            turboLoopSmall.rolloffMode = AudioRolloffMode.Custom;
            turboLoopSmall.minDistance = 1;
            turboLoopSmall.maxDistance = 10;
            turboLoopSmall.spatialBlend = 1;
            turboLoopSmall.loop = true;
        }

        internal static bool useButtonDown{ 
            get { return cInput.GetKeyDown("Use"); }
            
        }
        internal static bool useThrottleButton{ 
            get { return cInput.GetKey("Throttle"); }
        }

        internal bool throttleUsed{ 
            get
            {
                return (CarH.drivetrain.idlethrottle > 0f);
            }
        }

        internal static void SwitchUseFlutterSound(bool useFlutterSound){ 
            throw new NotImplementedException();
        }

        public void Init(SatsumaTurboCharger mod)
        {
            this.mod = mod;
        }
    }
}