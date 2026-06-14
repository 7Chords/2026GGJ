using DG.Tweening;
using GameCore.Helpers;
using SCFrame;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelPartSelectItem : _ASCUIPanelBase<UIMonoPartSelectItem>
    {
        private PartInfo _m_partInfo;
        private TweenContainer _m_tweenContainer;
        private bool _m_locked;

        public System.Action<PartInfo> onSelected;

        public UIPanelPartSelectItem(UIMonoPartSelectItem mono, SCUIShowType showType) : base(mono, showType)
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
            unbindButtons();
            GetGameObject().transform.localScale = Vector3.one;
            GameCommon.DiscardToolTip();
        }

        public override void OnShowPanel()
        {
            bindButtons();
        }

        public void SetInfo(PartInfo partInfo, bool locked)
        {
            _m_partInfo = partInfo;
            _m_locked = locked;
            refreshShow();
        }

        public void SetLocked(bool locked)
        {
            _m_locked = locked;
            if (mono.btnSelect != null)
                mono.btnSelect.interactable = !locked;
        }

        private void bindButtons()
        {
            if (mono.btnSelect == null)
                return;

            mono.btnSelect.RemoveClickDown(onBtnSelectClickDown);
            mono.btnSelect.AddMouseLeftClickDown(onBtnSelectClickDown);
            mono.btnSelect.RemoveMouseEnter(onBtnSelectMouseEnter);
            mono.btnSelect.RemoveMouseExit(onBtnSelectMouseExit);
            mono.btnSelect.AddMouseEnter(onBtnSelectMouseEnter);
            mono.btnSelect.AddMouseExit(onBtnSelectMouseExit);
        }

        private void unbindButtons()
        {
            if (mono.btnSelect == null)
                return;

            mono.btnSelect.RemoveClickDown(onBtnSelectClickDown);
            mono.btnSelect.RemoveMouseEnter(onBtnSelectMouseEnter);
            mono.btnSelect.RemoveMouseExit(onBtnSelectMouseExit);
        }

        private void refreshShow()
        {
            if (_m_partInfo?.partRefObj == null)
                return;

            if (mono.imgIcon != null)
                mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);

            if (mono.txtHealth != null)
                mono.txtHealth.text = PartHealthDisplay.FormatSlashLine(_m_partInfo.currentHealth, _m_partInfo.maxHealth);

            if (mono.btnSelect != null)
                mono.btnSelect.interactable = !_m_locked;
        }

        private void onBtnSelectClickDown(PointerEventData data, object[] objs)
        {
            if (_m_locked || _m_partInfo == null)
                return;

            onSelected?.Invoke(_m_partInfo);
        }

        private void onBtnSelectMouseEnter(PointerEventData data, object[] objs)
        {
            if (_m_locked || _m_partInfo == null)
                return;

            GameCommon.ShowTooltip(_m_partInfo, GetGameObject().transform.position);
            _m_tweenContainer?.RegDoTween(
                GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnSelectMouseExit(PointerEventData data, object[] objs)
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer?.RegDoTween(
                GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}
