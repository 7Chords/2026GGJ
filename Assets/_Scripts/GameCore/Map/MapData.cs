using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    [CreateAssetMenu(fileName = "NewMapData", menuName = "Map/MapData")]
    public class MapData : ScriptableObject
    {
        [Header("地图行列")]
        public Vector2Int layerCount = new Vector2Int(12, 5);
        [Header("地图交叉点")]
        public int repetitionCount = 5;

        [Header("节点配置")]
        [Tooltip("第一个节点")]
        public ERoomType firstNodeRoomType = ERoomType.ENEMY;
        [Tooltip("最后一个节点")]
        public ERoomType lastNodeRoomType = ERoomType.BOSS;

        [Tooltip("When true, the column before the last uses penultimateNodeRoomType instead of interior random/quota.")]
        public bool usePenultimateNodeRoomType;
        [Tooltip("Room type for the second-to-last column (used when usePenultimateNodeRoomType and map has at least 3 columns).")]
        public ERoomType penultimateNodeRoomType = ERoomType.ENEMY;

        [Header("节点概率")]
        public List<RoomProbability> roomProbabilities;

        [Tooltip("If true, use legacy per-step weighted random for interior rooms. If false (default), after routes are built interior visible nodes get types by count quota so ratios match Room Probabilities.")]
        public bool useLegacyInteriorRoomRandom = false;

        public ERoomType GetRandomMapNodeType(System.Random random)
        {
            if (roomProbabilities == null || roomProbabilities.Count == 0)
            {
                return ERoomType.ENEMY;
            }

            int totalWeight = roomProbabilities.Sum(rp => rp.weight);
            int randomValue = random.Next(0, totalWeight);
            int currentWeight = 0;

            foreach (var rp in roomProbabilities)
            {
                currentWeight += rp.weight;
                if (randomValue < currentWeight)
                {
                    return rp.type;
                }
            }

            return ERoomType.ENEMY;
        }
    }

    [Serializable]
    public class RoomProbability
    {
        public ERoomType type;
        public int weight;
    }

}
