using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 地图格子纯数据（不依赖 UI 父节点）。先由 MapGenerator 生成并交给 MapManager 暂存，打开地图 UI 后再实例化 MapNode。
    /// </summary>
    public class MapCellLayoutData
    {
        public int gridX;
        public int gridY;
        public bool isActive;
        public List<int> nextLayerConnectedNodes = new List<int>();
        public ERoomType roomType;
        public float angleZ;
        public Vector3 localPosition;
    }

    public struct MapLineSegment
    {
        public Vector2 start;
        public Vector2 end;
    }
}
