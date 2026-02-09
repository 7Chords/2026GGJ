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
        public bool isOnFace;
        public int rotateStep;
        public List<Vector2Int> localGridPosList;
        public List<Vector2Int> curOccpuyFacePosList;
        public BasePartLogic logicObj;//逻辑实例

        public PartInfo(PartRefObj _partRefObj)
        {
            partRefObj = _partRefObj;
            maxHealth = partRefObj.partHealth;
            currentHealth = maxHealth;
            isOnFace = false;
            localGridPosList = new List<Vector2Int>();
            for(int i =0;i<partRefObj.occupyPosList.Count;i++)
            {
                localGridPosList.Add(new Vector2Int(partRefObj.occupyPosList[i].x, partRefObj.occupyPosList[i].y));
            }
            curOccpuyFacePosList = new List<Vector2Int>();
            logicObj = PartLogicFactory.CreateLogic(partRefObj.id);
            if (logicObj != null)
                logicObj.Initialize(this);
            //else
            //    SCDebugHelper.LogError($"Failed to create logic: {partRefObj.partName}");
        }
        public void ResetToBusy()
        {
            isOnFace = false;
            rotateStep = 0;
            localGridPosList = new List<Vector2Int>();
            for (int i = 0; i < partRefObj.occupyPosList.Count; i++)
            {
                localGridPosList.Add(new Vector2Int(partRefObj.occupyPosList[i].x, partRefObj.occupyPosList[i].y));
            }

            curOccpuyFacePosList = new List<Vector2Int>();
        }

    }
}
