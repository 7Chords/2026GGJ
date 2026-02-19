using GameCore.Helpers;
using GameCore.RefData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public partial class GameModel
    {
        public void GenerateRandomEnemy()
        {
            List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList
                .Where(refObj => refObj.floor == playerInfo.playerFloor).ToList();
            if (enemies == null || enemies.Count == 0) return;
            EnemyRefObj enemyRef = enemies[Random.Range(0, enemies.Count)];
            curEnemyInfo = new EnemyInfo(enemyRef);

            if (enemyRef.initPartList != null && enemyRef.initPartList.Count > 0)
            {
                var partRefList = new List<PartRefObj>();
                for (int i = 0; i < enemyRef.initPartList.Count; i++)
                {
                    for (int j = 0; j < enemyRef.initPartList[i].partAmount; j++)
                    {
                        var pr = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == enemyRef.initPartList[i].partId);
                        if (pr != null) partRefList.Add(pr);
                    }
                }
                foreach (var pr in partRefList)
                    curEnemyInfo.deckPartInfoList.Add(new PartInfo(pr, true));

                int pickCount = Mathf.Min(GameConst.INIT_ENEMY_PART_COUNT, curEnemyInfo.deckPartInfoList.Count);
                for (int i = 0; i < pickCount; i++)
                {
                    int idx = Random.Range(0, curEnemyInfo.deckPartInfoList.Count);
                    PartInfo selectPartInfo = curEnemyInfo.deckPartInfoList[idx];
                    curEnemyInfo.deckPartInfoList.RemoveAt(idx);
                    curEnemyInfo.busyPartInfoList.Add(selectPartInfo);
                }
            }

            EnemyLayoutGenerator.GenerateLayout(curEnemyInfo, enemyFaceGridInfoList);
        }
    }
}