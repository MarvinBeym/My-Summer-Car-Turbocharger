using SatsumaTurboCharger;
using ModApi.Attachable;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrewablePartAPI.V2;
using static SatsumaTurboCharger.parts.AdvPart;

namespace SatsumaTurboCharger.parts
{
    public class Box
    {
        public string id;
        public GameObject box;
        public bool bought;
        public int spawnedCounter = 0;
        public AdvPart[] parts;
        public BoxLogic logic;
        public Box(SatsumaTurboCharger mod, GameObject box, GameObject part_gameObject, string partName, int numberOfParts, AdvPart parent, Dictionary<string, bool> partsBuySave, Vector3[] installLocations, Vector3[] installRotations, bool dontCollideOnRigid = true)
        {
            AdvPartBaseInfo advPartBaseInfo = new AdvPartBaseInfo
            {
                mod = mod,
                partsBuySave = partsBuySave,
            };

            this.box = box;
            id = partName.ToLower().Replace(" ", "_");
            string boughtId = id + "_box";

            parts = new AdvPart[numberOfParts];

            for (int i = 0; i < numberOfParts; i++)
            {
                int iOffset = i + 1;

                parts[i] = new AdvPart(advPartBaseInfo, 
                    id + iOffset, partName + " " + iOffset, 
                    parent.part, part_gameObject, installLocations[i], installRotations[i],
                    dontCollideOnRigid, boughtId);

                if (!parts[i].bought)
                {
                    parts[i].removePart();
                    parts[i].activePart.SetActive(false);
                }
            }
            logic = box.AddComponent<BoxLogic>();
            logic.Init(mod, parts, "Unpack " + partName.ToLower(), this);
            foreach (AdvPart part in parts)
            {
                mod.partsList.Add(part);
            }
        }

        public void AddScrewable(ScrewablePartV2BaseInfo baseInfo, AssetBundle screwableAssetsBundle, ScrewV2[] screws)
        {
            foreach (AdvPart part in parts)
            {
                part.screwablePart = new ScrewablePartV2(baseInfo, part.id, part.rigidPart, screws);
            }
        }

        public void CheckUnpackedOnSave()
        {
            if (parts[0].bought)
            {
                if (spawnedCounter < parts.Length)
                {
                    foreach (AdvPart part in parts)
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
