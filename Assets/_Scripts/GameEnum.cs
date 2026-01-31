using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public enum EPartType
    {
        EYE,
        NOSE,
        MOUTH,
    }

    public enum EGoodsType
    {
        PART,//部位
        HEAL,//回血
    }

    public enum EAttributeType
    {
        ATTACK,//攻击力
        CRITICAL_CHANCE,//暴击率
        HIT_CHANCE,//命中率
        DEFEND,//护盾
        HEALTH,//回血
        REFLECT,//反射
        SUCK,//吸血
    }
}