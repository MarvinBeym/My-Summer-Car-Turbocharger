using ModApi.Attachable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Parts
{
    public class NewPart : Part
    {
        private Action advPart_onAssemble;
        private Action advPart_onDisassemble;

        private List<NewPart> childParts;
        public NewPart(PartSaveInfo partSaveInfo, GameObject part, GameObject parent, Trigger trigger, Vector3 position, Vector3 rotation) : base(partSaveInfo, part, parent, trigger, position, new Quaternion {eulerAngles = rotation })
        {
            childParts = new List<NewPart>();
        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo()
        {
            installed = false, //Will make part installed

            position = ModsShop.FleetariSpawnLocation.desk,
            rotation = Quaternion.Euler(0, 0, 0),
        };

        public override GameObject rigidPart { get; set; }
        public override GameObject activePart { get; set; }

        public void SetOnAssemble(Action onAssemble)
        {
            advPart_onAssemble = onAssemble;
        }
        public void SetOnDisassemble(Action onDisassemble)
        {
            advPart_onDisassemble = onDisassemble;
        }


        protected override void assemble(bool startUp = false)
        {
            base.assemble(startUp);
            if(advPart_onAssemble != null)
            {
                advPart_onAssemble.Invoke();
            }
            
        }
        protected override void disassemble(bool startup = false)
        {
            base.disassemble(startup);
            if(advPart_onDisassemble != null)
            {
                advPart_onDisassemble.Invoke();
            }

            if(childParts != null)
            {
                foreach (NewPart childPart in childParts)
                {
                    if (childPart.installed)
                    {
                        childPart.removePart();
                    }
                }
            }

        }
        public void removePart()
        {
            if (installed)
            {
                disassemble(false);
            }
        }

        public void AddChildPart(NewPart part)
        {
            childParts.Add(part);
        }
    }
}
