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
                TVSwitchTransition.Run(() =>
                {
                    GameModel.instance.playerInfo.ApplyPendingMapMove();
                    UICoreMgr.instance.RemoveAllNodes(SCUINodeFuncType.BATTLE);
                    UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
                });
            });
            _m_enemyRefObj = SCRefDataMgr.instance.enemyRefList.refDataList.Find(x => x.id == GameModel.instance.curEnemyInfo.enemyRefObj.id);
            _m_winContainer?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_enemyRefObj == null)
            {
                Debug.LogWarning("UIPanelBattleWin: ????????????????");
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

        private List<PartInfo> RandomSelectBooty(List<BootyEffectObj> sourceList, int count)
        {
            List<PartInfo> resultList = new List<PartInfo>();
            if (sourceList == null || sourceList.Count == 0 || count <= 0)
                return resultList;

            List<BootyEffectObj> tempList = new List<BootyEffectObj>(sourceList);
            int actualCount = Mathf.Min(count, tempList.Count);

            for (int i = 0; i < actualCount; i++)
            {
                if (tempList.Count == 0)
                    break;

                float totalChance = 0;
                foreach (var booty in tempList)
                {
                    totalChance += Mathf.Max(0, booty.dropChance);
                }

                if (totalChance <= 0)
                {
                    int randomIndex = Random.Range(0, tempList.Count);
                    AddBootyToResult(tempList[randomIndex], resultList);
                    tempList.RemoveAt(randomIndex);
                }
                else
                {
                    float randomValue = Random.Range(0, totalChance);
                    float currentChance = 0;
                    int selectedIndex = -1;

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
                    if (selectedIndex >= 0)
                    {
                        AddBootyToResult(tempList[selectedIndex], resultList);
                        tempList.RemoveAt(selectedIndex);
                    }
                }
            }

            return resultList;
        }

        private void AddBootyToResult(BootyEffectObj booty, List<PartInfo> resultList)
        {
            PartLevelRefObj partLevelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == booty.partLevelId);
            if (partLevelRefObj == null)
                return;

            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == partLevelRefObj.partId);
            if (partRefObj == null)
                return;

            resultList.Add(new PartInfo(partRefObj, false, partLevelRefObj.partLevel));
        }
    }
}