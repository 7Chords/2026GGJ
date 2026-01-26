using UnityEngine;


namespace SCFrame.UI
{
    public abstract class _ASCUIAnimMonoBase : _ASCUIMonoBase
    {
        [Header("UI动画机")]
        public Animator uiAnimator;
        [Header("打开UI动画名")]
        public string showUIName;
        [Header("关闭UI动画名")]
        public string hideUIName;
        [Header("动画事件触发器")]
        public AnimationEventTrigger animEventTrigger;
    }
}
