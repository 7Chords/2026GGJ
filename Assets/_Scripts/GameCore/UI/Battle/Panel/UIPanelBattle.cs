using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.RefData;

namespace GameCore.UI
{
    public class UIPanelBattle : _ASCUIPanelBase<UIMonoBattle>
    {
        private UIPanelMaskCombineFace _playerFacePanel;
        private UIPanelMaskCombineFace _enemyFacePanel;

        public UIPanelBattle(UIMonoBattle _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            // Initialize Player Face Panel logic
            if (mono.playerFace != null)
            {
                _playerFacePanel = new UIPanelMaskCombineFace(mono.playerFace, showType);
                _playerFacePanel.AfterInitialize();
            }

            // Initialize Enemy Face Panel logic
            if (mono.enemyFace != null)
            {
                _enemyFacePanel = new UIPanelMaskCombineFace(mono.enemyFace, showType);
                _enemyFacePanel.AfterInitialize();
            }
        }

        public override void BeforeDiscard()
        {
            if (_playerFacePanel != null) _playerFacePanel.BeforeDiscard();
            if (_enemyFacePanel != null) _enemyFacePanel.BeforeDiscard();
        }

        public override void OnHidePanel()
        {
            if (_playerFacePanel != null) _playerFacePanel.OnHidePanel();
            if (_enemyFacePanel != null) _enemyFacePanel.OnHidePanel();
        }

        public override void OnShowPanel()
        {
            Debug.Log("[UIPanelBattle] OnShowPanel");
            if (_playerFacePanel != null) _playerFacePanel.OnShowPanel();
            if (_enemyFacePanel != null) _enemyFacePanel.OnShowPanel();

            RefreshBattle();
        }

        private void RefreshBattle()
        {
            Debug.Log("[UIPanelBattle] RefreshBattle");
            // 1. Setup HP Sliders
            if (mono.sliderPlayerHp != null)
            {
                mono.sliderPlayerHp.maxValue = GameModel.instance.playerMaxHealth;
                mono.sliderPlayerHp.value = GameModel.instance.playerHealth;
            }
            if (mono.sliderEnemyHp != null && GameModel.instance.currentEnemy != null)
            {
                mono.sliderEnemyHp.maxValue = GameModel.instance.currentEnemy.maxHealth;
                mono.sliderEnemyHp.value = GameModel.instance.currentEnemy.currentHealth;
            }

            // 2. Setup Faces logic
            if (_playerFacePanel != null)
            {
                var pParts = GameModel.instance.playerBattleParts;
                Debug.Log($"[UIPanelBattle] Player Parts Count: {(pParts!=null?pParts.Count:0)}");
                if (pParts != null) _playerFacePanel.Setup(pParts);
            }
            else
            {
                Debug.LogError("[UIPanelBattle] _playerFacePanel is null! Check UIMonoBattle.playerFace assignment.");
            }
            
            if (_enemyFacePanel != null)
            {
                var eParts = GameModel.instance.currentEnemy != null ? GameModel.instance.currentEnemy.parts : null;
                Debug.Log($"[UIPanelBattle] Enemy Parts Count: {(eParts!=null?eParts.Count:0)}");
                if (eParts != null) _enemyFacePanel.Setup(eParts);
            }
            else
            {
                 Debug.LogError("[UIPanelBattle] _enemyFacePanel is null! Check UIMonoBattle.enemyFace assignment.");
            }
        }
    }
}
