using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class PlayerInfo
    {
        public List<PartInfo> bagPartInfoList; //背包部位列表(玩家局外拥有的全部)
        public List<PartInfo> deckPartInfoList; //牌堆部位列表(在牌堆里但是玩家当前未持有的)
        public List<PartInfo> busyPartInfoList; //玩家当前持有的部位列表
        public List<PartInfo> battlePartInfoList;//当前战斗中的部位列表（在脸上）

        public int currentHealth; //玩家生命
        public int maxHealth;//玩家最大生命
        public int playerMoney;//玩家金钱

        public Vector2Int playerMapPosition = new Vector2Int(-1, -1);//玩家地图坐标位置
        public int playerFloor = 1;//玩家当前在第几个楼层

        public PlayerInfo(PlayerRefObj _refObj)
        {
            maxHealth = _refObj.playerHealth;
            currentHealth = maxHealth;
            playerMoney = _refObj.playerMoney;

            busyPartInfoList = new List<PartInfo>();
            bagPartInfoList = new List<PartInfo>();
            deckPartInfoList = new List<PartInfo>();
            battlePartInfoList = new List<PartInfo>();
        }

        public void ClearListForNewBattle()
        {
            if (deckPartInfoList == null) deckPartInfoList = new List<PartInfo>();
            else deckPartInfoList.Clear();

            if (busyPartInfoList == null) busyPartInfoList = new List<PartInfo>();
            else busyPartInfoList.Clear();

            if (battlePartInfoList == null) battlePartInfoList = new List<PartInfo>();
            else battlePartInfoList.Clear();
        }
    }
}
