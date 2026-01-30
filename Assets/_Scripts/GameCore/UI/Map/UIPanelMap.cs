using SCFrame.UI;

namespace GameCore.UI
{
    public class UIPanelMap : _ASCUIPanelBase<UIMonoMap>
    {
        public UIPanelMap(UIMonoMap _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void OnShowPanel()
        {

        }

        public override void OnHidePanel()
        {
        }

        public override void BeforeInitialize()
        {
            base.BeforeInitialize();
        }
        
        public override void AfterInitialize()
        {
            
        }
        
        public override void BeforeDiscard()
        {
            
        }

        public override void AfterDiscard()
        {
            base.AfterDiscard();
        }
    }
}
