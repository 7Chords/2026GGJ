using GameCore.Logic;
using GameCore.RefData;
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
        public List<EntryInfo> entryInfoList;

        public PartLogic logicObj;//逻辑实例
        public bool isEnemyPart;


        public PartInfo(PartRefObj _partRefObj,bool _isEnemyPart)
        {
            if (_partRefObj == null)
                return;
            partRefObj = _partRefObj;
            isEnemyPart = _isEnemyPart;
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

            entryInfoList = new List<EntryInfo>();
            EntryInfo entryInfo = null;
            foreach(var entry in partRefObj.entryList)
            {
                entryInfo = new EntryInfo(entry);
                entryInfoList.Add(entryInfo);
            }
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
        public void ResetToDeck()
        {
            ResetToBusy();
        }
        public void ResetToBag()
        {
            ResetToBusy();
            currentHealth = maxHealth;
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


        public void TriggerActiveLogic(EAttributeTriggerPointType _pointType,object[] _objs = null)
        {
            logicObj = PartLogicFactory.CreateLogic(partRefObj.id, this);
            switch (_pointType)
            {
                case EAttributeTriggerPointType.ACTIVE:
                    {
                        logicObj?.OnPartActive();
                    }
                    break;
                case EAttributeTriggerPointType.GET_HIT:
                    {
                        int damage = (int)_objs[0];
                        logicObj?.OnPartGetHit(damage);
                    }
                    break;
                case EAttributeTriggerPointType.DIE:
                    {
                        logicObj?.OnPartDie();
                    }
                    break;
                case EAttributeTriggerPointType.GET_EFFECT:
                    {
                        //todo
                    }
                    break;
            }

        }


    }
}
