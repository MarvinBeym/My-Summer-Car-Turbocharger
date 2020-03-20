using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using ModApi;
using ModApi.Attachable;
using MSCLoader;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
namespace SatsumaTurboCharger
{
    public class Graph : MonoBehaviour
    {

        public float graphWidth;
        public float graphHeight;
        LineRenderer newLineRenderer;
        List<int> decibels;
        int vertexAmount = 50;
        float xInterval;

        GameObject parentCanvas;

        // Use this for initialization
        public void Start()
        {

            parentCanvas = ModUI.GetCanvas();
            parentCanvas.AddComponent<LineRenderer>();
            graphWidth = 600;
            graphHeight = 300;
            newLineRenderer = parentCanvas.GetComponentInChildren<LineRenderer>();
            newLineRenderer.SetVertexCount(vertexAmount);

            xInterval = graphWidth / vertexAmount;
        }

        //Display 1 minute of data or as much as there is.
        public void Draw(List<int> decibels)
        {
            if (decibels.Count == 0)
                return;

            float x = 0;

            for (int i = 0; i < vertexAmount && i < decibels.Count; i++)
            {
                int _index = decibels.Count - i - 1;

                float y = decibels[_index] * (graphHeight / 130); //(Divide grapheight with the maximum value of decibels.
                x = i * xInterval;

                newLineRenderer.SetPosition(i, new Vector3(x - graphWidth / 2, y - graphHeight / 2, 0));
            }
        }
    }
}
