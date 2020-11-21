using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using SatsumaTurboCharger.painting_system;
namespace SatsumaTurboCharger
{
    public class PaintSystem_Logic : MonoBehaviour
    {
        private PaintSystem paintSystem;
        public GameObject[] paintableChilds;
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

        public void Init(PaintSystem paintSystem, string[] childs, Color initalColor, GameObject sprayCanTrigger)
        {
            this.paintSystem = paintSystem;
            this.sprayCanTrigger = sprayCanTrigger;
            paintableChilds = GetChilds(this.gameObject, childs);
            paintSystem.SetAllChildColors(paintableChilds, initalColor);
        }

        private GameObject[] GetChilds(GameObject gameobject, string[] childNames)
        {
            List<GameObject> childs = new List<GameObject>();
            foreach (string name in childNames)
            {
                try
                {
                    childs.Add(gameobject.transform.FindChild(name).gameObject);
                }
                catch
                {

                }
            }
            return childs.ToArray();
        }
    }
}