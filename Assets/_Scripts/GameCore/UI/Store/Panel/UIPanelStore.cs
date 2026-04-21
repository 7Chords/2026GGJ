using DG.Tweening;
using GameCore;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelStore : _ASCUIPanelBase<UIMonoStore>
    {

        private StoreRefObj _m_storeRefObj;

        private List<UIPanelStoreItem> _m_storeItemList;

        private List<GoodsInfo> _m_goodsInfoList;

        private TweenContainer _m_tweenContainer;
        public UIPanelStore(UIMonoStore _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_storeItemList = new List<UIPanelStoreItem>();
            UIPanelStoreItem item = null;
            for (int i =0;i<mono.monoItemList.Count;i++)
            {
                if (mono.monoItemList[i] == null)
                    continue;
                item = new UIPanelStoreItem(mono.monoItemList[i],SCUIShowType.INTERNAL);
                _m_storeItemList.Add(item);
            }

            _m_goodsInfoList = new List<GoodsInfo>();
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            foreach (var item in _m_storeItemList)
                item?.Discard();
            _m_goodsInfoList.Clear();
            _m_goodsInfoList = null;
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PURCHASE_GOODS, onPurchaseGoods);
            SCMsgCenter.UnregisterMsg(SCMsgConst.SELL_PART, onSellPart);
            mono.btnBag.RemoveClickDown(onBtnBagClickDown);
            mono.btnExit.RemoveClickDown(onBtnExitClickDonw);
            mono.btnBag.RemoveMouseEnter(onBtnBagMouseEnter);
            mono.btnBag.RemoveMouseExit(onBtnBagMouseExit);
            mono.btnExit.RemoveMouseEnter(onBtnExitMouseEnter);
            mono.btnExit.RemoveMouseExit(onBtnExitMouseExit);
            mono.btnGuide.RemoveClickDown(onBtnGuideClickDown);
            mono.btnGuide.RemoveMouseEnter(onBtnGuideMouseEnter);
            mono.btnGuide.RemoveMouseExit(onBtnGuideMouseExit);

            foreach (var item in _m_storeItemList)
                item?.HidePanel();
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            SCMsgCenter.RegisterMsg(SCMsgConst.PURCHASE_GOODS, onPurchaseGoods);
            SCMsgCenter.RegisterMsg(SCMsgConst.SELL_PART, onSellPart);

            mono.btnBag.AddMouseLeftClickDown(onBtnBagClickDown);
            mono.btnExit.AddMouseLeftClickDown(onBtnExitClickDonw);
            mono.btnBag.AddMouseEnter(onBtnBagMouseEnter);
            mono.btnBag.AddMouseExit(onBtnBagMouseExit);
            mono.btnExit.AddMouseEnter(onBtnExitMouseEnter);
            mono.btnExit.AddMouseExit(onBtnExitMouseExit);
            mono.btnGuide.AddMouseLeftClickDown(onBtnGuideClickDown);
            mono.btnGuide.AddMouseEnter(onBtnGuideMouseEnter);
            mono.btnGuide.AddMouseExit(onBtnGuideMouseExit);

            foreach (var item in _m_storeItemList)
                item?.ShowPanel();


            _m_storeRefObj = SCRefDataMgr.instance.storeRefList.refDataList.Find(x => x.id == GameModel.instance.rollStoreId);
            if (_m_storeRefObj == null)
                return;

            if (_m_goodsInfoList == null)
                _m_goodsInfoList = new List<GoodsInfo>();
            _m_goodsInfoList.Clear();


            GoodsEffectObj effectObj;
            for (int i = 0; i < _m_storeRefObj.goodsList.Count; i++)
            {
                effectObj = _m_storeRefObj.goodsList[i];
                if (effectObj == null)
                    continue;
                GoodsRefObj goodsRefObj = SCRefDataMgr.instance.goodsRefList.refDataList.Find(x => x.id == effectObj.goodsId);
                if (goodsRefObj == null)
                    continue;
                GoodsInfo info = new GoodsInfo(goodsRefObj, effectObj.goodsLevel);
                _m_goodsInfoList.Add(info);
            }
            refreshShow();
            if (!GameModel.instance.RunTutorialStoreAutoShown)
            {
                GameModel.instance.MarkRunTutorialStoreAutoShown();
                UICoreMgr.instance.AddNode(new UINodeGuideStore(SCUIShowType.ADDITION));
                GameRunSave.SaveFromGameModel();
            }
        }

        private void onCheatDebugUiRefresh()
        {
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_storeRefObj == null)
                return;

            for (int i = 0; i < _m_storeItemList.Count; i++)
            {
                _m_storeItemList[i].SetInfo(_m_goodsInfoList[i]);
            }
            mono.txtPlayerMoney.text = GameModel.instance.playerInfo.playerMoney.ToString();
            Tween healthTween = mono.imgHealthBar.DOFillAmount((float)GameModel.instance.playerInfo.currentHealth / GameModel.instance.playerInfo.maxHealth, mono.healthBarFadeDuration);
            _m_tweenContainer.RegDoTween(healthTween);
            mono.txtHealth.text = GameModel.instance.playerInfo.currentHealth + "/" + GameModel.instance.playerInfo.maxHealth;
        }
        private void onPurchaseGoods(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            long goodsId = (long)_objs[0];
            GoodsInfo info = _m_goodsInfoList.Find(x => x.goodsRefObj.id == goodsId);
            if (info == null)
                return;
            int boughtIndex = _m_goodsInfoList.IndexOf(info);

            info.hasBought = true;
            GameModel.instance.playerInfo.playerMoney = Mathf.Max(GameModel.instance.playerInfo.playerMoney - info.goodsRefObj.goodsPrice, 0);
            switch (info.goodsRefObj.goodsType)
            {
                case EGoodsType.PART:
                    {
                        PartLevelRefObj levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x=>x.id == info.goodsRefObj.partId);
                        PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == levelRefObj.partId);
                        GameModel.instance.playerInfo.bagPartInfoList.Add(new PartInfo(partRefObj,false, levelRefObj.partLevel));
                    }
                    break;
                case EGoodsType.HEAL:
                    {
                        GameModel.instance.PlayerHeal(info.goodsRefObj.healthValue);
                    }
                    break;
            }
            refreshShow();
            if (boughtIndex >= 0 && boughtIndex < _m_storeItemList.Count)
            {
                _m_storeItemList[boughtIndex]?.PlayPurchaseRotateEffect();
            }
        }
        private void onBtnBagClickDown(PointerEventData _arg, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeStoreBag(SCUIShowType.ADDITION));
        }
        private void onBtnGuideClickDown(PointerEventData _arg, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeGuideStore(SCUIShowType.ADDITION));
        }
        private void onBtnExitClickDonw(PointerEventData _arg, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            TVSwitchTransition.Run(() =>
            {
                GameModel.instance.playerInfo.ApplyPendingMapMove();
                UICoreMgr.instance.CloseTopNode();
                UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
            });
        }

        private void onBtnBagMouseEnter(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnBag.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnBagMouseExit(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnBag.transform.DOScale(Vector3.one, mono.scaleChgDuration));

        }

        private void onBtnExitMouseEnter(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnExit.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));

        }

        private void onBtnExitMouseExit(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnExit.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onBtnGuideMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnGuide == null) return;
            _m_tweenContainer.RegDoTween(mono.btnGuide.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnGuideMouseExit(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnGuide == null) return;
            _m_tweenContainer.RegDoTween(mono.btnGuide.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onSellPart(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;
            GoodsRefObj goodsRefObj = SCRefDataMgr.instance.goodsRefList.refDataList.Find(x => x.partId == partInfo.levelRefObj.id);
            GameModel.instance.playerInfo.playerMoney += goodsRefObj.goodsPrice / 2;
            refreshShow();
        }
    }
}
