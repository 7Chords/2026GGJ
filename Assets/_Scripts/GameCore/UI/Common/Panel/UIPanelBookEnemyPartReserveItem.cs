using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelBookEnemyPartReserveItem : _ASCUIPanelBase<UIMonoBookEnemyPartReserveItem>
    {
        public UIPanelBookEnemyPartReserveItem(UIMonoBookEnemyPartReserveItem mono, SCUIShowType showType) : base(mono, showType)
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

        public void SetInfo(string partName, int count)
        {
            if (mono.txtContent == null)
                return;

            string name = string.IsNullOrEmpty(partName) ? "" : partName;
            mono.txtContent.text = name + "\u00d7" + Mathf.Max(0, count);
        }
    }
}
