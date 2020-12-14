using HutongGames.PlayMaker;
using ModApi.Attachable;
using MSCLoader;
using ScrewablePartAPI;
using ScrewablePartAPI.V2;
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
        public ScrewablePartV2 screwablePart = null;

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

            part = new NewPart(
                partSaveInfo,
                partGameObject,
                parentPart,
                new Trigger(id + "_trigger", parentPart, installPosition, new Quaternion(0, 0, 0, 0), new Vector3(triggerSize, triggerSize, triggerSize), false),
                installPosition,
                installRotation);

            AttachParentUninstallFsm(parentPart);

            DefineBasePartCalls(part);
        }

        /// <summary>
        /// Injects the install & uninstall methods for original parts (from the game itself)
        /// </summary>
        /// <param name="parentPart"></param>
        private void AttachParentUninstallFsm(GameObject parentPart)
        {
            try
            {
                PlayMakerFSM[] fSMs = parentPart.GetComponents<PlayMakerFSM>();
                foreach (PlayMakerFSM fSM in fSMs)
                {
                    if (fSM.FsmName == "Removal")
                    {
                        bool requiredStatesFound = false;
                        foreach (FsmState state in fSM.FsmStates)
                        {
                            if (state.Name == "Remove part")
                            {
                                requiredStatesFound = true;
                            }
                        }
                        if (requiredStatesFound)
                        {
                            partTrigger.triggerGameObject.SetActive((parentPart.transform.parent != null && parentPart.transform.parent.name != ""));

                            GameObject parentFsmGameObject = fSM.FsmVariables.FindFsmGameObject("db_ThisPart").Value;

                            FsmHook.FsmInject(parentPart, "Remove part", OnParentUninstall);
                            GameObject trigger = PlayMakerFSM.FindFsmOnGameObject(parentFsmGameObject, "Data").FsmVariables.FindFsmGameObject("Trigger").Value;

                            if (Helper.CheckContainsState(Helper.FindFsmOnGameObject(trigger, "Assembly"), "Assemble"))
                            {
                                FsmHook.FsmInject(trigger, "Assemble", OnParentInstall);
                            }
                            else if(Helper.CheckContainsState(Helper.FindFsmOnGameObject(trigger, "Assembly"), "Assemble 2"))
                            {
                                FsmHook.FsmInject(trigger, "Assemble 2", OnParentInstall);
                            }
                        }
                    }
                }

            }
            catch(Exception ex)
            {

            }
        }

        public void AddChildPart(AdvPart part)
        {
            this.part.AddChildPart(part.part);
        }

        private void OnParentInstall()
        {
            partTrigger.triggerGameObject.SetActive(true);
        }

        private void OnParentUninstall()
        {
            if (installed) { removePart(); }
            partTrigger.triggerGameObject.SetActive(false);
            
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
            if (screwablePart != null)
            {
                screwablePart.OnPartAssemble();
            }
        }
        public void OnDisassemble()
        {
            if (screwablePart != null)
            {
                screwablePart.OnPartDisassemble();
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
            return part.getSaveInfo();
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
