using GameCore.RefData;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public List<PartInfo> playerBattlePartInfoList;//当前战斗中的部位列表（在脸上）

        public int playerHealth; //玩家生命
        public int playerMaxHealth;//玩家最大生命
        public int playerMoney;//玩家金钱

        public long rollStoreId; //进入商店节点后roll到的商店id
        
        public Vector2Int playerMapPosition = new Vector2Int(-1, -1);//玩家地图坐标位置


        public List<FaceGridInfo> faceGridInfoList;//玩家当前脸部格子信息列表
        public List<GameObject> faceGOList;

        public EnemyInfo currentEnemy;

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
            playerBattlePartInfoList = new List<PartInfo>();


            PartEffectObj partEffectObj = null;
            PartInfo info = null;
            PartRefObj partRefObj = null;
            for (int i = 0; i < playerRefObj.initPartList.Count; i++)
            {
                partEffectObj = playerRefObj.initPartList[i];
                if (partEffectObj == null)
                    continue;
                for(int j =0;j< partEffectObj.partAmount;j++)
                {
                    partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == partEffectObj.partId);
                    if (partRefObj == null)
                        continue;
                    info = new PartInfo(partRefObj);
                    bagPartInfoList.Add(info);
                }
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

        public void TakeDamage(int _amount)
        {
            playerHealth = Mathf.Clamp(playerHealth - _amount, 0, playerMaxHealth);
        }

        public List<Vector2Int> GetPlaceFaceOccupyPosList(GameObject _hitGridGO, Vector3 _mousePos, List<Vector2Int> _localGridList)
        {
            RectTransform gridRect = _hitGridGO.GetComponent<RectTransform>();
            if (gridRect == null) return null;

            //获取格子的像素尺寸
            Vector2 gridSize = new Vector2(gridRect.rect.width, gridRect.rect.height);
            Vector2 pixelPosInGrid;
            //转换鼠标屏幕坐标到格子Rect内的本地坐标
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect,
                _mousePos,
                SCGame.instance.gameCamera,
                out pixelPosInGrid))
            {
                return null;
            }

            //修正坐标（转为相对于格子左上角的正数）
            pixelPosInGrid += new Vector2(gridSize.x / 2f, gridSize.y / 2f);
            Vector2 ratio = new Vector2(pixelPosInGrid.x / gridSize.x, 1 - pixelPosInGrid.y / gridSize.y);

            //思路是这样 因为拖拽预览时拖拽的是图片的中心点
            //如果拖拽点在当前命中格子的第一象限 则当前命中格子当作原图形中心点的右下格子
            //如果拖拽点在当前命中格子的第二象限 则当前命中格子当作原图形中心点的左下格子
            //如果拖拽点在当前命中格子的第三象限 则当前命中格子当作原图形中心点的左上格子
            //如果拖拽点在当前命中格子的第四象限 则当前命中格子当作原图形中心点的右上格子

            Vector2 localCenterPos = GameCommon.CalculateLocalCenterPos(_localGridList);
            Vector2Int hitAsLocalGridPos = Vector2Int.zero;//这个是重要的概念 表示的是鼠标所在的格子映射为本地格子列表中的哪一个格子（这个格子不一定在列表里 但是自做一个偏移参考）

            int goIndex = faceGOList.IndexOf(_hitGridGO);

            if (ratio.x < 0.5 && ratio.y < 0.5)//第一象限
            {
                hitAsLocalGridPos = new Vector2Int(Mathf.CeilToInt(localCenterPos.x), (int)Mathf.CeilToInt(localCenterPos.y));
            }
            else if (ratio.x >= 0.5 && ratio.y < 0.5)//第二象限
            {
                hitAsLocalGridPos = new Vector2Int(Mathf.FloorToInt(localCenterPos.x), (int)Mathf.CeilToInt(localCenterPos.y));

            }
            else if (ratio.x >= 0.5 && ratio.y > 0.5)//第三象限
            {
                hitAsLocalGridPos = new Vector2Int((int)Mathf.FloorToInt(localCenterPos.x), (int)Mathf.FloorToInt(localCenterPos.y));

            }
            else if (ratio.x < 0.5 && ratio.y >= 0.5)//第四象限
            {
                hitAsLocalGridPos = new Vector2Int((int)Mathf.CeilToInt(localCenterPos.x), (int)Mathf.FloorToInt(localCenterPos.y));
            }
            List<Vector2Int> retList = new List<Vector2Int>();
            Vector2Int partFacePos = Vector2Int.zero;
            for (int i = 0; i < _localGridList.Count; i++)
            {
                partFacePos = (_localGridList[i] - hitAsLocalGridPos) + faceGridInfoList[goIndex].pos;
                retList.Add(partFacePos);
            }
            return retList;
        }

        public List<Vector2Int> GetPlaceFaceEffectPosList(List<Vector2Int> _localEffectPosList,List<Vector2Int> _faceOccupyPosList, List<Vector2Int> _localOccupyPosList)
        {
            if (_localEffectPosList == null || _faceOccupyPosList == null || _localOccupyPosList == null)
                return null;
            Vector2Int offset = _faceOccupyPosList[0] - _localOccupyPosList[0];
            List<Vector2Int> retList = _localEffectPosList.Select(p => p = new Vector2Int(p.x + offset.x, p.y + offset.y)).ToList();
            return retList;
        }

        public bool CanPlacePart(GameObject _hitGridGO ,Vector3 _mousePos, List<Vector2Int> _localGridList)
        {
            List<Vector2Int> facePosList = GetPlaceFaceOccupyPosList(_hitGridGO, _mousePos, _localGridList);
            if (facePosList == null)
                return false;
            FaceGridInfo info = null;
            for(int i =0;i<facePosList.Count;i++)
            {
                info = faceGridInfoList.Find(x => x.pos == facePosList[i]);
                if (info == null || info.hasPart)
                    return false;
            }
            return true;
        }
        public bool CanPlacePart(List<Vector2Int> _faceOccupyPosList)
        {
            if (_faceOccupyPosList == null)
                return false;
            FaceGridInfo info = null;
            for (int i = 0; i < _faceOccupyPosList.Count; i++)
            {
                info = faceGridInfoList.Find(x => x.pos == _faceOccupyPosList[i]);
                if (info == null || info.hasPart)
                    return false;
            }
            return true;

        }

        public void SetGridsEmpty(List<Vector2Int> _posList)
        {
            if (_posList == null)
                return;
            for(int i =0;i<_posList.Count;i++)
            {
                FaceGridInfo info = faceGridInfoList.Find(x => x.pos == _posList[i]);
                if (info == null)
                    continue;
                info.hasPart = false;
            }
        }

        public int GetBattleOrderByPartInfo(PartInfo _info)
        {
            if (_info == null)
                return -1;
            if (playerBattlePartInfoList == null || !playerBattlePartInfoList.Contains(_info))
                return -1;
            playerBattlePartInfoList.Sort((a, b) =>
            {
                Vector2Int aPos = a.GetMinGridPos();
                Vector2Int bPos = b.GetMinGridPos();
                if (aPos.y != bPos.y)
                    return aPos.y.CompareTo(bPos.y);
                return aPos.x.CompareTo(bPos.x);
            });
            return playerBattlePartInfoList.IndexOf(_info) + 1;//索引加1用于显示
        }







        public void GenerateRandomEnemy()
        {
            currentEnemy = new EnemyInfo();

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
                int pickCount = Mathf.Min(initEnemyPartCount, pool.Count);
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
            Debug.Log($"[GameModel] PrepareNextBattleRound Start. Deck: {deckPartInfoList.Count}, Busy: {busyPartInfoList.Count}, Battle: {playerBattlePartInfoList.Count}");
            
            // 1. Reset Lists
            if (deckPartInfoList == null) deckPartInfoList = new List<PartInfo>();
            else deckPartInfoList.Clear();
            
            if (busyPartInfoList == null) busyPartInfoList = new List<PartInfo>();
            else busyPartInfoList.Clear();
            
            if (playerBattlePartInfoList == null) playerBattlePartInfoList = new List<PartInfo>();
            else playerBattlePartInfoList.Clear(); // Just clear the reference list, parts are in Bag

            // 2. Return All Living Parts from Bag to Deck
            if (bagPartInfoList != null)
            {
                foreach(var part in bagPartInfoList)
                {
                    if (part.currentHealth > 0)
                    {
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
            DrawParts(4);
            
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

        public int initEnemyPartCount = 3; // Default 2 parts
        private List<Vector2Int> _cachedEnemyDisabledGrids;

        private void EnsureEnemyDisabledGridsLoaded()
        {
            if (_cachedEnemyDisabledGrids != null) return;
            _cachedEnemyDisabledGrids = new List<Vector2Int>();

            // Lazy load UI prefab to get config
            // Use GameCore.UI.UIMonoBattle to resolve ambiguity if any
            GameObject uiGO = ResourcesHelper.LoadGameObject("panel_battle");
            if (uiGO != null)
            {
                var battleMono = uiGO.GetComponent<UIMonoBattle>();
                if (battleMono != null && battleMono.enemyFace != null)
                {
                    if (battleMono.enemyFace.disabledGrids != null)
                    {
                        _cachedEnemyDisabledGrids.AddRange(battleMono.enemyFace.disabledGrids);
                        Debug.Log($"[GameModel] Loaded {_cachedEnemyDisabledGrids.Count} disabled grids from Enemy Face UI.");
                    }
                }
                ResourcesHelper.ReleaseInstance(uiGO);
            }
            else
            {
                Debug.LogWarning("[GameModel] Failed to load panel_battle for disabled grids config.");
            }
        }

        private void GenerateEnemyLayout(EnemyInfo enemy)
        {
            // Ensure config is loaded
            EnsureEnemyDisabledGridsLoaded();

            // 6x7 Grid (Hardcoded size for Model logic, or could read from prefab too if needed)
            bool[,] occupiedGrid = new bool[4, 7];

            foreach (var part in enemy.parts)
            {
                if (TryFindValidPlacement(occupiedGrid, part.partRefObj, out Vector2Int pos, out int rot))
                {
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
                int x = Random.Range(0, 4);
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
                
                // Check Disabled Grids
                if (_cachedEnemyDisabledGrids != null && _cachedEnemyDisabledGrids.Contains(p))
                {
                    return false;
                }
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
            if (part.occupyPosList != null)
            {
                foreach (var pObj in part.occupyPosList)
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

    public class EnemyInfo
    {
        public GameCore.RefData.EnemyRefObj enemyRef;
        public List<PartInfo> parts;
        public int maxHealth;
        public int currentHealth;
    }
}
