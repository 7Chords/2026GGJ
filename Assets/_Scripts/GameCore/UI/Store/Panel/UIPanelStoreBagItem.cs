using DG.Tweening;
using GameCore;
using GameCore.Helpers;
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

    public class UIPanelStoreBagItem : _ASCUIPanelBase<UIMonoStoreBagItem>
    {
        private PartInfo _m_partInfo;
        private TweenContainer _m_tweenContainer;

        public UIPanelStoreBagItem(UIMonoStoreBagItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();

        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.btnSell.RemoveClickDown(onBtnSellClickDown);
            mono.btnSell.RemoveMouseEnter(onBtnSellMouseEnter);
            mono.btnSell.RemoveMouseExit(onBtnSellMouseExit);
            mono.goContent.transform.RemoveMouseEnter(onGameObjMouseEnter);
            mono.goContent.transform.RemoveMouseExit(onGameObjMouseExit);
        }

        public override void OnShowPanel()
        {
            mono.btnSell.AddMouseLeftClickDown(onBtnSellClickDown);
            mono.btnSell.AddMouseEnter(onBtnSellMouseEnter);
            mono.btnSell.AddMouseExit(onBtnSellMouseExit);
            mono.goContent.transform.AddMouseEnter(onGameObjMouseEnter);
            mono.goContent.transform.AddMouseExit(onGameObjMouseExit);
        }

        public void SetInfo(PartInfo _partInfo)
        {
            _m_partInfo = _partInfo;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            mono.txtHealth.text = PartHealthDisplay.FormatMaxOnly(_m_partInfo.maxHealth);
            GoodsRefObj goodsRefObj = SCRefDataMgr.instance.goodsRefList.refDataList.Find(x => x.partId == _m_partInfo.levelRefObj.id);
            if(goodsRefObj!=null)
                mono.txtValue.text = (goodsRefObj.goodsPrice / 2).ToString();
        }

        private void onGameObjMouseExit(PointerEventData arg1, object[] arg2)
        {
            GameCommon.DiscardToolTip();
            if (mono.goContent != null)
                _m_tweenContainer.RegDoTween(mono.goContent.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onGameObjMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (_m_partInfo == null)
                return;

            GameCommon.ShowTooltip(_m_partInfo, GetGameObject().transform.position);

            if (mono.goContent != null)
                _m_tweenContainer.RegDoTween(mono.goContent.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private float sellHoverScale =>
            mono.scaleMouseEnterSell != 0f ? mono.scaleMouseEnterSell : mono.scaleMouseEnter;

        private void onBtnSellMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnSell == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSell.transform.DOScale(sellHoverScale, mono.scaleChgDuration));
        }

        private void onBtnSellMouseExit(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnSell == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSell.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onBtnSellClickDown(PointerEventData arg1, object[] arg2)
        {
            var bag = GameModel.instance.playerInfo.bagPartInfoList;
            if (bag == null || bag.Count <= 1)
            {
                GameCommon.ShowPopTip("?????????????????", Vector2.zero);
                return;
            }
            AudioMgr.instance.PlaySfx("sfx_money");
            GameCommon.DiscardToolTip();
            SCMsgCenter.SendMsg(SCMsgConst.SELL_PART, _m_partInfo);
        }
    }
}
