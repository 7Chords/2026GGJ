using DG.Tweening;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    /// <summary>
    /// ͨ�õĲ�λitempanel
    /// </summary>
    public class UIPanelCommonPartItem : _ASCUIPanelBase<UIMonoCommonPartItem>
    {
        private PartInfo _m_partInfo;
        private TweenContainer _m_tweenContainer;

        public UIPanelCommonPartItem(UIMonoCommonPartItem _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
        }

        public override void OnShowPanel()
        {
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
            if(!mono.isTxtHealthIsRunningInfo)
                mono.txtHealth.text =_m_partInfo.partRefObj.partHealth.ToString();
            else
                mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.maxHealth;

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
    }
}
