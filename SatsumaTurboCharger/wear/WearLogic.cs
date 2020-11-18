using ModApi;
using MSCLoader;
using SatsumaTurboCharger.wear;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class WearLogic : MonoBehaviour
    {
        private Wear wear;
        private List<WearCondition> wearConditions;
        private RaycastHit hit;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (wear.repairPurchaseMade)
            {
                wear.ResetModsShopRepairPurchase();
            }

            if (!wear.part.installed && Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
            {
                GameObject gameObjectHit;
                gameObjectHit = hit.collider?.gameObject;
                if (gameObjectHit != null && hit.collider)
                {
                    if (gameObjectHit.name == wear.part.activePart.name)
                    {
                        for (int i = 0; i < wearConditions.Count; i++)
                        {
                            WearCondition wearCondition = wearConditions[i];
                            WearCondition.Check check = wearCondition.conditionCheck;
                            if (check == WearCondition.Check.Equal)
                            {
                                if (wear.wear == wearCondition.check) { ModClient.guiInteraction = wearCondition.guiInteractionText; break; }
                            }
                            else if (check == WearCondition.Check.MoreThan)
                            {
                                if (wear.wear >= wearCondition.check) { ModClient.guiInteraction = wearCondition.guiInteractionText; break; }
                            }
                            else if (check == WearCondition.Check.LessThan)
                            {
                                if (wear.wear <= wearCondition.check) { ModClient.guiInteraction = wearCondition.guiInteractionText; break; }
                            }
                        }
                    }
                }
            }

        }


        internal void Init(Wear wear, List<WearCondition> wearConditions)
        {
            this.wear = wear;
            this.wearConditions = wearConditions;
        }
    }
}