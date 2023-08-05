﻿using MscModApi.Caching;
using MscModApi.Parts;
using SatsumaTurboCharger.turbo;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class BoostGauge : DerivablePart
	{
		protected override string partId => "boost-gauge";
		protected override string partName => "Boost Gauge";
		protected override Vector3 partInstallPosition => new Vector3(0.368f, -0.029f, 0.154f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		protected BoostGauge_Logic logic;

		public BoostGauge() : base(Cache.Find("dashboard(Clone)"), SatsumaTurboCharger.partBaseInfo)
		{
			AddScrew(new Screw(new Vector3(0f, -0.0270f, 0.003f), new Vector3(-90, 0, 0),
				Screw.Type.Normal, 0.4f));

			logic = AddWhenInstalledBehaviour<BoostGauge_Logic>();
			logic.Init(this);
		}


		public void SetBoost(float boostGaugeTarget, float boostBeforeRelease, TurboConfiguration config)
		{
			logic.SetBoost(boostGaugeTarget, boostBeforeRelease, config);
		}
	}
}