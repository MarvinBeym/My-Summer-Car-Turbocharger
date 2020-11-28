using ModApi;
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
        private Configuration config;
        SatsumaTurboCharger mod;
        Turbo turbo;

        private float blowoffTimer = 0;
        private float delayTimer = 0;
        private float backfireTimer = 0;
        private RaycastHit hit;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
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

            if (blowoffTimer >= config.blowoffDelay)
            {
                
                
                turbo.boost = CalculateBoost(engineRpm, config.boostStartingRpm, config.boostMin, turbo.boostMaxConfigured, config.boostIncreasement);
                turbo.boost = CalculateBoostDelay(turbo.boost, 0.1f, 0.4f);
                if (turbo.boost > 0)
                {
                    if((bool) mod.partsWearSetting.Value && (turbo.wears.Length > 0 || turbo.wears == null)) { turbo.boost = HandleWear(turbo.boost); }
                    if((bool) mod.backfireEffectSetting.Value) HandleBackfire(engineRpm);
                }
                else
                {
                    turbo.boost = config.boostMin;
                }                
            }
            turbo.rpm = turbo.CalculateRpm(engineRpm, config.rpmMultiplier);

            if (blowoffTimer < config.blowoffDelay || turbo.boostDelay <= 0)
            {
                turbo.boost = config.boostMin;
            }


            if (Helper.ThrottleButtonDown && engineRpm >= config.backfireThreshold && blowoffTimer > config.blowoffDelay)
            {
                turbo.blowoffAllowed = true;
            }

            if ((!Helper.ThrottleButtonDown && turbo.blowoffAllowed == true) && turbo.boost >= config.blowoffTriggerBoost)
            {
                turbo.boost = config.boostMin;
                turbo.blowoffAllowed = false;
                blowoffTimer = 0;
                turbo.BlowoffSound();
            }

            mod.SetBoostGaugeText(turbo.boost.ToString("0.00"));
            float finalMultiplicator = turbo.boost * config.extraPowerMultiplicator;
            turbo.powerMultiplier.Value = 1f + Mathf.Clamp(finalMultiplicator, config.boostMin, turbo.boostMaxConfigured);

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


        public float CalculateSoundBoost(float rpm, float boostMax, float increasement)
        {
            return GetBoostCalculationFunction(rpm, 0, 0, boostMax, increasement);
        }

        public float GetBoostCalculationFunction(float rpm, float startingRpm, float boostMin, float boostMax, float increasement)
        {
            float function = boostMax * (float)Math.Tanh((rpm - startingRpm) / (increasement));
            return Mathf.Clamp(function, boostMin, boostMax);
        }

        public float CalculateBoost(float rpm, float startingRpm, float boostMin, float boostMax, float increasement)
        {
            float increasementPercentageReduction = turbo.userSetBoost / boostMax;
            float newBoostMax = Mathf.Clamp(turbo.userSetBoost, config.minSettableBoost, boostMax);
            return GetBoostCalculationFunction(rpm, startingRpm, boostMin, newBoostMax, increasement * increasementPercentageReduction);
        }

        public float CalculateBoostDelay(float boost, float delay_comparer, float delayAdder)
        {
            delayTimer += Time.deltaTime;
            if (Helper.ThrottleButtonDown || turbo.boostDelayThrottleUsed)
            {
                if (delayTimer >= delay_comparer)
                {
                    delayTimer = 0;
                    turbo.boostDelay += delayAdder;
                    if (turbo.boostDelay >= 1)
                        turbo.boostDelay = 1;
                }
            }
            else if (!Helper.ThrottleButtonDown && !turbo.boostDelayThrottleUsed)
            {
                if (delayTimer >= delay_comparer)
                {
                    delayTimer = 0;
                    turbo.boostDelay -= (0.1f / 4);
                    if (turbo.boostDelay <= 0)
                        turbo.boostDelay = 0;
                }
            }

            return boost * turbo.boostDelay;
        }


        public void Init(SatsumaTurboCharger mod, Turbo turbo)
        {
            this.config = turbo.config;
            this.mod = mod;
            this.turbo = turbo;
        }
    }
}