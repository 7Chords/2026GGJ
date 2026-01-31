using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIMonoEnemyMask : _ASCUIMonoBase
    {
        [Header("�����б�")]
        public List<UIMonoEnemyMaskGrid> monoGridList;

        public GameObject gridPrefab;
        [Header("关闭按钮")]
        public UnityEngine.UI.Button btnClose;
        [Header("Grid内容父节点")]
        public Transform content_grid;
    }
}

