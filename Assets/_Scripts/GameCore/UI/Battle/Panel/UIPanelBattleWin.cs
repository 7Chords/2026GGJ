using DG.Tweening;
using GameCore;
using GameCore.Helpers;
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
        private readonly List<UIPanelBattleWinOptionItem> _m_optionItemList = new List<UIPanelBattleWinOptionItem>();
        private readonly List<bool> _m_optionSelectedFlags = new List<bool>();
        private List<List<PartInfo>> _m_bootyOfferGroups;
        private Tween _m_moneyTween;
        private bool _m_partSelectOpen;

        public UIPanelBattleWin(UIMonoBattleWin _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
            _m_moneyTween?.Kill(false);
            _m_moneyTween = null;
            for (int i = 0; i < _m_optionItemList.Count; i++)
                _m_optionItemList[i]?.Discard();
            _m_optionItemList.Clear();
            _m_optionSelectedFlags.Clear();
            _m_bootyOfferGroups = null;
            if (UIPanelPartSelect.pendingBattleWinHost == this)
            {
                UIPanelPartSelect.pendingBattleWinHost = null;
                UIPanelPartSelect.pendingOptionIndex = -1;
            }
        }

        public override void OnHidePanel()
        {
            mono.btnGoto.onClick.RemoveAllListeners();
            for (int i = 0; i < _m_optionItemList.Count; i++)
                _m_optionItemList[i]?.HidePanel();
            _m_moneyTween?.Kill(false);
            _m_moneyTween = null;
        }

        public override void OnShowPanel()
        {
            _m_partSelectOpen = false;
            _m_bootyOfferGroups = null;
            _m_optionSelectedFlags.Clear();

            mono.btnGoto.onClick.AddListener(() =>
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                TVSwitchTransition.Run(() =>
                {
                    GameModel.instance.playerInfo.ApplyPendingMapMove();

                    bool bossWin = GameModel.instance.LastWinEnemyWasBoss;
                    int winFloor = GameModel.instance.LastWinPlayerFloor;
                    bool finalBoss = bossWin && winFloor >= GameConst.RUN_TOTAL_FLOORS;

                    GameModel.instance.ClearEnemyWinSnapshot();

                    UICoreMgr.instance.RemoveAllNodes(SCUINodeFuncType.BATTLE);

                    if (finalBoss)
                    {
                        GameRunSave.SaveFromGameModel();
                        UICoreMgr.instance.AddNode(new UINodeWin(SCUIShowType.FULL));
                    }
                    else if (bossWin)
                    {
                        AudioMgr.instance.PlayBgm("bgm_main_music");
                        GameModel.instance.AdvanceToNextRunFloorAndResetMap();
                        GameRunSave.SaveFromGameModel();
                        UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
                    }
                    else
                    {
                        AudioMgr.instance.PlayBgm("bgm_main_music");
                        UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
                    }
                });
            });

            long winEnemyId = GameModel.instance.LastWinEnemyRefId;
            _m_enemyRefObj = winEnemyId != 0
                ? SCRefDataMgr.instance.enemyRefList.refDataList.Find(x => x.id == winEnemyId)
                : null;

            refreshMoneyReward();
            refreshBootyOptions();
        }

        public List<PartInfo> GetOrRollBootyOffers(int optionIndex)
        {
            ensureBootyOfferGroups();
            if (optionIndex < 0 || optionIndex >= _m_bootyOfferGroups.Count)
                return new List<PartInfo>();

            return _m_bootyOfferGroups[optionIndex];
        }

        public void OnBootySelected(int optionIndex, PartInfo selectedPart)
        {
            _m_partSelectOpen = false;
            if (selectedPart == null)
                return;

            if (optionIndex >= 0 && optionIndex < _m_optionSelectedFlags.Count)
                _m_optionSelectedFlags[optionIndex] = true;

            if (optionIndex >= 0 && optionIndex < _m_optionItemList.Count)
                _m_optionItemList[optionIndex]?.HidePanel();
        }

        public void OnPartSelectClosed()
        {
            _m_partSelectOpen = false;
        }

        private void ensureBootyOfferGroups()
        {
            if (_m_bootyOfferGroups != null)
                return;

            int optionCount = getOptionCount();
            if (_m_enemyRefObj == null || optionCount <= 0)
            {
                _m_bootyOfferGroups = new List<List<PartInfo>>();
                return;
            }

            _m_bootyOfferGroups = BattleBootyHelper.RollDistinctBootyOfferGroups(
                _m_enemyRefObj,
                optionCount,
                BattleBootyHelper.DefaultOfferCount);
        }

        private void refreshBootyOptions()
        {
            hideExtraOptionItems();

            int optionCount = getOptionCount();
            if (optionCount <= 0)
                return;

            ensureBootyOfferGroups();
            ensureOptionSelectedFlags(optionCount);

            int shownCount = 0;
            for (int i = 0; i < optionCount; i++)
            {
                if (_m_optionSelectedFlags[i])
                {
                    if (i < _m_optionItemList.Count)
                        _m_optionItemList[i].HidePanel();
                    continue;
                }

                UIPanelBattleWinOptionItem itemPanel = getOrCreateOptionItem(i);
                if (itemPanel == null)
                    continue;

                int capturedIndex = i;
                itemPanel.onClicked = () => onOptionItemClicked(capturedIndex);
                itemPanel.ShowPanel();
                playOptionPopAnim(itemPanel, shownCount);
                shownCount++;
            }

            for (int i = optionCount; i < _m_optionItemList.Count; i++)
                _m_optionItemList[i].HidePanel();
        }

        private void ensureOptionSelectedFlags(int optionCount)
        {
            while (_m_optionSelectedFlags.Count < optionCount)
                _m_optionSelectedFlags.Add(false);
        }

        private void playOptionPopAnim(UIPanelBattleWinOptionItem itemPanel, int popIndex)
        {
            var tr = itemPanel.GetGameObject().transform;
            tr.DOKill(false);
            tr.localScale = Vector3.zero;
            float popDuration = Mathf.Max(0.01f, mono.bootyPopDuration);
            float overshoot = Mathf.Clamp(mono.bootyPopOvershoot, 0.1f, 3.5f);
            tr.DOScale(Vector3.one, popDuration)
                .SetDelay(Mathf.Max(0f, mono.bootyPopInterval) * popIndex)
                .SetEase(Ease.OutBack, overshoot);
        }

        private UIPanelBattleWinOptionItem getOrCreateOptionItem(int index)
        {
            if (index < _m_optionItemList.Count)
                return _m_optionItemList[index];

            if (mono.monoContainer == null || mono.monoContainer.layoutGroup == null)
                return null;

            GameObject itemGO = ResourcesHelper.LoadGameObject(
                mono.monoContainer.prefabItemObjName,
                mono.monoContainer.layoutGroup.transform);
            if (itemGO == null)
                return null;

            UIMonoBattleWinOptionItem itemMono = itemGO.GetComponent<UIMonoBattleWinOptionItem>();
            if (itemMono == null)
            {
                Debug.LogError("prefab missing UIMonoBattleWinOptionItem: " + mono.monoContainer.prefabItemObjName);
                return null;
            }

            var itemPanel = new UIPanelBattleWinOptionItem(itemMono, SCUIShowType.INTERNAL);
            itemPanel.Initialize();
            _m_optionItemList.Add(itemPanel);
            return itemPanel;
        }

        private void hideExtraOptionItems()
        {
            for (int i = 0; i < _m_optionItemList.Count; i++)
                _m_optionItemList[i]?.HidePanel();
        }

        private int getOptionCount()
        {
            if (!hasBootyOffer())
                return 0;

            return _m_enemyRefObj.winCount;
        }

        private bool hasBootyOffer()
        {
            if (_m_enemyRefObj == null)
                return false;
            if (_m_enemyRefObj.winCount <= 0)
                return false;

            return BattleBootyHelper.HasBootyOffers(_m_enemyRefObj);
        }

        private void onOptionItemClicked(int optionIndex)
        {
            if (_m_partSelectOpen)
                return;
            if (optionIndex < 0 || optionIndex >= _m_optionSelectedFlags.Count)
                return;
            if (_m_optionSelectedFlags[optionIndex])
                return;

            _m_partSelectOpen = true;
            AudioMgr.instance.PlaySfx("sfx_click");
            UIPanelPartSelect.pendingBattleWinHost = this;
            UIPanelPartSelect.pendingOptionIndex = optionIndex;
            UICoreMgr.instance.AddNode(new UINodePartSelect(SCUIShowType.ADDITION));
        }

        private void refreshMoneyReward()
        {
            if (_m_enemyRefObj == null)
            {
                Debug.LogWarning("UIPanelBattleWin: missing enemy ref for win reward.");
                if (mono.txtMoney != null)
                    mono.txtMoney.text = "0";
                return;
            }

            int targetMoney = _m_enemyRefObj.winMoney;
            GameModel.instance.playerInfo.playerMoney += targetMoney;

            _m_moneyTween?.Kill(false);
            _m_moneyTween = null;
            if (mono.txtMoney == null)
                return;

            mono.txtMoney.text = "0";
            float dur = Mathf.Max(0f, mono.moneyCountUpDuration);
            if (dur <= 0.0001f)
            {
                mono.txtMoney.text = targetMoney.ToString();
                return;
            }

            int cur = 0;
            _m_moneyTween = DOTween.To(() => cur, v =>
            {
                cur = v;
                mono.txtMoney.text = cur.ToString();
            }, targetMoney, dur).SetEase(Ease.OutQuad);
        }
    }
}
