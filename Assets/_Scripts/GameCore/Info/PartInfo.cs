using GameCore.RefData;
using GameCore.Logic;
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
        public List<Vector2Int> localOccupyPosList;
        public List<Vector2Int> localEffectPosList;
        public List<Vector2Int> curOccupyFacePosList;
        public List<Vector2Int> curEffectFacePosList;

        public BasePartLogic logicObj;//逻辑实例

        public PartInfo(PartRefObj _partRefObj)
        {
            partRefObj = _partRefObj;
            maxHealth = partRefObj.partHealth;
            currentHealth = maxHealth;
            isOnFace = false;
            localOccupyPosList = new List<Vector2Int>();
            for(int i =0;i<partRefObj.occupyPosList.Count;i++)
            {
                localOccupyPosList.Add(new Vector2Int(partRefObj.occupyPosList[i].x, partRefObj.occupyPosList[i].y));
            }
            localEffectPosList = new List<Vector2Int>();
            for (int i = 0; i < partRefObj.effectPosList.Count; i++)
            {
                localEffectPosList.Add(new Vector2Int(partRefObj.effectPosList[i].x, partRefObj.effectPosList[i].y));
            }

            curOccupyFacePosList = new List<Vector2Int>();
            curEffectFacePosList = new List<Vector2Int>();


            logicObj = PartLogicFactory.CreateLogic(partRefObj.id);
            if (logicObj != null)
                logicObj.Initialize(this);
            //else
            //    SCDebugHelper.LogError($"Failed to create logic: {partRefObj.partName}");
        }
        public void ResetToBusy()
        {
            rotateStep = 0;
            localOccupyPosList = new List<Vector2Int>();
            for (int i = 0; i < partRefObj.occupyPosList.Count; i++)
            {
                localOccupyPosList.Add(new Vector2Int(partRefObj.occupyPosList[i].x, partRefObj.occupyPosList[i].y));
            }
            localEffectPosList = new List<Vector2Int>();
            for (int i = 0; i < partRefObj.effectPosList.Count; i++)
            {
                localEffectPosList.Add(new Vector2Int(partRefObj.effectPosList[i].x, partRefObj.effectPosList[i].y));
            }
            ClearOnFaceState();
        }
        public void ClearOnFaceState()
        {
            isOnFace = false;
            curOccupyFacePosList = new List<Vector2Int>();
            curEffectFacePosList = new List<Vector2Int>();
        }
        public void RotateOnce()
        {
            rotateStep = (rotateStep + 1) % 4;
            localEffectPosList = GameCommon.RotateShapeAndMoveBySample(localEffectPosList, 1, localOccupyPosList);
            localOccupyPosList = GameCommon.RotateShapeAndMove2Zero(localOccupyPosList, 1);

            SCDebugHelper.LogWarning("------------------------");

            for (int i = 0; i < localOccupyPosList.Count; i++)
            {
                SCDebugHelper.LogWarning("occupyPos:" + localOccupyPosList[i]);
            }
            for (int i =0;i< localEffectPosList.Count;i++)
            {
                SCDebugHelper.LogWarning("effectPos:" + localEffectPosList[i]);
            }
        }

        public Vector2Int GetMinGridPos()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            for(int i=0;i<curOccupyFacePosList.Count;i++)
            {
                minX = Mathf.Min(curOccupyFacePosList[i].x, minX);
                minY = Mathf.Min(curOccupyFacePosList[i].y, minY);
            }
            return new Vector2Int(minX, minY);
        }
    }
}
