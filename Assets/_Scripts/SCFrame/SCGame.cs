using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SCFrame
{

    /// <summary>
    /// �����൱��ȫ��Mono
    /// </summary>
    public class SCGame : SingletonPersistent<SCGame>
    {
        protected override void Awake()
        {
            base.Awake();
            // Avoid DOTween auto capacity growth warnings during heavy UI transitions.
            // Called once because SCGame is persistent.
            DOTween.SetTweensCapacity(1000, 200);
        }


        [Header("UI")]
        public Canvas mainCanvas;
        public GameObject fullLayerRoot;
        public GameObject additionLayerRoot;
        public GameObject topLayerRoot;

        [Header("Camera")]
        public Camera gameCamera;

        [Header("Volumn")]
        public Volume globalVolumn;
        public UniversalRendererData rendererData;

    }
}
