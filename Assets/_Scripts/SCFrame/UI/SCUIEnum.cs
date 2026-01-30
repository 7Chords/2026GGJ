using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame.UI
{

    //UI显示类型
    public enum SCUIShowType
    {
        NONE,
        FULL,//全屏
        ADDITION,//叠加
        TOP,//顶部 提示栏等
        INTERNAL,//面板内部的小面板
    }


    /// <summary>
    /// 节点功能模块类型
    /// </summary>
    public enum SCUINodeFuncType
    {
        BATTLE,
        STORE,
        MAP,
        COMMON,
    }


}
