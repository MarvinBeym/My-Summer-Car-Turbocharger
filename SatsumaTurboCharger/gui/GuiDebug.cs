using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SatsumaTurboCharger.gui
{
    public class GuiDebug
    {
        private Rect rect;
        private GuiDebugElement[] debugWindows;
        private string debugLabel;
        private bool windowEnabled = false;
        public GuiDebug(int left, int top, int width, string debugLabel, GuiDebugElement[] debugWindows)
        {
            rect = new Rect(left, top, width, Screen.height - top);
            this.debugWindows = debugWindows;
            this.debugLabel = debugLabel;
        }

        internal void Handle(GuiDebugInfo[] guiDebugInfos)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginVertical(debugLabel, "window", GUILayout.ExpandHeight(false));
            foreach (GuiDebugElement debugWindow in debugWindows)
            {
                debugWindow.Handle(guiDebugInfos);
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
