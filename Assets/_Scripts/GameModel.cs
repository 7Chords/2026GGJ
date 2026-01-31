using GameCore.RefData;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 游戏模型 放所有的运行时数据
    /// </summary>
    public class GameModel : Singleton<GameModel>
    {
        public List<PartInfo> bagPartInfoList;//背包部位列表(玩家局外拥有的全部)
        public List<PartInfo> deckPartInfoList;//牌堆部位列表(在牌堆里但是玩家当前未持有的)
        public List<PartInfo> busyPartInfoList;//玩家当前持有的部位列表


        public int playerHealth;//玩家生命
        public int playerMaxHealth;
        public int playerMoney;

        public List<FacePartInfo> facePartInfoList;//脸部装备的部位列表
        public List<FaceGridInfo> faceGridInfoList;//脸部格子信息列表

        public long rollStoreId;//进入商店节点后roll到的商店id
        public long rollEnemyId;//进入战斗节点后roll到的敌人id
        public override void OnInitialize()
        {
            //初始化数据从配表读取
            PlayerRefObj playerRefObj = SCRefDataMgr.instance.playerConfigRefObj;
            if (playerRefObj == null)
                return;
            playerMaxHealth = playerRefObj.playerHealth;
            playerHealth = playerMaxHealth;
            playerMoney = playerRefObj.playerMoney;

            busyPartInfoList = new List<PartInfo>();
            bagPartInfoList = new List<PartInfo>();
            deckPartInfoList = new List<PartInfo>();

            PartEffectObj partEffectObj = null;
            PartInfo info = null;
            PartRefObj partRefObj = null;
            for (int i =0;i<playerRefObj.initPartList.Count;i++)
            {
                partEffectObj = playerRefObj.initPartList[i];
                if (partEffectObj == null)
                    continue;
                partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == partEffectObj.partId);
                if (partRefObj == null)
                    continue;
                info = new PartInfo(partRefObj);
                busyPartInfoList.Add(info);
                bagPartInfoList.Add(info);
                deckPartInfoList.Add(info);

            }
        }

        public void Heal(int _amount)
        {
            playerHealth = Mathf.Clamp(playerHealth + _amount, 0, playerMaxHealth);
        }

    }
}
