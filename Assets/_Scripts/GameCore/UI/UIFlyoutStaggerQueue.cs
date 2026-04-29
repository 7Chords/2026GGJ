using System;
using System.Collections;
using System.Collections.Generic;
using GameCore.UI;
using SCFrame;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 仅 <c>prefab_damage_num</c>（伤害飘字）与 <c>prefab_effect_text</c> 入队，按 <see cref="GameConst.UIFLYOUT_STAGGER_INTERVAL"/> 间隔依次出现；PopTip / 治疗飘字不经过此队列。
    /// </summary>
    public static class UIFlyoutStaggerQueue
    {
        static readonly Queue<Action> s_queue = new Queue<Action>();
        static bool s_pumping;
        static readonly object s_coOwner = typeof(UIFlyoutStaggerQueue);

        public static void Enqueue(Action show)
        {
            if (show == null)
                return;
            s_queue.Enqueue(show);
            if (!s_pumping)
                SCTaskHelper.instance.CreateCoroutine(s_coOwner, CoPump(), "UIFlyoutStaggerQueue");
        }

        static IEnumerator CoPump()
        {
            s_pumping = true;
            while (true)
            {
                if (s_queue.Count == 0)
                {
                    s_pumping = false;
                    yield break;
                }

                var a = s_queue.Dequeue();
                a?.Invoke();

                if (s_queue.Count == 0)
                {
                    s_pumping = false;
                    yield break;
                }

                yield return new WaitForSeconds(GameConst.UIFLYOUT_STAGGER_INTERVAL);
            }
        }

        #region Immediate spawn (original GameCommon logic)

        public static void ShowDamageFloatTextImmediate(int damage, Vector3 worldPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.UIWorldToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                worldPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(damage, true);
        }

        public static void ShowDamageFloatTextImmediate(int damage, Vector2 screenPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.ScreenPointToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(damage, true);
        }

        public static void ShowDamageFloatTextImmediate(int damage, Transform anchor)
        {
            Transform parent = anchor != null ? anchor.parent.parent : SCGame.instance.topLayerRoot.transform;
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                parent);

            damageGO.transform.localPosition = Vector3.zero;
            damageGO.transform.localScale = Vector3.one;
            damageGO.transform.localRotation = Quaternion.identity;
            damageGO.transform.SetParent(anchor.parent.parent.parent, true);
            damageGO.GetComponent<DamageFloatText>().Initialize(damage, true);
        }

        public static void ShowHealFloatTextImmediate(int healAmount, Vector3 worldPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.UIWorldToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                worldPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(healAmount, false);
        }

        public static void ShowHealFloatTextImmediate(int healAmount, Vector2 screenPos)
        {
            GameObject damageGO = ResourcesHelper.LoadGameObject(
                "prefab_damage_num",
                SCGame.instance.topLayerRoot.transform);
            damageGO.GetRectTransform().localPosition = SCUICommon.ScreenPointToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                screenPos);
            damageGO.GetComponent<DamageFloatText>().Initialize(healAmount, false);
        }

        public static void ShowEffectTextImmediate(string content, Vector3 worldPos)
        {
            GameObject go = ResourcesHelper.LoadGameObject(
                "prefab_effect_text",
                SCGame.instance.topLayerRoot.transform);
            go.GetRectTransform().localPosition = SCUICommon.UIWorldToUIPoint(
                SCGame.instance.topLayerRoot.GetRectTransform(),
                worldPos);
            go.GetComponent<PartEffectText>().Initialize(content);
        }

        #endregion
    }
}
