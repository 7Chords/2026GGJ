using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;
using UnityEngine.EventSystems;
using System;

namespace GameCore.UI
{
    public class UIPanelStoreBag : _ASCUIPanelBase<UIMonoStoreBag>
    {
        private UIPanelStoreBagContainer _m_itemContainer;
        public UIPanelStoreBag(UIMonoStoreBag _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_itemContainer = new UIPanelStoreBagContainer(mono.monoContainer);
        }

        public override void BeforeDiscard()
        {
            _m_itemContainer?.Discard();
            _m_itemContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.btnClose.RemoveClickDown(onBtnCloseClickDown);

            _m_itemContainer?.HidePanel();
        }


        public override void OnShowPanel()
        {
            mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            _m_itemContainer?.ShowPanel();
            _m_itemContainer?.SetListInfo(GameModel.instance.busyPartInfoList);
        }

        private void onBtnCloseClickDown(PointerEventData _arg, object[] _objs)
        {
            UICoreMgr.instance.CloseTopNode();
        }
    }
}
