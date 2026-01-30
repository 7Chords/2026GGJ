using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    /// <summary>
    /// Container基类Panel脚本
    /// </summary>
    public abstract class UIPanelContainerBase<T_MONO, T_ITEM_PANEL, T_ITEM_MONO> : _ASCUIPanelBase<T_MONO>
        where T_MONO : _ASCUIMonoBase
        where T_ITEM_PANEL : _ASCUIPanelBase<T_ITEM_MONO>
        where T_ITEM_MONO : _ASCUIMonoBase
    {
        public UIPanelContainerBase(T_MONO _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }

        protected abstract T_ITEM_PANEL creatItemPanel(T_ITEM_MONO _mono);

        protected abstract GameObject creatItemGO();

    }
}
