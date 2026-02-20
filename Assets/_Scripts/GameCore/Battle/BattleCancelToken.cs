namespace GameCore.Battle
{
    /// <summary>
    /// 战斗流程取消令牌：TerminateBattle 时置为已取消，延迟回调中检查后不再执行后续步骤。
    /// 避免依赖协程的 KillAll，流程更清晰且易扩展。
    /// </summary>
    public class BattleCancelToken
    {
        public bool isCancelled { get; private set; }

        public void Cancel()
        {
            isCancelled = true;
        }

        public void Reset()
        {
            isCancelled = false;
        }
    }
}
