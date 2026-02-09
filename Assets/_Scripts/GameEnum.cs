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
        SKIN,
    }

    public enum EGoodsType
    {
        PART,//部位
        HEAL,//回血
    }

    public enum EAttributeType
    {
        ATTACK,//攻击力
        HIT_CHANCE,//命中率
        REFLECT,//反射
        TRIGGER_DOUBLE,//触发两次
        ATTACK_DOUBLE,//攻击两次
        HIT_CHANCE_UP,//命中率上升
        HIT_CHANCE_DOWN,//命中率下降
        TRIGGER_CHANCE_UP,//触发上升
        HEAL_PART,//给部位回血
        CLEAR_DEFULL,//清除负面效果
        CLEAR_BAD_SKIN,//清楚坏的皮肤
        PENETRATE,//穿透
        PART_LOSE_TURN,//部位无法行动
    }
    public enum ERoomType
    {
        NONE,
        ENEMY,
        SHOP,
        EVENT,
        BOSS,
        TRIAL,
    }

    public enum EQualityType
    {
        NORMAL,//普通的
        RARE,//稀有的
        PRECIOUS,//珍贵的
    }

    //public enum EGridPosType
    //{
    //    OCCUPY,
    //    EFFECT,
    //}
}