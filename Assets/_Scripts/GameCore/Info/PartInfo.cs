using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class PartInfo
    {
        public PartRefObj partRefObj;
        public int currentHealth;
        public Vector2Int gridPos; // Grid Coordinates
        public int rotation; // 0, 1, 2, 3 (CCW 90 degrees steps)

        public PartInfo(PartRefObj _partRefObj)
        {
            partRefObj = _partRefObj;
            currentHealth = partRefObj.partHealth;
            gridPos = new Vector2Int(-1, -1);
            rotation = 0;
        }
    }
}
