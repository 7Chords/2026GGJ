namespace GameCore.Battle
{
    /// <summary>
    /// 部位效果处理器：一种属性类型对应一个处理器，便于扩展新效果而不改 PartLogicFactory。
    /// </summary>
    public interface IPartEffectHandler
    {
        void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx);
    }
}
