using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public abstract class OldFileRenamer
    {
        public Dictionary<string, string> renamedFiles { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> oldToNew { get; set; } = new Dictionary<string, string>();
        public bool showGui { get; set; } = false;
        private SatsumaTurboCharger mod;
        private int guiWidth;
        public OldFileRenamer(SatsumaTurboCharger mod, int guiWidth)
        {
            this.mod = mod;
            this.guiWidth = guiWidth;
        }

        public void GuiHandler()
        {
            if (showGui)
            {
                const int baseGuiHeight = 200;
                int guiHeight = baseGuiHeight + (renamedFiles.Count * 20);

                GUI.Box(new Rect((Screen.width / 2) - (guiWidth / 2), (Screen.height / 2) - (guiHeight / 2), guiWidth, guiHeight), "[" + mod.ID + " " + mod.Version + "] - Old File Renamer");
                GUI.Label(new Rect((Screen.width / 2) - (guiWidth / 2) + 20, (Screen.height / 2) - (guiHeight / 2) + 20, guiWidth - 20, 20), "The following files have been renamed  ---  Original files have also been copied to the AUTO_BACKUP folder");
                int counter = 0;
                foreach (KeyValuePair<string, string> renamedFile in renamedFiles)
                {
                    string labelText = renamedFile.Key + " => " + renamedFile.Value;
                    GUI.Label(new Rect((Screen.width / 2) - (guiWidth / 2) + 20, (Screen.height / 2) - (guiHeight / 2) + 60 + (20 * counter), guiWidth - 20, 20), labelText);
                    counter++;
                }

                bool buttonState = GUI.Button(new Rect((Screen.width / 2) - (guiWidth / 2), (Screen.height / 2) + (guiHeight / 2) - 40, guiWidth, 40), "Close");
                if (buttonState)
                {
                    SetGui(false);
                }
            }
        }

        private void SetGui(bool guiState)
        {
            showGui = guiState;
        }

        public void RenameOldFiles(string directoryToCheck, Dictionary<string, string> oldToNew)
        {
            foreach(KeyValuePair<string, string> oldNewName in oldToNew)
            {
                string filePathOld = Path.Combine(directoryToCheck, oldNewName.Key);
                string filePathBackupFolder = Helper.CombinePaths(new string[] { directoryToCheck, "AUTO_BACKUP" });
                if (!File.Exists(filePathBackupFolder))
                {
                    Directory.CreateDirectory(filePathBackupFolder);
                }
                string filePathBackup = Helper.CombinePaths(new string[] { filePathBackupFolder, oldNewName.Key });

                string filePathNew = Path.Combine(directoryToCheck, oldNewName.Value);
                if (File.Exists(filePathOld) && !File.Exists(filePathNew))
                {
                    try
                    {
                        File.Copy(filePathOld, filePathBackup);
                        File.Move(filePathOld, filePathNew);
                        renamedFiles.Add(oldNewName.Key, oldNewName.Value);
                    }
                    catch(Exception ex)
                    {
                        ModConsole.Print("File " + oldNewName.Key + "could not be renamed to " + oldNewName.Value + " | Reason: " + ex.Message);
                    }
                }
            }
            if(renamedFiles.Count > 0)
            {
                SetGui(true);
            }
        }

        public Dictionary<string, string> GetRenamedFiles()
        {
            return this.renamedFiles;
        }
    }
}