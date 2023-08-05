﻿using System.Collections.Generic;
using MscModApi.Caching;
using MscModApi.Parts;
using SatsumaTurboCharger.turbo;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmall : DerivablePart
	{
		protected override string partId => "turboSmall";
		protected override string partName => "GT Turbo";
		protected override Vector3 partInstallPosition => new Vector3(-0.25f, -0.1665f, 0.0001f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public Turbo turbo;
		
		
		public TurboSmall(SatsumaTurboCharger mod, Dictionary<string, float> boostSave) : base(Cache.Find("cylinder head(Clone)"), SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0.0715f, -0.043f, 0.052f), new Vector3(0, 90, 0),
				new Vector3(0.5f, 0.5f, 0.5f));
			AddScrew(new Screw(new Vector3(0.0715f, -0.024f, 0.044f), new Vector3(180f, 0f, 0f),
				Screw.Type.Normal, 0.4f));
			
			turbo = new Turbo(mod.boostGauge, mod, this, boostSave, "turbocharger_loop.wav", "grinding sound.wav", null,
				new[]
				{
					false,
					true,
					true
				}, GetTurboConfig(), gameObject.transform.FindChild("turboSmall-wastegate").gameObject);
		}

		private TurboConfiguration GetTurboConfig()
		{
			return new TurboConfiguration
			{
				boostBase = 2f,
				boostStartingRpm = 3500,
				boostStartingRpmOffset = 2200,
				boostMin = -0.10f,
				minSettableBoost = 1.65f,
				boostSteepness = 2.2f,
				blowoffDelay = 0.8f,
				blowoffTriggerBoost = 0.6f,
				backfireThreshold = 4000,
				backfireRandomRange = 20,
				rpmMultiplier = 14,
				extraPowerMultiplicator = 1.5f,
				boostSettingSteps = 0.05f,
				soundboostMinVolume = 0.05f,
				soundboostMaxVolume = 0.3f,
				soundboostPitchMultiplicator = 4,
			};
		}

	}
}