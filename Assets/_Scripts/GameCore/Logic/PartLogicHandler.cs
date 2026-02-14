using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Logic
{
    public static class PartLogicHandler
    {
        public static void DealAttack(PartInfo _partInfo, EntryInfo _entryInfo)
        {
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
                GameModel.instance.PartTakeDamage(pair.Key, Mathf.RoundToInt(pair.Value * perGridDamage));
            }
        }
        public static void DealReflect(PartInfo _partInfo, EntryInfo _entryInfo)
        {

        }
        public static void DealTriggerMore(PartInfo _partInfo, EntryInfo _entryInfo)
        {

        }
        public static void DealAttackMore(PartInfo _partInfo, EntryInfo _entryInfo)
        {

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
    }
}
