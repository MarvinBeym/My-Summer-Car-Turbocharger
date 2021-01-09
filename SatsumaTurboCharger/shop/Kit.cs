﻿using Parts;
using SatsumaTurboCharger.parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class Kit
    {
        private SatsumaTurboCharger mod;
        public GameObject kitBox;
        public AdvPart[] parts;
        private KitLogic logic;
        public int spawnedCounter = 0;
        public string boughtId;
        public bool bought;
        public Kit(SatsumaTurboCharger mod, GameObject kitBox, AdvPart[] simpleParts)
        {
            this.mod = mod;
            this.kitBox = kitBox;
            this.parts = simpleParts;
            boughtId = simpleParts[0].boughtId;
            bought = simpleParts[0].bought;
            if (!bought)
            {
                foreach (AdvPart part in parts)
                {
                    part.removePart();
                    part.activePart.SetActive(false);
                }
            }

            logic = kitBox.AddComponent<KitLogic>();
            logic.Init(mod, this);
        }

        public void CheckUnpackedOnSave()
        {
            if (parts[0].bought)
            {
                if (spawnedCounter < parts.Length)
                {
                    foreach (AdvPart part in parts)
                    {
                        if (!part.installed && !part.activePart.activeSelf)
                        {
                            part.activePart.transform.position = kitBox.transform.position;
                            part.activePart.SetActive(true);
                        }
                    }
                }
                kitBox.SetActive(false);
                kitBox.transform.position = new Vector3(0, 0, 0);
                kitBox.transform.localPosition = new Vector3(0, 0, 0);
            }
        }
    }
}
