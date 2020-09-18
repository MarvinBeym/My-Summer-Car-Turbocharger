using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModApi.Attachable;

namespace SatsumaTurboCharger
{

    public class Racing_Hood_Part : Part
    {
        public Racing_Hood_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation){ 

        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo(){ 
            installed = false,
            
            position = SatsumaTurboCharger.turbocharger_hood_spawnLocation,
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
            // do stuff on assemble.
            base.assemble(startUp); // if you want assemble function, you need to call base!
            SatsumaTurboCharger.colorHasToChange = true;
        }

        protected override void disassemble(bool startup = false){ 
            // do stuff on dissemble.
            base.disassemble(startup); // if you want dissemble function, you need to call base!
            SatsumaTurboCharger.colorHasToChange = true;
        }

        public void removePart(){ 
            disassemble(false);
        }
    }

}
