using GameCore;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelPassiveItem : _ASCUIPanelBase<UIMonoPassiveItem>
    {
        private EnemyPassiveRefObj _m_passiveRef;
        private Graphic _m_hover;

        public UIPanelPassiveItem(UIMonoPassiveItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_hover = mono.hoverTarget != null ? mono.hoverTarget : mono.GetComponent<Graphic>();
            if (_m_hover == null)
                _m_hover = mono.gameObject.GetComponentInChildren<Graphic>();
        }

        public void SetInfo(EnemyPassiveRefObj _passiveRef)
        {
            _m_passiveRef = _passiveRef;
            mono.imgPassiveIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_passiveRef.passiveIconResName);
        }

        public override void OnHidePanel()
        {
            if (_m_hover != null)
            {
                _m_hover.RemoveMouseEnter(onMouseEnter);
                _m_hover.RemoveMouseExit(onMouseExit);
            }
            GameCommon.DiscardIntroTip();
        }

        public override void OnShowPanel()
        {
            if (_m_hover != null)
            {
                _m_hover.AddMouseEnter(onMouseEnter);
                _m_hover.AddMouseExit(onMouseExit);
            }
        }

        private void onMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (_m_passiveRef == null || _m_hover == null)
                return;
            string title = EnemyPassiveIntroText.ResolveTitle(_m_passiveRef);
            string desc = EnemyPassiveIntroText.ResolveDesc(_m_passiveRef);
            GameCommon.ShowIntroTip(title, desc, _m_hover.transform.position);
        }

        private void onMouseExit(PointerEventData _data, object[] _objs)
        {
            GameCommon.DiscardIntroTip();
        }

        public override void BeforeDiscard()
        {
        }
    }
}
