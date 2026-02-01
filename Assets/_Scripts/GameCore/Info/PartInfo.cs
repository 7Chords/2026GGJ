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
        
        public System.Func<Transform> GetAnchorTransformEvent;
        
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
                 Debug.Log($"[PartInfo] Attempting to create logic for {partRefObj.partName}: {partRefObj.logicClassName}");
                logicObj = PartLogicFactory.CreateLogic(partRefObj.logicClassName);
                if (logicObj != null) 
                {
                    logicObj.Initialize(this);
                     Debug.Log($"[PartInfo] Logic created successfully: {logicObj.GetType().Name}");
                }
                else
                {
                    Debug.LogError($"[PartInfo] Failed to create logic: {partRefObj.logicClassName}");
                }
            }
            else
            {
                 Debug.Log($"[PartInfo] No logic class name for {partRefObj.partName}");
            }
        }
    }
}
