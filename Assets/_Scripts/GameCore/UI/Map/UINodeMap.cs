using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeMap : _ASCUINodeBase
    {
        public override bool needHideWhenEnterNewSameTypeNode => true;
        public override bool needShowWhenQuitNewSameTypeNode => false;
        public override bool canQuitByEsc => false;
        public override bool canQuitByMouseRight => false;
        public override bool ignoreOnUIList => false;
        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.MAP; // 根据实际需求调整类型
        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelMap _m_mapPanel;
        private UIMonoMap _m_mapMono;

        public UINodeMap(SCUIShowType _showType) : base(_showType)
        {
        }

        public override string GetNodeName()
        {
            return nameof(UINodeMap);
        }

        public override string GetResName()
        {
            // 确保Resources文件夹下有这个预制体
            return "panel_map"; 
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_mapMono = _m_panelGO.GetComponent<UIMonoMap>();
            if (_m_mapMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_mapPanel = new UIPanelMap(_m_mapMono, _m_showType);
            _m_mapPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_mapPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_mapPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_mapPanel?.ShowPanel();
        }
    }
}
