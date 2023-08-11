﻿using MscModApi.Caching;
using MscModApi.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using MSCLoader;
using MscModApi.Parts;
using SatsumaTurboCharger.part;
using UnityEngine;
using EventType = MscModApi.Parts.EventType;
using Random = System.Random;


namespace SatsumaTurboCharger.turbo
{
	public class TurboLogic : MonoBehaviour
	{
		protected TurboConfiguration config => turboPart.config;
		TurboPart turboPart;

		protected float blowoffTimer = 0;
		protected float backfireTimer = 0;
		protected TurboConditionStorage conditionStorage;
		protected BoostGauge boostGauge;
		protected bool canBackfire = false;
		protected bool blowoffAllowed = false;

		public BackfireLogic backFireLogic;
		public GameObject spinningTurbineGameObject;
		protected AudioHandler audioHandler;
		private AudioSource turboLoopAudio;
		private AudioSource blowoffAudio;

		public float boost { get; protected set; } = 0;

		public float rpm
		{
			get
			{
				float turboRpm = engineRpm * config.rpmMultiplier;
				if (turboRpm <= 0)
				{
					turboRpm = 0;
				}
				return turboRpm;
			}
		}
		public float setBoost { get; protected set; } = 0;

		protected float engineRpm => CarH.drivetrain.rpm;

		public float boostMaxConfigured { get; protected set; } = 0;

		public GameObject boostChangingGameObject => turboPart.boostChangingGameObject;
		public bool requiredInstalledAndBolted => turboPart.requiredParts == null || turboPart.requiredParts.requiredInstalledAndBolted;

		protected void Start()
		{
			turboLoopAudio = audioHandler.Get("turboLoop");
			blowoffAudio = audioHandler.Get("blowoff");
			conditionStorage.DefineConditionsHaveUpdatedAction(() =>
			{
				float newCalculatedIncrease = 0;
				conditionStorage.GetConditions().ForEach(condition =>
				{
					if (condition.applyCondition)
					{
						newCalculatedIncrease += condition.valueToApply;
					}
				});


				boostMaxConfigured = config.boostBase + newCalculatedIncrease;
			});
		}

		protected void LateUpdate()
		{
			if (!requiredInstalledAndBolted || !CarH.running)
			{
				//Not all required installed RESET
				audioHandler.StopAll();
				return;
			}
			blowoffTimer += Time.deltaTime;

			audioHandler.Play(turboLoopAudio);

			//boostMaxConfigured = CalculateConfigurationBoost(boostMaxConfigured, config.boostBase, turbo.conditions);
			float soundBoost = CalculateBoost(engineRpm);

			soundBoost = soundBoost.Map(config.boostMin, boostMaxConfigured, config.soundboostMinVolume, config.soundboostMaxVolume);

			audioHandler.SetVolume(turboLoopAudio, soundBoost);
			audioHandler.SetPitch(turboLoopAudio, soundBoost * config.soundboostPitchMultiplicator);

			HandleBoostChanging();

			RotateTurbine(spinningTurbineGameObject);

			if (!CarH.playerInCar)
			{
				return;
			}


			float boostBeforeRelease = 0;

			if (UserInteraction.ThrottleDown && engineRpm >= config.backfireThreshold && blowoffTimer > config.blowoffDelay)
			{
				blowoffAllowed = true;
			}

			bool shifting = UserInteraction.ThrottleDown && (CarH.drivetrain.changingGear);
			bool lettingGoOfThrottle = !UserInteraction.ThrottleDown;
			if (boost >= config.blowoffTriggerBoost && blowoffAllowed == true && (shifting || lettingGoOfThrottle))
			{
				boost = config.boostMin;
				blowoffAllowed = false;
				blowoffTimer = 0;
				audioHandler.Play(blowoffAudio);
			}

			if (blowoffTimer >= config.blowoffDelay)
			{
				try
				{
					boost = CalculateBoost(engineRpm);

					if (boost > 0)
					{
						//if ((bool)mod.partsWearSetting.Value && (turbo.wears.Length > 0 || turbo.wears == null)) { boost = HandleWear(boost); }
						if ((bool)SatsumaTurboCharger.backfireEffectSetting.Value)
						{
							HandleBackfire(engineRpm);
						}
					}
					boostBeforeRelease = boost;
				}
				catch (Exception ex)
				{
					Logger.New("Exception was thrown while trying to calculate turbo boost", ex);
				}
			}
			else
			{
				boost = config.boostMin;
			}

			if (boost <= 0 || !UserInteraction.ThrottleDown)
			{
				boost = config.boostMin;
			}

			float boostGaugeTarget = UserInteraction.ThrottleDown ? boostBeforeRelease : config.boostMin;

			boostGauge.SetBoost(boostGaugeTarget, boostBeforeRelease, config);


			float finalMultiplication = boost * config.extraPowerMultiplicator;
			CarH.drivetrain.powerMultiplier = 1f + Mathf.Clamp(finalMultiplication, config.boostMin, boostMaxConfigured);

		}

