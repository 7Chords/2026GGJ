using GameCore.RefData;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 游戏模型 放所有的运行时数据并提供数据处理的相关方法
    /// </summary>
    public class GameModel : Singleton<GameModel>
    {
        public List<PartInfo> bagPartInfoList; //背包部位列表(玩家局外拥有的全部)
        public List<PartInfo> deckPartInfoList; //牌堆部位列表(在牌堆里但是玩家当前未持有的)
        public List<PartInfo> busyPartInfoList; //玩家当前持有的部位列表
        public List<PartInfo> battlePartInfoList;//当前战斗中的部位列表（在脸上）

        public int playerHealth; //玩家生命
        public int playerMaxHealth;//玩家最大生命
        public int playerMoney;//玩家金钱

        public long rollStoreId; //进入商店节点后roll到的商店id
        
        public Vector2Int playerMapPosition = new Vector2Int(-1, -1);//玩家地图坐标位置
        public int playerFloor;//玩家当前在第几个楼层

        public List<FaceGridInfo> playerFaceGridInfoList;//玩家当前脸部格子信息列表
        public List<GameObject> playerFaceGridGOList;//玩家当前脸部格子物体列表

        public EnemyInfo currentEnemy;
        public List<FaceGridInfo> enemyFaceGridInfoList;//敌人当前脸部格子信息列表



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
            battlePartInfoList = new List<PartInfo>();


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

            int goIndex = playerFaceGridGOList.IndexOf(_hitGridGO);

            if (ratio.x < 0.5 && ratio.y < 0.5)//第一象限
            {
                hitAsLocalGridPos = new Vector2Int(Mathf.CeilToInt(localCenterPos.x), Mathf.CeilToInt(localCenterPos.y));
            }
            else if (ratio.x >= 0.5 && ratio.y < 0.5)//第二象限
            {
                hitAsLocalGridPos = new Vector2Int(Mathf.FloorToInt(localCenterPos.x), Mathf.CeilToInt(localCenterPos.y));

            }
            else if (ratio.x >= 0.5 && ratio.y > 0.5)//第三象限
            {
                hitAsLocalGridPos = new Vector2Int(Mathf.FloorToInt(localCenterPos.x), Mathf.FloorToInt(localCenterPos.y));

            }
            else if (ratio.x < 0.5 && ratio.y >= 0.5)//第四象限
            {
                hitAsLocalGridPos = new Vector2Int(Mathf.CeilToInt(localCenterPos.x), Mathf.FloorToInt(localCenterPos.y));
            }
            List<Vector2Int> retList = new List<Vector2Int>();
            Vector2Int partFacePos = Vector2Int.zero;
            for (int i = 0; i < _localGridList.Count; i++)
            {
                partFacePos = (_localGridList[i] - hitAsLocalGridPos) + playerFaceGridInfoList[goIndex].pos;
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
                info = playerFaceGridInfoList.Find(x => x.pos == facePosList[i]);
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
                info = playerFaceGridInfoList.Find(x => x.pos == _faceOccupyPosList[i]);
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
                FaceGridInfo info = playerFaceGridInfoList.Find(x => x.pos == _posList[i]);
                if (info == null)
                    continue;
                info.hasPart = false;
            }
        }

        public int GetBattleOrderByPartInfo(PartInfo _info)
        {
            if (_info == null)
                return -1;
            if (battlePartInfoList == null || !battlePartInfoList.Contains(_info))
                return -1;
            battlePartInfoList.Sort((a, b) =>
            {
                Vector2Int aPos = a.GetMinGridPos();
                Vector2Int bPos = b.GetMinGridPos();
                if (aPos.y != bPos.y)
                    return aPos.y.CompareTo(bPos.y);
                return aPos.x.CompareTo(bPos.x);
            });
            return battlePartInfoList.IndexOf(_info) + 1;//索引加1用于显示
        }






        public void RollRandomShop()
        {
            List<StoreRefObj> storeRefList = SCRefDataMgr.instance.storeRefList.refDataList.Where(refObj => refObj.floor == playerFloor).ToList();
            long id = storeRefList[Random.Range(0, storeRefList.Count)].id;
            rollStoreId = id;
        }
        public void GenerateRandomEnemy()
        {

            //获得一个当前楼层的随机敌人的配表
            List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList.Where(refObj => refObj.floor == playerFloor).ToList();
            if (enemies == null || enemies.Count == 0) return;
            EnemyRefObj enemyRef = enemies[Random.Range(0, enemies.Count)];
            currentEnemy = new EnemyInfo(enemyRef);


            if (enemyRef.initPartList != null && enemyRef.initPartList.Count > 0)
            {
                //获得敌人的部位池子
                List<PartRefObj> partRefList =new List<PartRefObj>();
                for(int i =0;i<enemyRef.initPartList.Count;i++)
                {
                    for(int j = 0; j < enemyRef.initPartList[i].partAmount; j++)
                    {
                        partRefList.Add(SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == enemyRef.initPartList[i].partId));
                    }
                }
                //生成牌堆
                for (int i = 0; i < partRefList.Count; i++)
                {
                    PartInfo info = new PartInfo(partRefList[i]);
                    currentEnemy.deckParts.Add(info);
                }

                //随机出战斗的部位
                int pickCount = Mathf.Min(GameConst.INIT_ENEMY_PART_COUNT, currentEnemy.deckParts.Count);
                for (int i = 0; i < pickCount; i++)
                {
                    int idx = Random.Range(0, currentEnemy.deckParts.Count);
                    PartInfo selectPartRefObj = currentEnemy.deckParts[idx];
                    currentEnemy.deckParts.RemoveAt(idx);
                    currentEnemy.battleParts.Add(selectPartRefObj);
                }
            }

            //生成敌人的随机布局
            GenerateEnemyLayout(currentEnemy);
        }
        
        public void PrepareNextBattleRound()
        {
            Debug.Log($"[GameModel] PrepareNextBattleRound Start. Deck: {deckPartInfoList.Count}, Busy: {busyPartInfoList.Count}, Battle: {battlePartInfoList.Count}");
            
            // 1. Reset Lists
            if (deckPartInfoList == null) deckPartInfoList = new List<PartInfo>();
            else deckPartInfoList.Clear();
            
            if (busyPartInfoList == null) busyPartInfoList = new List<PartInfo>();
            else busyPartInfoList.Clear();
            
            if (battlePartInfoList == null) battlePartInfoList = new List<PartInfo>();
            else battlePartInfoList.Clear(); // Just clear the reference list, parts are in Bag

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

        private List<Vector2Int> _cachedEnemyDisabledGrids;

        //private void EnsureEnemyDisabledGridsLoaded()
        //{
        //    if (_cachedEnemyDisabledGrids != null) return;
        //    _cachedEnemyDisabledGrids = new List<Vector2Int>();

        //    // Lazy load UI prefab to get config
        //    // Use GameCore.UI.UIMonoBattle to resolve ambiguity if any
        //    GameObject uiGO = ResourcesHelper.LoadGameObject("panel_battle");
        //    if (uiGO != null)
        //    {
        //        var battleMono = uiGO.GetComponent<UIMonoBattle>();
        //        if (battleMono != null && battleMono.enemyFace != null)
        //        {
        //            if (battleMono.enemyFace.disabledGrids != null)
        //            {
        //                _cachedEnemyDisabledGrids.AddRange(battleMono.enemyFace.disabledGrids);
        //                Debug.Log($"[GameModel] Loaded {_cachedEnemyDisabledGrids.Count} disabled grids from Enemy Face UI.");
        //            }
        //        }
        //        ResourcesHelper.ReleaseInstance(uiGO);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("[GameModel] Failed to load panel_battle for disabled grids config.");
        //    }
        //}

        private void GenerateEnemyLayout(EnemyInfo _enemyInfo)
        {
            foreach (var part in _enemyInfo.battleParts)
            {
                if (TryFindValidPlacement(part.partRefObj, out Vector2Int pos, out int rotStep))
                {
                    MarkOccupancy(part, pos, rotStep);
                }
                else
                {
                    SCDebugHelper.LogWarning($"[GameModel] Could not fit enemy part {part.partRefObj.partName}");
                }
            }
        }
        

        private bool TryFindValidPlacement(PartRefObj part, out Vector2Int resultPos,out int resultRot)
        {
            resultPos = Vector2Int.zero;
            resultRot = 0;
            for (int i = 0; i < 50; i++)
            {
                int rot = Random.Range(0, 4);
                FaceGridInfo gridInfo = enemyFaceGridInfoList[Random.Range(0, enemyFaceGridInfoList.Count)];
                Vector2Int origin = gridInfo.pos;
                if (IsValidPlacement(part, origin, rot))
                {
                    resultPos = origin;
                    resultRot = rot;
                    return true;
                }
            }

            return false;
        }

        private bool IsValidPlacement(PartRefObj _part, Vector2Int _originFacePos, int _rotStep)
        {
            List<Vector2Int> shape = GameCommon.RotateShape(_part.GetOccupyPosList(), _rotStep);
            foreach (var offset in shape)
            {
                Vector2Int p = _originFacePos + offset;
                FaceGridInfo gridInfo = null;
                gridInfo = enemyFaceGridInfoList.Find(x => x.pos == p);
                if (gridInfo == null || gridInfo.hasPart)
                    return false;
            }

            return true;
        }

        private void MarkOccupancy(PartInfo part, Vector2Int origin, int rot)
        {
            List<Vector2Int> rotateLocalOccupyPosList = GameCommon.RotateShape(part.localOccupyPosList, rot);
            List<Vector2Int> rotateLocalEffectPosList = GameCommon.RotateShapeAndMoveBySample(part.localEffectPosList, rot, part.localOccupyPosList);

            foreach (var offset in rotateLocalOccupyPosList)
            {
                Vector2Int p = origin + offset;
                FaceGridInfo gridInfo = enemyFaceGridInfoList.Find(x => x.pos == p);
                if (gridInfo == null)
                    continue;
                gridInfo.hasPart = true;
                part.curOccupyFacePosList.Add(p);
            }
            foreach (var offset in rotateLocalEffectPosList)
            {
                Vector2Int p = origin + offset;
                FaceGridInfo gridInfo = enemyFaceGridInfoList.Find(x => x.pos == p);
                if (gridInfo == null)
                    continue;
                part.curEffectFacePosList.Add(p);
            }
            part.isOnFace = true;
        }

    }

}
