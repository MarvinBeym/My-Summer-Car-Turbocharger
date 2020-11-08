using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.old_file_checker
{
    public class SaveFileRenamer: OldFileRenamer
    {
        private SatsumaTurboCharger mod;
        public SaveFileRenamer(SatsumaTurboCharger mod) : base(mod)
        {
            this.mod = mod;

            
            oldToNew.Add("ecu_mod_ABSModule_partSave.txt", "abs_module_saveFile.txt");
            oldToNew.Add("ecu_mod_ESPModule_partSave.txt", "esp_module_saveFile.txt");
            oldToNew.Add("ecu_mod_TCSModule_partSave.txt", "tcs_module_saveFile.txt");
            oldToNew.Add("ecu_mod_CableHarness_partSave.txt", "cable_harness_saveFile.txt");
            oldToNew.Add("ecu_mod_MountingPlate_partSave.txt", "mounting_plate_saveFile.txt");
            oldToNew.Add("ecu_mod_ModShop_SaveFile.txt", "mod_shop_saveFile.txt");
            oldToNew.Add("ecu_mod_SmartEngineModule_partSave.txt", "smart_engine_module_saveFile.txt");
            oldToNew.Add("ecu_mod_CruiseControlPanel_partSave.txt", "cruise_control_panel_saveFile.txt");
            oldToNew.Add("ecu_InfoPanel_partSave.txt", "info_panel_saveFile.txt");
            oldToNew.Add("ecu_reverseCamera_saveFile.txt", "reverse_camera_saveFile.txt");
            oldToNew.Add("ecu_rainLightSensorboard_saveFile.txt", "rain_light_sensor_board_saveFile.txt");
            oldToNew.Add("ecu_mod_screwable_save.txt", "screwable_saveFile.txt");
            RenameOldFiles(ModLoader.GetModConfigFolder(mod), oldToNew);
        }
    }
}
