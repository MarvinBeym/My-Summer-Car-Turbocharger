﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModApi.Attachable;

namespace SatsumaTurboCharger
{
    public class Manifold_Weber_Part : Part
    {
        public Manifold_Weber_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation)
        {

        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo()
        {
            installed = false, //Will make part installed

            position = SatsumaTurboCharger.turbocharger_manifold_weber_spawnLocation, //Sets the spawn location -> where i can be found
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
            // do stuff on assemble.
            base.assemble(startUp); // if you want assemble function, you need to call base!
            if (SatsumaTurboCharger.turbocharger_manifold_weberCarb_screwable != null)
            {
                SatsumaTurboCharger.turbocharger_manifold_weberCarb_screwable.setScrewsOnAssemble();
            }
            SatsumaTurboCharger.colorHasToChange = true;
        }

        protected override void disassemble(bool startup = false)
        {
            // do stuff on dissemble.
            base.disassemble(startup); // if you want dissemble function, you need to call base!
            if (SatsumaTurboCharger.turbocharger_manifold_weberCarb_screwable != null)
            {
                SatsumaTurboCharger.turbocharger_manifold_weberCarb_screwable.resetScrewsOnDisassemble();
            }
            SatsumaTurboCharger.colorHasToChange = true;
        }
        public void removePart()
        {
            disassemble(false);
        }
    }
}
