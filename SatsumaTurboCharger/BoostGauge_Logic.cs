﻿using MSCLoader;
using SatsumaTurboCharger.parts;
using SatsumaTurboCharger.turbo;
using UnityEngine;
using ModApi;
using System;
using Tools;

using Parts;

namespace SatsumaTurboCharger
{
    public class BoostGauge_Logic : MonoBehaviour
    {
        public enum GaugeMode
        {
            Analog,
            Digital,
        };

        private AdvPart boostGauge;
        private GameObject analogDigitalSwitch;
        private GameObject analogNeedle;
        private TextMesh digitalText;
        private Animation analogNeedleAnimation;
        private int selectedColor = 0;
        private Color[] availableColors = new Color[]
        {
            Color.white,
            Color.blue,
            Color.red,
            Color.green,
            Color.yellow,
            Color.magenta,
        };


        private Material foregroundMaterial;

        public GaugeMode gaugeMode = GaugeMode.Analog;

        private const float minAngle = 45;
        private const float maxAngle = 315;

        private float boostSaved = 0;
        public float time = 0;
        public float timeComparer = 0.01f;
        public float reducer = 0.15f;

        private bool lastElectricityState = false;

        public void Init(AdvPart boostGauge)
        {
            this.boostGauge = boostGauge;

            boostGauge.activePart.transform.FindChild("boost_gauge_digital_text").gameObject.SetActive(false);

            analogDigitalSwitch = this.transform.FindChild("boost_gauge_button").gameObject;
            
            analogNeedle = this.transform.FindChild("boost_gauge_needle").gameObject;
            analogNeedleAnimation = analogNeedle.GetComponent<Animation>();

            GameObject digitalTextObject = this.transform.FindChild("boost_gauge_digital_text").gameObject;
            foreach(Material material in this.transform.FindChild("boost_gauge__").GetComponent<Renderer>().materials)
            {
                if(!material.name.Contains("boost_gauge_foreground")) { continue; }
                foregroundMaterial = material;
            }
            foregroundMaterial.SetColor("_Color", availableColors[selectedColor]);

            try
            {
                MeshRenderer meshRenderer = digitalTextObject.GetComponent<MeshRenderer>();
                digitalText = digitalTextObject.GetComponent<TextMesh>();

                FsmHook.FsmInject(CarH.electricity, "ON", delegate() 
                {
                    if (lastElectricityState == false) { lastElectricityState = true; SwitchedElectricityOn(); };
                });
                FsmHook.FsmInject(CarH.electricity, "OFF", delegate()
                {
                    if (lastElectricityState == true) { lastElectricityState = false; SwitchedElectricityOff(); };
                });

                GameObject airFuel = Game.Find("AirFuel");
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

                Color color = Color.white;
                color.a = 0.2f;
                foregroundMaterial.SetColor("_Color", color);
            }
            catch(Exception ex)
            {
                Logger.New("Setup of boost gauge digital display failed", ex);
            }
            
        }

        void Start()
        {
            analogNeedle.transform.localEulerAngles = new Vector3(0, 0, minAngle);
            digitalText.text = "";
        }

        void Update()
        {
            if(!CarH.hasPower || !Helper.DetectRaycastHitObject(analogDigitalSwitch, "Dashboard")) { return; }
            GaugeMode nextGaugeMode = gaugeMode == GaugeMode.Analog ? GaugeMode.Digital : GaugeMode.Analog;
            ModClient.guiInteract(
                $"[Left mouse] or [{cInput.GetText("Use")}] to switch to {nextGaugeMode}\n" +
                $"[SCROLL UP/Down] to change color"
            );
            switch (Helper.ScrollingUpDown())
            {
                case 1:
                    selectedColor += 1;
                    ChangeTextColor(selectedColor);
                    break;
                case -1:
                    selectedColor -= 1;
                    ChangeTextColor(selectedColor);
                    break;
            }

            if (Helper.UseButtonDown || Helper.LeftMouseDown)
            {
                SwitchGaugeMode(nextGaugeMode);
            }
        }

        private void ChangeTextColor(int newColorIndex)
        {
            newColorIndex = newColorIndex > availableColors.Length - 1 ? 0 : newColorIndex;
            newColorIndex = newColorIndex < 0 ? availableColors.Length - 1 : newColorIndex;
            selectedColor = newColorIndex;
            Color color = availableColors[newColorIndex];
            color.a = 0.6f;


            foregroundMaterial.SetColor("_Color", color);
        }

        private void SwitchedElectricityOn()
        {
            if(gaugeMode == GaugeMode.Analog)
            {
                analogNeedleAnimation.Play();
            }
            else
            {
                digitalText.text = "0.00";
            }
            ChangeTextColor(selectedColor);
        }
        private void SwitchedElectricityOff()
        {
            if (analogNeedleAnimation.isPlaying)
            {
                analogNeedleAnimation.Stop();
            }
            analogNeedle.transform.localEulerAngles = new Vector3(0, 0, minAngle);
            digitalText.text = "";

            Color color = Color.white;
            color.a = 0.2f;
            foregroundMaterial.SetColor("_Color", color);
        }

        private void SwitchGaugeMode(GaugeMode newGaugeMode)
        {
            Helper.PlayTouchSound(boostGauge.rigidPart);
            gaugeMode = newGaugeMode;
            switch (gaugeMode)
            {
                case GaugeMode.Analog:
                    digitalText.text = "";
                    break;
                case GaugeMode.Digital:
                    digitalText.text = "0.00";
                    analogNeedle.transform.localEulerAngles = new Vector3(0, 0, minAngle);
                    break;
            }
        }

        public void SetBoost(float target, float boost, TurboConfiguration turboConfig)
        {
            if (!CarH.hasPower || analogNeedleAnimation.isPlaying) { return; }
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
                    SetDigitalText(manipulatedBoostValue.ToString("0.00"));
                    break;
            }
        }

        internal void SetDigitalText(string text)
        {
            digitalText.text = text;
        }

        private float GetNeedleAngle(float valueMap, float minMap = 0f, float maxMap = 3)
        {
            return minAngle + (maxAngle - minAngle) * valueMap.Map(minMap, maxMap, 0, 1);
        }
    }
}