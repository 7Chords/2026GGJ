using SCFrame.UI;

namespace GameCore.UI
{
    public class UIPanelCommonIndexItem : _ASCUIPanelBase<UIMonoCommonIndexItem>
    {
        public UIPanelCommonIndexItem(UIMonoCommonIndexItem _mono, SCUIShowType _showType) : base(_mono, _showType)
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

        public void SetSelectState(bool _isSelect)
        {
            mono.imgIndex.color = _isSelect ? mono.isCurIndex : mono.isNotCurColor;
        }
    }
}
