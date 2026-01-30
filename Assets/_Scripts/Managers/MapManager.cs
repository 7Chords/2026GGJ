using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public MapNode[,] CurrentMapNodes { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
