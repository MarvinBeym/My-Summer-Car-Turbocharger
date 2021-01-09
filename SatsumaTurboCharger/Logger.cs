using MSCLoader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tools
{
    public static class Logger
    {
        private static string logging_filePath = "";
        private static Mod mod;
        private static int maxErrorsToLog = 0;
        private static int errorsLogged = 0;
        public static void InitLogger(Mod mod, string file_name, int maxErrorsToLog)
        {
            Logger.mod = mod;
            Logger.maxErrorsToLog = maxErrorsToLog;
            logging_filePath = Path.Combine(ModLoader.GetModConfigFolder(mod), file_name);
            InitFile();
        }

        private static void InitFile()
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
                // Ignore MSCLoader.
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
║ Name:         { mod.Name }
║ Version:      { mod.Version }
║ Author:       { mod.Author }
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

        private static string GenerateHeader(string description, int maxLineLength, char headerLine = '═')
        {
            string header = "════";
            header += description;
            header += new string(headerLine, (maxLineLength - header.Length));
            return header;
        }

        private static string AddBaseLogLine(string message)
        {
            DateTime dateTime = DateTime.Now;
            string formatedDateTime = dateTime.ToString("G", CultureInfo.CreateSpecificCulture("de-DE"));
            return $"[{formatedDateTime}] {message}{Environment.NewLine}";
        }

        private static string AddAdditionalInfoLine(string errorLogLine, string info)
        {
            if (info != null || info != "")
            {
                errorLogLine += $"=> Additional infos: {info}{Environment.NewLine}";
            }
            return errorLogLine;
        }
        private static string AddExceptionLine(string errorLogLine, Exception ex)
        {
            if (ex != null)
            {
                errorLogLine += $"=> Exception message: {ex.Message}{Environment.NewLine}";
            }
            return errorLogLine;
        }

        public static void New(string message)
        {
            if (errorsLogged <= maxErrorsToLog)
            {
                errorsLogged++;

                using (StreamWriter sw = new StreamWriter(logging_filePath, true))
                {
                    string errorLogLine = AddBaseLogLine(message);
                    errorLogLine += Environment.NewLine;
                    sw.Write(errorLogLine);
                }
            }
        }

        public static void New(string message, string additionalInfo)
        {
            if (errorsLogged <= maxErrorsToLog)
            {
                errorsLogged++;

                using (StreamWriter sw = new StreamWriter(logging_filePath, true))
                {
                    string errorLogLine = AddBaseLogLine(message);
                    errorLogLine = AddAdditionalInfoLine(errorLogLine, additionalInfo);
                    errorLogLine += Environment.NewLine;
                    sw.Write(errorLogLine);
                }
            }
        }
        public static void New(string message, Exception ex)
        {
            if (errorsLogged <= maxErrorsToLog)
            {
                errorsLogged++;

                using (StreamWriter sw = new StreamWriter(logging_filePath, true))
                {
                    string errorLogLine = AddBaseLogLine(message);
                    errorLogLine = AddExceptionLine(errorLogLine, ex);
                    errorLogLine += Environment.NewLine;
                    sw.Write(errorLogLine);
                }
            }
        }

        public static void New(string message, string additionalInfo, Exception ex)
        {
            if (errorsLogged <= maxErrorsToLog)
            {
                errorsLogged++;

                using (StreamWriter sw = new StreamWriter(logging_filePath, true))
                {
                    string errorLogLine = AddBaseLogLine(message);
                    errorLogLine = AddAdditionalInfoLine(errorLogLine, additionalInfo);
                    errorLogLine = AddExceptionLine(errorLogLine, ex);
                    errorLogLine += Environment.NewLine;
                    sw.Write(errorLogLine);
                }
            }
        }


        private static string BoolToCracked(bool steamValid)
        {
            if (steamValid)
            {
                return "Steam detected";
            }
            return "NO Steam detected";
        }
    }
}
