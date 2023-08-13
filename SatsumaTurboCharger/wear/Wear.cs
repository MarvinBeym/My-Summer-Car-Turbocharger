using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Shopping;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger.wear
{
	/*
	 * ToDo: Requires reimplementation
	public class WearMultiplicatorManipulator
	{
		internal float[] manipulatorValues;
		public WearMultiplicatorManipulator(float[] manipulatorValues)
		{
			this.manipulatorValues = manipulatorValues;
		}
	}

	public class Wear
	{
		public SatsumaTurboCharger mod;
		public string id = "";
		public Part part;
		public WearLogic activeLogic;
		public WearLogic rigidLogic;

		public List<WearCondition> wearConditions;
		public float wear = 100;
		public float wearReductionMultiplier;
		public float wearReductionIntervall;
		public int randomFallOff = -1;

		public float timer = 0;

		internal float[] multiplicatorManipulator;

		//Shop
		private ShopItem modsShop;
		private ProductDetails repairProduct;
		public bool repairPurchaseMade = false;

		public override string ToString()
		{
			return wear.ToString("000.00000");
		}

		public Wear(SatsumaTurboCharger mod, string id, Part part, List<WearCondition> wearConditions, float wearReductionMultiplier, float wearReductionIntervall, Dictionary<string, float> wearSave, float productPrice, string productImage, int randomFallOff = -1, float[] multiplicatorManipulator = null)
		{
			this.mod = mod;
			this.id = id;
			this.wearReductionMultiplier = wearReductionMultiplier;
			this.wearConditions = wearConditions;
			this.wearReductionIntervall = wearReductionIntervall;
			this.part = part;
			this.multiplicatorManipulator = multiplicatorManipulator;

			try
			{
				this.wear = wearSave[id];
			}
			catch
			{
				this.wear = 100;
			}
			this.randomFallOff = randomFallOff;


			activeLogic = part.AddWhenUninstalledBehaviour<WearLogic>();
			rigidLogic = part.AddEventBehaviour<WearLogic>();
			activeLogic.Init(this, wearConditions);
			rigidLogic.Init(this, wearConditions);

			SetupModsShop(productPrice, productImage);


		}
		public void SetupModsShop(float productPrice, string productImage)
		{
			if (Cache.Find("Shop for mods") == null)
			{
				ModUI.ShowMessage("ModsShop not found in the game!!");
				return;
			}

			modsShop = Cache.Find("Shop for mods").GetComponent<ShopItem>();

			repairProduct = new ProductDetails
			{
				productName = String.Format("REPAIR {0}", part.activePart.name.Replace("(Clone)", "")),
				multiplePurchases = false,
				productCategory = "DonnerTech Racing",
				productIcon = mod.assetsBundle.LoadAsset<Sprite>(productImage),
				productPrice = 4000
			};
			modsShop.Add(mod, repairProduct, ShopType.Fleetari, RepairPurchase, null);

		}

		public void RepairPurchase(PurchaseInfo purchaseInfo)
		{
			repairPurchaseMade = true;

			if (part.IsInstalled || !part.activePart.activeSelf || !Helper.CheckCloseToPosition(part.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
			{
				ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
				PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repairProduct.productPrice;
				return;
			}

			wear = 100;
			//Color has to then also be reset

			part.activePart.transform.position = FleetariSpawnLocation.desk;
			part.activePart.SetActive(true);
		}

		public void ResetModsShopRepairPurchase()
		{
			repairPurchaseMade = false;
			List<ShopItems> shopItems = modsShop.fleetariShopItems;
			foreach (ShopItems shopItem in shopItems)
			{
				if (shopItem.details.productName == repairProduct.productName)
				{
					shopItem.purchashed = false;
				}
			}
		}

		public float CalculateWearResult(float valueToManipulate)
		{
			float manipulatedValue = valueToManipulate;
			float manipulator = 1;
			timer += Time.deltaTime;
			for (int i = 0; i < wearConditions.Count; i++)
			{
				if (multiplicatorManipulator != null)
				{
					manipulator = multiplicatorManipulator[i];
				}
				WearCondition wearCondition = wearConditions[i];
				WearCondition.Check check = wearCondition.conditionCheck;
				if (check == WearCondition.Check.Equal)
				{
					if (wear == wearCondition.check) { manipulatedValue /= wearCondition.divider; break; }
				}
				else if (check == WearCondition.Check.MoreThan)
				{
					if (wear >= wearCondition.check) { manipulatedValue /= wearCondition.divider; break; }
				}
				else if (check == WearCondition.Check.LessThan)
				{
					if (wear <= wearCondition.check) { manipulatedValue /= wearCondition.divider; break; }
					HandleRandomFalloff();
				}
			}

			if (timer >= wearReductionIntervall)
			{
				timer = 0;
				wear -= valueToManipulate * wearReductionMultiplier;
			}

			return manipulatedValue;
		}

		internal void HandleRandomFalloff()
		{
			if (wear <= 0)
			{
				part.removePart();
			}

			if (randomFallOff != -1)
			{
				int randomValue = new Random().Next(randomFallOff);
				if (randomValue == 1)
				{
					part.removePart();
				}
			}
		}
		internal static void Save(Mod mod, string saveFile, Wear[] wears)
		{
			try
			{
				Dictionary<string, float> save = new Dictionary<string, float>();
				foreach (Wear wear in wears)
				{
					save[wear.id] = wear.wear;
				}
				SaveLoad.SerializeSaveFile<Dictionary<string, float>>(mod, save, saveFile);
			}
			catch (Exception ex)
			{
				Logger.New("Error while trying to save wear information", ex);
			}
		}
	}
	*/
}
