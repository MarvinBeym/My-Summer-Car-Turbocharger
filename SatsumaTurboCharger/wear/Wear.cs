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
        public SimplePart part;
        public WearLogic activeLogic;


        public List<WearCondition> wearConditions;
        public float wear = 100;
        public float wearReductionMultiplier;
        public float wearReductionIntervall;
        public int randomFallOff = -1;

        public float timer = 0;

        public Wear(SimplePart part, List<WearCondition> wearConditions, float wearReductionMultiplier, float wearReductionIntervall, int randomFallOff = -1, float baseWear = 100)
        {
            this.wearReductionMultiplier = wearReductionMultiplier;
            this.wearConditions = wearConditions;
            this.wearReductionIntervall = wearReductionIntervall;
            this.part = part;
            this.wear = baseWear;
            this.randomFallOff = randomFallOff;


            activeLogic = part.activePart.AddComponent<WearLogic>();
            activeLogic.Init(this, wearConditions);
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
    }
}
