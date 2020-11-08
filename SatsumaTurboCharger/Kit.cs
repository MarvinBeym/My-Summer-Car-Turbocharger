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
        public SimplePart[] parts;
        private KitLogic logic;
        public int spawnedCounter = 0;

        public Kit(SatsumaTurboCharger mod, GameObject kitBox, SimplePart[] simpleParts, bool boughtKit)
        {
            this.mod = mod;
            this.kitBox = kitBox;
            this.parts = simpleParts;

            if (!boughtKit)
            {
                foreach(SimplePart part in parts)
                {
                    part.removePart();
                    part.activePart.SetActive(false);
                }
            }

            logic = kitBox.AddComponent<KitLogic>();
            logic.Init(mod, this);
        }

        public void CheckUnpackedOnSave(bool boughtKit)
        {
            if (boughtKit)
            {
                if (spawnedCounter < parts.Length)
                {
                    foreach (SimplePart part in parts)
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
