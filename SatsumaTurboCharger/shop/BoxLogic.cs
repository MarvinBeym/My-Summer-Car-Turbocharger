using UnityEngine;
using ModApi;
using MSCLoader;
using System.Linq;
using Tools;
using Parts;

namespace ModShop
{
    public class BoxLogic : MonoBehaviour
    {
        private Box box;
        private string actionToDisplay;
        private Mod mod;
        private AdvPart[] parts;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Helper.DetectRaycastHitObject(this.gameObject) && box.spawnedCounter < parts.Length)
            {
                ModClient.guiInteraction = string.Format("Press [{0}] to {1}", cInput.GetText("Use"), actionToDisplay);
                if (Helper.UseButtonDown)
                {
                    AdvPart part = parts[box.spawnedCounter];

                    part.activePart.transform.position = this.gameObject.transform.position;

                    part.activePart.SetActive(true);
                    box.spawnedCounter++;
                }
            }
            if (box.spawnedCounter >= parts.Length)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void Init(Mod mod, AdvPart[] parts, string actionToDisplay, Box box)
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