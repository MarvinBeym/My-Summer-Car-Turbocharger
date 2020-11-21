using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace SatsumaTurboCharger.gui
{
    public class SaveableColor
    {
        public float r { get; set; } = 0f;
        public float g { get; set; } = 0f;
        public float b { get; set; } = 0f;
        public float a { get; set; } = 0f;

        public SaveableColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color ConvertToColor(SaveableColor saveableColor)
        {
            return new Color(saveableColor.r, saveableColor.g, saveableColor.b, saveableColor.a);
        }

        public static SaveableColor ConvertToSaveable(Color color)
        {
            return new SaveableColor(color.r, color.g, color.b, color.a);
        }
    }
}
