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
        private GameObject[] part_gameObjects;
        public SimplePart[] parts;
        private string[] saveFiles;
        public BoxLogic logic;
        public Box(SatsumaTurboCharger mod, GameObject box_gameObject, GameObject part_gameObject, string saveFilePrefix, int numberOfParts, string partName, SimplePart parent, bool bought_box, Vector3[] installLocations, Vector3[] rotations)
        {
            string partNameLowerCase = partName.ToLower();
            part_gameObjects = new GameObject[numberOfParts];
            saveFiles = new string[numberOfParts];
            parts = new SimplePart[numberOfParts];

            for(int i = 0; i < numberOfParts; i++)
            {
                int iOffset = i + 1;
                part_gameObjects[i] = GameObject.Instantiate(part_gameObject);
                Helper.SetObjectNameTagLayer(part_gameObjects[i], partName + " " + iOffset + "(Clone)");
                saveFiles[i] = saveFilePrefix + iOffset + "_saveFile.txt";

                parts[i] = new SimplePart(
                    SimplePart.LoadData(mod, saveFiles[i], bought_box),
                    part_gameObjects[i],
                    parent.rigidPart,
                    installLocations[i],
                    new Quaternion { eulerAngles = rotations[i] }
                );
            }
            logic = box_gameObject.AddComponent<BoxLogic>();
            logic.Init(mod, parts, "Unpack " + partNameLowerCase);
            foreach (SimplePart part in parts)
            {
                mod.partsList.Add(part);
            }
        }

        public void AddScrewable(SortedList<string, Screws> screwListSave, AssetBundle screwableAssetsBundle, Screw[] screws)
        {
            foreach(SimplePart part in parts)
            {
                part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, part.rigidPart, screws);
            }
        }
    }
}
