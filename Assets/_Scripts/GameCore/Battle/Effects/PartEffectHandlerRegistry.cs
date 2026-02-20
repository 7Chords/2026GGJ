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
            RegisterDefaults();
        }

        private static void RegisterDefaults()
        {
            Register(EAttributeType.ATTACK, new Effects.AttackEffectHandler());
            Register(EAttributeType.REAL_ATTACK, new Effects.RealAttackEffectHandler());
            Register(EAttributeType.REFLECT, new Effects.ReflectEffectHandler());
            Register(EAttributeType.TRIGGER_MORE, new Effects.TriggerMoreEffectHandler());
            Register(EAttributeType.DAMAGE_MULTIPILER, new Effects.DamageMultiplierEffectHandler());
            Register(EAttributeType.HEAL_PART, new Effects.HealPartEffectHandler());
            Register(EAttributeType.PART_LOSE_TURN, new Effects.PartLoseTurnEffectHandler());
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
