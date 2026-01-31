using GameCore.UI;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            damageGO.GetRectTransform().localPosition = SCUICommon.WorldPointToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                _worldPos);
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
            damageGO.GetRectTransform().localPosition = SCUICommon.WorldPointToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                _worldPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(_healAmount, false);
        }

        public static CommonTooltip ShowTooltip(string _name, string _desc, Vector2 _localPos)
        {
            DiscardToolTip();
            GameObject toolTipGo = ResourcesHelper.LoadGameObject(
                "prefab_tooltip",
                SCGame.instance.topLayerRoot.transform);
                
            RectTransform toolTipRT = toolTipGo.GetRectTransform();
            Vector2 localPoint;
            
            // Get correct camera
            Camera uiCam = null;
            Canvas canvas = toolTipRT.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCam = canvas.worldCamera;
            }
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                toolTipRT.parent as RectTransform,
                _localPos,
                uiCam,
                out localPoint
            );
            
            toolTipRT.localPosition = localPoint;
            
            // Fix: pass LOCAL POINT to ShowTooltip
            var tooltipComp = toolTipGo.GetComponent<CommonTooltip>();
            tooltipComp.ShowTooltip(_name, _desc, localPoint);
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
    }
}
