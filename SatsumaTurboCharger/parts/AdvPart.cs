using ModApi.Attachable;
using MSCLoader;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.parts
{
    public class AdvPart
    {
        private Mod mod;
        public NewPart part;
        private GameObject partGameObject;
        public string id;
        public string boughtId;
        public string saveFile;

        private const float triggerSize = 0.08f;
        public ScrewablePart screwablePart = null;

        public NewPart parentPart;
        public List<NewPart> childParts;
        public bool bought;

        public void DefineBaseInfo(Mod mod, string id, Dictionary<string, bool> partsBuySave, string boughtId)
        {
            this.mod = mod;
            this.id = id;
            this.saveFile = id + "_saveFile.json";

            if (boughtId == null || boughtId == "")
            {
                this.boughtId = id;
            }
            else
            {
                this.boughtId = boughtId;
            }
            this.bought = CheckBought(partsBuySave, this.boughtId);
        }
        public void DefineBasePartCalls(NewPart part)
        {
            part.SetOnAssemble(new Action(OnAssemble));
            part.SetOnDisassemble(new Action(OnDisassemble));
            FixRigidPartNaming(part);
        }
        private string DefinePrefabName(string prefabName)
        {
            if (prefabName == null || prefabName == "")
            {
                prefabName = id + ".prefab";
            }
            return prefabName;
        }

        public AdvPart(Mod mod, string id, string name, NewPart parentPart, GameObject objectToInstantiate, Vector3 installPosition, Vector3 installRotation, Dictionary<string, bool> partsBuySave = null, string boughtId = null)
        {
            DefineBaseInfo(mod, id, partsBuySave, boughtId);

            this.parentPart = parentPart;

            partGameObject = GameObject.Instantiate(objectToInstantiate);

            Helper.SetObjectNameTagLayer(partGameObject, name + "(Clone)");

            bought = CheckBought(partsBuySave, boughtId);
            PartSaveInfo partSaveInfo = GetPartSaveInfo(bought, mod, saveFile);

            part = new NewPart(
                partSaveInfo,
                partGameObject,
                parentPart.rigidPart,
                new Trigger(id + "_trigger", parentPart.rigidPart, installPosition, new Quaternion(0, 0, 0, 0), new Vector3(triggerSize, triggerSize, triggerSize), false),
                installPosition,
                installRotation);
            parentPart.AddChildPart(part);

            DefineBasePartCalls(part);
        }

        public AdvPart(Mod mod, string id, string name, NewPart parentPart, string prefabName, Vector3 installPosition, Vector3 installRotation, AssetBundle assetBundle, Dictionary<string, bool> partsBuySave = null, string boughtId = null)
        {
            DefineBaseInfo(mod, id, partsBuySave, boughtId);
            prefabName = DefinePrefabName(prefabName);

            this.parentPart = parentPart;

            partGameObject = Helper.LoadPartAndSetName(assetBundle, prefabName, name);

            PartSaveInfo partSaveInfo = GetPartSaveInfo(bought, mod, saveFile);

            part = new NewPart(
                partSaveInfo,
                partGameObject,
                parentPart.rigidPart,
                new Trigger(id + "_trigger", parentPart.rigidPart, installPosition, new Quaternion(0, 0, 0, 0), new Vector3(triggerSize, triggerSize, triggerSize), false),
                installPosition,
                installRotation);
            parentPart.AddChildPart(part);

            DefineBasePartCalls(part);
        }
        public AdvPart(Mod mod, string id, string name, GameObject parentPart, string prefabName, Vector3 installPosition, Vector3 installRotation, AssetBundle assetBundle, Dictionary<string, bool> partsBuySave = null, string boughtId = null)
        {
            DefineBaseInfo(mod, id, partsBuySave, boughtId);
            prefabName = DefinePrefabName(prefabName);

            partGameObject = Helper.LoadPartAndSetName(assetBundle, prefabName, name);

            PartSaveInfo partSaveInfo = GetPartSaveInfo(bought, mod, saveFile);

            try
            {
                part = new NewPart(
                    partSaveInfo,
                    partGameObject,
                    parentPart,
                    new Trigger(id + "_trigger", parentPart, installPosition, new Quaternion(0, 0, 0, 0), new Vector3(triggerSize, triggerSize, triggerSize), false),
                    installPosition,
                    installRotation);
            } catch(Exception ex)
            {

            }


            DefineBasePartCalls(part);
        }

        private void FixRigidPartNaming(NewPart part)
        {
            part.rigidPart.name = part.rigidPart.name.Replace("(Clone)(Clone)", "(Clone)");
        }

        public Dictionary<string, bool> GetBought(Dictionary<string, bool> partsBuySave)
        {
            if (boughtId == null || boughtId == "")
            {
                return partsBuySave;
            }

            partsBuySave[boughtId] = bought;
            return partsBuySave;
        }

        private PartSaveInfo GetPartSaveInfo(bool bought, Mod mod, string saveFile)
        {
            if (bought)
            {
                try
                {
                    return SaveLoad.DeserializeSaveFile<PartSaveInfo>(mod, saveFile);
                }
                catch
                {
                    return null;
                }

            }
            return null;
        }

        private bool CheckBought(Dictionary<string, bool> partsBuySave, string boughtId)
        {
            if (partsBuySave == null)
            {
                return true;
            }
            else
            {
                try
                {
                    return partsBuySave[boughtId];
                }
                catch
                {
                    return false;
                }
            }
        }


        public void OnAssemble()
        {
            if (this.screwablePart != null)
            {
                this.screwablePart.setScrewsOnAssemble();
            }
        }
        public void OnDisassemble()
        {
            if (screwablePart != null)
            {
                screwablePart.resetScrewsOnDisassemble();
            }
        }

        public bool InstalledScrewed(bool ignoreScrewed = false)
        {
            if (!ignoreScrewed)
            {
                if (screwablePart != null)
                {
                    return (installed && screwablePart.partFixed);
                }
            }
            return installed;
        }









        public void removePart()
        {
            part.removePart();
        }
        public bool installed
        {
            get { return part.installed; }
        }
        public Trigger partTrigger
        {
            get { return part.partTrigger; }
        }
        public GameObject parent
        {
            get { return part.parent; }
        }
        public GameObject activePart
        {
            get { return part.activePart; }
        }
        public GameObject rigidPart
        {
            get { return part.rigidPart; }
        }

        public PartSaveInfo getSaveInfo()
        {
            return part.defaultPartSaveInfo;
        }
        public PartSaveInfo defaultPartSaveInfo
        {
            get
            {
                return part.defaultPartSaveInfo;
            }
        }
    }
}
