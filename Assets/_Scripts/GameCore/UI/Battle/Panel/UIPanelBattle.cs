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
            if (mono != null) mono.onUpdate = null;
            if (_playerFacePanel != null) _playerFacePanel.OnHidePanel();
            if (_enemyFacePanel != null) _enemyFacePanel.OnHidePanel();
        }

        public override void OnShowPanel()
        {
            Debug.Log("[UIPanelBattle] OnShowPanel");
            {
                mono.gameObject.SetActive(true); // Ensure active
            }
            
            // Subscribe to Update
            if (mono != null) mono.onUpdate = OnMonoUpdate;
            
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
            
            // 3. Start Battle Logic
            // StartBattleTurn(); 
            _pendingBattleStart = true;
        }
        
        private bool _pendingBattleStart = false;

        private void OnMonoUpdate()
        {
            if (_pendingBattleStart)
            {
                if (mono != null && mono.gameObject.activeInHierarchy)
                {
                    _pendingBattleStart = false;
                    StartBattleTurn();
                }
            }
        }
        
        private void StartBattleTurn()
        {
            if (_battleCoroutine != null) mono.StopCoroutine(_battleCoroutine);
            
            if (mono.gameObject.activeInHierarchy)
            {
                _battleCoroutine = mono.StartCoroutine(BattleTurnLoop());
            }
            else
            {
                Debug.LogWarning("[UIPanelBattle] Cannot start BattleTurnLoop because GameObject is inactive. Waiting for activation...");
                // Optional: Try to start it in Update or OnEnable if possible, but for now just Warn.
                // Or force active? We did logic above.
                mono.gameObject.SetActive(true);
                if (mono.gameObject.activeInHierarchy)
                {
                     _battleCoroutine = mono.StartCoroutine(BattleTurnLoop());
                }
            }
        }
        
        private Coroutine _battleCoroutine;
        
        private IEnumerator BattleTurnLoop()
        {
            Debug.Log("[Battle] Turn Start...");
            yield return new WaitForSeconds(0.5f); // Initial Delay
            
            // 1. Get and Sort Parts
            var playerParts = new List<PartInfo>(GameModel.instance.playerBattleParts);
            var enemyParts = (GameModel.instance.currentEnemy != null && GameModel.instance.currentEnemy.parts != null) 
                             ? new List<PartInfo>(GameModel.instance.currentEnemy.parts) 
                             : new List<PartInfo>();
            
            SortParts(playerParts);
            SortParts(enemyParts);
            
            // 2. Interleaved Execution
            int pIndex = 0;
            int eIndex = 0;
            int maxCount = Mathf.Max(playerParts.Count, enemyParts.Count);
            
            // Execution Queue: Player 1 -> Enemy 1 -> Player 2 -> Enemy 2 ...
            // As per user request: "Execute one on both sides simultaneously, then execute one on both sides simultaneously"
            // Implementation: Run Logic for P[i] and E[i] "simultaneously" (in same frame or close succession), then wait.
            
            for (int i = 0; i < maxCount; i++)
            {
                // Player Action
                if (i < playerParts.Count)
                {
                    var pPart = playerParts[i];
                    Debug.Log($"[Battle] Player Part Turn: {pPart.partRefObj.partName}");
                    if (pPart.logicObj != null) 
                    {
                        pPart.logicObj.OnTurnStart(); 
                    }
                }
                
                // Enemy Action
                if (i < enemyParts.Count)
                {
                    var ePart = enemyParts[i];
                    Debug.Log($"[Battle] Enemy Part Turn: {ePart.partRefObj.partName}");
                    if (ePart.logicObj != null) 
                    {
                        ePart.logicObj.OnTurnStart();
                    }
                }
                
                // Wait for turn completion (Animation or just intervals)
                yield return new WaitForSeconds(1.0f); 
            }
            
            Debug.Log("[Battle] All Parts Executed.");
        }
        
        private void SortParts(List<PartInfo> list)
        {
             // Rule: Top-to-Bottom (Max Y Descending), then Left-to-Right (Min X Ascending)
             // Grid: Y=0 is Bottom, Y=Max is Top. So Max Y Descending is Correct.
             // Grid: X=0 is Left. So Min X Ascending is Correct.
             
             list.Sort((a, b) => {
                 // Sort by Y descending
                 int ret = a.gridPos.y.CompareTo(b.gridPos.y); 
                 if (ret != 0) return ret;
                 // Sort by X ascending
                 return b.gridPos.x.CompareTo(a.gridPos.x);
             });
        }
    }
}
