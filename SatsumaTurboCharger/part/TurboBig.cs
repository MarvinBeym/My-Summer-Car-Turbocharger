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
	public class TurboBig : DerivablePart
	{
		protected override string partName => "Racing Turbo";
		protected override string partId => "turboBig";
		protected override Vector3 partInstallPosition => new Vector3(-0.0668f, 0.28826f, -0.08025f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);
		public Turbo turbo;

		public TurboBig(SatsumaTurboCharger mod, Dictionary<string, float> boostSave, TurboBigExhaustInletTube parent, TurboBigBlowoffValve turboBigBlowoffValve, TurboBigExhaustOutletStraight turboBigExhaustOutletStraight) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0.078f, -0.09f, -0.058f), new Vector3(-90, 0, 0),
				new Vector3(0.85f, 0.85f, 0.85f));
			AddScrew(new Screw(new Vector3(0.0540f, -0.09f, -0.091f), new Vector3(0, -90, 0),
				Screw.Type.Normal, 0.5f));

			PaintingSystem.Setup(partBaseInfo.mod, this, gameObject.FindChild("turboBig-center").FindChild("turboBig-compressor-turbine").gameObject);
			
			turbo = new Turbo(mod.boostGauge, mod, this, boostSave, "turbocharger_loop.wav", "grinding sound.wav",
				"turbocharger_blowoff.wav",
				new[]
				{
					true,
					false,
					true
				}, GetTurboConfig(),
				turboBigBlowoffValve.gameObject.transform.FindChild("turboBig-blowoff-valve-main").gameObject);

			turbo.turbine = gameObject.transform.FindChild("turboBig-center").FindChild("turboBig-compressor-turbine").gameObject;
			
			Dictionary<string, Condition> racingTurbo_conditions = new Dictionary<string, Condition>();
			racingTurbo_conditions["weberCarb"] = new Condition("weberCarb", 0.5f);
			racingTurbo_conditions["twinCarb"] = new Condition("twinCarb", 0.2f);
			turbo.SetConditions(racingTurbo_conditions);
		}

		public void AddBackfire(Part backfirePart)
		{
			turbo.backfire_Logic = backfirePart.gameObject.AddComponent<Backfire_Logic>();

		}
		
		private TurboConfiguration GetTurboConfig()
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
	}
}