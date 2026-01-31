using GameCore.RefData;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelBattleWin : _ASCUIPanelBase<UIMonoBattleWin>
    {
        private EnemyRefObj _m_enemyRefObj;
        private UIPanelStoreBagContainer _m_winContainer;

        public UIPanelBattleWin(UIMonoBattleWin _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_winContainer = new UIPanelStoreBagContainer(mono.monoContainer);
            // 初始化容器（保证容器完成初始化，避免SetListInfo失效）
            _m_winContainer.AfterInitialize();
        }

        public override void BeforeDiscard()
        {
            _m_winContainer?.Discard();
            _m_winContainer = null;
        }

        public override void OnHidePanel()
        {
            _m_winContainer?.HidePanel();
        }

        public override void OnShowPanel()
        {
            
            _m_enemyRefObj = SCRefDataMgr.instance.enemyRefList.refDataList.Find(x => x.id == GameModel.instance.rollEnemyId);

            refreshShow();
        }

        private void refreshShow()
        {
            // 边界校验：敌人数据为空直接返回
            if (_m_enemyRefObj == null)
            {
                Debug.LogWarning("UIPanelBattleWin: 敌人配置数据为空！");
                return;
            }

            // 1. 获取基础数据并校验
            List<GoodsEffectObj> sourceList = _m_enemyRefObj.initPartList;
            int targetCount = _m_enemyRefObj.winCount;

            // 2. 核心逻辑：随机抽取不重复的targetCount个元素
            List<PartInfo> randomSelectedList = RandomSelectUniqueItems(sourceList, targetCount);

            // 3. 传递抽取结果给容器，刷新UI
            _m_winContainer?.SetListInfo(randomSelectedList);
        }

        /// <summary>
        /// 从源列表中随机抽取指定数量的不重复元素
        /// </summary>
        /// <param name="sourceList">源列表</param>
        /// <param name="count">抽取数量</param>
        /// <returns>抽取后的列表</returns>
        private List<PartInfo> RandomSelectUniqueItems(List<GoodsEffectObj> sourceList, int count)
        {
            List<PartInfo> resultList = new List<PartInfo>();

            // 复制源列表（避免修改原数据）
            List<GoodsEffectObj> tempList = new List<GoodsEffectObj>(sourceList);
            // 洗牌算法（Fisher-Yates）：随机打乱后取前count个，保证随机性且不重复
            for (int i = tempList.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                // 交换元素
                (tempList[i], tempList[randomIndex]) = (tempList[randomIndex], tempList[i]);
            }

            // 取前count个元素作为结果
            for (int i = 0; i < count; i++)
            {
                PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == tempList[i].goodsId);
                resultList.Add(new PartInfo(partRefObj));
            }

            return resultList;
        }
    }
}