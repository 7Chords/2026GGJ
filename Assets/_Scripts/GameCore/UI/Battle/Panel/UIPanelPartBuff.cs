using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelPartBuff : _ASCUIPanelBase<UIMonoPartBuff>
    {
        private BuffInfo _m_buffInfo;
        public UIPanelPartBuff(UIMonoPartBuff _mono, SCUIShowType _showType) : base(_mono, _showType)
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
        }

        public override void OnShowPanel()
        {
        }
        public void SetInfo(BuffInfo _buffInfo)
        {
            _m_buffInfo = _buffInfo;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_buffInfo == null)
                return;
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_buffInfo.buffRefObj.buffIconResName);
        }
    }
}
