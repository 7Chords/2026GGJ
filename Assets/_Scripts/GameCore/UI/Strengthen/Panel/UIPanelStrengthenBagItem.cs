using DG.Tweening;
using GameCore.Helpers;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelStrengthenBagItem : _ASCUIPanelBase<UIMonoStrengthenBagItem>
    {
        private PartInfo _m_partInfo;
        private TweenContainer _m_tweenContainer;
        private bool _m_hasSelected;
        public UIPanelStrengthenBagItem(UIMonoStrengthenBagItem _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.btnItem.AddMouseLeftClickDown(onBtnItemClickDown);
            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
        }

        public override void OnShowPanel()
        {
            mono.btnItem.AddMouseLeftClickDown(onBtnItemClickDown);
            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);
        }
        public void SetInfo(PartInfo _partInfo,bool _hasSelected)
        {
            _m_partInfo = _partInfo;
            _m_hasSelected = _hasSelected;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            mono.txtHealth.text = PartHealthDisplay.FormatSlashLine(_m_partInfo.currentHealth, _m_partInfo.maxHealth);

            SCCommon.SetGameObjectEnable(mono.goHasSelectedShowList, _m_hasSelected);
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

        private void onBtnItemClickDown(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            SCMsgCenter.SendMsg(SCMsgConst.SELECT_STRENGTHEN_PART, _m_partInfo);
        }
    }
}
