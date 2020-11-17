using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.gui
{
    public class GuiDebug
    {
        public string text = "";
        public bool baseEnabled = false;
        public int baseLeft = 0;
        public int baseTop = 0;
        public int baseWidth = 0;
        public string currentElement = "";
        public const int buttonHeight = 20;
        public const int space = 10;
        public List<GuiButtonElement> guiButtonElements;

        public GuiDebug(int left, int top, int width, string text, List<GuiButtonElement> guiButtonElements)
        {
            this.text = text;
            this.guiButtonElements = guiButtonElements;
            baseLeft = left;
            baseTop = top;
            baseWidth = width;

            foreach(GuiButtonElement guiButtonElement in guiButtonElements)
            {
                guiButtonElement.SetGuiInfo(this);
            }
        }


        public void Handle(List<GuiInfo> guiInfos)
        {
            if (GUI.Button(new Rect(baseLeft, baseTop, baseWidth, buttonHeight), text))
            {
                baseEnabled = !baseEnabled;
            }

            if (baseEnabled)
            {
                int topPosition = baseTop + buttonHeight + space;
                int boxHeight = (guiButtonElements.Count * (buttonHeight + 5)) + 35;
                GUI.Box(new Rect(baseLeft, topPosition, baseWidth, boxHeight), text);
                for(int i = 0; i < guiButtonElements.Count; i++)
                {
                    GuiButtonElement guiButtonElement = guiButtonElements[i];
                    int nextBoxPosition = boxHeight + topPosition + space;
                    guiButtonElement.Handle(i * (space + 15) + (topPosition + 30), nextBoxPosition, guiInfos);
                }
            }

        }
    }
}
