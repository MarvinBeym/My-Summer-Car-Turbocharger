using System.Collections.Generic;
using MscModApi.Caching;
using MscModApi.Parts;
using SatsumaTurboCharger.turbo;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmall : TurboPart
	{
		protected override string partId => "turboSmall";
		protected override string partName => "GT Turbo";
		protected override Vector3 partInstallPosition => new Vector3(-0.1398f, 0.1299f, -0.0155f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public TurboSmall(SatsumaTurboCharger mod, BoostGauge boostGauge, TurboSmallExhaustInletTube parent, Dictionary<string, float> boostSave) : base(mod, boostGauge, parent, boostSave)
		{
			AddClampModel(new Vector3(-0.00775f, -0.07375f, -0.0355f), new Vector3(-45, 90, 0),
				new Vector3(0.5f, 0.5f, 0.49f));
			AddScrew(new Screw(new Vector3(-0.0216f, -0.0603f, -0.0578f), new Vector3(180f, 0f, 0f),
				Screw.Type.Normal, 0.4f));

			DefineBoostChangingGameObject(gameObject.transform.FindChild("turboSmall-wastegate").gameObject);

			audioHandler.Add("turboLoop", this, "turbocharger_loop.wav", true);
			audioHandler.Add("grinding", this, "grinding sound.wav", true);
		}

		protected override TurboConfiguration SetupTurboConfig()
		{
			return new TurboConfiguration()
			{
				boostBase = 0.8f,
				boostStartingRpm = 1200,
				boostStartingRpmOffset = 1200,
				boostMin = -0.10f,
				minSettableBoost = 1.65f,
				boostSteepness = 1f,
				blowoffDelay = 0.2f,
				blowoffTriggerBoost = 0.6f,
				backfireThreshold = 4000,
				backfireRandomRange = 20,
				rpmMultiplier = 10,
				extraPowerMultiplicator = 1.5f,
				boostSettingSteps = 0.05f,
				soundboostMinVolume = 0.03f,
				soundboostMaxVolume = 0.08f,
				soundboostPitchMultiplicator = 4,
				backfireDelay = 0.05f,
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