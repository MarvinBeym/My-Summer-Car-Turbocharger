using ModApi.Attachable;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class Exhaust_Header_Part : Part
    {
        public Exhaust_Header_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation){ 

        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo(){ 
            installed = false,

            position = SatsumaTurboCharger.turbocharger_exhaust_header_spawnLocation,
            rotation = Quaternion.Euler(0f, 0f, 0f),
        };

        public override GameObject rigidPart{ 
            get;
            set;
        }
        public override GameObject activePart{ 
            get;
            set;
        }

        protected override void assemble(bool startUp = false){ 
            base.assemble(startUp);
            if (SatsumaTurboCharger.turbocharger_exhaust_header_screwable != null)
            {
                SatsumaTurboCharger.turbocharger_exhaust_header_screwable.setScrewsOnAssemble();
            }
        }

        protected override void disassemble(bool startup = false){ 
            base.disassemble(startup);
            if (SatsumaTurboCharger.turbocharger_exhaust_header_screwable != null)
            {
                SatsumaTurboCharger.turbocharger_exhaust_header_screwable.resetScrewsOnDisassemble();
            }
        }

        public void removePart(){ 
            disassemble(false);
        }
    }
}
