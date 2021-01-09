using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tools
{
    public static class Game
    {
        public static Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

        public static GameObject Find(string findText)
        {
            try
            {
                GameObject gameObject = gameObjects[findText];
                return gameObject;
            }
            catch
            {
                
            }

            GameObject foundObject = GameObject.Find(findText);
            gameObjects[findText] = foundObject;
            return foundObject;
        }
    }
}
