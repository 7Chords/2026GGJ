using SCFrame;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace SCFrame
{
    /// <summary>
    /// 单个动画节点
    /// </summary>

    public class SingleAnimationNode : AnimationNodeBase
    {
        private AnimationClipPlayable clipPlayable;
        public void Init(PlayableGraph graph, AnimationMixerPlayable outputMixer, AnimationClip animationClip, float speed, int inputPort)
        {
            clipPlayable = AnimationClipPlayable.Create(graph, animationClip);
            clipPlayable.SetSpeed(speed);
            InputPort = inputPort;
            graph.Connect(clipPlayable, 0, outputMixer, inputPort);
        }

        public AnimationClip GetAnimationClip()
        {
            return clipPlayable.GetAnimationClip();
        }

        public override void SetSpeed(float speed)
        {
            clipPlayable.SetSpeed(speed);
        }
    }
}
