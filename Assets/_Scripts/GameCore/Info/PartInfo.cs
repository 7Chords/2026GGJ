using GameCore.Battle;
using GameCore.RefData;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class PartInfo
    {
        public PartRefObj partRefObj;
        public int partLevel;
        public int currentHealth;
        public int maxHealth;
        public bool isOnFace;
        public bool isEnemyPart;
        public int rotateStep;
        public List<Vector2Int> localOccupyPosList;
        public List<Vector2Int> localEffectPosList;
        public List<Vector2Int> curOccupyFacePosList;
        public List<Vector2Int> curEffectFacePosList;
        public List<EntryInfo> entryInfoList;

        public PartLogic partLogic;//逻辑实例
        public BuffLogic buffLogic;

        public PartLevelRefObj levelRefObj;

        public PartInfo(PartRefObj _partRefObj,bool _isEnemyPart)
        {
            if (_partRefObj == null)
                return;
            partRefObj = _partRefObj;
            partLevel = 1;//初始为1级
            levelRefObj = GetLevelRefObj();
            if (levelRefObj == null)
                return;
            isEnemyPart = _isEnemyPart;
            maxHealth = levelRefObj.partHealth;
            currentHealth = maxHealth;
            isOnFace = false;
            localOccupyPosList = new List<Vector2Int>();
            for(int i =0;i<partRefObj.occupyPosList.Count;i++)
            {
                localOccupyPosList.Add(new Vector2Int(partRefObj.occupyPosList[i].x, partRefObj.occupyPosList[i].y));
            }
            localEffectPosList = new List<Vector2Int>();
            for (int i = 0; i < levelRefObj.effectPosList.Count; i++)
            {
                localEffectPosList.Add(new Vector2Int(levelRefObj.effectPosList[i].x, levelRefObj.effectPosList[i].y));
            }

            curOccupyFacePosList = new List<Vector2Int>();
            curEffectFacePosList = new List<Vector2Int>();

            entryInfoList = new List<EntryInfo>();
            EntryInfo entryInfo = null;
            foreach(var entry in levelRefObj.entryList)
            {
                entryInfo = new EntryInfo(entry);
                entryInfoList.Add(entryInfo);
            }
            partLogic = PartLogicFactory.CreateLogic(this);
            buffLogic = new BuffLogic();

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
            for (int i = 0; i < levelRefObj.effectPosList.Count; i++)
            {
                localEffectPosList.Add(new Vector2Int(levelRefObj.effectPosList[i].x, levelRefObj.effectPosList[i].y));
            }

            entryInfoList = new List<EntryInfo>();
            EntryInfo entryInfo = null;
            foreach (var entry in levelRefObj.entryList)
            {
                entryInfo = new EntryInfo(entry);
                entryInfoList.Add(entryInfo);
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
            buffLogic.ClearAllBuffs();
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
        public bool HasBuff()
        {
            return buffLogic != null && buffLogic.buffList.Count > 0;
        }
        public int GetStrengthenCount()
        {
            return partLevel - 1;
        }
        public void TriggerActiveLogic()
        {
            if (partLogic == null)
                return;
            if (currentHealth == 0)
                return;
            PartLogicFactory.RefreshTriggers(partLogic, this, EAttributeTriggerPointType.ACTIVE);
            partLogic.OnPartActive();
        }
        public void TriggerGetHitLogic(PartInfo senderInfo, int damage)
        {
            if (partLogic == null)
                return;
            if (currentHealth == 0)
                return;
            PartLogicFactory.RefreshTriggers(partLogic, this, EAttributeTriggerPointType.GET_HIT);
            partLogic.OnPartGetHit(senderInfo, damage);
        }

        public void TriggerBuff(EAttributeTriggerPointType _triggerPointType)
        {
            buffLogic.TriggerPartBuff(_triggerPointType);
        }

        public bool HasBuff(EAttributeTriggerPointType _triggerPointType)
        {
            return buffLogic.HasFindByTriggerPointType(_triggerPointType);
        }
        public void AddBuff(BuffInfo _buffInfo)
        {
            if (_buffInfo == null)
                return;
            buffLogic.AddBuff(_buffInfo);
        }

        public void RemoveBuff(BuffInfo _buffInfo)
        {
            if (_buffInfo == null)
                return;
            buffLogic.RemoveBuff(_buffInfo);
        }

        public BuffInfo GetBuff(long _id)
        {
            return buffLogic.FindBuff(_id);
        }
        public BuffInfo GetBuff(EBuffType _buffType)
        {
            return buffLogic.FindBuff(_buffType);
        }
        public void LevelUp()
        {
            partLevel++;
            levelRefObj = GetLevelRefObj();
            if (levelRefObj == null)
                return;
            maxHealth = levelRefObj.partHealth;
            currentHealth = maxHealth;
            localEffectPosList = levelRefObj.GetEffectPosList();

            entryInfoList = new List<EntryInfo>();
            EntryInfo entryInfo = null;
            foreach (var entry in levelRefObj.entryList)
            {
                entryInfo = new EntryInfo(entry);
                entryInfoList.Add(entryInfo);
            }
        }

        public PartLevelRefObj GetLevelRefObj()
        {
            PartLevelRefObj levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.partId == partRefObj.id
    && x.partLevel == partLevel);
            return levelRefObj;
        }

        public bool HasNextLevel()
        {
            PartLevelRefObj levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.partId == partRefObj.id
&& x.partLevel == partLevel + 1);
            return levelRefObj != null;
        }
    }
}
