using GameCore.RefData;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public partial class GameModel
    {
        /// <summary> 读档后由 MapGenerator 在生成地图前读取；成功写入 pending 布局后清空。 </summary>
        public int? PendingRunMapLayoutSeed { get; private set; }

        public void ClearPendingRunMapLayoutSeed()
        {
            PendingRunMapLayoutSeed = null;
        }

        /// <summary>
        /// 完全重新开始一局：重置玩家数据到配表初始（含初始背包），清空敌人与 roll，并清除 pending。
        /// </summary>
        public void ResetRunForNewGame()
        {
            PlayerRefObj playerRefObj = SCRefDataMgr.instance.playerConfigRefObj;
            if (playerRefObj == null)
                return;

            playerInfo = new PlayerInfo(playerRefObj);

            PartEffectObj partEffectObj = null;
            PartInfo info = null;
            PartRefObj partRefObj = null;
            PartLevelRefObj levelRefObj = null;
            for (int i = 0; i < playerRefObj.initPartList.Count; i++)
            {
                partEffectObj = playerRefObj.initPartList[i];
                if (partEffectObj == null)
                    continue;
                for (int j = 0; j < partEffectObj.partAmount; j++)
                {
                    levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == partEffectObj.partId);
                    if (levelRefObj == null)
                        continue;
                    partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == levelRefObj.partId);
                    if (partRefObj == null)
                        continue;
                    info = new PartInfo(partRefObj, false, levelRefObj.partLevel);
                    playerInfo.bagPartInfoList.Add(info);
                }
            }

            playerInfo.ClearPendingMapMove();
            curEnemyInfo = null;
            rollStoreId = 0;
            rollEventId = 0;
            enemyFaceLayoutTurnIndex = 0;
            PendingRunMapLayoutSeed = null;
        }

        /// <summary> 从存档恢复玩家与地图进度字段。 </summary>
        public void ApplyRunSaveData(GameRunSave.RunSaveData data)
        {
            if (data == null)
                return;

            PlayerRefObj playerRefObj = SCRefDataMgr.instance.playerConfigRefObj;
            if (playerRefObj == null)
                return;

            playerInfo = new PlayerInfo(playerRefObj);
            playerInfo.playerMapPosition = new Vector2Int(data.mapX, data.mapY);
            playerInfo.pendingMapTargetPosition = new Vector2Int(data.pendingX, data.pendingY);
            playerInfo.playerFloor = data.floor;
            playerInfo.currentHealth = data.health;
            playerInfo.maxHealth = data.maxHealth;
            playerInfo.playerMoney = data.money;

            rollStoreId = data.rollStoreId;
            rollEventId = data.rollEventId;

            playerInfo.bagPartInfoList = DeserializeSavedParts(data.bagParts);
            playerInfo.deckPartInfoList = DeserializeSavedParts(data.deckParts);
            playerInfo.busyPartInfoList = DeserializeSavedParts(data.busyParts);
            playerInfo.battlePartInfoList = DeserializeSavedParts(data.battleParts);

            curEnemyInfo = null;
            enemyFaceLayoutTurnIndex = 0;

            PendingRunMapLayoutSeed = data.mapLayoutFromSave ? data.mapLayoutSeed : (int?)null;
        }

        static List<PartInfo> DeserializeSavedParts(GameRunSave.PartSaveEntry[] arr)
        {
            var list = new List<PartInfo>();
            if (arr == null)
                return list;
            foreach (var e in arr)
            {
                if (e == null)
                    continue;
                PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == e.partRefId);
                if (partRefObj == null)
                    continue;
                var pi = new PartInfo(partRefObj, false, e.level);
                pi.currentHealth = Mathf.Clamp(e.currentHealth, 0, pi.maxHealth);
                list.Add(pi);
            }
            return list;
        }
    }
}
