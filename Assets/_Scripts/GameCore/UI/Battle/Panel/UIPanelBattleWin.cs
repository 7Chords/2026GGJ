using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelBattleWin : _ASCUIPanelBase<UIMonoBattleWin>
    {
        private EnemyRefObj _m_enemyRefObj;
        private UIPanelCommonPartContainer _m_winContainer;

        public UIPanelBattleWin(UIMonoBattleWin _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_winContainer = new UIPanelCommonPartContainer(mono.monoContainer);
        }

        public override void BeforeDiscard()
        {
            _m_winContainer?.Discard();
            _m_winContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.btnGoto.onClick.RemoveAllListeners();
            _m_winContainer?.HidePanel();
        }

        public override void OnShowPanel()
        {
            mono.btnGoto.onClick.AddListener(() =>
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                UICoreMgr.instance.RemoveAllNodes(SCUINodeFuncType.BATTLE);
                UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));

            });
            _m_enemyRefObj = SCRefDataMgr.instance.enemyRefList.refDataList.Find(x => x.id == GameModel.instance.currentEnemy.enemyRef.id);
            _m_winContainer?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_enemyRefObj == null)
            {
                Debug.LogWarning("UIPanelBattleWin: 敌人配置数据为空！");
                return;
            }

            List<PartEffectObj> sourceList = _m_enemyRefObj.initPartList;
            int targetCount = _m_enemyRefObj.winCount;
            List<PartInfo> randomSelectedList = RandomSelectUniqueItems(sourceList, targetCount);

            _m_winContainer?.SetListInfo(randomSelectedList);

            mono.txtMoney.text = _m_enemyRefObj.winMoney.ToString();

            GameModel.instance.bagPartInfoList.AddRange(randomSelectedList);
            GameModel.instance.playerMoney += _m_enemyRefObj.winMoney;
        }

        /// <summary>
        /// 从源列表中随机抽取指定数量的不重复元素
        /// </summary>
        /// <param name="sourceList">源列表</param>
        /// <param name="count">抽取数量</param>
        /// <returns>抽取后的列表</returns>
        private List<PartInfo> RandomSelectUniqueItems(List<PartEffectObj> sourceList, int count)
        {
            List<PartInfo> resultList = new List<PartInfo>();

            // 复制源列表（避免修改原数据）
            List<PartEffectObj> tempList = new List<PartEffectObj>(sourceList);
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
                PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == tempList[i].partId);
                resultList.Add(new PartInfo(partRefObj));
            }

            return resultList;
        }
    }
}