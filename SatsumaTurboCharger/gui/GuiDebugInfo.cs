using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tools.gui
{
    public class GuiDebugInfo
    {
        internal string elementName;
        private string description;
        private string value;
        private bool isLabel = false;
        private GUIStyle valueStyle;
        private GUIStyle descriptionStyle;
        public GuiDebugInfo(string elementName, string description, string value)
        {
            this.elementName = elementName;
            this.description = description;
            this.value = value;
            RectOffset padding = new RectOffset(10, 10, 0, 0);
            valueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                padding = padding,
            };
            descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = padding,
            };
        }

        public GuiDebugInfo(string elementName, string description) : this(elementName, description, "")
        {
            isLabel = true;
        }

        internal void Handle()
        {
            if (isLabel)
            {
                GUILayout.Label(description, descriptionStyle);
                return;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label(description, descriptionStyle);
            GUILayout.BeginVertical("box", GUILayout.Width(100));
            GUILayout.Label(value, valueStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
