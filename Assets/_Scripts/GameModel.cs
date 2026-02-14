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


        public long rollStoreId; //进入商店节点后roll到的商店id

        public PlayerInfo playerInfo;
        public List<FaceGridInfo> playerFaceGridInfoList;//玩家当前脸部格子信息列表
        public List<GameObject> playerFaceGridGOList;//玩家当前脸部格子物体列表

        public EnemyInfo curEnemyInfo;
        public List<FaceGridInfo> enemyFaceGridInfoList;//敌人当前脸部格子信息列表

        public ETurnOwnerType curTurnOwner;//当前行动方
        public int curActivePartIndex;//当前行动的部位索引

        public override void OnInitialize()
        {
            //初始化数据从配表读取
            PlayerRefObj playerRefObj = SCRefDataMgr.instance.playerConfigRefObj;
            if (playerRefObj == null)
                return;
            playerInfo = new PlayerInfo(playerRefObj);


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
                    info = new PartInfo(partRefObj,false);
                    playerInfo.bagPartInfoList.Add(info);
                }
            }
            
        }

        public void PlayerHeal(int _amount)
        {
            playerInfo.currentHealth = Mathf.Clamp(playerInfo.currentHealth + _amount, 0, playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HEAL);
        }

        public void PlayerTakeDamage(int _amount)
        {
            playerInfo.currentHealth = Mathf.Clamp(playerInfo.currentHealth - _amount, 0, playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HURT);

        }

        public void EnemyHeal(int _amount)
        {
            curEnemyInfo.currentHealth = Mathf.Clamp(curEnemyInfo.currentHealth + _amount, 0, curEnemyInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HEAL);

        }

        public void EnemyTakeDamage(int _amount)
        {
            curEnemyInfo.currentHealth = Mathf.Clamp(curEnemyInfo.currentHealth - _amount, 0, curEnemyInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HURT);

        }

        public void PartTakeDamage(PartInfo _partInfo,int _amount)
        {
            _partInfo.currentHealth = Mathf.Clamp(_partInfo.currentHealth - _amount, 0, _partInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HURT, _partInfo, _amount);

        }
        public void PartHeal(PartInfo _partInfo, int _amount)
        {
            _partInfo.currentHealth = Mathf.Clamp(_partInfo.currentHealth + _amount, 0, _partInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HEAL, _partInfo, _amount);

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

            Vector2 localCenterPos = GameCommon.CalculateGridCenterPos(_localGridList);
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
                info.SetEmpty();
            }
        }

        public int GetPlayerBattleOrderByPartInfo(PartInfo _info)
        {
            if (_info == null)
                return -1;
            if (playerInfo.battlePartInfoList == null || !playerInfo.battlePartInfoList.Contains(_info))
                return -1;
            playerInfo.battlePartInfoList.Sort((a, b) =>
            {
                Vector2Int aPos = a.GetMinGridPos();
                Vector2Int bPos = b.GetMinGridPos();
                if (aPos.y != bPos.y)
                    return aPos.y.CompareTo(bPos.y);
                return aPos.x.CompareTo(bPos.x);
            });
            return playerInfo.battlePartInfoList.IndexOf(_info) + 1;//索引加1用于显示
        }
        public int GetEnemyBattleOrderByPartInfo(PartInfo _info)
        {
            if (_info == null)
                return -1;
            if (curEnemyInfo == null || !curEnemyInfo.battlePartInfoList.Contains(_info))
                return -1;
            return curEnemyInfo.battlePartInfoList.IndexOf(_info) + 1;//索引加1用于显示
        }

        public void SortEnemtBattleOrder()
        {
            if (curEnemyInfo == null)
                return;
            curEnemyInfo.battlePartInfoList.Sort((a, b) =>
            {
                Vector2Int aPos = a.GetMinGridPos();
                Vector2Int bPos = b.GetMinGridPos();
                if (aPos.y != bPos.y)
                    return aPos.y.CompareTo(bPos.y);
                return aPos.x.CompareTo(bPos.x);
            });
        }



        public void RollRandomShop()
        {
            List<StoreRefObj> storeRefList = SCRefDataMgr.instance.storeRefList.refDataList.Where(refObj => refObj.floor == playerInfo.playerFloor).ToList();
            long id = storeRefList[Random.Range(0, storeRefList.Count)].id;
            rollStoreId = id;
        }
        public void GenerateNewBattle()
        {
            playerInfo.ClearListForNewBattle();

            if (playerInfo.bagPartInfoList != null)
            {
                foreach (var part in playerInfo.bagPartInfoList)
                {
                    if (part.currentHealth > 0)
                    {
                        playerInfo.deckPartInfoList.Add(part);
                    }
                }
            }
            PlayerDrawParts(GameConst.DRAW_CARD_COUNT_PER_TURN);
            GenerateRandomEnemy();

            //todo：由于unity gridlayout的创建问题 要等一段时间格子坐标啥的才创建完成不能马上创建敌人部位
            //否则位置出错 后续优化 采用一段动画表现显示页面给unity留出时间
            SCMsgCenter.SendMsg(SCMsgConst.NEW_GANE_START);
        }

        public void DealNextTurn()
        {
            //下个回合要做的处理
            //敌人和玩家都要做：把脸部五官回收到busy 原来busy回收到deck deck抽min(3,busyMax-curBusy) 格子全部设置为不占用
            //敌人还要重新生成一份布局
            playerInfo.deckPartInfoList.AddRange(playerInfo.busyPartInfoList);
            playerInfo.busyPartInfoList.Clear();
            playerInfo.busyPartInfoList.AddRange(playerInfo.battlePartInfoList);
            playerInfo.battlePartInfoList.Clear();
            int playerDrawCnt = Mathf.Min(GameConst.DRAW_CARD_COUNT_PER_TURN, GameConst.BUSY_CARD_MAX_COUNT - playerInfo.battlePartInfoList.Count);
            PlayerDrawParts(playerDrawCnt);
            foreach (var info in playerFaceGridInfoList)
                info.SetEmpty();

            curEnemyInfo.deckPartInfoList.AddRange(curEnemyInfo.busyPartInfoList);
            curEnemyInfo.busyPartInfoList.Clear();
            curEnemyInfo.busyPartInfoList.AddRange(curEnemyInfo.battlePartInfoList);
            curEnemyInfo.battlePartInfoList.Clear();
            int enemyDrawCnt = Mathf.Min(GameConst.DRAW_CARD_COUNT_PER_TURN, GameConst.BUSY_CARD_MAX_COUNT - curEnemyInfo.battlePartInfoList.Count);
            EnemyDrawParts(enemyDrawCnt);
            foreach (var info in enemyFaceGridInfoList)
                info.SetEmpty();

            GenerateEnemyLayout(curEnemyInfo);
            //SCMsgCenter.SendMsg(SCMsgConst.NEW_TURN_START);

        }

        public void PlayerDrawParts(int _count)
        {
            if (playerInfo.deckPartInfoList == null || playerInfo.deckPartInfoList.Count == 0)
            {
                return;
            }
            for (int i = 0; i < _count; i++)
            {
                if (playerInfo.deckPartInfoList.Count == 0) break;
                if (playerInfo.busyPartInfoList.Count == GameConst.BUSY_CARD_MAX_COUNT) break;
                int idx = Random.Range(0, playerInfo.deckPartInfoList.Count);
                PartInfo drawn = playerInfo.deckPartInfoList[idx];
                playerInfo.deckPartInfoList.RemoveAt(idx);

                if (playerInfo.busyPartInfoList == null) playerInfo.busyPartInfoList = new List<PartInfo>();
                playerInfo.busyPartInfoList.Add(drawn);
            }
        }

        public void EnemyDrawParts(int _count)
        {
            if (curEnemyInfo.deckPartInfoList == null || curEnemyInfo.deckPartInfoList.Count == 0)
            {
                return;
            }
            for (int i = 0; i < _count; i++)
            {
                if (curEnemyInfo.deckPartInfoList.Count == 0) break;
                if (curEnemyInfo.busyPartInfoList.Count == GameConst.BUSY_CARD_MAX_COUNT) break;
                int idx = Random.Range(0, curEnemyInfo.deckPartInfoList.Count);
                PartInfo drawn = curEnemyInfo.deckPartInfoList[idx];
                curEnemyInfo.deckPartInfoList.RemoveAt(idx);

                if (curEnemyInfo.busyPartInfoList == null) curEnemyInfo.busyPartInfoList = new List<PartInfo>();
                curEnemyInfo.busyPartInfoList.Add(drawn);
            }
        }

        public void GenerateRandomEnemy()
        {

            //获得一个当前楼层的随机敌人的配表
            List<EnemyRefObj> enemies = SCRefDataMgr.instance.enemyRefList.refDataList.Where(refObj => refObj.floor == playerInfo.playerFloor).ToList();
            if (enemies == null || enemies.Count == 0) return;
            EnemyRefObj enemyRef = enemies[Random.Range(0, enemies.Count)];
            curEnemyInfo = new EnemyInfo(enemyRef);


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
                    PartInfo info = new PartInfo(partRefList[i],true);
                    curEnemyInfo.deckPartInfoList.Add(info);
                }

                //随机出手牌区
                int pickCount = Mathf.Min(GameConst.INIT_ENEMY_PART_COUNT, curEnemyInfo.deckPartInfoList.Count);
                for (int i = 0; i < pickCount; i++)
                {
                    int idx = Random.Range(0, curEnemyInfo.deckPartInfoList.Count);
                    PartInfo selectPartInfo = curEnemyInfo.deckPartInfoList[idx];
                    curEnemyInfo.deckPartInfoList.RemoveAt(idx);
                    curEnemyInfo.busyPartInfoList.Add(selectPartInfo);
                }
            }

            //生成敌人的随机布局
            GenerateEnemyLayout(curEnemyInfo);
        }
      
        private void GenerateEnemyLayout(EnemyInfo _enemyInfo)
        {
            List<PartInfo> readyToRemoveInfoList = new List<PartInfo>();
            foreach (var part in _enemyInfo.busyPartInfoList)
            {
                part.ResetToBusy();
                if (TryFindValidPlacement(part.partRefObj, out Vector2Int pos, out int rotStep))
                {
                    readyToRemoveInfoList.Add(part);
                    MarkOccupancy(part, pos, rotStep);
                }
                else
                {
                    SCDebugHelper.LogWarning($"[GameModel] Could not fit enemy part {part.partRefObj.partName}");
                }
            }
            foreach(var info in readyToRemoveInfoList)
            {
                _enemyInfo.busyPartInfoList.Remove(info);
            }
            SortEnemtBattleOrder();
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
            List<Vector2Int> shape = GameCommon.RotateShapeAndMove2Zero(_part.GetOccupyPosList(), _rotStep);
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
            for(int i =0;i<rot;i++)
                part.RotateOnce();
            foreach (var offset in part.localOccupyPosList)
            {
                Vector2Int p = origin + offset;
                FaceGridInfo gridInfo = enemyFaceGridInfoList.Find(x => x.pos == p);
                if (gridInfo == null)
                    continue;
                gridInfo.SetOwnerPart(part);
                part.curOccupyFacePosList.Add(p);
            }
            foreach (var offset in part.localEffectPosList)
            {
                Vector2Int p = origin + offset;
                //FaceGridInfo gridInfo = enemyFaceGridInfoList.Find(x => x.pos == p);
                //if (gridInfo == null)
                //    continue;
                part.curEffectFacePosList.Add(p);
            }
            part.isOnFace = true;
            curEnemyInfo.battlePartInfoList.Add(part);
        }

    }

}
