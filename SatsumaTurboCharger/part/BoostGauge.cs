﻿using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
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

		protected BoostGaugeLogic logic;

		public BoostGaugeLogic.GaugeMode gaugeMode => logic.gaugeMode;

		public BoostGauge(GamePart parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			AddScrew(new Screw(new Vector3(0f, -0.0270f, 0.003f), new Vector3(-90, 0, 0),
				Screw.Type.Normal, 0.4f, 8));

			logic = AddEventBehaviour<BoostGaugeLogic>(PartEvent.Type.InstallOnCar);
			logic.Init(this);
		}


		public void SetBoost(float boostGaugeTarget, float boostBeforeRelease, TurboConfiguration config)
		{
			logic.SetBoost(boostGaugeTarget, boostBeforeRelease, config);
		}

		public void SetDigitalText(string text)
		{
			logic.SetDigitalText(text);
		}
	}
}