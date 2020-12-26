using MSCLoader;
using SatsumaTurboCharger.parts;
using SatsumaTurboCharger.turbo;
using UnityEngine;
using ModApi;
using System;

namespace SatsumaTurboCharger
{
    public class BoostGauge_Logic : MonoBehaviour
    {
        private enum GaugeMode
        {
            Analog,
            Digital,
        };

        private AdvPart boostGauge;
        private GameObject analogDigitalSwitch;
        private GameObject analogNeedle;
        private TextMesh digitalText;

        private Drivetrain drivetrain;

        private GaugeMode gaugeMode = GaugeMode.Analog;

        private const float minAngle = 45;
        private const float maxAngle = 315;

        private float boostSaved = 0;
        public float time = 0;
        public float timeComparer = 0.01f;
        public float reducer = 0.15f;

        public void Init(AdvPart boostGauge)
        {
            this.boostGauge = boostGauge;
            drivetrain = GameObject.Find("SATSUMA(557kg, 248)").GetComponent<Drivetrain>();

            boostGauge.activePart.transform.FindChild("boost_gauge_digital_text").gameObject.SetActive(false);

            analogDigitalSwitch = boostGauge.rigidPart.transform.FindChild("boost_gauge_button").gameObject;
            
            analogNeedle = boostGauge.rigidPart.transform.FindChild("boost_gauge_needle").gameObject;
            GameObject digitalTextObject = boostGauge.rigidPart.transform.FindChild("boost_gauge_digital_text").gameObject;



            //Setting up boost gauge digital display
            try
            {
                MeshRenderer meshRenderer = digitalTextObject.GetComponent<MeshRenderer>();
                digitalText = digitalTextObject.GetComponent<TextMesh>();


                GameObject airFuel = GameObject.Find("AirFuel");
                GameObject lcd = airFuel.transform.FindChild("LCD").gameObject;
                MeshRenderer lcdMeshRenderer = lcd.GetComponent<MeshRenderer>();
                TextMesh lcdTextMesh = lcd.GetComponent<TextMesh>();

                //Coping values from af ratio gauge.
                meshRenderer.material = lcdMeshRenderer.material;

                digitalText.transform.localPosition = new Vector3(0f, -0.0135f, 0.0135f);
                digitalText.font = lcdTextMesh.font;
                digitalText.fontSize = 0;
                digitalText.characterSize = 1.55f;
                digitalText.transform.localScale = lcdTextMesh.transform.localScale;
            }
            catch(Exception ex)
            {
                Logger.New("Setup of boost gauge digital display failed", ex);
            }
            
        }

        void Start()
        {

        }

        void Update()
        {
            if(Helper.DetectRaycastHitObject(analogDigitalSwitch, "Dashboard"))
            {
                GaugeMode nextGaugeMode = gaugeMode == GaugeMode.Analog ? GaugeMode.Digital : GaugeMode.Analog;
                ModClient.guiInteract(
                    $"[Left mouse] or [{cInput.GetText("Use")}]\n" +
                    $"to switch to {nextGaugeMode}"
                );
                if(Helper.UseButtonDown || Helper.LeftMouseDown)
                {
                    SwitchGaugeMode(nextGaugeMode);
                }
            }
        }

        private void SwitchGaugeMode(GaugeMode newGaugeMode)
        {
            Helper.playTouchSound(boostGauge.rigidPart);
            gaugeMode = newGaugeMode;
            switch (gaugeMode)
            {
                case GaugeMode.Analog:
                    digitalText.text = "";
                    break;
                case GaugeMode.Digital:
                    analogNeedle.transform.localEulerAngles = new Vector3(0, 0, minAngle);
                    break;
            }
        }

        public void SetBoost(float target, float boost, TurboConfiguration turboConfig)
        {
            float boostMin = 0;


            float manipulatedBoostValue = 0;
            if (cInput.GetKeyUp("Throttle") && cInput.GetKeyDown("Throttle"))
            {
                boostSaved = boost;
            }

            if (!Helper.ThrottleButtonDown && boostSaved > target)
            {
                time += Time.deltaTime;
                if (time >= timeComparer)
                {
                    time = 0;
                    boostSaved -= reducer;
                }
                boostSaved = boostSaved <= boostMin ? boostMin : boostSaved;
                manipulatedBoostValue = boostSaved;
            }
            else if (Helper.ThrottleButtonDown && boostSaved < target)
            {
                time += Time.deltaTime;
                if (time >= timeComparer)
                {
                    time = 0;
                    boostSaved += reducer;
                }
                boostSaved = boostSaved <= boostMin ? boostMin : boostSaved;
                manipulatedBoostValue = boostSaved;
            }
            else if (Helper.ThrottleButtonDown)
            {
                manipulatedBoostValue = boost;
            }

            switch (gaugeMode)
            {
                case GaugeMode.Analog:
                    analogNeedle.transform.localEulerAngles = new Vector3(0, 0, GetNeedleAngle(manipulatedBoostValue));
                    break;
                case GaugeMode.Digital:
                    digitalText.text = manipulatedBoostValue.ToString("0.00");
                    break;
            }
        }

        private float GetNeedleAngle(float valueMap, float minMap = 0f, float maxMap = 3)
        {
            return minAngle + (maxAngle - minAngle) * valueMap.Map(minMap, maxMap, 0, 1);
        }
    }
}