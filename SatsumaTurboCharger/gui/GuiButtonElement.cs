using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.gui
{
    public class GuiButtonElement
    {
        public bool buttonEnabled = false;
        public string text = "";
        private GuiDebug guiDebug;
        public GuiButtonElement(string text)
        {
            this.text = text;
        }

        public void SetGuiInfo(GuiDebug guiDebug)
        {
            this.guiDebug = guiDebug;
        }

        public void Handle(float topPosition, int nextTopPosition, List<GuiInfo> guiInfos)
        {
            if (GUI.Button(new Rect(guiDebug.baseLeft + GuiDebug.space, topPosition, guiDebug.baseWidth - GuiDebug.space * 2, GuiDebug.buttonHeight), text))
            {
                foreach(GuiButtonElement guiButtonElement in guiDebug.guiButtonElements)
                {
                    if(guiButtonElement != this)
                    {
                        guiButtonElement.buttonEnabled = false;
                    }
                    
                }
                buttonEnabled = !buttonEnabled;
                if (!buttonEnabled)
                {
                    guiDebug.currentElement = "";
                }
                else
                {
                    guiDebug.currentElement = text;
                }
                
            }
            if (buttonEnabled)
            {
                int elements = 0;
                foreach (GuiInfo guiInfo in guiInfos)
                {
                    if (guiDebug.currentElement == guiInfo.parentElement)
                    {
                        elements++;
                    }

                }

                int boxHeight = 30 + (elements * 30) + 10;

                GUI.Box(new Rect(guiDebug.baseLeft, nextTopPosition, guiDebug.baseWidth, boxHeight), text);
                int counter = 0;
                foreach(GuiInfo guiInfo in guiInfos)
                {
                    if(guiDebug.currentElement == guiInfo.parentElement)
                    {
                        counter++;
                        int nextPosition = (nextTopPosition + 10) + counter * (GuiDebug.buttonHeight + GuiDebug.space);
                        GUI.Label(new Rect(guiDebug.baseLeft + GuiDebug.space, nextPosition, guiDebug.baseWidth - GuiDebug.space * 2, GuiDebug.buttonHeight), guiInfo.description + guiInfo.value);
                    }

                }
            }
        }
    }
}
