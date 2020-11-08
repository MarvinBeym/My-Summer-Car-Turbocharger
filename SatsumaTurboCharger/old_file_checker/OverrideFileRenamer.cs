using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class OverrideFileRenamer : OldFileRenamer
    {
        SatsumaTurboCharger mod;
        public OverrideFileRenamer(SatsumaTurboCharger mod) : base(mod)
        {
            this.mod = mod;

            oldToNew.Add("OVERRIDE_ECU-Mod-Panel-Page0.png", "OVERRIDE_main_page.png");
            oldToNew.Add("OVERRIDE_ECU-Mod-Panel_Modules-Page1.png", "OVERRIDE_modules_page.png");
            oldToNew.Add("OVERRIDE_ECU-Mod-Panel_Faults-Page2.png", "OVERRIDE_faults_page.png");
            oldToNew.Add("OVERRIDE_ECU-Mod-Panel_Faults-Page3.png", "OVERRIDE_faults2_page.png");
            oldToNew.Add("OVERRIDE_ECU-Mod-Panel_Tuner-Page4.png", "OVERRIDE_tuner_page.png");
            oldToNew.Add("OVERRIDE_ECU-Mod-Panel-Turbocharger-Page5.png", "OVERRIDE_turbocharger_page.png");
            oldToNew.Add("OVERRIDE_ECU-Mod-Panel-Assistance-Page6.png", "OVERRIDE_assistance_page.png");
            oldToNew.Add("OVERRIDE_ECU-Mod-Panel-Airride-Page7.png", "OVERRIDE_airride_page.png");
            oldToNew.Add("OVERRIDE_Handbrake-Icon.png", "OVERRIDE_handbrake_icon.png");
            oldToNew.Add("OVERRIDE_Indicator-Left-Icon.png", "OVERRIDE_blinker_left_icon.png");
            oldToNew.Add("OVERRIDE_Indicator-Right-Icon.png", "OVERRIDE_blinker_right_icon.png");
            oldToNew.Add("OVERRIDE_LowBeam-Icon.png", "OVERRIDE_low_beam_icon.png");
            oldToNew.Add("OVERRIDE_HighBeam-Icon.png", "OVERRIDE_high_beam_icon.png");
            oldToNew.Add("OVERRIDE_Rpm-Needle.png", "OVERRIDE_needle_icon.png");
            oldToNew.Add("OVERRIDE_TurbineWheel.png", "OVERRIDE_turbine_icon.png");
            RenameOldFiles(ModLoader.GetModAssetsFolder(mod), oldToNew);
        }

    }
}
