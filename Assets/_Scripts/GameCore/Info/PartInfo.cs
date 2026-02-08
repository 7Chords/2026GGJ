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
        public int maxHealth;
        public Vector2Int startGridPos;
        
        public BasePartLogic logicObj;//逻辑实例

        public PartInfo(PartRefObj _partRefObj)
        {
            partRefObj = _partRefObj;
            maxHealth = partRefObj.partHealth;
            currentHealth = maxHealth;
            startGridPos = new Vector2Int(-1, -1);

            logicObj = PartLogicFactory.CreateLogic(partRefObj.id);
            if (logicObj != null)
                logicObj.Initialize(this);
            else
                SCDebugHelper.LogError($"Failed to create logic: {partRefObj.partName}");
        }
    }
}
