using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tools.gui
{
    public class GuiDebugElement
    {
        private string elementName;
        private bool elementEnabled = false;
        public GuiDebugElement(string elementName)
        {
            this.elementName = elementName;
        }

        public void Handle(GuiDebugInfo[] debugInfos)
        {
            if (GUILayout.Button(elementName))
            {
                elementEnabled = !elementEnabled;
            }

            if (elementEnabled)
            {
                GUILayout.BeginVertical("", "box", GUILayout.ExpandHeight(false));
                foreach (GuiDebugInfo debugInfo in debugInfos)
                {
                    if (debugInfo.elementName == elementName)
                    {
                        debugInfo.Handle();
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
