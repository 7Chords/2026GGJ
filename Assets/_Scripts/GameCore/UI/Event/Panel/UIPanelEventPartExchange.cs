using GameCore;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelEventPartExchange : _ASCUIPanelBase<UIMonoEventPartExchange>
    {
        private UIPanelEventPartExchangeContainer _m_container;

        public UIPanelEventPartExchange(UIMonoEventPartExchange _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_container = new UIPanelEventPartExchangeContainer(mono.monoContainer, SCUIShowType.INTERNAL);
        }

        public override void BeforeDiscard()
        {
            _m_container?.Discard();
            _m_container = null;
        }

        public override void OnHidePanel()
        {
            GameCommon.DiscardToolTip();
            mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            _m_container?.HidePanel();
        }

        public override void OnShowPanel()
        {
            mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            _m_container?.ShowPanel();
            refreshList();
        }

        private void refreshList()
        {
            List<PartInfo> bag = GameModel.instance.playerInfo.bagPartInfoList;
            _m_container?.SetListInfo(bag, onSelectPart);
        }

        private void closeExchangeNode()
        {
            // CloseTopNode only hides ADDITION nodes but leaves them on the stack top, so the event UI stays blocked.
            UICoreMgr.instance.RemoveNode(nameof(UINodeEventPartExchange));
        }

        private void onSelectPart(PartInfo _part)
        {
            if (_part == null)
                return;
            if (!EventPartExchangeHelper.TryExecute(_part))
                return;
            SCMsgCenter.SendMsg(SCMsgConst.EVENT_PART_EXCHANGE_COMPLETED);
            closeExchangeNode();
        }

        private void onBtnCloseClickDown(PointerEventData _arg, object[] _objs)
        {
            closeExchangeNode();
        }
    }
}
