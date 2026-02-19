namespace GameCore.Battle
{
    /// <summary>
    /// 效果执行时的上下文（如触发主动技时为默认，受击时为发送方与伤害）
    /// </summary>
    public struct PartEffectContext
    {
        public PartInfo SenderPart;
        public int Damage;

        public static PartEffectContext Active => new PartEffectContext { SenderPart = null, Damage = 0 };
        public static PartEffectContext GetHit(PartInfo sender, int damage) => new PartEffectContext { SenderPart = sender, Damage = damage };
    }
}
