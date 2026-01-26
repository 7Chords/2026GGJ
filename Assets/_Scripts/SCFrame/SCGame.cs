using UnityEngine;

namespace SCFrame
{

    /// <summary>
    /// 功能相当于全局Mono
    /// </summary>
    public class SCGame : SingletonPersistent<SCGame>
    {

        [Header("UI")]
        public Canvas mainCanvas;
        public GameObject fullLayerRoot;
        public GameObject additionLayerRoot;
        public GameObject topLayerRoot;

        [Header("Camera")]
        public Camera gameCamera;

    }
}
