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
        [Header("x：每列多少个节点/y：多少列")]
        public Vector2Int layerCount = new Vector2Int(12, 5);
        [Header("有多少个节点是公共的(交叉路口节点)")]
        public int repetitionCount = 5;

        [Header("首层 / 末层节点类型（原逻辑写死为战斗与 BOSS，现可配）")]
        [Tooltip("第一层（入口侧）节点固定使用的房间类型")]
        public ERoomType firstNodeRoomType = ERoomType.ENEMY;
        [Tooltip("最后一层节点固定使用的房间类型")]
        public ERoomType lastNodeRoomType = ERoomType.BOSS;

        [Header("房间分布概率排布")]
        public List<RoomProbability> roomProbabilities;

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
