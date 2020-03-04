using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModApi.Attachable;

namespace SatsumaTurboCharger
{

    public class TurboChargerBigExhaustInletTubePart : Part
    {
        public TurboChargerBigExhaustInletTubePart(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation)
        {

        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo()
        {
            installed = false, //Will make part installed

            position = new Vector3(-17f, 0.25f, -3.5f), //Sets the spawn location -> where i can be found
            rotation = Quaternion.Euler(-0.03519259f, -0.322451085f, 0.825269341f), // Rotation at spawn location
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
            // do stuff on assemble.
            base.assemble(startUp); // if you want assemble function, you need to call base!
        }

        protected override void disassemble(bool startup = false)
        {
            // do stuff on dissemble.
            base.disassemble(startup); // if you want dissemble function, you need to call base!
        }
    }

}
