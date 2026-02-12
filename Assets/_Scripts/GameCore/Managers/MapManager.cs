using System.Collections;
using System.Collections.Generic;
using GameCore.UI;
using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore
{
    public class MapManager : Singleton<MapManager>
    {
        public MapNode[,] currentMapNodes { get; private set; }

        public override void OnInitialize()
        {
        }

        public override void OnDiscard()
        {
            currentMapNodes = null;
        }


        public void SetMapData(MapNode[,] mapNodes)
        {
            currentMapNodes = mapNodes;
            Debug.Log($"Map generated with {mapNodes.GetLength(0)} layers and {mapNodes.GetLength(1)} width.");
        }

        public MapNode GetNode(int x, int y)
        {
            if (currentMapNodes == null) return null;
            if (x < 0 || x >= currentMapNodes.GetLength(0)) return null;
            if (y < 0 || y >= currentMapNodes.GetLength(1)) return null;
            return currentMapNodes[x, y];
        }
    }
}
