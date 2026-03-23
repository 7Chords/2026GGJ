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
        /// 生成当前敌人：优先使用 ScriptableObject 预设脸部布局；若无配置或应用失败则回退为随机手牌 + 算法摆脸。
        /// </summary>
        public void GenerateRandomEnemy(long _id = -1)
        {
            enemyFaceLayoutTurnIndex = 0;

            EnemyRefObj enemyRef = null;
            if (_id != -1)
            {
                List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList
                    .Where(refObj => refObj.floor == playerInfo.playerFloor
                        && refObj.column == playerInfo.playerMapPosition.x + 1).ToList();
                if (enemies == null || enemies.Count == 0) return;
                enemyRef = enemies.Find(x => x.id == _id);
            }
            else
            {
                List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList
                    .Where(refObj => refObj.floor == playerInfo.playerFloor
                        && !refObj.isBoss
                        && refObj.column == playerInfo.playerMapPosition.x + 1).ToList();
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

            if (encounterPreset != null && encounterPreset.turnLayouts != null && encounterPreset.turnLayouts.Count > 0)
            {
                int turnIdx = EnemyLayoutPresetApplicator.GetClampedTurnIndex(0, encounterPreset.turnLayouts.Count);
                var turnLayout = encounterPreset.turnLayouts[turnIdx];
                if (turnLayout != null && turnLayout.slots != null && turnLayout.slots.Count > 0)
                {
                    if (EnemyLayoutPresetApplicator.TryPrepareBusyFromTurnLayout(curEnemyInfo, turnLayout))
                    {
                        EnemyLayoutPresetApplicator.ApplyTurnLayoutToFace(curEnemyInfo, enemyFaceGridInfoList, turnLayout);
                        return;
                    }
                }
                SCDebugHelper.LogWarning($"[EnemyLayoutPreset] 敌人 id={enemyRef.id} 预设布局未成功应用，回退随机摆脸");
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
