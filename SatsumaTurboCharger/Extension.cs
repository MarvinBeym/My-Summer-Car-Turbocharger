using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace Tools
{
    public static class Extension
    {
        public static string ToStringOrEmpty(this Object value)
        {
            return value == null ? "" : value.ToString();
        }

        public static float Map(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static string ToOnOff(this bool value, string onText = "ON", string offText = "OFF")
        {
            return value ? onText : offText;
        }

        public static GameObject FindChild(this GameObject gameObject, string childName)
        {
            return gameObject.transform.FindChild(childName).gameObject;
        }
    }
}
