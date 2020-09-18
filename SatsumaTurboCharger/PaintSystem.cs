using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class PaintSystem : MonoBehaviour
    {

        private MeshRenderer[] sprayCansMeshRenders;
        private Material regularCarPaintMaterial;
        private MeshRenderer turbocharger_hood_renderer;

        private FsmGameObject itemInHand;
        private bool isItemInHand = false;

        // Use this for initialization
        void Start()
        {
            itemInHand = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("ItemPivot");

            Material[] materialCollecion = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material material in materialCollecion)
            {
                if (material.name == "CAR_PAINT_REGULAR")
                {
                    regularCarPaintMaterial = material;
                    break;
                }

            }
        }

        // Update is called once per frame
        void Update()
        {
            isItemInHand = GetIsItemInHand();
        }

        private bool GetIsItemInHand()
        {
            bool returnValue = false;
            if (itemInHand.Value.GetComponentInChildren<MeshRenderer>() != null)
            {
                return isItemInHand;
            }

            if (itemInHand.Value.GetComponentInChildren<MeshRenderer>().name == "spray can(itemx)" && Input.GetKey(KeyCode.F))
            {
                sprayCansMeshRenders = itemInHand.Value.GetComponentsInChildren<MeshRenderer>();
                returnValue = true;
            }
            else if (itemInHand.Value.GetComponentInChildren<MeshRenderer>().name == "spray can(itemx)" && isItemInHand == true)
            {
                returnValue = false;
            }

            return returnValue;
        }
    }
}