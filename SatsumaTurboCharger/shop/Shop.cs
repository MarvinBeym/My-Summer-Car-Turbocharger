using SatsumaTurboCharger.shop;
using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger
{
    class Shop
    {
        private SatsumaTurboCharger mod;
        private ShopItem modsShopItem;
        private AssetBundle assetBundle;
        private PartBuySave partBuySave;
        private List<ProductInformation> shopItems;

        public Shop(SatsumaTurboCharger mod, ShopItem modsShopItem, AssetBundle assetBundle, PartBuySave partBuySave, List<ProductInformation> shopItems)
        {
            this.mod = mod;
            this.modsShopItem = modsShopItem;
            this.assetBundle = assetBundle;
            this.partBuySave = partBuySave;
            this.shopItems = shopItems;
        }

        private void PurchaseMade(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            shopItems.ForEach(delegate (ProductInformation productInformation)
            {
                if(productInformation.gameObjectName == item.gameObject.name)
                {
                    SetPartBought(true, productInformation);
                }
            });
        }
        
        private void SetPartBought(bool bought, ProductInformation productInformation)
        {
            productInformation.bought = bought;
            switch (productInformation.productName)
            {
                case "Racing Turbocharger Kit": partBuySave.bought_turboBig_kit = bought; break;
                case "Racing Turbocharger Straight Exhaust": partBuySave.bought_turboBig_exhaust_outlet_straight = bought; break;
                case "Racing Turbocharger Blowoff Valve": partBuySave.bought_turboBig_blowoff_valve = bought; break;
                case "Racing Turbocharger Hood": partBuySave.bought_turboBig_hood = bought; break;

                case "GT Turbocharger Intercooler Tube": partBuySave.bought_turboSmall_intercooler_tube = bought; break;
                case "GT Turbocharger Airfilter": partBuySave.bought_turboSmall_airfilter = bought; break;

                case "TwinCarb Manifold Kit": partBuySave.bought_manifold_twinCarb_kit = bought; break;
                case "Weber Manifold Kit": partBuySave.bought_manifold_weber_kit = bought; break;

                case "Intercooler": partBuySave.bought_intercooler = bought; break;
                case "Boost Gauge": partBuySave.bought_boost_gauge = bought; break;
                case "Turbocharger Exhaust Header": partBuySave.bought_exhaust_header = bought; break;
            }
    }

        private void AddToShop(ProductInformation productInformation)
        {
            if (!productInformation.bought)
            {
                this.modsShopItem.Add(mod, productInformation.product, ModsShop.ShopType.Fleetari, PurchaseMade, productInformation.gameObject);
                productInformation.gameObject.SetActive(false);
            }
        }

        public void SetupShopItems()
        {
            shopItems.ForEach(delegate (ProductInformation productInformation)
            {
                Sprite productIcon = null;
                if (productInformation.iconName != null && productInformation.iconName != "")
                {
                    productIcon = assetBundle.LoadAsset<Sprite>(productInformation.iconName);
                }
                ProductDetails product = new ModsShop.ProductDetails
                {
                    productName = productInformation.productName,
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = productIcon,
                    productPrice = productInformation.price
                };
                productInformation.product = product;
                productInformation.gameObjectName = productInformation.gameObject.name;
                AddToShop(productInformation);
            });
        }
    }
}
