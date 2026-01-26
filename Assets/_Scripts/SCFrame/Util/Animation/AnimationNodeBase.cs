using SCFrame;

/// <summary>
/// 动画节点基类
/// </summary>
/// 
namespace SCFrame
{
    public abstract class AnimationNodeBase
    {
        public int InputPort;

        public abstract void SetSpeed(float speed);
        public virtual void PushPool()
        {
            this.ObjectPushPool();
        }
    }
}