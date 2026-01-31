using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeMaskCombine : _ASCUINodeBase
    {
        public override bool needHideWhenEnterNewSameTypeNode => true;

        public override bool needShowWhenQuitNewSameTypeNode => true;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.BATTLE;

        public override bool needMoveToBottomWhenHide => false;


        private GameObject _m_panelGO;
        private UIPanelMaskCombine _m_maskCombinePanel;
        private UIMonoMaskCombine _m_maskCombineMono;

        public UINodeMaskCombine(SCUIShowType _showType) : base(_showType)
        {
        }

        public override string GetNodeName()
        {
            return nameof(UINodeMaskCombine);
        }

        public override string GetResName()
        {
            return "panel_mask_combine";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_maskCombineMono = _m_panelGO.GetComponent<UIMonoMaskCombine>();
            if (_m_maskCombineMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_maskCombinePanel = new UIPanelMaskCombine(_m_maskCombineMono, _m_showType);
            _m_maskCombinePanel.Initialize();
        }

        public override void OnHideNode()
        {
            if (_m_maskCombinePanel == null)
                return;
            _m_maskCombinePanel.HidePanel();
        }

        public override void OnQuitNode()
        {
            if (_m_maskCombinePanel == null)
                return;
            _m_maskCombinePanel.Discard();
        }

        public override void OnShowNode()
        {
            if (_m_maskCombinePanel == null)
                return;
            _m_maskCombinePanel.ShowPanel();
        }
    }
}
