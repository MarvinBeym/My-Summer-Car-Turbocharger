using UnityEngine;
using System.Collections;
using SatsumaTurboCharger;
using ModApi;
using MSCLoader;
using System.Linq;

namespace SatsumaTurboCharger
{
    public class BoxLogic : MonoBehaviour
    {
        private string actionToDisplay;
        private SatsumaTurboCharger mod;
        private RaycastHit hit;
        private int spawnedCounter = 0;
        private SimplePart[] parts;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Camera.main != null && spawnedCounter < parts.Length)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
                {
                    GameObject gameObjectHit;
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null && hit.collider)
                    {
                        if (gameObjectHit.name == this.gameObject.name)
                        {
                            ModClient.guiInteraction = string.Format("Press [{0}] to {1}", cInput.GetText("Use"), actionToDisplay);
                            if (useButtonDown)
                            {
                                SimplePart part = parts[spawnedCounter];

                                part.activePart.transform.position = hit.point;

                                part.activePart.SetActive(true);
                                spawnedCounter++;
                            }
                        }
                    }
                }
            }
            if(spawnedCounter >= parts.Length)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void Init(SatsumaTurboCharger mod, SimplePart[] parts, string actionToDisplay)
        {
            this.mod = mod;
            this.parts = parts;
            this.actionToDisplay = actionToDisplay;
        }

        public void CheckBoxPosReset(bool boughtBox)
        {
            if(boughtBox)
            {
                if(!parts.Any(part => part.installed || part.activePart.activeSelf))
                {
                    this.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
                }
            }
        }

        public void CheckUnpackedOnSave(bool boughtBox)
        {
            if(boughtBox)
            {
                if (spawnedCounter < parts.Length)
                {
                    foreach (SimplePart part in parts)
                    {
                        if (!part.installed && !part.activePart.activeSelf)
                        {
                            part.activePart.transform.position = this.gameObject.transform.position;
                            part.activePart.SetActive(true);
                        }
                    }
                }
                this.gameObject.SetActive(false);
                this.gameObject.transform.position = new Vector3(0, 0, 0);
                this.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            }
        }

        private bool useButtonDown
        {
            get { return cInput.GetKeyDown("Use"); }
        }
    }
}