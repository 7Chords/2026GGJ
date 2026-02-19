using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle
{
    public static class PartLogicHandler
    {
        public static void DealAttack(PartInfo _partInfo, EntryInfo _entryInfo)
        {
            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_SUCCESS, _partInfo);
            float totalDamage = _entryInfo.attributeValue;
            List<PartInfo> attackPartInfoList = new List<PartInfo>();
            Dictionary<PartInfo, int> partOccpuyGridNumDic = new Dictionary<PartInfo, int>();
            float perGridDamage = _entryInfo.attributeValue / _partInfo.curEffectFacePosList.Count;
            int emptyGridNum = 0;
            for(int i =0;i<_partInfo.curEffectFacePosList.Count;i++)
            {
                if(!_partInfo.isEnemyPart)
                {
                    FaceGridInfo gridInfo = GameModel.instance.enemyFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                    if (gridInfo == null)
                        return;

                    if (gridInfo.ownerPart == null)
                        emptyGridNum++;
                    else
                    {
                        if (!partOccpuyGridNumDic.ContainsKey(gridInfo.ownerPart))
                            partOccpuyGridNumDic.Add(gridInfo.ownerPart, 1);
                        else
                            partOccpuyGridNumDic[gridInfo.ownerPart]++;
                    }
                }
                else
                {
                    FaceGridInfo gridInfo = GameModel.instance.playerFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                    if (gridInfo == null)
                        return;

                    if (gridInfo.ownerPart == null)
                        emptyGridNum++;
                    else
                    {
                        if (!partOccpuyGridNumDic.ContainsKey(gridInfo.ownerPart))
                            partOccpuyGridNumDic.Add(gridInfo.ownerPart, 1);
                        else
                            partOccpuyGridNumDic[gridInfo.ownerPart]++;
                    }
                }
            }
            if (!_partInfo.isEnemyPart)
            {
                GameModel.instance.EnemyTakeDamage(Mathf.RoundToInt(perGridDamage * emptyGridNum));
            }
            else
            {
                GameModel.instance.PlayerTakeDamage(Mathf.RoundToInt(perGridDamage * emptyGridNum));
            }
            foreach(var pair in partOccpuyGridNumDic)
            {
                GameModel.instance.PartTakeDamage(pair.Key, _partInfo,Mathf.RoundToInt(pair.Value * perGridDamage));
            }
        }
        public static void DealRealAttack(PartInfo _partInfo, EntryInfo _entryInfo)
        {
            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_SUCCESS, _partInfo);

            float damage = _entryInfo.attributeValue;
            if (!_partInfo.isEnemyPart)
            {
                GameModel.instance.EnemyTakeDamage(Mathf.RoundToInt(damage));
            }
            else
            {
                GameModel.instance.PlayerTakeDamage(Mathf.RoundToInt(damage));
            }
        }
        public static void DealReflect(PartInfo _receiverInfo, EntryInfo _entryInfo,PartInfo _senderInfo,int _damage)
        {
            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_SUCCESS, _senderInfo);

            float damage = _entryInfo.attributeValue;
            GameModel.instance.PartTakeDamage(_senderInfo, _receiverInfo, Mathf.RoundToInt(damage));
        }
        public static void DealTriggerMore(PartInfo _partInfo, EntryInfo _entryInfo)
        {
            float chance = _entryInfo.attributeChance;
            float triggerMoreTimes = _entryInfo.attributeValue;
            float randomNum = RandomUtility.GetRandomGenerator(EModuleType.COMBAT).Next(0, 100)/100f;
            List<PartInfo> partInfoList = new List<PartInfo>();

            if(randomNum < chance)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_SUCCESS, _partInfo);
                for (int j =0;j< triggerMoreTimes-1;j++)
                {
                    if (!_partInfo.isEnemyPart)
                    {
                        for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                        {
                            FaceGridInfo gridInfo = GameModel.instance.playerFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                            if (gridInfo == null)
                                continue;
                            if (gridInfo.hasPart && gridInfo.ownerPart != null)
                                partInfoList.Add(gridInfo.ownerPart);
                        }
                        for (int i = 0; i < partInfoList.Count; i++)
                        {
                            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, partInfoList[i], _partInfo);
                            BattleManager.instance.InsertPartAt(true, BattleManager.instance.GetIndexOfPartInfo(partInfoList[i], true), partInfoList[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                        {
                            FaceGridInfo gridInfo = GameModel.instance.enemyFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                            if (gridInfo == null)
                                continue;
                            if (gridInfo.hasPart && gridInfo.ownerPart != null)
                                partInfoList.Add(gridInfo.ownerPart);
                        }
                        for (int i = 0; i < partInfoList.Count; i++)
                        {
                            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, partInfoList[i], _partInfo);
                            BattleManager.instance.InsertPartAt(false, BattleManager.instance.GetIndexOfPartInfo(partInfoList[i], false), partInfoList[i]);
                        }
                    }
                }
            }
            else
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_FAIL, _partInfo);
            }
        }
        public static void DealAttackMultiplier(PartInfo _partInfo, EntryInfo _entryInfo)
        {
            float chance = _entryInfo.attributeChance;
            float mulitiplier = _entryInfo.attributeValue;
            float randomNum = RandomUtility.GetRandomGenerator(EModuleType.COMBAT).Next(0, 100) / 100f;
            List<PartInfo> partInfoList = new List<PartInfo>();

            if (randomNum < chance)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_SUCCESS, _partInfo);

                if (!_partInfo.isEnemyPart)
                {
                    for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                    {
                        FaceGridInfo gridInfo = GameModel.instance.playerFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                        if (gridInfo == null)
                            continue;
                        if (gridInfo.hasPart && gridInfo.ownerPart != null && !partInfoList.Contains(gridInfo.ownerPart))
                            partInfoList.Add(gridInfo.ownerPart);
                    }
                    for (int i = 0; i < partInfoList.Count; i++)
                    {
                        if (partInfoList[i].partRefObj.partType == EPartType.MOUTH)
                        {
                            EntryInfo info = partInfoList[i].entryInfoList.Find(x => (x.attributeType == EAttributeType.ATTACK || x.attributeType == EAttributeType.REAL_ATTACK));
                            if (info == null)
                                continue;
                            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, partInfoList[i], _partInfo);
                            info.attributeValue *= mulitiplier;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                    {
                        FaceGridInfo gridInfo = GameModel.instance.enemyFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                        if (gridInfo == null)
                            continue;
                        if (gridInfo.hasPart && gridInfo.ownerPart != null && !partInfoList.Contains(gridInfo.ownerPart))
                            partInfoList.Add(gridInfo.ownerPart);
                    }
                    for (int i = 0; i < partInfoList.Count; i++)
                    {
                        if (partInfoList[i].partRefObj.partType == EPartType.MOUTH)
                        {
                            EntryInfo info = partInfoList[i].entryInfoList.Find(x => x.attributeType == EAttributeType.ATTACK);
                            if (info == null)
                                continue;
                            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, partInfoList[i], _partInfo);
                            info.attributeValue *= mulitiplier;
                        }
                    }
                }
            }
            else
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_FAIL, _partInfo);

            }
        }
        public static void DealHitChanceUp(PartInfo _partInfo, EntryInfo _entryInfo)
        {

        }
        public static void DealHitChanceDown(PartInfo _partInfo, EntryInfo _entryInfo)
        {

        }
        public static void DealTriggerChanceUp(PartInfo _partInfo, EntryInfo _entryInfo)
        {

        }
        public static void DealHealPart(PartInfo _partInfo, EntryInfo _entryInfo)
        {
            float healAmount = _entryInfo.attributeValue;
            List<PartInfo> partInfoList = new List<PartInfo>();

            if (!_partInfo.isEnemyPart)
            {
                for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                {
                    FaceGridInfo gridInfo = GameModel.instance.playerFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                    if (gridInfo == null)
                        continue;
                    if (gridInfo.hasPart && gridInfo.ownerPart != null)
                        partInfoList.Add(gridInfo.ownerPart);
                }
                for (int i = 0; i < partInfoList.Count; i++)
                    GameModel.instance.PartHeal(partInfoList[i], Mathf.RoundToInt(healAmount));
            }
            else
            {
                for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                {
                    FaceGridInfo gridInfo = GameModel.instance.enemyFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                    if (gridInfo == null)
                        continue;
                    if (gridInfo.hasPart && gridInfo.ownerPart != null)
                        partInfoList.Add(gridInfo.ownerPart);
                }
                for (int i = 0; i < partInfoList.Count; i++)
                    GameModel.instance.PartHeal(partInfoList[i], Mathf.RoundToInt(healAmount));
            }
        }
        public static void DealPartLoseTurn(PartInfo _partInfo, EntryInfo _entryInfo)
        {
            float chance = _entryInfo.attributeChance;
            float randomNum = RandomUtility.GetRandomGenerator(EModuleType.COMBAT).Next(0, 100)/100f;
            List<PartInfo> partInfoList = new List<PartInfo>();

            if (randomNum < chance)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_SUCCESS, _partInfo);

                if (!_partInfo.isEnemyPart)
                {
                    for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                    {
                        FaceGridInfo gridInfo = GameModel.instance.enemyFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                        if (gridInfo == null)
                            continue;
                        if (gridInfo.hasPart && gridInfo.ownerPart != null)
                            partInfoList.Add(gridInfo.ownerPart);
                    }
                    for (int i = 0; i < partInfoList.Count; i++)
                    {
                        SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, partInfoList[i], _partInfo);
                        BattleManager.instance.RemovePartFromList(false, partInfoList[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < _partInfo.curEffectFacePosList.Count; i++)
                    {
                        FaceGridInfo gridInfo = GameModel.instance.playerFaceGridInfoList.Find(x => x.pos == _partInfo.curEffectFacePosList[i]);
                        if (gridInfo == null)
                            continue;
                        if (gridInfo.hasPart && gridInfo.ownerPart != null)
                            partInfoList.Add(gridInfo.ownerPart);
                    }
                    for (int i = 0; i < partInfoList.Count; i++)
                    {
                        SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, partInfoList[i], _partInfo);
                        BattleManager.instance.RemovePartFromList(true, partInfoList[i]);
                    }
                }
            }
            else
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_FAIL, _partInfo);
            }
        }
    }
}
