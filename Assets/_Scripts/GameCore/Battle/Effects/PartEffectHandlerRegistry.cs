using System;
using System.Collections.Generic;

namespace GameCore.Battle
{
    /// <summary>
    /// 效果处理器注册表：按 EAttributeType 查找处理器，新增效果只需注册，无需改 PartLogicFactory。
    /// </summary>
    public static class PartEffectHandlerRegistry
    {
        private static readonly Dictionary<EAttributeType, IPartEffectHandler> _m_handlerDict = new Dictionary<EAttributeType, IPartEffectHandler>();

        static PartEffectHandlerRegistry()
        {
            RegisterHandlers();
        }

        private static void RegisterHandlers()
        {
            Register(EAttributeType.CLEAR_DEFULL, new Effects.ClearDebuffEffectHandler());
            Register(EAttributeType.ATTACK, new Effects.AttackEffectHandler());
            Register(EAttributeType.REAL_ATTACK, new Effects.RealAttackEffectHandler());
            Register(EAttributeType.REFLECT, new Effects.ReflectEffectHandler());
            Register(EAttributeType.TRIGGER_MORE, new Effects.TriggerMoreEffectHandler());
            Register(EAttributeType.DAMAGE_MULTIPILER, new Effects.DamageMultiplierEffectHandler());
            Register(EAttributeType.HEAL_ALL_PART, new Effects.HealPartEffectHandler());
            Register(EAttributeType.PART_LOSE_TURN, new Effects.PartLoseTurnEffectHandler());
            Register(EAttributeType.SELF_GET_BUFF, new Effects.SelfGetBuffEffectHandler());
            Register(EAttributeType.SELF_MOUTH_GET_BUFF, new Effects.SelfMouthGetBuffEffectHandler());
            Register(EAttributeType.ENEMY_GET_BUFF, new Effects.EnemyGetBuffEffectHandler());
            Register(EAttributeType.ENEMY_MOUTH_GET_BUFF, new Effects.EnemyMouthGetBuffEffectHandler());
            Register(EAttributeType.USE_HEAT_2_ATTACK_AGAIN, new Effects.UseHeat2AttackAgainEffectHandler());
            Register(EAttributeType.SELF_BUFF_MULTIPLIER, new Effects.SelfBuffMulitiplierEffectHandler());
            Register(EAttributeType.ENEMY_BUFF_MULTIPLIER, new Effects.EnemyBuffMultiplierEffectHandler());
            Register(EAttributeType.SEND_BLEED_BY_GET_HIT, new Effects.SendBleedByGetHitEffectHandler());
            Register(EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF, new Effects.ClearSelfBleedAndHealSelfEffectHandler());
            Register(EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART, new Effects.ClearEnemyBleedAndHealPartEffectHandler());
            Register(EAttributeType.ATTACK_BY_ENEMY_BLEED, new Effects.AttackByBleedEffectHandler());
            Register(EAttributeType.CHANGE_FAT_2_BURN, new Effects.ChangeFat2BurnEffectHandler());
            Register(EAttributeType.INCREASE_ADD_BURN, new Effects.IncreaseAddBurnEffectHandler());
            Register(EAttributeType.SPREAD_BURN, new Effects.SpreadBurnEffectHandler());
            Register(EAttributeType.SEND_ALL_FAT_BY_GET_HIT, new Effects.SendAllFatByGetHitEffectHandler());


        }

        public static void Register(EAttributeType _type, IPartEffectHandler _handler)
        {
            if (_handler == null) return;
            _m_handlerDict[_type] = _handler;
        }

        public static IPartEffectHandler Get(EAttributeType _type)
        {
            return _m_handlerDict.TryGetValue(_type, out var h) ? h : null;
        }

        /// <summary> 用注册的处理器执行效果；若未注册则返回 false，调用方可走旧逻辑 </summary>
        public static bool TryExecute(EAttributeType _type, PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var handler = Get(_type);
            if (handler == null) return false;
            handler.Execute(_caster, _entry, _ctx);
            return true;
        }
    }
}
