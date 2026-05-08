using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class PlayerInfo
    {
        public List<PartInfo> bagPartInfoList;
        public List<PartInfo> deckPartInfoList;
        public List<PartInfo> busyPartInfoList;
        public List<PartInfo> battlePartInfoList;
        /// <summary>
        /// Parts that died during the current battle. They are removed from battle execution/face,
        /// but should return to bag after the battle (full reset).
        /// </summary>
        public List<PartInfo> deadPartInfoList;

        public int currentHealth;
        public int maxHealth;
        public int playerMoney;

        public Vector2Int playerMapPosition = new Vector2Int(-1, -1);
        public Vector2Int pendingMapTargetPosition = new Vector2Int(-1, -1);
        public int playerFloor = 1;

        public PlayerInfo(PlayerRefObj _refObj)
        {
            maxHealth = _refObj.playerHealth;
            currentHealth = maxHealth;
            playerMoney = _refObj.playerMoney;

            busyPartInfoList = new List<PartInfo>();
            bagPartInfoList = new List<PartInfo>();
            deckPartInfoList = new List<PartInfo>();
            battlePartInfoList = new List<PartInfo>();
            deadPartInfoList = new List<PartInfo>();
        }

        public void SetPendingMapTarget(Vector2Int target)
        {
            pendingMapTargetPosition = target;
        }

        public void ApplyPendingMapMove()
        {
            if (pendingMapTargetPosition.x < 0 || pendingMapTargetPosition.y < 0)
                return;
            playerMapPosition = pendingMapTargetPosition;
            pendingMapTargetPosition = new Vector2Int(-1, -1);
        }

        public void ClearPendingMapMove()
        {
            pendingMapTargetPosition = new Vector2Int(-1, -1);
        }

        public int GetMapLayerXForEncounter()
        {
            if (pendingMapTargetPosition.x >= 0 && pendingMapTargetPosition.y >= 0)
                return pendingMapTargetPosition.x;
            return playerMapPosition.x;
        }

        public void ClearListForNewBattle()
        {
            if (deckPartInfoList == null) deckPartInfoList = new List<PartInfo>();
            else deckPartInfoList.Clear();

            if (busyPartInfoList == null) busyPartInfoList = new List<PartInfo>();
            else busyPartInfoList.Clear();

            if (battlePartInfoList == null) battlePartInfoList = new List<PartInfo>();
            else battlePartInfoList.Clear();

            if (deadPartInfoList == null) deadPartInfoList = new List<PartInfo>();
            else deadPartInfoList.Clear();
        }
    }
}
