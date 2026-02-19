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
        private const float EyeTopAreaRatio = 0.3f;

        /// <summary> 将敌人手牌区部位按优先级摆到脸上，并更新 battlePartInfoList 与 faceGrids。最后会排序战斗顺序。 </summary>
        public static void GenerateLayout(EnemyInfo enemy, List<FaceGridInfo> faceGrids)
        {
            if (enemy == null || faceGrids == null) return;

            var readyToRemove = new List<PartInfo>();
            var orderedParts = new List<PartInfo>();

            foreach (EPartType partType in PartPlaceOrder)
            {
                var targetPart = enemy.busyPartInfoList.FindAll(p => p.partRefObj.partType == partType);
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
                    faceGrids,
                    out Vector2Int pos,
                    out int rotStep);

                if (success)
                {
                    readyToRemove.Add(curPart);
                    MarkOccupancy(curPart, pos, rotStep, enemy, faceGrids);
                }
                else
                {
                    SCDebugHelper.LogWarning($"部位{curPart.partRefObj.partName}无法摆放");
                }
            }

            foreach (var p in readyToRemove)
                enemy.busyPartInfoList.Remove(p);

            BattleOrderHelper.SortBattleOrder(enemy.battlePartInfoList);
        }

        private static bool TryFindGlobalOptimalPlacement(
            PartInfo curPart,
            List<PartInfo> placedParts,
            List<PartInfo> remainingParts,
            List<FaceGridInfo> faceGrids,
            out Vector2Int resultPos,
            out int resultRot)
        {
            resultPos = Vector2Int.zero;
            resultRot = 0;
            int maxTotalScore = -1;
            var bestPosList = new List<Vector2Int>();
            var bestRotList = new List<int>();
            PartRefObj partRef = curPart.partRefObj;
            List<FaceGridInfo> filteredEmpty = GetFilteredEmptyGrids(curPart, faceGrids);

            for (int rot = 0; rot < 4; rot++)
            {
                List<Vector2Int> rotatedOccupy = GameCommon.RotateShapeAndMove2Zero(partRef.GetOccupyPosList(), rot);
                List<Vector2Int> rotatedEffect = GameCommon.RotateShapeAndMoveBySample(partRef.GetEffectPosList(), rot, partRef.GetOccupyPosList());
                var emptyGrids = new List<FaceGridInfo>(filteredEmpty);
                ShuffleGridList(emptyGrids);

                foreach (FaceGridInfo grid in emptyGrids)
                {
                    Vector2Int origin = grid.pos;
                    if (!IsValidPlacement(partRef, origin, rot, faceGrids)) continue;

                    int score = CalculateGlobalEffectScore(rotatedOccupy, rotatedEffect, origin, placedParts, remainingParts, faceGrids);
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
                resultPos = bestPosList[rnd];
                resultRot = bestRotList[rnd];
                return true;
            }
            return false;
        }

        private static List<FaceGridInfo> GetFilteredEmptyGrids(PartInfo curPart, List<FaceGridInfo> allFaceGrids)
        {
            var allEmpty = allFaceGrids.Where(g => !g.hasPart).ToList();
            if (allEmpty.Count == 0) return allEmpty;
            if (curPart.partRefObj.partType != EPartType.EYE) return allEmpty;

            int maxFaceY = allFaceGrids.Max(g => g.pos.y);
            int eyeAreaMaxY = Mathf.FloorToInt(maxFaceY * EyeTopAreaRatio);
            var eyeTop = allEmpty.Where(g => g.pos.y <= eyeAreaMaxY).ToList();
            return eyeTop.Count > 0 ? eyeTop : allEmpty;
        }

        private static int CalculateGlobalEffectScore(
            List<Vector2Int> currentRotatedOccupy,
            List<Vector2Int> currentRotatedEffect,
            Vector2Int origin,
            List<PartInfo> placedParts,
            List<PartInfo> remainingParts,
            List<FaceGridInfo> faceGrids)
        {
            int totalScore = 0;
            var currentOccupyWorld = currentRotatedOccupy.Select(o => origin + o).ToList();
            var currentEffectWorld = currentRotatedEffect.Select(e => origin + e).ToList();

            foreach (PartInfo placed in placedParts)
            {
                foreach (Vector2Int pos in currentOccupyWorld)
                {
                    if (placed.curEffectFacePosList.Contains(pos)) { totalScore += 4; break; }
                }
            }
            foreach (PartInfo placed in placedParts)
            {
                foreach (Vector2Int pos in placed.curOccupyFacePosList)
                {
                    if (currentEffectWorld.Contains(pos)) { totalScore += 5; break; }
                }
            }
            foreach (PartInfo remaining in remainingParts)
            {
                List<Vector2Int> remainingOccupy = remaining.partRefObj.GetOccupyPosList();
                for (int r = 0; r < 4; r++)
                {
                    List<Vector2Int> remRotOccupy = GameCommon.RotateShapeAndMove2Zero(remainingOccupy, r);
                    foreach (Vector2Int effectPos in currentEffectWorld)
                    {
                        if (remRotOccupy.Contains(effectPos - origin)) { totalScore += 2; break; }
                    }
                }
            }
            foreach (Vector2Int effectPos in currentEffectWorld)
            {
                if (faceGrids.Exists(g => g.pos == effectPos)) { totalScore += 1; break; }
            }
            return totalScore;
        }

        private static void ShuffleGridList(List<FaceGridInfo> gridList)
        {
            for (int i = gridList.Count - 1; i > 0; i--)
            {
                int r = Random.Range(0, i + 1);
                (gridList[i], gridList[r]) = (gridList[r], gridList[i]);
            }
        }

        private static bool IsValidPlacement(PartRefObj part, Vector2Int originFacePos, int rotStep, List<FaceGridInfo> faceGrids)
        {
            List<Vector2Int> shape = GameCommon.RotateShapeAndMove2Zero(part.GetOccupyPosList(), rotStep);
            foreach (var offset in shape)
            {
                Vector2Int p = originFacePos + offset;
                FaceGridInfo gridInfo = faceGrids.Find(x => x.pos == p);
                if (gridInfo == null || gridInfo.hasPart) return false;
            }
            return true;
        }

        private static void MarkOccupancy(PartInfo part, Vector2Int origin, int rot, EnemyInfo enemy, List<FaceGridInfo> faceGrids)
        {
            for (int i = 0; i < rot; i++) part.RotateOnce();
            foreach (var offset in part.localOccupyPosList)
            {
                Vector2Int p = origin + offset;
                FaceGridInfo gridInfo = faceGrids.Find(x => x.pos == p);
                if (gridInfo == null) continue;
                gridInfo.SetOwnerPart(part);
                part.curOccupyFacePosList.Add(p);
            }
            foreach (var offset in part.localEffectPosList)
            {
                part.curEffectFacePosList.Add(origin + offset);
            }
            part.isOnFace = true;
            enemy.battlePartInfoList.Add(part);
        }
    }
}
