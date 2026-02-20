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
        ATTACK,//攻击
        REAL_ATTACK,//真实伤害
        REFLECT,//反射
        TRIGGER_MORE,//触发两次
        DAMAGE_MULTIPILER,//攻击多倍伤害
        //HIT_CHANCE_UP,//命中率上升
        //HIT_CHANCE_DOWN,//命中率下降
        TRIGGER_CHANCE_UP,//触发上升
        HEAL_ALL_PART,//给范围内所有部位回血
        HEAL_WEAK_PART,//给范围内血量最低的部位回血
        CLEAR_DEFULL,//清除负面效果
        CLEAR_BAD_SKIN,//清楚坏的皮肤
        PART_LOSE_TURN,//部位无法行动
        GET_COIN,
        GET_DAMAGE_BY_ATTACK,
        ATTACK_BY_COIN,
        SELF_GET_BUFF,
        ENEMT_GET_BUFF,
        SELF_BUFF_MULTIPLIER,
        ENEMY_BUFF_MULTIPLIER,
        CLEAR_SELF_BLEED_AND_HEAL_PLAYER,
        CLEAR_ENEMY_BLEED_AND_HEAL_PART,
        ATTACK_BY_ENEMY_BLEED,
        SEND_BLEED_BY_GET_HIT,
    }

    public enum EAttributeTriggerPointType
    {
        ACTIVE,
        GET_HIT,
        DIE,
        GET_EFFECT,
    }

    public enum ERoomType
    {
        NONE,
        ENEMY,
        SHOP,
        EVENT,
        BOSS,
        TRIAL,
        STRENGTHEN
    }

    public enum EQualityType
    {
        NONE,
        NORMAL,//普通的
        RARE,//稀有的
        PRECIOUS,//珍贵的
    }

    public enum EGridPosType
    {
        OCCUPY,
        EFFECT,
        BOTH,
    }

    public enum ETurnOwnerType
    {
        PLAYER,
        ENEMY,
    }

    public enum EBuffType
    {
        BLEED,
    }

}