using MSCLoader;
using SatsumaTurboCharger.parts;
using SatsumaTurboCharger.turbo;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class BoostGauge_Logic : MonoBehaviour
    {
        private GameObject analogNeedle;
        private GameObject digitalText1;
        private GameObject digitalText2;
        private GameObject digitalText3;

        private Drivetrain drivetrain;
        public void Init(AdvPart boostGaugePart)
        {
            drivetrain = GameObject.Find("SATSUMA(557kg, 248)").GetComponent<Drivetrain>();
            analogNeedle = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_needle").gameObject;
            digitalText1 = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_digital_text_1").gameObject;
            digitalText2 = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_digital_text_2").gameObject;
            digitalText3 = boostGaugePart.rigidPart.transform.FindChild("boost_gauge_digital_text_3").gameObject;
            DisableText(boostGaugePart.activePart);
        }

        private float boostSaved = 0;

        public float time = 0;
        public float timeComparer = 0.01f;
        public float reducer = 0.15f;
        public void SetBoost(float target, float boost, TurboConfiguration turboConfig, float minAngle, float maxAngle)
        {

            if (cInput.GetKeyUp("Throttle") && cInput.GetKeyDown("Throttle"))
            {
                boostSaved = boost;
            }

            if (!Helper.ThrottleButtonDown && boostSaved > target)
            {
                time += Time.deltaTime;
                if(time >= timeComparer)
                {
                    time = 0;
                    boostSaved -= reducer;
                }
                boostSaved = boostSaved <= turboConfig.boostMin ? turboConfig.boostMin : boostSaved;

                analogNeedle.transform.localEulerAngles = new Vector3(0, 0, GetNeedleAngle(minAngle, maxAngle, boostSaved));
                return;
            }
            else if(Helper.ThrottleButtonDown && boostSaved < target)
            {
                time += Time.deltaTime;
                if(time >= timeComparer)
                {
                    time = 0;
                    boostSaved += reducer;
                }
                boostSaved = boostSaved <= turboConfig.boostMin ? turboConfig.boostMin : boostSaved;

                analogNeedle.transform.localEulerAngles = new Vector3(0, 0, GetNeedleAngle(minAngle, maxAngle, boostSaved));
                return;
            }
            else if (Helper.ThrottleButtonDown)
            {
                analogNeedle.transform.localEulerAngles = new Vector3(0, 0, GetNeedleAngle(minAngle, maxAngle, boost));
                return;
            }
        }

        private float GetNeedleAngle(float minAngle, float maxAngle, float valueMap, float minMap = -0.1f, float maxMap = 3)
        {
            return minAngle + (maxAngle - minAngle) * valueMap.Map(minMap, maxMap, 0, 1);
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