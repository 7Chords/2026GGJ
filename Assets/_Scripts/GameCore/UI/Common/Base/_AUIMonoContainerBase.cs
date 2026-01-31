using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class _AUIMonoContainerBase : _ASCUIMonoBase
    {
        [Header("Item模板资源obj名")]
        public string prefabItemObjName;
        [Header("布局组件")]
        public LayoutGroup layoutGroup;
        //[Header("item父物体")]
        //public Transform parentTransform;
        //[Header("x间隔")]
        //public float xSpace;
        //[Header("y间隔")]
        //public float ySpace;
        //[Header("水平item数量")]
        //public float xCount;
        //[Header("竖直item数量")]
        //public float yCount;
    }
}

