using System;
using System.Collections;
using System.Collections.Generic;
using GameCore.UI;
using SCFrame;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 飘字 / PopTip 入队，按 <see cref="GameConst.UIFLYOUT_STAGGER_INTERVAL"/> 间隔依次播放，避免同一帧内全部叠在一起。
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

        public static void ShowPopTipImmediate(string content, Vector3 worldPos)
        {
            GameObject popTipGo = ResourcesHelper.LoadGameObject(
                GameConst.PREFAB_POP_TIP,
                SCGame.instance.topLayerRoot.transform);

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(SCGame.instance.gameCamera, worldPos);
            RectTransform rt = popTipGo.GetRectTransform();
            Vector2 localPoint = SCUICommon.ScreenPointToUIPoint(rt, screenPos);
            rt.localPosition = localPoint;
            var popTipComp = popTipGo.GetComponent<CommonPopTip>();
            popTipComp.Initialize(content);
        }

        public static void ShowPopTipImmediate(string content, Vector2 uiLocalPos)
        {
            GameObject popTipGo = ResourcesHelper.LoadGameObject(
                GameConst.PREFAB_POP_TIP,
                SCGame.instance.topLayerRoot.transform);

            RectTransform rt = popTipGo.GetRectTransform();
            rt.localPosition = uiLocalPos;
            var popTipComp = popTipGo.GetComponent<CommonPopTip>();
            popTipComp.Initialize(content);
        }

        #endregion
    }
}
