﻿using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger
{
    public class GT_Turbocharger_Logic : MonoBehaviour
    {
        private SatsumaTurboCharger donnerTech_turbocharger_mod;

        //Save
        private BoostSave boostSave;
        private PartsWearSave partsWearSave;

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

        //Car
        private GameObject satsuma;
        private Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        private FsmString playerCurrentVehicle;
        private PlayMakerFSM carElectricsPower;

        //Audio
        private ModAudio turbocharger_loop_small = new ModAudio();
        private ModAudio turbocharger_grinding_loop = new ModAudio();

        private AudioSource turboLoopSmall;
        private AudioSource turboGrindingLoop;

        //Small turbo playmakerFSM
        private PlayMakerFSM turbocharger_smallFSM;
        private FsmFloat turbocharger_small_rpm;
        private FsmFloat turbocharger_small_pressure;
        private FsmFloat turbocharger_small_max_boost;
        private FsmFloat turbocharger_small_wear;
        private FsmFloat turbocharger_small_exhaust_temp;
        private FsmFloat turbocharger_small_intake_temp;
        private FsmBool turbocharger_small_allInstalled;

        //Installed Objects
        private FsmBool weberCarb_inst;
        private FsmBool twinCarb_inst;

        //Time Comparer
        private float timeSinceLastBlowOff;
        private float timer_delay_turbocharger_small;

        //Turbo delay
        private float turbocharger_delay;

        //Wear Logic
        private Random randDestroyValue;
        private float timer_wear_turbocharger_small;
        private float timer_wear_intercooler;
        private float timer_wear_airfilter;

        void Start(){ 
            System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
            Mod[] modsArr = mods.ToArray();
            foreach (Mod mod in modsArr)
            {
                if (mod.ID == "SatsumaTurboCharger")
                {
                    donnerTech_turbocharger_mod = (SatsumaTurboCharger)mod;
                    break;
                }
            }
            ecu_mod_installed = ModLoader.IsModPresent("DonnerTech_ECU_Mod");
            
            boostSave = donnerTech_turbocharger_mod.GetBoostSave();
            partsWearSave = donnerTech_turbocharger_mod.GetPartsWearSave();

            CreateTurboGrindingLoop();
            CreateTurboLoopSmall();

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


            turbocharger_smallFSM = this.gameObject.AddComponent<PlayMakerFSM>();
            turbocharger_smallFSM.FsmName = "SatsumaTurboCharger_Values";

            turbocharger_small_rpm = new FsmFloat("Rpm");
            turbocharger_small_pressure = new FsmFloat("Pressure");
            turbocharger_small_max_boost = new FsmFloat("Max boost");
            turbocharger_small_wear = new FsmFloat("Wear");
            turbocharger_small_exhaust_temp = new FsmFloat("Exhaust temperature");
            turbocharger_small_intake_temp = new FsmFloat("Intake temperature");
            turbocharger_small_allInstalled = new FsmBool("All installed");

            turbocharger_smallFSM.FsmVariables.FloatVariables = new FsmFloat[]
            {
                        turbocharger_small_rpm,
                        turbocharger_small_pressure,
                        turbocharger_small_max_boost,
                        turbocharger_small_wear,
                        turbocharger_small_exhaust_temp,
                        turbocharger_small_intake_temp
            };
            turbocharger_smallFSM.FsmVariables.BoolVariables = new FsmBool[]
            {
                        turbocharger_small_allInstalled
            };
        }

        // Update is called once per frame
        void Update(){ 
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

            if (hasPower && donnerTech_turbocharger_mod.GetAirfilterInstalled() && !donnerTech_turbocharger_mod.GetAirfilterScrewed())
            {
                turbocharger_small_max_boost.Value = boostSave.turbocharger_big_max_boost;
                turbocharger_small_exhaust_temp.Value = 0f;
                turbocharger_small_intake_temp.Value = 0f;
                turbocharger_small_rpm.Value = 0;
                turbocharger_small_pressure.Value = 0;
                turbocharger_small_allInstalled.Value = false;

                turbocharger_loop_small.Stop();
                turbocharger_grinding_loop.Stop();
                donnerTech_turbocharger_mod.SetBoostGaugeText("ERR");
            }
            else if (hasPower && donnerTech_turbocharger_mod.GetAllOtherPartsInstalledScrewed() && donnerTech_turbocharger_mod.GetAllSmallPartsInstalledScrewed() && !donnerTech_turbocharger_mod.GetAllBigPartsInstalledScrewed())
            {
                if (!turboLoopSmall.isPlaying)
                {
                    turboLoopSmall.Play();
                }
                if (donnerTech_turbocharger_mod.GetAirfilerInstalledScrewed())
                {
                    turboLoopSmall.volume = satsumaDriveTrain.rpm * 0.00003f;
                    turboLoopSmall.pitch = (satsumaDriveTrain.rpm - 500) * 0.0003f;
                }
                else
                {
                    turboLoopSmall.volume = satsumaDriveTrain.rpm * 0.000026f;
                    turboLoopSmall.pitch = (satsumaDriveTrain.rpm - 500) * 0.00045f;
                }

                timer_wear_turbocharger_small += Time.deltaTime;
                timer_wear_intercooler += Time.deltaTime;
                timer_wear_airfilter += Time.deltaTime;
                timeSinceLastBlowOff += Time.deltaTime;


                if (timeSinceLastBlowOff >= 0.2f)
                {
                    calculated_boost = CalculateTurboBoost();
                    calculated_boost = HandleTurboDelay(calculated_boost, 0.1f, 0.4f);

                    if (calculated_boost > 0f)
                    {
                        if (SatsumaTurboCharger.GetPartsWearEnabled())
                        {
                            calculated_boost = HandleWear(calculated_boost);
                        }
                        donnerTech_turbocharger_mod.SetBoostGaugeText(calculated_boost, true);
                        powerMultiplier.Value = (0.90f + (calculated_boost * 1.5f));
                    }
                    else
                    {
                        timer_wear_turbocharger_small = 0;
                        timer_wear_intercooler = 0;
                        timer_wear_airfilter = 0;
                        calculated_boost = -0.04f;
                        powerMultiplier.Value = 1f - 0.04f;
                        donnerTech_turbocharger_mod.SetBoostGaugeText(0.04f, false);
                    }


                }

                if(timeSinceLastBlowOff < 0.2f || turbocharger_delay <= 0)
                {
                    calculated_boost = -0.04f;
                    powerMultiplier.Value = 1f - 0.04f;
                    donnerTech_turbocharger_mod.SetBoostGaugeText(0.04f, false);
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

                    if (ecu_mod_installed && smart_engine_module_alsModuleEnabled != null)
                    {
                        donnerTech_turbocharger_mod.SetBoostGaugeText(0.10f, false);
                        TriggerBlowoff();
                    }
                }

                turbocharger_small_max_boost.Value = boostSave.turbocharger_small_max_boost;
                turbocharger_small_exhaust_temp.Value = 0f;
                turbocharger_small_intake_temp.Value = 0f;
                turbocharger_small_rpm.Value = CalculateRpm(calculated_boost);
                turbocharger_small_pressure.Value = calculated_boost;
                turbocharger_small_wear.Value = partsWearSave.turbocharger_small_wear;
                turbocharger_small_allInstalled.Value = true;
            }
            else if (!donnerTech_turbocharger_mod.GetAllBigPartsInstalledScrewed())
            {
                turbocharger_small_max_boost.Value = boostSave.turbocharger_big_max_boost;
                turbocharger_small_exhaust_temp.Value = 0f;
                turbocharger_small_intake_temp.Value = 0f;
                turbocharger_small_rpm.Value = 0;
                turbocharger_small_pressure.Value = 0;
                turbocharger_small_allInstalled.Value = false;

                turbocharger_loop_small.Stop();
                turbocharger_grinding_loop.Stop();
                donnerTech_turbocharger_mod.SetBoostGaugeText("ERR");


            }
            else
            {
                turbocharger_small_max_boost.Value = boostSave.turbocharger_big_max_boost;
                turbocharger_small_exhaust_temp.Value = 0f;
                turbocharger_small_intake_temp.Value = 0f;
                turbocharger_small_rpm.Value = 0;
                turbocharger_small_pressure.Value = 0;
                turbocharger_small_allInstalled.Value = false;

                turbocharger_loop_small.Stop();
                turbocharger_grinding_loop.Stop();
            }
        }

        private float HandleTurboDelay(float calculated_boost, float delay_comparer, float delayAdder){ 
            timer_delay_turbocharger_small += Time.deltaTime;
            if (useThrottleButton || throttleUsed)
            {
                if (timer_delay_turbocharger_small >= delay_comparer)
                {
                    timer_delay_turbocharger_small = 0;
                    turbocharger_delay += delayAdder;
                    if (turbocharger_delay >= 1)
                        turbocharger_delay = 1;
                }
            }
            else if (!useThrottleButton && !throttleUsed)
            {
                if (timer_delay_turbocharger_small >= delay_comparer)
                {
                    timer_delay_turbocharger_small = 0;
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
            if (partsWearSave.turbocharger_small_wear <= 0f)
            {
                donnerTech_turbocharger_mod.removePartSmallTurbo();
            }
            else if (partsWearSave.turbocharger_small_wear <= 15f)
            {

                int randVal = randDestroyValue.Next(100);
                if (randVal == 1)
                {
                    //Part should destroy
                    donnerTech_turbocharger_mod.removePartSmallTurbo();
                }
            }
            if (donnerTech_turbocharger_mod.GetAirfilerInstalledScrewed())
            {
                if (partsWearSave.airfilter_wear <= 0f)
                {
                    donnerTech_turbocharger_mod.removePartAirfilter();
                }
                else if (partsWearSave.airfilter_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(100);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        donnerTech_turbocharger_mod.removePartAirfilter();
                    }
                }
            }

            if (donnerTech_turbocharger_mod.GetIntercoolerInstalledScrewed())
            {
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
            if (partsWearSave.turbocharger_small_wear <= 0)
            {
                partsWearSave.turbocharger_small_wear = 0;
            }
            else if (timer_wear_turbocharger_small >= 0.5f)
            {
                timer_wear_turbocharger_small = 0;
                if (donnerTech_turbocharger_mod.GetAirfilerInstalledScrewed())
                {
                    partsWearSave.turbocharger_small_wear -= (newCalculated_boost * (0.003f - (partsWearSave.airfilter_wear / 60000)));
                }
                else
                {
                    partsWearSave.turbocharger_small_wear -= (newCalculated_boost * 0.003f);
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
            }

            if (donnerTech_turbocharger_mod.GetAirfilerInstalledScrewed())
            {
                if (newCalculated_boost >= 0.2f)
                {
                    if (partsWearSave.airfilter_wear <= 0)
                    {
                        partsWearSave.airfilter_wear = 0;
                    }
                    else if (timer_wear_airfilter >= 0.5f)
                    {
                        timer_wear_airfilter = 0;
                        partsWearSave.airfilter_wear -= (newCalculated_boost * 0.0045f);
                    }
                }
                else
                {
                    timer_wear_airfilter = 0;
                }
            }


            if (donnerTech_turbocharger_mod.GetIntercoolerInstalledScrewed())
            {
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
            }
            
            return newCalculated_boost;
        }

        private float CalculateTurboBoost(){ 
            if (twinCarb_inst.Value && donnerTech_turbocharger_mod.GetIntercoolerInstalledScrewed() && donnerTech_turbocharger_mod.GetAirfilerInstalledScrewed())
            {
                boostSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.15f + 0.19f);
            }
            else if (twinCarb_inst.Value && donnerTech_turbocharger_mod.GetIntercoolerInstalledScrewed())
            {
                boostSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.15f);
            }
            else if (twinCarb_inst.Value && donnerTech_turbocharger_mod.GetIntercoolerInstalledScrewed())
            {
                boostSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.00f + 0.11f);
            }
            else if (weberCarb_inst.Value && donnerTech_turbocharger_mod.GetIntercoolerInstalledScrewed() && donnerTech_turbocharger_mod.GetAirfilerInstalledScrewed())
            {
                boostSave.turbocharger_small_max_boost_limit = (0.95f + 0.11f + 0.15f + 0.19f);
            }
            else if (weberCarb_inst.Value && donnerTech_turbocharger_mod.GetIntercoolerInstalledScrewed())
            {
                boostSave.turbocharger_small_max_boost_limit = (0.95f + 0.11f + 0.15f);
            }
            else
            {
                boostSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f);
            }
            if (boostSave.turbocharger_small_max_boost >= boostSave.turbocharger_small_max_boost_limit)
            {
                boostSave.turbocharger_small_max_boost = boostSave.turbocharger_small_max_boost_limit;
            }

            if (ecu_mod_installed && ecu_mod_installed && smart_engine_module_alsModuleEnabled != null && smart_engine_module_alsModuleEnabled.Value && satsumaDriveTrain.revLimiterTriggered)
            {
                calculated_boost = Convert.ToSingle(Math.Log(10000 / 1600, 10)) * 2.2f;
            }
            else
            {
                calculated_boost = Convert.ToSingle(Math.Log(satsumaDriveTrain.rpm / 1600, 10)) * 2.2f;
            }

            if (calculated_boost > boostSave.turbocharger_small_max_boost)
            {
                calculated_boost = boostSave.turbocharger_small_max_boost;
            }

            return calculated_boost;
        }

        private void CreateTurboGrindingLoop(){ 
            turboGrindingLoop = this.gameObject.AddComponent<AudioSource>();
            turbocharger_grinding_loop.audioSource = turboGrindingLoop;
            turbocharger_grinding_loop.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_turbocharger_mod), "grinding sound.wav"), true, false);

            turboGrindingLoop.rolloffMode = AudioRolloffMode.Linear;
            turboGrindingLoop.minDistance = 1;
            turboGrindingLoop.maxDistance = 10;
            turboGrindingLoop.spatialBlend = 0.6f;
            turboGrindingLoop.loop = true;
        }

        private void CreateTurboLoopSmall(){ 
            turboLoopSmall = this.gameObject.AddComponent<AudioSource>();
            turbocharger_loop_small.audioSource = turboLoopSmall;
            turbocharger_loop_small.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_turbocharger_mod), "turbocharger_loop.wav"), true, false);

            turboLoopSmall.rolloffMode = AudioRolloffMode.Custom;
            turboLoopSmall.minDistance = 1;
            turboLoopSmall.maxDistance = 10;
            turboLoopSmall.spatialBlend = 1;
            turboLoopSmall.loop = true;
        }

        private bool hasPower{ 
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
        internal static bool useButtonDown{ 
            get { return cInput.GetKeyDown("Use"); }
            
        }
        internal static bool useThrottleButton{ 
            get { return cInput.GetKey("Throttle"); }
        }

        internal bool throttleUsed{ 
            get
            {
                return (satsumaDriveTrain.idlethrottle > 0f);
            }
        }

        internal static void SwitchUseFlutterSound(bool useFlutterSound){ 
            throw new NotImplementedException();
        }
    }
}