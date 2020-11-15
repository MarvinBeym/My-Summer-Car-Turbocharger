using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SatsumaTurboCharger.wear
{
    public class WearCondition
    {
        public string guiInteractionText;
        public float check = 0;
        public float divider = 0;
        public Check conditionCheck = Check.LessThan;
        public enum Check
        {
            LessThan,
            MoreThan,
            Equal,
        }

        public WearCondition(float check, Check conditionCheck, float divider, string guiInteractionText)
        {
            this.check = check;
            this.conditionCheck = conditionCheck;
            this.divider = divider;
            this.guiInteractionText = guiInteractionText;
        }
    }
}
