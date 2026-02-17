using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public partial class GameModel
    {
        // 摆放优先级：包含所有部位（你定义的顺序）
        private List<EPartType> _partPlaceOrder = new List<EPartType>()
        { EPartType.EYE, EPartType.MOUTH, EPartType.NOSE, EPartType.SKIN };

        // 可调整：眼睛偏上区域占比（0.5=脸部上半区，0.6=上60%区域，建议0.5~0.6）
        private const float EYE_TOP_AREA_RATIO = 0.3f;
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
                List<PartRefObj> partRefList = new List<PartRefObj>();
                for (int i = 0; i < enemyRef.initPartList.Count; i++)
                {
                    for (int j = 0; j < enemyRef.initPartList[i].partAmount; j++)
                    {
                        partRefList.Add(SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == enemyRef.initPartList[i].partId));
                    }
                }
                //生成牌堆
                for (int i = 0; i < partRefList.Count; i++)
                {
                    PartInfo info = new PartInfo(partRefList[i], true);
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


        // 【完全保留你的主方法，仅修改摆位调用】
        private void GenerateEnemyLayout(EnemyInfo _enemyInfo)
        {
            List<PartInfo> readyToRemoveInfoList = new List<PartInfo>();
            List<PartInfo> orderedParts = new List<PartInfo>();

            // 按优先级取所有部位（你的逻辑不变，支持同类型多部位FindAll）
            for (int i = 0; i < _partPlaceOrder.Count; i++)
            {
                EPartType partType = _partPlaceOrder[i];
                List<PartInfo> targetPart = _enemyInfo.busyPartInfoList.FindAll(p => p.partRefObj.partType == partType);
                if (targetPart != null)
                {
                    orderedParts.AddRange(targetPart);
                }
            }

            // 核心修改：必须按顺序摆，但摆的时候要考虑“未来要摆的部位”，而不是只看已摆的
            for (int i = 0; i < orderedParts.Count; i++)
            {
                PartInfo curPart = orderedParts[i];
                curPart.ResetToBusy();
                Vector2Int pos = Vector2Int.zero;
                int rotStep = 0;

                // 关键修改：传入“剩余未摆的部位”，而不是只看已摆的
                bool success = TryFindGlobalOptimalPlacement(
                    _curPart: curPart,
                    _placedParts: orderedParts.GetRange(0, i), // 已摆的
                    _remainingParts: orderedParts.GetRange(i + 1, orderedParts.Count - i - 1), // 未摆的
                    _resultPos: out pos,
                    _resultRot: out rotStep
                );

                if (success)
                {
                    readyToRemoveInfoList.Add(curPart);
                    MarkOccupancy(curPart, pos, rotStep);
                }
                else
                {
                    SCDebugHelper.LogWarning($"部位{curPart.partRefObj.partName}无法摆放");
                }
            }

            // 移除已摆部位（你的逻辑不变）
            for (int i = 0; i < readyToRemoveInfoList.Count; i++)
            {
                _enemyInfo.busyPartInfoList.Remove(readyToRemoveInfoList[i]);
            }
            SortEnemyBattleOrder();
        }

        // 核心重写：真正的全局最优（新增眼睛偏上限制，其他逻辑不变）
        private bool TryFindGlobalOptimalPlacement(
            PartInfo _curPart,
            List<PartInfo> _placedParts,
            List<PartInfo> _remainingParts,
            out Vector2Int _resultPos,
            out int _resultRot)
        {
            _resultPos = Vector2Int.zero;
            _resultRot = 0;
            int maxTotalScore = -1;

            PartRefObj partRef = _curPart.partRefObj;
            List<FaceGridInfo> allFaceGrids = enemyFaceGridInfoList;

            // 存储所有“完美解”（效果无浪费 + 互相覆盖）
            List<Vector2Int> bestPosList = new List<Vector2Int>();
            List<int> bestRotList = new List<int>();

            // 核心新增：根据是否是眼睛，筛选专属摆位区域（眼睛=上半区，其他=全区域）
            List<FaceGridInfo> filterdEmptyGrids = GetFilteredEmptyGrids(_curPart, allFaceGrids);

            // 遍历所有旋转（你的逻辑不变）
            for (int rot = 0; rot < 4; rot++)
            {
                List<Vector2Int> rotatedOccupy = GameCommon.RotateShapeAndMove2Zero(partRef.GetOccupyPosList(), rot);
                List<Vector2Int> rotatedEffect = GameCommon.RotateShapeAndMoveBySample(partRef.GetEffectPosList(), rot, partRef.GetOccupyPosList());

                // 遍历筛选后的格子（先打乱，避免左上角）
                List<FaceGridInfo> emptyGrids = new List<FaceGridInfo>(filterdEmptyGrids);
                ShuffleGridList(emptyGrids);

                foreach (FaceGridInfo grid in emptyGrids)
                {
                    Vector2Int origin = grid.pos;

                    // 合法性校验（你的逻辑不变）
                    if (!IsValidPlacement(partRef, origin, rot))
                    {
                        continue;
                    }

                    // 计算：这个位置对“全局”的效果贡献（核心修复）
                    int score = CalculateGlobalEffectScore(
                        _currentRotatedOccupy: rotatedOccupy,
                        _currentRotatedEffect: rotatedEffect,
                        _origin: origin,
                        _placedParts: _placedParts,
                        _remainingParts: _remainingParts
                    );

                    // 更新最优解
                    if (score > maxTotalScore)
                    {
                        maxTotalScore = score;
                        bestPosList.Clear();
                        bestRotList.Clear();
                        bestPosList.Add(origin);
                        bestRotList.Add(rot);
                    }
                    else if (score == maxTotalScore && score > 0)
                    {
                        bestPosList.Add(origin);
                        bestRotList.Add(rot);
                    }
                }
            }

            // 随机选一个最优解（布局自然）
            if (bestPosList.Count > 0)
            {
                int rnd = Random.Range(0, bestPosList.Count);
                _resultPos = bestPosList[rnd];
                _resultRot = bestRotList[rnd];
                return true;
            }

            // 兜底：真的没位置了才返回false
            return false;
        }

        // 核心新增：格子区域筛选（眼睛强制上半区，其他部位全区域）
        private List<FaceGridInfo> GetFilteredEmptyGrids(PartInfo curPart, List<FaceGridInfo> allFaceGrids)
        {
            // 筛选所有空格里
            List<FaceGridInfo> allEmptyGrids = allFaceGrids.Where(g => !g.hasPart).ToList();
            if (allEmptyGrids.Count == 0) return allEmptyGrids;

            // 非眼睛部位，直接返回全区域空格子
            if (curPart.partRefObj.partType != EPartType.EYE)
            {
                return allEmptyGrids;
            }

            // 眼睛部位：计算脸部Y轴最大值，筛选上半区格子（Y越小越靠上）
            int maxFaceY = allFaceGrids.Max(g => g.pos.y);
            int eyeAreaMaxY = Mathf.FloorToInt(maxFaceY * EYE_TOP_AREA_RATIO);
            // 强制筛选出Y坐标≤上半区阈值的格子（偏上区域）
            List<FaceGridInfo> eyeTopAreaGrids = allEmptyGrids.Where(g => g.pos.y <= eyeAreaMaxY).ToList();

            // 极端情况：上半区无空格子，返回全区域（避免眼睛无法摆放）
            return eyeTopAreaGrids.Count > 0 ? eyeTopAreaGrids : allEmptyGrids;
        }

        // 核心修复：全局效果得分（不仅当前吃别人，还要让未来部位能吃自己）
        private int CalculateGlobalEffectScore(
            List<Vector2Int> _currentRotatedOccupy,
            List<Vector2Int> _currentRotatedEffect,
            Vector2Int _origin,
            List<PartInfo> _placedParts,
            List<PartInfo> _remainingParts)
        {
            int totalScore = 0;
            Vector2Int origin = _origin;

            // 1. 当前部位吃到已摆部位的效果（你的原有得分）
            List<Vector2Int> currentOccupyWorld = new List<Vector2Int>();
            for (int i = 0; i < _currentRotatedOccupy.Count; i++)
            {
                currentOccupyWorld.Add(origin + _currentRotatedOccupy[i]);
            }

            List<Vector2Int> currentEffectWorld = new List<Vector2Int>();
            for (int i = 0; i < _currentRotatedEffect.Count; i++)
            {
                currentEffectWorld.Add(origin + _currentRotatedEffect[i]);
            }

            // 得分A：当前部位被已摆部位覆盖（必须有）
            foreach (PartInfo placed in _placedParts)
            {
                foreach (Vector2Int pos in currentOccupyWorld)
                {
                    if (placed.curEffectFacePosList.Contains(pos))
                    {
                        totalScore += 4; // 权重提高，确保优先
                        break;
                    }
                }
            }

            // 得分B：当前部位覆盖已摆部位（必须有）
            foreach (PartInfo placed in _placedParts)
            {
                foreach (Vector2Int pos in placed.curOccupyFacePosList)
                {
                    if (currentEffectWorld.Contains(pos))
                    {
                        totalScore += 5;
                        break;
                    }
                }
            }

            //核心新增：得分C - 当前部位的效果格子，是否能被“未摆的部位”利用
            //解决“效果格子空着”的问题
            foreach (PartInfo remaining in _remainingParts)
            {
                // 模拟剩余部位的所有可能摆放位置（简化版）
                List<Vector2Int> remainingOccupy = remaining.partRefObj.GetOccupyPosList();
                for (int r = 0; r < 4; r++)
                {
                    List<Vector2Int> remRotOccupy = GameCommon.RotateShapeAndMove2Zero(remainingOccupy, r);
                    // 检查剩余部位是否能放在当前部位的效果格子里
                    foreach (Vector2Int effectPos in currentEffectWorld)
                    {
                        if (remRotOccupy.Contains(effectPos - origin)) // 相对位置
                        {
                            totalScore += 2; // 未来部位能吃到，加分
                            break; // 只要能吃到一次就够了
                        }
                    }
                }
            }

            // 得分D：效果格子不浪费（必须覆盖脸部，避免空效果）
            foreach (Vector2Int effectPos in currentEffectWorld)
            {
                if (enemyFaceGridInfoList.Exists(g => g.pos == effectPos))
                {
                    totalScore += 1; // 效果格子有效，不浪费
                    break;
                }
            }

            return totalScore;
        }

        // 你的原有方法：完全保留
        private void ShuffleGridList(List<FaceGridInfo> _gridList)
        {
            for (int i = _gridList.Count - 1; i > 0; i--)
            {
                int r = Random.Range(0, i + 1);
                (_gridList[i], _gridList[r]) = (_gridList[r], _gridList[i]);
            }
        }

        // 你的原有方法：完全保留
        private bool IsValidPlacement(PartRefObj _part, Vector2Int _originFacePos, int _rotStep)
        {
            List<Vector2Int> shape = GameCommon.RotateShapeAndMove2Zero(_part.GetOccupyPosList(), _rotStep);
            foreach (var offset in shape)
            {
                Vector2Int p = _originFacePos + offset;
                FaceGridInfo gridInfo = enemyFaceGridInfoList.Find(x => x.pos == p);
                if (gridInfo == null || gridInfo.hasPart)
                    return false;
            }
            return true;
        }

        // 你的原有方法：完全保留
        private void MarkOccupancy(PartInfo part, Vector2Int origin, int rot)
        {
            for (int i = 0; i < rot; i++)
                part.RotateOnce();
            foreach (var offset in part.localOccupyPosList)
            {
                Vector2Int p = origin + offset;
                FaceGridInfo gridInfo = enemyFaceGridInfoList.Find(x => x.pos == p);
                if (gridInfo == null) continue;
                gridInfo.SetOwnerPart(part);
                part.curOccupyFacePosList.Add(p);
            }
            foreach (var offset in part.localEffectPosList)
            {
                Vector2Int p = origin + offset;
                part.curEffectFacePosList.Add(p);
            }
            part.isOnFace = true;
            curEnemyInfo.battlePartInfoList.Add(part);
        }

    }
}