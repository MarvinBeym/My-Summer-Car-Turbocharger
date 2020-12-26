﻿using ModApi;
using MSCLoader;
using SatsumaTurboCharger.wear;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger.turbo
{
    public class Turbo_Logic : MonoBehaviour
    {
        private TurboConfiguration config;
        SatsumaTurboCharger mod;
        Turbo turbo;

        private float blowoffTimer = 0;
        private float backfireTimer = 0;
        private RaycastHit hit;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void LateUpdate()
        {
            if(!turbo.CheckAllRequiredInstalled() || !mod.engineRunning)
            {
                //Not all required installed RESET
                turbo.LoopSound(false);
                turbo.GrindingSound(false);
                turbo.blowoff_source.Stop();
                return;
            }
            blowoffTimer += Time.deltaTime;
            float engineRpm = turbo.carDriveTrain.rpm;
            
            turbo.LoopSound(true);

            float soundBoost = CalculateSoundBoost(engineRpm, config.soundBoostMaxVolume, config.soundBoostIncreasement);
            turbo.loop_source.volume = soundBoost;
            turbo.loop_source.pitch = soundBoost * config.soundBoostPitchMultiplicator;

            turbo.boostMaxConfigured = CalculateConfigurationBoost(turbo.boostMaxConfigured, config.boostBase, turbo.conditions);
            HandleBoostChanging();

            RotateTurbine(turbo.turbine);

            if (!Helper.PlayerInCar())
            {
                return;
            }


            float boostBeforeRelease = 0;

            if (Helper.ThrottleButtonDown && engineRpm >= config.backfireThreshold && blowoffTimer > config.blowoffDelay)
            {
                turbo.blowoffAllowed = true;
            }

            bool shifting = Helper.ThrottleButtonDown && (turbo.carDriveTrain.changingGear);
            bool lettingGoOfThrottle = !Helper.ThrottleButtonDown;
            if (turbo.boost >= config.blowoffTriggerBoost && turbo.blowoffAllowed == true && (shifting || lettingGoOfThrottle))
            {
                turbo.boost = config.boostMin;
                turbo.blowoffAllowed = false;
                blowoffTimer = 0;
                turbo.BlowoffSound();
            }

            if (blowoffTimer >= config.blowoffDelay)
            {
                try
                {
                    turbo.boost = CalculateBoost(engineRpm, config.boostStartingRpm, config.boostStartingRpmOffset, config.boostMin, turbo.boostMaxConfigured, config.boostSteepness);

                    if (turbo.boost > 0)
                    {
                        if ((bool)mod.partsWearSetting.Value && (turbo.wears.Length > 0 || turbo.wears == null)) { turbo.boost = HandleWear(turbo.boost); }
                        if ((bool)mod.backfireEffectSetting.Value) HandleBackfire(engineRpm);
                    }
                    boostBeforeRelease = turbo.boost;
                }
                catch(Exception ex)
                {
                    Logger.New("Exception was thrown while trying to calculate turbo boost", ex);
                }
            }
            else
            {
                turbo.boost = config.boostMin;
            }

            if (turbo.boost <= 0 || !Helper.ThrottleButtonDown)
            {
                turbo.boost = config.boostMin;
            }

            float boostGaugeTarget = Helper.ThrottleButtonDown ? boostBeforeRelease : config.boostMin;

            mod.boostGaugeLogic.SetBoost(boostGaugeTarget, boostBeforeRelease, config, 45, 315);

            turbo.rpm = turbo.CalculateRpm(engineRpm, config.rpmMultiplier);
            
            mod.SetBoostGaugeText(turbo.boost.ToString("0.00"));
            float finalMultiplicator = turbo.boost * config.extraPowerMultiplicator;
            turbo.carDriveTrain.powerMultiplier = 1f + Mathf.Clamp(finalMultiplicator, config.boostMin, turbo.boostMaxConfigured);

        }

        private void HandleBoostChanging()
        {
            if (turbo.boostChangingGameObject == null || !Helper.DetectRaycastHitObject(turbo.boostChangingGameObject, "Default"))
            {
                return;
            }

            float setBoost = turbo.userSetBoost;
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            switch (scrollWheel)
            {
                case float _ when scrollWheel > 0f:
                    setBoost += turbo.config.boostSettingSteps;
                    break;
                case float _ when scrollWheel < 0f:
                    setBoost -= turbo.config.boostSettingSteps;
                    break;
            }

            if (setBoost >= turbo.boostMaxConfigured) { setBoost = turbo.boostMaxConfigured; }
            else if (setBoost <= turbo.config.minSettableBoost) { setBoost = turbo.config.minSettableBoost; }

            ModClient.guiInteract("" +
                "[SCROLL UP] to increase boost\n" +
                "[SCROLL DOWN] to decrease boost\n" +
                "Boost: " + setBoost.ToString("0.00"));
            turbo.userSetBoost = setBoost;
        }

        private float HandleWear(float boost)
        {
            float newBoost = boost;
            foreach(Wear wear in turbo.wears)
            {
                newBoost = wear.CalculateWearResult(newBoost);
            }
            return newBoost;
            
        }

        private void HandleBackfire(float rpm)
        {
            if (turbo.backfire_Logic == null || rpm < config.backfireThreshold)
            {
                return;
            }

            backfireTimer += Time.deltaTime;

            if (turbo.carDriveTrain.revLimiterTriggered)
            {
                turbo.backfire_Logic.TriggerBackfire();
            }

            if (backfireTimer >= 0.1f)
            {
                if (rpm >= config.backfireThreshold && !Helper.ThrottleButtonDown)
                {
                    if (turbo.canBackfire)
                    {
                        Random randomShouldBackfire = new Random();
                        if (randomShouldBackfire.Next(config.backfireRandomRange) == 1)
                        {
                            backfireTimer = 0;
                            turbo.backfire_Logic.TriggerBackfire();
                            turbo.canBackfire = false;
                        }
                    }
                }
            }

            if (Helper.ThrottleButtonDown)
            {
                turbo.canBackfire = true;
            }
        }

        private void RotateTurbine(GameObject turbine)
        {
            if(turbine != null && (bool) mod.rotateTurbineSetting.Value)
            {
                turbine.transform.Rotate(0, 0, (turbo.carDriveTrain.rpm / 500));
            }
        }

        private float CalculateConfigurationBoost(float currentMaxBoost, float baseMaxBoost, Dictionary<string, Condition> conditions)
        {
            if (!turbo.conditionsHaveUpdated)
            {
                return currentMaxBoost;
            }

            float newCalculatedIncrease = 0;
            foreach(KeyValuePair<string, Condition> entry in conditions)
            {
                Condition condition = entry.Value;
                if (condition.applyCondition)
                {
                    newCalculatedIncrease += condition.valueToApply;
                }
            }
            turbo.conditionsHaveUpdated = false;
            return baseMaxBoost + newCalculatedIncrease;
        }


        public float CalculateSoundBoost(float rpm, float boostMax, float steepness)
        {
            return GetBoostCalculationFunction(rpm, 0, 0, 0, boostMax, steepness);
        }

        public float GetBoostCalculationFunction(float rpm, float startingRpm, float startingRpmOffset, float boostMin, float boostMax, float steepness)
        {
            float function = boostMax / (1 + (float)Math.Exp(-(steepness / 1000) * (rpm - startingRpm - startingRpmOffset)));
            //float function = boostMax * (float)Math.Tanh((rpm - startingRpm) / (steepness));
            return Mathf.Clamp(function, boostMin, boostMax);
        }

        public float CalculateBoost(float rpm, float startingRpm, float startingRpmOffset, float boostMin, float boostMax, float steepness)
        {
            float newBoostMax = Mathf.Clamp(turbo.userSetBoost, config.minSettableBoost, boostMax);
            return GetBoostCalculationFunction(rpm, startingRpm, startingRpmOffset, boostMin, newBoostMax, steepness);
        }



        public void Init(SatsumaTurboCharger mod, Turbo turbo)
        {
            this.config = turbo.config;
            this.mod = mod;
            this.turbo = turbo;
        }
    }
}