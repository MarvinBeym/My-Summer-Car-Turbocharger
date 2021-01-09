using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using SatsumaTurboCharger.painting_system;
using Tools;

namespace SatsumaTurboCharger
{
    public class PaintSystem_Logic : MonoBehaviour
    {
        private PaintSystem paintSystem;
        public GameObject[] paintableChilds;

        public Material[] paintableMaterials;

        private GameObject sprayCanTrigger;

        // Use this for initialization
        void Start()
        {

        }
        


        // Update is called once per frame
        void Update()
        {
            if (!paintSystem.setupDone && paintSystem.sprayCanGameObject.activeSelf)
            {
                paintSystem.SetupPaintSystem();
            }
            if (!paintSystem.setupDone) { return; }

            if (paintSystem.state == PaintSystem.State.Painting && Helper.DetectRaycastHitObject(sprayCanTrigger, LayerMask.LayerToName(sprayCanTrigger.layer)))
            {
                paintSystem.SetSprayCanColor(paintSystem.sprayCanColorFsm.Value);
            }
        }

        public void Init(PaintSystem paintSystem, string nameOfMaterial, Color initalColor, GameObject sprayCanTrigger)
        {
            this.paintSystem = paintSystem;
            this.sprayCanTrigger = sprayCanTrigger;

            List<Material> materials = new List<Material>();
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                foreach(Material material in renderer.materials)
                {
                    if (material.name.Contains(nameOfMaterial)) 
                    {
                        if (paintSystem.useCarPaintMaterial)
                        {
                            material.shader = paintSystem.carPaintRegular.shader;
                            //material.SetTexture("_MainTex", paintSystem.carPaintRegular.mainTexture);
                            material.SetFloat("_Glossiness", paintSystem.carPaintRegular.GetFloat("_Glossiness"));
                            material.SetTexture("_SpecGlossMap", paintSystem.carPaintRegular.GetTexture("_SpecGlossMap"));
                        }
                        material.SetColor("_Color", initalColor);

                        materials.Add(material);
                        
                    }
                }
            }
            paintableMaterials = materials.ToArray();
        }
    }
}