using DG.Tweening;
using SCFrame;
using SCFrame.UI;

namespace GameCore.UI
{
    public class UIPanelBattleOrder : _ASCUIPanelBase<UIMonoBattleOrder>
    {
        private TweenContainer _m_tweenContainer;
        public UIPanelBattleOrder(UIMonoBattleOrder _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
            _m_tweenContainer?.RegDoTween(DOVirtual.DelayedCall(SCSettingMgr.instance.ScaleBattleDuration(mono.showDuration), () =>
            {
                UICoreMgr.instance.CloseTopNode();
            }));
            refreshShow();
        }

        private void refreshShow()
        {
            mono.txtOrder.text = GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER ? "我方先手" : "敌方先手";
            SCCommon.SetGameObjectEnable(mono.goIsPlayerFirstShow, GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER);
            SCCommon.SetGameObjectEnable(mono.goIsEnemyFirstShow, GameModel.instance.curTurnOwner == ETurnOwnerType.ENEMY);
        }
    }
}
