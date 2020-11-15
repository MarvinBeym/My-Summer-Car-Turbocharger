using MSCLoader;
using UnityEngine;

namespace SatsumaTurboCharger.turbo
{
    public class Condition
    {
        public string id = "conditionDefaultID";
        public bool applyCondition = false;
        public float valueToApply = 0;
        public Condition(string id, float valueToApply)
        {
            this.id = id;
            this.valueToApply = valueToApply;
        }
    }
}