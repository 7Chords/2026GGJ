using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "Map/MapData")]
public class MapData : ScriptableObject
{
    [Header("Generation Settings")]
    public Vector2Int layerCount = new Vector2Int(12, 5); // x: Layers (Depth), y: Width
    public int repetitionCount = 5; // How many paths to start with

    [Header("Room Probabilities")]
    public List<RoomProbability> roomProbabilities;

    public MapNodeType GetRandomMapNodeType(System.Random random)
    {
        if (roomProbabilities == null || roomProbabilities.Count == 0)
        {
            return new MapNodeType { NodeType = RoomType.Enemy };
        }

        int totalWeight = roomProbabilities.Sum(rp => rp.weight);
        int randomValue = random.Next(0, totalWeight);
        int currentWeight = 0;

        foreach (var rp in roomProbabilities)
        {
            currentWeight += rp.weight;
            if (randomValue < currentWeight)
            {
                return new MapNodeType { NodeType = rp.type };
            }
        }

        return new MapNodeType { NodeType = RoomType.Enemy };
    }
}

[Serializable]
public class RoomProbability
{
    public RoomType type;
    public int weight;
}

[Serializable]
public struct MapNodeType
{
    public RoomType NodeType;
}
