using DG.Tweening;
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
            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
        }

        public override void OnShowPanel()
        {
            mono.btnSell.AddMouseLeftClickDown(onBtnSellClickDown);
            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);

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
            mono.txtHealth.text = _m_partInfo.partRefObj.partHealth.ToString();
            GoodsRefObj goodsRefObj = SCRefDataMgr.instance.goodsRefList.refDataList.Find(x => x.partId == _m_partInfo.partRefObj.id);
            if(goodsRefObj!=null)
                mono.txtValue.text = (goodsRefObj.goodsPrice / 2).ToString();
        }

        private void onGameObjMouseExit(PointerEventData arg1, object[] arg2)
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onGameObjMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (_m_partInfo == null)
                return;


            GameCommon.ShowTooltip(_m_partInfo, GetGameObject().transform.position);

            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));

        }

        private void onBtnSellClickDown(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_money");
            GameCommon.DiscardToolTip();
            SCMsgCenter.SendMsg(SCMsgConst.SELL_PART, _m_partInfo);
        }
    }
}
