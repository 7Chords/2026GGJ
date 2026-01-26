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

    //确认面板类型
    public enum SCUIConfirmType
    {
        NONE,
        SKILL,
        ITEM
    }

    public enum SCUINodeFuncType
    {
        NONE,
        OW,
        TBS,
        COMMON,
    }


}
