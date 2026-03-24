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

        /// <summary> 已生成、待实例化到地图 UI 的纯数据（无父节点时先生成并保存在此）。 </summary>
        public MapCellLayoutData[,] PendingLayout { get; private set; }
        public Vector2 PendingContentSize { get; private set; }
        public System.Collections.Generic.List<MapLineSegment> PendingLines { get; private set; }

        /// <summary> 当前地图布局对应的 MAP 模块随机种子，用于存档后「继续游戏」复现同一张图。未生成过为 -1。 </summary>
        public int LastMapLayoutSeed { get; private set; } = -1;

        public bool HasPendingLayout => PendingLayout != null;

        public override void OnInitialize()
        {
        }

        public override void OnDiscard()
        {
            currentMapNodes = null;
            ClearPendingLayout();
            LastMapLayoutSeed = -1;
        }

        public void SetPendingMapLayout(MapCellLayoutData[,] layout, Vector2 contentSize, System.Collections.Generic.List<MapLineSegment> lines)
        {
            PendingLayout = layout;
            PendingContentSize = contentSize;
            PendingLines = lines;
        }

        public void ClearPendingLayout()
        {
            PendingLayout = null;
            PendingLines = null;
        }

        public void ClearCurrentMapNodes()
        {
            currentMapNodes = null;
        }

        public void SetMapData(MapNode[,] mapNodes)
        {
            currentMapNodes = mapNodes;
            Debug.Log($"Map generated with {mapNodes.GetLength(0)} layers and {mapNodes.GetLength(1)} width.");
        }

        public void SetLastMapLayoutSeed(int seed)
        {
            LastMapLayoutSeed = seed;
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
