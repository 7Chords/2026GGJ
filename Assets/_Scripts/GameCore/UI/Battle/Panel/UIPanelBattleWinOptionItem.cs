using DG.Tweening;
using SCFrame;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelBattleWinOptionItem : _ASCUIPanelBase<UIMonoBattleWinOptionItem>
    {
        private TweenContainer _m_tweenContainer;

        public System.Action onClicked;

        public UIPanelBattleWinOptionItem(UIMonoBattleWinOptionItem mono, SCUIShowType showType) : base(mono, showType)
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
        }

        public override void OnShowPanel()
        {
            bindButtons();
        }

        private void bindButtons()
        {
            if (mono.btnSelect == null)
                return;

            mono.btnSelect.RemoveClickDown(onBtnSelectClickDown);
            mono.btnSelect.RemoveMouseEnter(onBtnSelectMouseEnter);
            mono.btnSelect.RemoveMouseExit(onBtnSelectMouseExit);
            mono.btnSelect.AddMouseLeftClickDown(onBtnSelectClickDown);
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

        private void onBtnSelectClickDown(PointerEventData data, object[] objs)
        {
            onClicked?.Invoke();
        }

        private void onBtnSelectMouseEnter(PointerEventData data, object[] objs)
        {
            _m_tweenContainer?.RegDoTween(
                GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnSelectMouseExit(PointerEventData data, object[] objs)
        {
            _m_tweenContainer?.RegDoTween(
                GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}
