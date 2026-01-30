using System.Collections;
using System.Collections.Generic;
using GameCore.UI;
using SCFrame;
using SCFrame.UI;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    public MapNode[,] CurrentMapNodes { get; private set; }

    public override void OnInitialize()
    {
        UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
    }

    public override void OnDiscard()
    {
        CurrentMapNodes = null;
    }

    public void StartGameMap()
    {
        UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
    }

    public void SetMapData(MapNode[,] mapNodes)
    {
        CurrentMapNodes = mapNodes;
        Debug.Log($"Map generated with {mapNodes.GetLength(0)} layers and {mapNodes.GetLength(1)} width.");
    }

    public MapNode GetNode(int x, int y)
    {
        if (CurrentMapNodes == null) return null;
        if (x < 0 || x >= CurrentMapNodes.GetLength(0)) return null;
        if (y < 0 || y >= CurrentMapNodes.GetLength(1)) return null;
        return CurrentMapNodes[x, y];
    }
}
