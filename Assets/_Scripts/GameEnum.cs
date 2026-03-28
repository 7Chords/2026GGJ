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
        GET_COIN,//获得金币
        GET_COIN_BY_ATTACK,//攻击成功获得金币
        ATTACK_BY_COIN,//根据金币数获得攻击力
        SELF_GET_BUFF,//玩家获得buff
        ENEMY_GET_BUFF,//敌人获得buff
        SELF_BUFF_MULTIPLIER,//玩家BUFF层数翻倍数
        ENEMY_BUFF_MULTIPLIER,// 敌人BUFF层数翻倍数
        CLEAR_SELF_BLEED_AND_HEAL_SELF,// 清除自身流血状态并治疗本体
        CLEAR_ENEMY_BLEED_AND_HEAL_PART,// 清除敌方流血状态并治疗自己（部位）
        ATTACK_BY_ENEMY_BLEED,// 通过敌方流血状态获得攻击力
        SEND_BLEED_BY_GET_HIT,// 被击中时施加敌方流血效果
        CHANGE_FAT_2_BURN,//转化油脂为燃烧
        SPREAD_BURN,//传播范围内最大的燃烧层数（-2且最少1层）
        SEND_ALL_FAT_BY_GET_HIT,//被击中时施加对方自身所有油脂
        INCREASE_ADD_BURN,//增加己方部位提供燃烧效果时的层数
        ENEMY_MOUTH_GET_BUFF,//敌人嘴巴获得buff,
        USE_HEAT_2_ATTACK_AGAIN,//自身强壮超过x层后，消耗超过的强壮概率再次攻击
    }

    public enum EAttributeTriggerPointType
    {
        NONE,
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
        BURN,//燃烧
        STRONG,//强壮
        PREY,//猎物
        HATE,//怨恨
    }

    public enum EEventType
    {
        NONE,
        BLOOD_2_PART_HIGH,
        BLOOD_2_PART_MIDDLE,
        BLOOD_2_PART_LOW,
        PART_2_PART,
        TREASURE_COIN,
        TREASURE_PART,
        TRAP_BATTLE,
    }

    public enum EEventDialogueType
    {
        STANDARD,
        SELECT,
    }

    public enum EEventDialogueFlagType
    {
        NONE,
        BEGIN,
        END
    }
    public enum EBossType
    {
        NONE,
        MALFORMED,//畸形的人
    }

    public enum EBattleType
    {
        NORMAL,
        EVENT,
    }
}