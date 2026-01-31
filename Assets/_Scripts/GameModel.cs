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
        public long rollEnemyId; //进入战斗节点后roll到的敌人id

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
                busyPartInfoList.Add(info);
                bagPartInfoList.Add(info);
                deckPartInfoList.Add(info);

            }
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

        private void GenerateEnemyLayout(EnemyData enemy)
        {
            // 6x7 Grid
            bool[,] occupiedGrid = new bool[4, 7];

            foreach (var part in enemy.parts)
            {
                if (TryFindValidPlacement(occupiedGrid, part.partRefObj, out Vector2Int pos, out int rot))
                {
                    part.gridPos = pos;
                    part.rotation = rot;
                    MarkOccupancy(occupiedGrid, part.partRefObj, pos, rot);
                }
                else
                {
                    Debug.LogWarning($"[GameModel] Could not fit enemy part {part.partRefObj.partName}");
                }
            }
        }

        // Copied helper methods from UIPanelEnemyMask (simplified)
        private bool TryFindValidPlacement(bool[,] grid, GameCore.RefData.PartRefObj part, out Vector2Int resultPos,
            out int resultRot)
        {
            resultPos = Vector2Int.zero;
            resultRot = 0;
            for (int i = 0; i < 50; i++)
            {
                int rot = Random.Range(0, 4);
                int x = Random.Range(0, 6);
                int y = Random.Range(0, 7);
                Vector2Int origin = new Vector2Int(x, y);
                if (IsValidPlacement(grid, part, origin, rot))
                {
                    resultPos = origin;
                    resultRot = rot;
                    return true;
                }
            }

            return false;
        }

        private bool IsValidPlacement(bool[,] grid, GameCore.RefData.PartRefObj part, Vector2Int origin, int rot)
        {
            List<Vector2Int> shape = GetRotatedShape(part, rot);
            foreach (var offset in shape)
            {
                Vector2Int p = origin + offset;
                if (p.x < 0 || p.x >= 4 || p.y < 0 || p.y >= 7) return false;
                if (grid[p.x, p.y]) return false;
            }

            return true;
        }

        private void MarkOccupancy(bool[,] grid, GameCore.RefData.PartRefObj part, Vector2Int origin, int rot)
        {
            List<Vector2Int> shape = GetRotatedShape(part, rot);
            foreach (var offset in shape)
            {
                Vector2Int p = origin + offset;
                grid[p.x, p.y] = true;
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
