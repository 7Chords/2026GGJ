using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelCommonGuide : _ASCUIPanelBase<UIMonoCommonGuide>
    {
        private UIPanelCommonIndexContainer _m_indexContainer;
        private int _m_curSelectIndex;
        public UIPanelCommonGuide(UIMonoCommonGuide _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_indexContainer = new UIPanelCommonIndexContainer(mono.monoIndexContainer, SCUIShowType.INTERNAL);
        }

        public override void BeforeDiscard()
        {
            _m_indexContainer?.Discard();
        }

        public override void OnHidePanel()
        {
            mono.btnNext.RemoveClickDown(onBtnNextClickDown);
            mono.btnLast.RemoveClickDown(onBtnLastClickDown);
            mono.btnClose.RemoveClickDown(onBtnCloseClickDonw);
            _m_indexContainer?.HidePanel();
        }

        public override void OnShowPanel()
        {
            mono.btnNext.AddMouseLeftClickDown(onBtnNextClickDown);
            mono.btnLast.AddMouseLeftClickDown(onBtnLastClickDown);
            mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDonw);

            _m_indexContainer?.ShowPanel();
            _m_curSelectIndex = 0;
            _m_indexContainer.SetIndexs(mono.goGuideList.Count, _m_curSelectIndex);
            refreshShow();
        }

        private void refreshShow()
        {
            _m_indexContainer.RefreshShow(_m_curSelectIndex);
            for(int i =0;i<mono.goGuideList.Count;i++)
            {
                SCCommon.SetGameObjectEnable(mono.goGuideList[i], i == _m_curSelectIndex);
            }
        }
        private void onBtnNextClickDown(PointerEventData _data, object[] _objs)
        {
            _m_curSelectIndex = Mathf.Min(_m_curSelectIndex + 1, mono.goGuideList.Count - 1);
            refreshShow();
        }
        private void onBtnLastClickDown(PointerEventData _data, object[] _objs)
        {
            _m_curSelectIndex = Mathf.Max(_m_curSelectIndex -1 , 0);
            refreshShow();
        }

        private void onBtnCloseClickDonw(PointerEventData _data, object[] _objs)
        {
            // CloseTopNode() uses list tail as "top"; after hiding tutorial with needMoveToBottomWhenHide,
            // order can be [Guide, Map] so tail is Map and the map gets closed. ADDITION stack matches visuals.
            UICoreMgr.instance.CloseTopAdditionNode();
        }
    }
}
