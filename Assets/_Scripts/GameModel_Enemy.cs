using GameCore.Helpers;
using GameCore.RefData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public partial class GameModel
    {
        public void GenerateRandomEnemy(long _id = -1)
        {
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