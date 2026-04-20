using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeSetting : _ASCUINodeBase
    {
        public UINodeSetting(SCUIShowType _showType,bool _needShowReturnBtn) : base(_showType)
        {
            _m_needShowReturnBtn = _needShowReturnBtn;
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => true;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.COMMON;

        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelSetting _m_settingPanel;
        private UIMonoSetting _m_settingMono;

        private bool _m_needShowReturnBtn;
        public override string GetNodeName()
        {
            return nameof(UINodeSetting);
        }

        public override string GetResName()
        {
            return "panel_setting";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_settingMono = _m_panelGO.GetComponent<UIMonoSetting>();
            if (_m_settingMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_settingPanel = new UIPanelSetting(_m_settingMono, _m_showType);
            _m_settingPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_settingPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_settingPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_settingPanel?.ShowPanel();
            _m_settingPanel.SetReturnBtnShowState(_m_needShowReturnBtn);
        }

        public override void CopyData(_ASCUINodeBase _anotherNode)
        {
            _m_needShowReturnBtn = (_anotherNode as UINodeSetting)._m_needShowReturnBtn;
        }
    }
}
