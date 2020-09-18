using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;
using MSCLoader;
using System.Globalization;

namespace SatsumaTurboCharger
{
    class Logger
    {
        private string modVersion { get; set; }
        private string filePath;
        private bool needToWriteHeader = true;
        private Mod mod;

        public Logger(string filePath, Mod mod){ 
            this.mod = mod;
            this.filePath = filePath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        public void LogError(string errorMessage, Exception e, int errorLevel = 1){ 
            string errorLine = "[" + DateTime.UtcNow.ToString("G", CultureInfo.CreateSpecificCulture("de-DE")) + "][" + ErrorLevelToString(errorLevel) + "] " + errorMessage + Environment.NewLine;
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }
            }
            catch(Exception ex)
            {
                ModConsole.Error("Error while logging error:" + ex.Message);
            }

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                if (needToWriteHeader)
                {
                    WriteErrorBeginText(writer);
                    needToWriteHeader = false;
                }
                writer.Write(errorLine + ": " + e.StackTrace);
                writer.Flush();
            }

        }

        private string ErrorLevelToString(int errorLevel){ 
            if(errorLevel == 1)
            {
                return "ERROR";
            }
            else if(errorLevel == 0)
            {
                return "WARNING";
            }
            else
            {
                return "NOTICE";
            }
        }

        private void WriteErrorBeginText(StreamWriter writer){ 
            string header =
                String.Format("MOD-ID: {0}", mod.ID) + Environment.NewLine
                + String.Format("MOD-Name: {0}", mod.Name) + Environment.NewLine
                + String.Format("MOD-Version: {0}", mod.Version) + Environment.NewLine
                + String.Format("Cracked: {0}", !ModLoader.CheckSteam()) + Environment.NewLine;
            writer.Write(header);
            writer.Flush();
        }
    }
}
