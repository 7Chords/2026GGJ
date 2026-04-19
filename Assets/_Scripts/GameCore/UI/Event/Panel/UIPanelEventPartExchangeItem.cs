using System;
using DG.Tweening;
using GameCore;
using SCFrame;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelEventPartExchangeItem : _ASCUIPanelBase<UIMonoCommonPartItem>
    {
        private PartInfo _m_partInfo;
        private Action<PartInfo> _m_onSelect;
        private TweenContainer _m_tweenContainer;

        public UIPanelEventPartExchangeItem(UIMonoCommonPartItem _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            GameCommon.DiscardToolTip();
            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
            GetGameObject().transform.RemoveClickDown(onGameObjClick);
        }

        public override void OnShowPanel()
        {
            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);
            GetGameObject().transform.AddMouseLeftClickDown(onGameObjClick);
        }

        public void SetInfo(PartInfo _partInfo, Action<PartInfo> _onSelect)
        {
            _m_partInfo = _partInfo;
            _m_onSelect = _onSelect;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            if (!mono.isTxtHealthIsRunningInfo)
                mono.txtHealth.text = _m_partInfo.maxHealth.ToString();
            else
                mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.maxHealth;
        }

        private void onGameObjClick(PointerEventData _data, object[] _objs)
        {
            if (_m_partInfo == null)
                return;
            _m_onSelect?.Invoke(_m_partInfo);
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
