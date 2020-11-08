using ModsShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.shop
{
    class ProductInformation
    {
        public GameObject gameObject { get; set; }
        public string productName { get; set; }
        public float price { get; set; }
        public string iconName { get; set; }
        public bool bought { get; set; }
        public string gameObjectName { get; set; }

        public ProductDetails product { get; set; }
        public ProductInformation(GameObject gameObject, string productName, float price, string iconName, bool bought)
        {
            this.gameObject = gameObject;
            this.productName = productName;
            this.price = price;
            this.iconName = iconName;
            this.bought = bought;
        }


    }
}
