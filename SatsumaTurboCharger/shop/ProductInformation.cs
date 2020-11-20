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
        public GameObject gameObject;
        public SimplePart part;
        public SimplePart[] parts;
        public bool usingSimplePart = false;

        public string productName;
        public float price;
        public string iconName;
        public bool bought;
        public string gameObjectName;

        public ProductDetails product;
        public ProductInformation(Kit kit, string productName, float price, string iconName, bool bought)
        {
            this.gameObject = kit.kitBox;
            parts = kit.parts;
            this.productName = productName;
            this.price = price;
            this.iconName = iconName;
            this.bought = bought;
        }

        public ProductInformation(SimplePart part, string productName, float price, string iconName)
        {
            usingSimplePart = true;
            this.part = part;
            this.gameObject = part.activePart;
            this.productName = productName;
            this.price = price;
            this.iconName = iconName;
            this.bought = part.bought;
        }


    }
}
