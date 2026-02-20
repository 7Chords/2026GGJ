namespace GameCore.Battle
{
    /// <summary>
    /// 效果执行时的上下文（如触发主动技时为默认，受击时为发送方与伤害）
    /// </summary>
    public struct PartEffectContext
    {
        public PartInfo senderPart;
        public int damage;

        public static PartEffectContext Active => new PartEffectContext { senderPart = null, damage = 0 };
        public static PartEffectContext GetHit(PartInfo _sender, int _damage) => new PartEffectContext { senderPart = _sender, damage = _damage };
    }
}
