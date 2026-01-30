using SCFrame;
using SCFrame.UI;
using System;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelCommonTwoBtn : _ASCUIPanelBase<UIMonoCommonTwoBtn>
    {
        private Action _m_onLeftBtnClick;
        private Action _m_onRightBtnClick;

        private bool _m_isSelectLeft;
        public UIPanelCommonTwoBtn(UIMonoCommonTwoBtn _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
        }

        public override void OnHidePanel()
        {
            mono.btnLeft.RemoveClickDown(onBtnLeftClickDown);
            mono.btnRight.RemoveClickDown(onBtnRightClickDown);
            mono.btnLeft.RemoveMouseEnter(onBtnLeftMouseEnter);
            mono.btnRight.RemoveMouseEnter(onBtnRightMouseEnter);
        }

        public override void OnShowPanel()
        {
            mono.btnLeft.AddMouseLeftClickDown(onBtnLeftClickDown);
            mono.btnRight.AddMouseLeftClickDown(onBtnRightClickDown);
            mono.btnLeft.AddMouseEnter(onBtnLeftMouseEnter);
            mono.btnRight.AddMouseEnter(onBtnRightMouseEnter);

            _m_isSelectLeft = true;
            refreshShow();
        }

        public void SetInfo(string _content, string _leftContent, string _rightContent, Action _onLeftBtnClick, Action _onRightBtnClick)
        {
            mono.txtContent.text = _content;
            mono.txtLeft.text = _leftContent;
            mono.txtRight.text = _rightContent;

            _m_onLeftBtnClick = _onLeftBtnClick;
            _m_onRightBtnClick = _onRightBtnClick;
        }

        private void refreshShow()
        {
            mono.btnLeft.image.color = _m_isSelectLeft ? mono.colorBtnSelect : mono.colorBtnUnselect;
            mono.txtLeft.color = _m_isSelectLeft ? mono.colorTextSelect : mono.colorTextUnselect;
            mono.btnRight.image.color = !_m_isSelectLeft ? mono.colorBtnSelect : mono.colorBtnUnselect;
            mono.txtRight.color = !_m_isSelectLeft ? mono.colorTextSelect : mono.colorTextUnselect;
        }
        private void onBtnRightClickDown(PointerEventData _data, object[] _objs)
        {
            _m_onRightBtnClick?.Invoke();
        }

        private void onBtnLeftClickDown(PointerEventData _data, object[] _objs)
        {
            _m_onLeftBtnClick?.Invoke();
        }

        private void onBtnRightMouseEnter(PointerEventData _data, object[] _objs)
        {
            _m_isSelectLeft = false;
            refreshShow();
        }

        private void onBtnLeftMouseEnter(PointerEventData _data, object[] _objs)
        {
            _m_isSelectLeft = true;
            refreshShow();
        }
    }

}
