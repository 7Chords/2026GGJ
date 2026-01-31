using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelStoreBagItem : _ASCUIPanelBase<UIMonoStoreBagItem>
    {
        private PartInfo _m_partInfo;
        public UIPanelStoreBagItem(UIMonoStoreBagItem _mono, SCUIShowType _showType) : base(_mono, _showType)
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

        public void SetInfo(PartInfo _partInfo)
        {
            _m_partInfo = _partInfo;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            mono.txtHealth.text =_m_partInfo.partRefObj.partHealth.ToString();
        }
    }
}
