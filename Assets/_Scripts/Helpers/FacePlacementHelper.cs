using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// 脸部格子上的摆放逻辑：根据拖拽/占格计算世界占格与效果格坐标、是否可放置。纯计算，不依赖 GameModel。
    /// </summary>
    public static class FacePlacementHelper
    {
        /// <summary>
        /// 根据命中的格子、鼠标位置和部位本地占格，计算在脸上的占格世界坐标列表。
        /// 若转换失败返回 null。
        /// </summary>
        public static List<Vector2Int> GetPlaceFaceOccupyPosList(
            GameObject _hitGridGO,
            Vector3 _mousePos,
            List<Vector2Int> _localGridList,
            List<FaceGridInfo> _gridInfoList,
            List<GameObject> _gridGOList,
            Camera _camera)
        {
            if (_gridInfoList == null || _gridGOList == null || _localGridList == null || _localGridList.Count == 0)
                return null;
            RectTransform gridRect = _hitGridGO.GetComponent<RectTransform>();
            if (gridRect == null) return null;

            Vector2 gridSize = new Vector2(gridRect.rect.width, gridRect.rect.height);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect, _mousePos, _camera, out Vector2 pixelPosInGrid))
                return null;

            pixelPosInGrid += new Vector2(gridSize.x / 2f, gridSize.y / 2f);
            Vector2 ratio = new Vector2(pixelPosInGrid.x / gridSize.x, 1 - pixelPosInGrid.y / gridSize.y);

            Vector2 localCenterPos = GameCommon.CalculateGridCenterPos(_localGridList);
            Vector2Int hitAsLocalGridPos;
            if (ratio.x < 0.5f && ratio.y < 0.5f)
                hitAsLocalGridPos = new Vector2Int(Mathf.CeilToInt(localCenterPos.x), Mathf.CeilToInt(localCenterPos.y));
            else if (ratio.x >= 0.5f && ratio.y < 0.5f)
                hitAsLocalGridPos = new Vector2Int(Mathf.FloorToInt(localCenterPos.x), Mathf.CeilToInt(localCenterPos.y));
            else if (ratio.x >= 0.5f && ratio.y > 0.5f)
                hitAsLocalGridPos = new Vector2Int(Mathf.FloorToInt(localCenterPos.x), Mathf.FloorToInt(localCenterPos.y));
            else
                hitAsLocalGridPos = new Vector2Int(Mathf.CeilToInt(localCenterPos.x), Mathf.FloorToInt(localCenterPos.y));

            int goIndex = _gridGOList.IndexOf(_hitGridGO);
            if (goIndex < 0 || goIndex >= _gridInfoList.Count) return null;

            Vector2Int origin = _gridInfoList[goIndex].pos;
            List<Vector2Int> retList = new List<Vector2Int>();
            for (int i = 0; i < _localGridList.Count; i++)
                retList.Add((_localGridList[i] - hitAsLocalGridPos) + origin);
            return retList;
        }

        /// <summary> 根据占格与效果格的本地/世界对应关系，计算效果格的世界坐标列表。 </summary>
        public static List<Vector2Int> GetPlaceFaceEffectPosList(
            List<Vector2Int> _localEffectPosList,
            List<Vector2Int> _faceOccupyPosList,
            List<Vector2Int> _localOccupyPosList)
        {
            if (_localEffectPosList == null || _faceOccupyPosList == null || _localOccupyPosList == null || _localOccupyPosList.Count == 0)
                return null;
            Vector2Int offset = _faceOccupyPosList[0] - _localOccupyPosList[0];
            var retList = new List<Vector2Int>(_localEffectPosList.Count);
            for (int i = 0; i < _localEffectPosList.Count; i++)
                retList.Add(new Vector2Int(_localEffectPosList[i].x + offset.x, _localEffectPosList[i].y + offset.y));
            return retList;
        }

        /// <summary> 仅根据目标占格坐标列表与当前格子信息，判断是否可放置（不越界且无占用）。 </summary>
        public static bool CanPlacePart(List<Vector2Int> _faceOccupyPosList, List<FaceGridInfo> _gridInfoList)
        {
            if (_faceOccupyPosList == null || _gridInfoList == null) return false;
            for (int i = 0; i < _faceOccupyPosList.Count; i++)
            {
                FaceGridInfo info = _gridInfoList.Find(x => x.pos == _faceOccupyPosList[i]);
                if (info == null || info.hasPart) return false;
            }
            return true;
        }

        /// <summary> 结合命中格子与鼠标位置，先算占格再判断是否可放置。 </summary>
        public static bool CanPlacePart(
            GameObject _hitGridGO,
            Vector3 _mousePos,
            List<Vector2Int> _localGridList,
            List<FaceGridInfo> _gridInfoList,
            List<GameObject> _gridGOList,
            Camera _camera)
        {
            List<Vector2Int> facePosList = GetPlaceFaceOccupyPosList(_hitGridGO, _mousePos, _localGridList, _gridInfoList, _gridGOList, _camera);
            return facePosList != null && CanPlacePart(facePosList, _gridInfoList);
        }
    }
}
