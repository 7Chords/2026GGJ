using DG.Tweening;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelBookEnemyItem : _ASCUIPanelBase<UIMonoBookEnemyItem>
    {
        private EnemyRefObj _m_enemyRef;
        private TweenContainer _m_tweenContainer;

        public System.Action<EnemyRefObj> onSelected;

        public UIPanelBookEnemyItem(UIMonoBookEnemyItem mono, SCUIShowType showType) : base(mono, showType)
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

        public void SetInfo(EnemyRefObj enemyRef)
        {
            _m_enemyRef = enemyRef;
            refreshShow();
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

        private void refreshShow()
        {
            if (_m_enemyRef == null)
                return;

            if (mono.txtName != null)
                mono.txtName.text = string.IsNullOrEmpty(_m_enemyRef.enemyName) ? "" : _m_enemyRef.enemyName;

            if (mono.txtHealth != null)
                mono.txtHealth.text = PartHealthDisplay.FormatMaxOnly(_m_enemyRef.enemyHealth);

            if (mono.txtType != null)
                mono.txtType.text = _m_enemyRef.isBoss ? "\u9996\u9886" : "\u666e\u901a";
        }

        private void onBtnSelectClickDown(PointerEventData data, object[] objs)
        {
            if (_m_enemyRef == null)
                return;

            onSelected?.Invoke(_m_enemyRef);
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
