using HutongGames.PlayMaker;
using MSCLoader;
using Parts;
using SatsumaTurboCharger.gui;
using SatsumaTurboCharger.parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
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

        public AdvPart part;


        public State state = State.NotPainting;

        PaintSystem_Logic activeLogic;
        PaintSystem_Logic rigidLogic;

        public GameObject sprayCanGameObject;
        public PlayMakerFSM sprayCanFsm;
        public FsmColor sprayCanColorFsm;
        public Material carPaintRegular;


        public Color color;
        public Color defaultColor;
        public bool setupDone = false;
        public bool useCarPaintMaterial = false;

        public PaintSystem(Dictionary<string, SaveableColor> partsColorSave, AdvPart part, Color defaultColor, bool useCarPaintMaterial = false, string nameOfMaterial = "Paintable")
        {
            this.part = part;
            this.defaultColor = defaultColor;
            this.useCarPaintMaterial = useCarPaintMaterial;

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

            Material[] materialCollecion = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material material in materialCollecion)
            {
                if (material.name == "CAR_PAINT_REGULAR")
                {
                    carPaintRegular = material;
                    break;
                }

            }

            activeLogic.Init(this, nameOfMaterial, color, part.activePart);
            rigidLogic.Init(this, nameOfMaterial, color, part.rigidPart);

            sprayCanGameObject = Game.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/SprayCan");
            sprayCanFsm = sprayCanGameObject.GetComponent<PlayMakerFSM>();
            sprayCanColorFsm = sprayCanFsm.FsmVariables.FindFsmColor("SprayColor");

            //CUSTOM COLORS // STILL NEEDED/USEFULL
            /*
            modSprayColors[0] = new Color(205f / 255, 205f / 255, 205f / 255, 1f);    // white
            modSprayColors[1] = new Color(40f / 255, 40f / 255, 40f / 255, 1f);       // black
            modSprayColors[2] = new Color(205f / 255, 0f / 255, 0f / 255, 1f);        // red
            modSprayColors[3] = new Color(0f / 255, 0f / 255, 220f / 255, 1f);        // blue
            modSprayColors[4] = new Color(130f / 255, 60f / 255, 0f / 255, 1f);       // brown
            modSprayColors[5] = new Color(250f / 255, 105f / 255, 0f / 255, 1f);      // orange
            modSprayColors[6] = new Color(190f / 255, 190f / 255, 0f / 255, 1f);      // yellow
            modSprayColors[7] = new Color(0f / 255, 120f / 255, 0f / 255, 1f);        // green
            modSprayColors[8] = new Color(0f / 255, 170f / 255, 210f / 255, 1f);      // lightblue
            modSprayColors[9] = new Color(130f / 255, 130f / 255, 130f / 255, 1f);    // grey
            modSprayColors[10] = new Color(220f / 255, 55f / 255, 220f / 255, 1f);     // pink
            modSprayColors[11] = new Color(0f / 255, 0f / 255, 220f / 255, 1f);        // turquoise
            modSprayColors[12] = new Color(40f / 255, 40f / 255, 40f / 255, 1f);       // mattblack
            */

            //PaintSystem //CAR_PAINT_REGULAR (Instance) // MAYBE STILL NEEDED/USEFULL
        }

        private void SetColor(Material[] materials, Color color)
        {
            this.color = color;
            foreach (Material material in materials)
            {
                material.SetColor("_Color", color);
            }
        }

        public void SetSprayCanColor(Color color)
        {
            this.color = color;
            SetColor(activeLogic.paintableMaterials, color);
            SetColor(rigidLogic.paintableMaterials, color);
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

        public static Dictionary<string, SaveableColor> CollectSave(PaintSystem[] paintSystems)
        {
            Dictionary<string, SaveableColor> save = new Dictionary<string, SaveableColor>();
            foreach (PaintSystem paintSystem in paintSystems)
            {
                save[paintSystem.part.id] = SaveableColor.ConvertToSaveable(paintSystem.color);
            }
            return save;
        }

        internal void SetMetal(float metal = 0f, float gloss = 0.5f)
        {
            foreach (Material material in activeLogic.paintableMaterials)
            {
                material.SetFloat("_Metallic", metal);
                material.SetFloat("_Glossiness", gloss);
            }
            foreach (Material material in rigidLogic.paintableMaterials)
            {
                material.SetFloat("_Metallic", metal);
                material.SetFloat("_Glossiness", gloss);
            }
        }
        internal static void Save(Mod mod, string saveFile, PaintSystem[] paintSystems)
        {
            try
            {
                Dictionary<string, SaveableColor> save = new Dictionary<string, SaveableColor>();
                foreach (PaintSystem paintSystem in paintSystems)
                {
                    save[paintSystem.part.id] = SaveableColor.ConvertToSaveable(paintSystem.color);
                }
                SaveLoad.SerializeSaveFile<Dictionary<string, SaveableColor>>(mod, save, saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save color information", ex);
            }
        }
    }
}
