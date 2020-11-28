using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.old_file_checker
{
    public class SaveFileRenamer : OldFileRenamer
    {
        private SatsumaTurboCharger mod;
        public SaveFileRenamer(SatsumaTurboCharger mod, int guiWidth) : base(mod, guiWidth)
        {
            this.mod = mod;
            //Big Turbo
            oldToNew.Add("turbocharger_big_partSave.txt", "turboBig_saveFile.json");
            oldToNew.Add("turbocharger_big_intercooler_tube_partSave.txt", "turboBig_intercooler_tube_saveFile.json");
            oldToNew.Add("turbocharger_big_exhaust_inlet_tube_partSave.txt", "turboBig_exhaustInlet_tube_saveFile.json");
            oldToNew.Add("turbocharger_big_exhaust_outlet_tube_partSave.txt", "turboBig_exhaustOutlet_tube_saveFile.json");
            oldToNew.Add("turbocharger_big_blowoff_valve_partSave.txt", "turboBig_blowoffValve_saveFile.json");
            oldToNew.Add("turbocharger_big_exhaust_outlet_straight_partSave.txt", "turboBig_exhaust_outlet_straight_saveFile.json");
            oldToNew.Add("turbocharger_hood_partSave.txt", "turboBig_hood_saveFile.json");
            oldToNew.Add("turbocharger_turboBig_hood_partSave.txt", "turboBig_hood_saveFile.json");

            //Small Turbo
            oldToNew.Add("turbocharger_small_partSave.txt", "turboSmall_saveFile.json");
            oldToNew.Add("turbocharger_small_intercooler_tube_partSave.txt", "turboSmall_intercooler_tube_saveFile.json");
            oldToNew.Add("turbocharger_small_exhaust_inlet_tube_partSave.txt", "turboSmall_exhaustInlet_tube_saveFile.json");
            oldToNew.Add("turbocharger_small_exhaust_outlet_tube_partSave.txt", "turboSmall_exhaustOutlet_tube_saveFile.json");
            oldToNew.Add("turbocharger_small_manifold_twinCarb_tube_partSave.txt", "turboSmall_manifoldTwinCarb_tube_saveFile.json");
            oldToNew.Add("turbocharger_small_airfilter_partSave.txt", "turboSmall_airfilter_saveFile.json");

            //Other Parts
            oldToNew.Add("turbocharger_exhaust_header_partSave.txt", "exhaustHeader_saveFile.json");

            oldToNew.Add("turbocharger_manifold_weber_partSave.txt", "manifoldWeber_saveFile.json");
            oldToNew.Add("turbocharger_manifold_twinCarb_partSave.txt", "manifoldTwinCarb_saveFile.json");
            oldToNew.Add("turbocharger_boost_gauge_partSave.txt", "boostGauge_saveFile.json");
            oldToNew.Add("turbocharger_intercooler_partSave.txt", "intercooler_saveFile.json");
            oldToNew.Add("turbocharger_intercooler_manifold_tube_weber_partSave.txt", "intercooler_manifoldWeber_tube_saveFile.json");
            oldToNew.Add("turbocharger_intercooler_manifold_tube_twinCarb_partSave.txt", "intercooler_manifoldTwinCarb_tube_saveFile.json");

            //Other Saves
            oldToNew.Add("turbocharger_mod_ModsShop_SaveFile.txt", "mod_shop_saveFile.json");
            //oldToNew.Add("turbocharger_mod_boost_SaveFile.txt", "");
            oldToNew.Add("turbocharger_mod_wear_SaveFile.txt", "wear_saveFile.json");
            oldToNew.Add("turbocharger_mod_screws_SaveFile.txt", "OLD_screwable_saveFile.txt");

            RenameOldFiles(ModLoader.GetModConfigFolder(mod), oldToNew);
        }
    }
}
