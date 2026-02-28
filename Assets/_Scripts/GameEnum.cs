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
        TRIGGER_CHANCE_UP,//触发上升
        HEAL_ALL_PART,//给范围内所有部位回血
        HEAL_WEAK_PART,//给范围内血量最低的部位回血
        CLEAR_DEFULL,//清除负面效果
        CLEAR_BAD_SKIN,//清楚坏的皮肤
        PART_LOSE_TURN,//部位无法行动
        GET_COIN,
        GET_COIN_BY_ATTACK,
        ATTACK_BY_COIN,
        SELF_GET_BUFF,
        ENEMY_GET_BUFF,
        SELF_BUFF_MULTIPLIER,//玩家BUFF层数翻倍数
        ENEMY_BUFF_MULTIPLIER,// 敌人BUFF层数翻倍数
        CLEAR_SELF_BLEED_AND_HEAL_SELF,// 清除自身流血状态并治疗本体
        CLEAR_ENEMY_BLEED_AND_HEAL_PART,// 清除敌方流血状态并治疗自己（部位）
        ATTACK_BY_ENEMY_BLEED,// 通过敌方流血状态获得攻击力
        SEND_BLEED_BY_GET_HIT,// 被击中时施加敌方流血效果
    }

    public enum EAttributeTriggerPointType
    {
        ACTIVE,
        GET_HIT,
        DIE,
        GET_EFFECT,
        TURN_OVER,
        ACTION_OVER,
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
        BLEED,//点燃
        FAT,//油脂
        BURN//燃烧
    }

}