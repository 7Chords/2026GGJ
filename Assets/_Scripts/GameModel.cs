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
        public List<PartInfo> deckPartInfoList;//牌堆部位列表
        public int playerHealth;//玩家生命
        public List<FacePartInfo> facePartInfoList;//脸部装备的部位列表
        public List<PartInfo> bagPartInfoList;//背包部位列表
        public List<FaceGridInfo> faceGridInfoList;//脸部格子信息列表
        public override void OnInitialize()
        {
            //初始化数据从配表读取
            PlayerRefObj playerRefObj = SCRefDataMgr.instance.playerConfigRefObj;
            if (playerRefObj == null)
                return;
            playerHealth = playerRefObj.playerHealth;
            deckPartInfoList = new List<PartInfo>();
            GoodsEffectObj goodsEffectObj = null;
            PartInfo info = null;
            PartRefObj partRefObj = null;
            for (int i =0;i<playerRefObj.initPartList.Count;i++)
            {
                goodsEffectObj = playerRefObj.initPartList[i];
                if (goodsEffectObj == null)
                    continue;
                partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == goodsEffectObj.goodsId);
                if (partRefObj == null)
                    continue;
                info = new PartInfo(partRefObj);
                deckPartInfoList.Add(info);
            }
        }

    }
}
