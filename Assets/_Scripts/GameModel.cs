using GameCore.RefData;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 游戏模型 放所有的运行时数据
    /// </summary>
    public class GameModel : Singleton<GameModel>
    {
        public List<PartInfo> bagPartInfoList; //背包部位列表(玩家局外拥有的全部)
        public List<PartInfo> deckPartInfoList; //牌堆部位列表(在牌堆里但是玩家当前未持有的)
        public List<PartInfo> busyPartInfoList; //玩家当前持有的部位列表


        public int playerHealth; //玩家生命
        public int playerMaxHealth;
        public int playerMoney;

        public List<FacePartInfo> facePartInfoList; //脸部装备的部位列表
        public List<FaceGridInfo> faceGridInfoList; //脸部格子信息列表

        public long rollStoreId; //进入商店节点后roll到的商店id
        //public long rollEnemyId; //进入战斗节点后roll到的敌人id
        
        public Vector2Int playerMapPosition = new Vector2Int(-1, -1); // Current Player Position in Map (Layer, Index)

        public override void OnInitialize()
        {
            //初始化数据从配表读取
            PlayerRefObj playerRefObj = SCRefDataMgr.instance.playerConfigRefObj;
            if (playerRefObj == null)
                return;
            playerMaxHealth = playerRefObj.playerHealth;
            playerHealth = playerMaxHealth;
            playerMoney = playerRefObj.playerMoney;

            busyPartInfoList = new List<PartInfo>();
            bagPartInfoList = new List<PartInfo>();
            deckPartInfoList = new List<PartInfo>();

            PartEffectObj partEffectObj = null;
            PartInfo info = null;
            PartRefObj partRefObj = null;
            for (int i = 0; i < playerRefObj.initPartList.Count; i++)
            {
                partEffectObj = playerRefObj.initPartList[i];
                if (partEffectObj == null)
                    continue;
                partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == partEffectObj.partId);
                if (partRefObj == null)
                    continue;
                info = new PartInfo(partRefObj);
                // busyPartInfoList.Add(info); // Don't add to hand directly
                bagPartInfoList.Add(info); 
                // deckPartInfoList.Add(info); // Don't add to Deck manually, PrepareNextBattleRound handles it
            }
            
            Debug.Log($"[GameModel] OnInitialize Loop Done. Bag Count: {bagPartInfoList.Count}");

            // Initial Setup using Unified Logic
            PrepareNextBattleRound();
            
            Debug.Log($"[GameModel] OnInitialize Complete. Bag: {bagPartInfoList.Count}, Deck: {deckPartInfoList.Count}, Busy: {busyPartInfoList.Count}");
        }

        public void Heal(int _amount)
        {
            playerHealth = Mathf.Clamp(playerHealth + _amount, 0, playerMaxHealth);
        }

        public List<PartInfo> playerBattleParts = new List<PartInfo>();

        // --- Enemy Logic ---
        public EnemyData currentEnemy;

        public void GenerateRandomEnemy()
        {
            currentEnemy = new EnemyData();

            // 1. Random Enemy Ref
            var enemies = SCRefDataMgr.instance.enemyRefList.refDataList;
            if (enemies == null || enemies.Count == 0) return;
            var enemyRef = enemies[Random.Range(0, enemies.Count)];
            currentEnemy.enemyRef = enemyRef;
            currentEnemy.maxHealth = enemyRef.enemyHealth;
            currentEnemy.currentHealth = enemyRef.enemyHealth;

            // 2. Random Parts
            currentEnemy.parts = new List<PartInfo>();
            if (enemyRef.initPartList != null && enemyRef.initPartList.Count > 0)
            {
                List<GameCore.RefData.PartEffectObj> pool =
                    new List<GameCore.RefData.PartEffectObj>(enemyRef.initPartList);
                int pickCount = Mathf.Min(2, pool.Count);
                for (int k = 0; k < pickCount; k++)
                {
                    int idx = Random.Range(0, pool.Count);
                    var effect = pool[idx];
                    pool.RemoveAt(idx);

                    var partRef = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == effect.partId);
                    if (partRef != null)
                    {
                        currentEnemy.parts.Add(new PartInfo(partRef));
                    }
                }
            }

            // 3. Generate Layout (Logic from UIPanelEnemyMask moved here)
            GenerateEnemyLayout(currentEnemy);
        }
        
        public void PrepareNextBattleRound()
        {
            Debug.Log($"[GameModel] PrepareNextBattleRound Start. Deck: {deckPartInfoList.Count}, Busy: {busyPartInfoList.Count}, Battle: {playerBattleParts.Count}");
            
            // 1. Reset Lists
            if (deckPartInfoList == null) deckPartInfoList = new List<PartInfo>();
            else deckPartInfoList.Clear();
            
            if (busyPartInfoList == null) busyPartInfoList = new List<PartInfo>();
            else busyPartInfoList.Clear();
            
            if (playerBattleParts == null) playerBattleParts = new List<PartInfo>();
            else playerBattleParts.Clear(); // Just clear the reference list, parts are in Bag

            // 2. Return All Living Parts from Bag to Deck
            if (bagPartInfoList != null)
            {
                foreach(var part in bagPartInfoList)
                {
                    if (part.currentHealth > 0)
                    {
                        // Reset State
                        part.gridPos = new Vector2Int(-1, -1);
                        part.rotation = 0;
                        deckPartInfoList.Add(part);
                    }
                    else
                    {
                         Debug.Log($"[GameModel] Part {part.partRefObj.partName} is Dead/Broken. Remaining in Bag but not in Deck.");
                    }
                }
            }
            
            Debug.Log($"[GameModel] After Return - Deck: {deckPartInfoList.Count}");
            
            // 3. Random Draw 3
            DrawParts(3);
            
            Debug.Log($"[GameModel] After Draw - Deck: {deckPartInfoList.Count}, Busy: {busyPartInfoList.Count}");
            
            // 4. Reset Enemy (But Keep HP if valid)
            int preservedHp = -1;
            if (currentEnemy != null) preservedHp = currentEnemy.currentHealth;
            
            GenerateRandomEnemy();
            
            if (preservedHp > 0 && currentEnemy != null)
            {
                currentEnemy.currentHealth = preservedHp;
                Debug.Log($"[GameModel] Preserved Enemy HP: {preservedHp}");
            }
        }
        
        public void DrawParts(int count)
        {
            if (deckPartInfoList == null || deckPartInfoList.Count == 0) 
            {
                Debug.LogWarning("[GameModel] Deck is Empty! Cannot draw.");
                return;
            }
            
            for(int i=0; i<count; i++)
            {
                if (deckPartInfoList.Count == 0) break;
                
                int idx = Random.Range(0, deckPartInfoList.Count);
                PartInfo drawn = deckPartInfoList[idx];
                deckPartInfoList.RemoveAt(idx);
                
                if (busyPartInfoList == null) busyPartInfoList = new List<PartInfo>();
                busyPartInfoList.Add(drawn);
                Debug.Log($"[GameModel] Drawn part: {drawn.partRefObj.partName}");
            }
        }

        private void GenerateEnemyLayout(EnemyData enemy)
        {
             // Layout generation moved to UIPanelEnemyMask to access view configuration (disabled grids) directly.
             // We just ensure parts list is ready.
             // Initial positions will be (0,0) or invalid until UI initializes.
             foreach(var part in enemy.parts)
             {
                 part.gridPos = new Vector2Int(-1, -1);
                 part.rotation = 0;
             }
        }

        private List<Vector2Int> GetRotatedShape(GameCore.RefData.PartRefObj part, int rot)
        {
            List<Vector2Int> list = new List<Vector2Int>();
            if (part.posList != null)
            {
                foreach (var pObj in part.posList)
                {
                    Vector2Int p = new Vector2Int(pObj.x, pObj.y);
                    for (int k = 0; k < rot; k++) p = new Vector2Int(-p.y, p.x);
                    list.Add(p);
                }
            }
            else list.Add(Vector2Int.zero);

            return list;
        }
    }

    public class EnemyData
    {
        public GameCore.RefData.EnemyRefObj enemyRef;
        public List<PartInfo> parts;
        public int maxHealth;
        public int currentHealth;
    }
}
