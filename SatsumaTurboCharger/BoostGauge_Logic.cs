using MSCLoader;
using SatsumaTurboCharger.parts;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class BoostGauge_Logic : MonoBehaviour
    {
        private GameObject analogNeedle;
        private GameObject digitalText1;
        private GameObject digitalText2;
        private GameObject digitalText3;

        private float alteredBoost = 0;
        public float boostReduction = 0.15f;

        public void Init(AdvPart boostGaugePart)
        {
            analogNeedle = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_needle").gameObject;
            digitalText1 = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_digital_text_1").gameObject;
            digitalText2 = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_digital_text_2").gameObject;
            digitalText3 = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_digital_text_3").gameObject;
            DisableText(boostGaugePart.activePart);
        }

        public void SetBoost(float boost, float minBoost, float maxBoost, float minAngle, float maxAngle)
        {


            if (boost >= 0 && Helper.ThrottleButtonDown && alteredBoost == 0)
            {
                alteredBoost = boost;
                
            }

            if(alteredBoost > 0 && !Helper.ThrottleButtonDown)
            {
                alteredBoost -= boostReduction;
            }
            else if(alteredBoost <= boost && Helper.ThrottleButtonDown)
            {
                alteredBoost += boostReduction;
            }
            
            alteredBoost = alteredBoost >= maxBoost ? maxBoost : alteredBoost;
            alteredBoost = alteredBoost <= minBoost ? minBoost : alteredBoost;
            ModConsole.Print(alteredBoost);
            float angle = minAngle + (maxAngle - minAngle) * alteredBoost.Map(minBoost, 3, 0, 1);

            analogNeedle.transform.localEulerAngles = new Vector3(0, 0, angle);

        }

        public void DisableText(GameObject part)
        {
            GameObject digitalText1 = part.transform.FindChild("boost_gauge_digital_text_1").gameObject;
            GameObject digitalText2 = part.transform.FindChild("boost_gauge_digital_text_2").gameObject;
            GameObject digitalText3 = part.transform.FindChild("boost_gauge_digital_text_3").gameObject;
            GameObject digitalTextDots = part.transform.FindChild("boost_gauge_digital_text_dots").gameObject;
            digitalText1.SetActive(false);
            digitalText2.SetActive(false);
            digitalText3.SetActive(false);
            digitalTextDots.SetActive(false);
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}