using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Helpers
{
    /// <summary>
    /// Per-pixel alpha UI raycasts: Unity only allows non-zero <see cref="Image.alphaHitTestMinimumThreshold"/>
    /// when the sprite texture is CPU-readable or uses Crunch compression (see UnityEngine.UI.Image setter).
    /// Otherwise skip (reset to 0) to avoid InvalidOperationException.
    /// </summary>
    public static class PartSpriteRaycastHelper
    {
        public const float AlphaHitTestMinimumThreshold = 0.01f;

        public static void ApplyToPartImage(Image image)
        {
            if (image == null)
                return;
            if (CanUseAlphaHitTest(image))
                image.alphaHitTestMinimumThreshold = AlphaHitTestMinimumThreshold;
            else
                image.alphaHitTestMinimumThreshold = 0f;
        }

        public static void ApplyToPartImages(Image a, Image b)
        {
            ApplyToPartImage(a);
            ApplyToPartImage(b);
        }

        /// <summary> Matches Unity UGUI rules for when alpha hit threshold may be set. </summary>
        public static bool CanUseAlphaHitTest(Image image)
        {
            if (image == null)
                return false;
            Sprite s = image.overrideSprite != null ? image.overrideSprite : image.sprite;
            return CanUseAlphaHitTest(s);
        }

        public static bool CanUseAlphaHitTest(Sprite sprite)
        {
            if (sprite == null)
                return false;
            Texture2D tex = sprite.texture;
            if (tex == null)
                return false;
            if (tex.isReadable)
                return true;
            return TextureUsesCrunchCompression(tex);
        }

        private static bool TextureUsesCrunchCompression(Texture2D tex)
        {
            // Unity allows alpha hit test for Crunch without Read/Write; format names include "Crunched".
            string fmt = tex.format.ToString();
            return fmt.IndexOf("Crunched", StringComparison.Ordinal) >= 0;
        }
    }
}
