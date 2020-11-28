using SatsumaTurboCharger;
using ModApi.Attachable;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.parts
{
    public class Box
    {
        public string id;
        public GameObject box;
        public bool bought;
        private GameObject[] part_gameObjects;
        public int spawnedCounter = 0;
        public SimplePart[] parts;
        private string[] saveFiles;
        public BoxLogic logic;
        public Box(SatsumaTurboCharger mod, GameObject box, GameObject part_gameObject, string partName, int numberOfParts, SimplePart parent, Dictionary<string, bool> partsBuySave, Vector3[] installLocations, Vector3[] rotations)
        {
            this.box = box;
            id = partName.ToLower().Replace(" ", "_");
            string boughtId = id + "_box";

            try
            {
                bought = partsBuySave[boughtId];
            }
            catch
            {
                bought = false;
            }

            part_gameObjects = new GameObject[numberOfParts];
            saveFiles = new string[numberOfParts];
            parts = new SimplePart[numberOfParts];

            for (int i = 0; i < numberOfParts; i++)
            {
                int iOffset = i + 1;
                part_gameObjects[i] = GameObject.Instantiate(part_gameObject);
                Helper.SetObjectNameTagLayer(part_gameObjects[i], partName + " " + iOffset + "(Clone)");
                saveFiles[i] = id + iOffset + "_saveFile.txt";

                parts[i] = new SimplePart(
                    SimplePart.LoadData(mod, id + iOffset, partsBuySave, boughtId),
                    part_gameObjects[i],
                    parent.rigidPart,
                    installLocations[i],
                    new Quaternion { eulerAngles = rotations[i] }
                );

                if (!bought)
                {
                    parts[i].removePart();
                    parts[i].activePart.SetActive(false);
                }
            }
            logic = box.AddComponent<BoxLogic>();
            logic.Init(mod, parts, "Unpack " + partName.ToLower(), this);
            foreach (SimplePart part in parts)
            {
                mod.partsList.Add(part);
            }
        }

        public void AddScrewable(SortedList<string, Screws> screwListSave, AssetBundle screwableAssetsBundle, Screw[] screws)
        {
            foreach (SimplePart part in parts)
            {
                part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, part.rigidPart, screws);
            }
        }

        public void CheckUnpackedOnSave()
        {
            if (parts[0].bought)
            {
                if (spawnedCounter < parts.Length)
                {
                    foreach (SimplePart part in parts)
                    {
                        if (!part.installed && !part.activePart.activeSelf)
                        {
                            part.activePart.transform.position = box.transform.position;
                            part.activePart.SetActive(true);
                        }
                    }
                }
                box.SetActive(false);
                box.transform.position = new Vector3(0, 0, 0);
                box.transform.localPosition = new Vector3(0, 0, 0);
            }
        }
    }
}
