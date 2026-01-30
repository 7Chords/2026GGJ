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

        public PartInfo(PartRefObj _partRefObj)
        {
            partRefObj = _partRefObj;
            currentHealth = partRefObj.partHealth;
        }
    }
}
