using HutongGames.PlayMaker;
using MSCLoader;
using SatsumaTurboCharger.gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.painting_system
{

    public class PaintSystem
    {
        public enum State
        {
            Painting,
            NotPainting
        }

        public SimplePart part;


        public State state = State.NotPainting;

        PaintSystem_Logic activeLogic;
        PaintSystem_Logic rigidLogic;

        public GameObject sprayCanGameObject;
        public PlayMakerFSM sprayCanFsm;
        public FsmColor sprayCanColorFsm;

        public Color color;
        public Color defaultColor;
        public bool setupDone = false;

        public PaintSystem(Dictionary<string, SaveableColor> partsColorSave, SimplePart part, Color defaultColor, string[] paintablePartsName = null)
        {
            this.part = part;
            this.defaultColor = defaultColor;

            activeLogic = part.activePart.AddComponent<PaintSystem_Logic>();
            rigidLogic =  part.rigidPart.AddComponent<PaintSystem_Logic>();

            try
            {
                color = SaveableColor.ConvertToColor(partsColorSave[part.id]);
            }
            catch
            {
                color = defaultColor;
            }

            if(paintablePartsName == null || paintablePartsName.Length == 0)
            {
                paintablePartsName = new string[] { "default" };
            }

            activeLogic.Init(this, paintablePartsName, color, part.activePart);
            rigidLogic.Init(this, paintablePartsName, color, part.partTrigger.triggerGameObject);

            sprayCanGameObject = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/SprayCan");
            sprayCanFsm = sprayCanGameObject.GetComponent<PlayMakerFSM>();
            sprayCanColorFsm = sprayCanFsm.FsmVariables.FindFsmColor("SprayColor");
        }

        public void SetAllChildColors(GameObject[] childs, Color color)
        {
            foreach (GameObject child in childs)
            {
                foreach (Material material in child.GetComponent<MeshRenderer>().materials)
                {
                    material.SetColor("_Color", color);
                }
            }
        }

        public void SetSprayCanColor(Color color)
        {
            this.color = color;
            SetAllChildColors(activeLogic.paintableChilds, color);
            SetAllChildColors(rigidLogic.paintableChilds, color);
        }

        public void SetupPaintSystem()
        {
            setupDone = true;
            FsmHook.FsmInject(sprayCanGameObject, "Stage 1", delegate () { HookPainting(State.NotPainting); });
            FsmHook.FsmInject(sprayCanGameObject, "Painting", delegate () { HookPainting(State.Painting); });
        }

        private void HookPainting(State state)
        {
            this.state = state;
        }

        public Dictionary<string, SaveableColor> GetColor(Dictionary<string, SaveableColor> partsColorSave)
        {

            partsColorSave[part.id] = SaveableColor.ConvertToSaveable(color);
            return partsColorSave;
        }
    }
}
