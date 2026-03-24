using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
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
                GameModel.instance.playerInfo.ApplyPendingMapMove();
                UICoreMgr.instance.RemoveAllNodes(SCUINodeFuncType.BATTLE);
                UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));

            });
            _m_enemyRefObj = SCRefDataMgr.instance.enemyRefList.refDataList.Find(x => x.id == GameModel.instance.curEnemyInfo.enemyRefObj.id);
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

            List<BootyEffectObj> sourceList = _m_enemyRefObj.bootyList;
            int targetCount = _m_enemyRefObj.winCount;
            List<PartInfo> randomSelectedList = RandomSelectBooty(sourceList, targetCount);

            _m_winContainer?.SetListInfo(randomSelectedList);

            mono.txtMoney.text = _m_enemyRefObj.winMoney.ToString();

            GameModel.instance.playerInfo.bagPartInfoList.AddRange(randomSelectedList);
            GameModel.instance.playerInfo.playerMoney += _m_enemyRefObj.winMoney;
        }

        /// <summary>
        /// 从源列表中根据掉落率随机抽取指定数量的不重复元素
        /// </summary>
        /// <param name="sourceList">源列表</param>
        /// <param name="count">抽取数量</param>
        /// <returns>抽取后的列表</returns>
        private List<PartInfo> RandomSelectBooty(List<BootyEffectObj> sourceList, int count)
        {
            List<PartInfo> resultList = new List<PartInfo>();
            if (sourceList == null || sourceList.Count == 0 || count <= 0)
                return resultList;

            // 复制源列表（避免修改原数据）
            List<BootyEffectObj> tempList = new List<BootyEffectObj>(sourceList);
            // 需要抽取的数量不能超过列表总数
            int actualCount = Mathf.Min(count, tempList.Count);

            for (int i = 0; i < actualCount; i++)
            {
                if (tempList.Count == 0)
                    break;

                // 步骤1：计算所有剩余战利品的掉落率总和
                float totalChance = 0;
                foreach (var booty in tempList)
                {
                    // 确保掉落率为非负数
                    totalChance += Mathf.Max(0, booty.dropChance);
                }

                // 处理所有掉落率都为0的情况（退化为随机抽取）
                if (totalChance <= 0)
                {
                    int randomIndex = Random.Range(0, tempList.Count);
                    AddBootyToResult(tempList[randomIndex], resultList);
                    tempList.RemoveAt(randomIndex);
                }
                else
                {
                    // 步骤2：生成0到总掉落率之间的随机数
                    float randomValue = Random.Range(0, totalChance);
                    float currentChance = 0;
                    int selectedIndex = -1;

                    // 步骤3：遍历找到随机数所在的掉落率区间
                    for (int j = 0; j < tempList.Count; j++)
                    {
                        float chance = Mathf.Max(0, tempList[j].dropChance);
                        currentChance += chance;

                        if (randomValue <= currentChance)
                        {
                            selectedIndex = j;
                            break;
                        }
                    }

                    // 步骤4：添加选中的战利品到结果列表，并从临时列表移除（避免重复）
                    if (selectedIndex >= 0)
                    {
                        AddBootyToResult(tempList[selectedIndex], resultList);
                        tempList.RemoveAt(selectedIndex);
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        /// 将战利品转换为PartInfo并添加到结果列表
        /// </summary>
        private void AddBootyToResult(BootyEffectObj booty, List<PartInfo> resultList)
        {
            PartLevelRefObj partLevelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == booty.partLevelId);
            if (partLevelRefObj == null)
            {
                Debug.LogWarning($"找不到partLevelId为{booty.partLevelId}的配置数据");
                return;
            }

            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == partLevelRefObj.partId);
            if (partRefObj == null)
            {
                Debug.LogWarning($"找不到partId为{partLevelRefObj.partId}的配置数据");
                return;
            }

            resultList.Add(new PartInfo(partRefObj, false, partLevelRefObj.partLevel));
        }
    }
}