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
        private UIPanelBattleWinOptionItem _m_optionItem;
        private List<PartInfo> _m_bootyOffers;
        private Tween _m_moneyTween;
        private bool _m_bootySelected;
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
            _m_optionItem?.Discard();
            _m_optionItem = null;
            _m_bootyOffers = null;
            if (UIPanelPartSelect.pendingBattleWinHost == this)
                UIPanelPartSelect.pendingBattleWinHost = null;
        }

        public override void OnHidePanel()
        {
            mono.btnGoto.onClick.RemoveAllListeners();
            _m_optionItem?.HidePanel();
            _m_moneyTween?.Kill(false);
            _m_moneyTween = null;
        }

        public override void OnShowPanel()
        {
            _m_bootySelected = false;
            _m_partSelectOpen = false;
            _m_bootyOffers = null;

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
            refreshBootyOption();
        }

        public List<PartInfo> GetOrRollBootyOffers()
        {
            if (_m_bootyOffers != null)
                return _m_bootyOffers;

            if (_m_enemyRefObj == null)
                _m_bootyOffers = new List<PartInfo>();
            else
                _m_bootyOffers = BattleBootyHelper.RollBootyOffers(_m_enemyRefObj, BattleBootyHelper.DefaultOfferCount);

            return _m_bootyOffers;
        }

        public void OnBootySelected(PartInfo selectedPart)
        {
            _m_partSelectOpen = false;
            if (selectedPart == null)
                return;

            _m_bootySelected = true;
            _m_optionItem?.HidePanel();
        }

        public void OnPartSelectClosed()
        {
            _m_partSelectOpen = false;
        }

        private void refreshBootyOption()
        {
            if (_m_bootySelected || !hasBootyOffer())
            {
                _m_optionItem?.HidePanel();
                return;
            }

            ensureOptionItem();
            if (_m_optionItem == null)
                return;

            _m_optionItem.onClicked = onOptionItemClicked;
            _m_optionItem.ShowPanel();

            var tr = _m_optionItem.GetGameObject().transform;
            tr.DOKill(false);
            tr.localScale = Vector3.zero;
            float popDuration = Mathf.Max(0.01f, mono.bootyPopDuration);
            float overshoot = Mathf.Clamp(mono.bootyPopOvershoot, 0.1f, 3.5f);
            tr.DOScale(Vector3.one, popDuration)
                .SetDelay(Mathf.Max(0f, mono.bootyPopInterval))
                .SetEase(Ease.OutBack, overshoot);
        }

        private void ensureOptionItem()
        {
            if (_m_optionItem != null)
                return;

            if (mono.monoContainer == null || mono.monoContainer.layoutGroup == null)
                return;

            GameObject itemGO = ResourcesHelper.LoadGameObject(
                mono.monoContainer.prefabItemObjName,
                mono.monoContainer.layoutGroup.transform);
            if (itemGO == null)
                return;

            UIMonoBattleWinOptionItem itemMono = itemGO.GetComponent<UIMonoBattleWinOptionItem>();
            if (itemMono == null)
            {
                Debug.LogError("prefab missing UIMonoBattleWinOptionItem: " + mono.monoContainer.prefabItemObjName);
                return;
            }

            _m_optionItem = new UIPanelBattleWinOptionItem(itemMono, SCUIShowType.INTERNAL);
            _m_optionItem.Initialize();
        }

        private bool hasBootyOffer()
        {
            if (_m_enemyRefObj == null)
                return false;
            if (_m_enemyRefObj.winCount <= 0)
                return false;

            return BattleBootyHelper.HasBootyOffers(_m_enemyRefObj);
        }

        private void onOptionItemClicked()
        {
            if (_m_bootySelected || _m_partSelectOpen)
                return;

            _m_partSelectOpen = true;
            AudioMgr.instance.PlaySfx("sfx_click");
            UIPanelPartSelect.pendingBattleWinHost = this;
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
