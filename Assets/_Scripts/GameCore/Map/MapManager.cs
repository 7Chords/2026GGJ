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

        /// <summary>
        /// Same rules as the floating map marker: landed cell, else pending target, else first-column start fallback.
        /// </summary>
        public Vector2Int GetDisplayedPlayerMapGrid(out bool hasGrid)
        {
            hasGrid = false;
            var p = GameModel.instance?.playerInfo;
            if (p == null || currentMapNodes == null)
                return new Vector2Int(-1, -1);

            if (p.playerMapPosition.x >= 0)
            {
                hasGrid = true;
                return p.playerMapPosition;
            }

            if (p.pendingMapTargetPosition.x >= 0)
            {
                hasGrid = true;
                return p.pendingMapTargetPosition;
            }

            var grid = currentMapNodes;
            int h = grid.GetLength(1);
            if (h <= 0 || grid.GetLength(0) <= 0)
                return new Vector2Int(-1, -1);

            int cy = h / 2;
            MapNode n = GetNode(0, cy);
            if (n != null && n.isActive)
            {
                hasGrid = true;
                return new Vector2Int(0, cy);
            }

            for (int j = 0; j < h; j++)
            {
                n = GetNode(0, j);
                if (n != null && n.isActive)
                {
                    hasGrid = true;
                    return new Vector2Int(0, j);
                }
            }

            return new Vector2Int(-1, -1);
        }
    }
}
