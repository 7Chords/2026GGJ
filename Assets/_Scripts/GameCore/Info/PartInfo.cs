using GameCore.RefData;
using GameCore.Logic; // Added
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
        
        public BasePartLogic logicObj; // 逻辑实例

        public PartInfo(PartRefObj _partRefObj)
        {
            partRefObj = _partRefObj;
            currentHealth = partRefObj.partHealth;
            gridPos = new Vector2Int(-1, -1);
            rotation = 0;
            
            // 初始化逻辑对象
            if (!string.IsNullOrEmpty(partRefObj.logicClassName))
            {
                logicObj = PartLogicFactory.CreateLogic(partRefObj.logicClassName);
                logicObj?.Initialize(this);
            }
        }
    }
}
