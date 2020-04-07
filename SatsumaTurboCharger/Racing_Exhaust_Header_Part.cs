using ModApi.Attachable;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class Racing_Exhaust_Header_Part : Part
    {
        public Racing_Exhaust_Header_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation)
        {

        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo()
        {
            installed = false, //Will make part installed

            position = SatsumaTurboCharger.turbocharger_big_exhaust_header_spawnLocation, //Sets the spawn location -> where i can be found
            rotation = Quaternion.Euler(0f, 0f, 0f), // Rotation at spawn location
        };

        public override GameObject rigidPart
        {
            get;
            set;
        }
        public override GameObject activePart
        {
            get;
            set;
        }

        protected override void assemble(bool startUp = false)
        {
            base.assemble(startUp);
            if (SatsumaTurboCharger.turbocharger_big_exhaust_header_screwable != null)
            {
                SatsumaTurboCharger.turbocharger_big_exhaust_header_screwable.setScrewsOnAssemble();
            }
        }

        protected override void disassemble(bool startup = false)
        {
            base.disassemble(startup);
            if (SatsumaTurboCharger.turbocharger_big_exhaust_header_screwable != null)
            {
                SatsumaTurboCharger.turbocharger_big_exhaust_header_screwable.resetScrewsOnDisassemble();
            }
        }
    }
}
