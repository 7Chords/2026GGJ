using GameCore.RefData;
using GameCore.UI;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore
{
    /// <summary>
    /// 封装游戏通用方法
    /// </summary>
    public static class GameCommon
    {

        private static GameObject _m_toolTipCache;
        /// <summary>
        /// 展示伤害飘字
        /// </summary>
        public static void ShowDamageFloatText(int _damage, Vector3 _worldPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.UIWorldToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                _worldPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(_damage, true);
        }
        /// <summary>
        /// 展示伤害飘字
        /// </summary>
        public static void ShowDamageFloatText(int _damage, Vector2 _screenPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.ScreenPointToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                _screenPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(_damage, true);
        }
        /// <summary>
        /// 展示伤害飘字
        /// </summary>
        public static void ShowDamageFloatText(int _damage, Transform _anchor)
        {
            Transform parent = _anchor != null ? _anchor.parent.parent : SCGame.instance.topLayerRoot.transform;
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                parent);
            
            damageGO.transform.localPosition = Vector3.zero;
            damageGO.transform.localScale = Vector3.one; // Ensure scale is reset
            damageGO.transform.localRotation = Quaternion.identity; // Ensure rotation matches parent or is reset? 
            // If parent is rotated, text will rotate. This might be desired or not.
            // User requested "Parent to part", usually implies following its transform.
            damageGO.transform.SetParent(_anchor.parent.parent.parent,true);
            damageGO.GetComponent<DamageFloatText>().Initialize(_damage, true);
        }
        /// <summary>
        /// 展示治疗量飘字
        /// </summary>
        public static void ShowHealFloatText(int _healAmount, Vector3 _worldPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.UIWorldToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                _worldPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(_healAmount, false);
        }
        /// <summary>
        /// 展示治疗量飘字
        /// </summary>
        public static void ShowHealFloatText(int _healAmount, Vector2 _screenPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.ScreenPointToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                _screenPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(_healAmount, false);
        }

        /// <summary>
        /// 展示效果文本
        /// </summary>
        public static void ShowEffectText(string _content, Vector3 _worldPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_effect_text",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.UIWorldToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                _worldPos);
            damageGO.GetComponent<PartEffectText>().Initialize(_content);
        }

        public static CommonTooltip ShowTooltip(string _name, string _desc, Vector3 _worldPos, EQualityType _qualityType = EQualityType.NONE, bool _showGridInfo = true)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);

            Vector2 screenPos = Vector2.zero;

            float itemScreenX = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos).x;
            float itemScreenY = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos).y;

            bool showOnLeft = itemScreenX > Screen.width * GameConst.TOOLTIP_SHOW_ON_LEFT_THRESHOLD;
            bool showOnUp = itemScreenY < Screen.height * GameConst.TOOLTIP_SHOW_ON_UP_THRESHOLD;

            Vector2 offset = new Vector3(GameConst.TOOLTIP_SHOW_X_OFFSET_SCREEN_RATIO * Screen.width * (showOnLeft?-1:1),
                GameConst.TOOLTIP_SHOW_Y_OFFSET_SCREEN_RATIO * Screen.height * (showOnUp ? -1 : 1));
            screenPos = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos) + offset;

            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos,
                SCGame.instance.gameCamera,
                out localPoint
            );
            
            toolTipRT.localPosition = localPoint;
            
            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_name, _desc, localPoint, _qualityType,_showGridInfo);
            _m_toolTipCache = toolTipGo;
            return tooltipComp;
        }
        public static CommonTooltip ShowTooltip(string _name, string _desc, Vector3 _worldPos, Vector2 _showScreenRatioOffset, EQualityType _qualityType = EQualityType.NONE,bool _showGridInfo = true)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);

            Vector2 offset = new Vector3(_showScreenRatioOffset.x * Screen.width, _showScreenRatioOffset.y * Screen.height);
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos) + offset;

            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos,
                SCGame.instance.gameCamera,
                out localPoint
            );

            toolTipRT.localPosition = localPoint;

            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_name, _desc, localPoint, _qualityType, _showGridInfo);
            _m_toolTipCache = toolTipGo;
            return tooltipComp;
        }
        public static CommonTooltip ShowTooltip(string _name, string _desc, Vector2 _screenRatio, EQualityType _qualityType = EQualityType.NONE, bool _showGridInfo = true)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);

            Vector2 screenPos = new Vector2(Screen.width * _screenRatio.x, Screen.height * _screenRatio.y);

            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos,
                SCGame.instance.gameCamera,
                out localPoint
            );

            toolTipRT.localPosition = localPoint;

            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_name, _desc, localPoint, _qualityType,_showGridInfo);
            _m_toolTipCache = toolTipGo;
            return tooltipComp;
        }
        public static CommonTooltip ShowTooltip(PartInfo _partInfo, Vector2 _screenRatio, bool _showGridInfo = true)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);

            Vector2 screenPos = new Vector2(Screen.width * _screenRatio.x, Screen.height * _screenRatio.y);

            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos,
                SCGame.instance.gameCamera,
                out localPoint
            );

            toolTipRT.localPosition = localPoint;

            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_partInfo, localPoint, _showGridInfo);
            _m_toolTipCache = toolTipGo;
            return tooltipComp;
        }
        public static CommonTooltip ShowTooltip(PartInfo _partInfo, Vector3 _worldPos, bool _showGridInfo = true)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);

            Vector2 screenPos = Vector2.zero;

            float itemScreenX = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos).x;
            float itemScreenY = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos).y;

            bool showOnLeft = itemScreenX > Screen.width * GameConst.TOOLTIP_SHOW_ON_LEFT_THRESHOLD;
            bool showOnUp = itemScreenY < Screen.height * GameConst.TOOLTIP_SHOW_ON_UP_THRESHOLD;

            Vector2 offset = new Vector3(GameConst.TOOLTIP_SHOW_X_OFFSET_SCREEN_RATIO * Screen.width * (showOnLeft ? -1 : 1),
                GameConst.TOOLTIP_SHOW_Y_OFFSET_SCREEN_RATIO * Screen.height * (showOnUp ? -1 : 1));
            screenPos = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos) + offset;

            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos,
                SCGame.instance.gameCamera,
                out localPoint
            );

            toolTipRT.localPosition = localPoint;

            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_partInfo, localPoint, _showGridInfo);
            _m_toolTipCache = toolTipGo;
            return tooltipComp;
        }

        public static CommonTooltip ShowTooltip(PartRefObj _partRefObj, Vector2 _screenRatio, bool _showGridInfo = true)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);

            Vector2 screenPos = new Vector2(Screen.width * _screenRatio.x, Screen.height * _screenRatio.y);

            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos,
                SCGame.instance.gameCamera,
                out localPoint
            );

            toolTipRT.localPosition = localPoint;

            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_partRefObj, localPoint, _showGridInfo);
            _m_toolTipCache = toolTipGo;
            return tooltipComp;
        }

        public static CommonTooltip ShowTooltip(PartRefObj _partRefObj, Vector3 _worldPos, bool _showGridInfo = true)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);

            Vector2 screenPos = Vector2.zero;

            float itemScreenX = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos).x;
            float itemScreenY = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos).y;

            bool showOnLeft = itemScreenX > Screen.width * GameConst.TOOLTIP_SHOW_ON_LEFT_THRESHOLD;
            bool showOnUp = itemScreenY < Screen.height * GameConst.TOOLTIP_SHOW_ON_UP_THRESHOLD;

            Vector2 offset = new Vector3(GameConst.TOOLTIP_SHOW_X_OFFSET_SCREEN_RATIO * Screen.width * (showOnLeft ? -1 : 1),
                GameConst.TOOLTIP_SHOW_Y_OFFSET_SCREEN_RATIO * Screen.height * (showOnUp ? -1 : 1));
            screenPos = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, _worldPos) + offset;

            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos,
                SCGame.instance.gameCamera,
                out localPoint
            );

            toolTipRT.localPosition = localPoint;

            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_partRefObj, localPoint, _showGridInfo);
            _m_toolTipCache = toolTipGo;
            return tooltipComp;
        }
        public static void DiscardToolTip()
        {
            if (_m_toolTipCache == null)
                return;
            _m_toolTipCache.GetComponent<CommonTooltip>().Discard();
            _m_toolTipCache = null;
        }



        /// <summary>
        /// 绕着中心点旋转
        /// </summary>
        /// <param name="_originalPos"></param>
        /// <param name="_centerPos"></param>
        /// <param name="_rotateStep"></param>
        /// <returns></returns>
        public static Vector2Int RotateAroundCenter(Vector2Int _originalPos, Vector2 _centerPos,int _rotateStep)
        {
            Vector2 vector = _originalPos - _centerPos;
            for (int i = 0; i < _rotateStep; i++)
            {
                vector = new Vector2(vector.y, -vector.x);
            }
            Vector2Int ret = new Vector2Int((int)(vector.x + _centerPos.x), (int)(vector.y + _centerPos.y));
            return ret;
        }


        public static Vector2 ScreenPoint2UILocalPoint(RectTransform _parentTran,Vector2 _screentPoint)
        {
            Vector2 localPoint = Vector2.zero;
            Camera uiCam = null;
            Canvas canvas = _parentTran.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCam = canvas.worldCamera;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentTran, _screentPoint, uiCam, out localPoint);
            return localPoint;
        }

        public static Vector2 CalculateWorldCenterPos(List<Vector2> _occupyPosList)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var pos in _occupyPosList)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            return new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
        }

        public static Vector2 CalculateStandardCenterPos(List<Vector3> _occupyPosList)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var pos in _occupyPosList)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            return new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
        }


        public static Vector2 CalculateGridCenterPos(List<Vector2Int> _occupyPosList)
        {
            Vector2 dealPos = new Vector2(CalculateBounds(_occupyPosList).x - 1, CalculateBounds(_occupyPosList).y - 1);
            return dealPos / 2f;
        }

        public static Vector2 CalculateBounds(List<Vector2Int> _occupyPosList)
        {
            if (_occupyPosList == null || _occupyPosList.Count == 0) return Vector2.zero;

            //找包围盒的最小/最大x/y
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (var pos in _occupyPosList)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            int boundsWidth = maxX - minX + 1; // 宽度（格子数）
            int boundsHeight = maxY - minY + 1; // 高度（格子数）

            return new Vector2(boundsWidth, boundsHeight);
        }

        /// <summary>
        /// 逆时针旋转格子形状
        /// </summary>
        public static List<Vector2Int> RotateShape(List<Vector2Int> _originalPoints, int _step)
        {
            if (_originalPoints == null || _originalPoints.Count == 0)
                return new List<Vector2Int>();

            //找到当前最上最左点（minX, minY）
            int minX = _originalPoints.Min(p => p.x);
            int minY = _originalPoints.Min(p => p.y);

            //平移到本地坐标系（00 是最上最左）
            List<Vector2Int> localPoints = _originalPoints.Select(p => new Vector2Int(p.x - minX, p.y - minY)).ToList();

            //逆时针旋转
            List<Vector2Int> rotated = localPoints.Select(p => RotatePoint(p, _step)).ToList();

            return rotated;
        }

        /// <summary>
        /// 逆时针旋转格子形状
        /// 旋转后把最上最左点重置为 (0,0)
        /// X 右正，Y 下正
        /// </summary>
        public static List<Vector2Int> RotateShapeAndMove2Zero(List<Vector2Int> _originalPoints, int _step)
        {
            if (_originalPoints == null || _originalPoints.Count == 0)
                return new List<Vector2Int>();

            //逆时针旋转
            List<Vector2Int> rotated = RotateShape(_originalPoints, _step);

            //再次找到新的最上最左，平移回 00
            int newMinX = rotated.Min(p => p.x);
            int newMinY = rotated.Min(p => p.y);

            var result = rotated.Select(p => new Vector2Int(p.x - newMinX, p.y - newMinY)).ToList();

            return result.Distinct().ToList();
        }

        /// <summary>
        /// 逆时针旋转格子形状
        /// 旋转后根据参考的旋转列表归0所需要的偏移 进行格子偏移 用于效果格子的旋转
        /// </summary>
        public static List<Vector2Int> RotateShapeAndMoveBySample(List<Vector2Int> _originalPoints, int _step, List<Vector2Int> _sampleList)
        {
            if (_originalPoints == null || _originalPoints.Count == 0)
                return new List<Vector2Int>();
            //逆时针旋转
            List<Vector2Int> rotated = _originalPoints.Select(p => RotatePoint(p, _step)).ToList();

            List<Vector2Int> originalSampleRotateList = RotateShape(_sampleList, _step);
            List<Vector2Int> dealSampleRotateList = RotateShapeAndMove2Zero(_sampleList, _step);
            Vector2Int sampleMove = dealSampleRotateList[0] - originalSampleRotateList[0];

            List<Vector2Int> ret = rotated.Select(p => new Vector2Int(p.x + sampleMove.x,p.y + sampleMove.y)).ToList();


            return ret;
        }




        /// <summary>
        /// 逆时针旋转公式 y轴以下为正 因为游戏里的设计
        /// </summary>
        private static Vector2Int RotatePoint(Vector2Int _p, int _step)
        {
            return _step switch
            {
                1 => new Vector2Int(_p.y, -_p.x),   // 逆时针 90
                2 => new Vector2Int(-_p.x, -_p.y), // 180
                3 => new Vector2Int(-_p.y, _p.x),   // 逆时针 270
                _ => _p
            };
        }

        public static GameObject GetHitGridGameObj(PointerEventData _eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventData, results);
            foreach (var result in results)
            {
                if (result.gameObject.tag == GameConst.FACE_GRID_TAG)
                {
                    return result.gameObject;
                }
            }
            return null;
        }
    }
}
