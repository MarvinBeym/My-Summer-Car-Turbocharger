using UnityEngine;
using System.Collections;
using SatsumaTurboCharger;
using ModApi;
using MSCLoader;
using System.Linq;
using SatsumaTurboCharger.parts;

namespace SatsumaTurboCharger
{
    public class BoxLogic : MonoBehaviour
    {
        private Box box;
        private string actionToDisplay;
        private SatsumaTurboCharger mod;
        private AdvPart[] parts;
        private RaycastHit hit;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Camera.main != null && box.spawnedCounter < parts.Length && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
            {
                ModClient.guiInteraction = string.Format("Press [{0}] to {1}", cInput.GetText("Use"), actionToDisplay);
                if (Helper.UseButtonDown)
                {
                    AdvPart part = parts[box.spawnedCounter];

                    part.activePart.transform.position = hit.point;

                    part.activePart.SetActive(true);
                    box.spawnedCounter++;
                }
            }
            if (box.spawnedCounter >= parts.Length)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void Init(SatsumaTurboCharger mod, AdvPart[] parts, string actionToDisplay, Box box)
        {
            this.box = box;
            this.mod = mod;
            this.parts = parts;
            this.actionToDisplay = actionToDisplay;
        }

        public void CheckBoxPosReset(bool boughtBox)
        {
            if (boughtBox)
            {
                if (!parts.Any(part => part.installed || part.activePart.activeSelf))
                {
                    this.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
                }
            }
        }
    }
}