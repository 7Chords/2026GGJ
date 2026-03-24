using GameCore.Data;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public partial class GameModel
    {
        /// <summary>
        /// 生成当前敌人：若 enemy 表配置了 layoutPresetName 则仅用预设摆脸（缺牌时跳过槽位，不回退随机）；否则随机手牌 + 算法摆脸。
        /// </summary>
        public void GenerateRandomEnemy(long _id = -1)
        {
            enemyFaceLayoutTurnIndex = 0;

            EnemyRefObj enemyRef = null;
            if (_id != -1)
            {
                int layerX = playerInfo.GetMapLayerXForEncounter();
                List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList
                    .Where(refObj => refObj.floor == playerInfo.playerFloor
                        && refObj.column == layerX + 1).ToList();
                if (enemies == null || enemies.Count == 0) return;
                enemyRef = enemies.Find(x => x.id == _id);
            }
            else
            {
                int layerX = playerInfo.GetMapLayerXForEncounter();
                List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList
                    .Where(refObj => refObj.floor == playerInfo.playerFloor
                        && !refObj.isBoss
                        && refObj.column == layerX + 1).ToList();
                if (enemies == null || enemies.Count == 0) return;
                enemyRef = enemies[Random.Range(0, enemies.Count)];
            }
            if (enemyRef == null)
                return;
            curEnemyInfo = new EnemyInfo(enemyRef);

            if (enemyRef.initPartList != null && enemyRef.initPartList.Count > 0)
            {
                var partRefList = new List<PartLevelRefObj>();
                for (int i = 0; i < enemyRef.initPartList.Count; i++)
                {
                    for (int j = 0; j < enemyRef.initPartList[i].partAmount; j++)
                    {
                        PartLevelRefObj pr = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == enemyRef.initPartList[i].partLevelId);
                        if (pr != null) partRefList.Add(pr);
                    }
                }
                foreach (var pr in partRefList)
                {
                    PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == pr.partId);
                    curEnemyInfo.deckPartInfoList.Add(new PartInfo(partRefObj, true, pr.partLevel));
                }
            }

            var encounterPreset = ResourcesHelper.LoadAsset<EnemyLayoutPreset>(enemyRef.layoutPresetName);

            if (encounterPreset != null)
            {
                EnemyTurnFaceLayout turnLayout = null;
                if (encounterPreset.turnLayouts != null && encounterPreset.turnLayouts.Count > 0)
                {
                    int turnIdx = EnemyLayoutPresetApplicator.ResolveEnemyLayoutTurnIndex(0, encounterPreset.turnLayouts.Count);
                    turnLayout = encounterPreset.turnLayouts[turnIdx];
                }
                EnemyLayoutPresetApplicator.PrepareBusyFromTurnLayoutBestEffort(curEnemyInfo, turnLayout, out var resolvedSlots);
                EnemyLayoutPresetApplicator.ApplyTurnLayoutToFace(curEnemyInfo, enemyFaceGridInfoList, resolvedSlots);
                return;
            }

            int pickCount = Mathf.Min(GameConst.INIT_ENEMY_PART_COUNT, curEnemyInfo.deckPartInfoList.Count);
            for (int i = 0; i < pickCount; i++)
            {
                int idx = Random.Range(0, curEnemyInfo.deckPartInfoList.Count);
                PartInfo selectPartInfo = curEnemyInfo.deckPartInfoList[idx];
                curEnemyInfo.deckPartInfoList.RemoveAt(idx);
                curEnemyInfo.busyPartInfoList.Add(selectPartInfo);
            }

            EnemyLayoutGenerator.GenerateLayout(curEnemyInfo, enemyFaceGridInfoList);
        }
    }
}
