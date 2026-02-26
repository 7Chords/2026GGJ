using GameCore.RefData;
using SCFrame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// 敌人脸部布局生成：按部位类型优先级摆放到格子，支持眼睛偏上、全局效果覆盖等规则。
    /// 从 GameModel_Enemy 抽离，便于单测与复用。
    /// </summary>
    public static class EnemyLayoutGenerator
    {
        private static readonly List<EPartType> PartPlaceOrder = new List<EPartType>
            { EPartType.EYE, EPartType.MOUTH, EPartType.NOSE, EPartType.SKIN };
        private const float EyeTopAreaRatio = 0.5f;

        /// <summary> 将敌人手牌区部位按优先级摆到脸上，并更新 battlePartInfoList 与 faceGrids。最后会排序战斗顺序。 </summary>
        public static void GenerateLayout(EnemyInfo _enemy, List<FaceGridInfo> _faceGrids)
        {
            if (_enemy == null || _faceGrids == null) return;

            var readyToRemove = new List<PartInfo>();
            var orderedParts = new List<PartInfo>();

            foreach (EPartType partType in PartPlaceOrder)
            {
                var targetPart = _enemy.busyPartInfoList.FindAll(p => p.partRefObj.partType == partType);
                if (targetPart != null) orderedParts.AddRange(targetPart);
            }

            for (int i = 0; i < orderedParts.Count; i++)
            {
                PartInfo curPart = orderedParts[i];
                curPart.ResetToBusy();
                bool success = TryFindGlobalOptimalPlacement(
                    curPart,
                    orderedParts.GetRange(0, i),
                    i + 1 < orderedParts.Count ? orderedParts.GetRange(i + 1, orderedParts.Count - i - 1) : new List<PartInfo>(),
                    _faceGrids,
                    out Vector2Int pos,
                    out int rotStep);

                if (success)
                {
                    readyToRemove.Add(curPart);
                    MarkOccupancy(curPart, pos, rotStep, _enemy, _faceGrids);
                }
                else
                {
                    SCDebugHelper.LogWarning($"部位{curPart.partRefObj.partName}无法摆放");
                }
            }

            foreach (var p in readyToRemove)
                _enemy.busyPartInfoList.Remove(p);

            BattleOrderHelper.SortBattleOrder(_enemy.battlePartInfoList);
        }

        private static bool TryFindGlobalOptimalPlacement(
            PartInfo _curPart,
            List<PartInfo> _placedParts,
            List<PartInfo> _remainingParts,
            List<FaceGridInfo> _faceGrids,
            out Vector2Int _resultPos,
            out int _resultRot)
        {
            _resultPos = Vector2Int.zero;
            _resultRot = 0;
            int maxTotalScore = -1;
            var bestPosList = new List<Vector2Int>();
            var bestRotList = new List<int>();
            PartRefObj partRef = _curPart.partRefObj;
            List<FaceGridInfo> filteredEmpty = GetFilteredEmptyGrids(_curPart, _faceGrids);

            for (int rot = 0; rot < 4; rot++)
            {
                List<Vector2Int> rotatedOccupy = GameCommon.RotateShapeAndMove2Zero(partRef.GetOccupyPosList(), rot);
                List<Vector2Int> rotatedEffect = GameCommon.RotateShapeAndMoveBySample(partRef.GetEffectPosList(), rot, partRef.GetOccupyPosList());
                var emptyGrids = new List<FaceGridInfo>(filteredEmpty);
                ShuffleGridList(emptyGrids);

                foreach (FaceGridInfo grid in emptyGrids)
                {
                    Vector2Int origin = grid.pos;
                    if (!IsValidPlacement(partRef, origin, rot, _faceGrids)) continue;

                    int score = CalculateGlobalEffectScore(rotatedOccupy, rotatedEffect, origin, _placedParts, _remainingParts, _faceGrids);
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

            if (bestPosList.Count > 0)
            {
                int rnd = Random.Range(0, bestPosList.Count);
                _resultPos = bestPosList[rnd];
                _resultRot = bestRotList[rnd];
                return true;
            }
            return false;
        }

        private static List<FaceGridInfo> GetFilteredEmptyGrids(PartInfo _curPart, List<FaceGridInfo> _allFaceGrids)
        {
            var allEmpty = _allFaceGrids.Where(g => !g.hasPart).ToList();
            if (allEmpty.Count == 0) return allEmpty;
            if (_curPart.partRefObj.partType != EPartType.EYE) return allEmpty;

            int maxFaceY = _allFaceGrids.Max(g => g.pos.y);
            int eyeAreaMaxY = Mathf.FloorToInt(maxFaceY * EyeTopAreaRatio);
            var eyeTop = allEmpty.Where(g => g.pos.y <= eyeAreaMaxY).ToList();
            return eyeTop.Count > 0 ? eyeTop : allEmpty;
        }

        private static int CalculateGlobalEffectScore(
            List<Vector2Int> _currentRotatedOccupy,
            List<Vector2Int> _currentRotatedEffect,
            Vector2Int _origin,
            List<PartInfo> _placedParts,
            List<PartInfo> _remainingParts,
            List<FaceGridInfo> _faceGrids)
        {
            int totalScore = 0;
            var currentOccupyWorld = _currentRotatedOccupy.Select(o => _origin + o).ToList();
            var currentEffectWorld = _currentRotatedEffect.Select(e => _origin + e).ToList();

            foreach (PartInfo placed in _placedParts)
            {
                foreach (Vector2Int pos in currentOccupyWorld)
                {
                    if (placed.curEffectFacePosList.Contains(pos)) { totalScore += 4; break; }
                }
            }
            foreach (PartInfo placed in _placedParts)
            {
                foreach (Vector2Int pos in placed.curOccupyFacePosList)
                {
                    if (currentEffectWorld.Contains(pos)) { totalScore += 5; break; }
                }
            }
            foreach (PartInfo remaining in _remainingParts)
            {
                List<Vector2Int> remainingOccupy = remaining.partRefObj.GetOccupyPosList();
                for (int r = 0; r < 4; r++)
                {
                    List<Vector2Int> remRotOccupy = GameCommon.RotateShapeAndMove2Zero(remainingOccupy, r);
                    foreach (Vector2Int effectPos in currentEffectWorld)
                    {
                        if (remRotOccupy.Contains(effectPos - _origin)) { totalScore += 2; break; }
                    }
                }
            }
            foreach (Vector2Int effectPos in currentEffectWorld)
            {
                if (_faceGrids.Exists(g => g.pos == effectPos)) { totalScore += 1; break; }
            }
            return totalScore;
        }

        private static void ShuffleGridList(List<FaceGridInfo> _gridList)
        {
            for (int i = _gridList.Count - 1; i > 0; i--)
            {
                int r = Random.Range(0, i + 1);
                (_gridList[i], _gridList[r]) = (_gridList[r], _gridList[i]);
            }
        }

        private static bool IsValidPlacement(PartRefObj _part, Vector2Int _originFacePos, int _rotStep, List<FaceGridInfo> _faceGrids)
        {
            List<Vector2Int> shape = GameCommon.RotateShapeAndMove2Zero(_part.GetOccupyPosList(), _rotStep);
            foreach (var offset in shape)
            {
                Vector2Int p = _originFacePos + offset;
                FaceGridInfo gridInfo = _faceGrids.Find(x => x.pos == p);
                if (gridInfo == null || gridInfo.hasPart) return false;
            }
            return true;
        }

        private static void MarkOccupancy(PartInfo _part, Vector2Int _origin, int _rot, EnemyInfo _enemy, List<FaceGridInfo> _faceGrids)
        {
            for (int i = 0; i < _rot; i++) _part.RotateOnce();
            foreach (var offset in _part.localOccupyPosList)
            {
                Vector2Int p = _origin + offset;
                FaceGridInfo gridInfo = _faceGrids.Find(x => x.pos == p);
                if (gridInfo == null) continue;
                gridInfo.SetOwnerPart(_part);
                _part.curOccupyFacePosList.Add(p);
            }
            foreach (var offset in _part.localEffectPosList)
            {
                _part.curEffectFacePosList.Add(_origin + offset);
            }
            _part.isOnFace = true;
            _enemy.battlePartInfoList.Add(_part);
        }
    }
}
