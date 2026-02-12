using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.RefData;
using GameCore;
using SCFrame;

namespace GameCore.UI
{
    public class UIPanelBattle : _ASCUIPanelBase<UIMonoBattle>
    {
        private UIPanelBattleFace _m_playerBattleFace;
        private UIPanelBattleFace _m_enemyBattleFace;

        public UIPanelBattle(UIMonoBattle _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_playerBattleFace = new UIPanelBattleFace(mono.monoPlayerFace,SCUIShowType.INTERNAL);
            _m_enemyBattleFace = new UIPanelBattleFace(mono.monoEnemyFace, SCUIShowType.INTERNAL);

        }

        public override void BeforeDiscard()
        {
            _m_playerBattleFace?.Discard();
            _m_enemyBattleFace?.Discard();

        }

        public override void OnHidePanel()
        {
            _m_playerBattleFace?.HidePanel();
            _m_enemyBattleFace?.HidePanel();
        }

        public override void OnShowPanel()
        {
            _m_playerBattleFace?.ShowPanel();
            _m_enemyBattleFace?.ShowPanel();
        }
    }
}
