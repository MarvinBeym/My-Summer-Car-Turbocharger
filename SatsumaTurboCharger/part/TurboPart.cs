using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Tools;
using SatsumaTurboCharger.turbo;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public abstract class TurboPart : DerivablePart
	{
		public float boost => logic.boost;
		public float rpm => logic.rpm;
		public float setBoost => logic.setBoost;

		public Backfire_Logic backFireLogic => logic.backFireLogic;

		public FsmFloat powerMultiplier;
		public TurboConditionStorage conditionStorage { get; protected set; }
		protected readonly BoostGauge boostGauge;
		protected readonly List<Part> requiredParts;
		protected readonly TurboLogic logic;
		public readonly AudioHandler audioHandler;
		public GameObject boostChangingGameObject;
		protected static GameObject backfireFxModel;

		protected TurboPart(SatsumaTurboCharger mod, BoostGauge boostGauge, Part parent, List<Part> requiredParts, Dictionary<string, float> boostSave) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			audioHandler = new AudioHandler(mod);
			this.boostGauge = boostGauge;
			this.requiredParts = requiredParts;
			CarH.drivetrain.clutchTorqueMultiplier = 10f;
			config = SetupTurboConfig();
			conditionStorage = SetupTurboConditions();
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

			logic = AddWhenInstalledBehaviour<TurboLogic>();
			logic.Init(
				audioHandler,
				boostGauge, 
				this, 
				conditionStorage,
				boostSave.TryGetValue(id, out var value) ? value : config.boostBase
			);
		}

		public bool requiredInstalled => requiredParts.All(part => part.installed && part.bolted && parentInstalled && parentBolted);

		public void DefineBoostChangingGameObject(GameObject boostChangingGameObject)
		{
			if (boostChangingGameObject != null)
			{
				return;
			}

			this.boostChangingGameObject = boostChangingGameObject;
		}

		public void DefineBackfire(Part backfirePart, AudioSource backfireAudioSource)
		{
			if (logic.backFireLogic != null)
			{
				return;
			}

			GameObject backfireFxGameObject = GameObject.Instantiate(backfireFxModel);
			backfireFxGameObject.transform.parent = backfirePart.transform;

			logic.backFireLogic = backfirePart.AddWhenInstalledBehaviour<Backfire_Logic>();
			logic.backFireLogic.Init(backfireAudioSource);
		}

		public void DefineSpinningTurbineGameObject(GameObject gameObject)
		{
			if (logic.spinningTurbineGameObject != null)
			{
				return;
			}
			logic.spinningTurbineGameObject = gameObject;
		}

		public TurboConfiguration config { get; protected set; }

		protected abstract TurboConfiguration SetupTurboConfig();

		protected abstract TurboConditionStorage SetupTurboConditions();

		public static void Save(SatsumaTurboCharger mod, string saveFile, TurboPart[] turbos)
		{
			try
			{
				Dictionary<string, float> save = new Dictionary<string, float>();
				foreach (TurboPart turbo in turbos)
				{
					save[turbo.id] = turbo.setBoost;
				}
				SaveLoad.SerializeSaveFile<Dictionary<string, float>>(mod, save, saveFile);
			}
			catch (Exception ex)
			{
				Logger.New("Error while trying to save configured boost information", ex);
			}
		}

		public static void LoadAssets(AssetBundle assetBundle)
		{
			if (backfireFxModel != null)
			{
				return;
			}
			backfireFxModel = assetBundle.LoadAsset<GameObject>("fx_fire.prefab");
		}
	}
}