using MSCLoader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class Logger
    {
        private string logging_filePath = "";
        private string file_name = "";
        private SatsumaTurboCharger mod;
        private int maxErrorsToLog = 0;
        private int errorsLogged = 0;
        public Logger(SatsumaTurboCharger mod, string file_name, int maxErrorsToLog)
        {
            this.mod = mod;
            this.file_name = file_name;
            this.maxErrorsToLog = maxErrorsToLog;
            logging_filePath = Path.Combine(ModLoader.GetModConfigFolder(mod), file_name);
            if (!File.Exists(logging_filePath)){
                WriteBaseInformationToFile();
            }

        }

        private void WriteBaseInformationToFile()
        {
            string os = SystemInfo.operatingSystem;
            int build = int.Parse(os.Split('(')[1].Split(')')[0].Split('.')[2]);
            if (build > 9600)
            {
                os = $"Windows 10 (10.0.{build})";

                if (SystemInfo.operatingSystem.Contains("64bit"))
                {
                    os += " 64bit";
                }

            }

            string modsInstalled = "";
            int maxLineLength = 62;
            foreach (Mod mod in ModLoader.LoadedMods)
            {
                // Ignore MSCLoader or MOP.
                if (mod.ID == "MSCLoader_Console" || mod.ID == "MSCLoader_Settings")
                    continue;

                string modLine = String.Format("║ ID: {0} Name: {1} Version: {2}" + Environment.NewLine, mod.ID, mod.Name, mod.Version);
                modsInstalled += modLine;
                maxLineLength = modLine.Length;
            }
            modsInstalled += "║";


            string baseInformation =
$@"╔{GenerateHeader(" Environment ", maxLineLength)}
║ Steam:        { BoolToCracked(ModLoader.CheckSteam()) }
║ OS:           { os }
╠{GenerateHeader(" Mod ", maxLineLength)}
║ Name:         { this.mod.Name }
║ Version:      { this.mod.Version }
║ Author:       { this.mod.Author }
╠{GenerateHeader(" ModLoader ", maxLineLength)}
║ Version:      { ModLoader.MSCLoader_Ver }
║ Dev:          { ModLoader.devMode}
║ Experimental: { ModLoader.experimental }
╠{GenerateHeader(" Mods ", maxLineLength)}
║
{ modsInstalled }
╚{GenerateHeader("", maxLineLength)}
";
            using (StreamWriter streamWriter = new StreamWriter(logging_filePath, false))
            {
                streamWriter.Write(baseInformation);
            }
        }

        private string GenerateHeader(string description, int maxLineLength, char headerLine = '═')
        {
            string header = "════";
            header += description;
            header += new string(headerLine, (maxLineLength - header.Length));
            return header;
        }

        public void New(string message, string additionalInfo = "", Exception ex = null)
        {
            if(errorsLogged <= maxErrorsToLog)
            {
                errorsLogged++;

                using (StreamWriter sw = new StreamWriter(logging_filePath, true))
                {
                    DateTime dateTime = DateTime.Now;
                    string formatedDateTime = dateTime.ToString("G", CultureInfo.CreateSpecificCulture("de-DE"));
                    string errorLogLine = $"[{formatedDateTime}] {message}{Environment.NewLine}";
                  
                    if(additionalInfo != null || additionalInfo != "")
                    {
                        string additionalInfoLogLine = $"=> Additional infos: {additionalInfo}{Environment.NewLine}";
                        errorLogLine += additionalInfoLogLine;
                    }
                    if (ex != null)
                    {
                        string exceptionLogLine = $"=> Exception message: {ex.Message}{Environment.NewLine}";
                        errorLogLine += exceptionLogLine;
                    }

                    errorLogLine += Environment.NewLine;
                    sw.Write(errorLogLine);
                }
            }
            
        }

        private string BoolToCracked(bool steamValid)
        {
            if (steamValid)
            {
                return "Steam detected";
            }
            return "NO Steam detected";
        }

        public void WriteToFile(string message)
        {

        }
    }
}