		private void HandleBoostChanging()
		{
			if (boostChangingGameObject == null || !boostChangingGameObject.IsLookingAt())
			{
				return;
			}

			float tmpSetBoost = setBoost;
			if (UserInteraction.MouseScrollWheel.Up)
			{
				tmpSetBoost += config.boostSettingSteps;
			}

			if (UserInteraction.MouseScrollWheel.Down)
			{
				tmpSetBoost -= config.boostSettingSteps;
			}

			tmpSetBoost = tmpSetBoost >= boostMaxConfigured ? tmpSetBoost : boostMaxConfigured;
			tmpSetBoost = tmpSetBoost <= config.minSettableBoost ? tmpSetBoost : config.minSettableBoost;

			UserInteraction.GuiInteraction("" +
				"[SCROLL UP] to increase boost\n" +
				"[SCROLL DOWN] to decrease boost\n" +
				"Boost: " + tmpSetBoost.ToString("0.00"));
			setBoost = tmpSetBoost;
		}

		/*
		private float HandleWear(float boost)
		{
			float newBoost = boost;
			foreach (Wear wear in turbo.wears)
			{
				newBoost = wear.CalculateWearResult(newBoost);
			}
			return newBoost;
		}
		*/

		protected void HandleBackfire(float rpm)
		{
			if (backFireLogic == null || rpm < config.backfireThreshold)
			{
				return;
			}

			backfireTimer += Time.deltaTime;

			if (CarH.drivetrain.revLimiterTriggered)
			{
				backFireLogic.TriggerBackfire();
			}

			if (backfireTimer >= 0.1f)
			{
				if (rpm >= config.backfireThreshold && !UserInteraction.ThrottleDown)
				{
					if (canBackfire)
					{
						Random randomShouldBackfire = new Random();
						if (randomShouldBackfire.Next(config.backfireRandomRange) == 1)
						{
							backfireTimer = 0;
							backFireLogic.TriggerBackfire();
							canBackfire = false;
						}
					}
				}
			}

			if (UserInteraction.ThrottleDown)
			{
				canBackfire = true;
			}
		}

		private void RotateTurbine(GameObject turbine)
		{
			if (turbine != null && (bool)SatsumaTurboCharger.rotateTurbineSetting.Value)
			{
				turbine.transform.Rotate(0, 0, (CarH.drivetrain.rpm / 500));
			}
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

		public float CalculateBoost(float rpm)
		{
			float newBoostMax = Mathf.Clamp(setBoost, config.minSettableBoost, boostMaxConfigured);
			return GetBoostCalculationFunction(rpm, config.boostStartingRpm, config.boostStartingRpmOffset, config.boostMin, newBoostMax, config.boostSteepness);
		}

		public void Init(AudioHandler audioHandler, BoostGauge boostGauge, TurboPart turboPart, TurboConditionStorage conditionStorage, float setBoost)
		{
			this.audioHandler = audioHandler;
			this.boostGauge = boostGauge;
			this.turboPart = turboPart;
			this.setBoost = setBoost;
			this.conditionStorage = conditionStorage;
		}

		/* //Backup for future implementation
		private bool SetupEcuMod()
		{
			bool installed = ModLoader.IsModPresent("SatsumaTurboCharger");
			if (!installed)
			{
				return false;
			}
			try
			{
				foreach (PlayMakerFSM fsm in Cache.Find("DonnerTech_ECU_Mod").GetComponents<PlayMakerFSM>())
				{
					switch (fsm.FsmName)
					{
						case "Installed":
							ecu_installedFSM = fsm;
							break;
						case "Modules":
							ecu_modulesFSM = fsm;
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.New("Ecu mod gameobject could not be found or the component is missing", "Mod was found but not gameobject", ex);
				return false;
			}
			return true;
		}
		*/
	}
}