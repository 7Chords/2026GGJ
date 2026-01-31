using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 封装一些UI通用方法
    /// </summary>
    public static class SCUICommon
    {
        public static Vector2 ScreenPointToUIPoint(RectTransform _rt, Vector2 _screenPoint)
        {
            Vector2 localPoint;


            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rt.parent as RectTransform,
                _screenPoint,
                null,
                out localPoint
            );
            return localPoint;
        }

        public static Vector2 WorldPointToUIPoint(RectTransform _rt, Vector3 _worldPoint)
        {
            Vector2 screenPoint = SCGame.instance.gameCamera.WorldToScreenPoint(_worldPoint);
            return ScreenPointToUIPoint(_rt, screenPoint);
        }

        public static string AddColorForRichText(string txt, Color color)
        {
            string richTextColor = "#" + ColorToString(color);
            return string.Format("<color={0}>{1}</color>", richTextColor, txt);
        }


        private static string ColorToString(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255.0f);
            int g = Mathf.RoundToInt(color.g * 255.0f);
            int b = Mathf.RoundToInt(color.b * 255.0f);
            int a = Mathf.RoundToInt(color.a * 255.0f);
            string hex = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
            return hex;
        }
    }
}
