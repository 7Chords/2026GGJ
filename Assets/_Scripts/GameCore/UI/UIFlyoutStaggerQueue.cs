using System;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using GameCore.UI;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 伤害 / 治疗飘字与效果文本按「运行时部位」分别排队：<see cref="PartInfo"/> 相同则共享一条间隔序列；
    /// 无部位上下文（如整体血条）使用 <c>null</c> 键，走全局一条队列。
    /// 间隔 <see cref="GameConst.UIFLYOUT_STAGGER_INTERVAL"/>（按 key 记录上次弹出时间，避免同一帧内先后入队、泵已结束又开新泵时两条在同一帧显示）。
    /// </summary>
    public static class UIFlyoutStaggerQueue
    {
        sealed class LaneState
        {
            public readonly Queue<Action> queue = new Queue<Action>();
            public bool pumping;
        }

        static readonly Dictionary<object, LaneState> s_lanes = new Dictionary<object, LaneState>(64);
        /// <summary>每条车道上次真正 Instantiate 飘字后的 <see cref="Time.time"/>，跨多次 CoPump 仍生效。</summary>
        static readonly Dictionary<object, float> s_lastInvokeTimeByKey = new Dictionary<object, float>(64);
        static readonly object s_globalLaneKey = new object();
        static readonly object s_coOwner = typeof(UIFlyoutStaggerQueue);

        public static void Enqueue(PartInfo partFlyoutKey, Action show)
        {
            if (show == null)
                return;
            object key = (object)partFlyoutKey ?? s_globalLaneKey;
            if (!s_lanes.TryGetValue(key, out var lane))
            {
                lane = new LaneState();
                s_lanes[key] = lane;
            }
            lane.queue.Enqueue(show);
            if (!lane.pumping)
            {
                lane.pumping = true;
                SCTaskHelper.instance.CreateCoroutine(s_coOwner, CoPump(key), "UIFlyoutStaggerQueue");
            }
        }

        static IEnumerator CoPump(object key)
        {
            if (!s_lanes.TryGetValue(key, out var lane))
                yield break;

            float interval = GameConst.UIFLYOUT_STAGGER_INTERVAL;

            while (true)
            {
                if (lane.queue.Count == 0)
                {
                    lane.pumping = false;
                    s_lanes.Remove(key);
                    yield break;
                }

                var a = lane.queue.Dequeue();

                float now = Time.time;
                if (s_lastInvokeTimeByKey.TryGetValue(key, out float last))
                {
                    float wait = last + interval - now;
                    if (wait > 0f)
                        yield return new WaitForSeconds(wait);
                }

                a?.Invoke();
                s_lastInvokeTimeByKey[key] = Time.time;

                if (lane.queue.Count == 0)
                {
                    lane.pumping = false;
                    s_lanes.Remove(key);
                    yield break;
                }
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
