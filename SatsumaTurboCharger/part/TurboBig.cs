using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSCLoader;
using MscModApi.PaintingSystem;
using MscModApi.Parts;
using MscModApi.Tools;
using SatsumaTurboCharger.turbo;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboBig : TurboPart
	{
		private readonly TurboBigBlowoffValve turboBigBlowoffValve;

		protected override string partName => "Racing Turbo";
		protected override string partId => "turboBig";
		protected override Vector3 partInstallPosition => new Vector3(-0.0668f, 0.28826f, -0.08025f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public TurboBig(SatsumaTurboCharger mod, BoostGauge boostGauge, TurboBigBlowoffValve turboBigBlowoffValve, Part parent, Dictionary<string, float> boostSave) : base(mod, boostGauge, parent, boostSave)
		{
			this.turboBigBlowoffValve = turboBigBlowoffValve;
			AddClampModel(new Vector3(0.078f, -0.09f, -0.058f), new Vector3(-90, 0, 0),
				new Vector3(0.85f, 0.85f, 0.85f));
			AddScrew(new Screw(new Vector3(0.0540f, -0.09f, -0.091f), new Vector3(0, -90, 0),
				Screw.Type.Normal, 0.5f));

			PaintingSystem.Setup(partBaseInfo.mod, this, gameObject.FindChild("turboBig-center").FindChild("turboBig-compressor-turbine").gameObject);

			GameObject boostChangingGameObject = turboBigBlowoffValve.gameObject.transform
				.FindChild("turboBig-blowoff-valve-main").gameObject;

			//Fixing collider too small on model
			boostChangingGameObject.GetComponent<BoxCollider>().size = new Vector3(0.09f, 0.09f, 0.09f);

			DefineBoostChangingGameObject(
				boostChangingGameObject
				);
			DefineSpinningTurbineGameObject(gameObject.transform.FindChild("turboBig-center").FindChild("turboBig-compressor-turbine").gameObject);

			audioHandler.Add("turboLoop", this, "turbocharger_loop.wav", true);
			audioHandler.Add("grinding", this, "grinding sound.wav", true);
			audioHandler.Add("blowoff", this, "turbocharger_blowoff.wav");
		}


		protected override TurboConfiguration SetupTurboConfig()
		{
			return new TurboConfiguration
			{
				boostBase = 2f,
				boostStartingRpm = 3000,
				boostStartingRpmOffset = 1600,
				boostMin = -0.10f,
				minSettableBoost = 1.65f,
				boostSteepness = 1.5f,
				blowoffDelay = 0.8f,
				blowoffTriggerBoost = 0.6f,
				backfireThreshold = 4000,
				backfireRandomRange = 20,
				rpmMultiplier = 14,
				extraPowerMultiplicator = 1.5f,
				boostSettingSteps = 0.05f,
				soundboostMinVolume = 0.1f,
				soundboostMaxVolume = 0.35f,
				soundboostPitchMultiplicator = 4
			};
		}

		protected override TurboConditionStorage SetupTurboConditions()
		{
			TurboConditionStorage turboConditionStorage = new TurboConditionStorage();
			turboConditionStorage.AddConditions(new Condition[]
			{
				new Condition("weberCarb", 0.5f),
				new Condition("twinCarb", 0.2f),
			});
			return turboConditionStorage;
		}
	}
}