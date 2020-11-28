using ModsShop;
using MSCLoader;
using SatsumaTurboCharger.parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger.wear
{
    public class Wear
    {
        public SatsumaTurboCharger mod;
        public string id = "";
        public AdvPart part;
        public WearLogic activeLogic;
        public WearLogic rigidLogic;

        public List<WearCondition> wearConditions;
        public float wear = 100;
        public float wearReductionMultiplier;
        public float wearReductionIntervall;
        public int randomFallOff = -1;

        public float timer = 0;

        //Shop
        private ShopItem modsShop;
        private ProductDetails repairProduct;
        public bool repairPurchaseMade = false;

        public Wear(SatsumaTurboCharger mod, string id, AdvPart part, List<WearCondition> wearConditions, float wearReductionMultiplier, float wearReductionIntervall, Dictionary<string, float> wearSave, float productPrice, string productImage, int randomFallOff = -1)
        {
            this.mod = mod;
            this.id = id;
            this.wearReductionMultiplier = wearReductionMultiplier;
            this.wearConditions = wearConditions;
            this.wearReductionIntervall = wearReductionIntervall;
            this.part = part;
            
            try
            {
                this.wear = wearSave[id];
            }
            catch
            {
                this.wear = 100;
            }
            this.randomFallOff = randomFallOff;


            activeLogic = part.activePart.AddComponent<WearLogic>();
            rigidLogic = part.rigidPart.AddComponent<WearLogic>();
            activeLogic.Init(this, wearConditions);
            rigidLogic.Init(this, wearConditions);

            SetupModsShop(productPrice, productImage);
            
            
        }
        public void SetupModsShop(float productPrice, string productImage)
        {
            if (GameObject.Find("Shop for mods") == null)
            {
                ModUI.ShowMessage("ModsShop not found in the game!!");
                return;
            }

            modsShop = GameObject.Find("Shop for mods").GetComponent<ShopItem>();

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

            if (part.installed || !part.activePart.activeSelf || !Helper.CheckCloseToPosition(part.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
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
                if(shopItem.details.productName == repairProduct.productName)
                {
                    shopItem.purchashed = false;
                }
            }
        }

        public float CalculateWearResult(float valueToManipulate)
        {
            float manipulatedValue = valueToManipulate;
            timer += Time.deltaTime;
            for (int i = 0; i < wearConditions.Count; i++)
            {
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
            if(wear <= 0)
            {
                part.removePart();
            }

            if(randomFallOff != -1)
            {
                int randomValue = new Random().Next(randomFallOff);
                if(randomValue == 1)
                {
                    part.removePart();
                }
            }
        }

        public Dictionary<string, float> GetWear(Dictionary<string, float> partsWear)
        {
            partsWear[id] = wear;
            return partsWear;
        }
    }
}
