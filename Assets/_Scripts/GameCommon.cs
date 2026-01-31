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
    }
}
