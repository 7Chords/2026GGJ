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
        /// 生成当前敌人：从 initPartList 填充牌堆，并按 layoutPresetName 加载 <see cref="EnemyLayoutPreset"/> 摆脸（缺牌时跳过槽位）。必须配置有效预设资源。
        /// </summary>
        public void GenerateRandomEnemy(long _id = -1)
        {
            enemyFaceLayoutTurnIndex = 0;

            EnemyRefObj enemyRef = null;
            if (_id != -1)
            {
                enemyRef = SCRefDataMgr.instance.enemyRefList.refDataList.Find(
                    x => x.id == _id && x.floor == playerInfo.playerFloor);
            }
            else
            {
                int layerX = playerInfo.GetMapLayerXForEncounter();
                int floor = playerInfo.playerFloor;
                List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList
                    .Where(refObj => refObj.floor == floor
                        && !refObj.isBoss
                        && refObj.battleType != EBattleType.EVENT
                        && refObj.column == layerX + 1).ToList();
                if (enemies == null || enemies.Count == 0)
                {
                    enemies = SCRefDataMgr.instance.enemyRefList.refDataList
                        .Where(refObj => refObj.floor == 1
                            && !refObj.isBoss
                            && refObj.battleType != EBattleType.EVENT
                            && refObj.column == layerX + 1).ToList();
                }
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
            if (encounterPreset == null)
            {
                SCDebugHelper.LogError(
                    $"[Enemy] id={enemyRef.id} layoutPresetName=\"{enemyRef.layoutPresetName}\" 未加载到 EnemyLayoutPreset，请检查 Resources 路径与配表。");
                return;
            }

            EnemyTurnFaceLayout turnLayout = null;
            if (encounterPreset.turnLayouts != null && encounterPreset.turnLayouts.Count > 0)
            {
                int turnIdx = EnemyLayoutPresetApplicator.ResolveEnemyLayoutTurnIndex(0, encounterPreset.turnLayouts.Count);
                turnLayout = encounterPreset.turnLayouts[turnIdx];
            }
            EnemyLayoutPresetApplicator.PrepareBusyFromTurnLayoutBestEffort(curEnemyInfo, turnLayout, out var resolvedSlots);
            EnemyLayoutPresetApplicator.ApplyTurnLayoutToFace(curEnemyInfo, enemyFaceGridInfoList, resolvedSlots);
        }
    }
}
