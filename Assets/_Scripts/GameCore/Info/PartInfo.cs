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

        public PartInfo(PartRefObj _partRefObj,bool _isEnemyPart,int _level = 1)
        {
            if (_partRefObj == null)
                return;
            partRefObj = _partRefObj;
            partLevel = _level;//初始为1级
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
        /// <summary>
        /// Battle order key: first occupied cell in face reading order (top-to-bottom, then left-to-right).
        /// Not the AABB corner — independent min X / min Y can point at an empty cell inside the bbox.
        /// </summary>
        public Vector2Int GetMinGridPos()
        {
            if (curOccupyFacePosList == null || curOccupyFacePosList.Count == 0)
                return new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int best = curOccupyFacePosList[0];
            for (int i = 1; i < curOccupyFacePosList.Count; i++)
            {
                Vector2Int p = curOccupyFacePosList[i];
                if (p.y < best.y || (p.y == best.y && p.x < best.x))
                    best = p;
            }
            return best;
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

        public void ReduceBuffLayer(long _buffId,int _reduceLayer)
        {
            if (_buffId < 0 || _reduceLayer <= 0)
                return;
            buffLogic.ReduceBuffLayer(_buffId, _reduceLayer);
        }

        public void ReduceAllBuffLayer(int _reduceLayer)
        {
            if (_reduceLayer <= 0)
                return;
            buffLogic.ReduceAllBuffLayer(_reduceLayer);

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

        /// <summary> 对应 part_level 表主键，用于敌人预设布局匹配部位实例 </summary>
        public long GetPartLevelRefId()
        {
            return levelRefObj != null ? levelRefObj.id : -1;
        }

        public bool HasNextLevel()
        {
            PartLevelRefObj levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.partId == partRefObj.id
&& x.partLevel == partLevel + 1);
            return levelRefObj != null;
        }
    }
}
