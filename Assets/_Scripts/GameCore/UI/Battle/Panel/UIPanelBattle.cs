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
            if (mono.imgHealthBar_player != null)
            {
                mono.imgHealthBar_player.fillAmount = (float)GameModel.instance.playerHealth / GameModel.instance.playerMaxHealth;
                mono.txtHealth_player.text = GameModel.instance.playerHealth + "/" + GameModel.instance.playerMaxHealth;
            }
            if (mono.imgHealthBar_enemy != null)
            {
                mono.imgHealthBar_enemy.fillAmount = (float)GameModel.instance.currentEnemy.currentHealth / GameModel.instance.currentEnemy.maxHealth;
                mono.txtHealth_enemy.text = GameModel.instance.currentEnemy.currentHealth + "/" + GameModel.instance.currentEnemy.maxHealth;
            }

            // 2. Setup Faces logic
            if (_playerFacePanel != null)
            {
                var pParts = GameModel.instance.playerBattleParts;
                Debug.Log($"[UIPanelBattle] Player Parts Count: {(pParts!=null?pParts.Count:0)}");
                if (pParts != null) _playerFacePanel.Setup(pParts,true);
            }
            else
            {
                Debug.LogError("[UIPanelBattle] _playerFacePanel is null! Check UIMonoBattle.playerFace assignment.");
            }
            
            if (_enemyFacePanel != null)
            {
                var eParts = GameModel.instance.currentEnemy != null ? GameModel.instance.currentEnemy.parts : null;
                Debug.Log($"[UIPanelBattle] Enemy Parts Count: {(eParts!=null?eParts.Count:0)}");
                if (eParts != null) _enemyFacePanel.Setup(eParts, true);
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
            UpdateBattleUI(); 
            
            // 0. Check Battle End Condition (Start of Turn Check)
            if (CheckBattleEnd()) yield break;

            yield return new WaitForSeconds(0.5f);
            
            // 1. Get and Sort Parts
            var playerParts = new List<PartInfo>();
            if(GameModel.instance.playerBattleParts != null) 
                playerParts.AddRange(GameModel.instance.playerBattleParts);
                
            var enemyParts = new List<PartInfo>();
            if(GameModel.instance.currentEnemy != null && GameModel.instance.currentEnemy.parts != null) 
                enemyParts.AddRange(GameModel.instance.currentEnemy.parts);
            
            SortParts(playerParts);
            SortParts(enemyParts);
            
            // Global Buffs Calculation
            // "CRITICAL_CHANCE and HIT_CHANCE apply to all parts with ATTACK"
            // Wait, calculate total available from ALL parts (Sum of all Crit/Hit entries?)
            // Or does each part contribute? "Parts with attributes... boost their values"
            // Re-reading: "CRITICAL_CHANCE... act on all parts with ATTACK" -> Likely Sum of all Crit/Hit found in the team.
            float pGlobalCrit = CalculateGlobalAttribute(playerParts, EAttributeType.CRITICAL_CHANCE);
            float pGlobalHit = CalculateGlobalAttribute(playerParts, EAttributeType.HIT_CHANCE);
            float eGlobalCrit = CalculateGlobalAttribute(enemyParts, EAttributeType.CRITICAL_CHANCE);
            float eGlobalHit = CalculateGlobalAttribute(enemyParts, EAttributeType.HIT_CHANCE);
            
            int maxCount = Mathf.Max(playerParts.Count, enemyParts.Count);
            
            for (int i = 0; i < maxCount; i++)
            {
                if (CheckBattleEnd()) yield break;

                // 3. Execution Delay (Initial)
                // yield return new WaitForSeconds(1.0f); 

                
                    PartInfo pPart = (i < playerParts.Count) ? playerParts[i] : null;
                    PartInfo ePart = (i < enemyParts.Count) ? enemyParts[i] : null;

                    // 2. Update Info UI
                    if (mono.playerPartInfoText != null) 
                        mono.playerPartInfoText.text = pPart != null ? GetPartInfoStr(pPart) : "";
                    if (mono.enemyPartInfoText != null) 
                        mono.enemyPartInfoText.text = ePart != null ? GetPartInfoStr(ePart) : "";

                    
                    // 4. Execution - PLAYER
                    if (pPart != null)
                    {
                        if (pPart.currentHealth > 0)
                        {
                            Debug.Log($"[Battle] Player Part {pPart.partRefObj.partName} Acting...");
                            if (pPart.logicObj != null) pPart.logicObj.OnTurnStart();
                            ExecuteAttackLogic(pPart, true, pGlobalCrit, pGlobalHit);
                            UpdateBattleUI(); // Update HP immediately
                            yield return new WaitForSeconds(1.0f); // Wait 1s
                        }
                        else
                        {
                            Debug.Log($"[Battle] Player Part {pPart.partRefObj.partName} is Dead/Inactive.");
                        }
                    }
                    
                    // 4. Execution - ENEMY
                    if (ePart != null && ePart.currentHealth > 0)
                    {
                         Debug.Log($"[Battle] Enemy Part {ePart.partRefObj.partName} Acting...");
                        if (ePart.logicObj != null) ePart.logicObj.OnTurnStart();
                        ExecuteAttackLogic(ePart, false, eGlobalCrit, eGlobalHit);
                        UpdateBattleUI(); // Update HP immediately
                        yield return new WaitForSeconds(1.0f); // Wait 1s
                    }
                    
                    // 5. Final Update UI (Safe measure)
                    UpdateBattleUI();
                    
                    // 6. Check Death
                    CheckDeadParts(playerParts);
                    CheckDeadParts(enemyParts);
            
            }
            
            Debug.Log("[Battle] Round Ended.");
            
            // Check Final Result
            if (!CheckBattleEnd())
            {
                // If neither dead, go to Combine UI
                 Debug.Log("[Battle] Round Over -> No Death -> Go Combine");
                 GoToCombine();
            }
        }
        
        private bool CheckBattleEnd()
        {
            float pHp = GameModel.instance.playerHealth;
            float eHp = (GameModel.instance.currentEnemy != null) ? GameModel.instance.currentEnemy.currentHealth : 0;
            
            Debug.Log($"[Battle] CheckBattleEnd: PlayerHP={pHp}, EnemyHP={eHp}");

            if (pHp <= 0)
            {
                 Debug.Log("[Battle] Player Defeated -> Go Map (Loss)");
                UICoreMgr.instance.AddNode(new UINodeLose(SCUIShowType.FULL));
                return true;
            }
            
            if (eHp <= 0 && GameModel.instance.currentEnemy != null)
            {
                 Debug.Log("[Battle] Enemy Defeated -> Go Map (Win)");
                // Normally Win -> Rewards -> Map? 
                // User says "Battle end go to map ui".
                GoToMap();
                 return true;
            }
            return false;
        }
        
        private void GoToCombine()
        {
             // Close Battle, Open Combine
             // User request: "Wash back to pool... redraw... reset enemy"
             
             
             // Close Self
             UICoreMgr.instance.RemoveNode(nameof(UINodeBattle));
             
             // Open Combine
             UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL)); 
        }
        
        private void GoToMap()
        {
             // Close Battle, Open Map
             // Ensure we clean up battle state (return parts, redraw, reset enemy) even on Win/Loss
             GameModel.instance.PrepareNextBattleRound();
             
             //UICoreMgr.instance.RemoveNode(nameof(UINodeBattle));
             //UICoreMgr.instance.RemoveNode(nameof(UINodeMaskCombine));

             // Check if Boss Battle
             bool isBoss = false;
             // Assuming MapManager exists and tracks nodes. Using safe access.
             if (MapManager.instance != null && GameModel.instance != null)
             {
                 var node = MapManager.instance.GetNode(GameModel.instance.playerMapPosition.x,GameModel.instance.playerMapPosition.y);
                 if (node != null && node.NodeType == RoomType.Boss)
                 {
                     isBoss = true;
                 }
             }

             if (isBoss)
             {
                 Debug.Log("[Battle] Boss Defeated -> Game Win!");
                 UICoreMgr.instance.AddNode(new UINodeWin(SCUIShowType.FULL)); // Game Clear
             }
             else
             {
                 Debug.Log("[Battle] Enemy Defeated -> Loot!");
                 UICoreMgr.instance.AddNode(new UINodeBattleWin(SCUIShowType.ADDITION)); // Battle Win/Loot
             }
        }

        private float CalculateGlobalAttribute(List<PartInfo> parts, EAttributeType type)
        {
            float total = 0;
            foreach(var p in parts)
            {
                if (p.currentHealth > 0)
                {
                    total += GetAttribute(p, type);
                }
            }
            return total;
        }
        
        private string GetPartInfoStr(PartInfo part)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<color=yellow>{part.partRefObj.partName}</color>");
            if (part.partRefObj.entryList != null)
            {
                foreach(var entry in part.partRefObj.entryList)
                {
                    sb.AppendLine($"{entry.attributeType}: {entry.attributeValue}");
                }
            }
            return sb.ToString();
        }
        
        private void ExecuteAttackLogic(PartInfo attacker, bool isPlayerAttacker, float globalCrit, float globalHit)
        {
            float att = GetAttribute(attacker, EAttributeType.ATTACK);
            // Debug Log for Attribute
            Debug.Log($"[Battle] ExecuteAttack: {attacker.partRefObj.partName} (Player:{isPlayerAttacker}) AttackValue: {att}");
            
            // If no attack, skip
            if (att <= 0) return;

            // Target Lists
            List<PartInfo> targetsList = isPlayerAttacker 
                ? (GameModel.instance.currentEnemy != null ? GameModel.instance.currentEnemy.parts : null)
                : GameModel.instance.playerBattleParts;
                
            if (targetsList == null) targetsList = new List<PartInfo>();

            // 1. Find overlapping targets
            List<Vector2Int> attackShape = GetOccupiedGrids(attacker);
            List<PartInfo> hitParts = new List<PartInfo>();
            
            foreach(var pos in attackShape)
            {
                // Find target part at this pos
                PartInfo hit = targetsList.Find(t => t.currentHealth > 0 && IsGridOccupiedBy(t, pos));
                if (hit != null) 
                {
                    if (!hitParts.Contains(hit))
                    {
                        hitParts.Add(hit);
                        Debug.Log($"[Battle] Hit Part Finding: {attacker.partRefObj.partName} hits {hit.partRefObj.partName} at {pos}");
                    }
                }
            }
            
            // 2. Resolve Damage
            if (hitParts.Count > 0)
            {
                foreach(var target in hitParts)
                {
                    ResolveDamage(attacker, target, att, isPlayerAttacker, globalCrit, globalHit);
                }
            }
            else
            {
                // Attack Body
                Debug.Log($"[Battle] {attacker.partRefObj.partName} -> No Parts Hit. Attacking Body Directly.");
                ResolveBodyDamage(attacker, att, isPlayerAttacker, globalCrit, globalHit);
            }
        }
        
        private void ResolveDamage(PartInfo attacker, PartInfo target, float damageBase, bool isPlayerAttacker, float globalCrit, float globalHit)
        {
            // 1. Hit Check
            // Formula: Base 100% + Global Hit Bonus (Sum of all parts)
            float finalHit = 0f + globalHit;
            
            if (Random.Range(0f, 100f) > finalHit) 
            {
                 Debug.Log($"[Battle] Miss! {attacker.partRefObj.partName} -> {target.partRefObj.partName} (Chance: {finalHit}%)");
                 return;
            }

            // 2. Crit Check
            // Formula: Global Crit Chance (Sum of all parts)
            float finalCrit = globalCrit;
            
            bool isCrit = false;
            if (Random.Range(0f, 100f) <= finalCrit)
            {
                isCrit = true;
                damageBase *= 1.5f;
            }
            
            // 3. Defense
            float def = GetAttribute(target, EAttributeType.DEFEND);
            float finalDmg = Mathf.Max(0, damageBase - def);
            
            Debug.Log($"[Battle] Hit! {attacker.partRefObj.partName} -> {target.partRefObj.partName} Dmg:{finalDmg} (Crit:{isCrit})");
            
            // 4. Apply Damage & Trample
            int dmgInt = Mathf.FloorToInt(finalDmg);
            int overflow = 0;
            
            if (dmgInt > target.currentHealth)
            {
                overflow = dmgInt - target.currentHealth;
                target.currentHealth = 0;
            }
            else
            {
                target.currentHealth -= dmgInt;
            }
            
            if (target != null && target.logicObj != null)
            {
                target.logicObj.OnDamageTaken(dmgInt);
            }

            if (overflow > 0)
            {
                Debug.Log($"[Battle] Trample! Overflow {overflow} to Body.");
                ApplyBodyDamage(overflow, isPlayerAttacker);
            }
        }
        
        private void ResolveBodyDamage(PartInfo attacker, float damageBase, bool isPlayerAttacker, float globalCrit, float globalHit)
        {
             // Similar Hit/Crit logic
             float finalHit = 100f + globalHit;

             if (Random.Range(0f, 100f) > finalHit) return;
             
             float finalCrit = globalCrit;
             
             if (Random.Range(0f, 100f) <= finalCrit) damageBase *= 1.5f;
             
             // Body Defense? Assume 0 for now.
             float finalDmg = Mathf.Max(0, damageBase);
             ApplyBodyDamage(Mathf.FloorToInt(finalDmg), isPlayerAttacker);
        }
        
        private void ApplyBodyDamage(int amount, bool isPlayerAttacker)
        {
            if (isPlayerAttacker)
            {
                // Damage Enemy
                if (GameModel.instance.currentEnemy != null)
                {
                    GameModel.instance.currentEnemy.currentHealth = Mathf.Max(0, GameModel.instance.currentEnemy.currentHealth - amount);
                    //if (GameModel.instance.currentEnemy.currentHealth== 0)
                    //    UICoreMgr.instance.AddNode(new UINodeBattleWin(SCUIShowType.ADDITION));
                }
            }
            else
            {
                // Damage Player
                GameModel.instance.playerHealth = Mathf.Max(0, GameModel.instance.playerHealth - amount);
                //
                if (GameModel.instance.playerHealth == 0)
                    UICoreMgr.instance.AddNode(new UINodeLose(SCUIShowType.FULL));
            }
        }

        private float GetAttribute(PartInfo part, EAttributeType type)
        {
            if (part == null || part.partRefObj == null || part.partRefObj.entryList == null) return 0;
            var entry = part.partRefObj.entryList.Find(x => x.attributeType == type);
            return entry != null ? entry.attributeValue : 0;
        }
        
        private List<Vector2Int> GetOccupiedGrids(PartInfo part)
        {
             List<Vector2Int> grids = new List<Vector2Int>();
             if (part == null) return grids;
             
             var shape = part.partRefObj.posList;
             Vector2Int origin = part.gridPos;
            int rot = 0;
             
             if (shape != null)
             {
                 foreach(var p in shape)
                 {
                     Vector2Int rotatedP = new Vector2Int(p.x, p.y);
                     for(int k=0; k<rot; k++) rotatedP = new Vector2Int(-rotatedP.y, rotatedP.x);
                     grids.Add(origin + rotatedP);
                 }
             }
             else grids.Add(origin);
             
             return grids;
        }
        
        private bool IsGridOccupiedBy(PartInfo part, Vector2Int pos)
        {
            var occupied = GetOccupiedGrids(part);
            return occupied.Contains(pos);
        }
        
        private void CheckDeadParts(List<PartInfo> parts)
        {
            // Just ensure 0 is handled, cleanup happens via View?
            // "Part broken logic" -> OnPartBroken
            foreach(var p in parts)
            {
                if (p.currentHealth <= 0)
                {
                    // If just died? status check? 
                    // Should we remove them? 
                    // For now, Logic execution checks currentHealth > 0, so they are effectively disabled.
                    // Visuals might need update (Gray out or Destroy).
                }
            }
        }
        
        private void UpdateBattleUI()
        {
            //if (mono.sliderPlayerHp != null) mono.sliderPlayerHp.value = GameModel.instance.playerHealth;
            //if (mono.sliderEnemyHp != null && GameModel.instance.currentEnemy != null) 
            //    mono.sliderEnemyHp.value = GameModel.instance.currentEnemy.currentHealth;


            // 1. Setup HP Sliders
            if (mono.imgHealthBar_player != null)
            {
                mono.imgHealthBar_player.fillAmount = (float)GameModel.instance.playerHealth / GameModel.instance.playerMaxHealth;
                mono.txtHealth_player.text = GameModel.instance.playerHealth + "/" + GameModel.instance.playerMaxHealth;
            }
            if (mono.imgHealthBar_enemy != null)
            {
                mono.imgHealthBar_enemy.fillAmount = (float)GameModel.instance.currentEnemy.currentHealth / GameModel.instance.currentEnemy.maxHealth;
                mono.txtHealth_enemy.text = GameModel.instance.currentEnemy.currentHealth + "/" + GameModel.instance.currentEnemy.maxHealth;
            }
            // Update Part Visuals (HP Text)
            if (_playerFacePanel != null) _playerFacePanel.RefreshParts();
            if (_enemyFacePanel != null) _enemyFacePanel.RefreshParts();
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
