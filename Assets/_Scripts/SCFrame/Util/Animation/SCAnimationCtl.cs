using SCFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace SCFrame
{
    /// <summary>
    /// 动画控制器
    /// </summary>
    public class SCAnimationCtl : _ASCLifeObjBase
    {
        private Animator _m_animator;
        private PlayableGraph _m_graph;
        private AnimationMixerPlayable _m_mixer;

        private AnimationNodeBase _m_previousNode; // 上一个节点
        private AnimationNodeBase _m_currentNode;  // 当前节点
        private int _m_inputPort0 = 0;
        private int _m_inputPort1 = 1;

        private string _m_transitionCoroutineId;

        private float _m_speed;
        public float speed
        {
            get => _m_speed;
            set
            {
                _m_speed = value;
                _m_currentNode.SetSpeed(_m_speed);
            }
        }


        public void SetAnimator(Animator _anim)
        {
            if (_anim == null)
            {
                Debug.LogError("传入了一个空的动画控制器！！！");
                return;
            }
            _m_animator = _anim;
        }
        public override void OnInitialize()
        {
            if(_m_animator == null)
            {
                Debug.LogError("动画控制器为空！！！");
                return;
            }
            // 创建图
            _m_graph = PlayableGraph.Create("Animation_Controller_" + _m_animator.gameObject.name);
            // 设置图的时间模式
            _m_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            // 创建混合器
            _m_mixer = AnimationMixerPlayable.Create(_m_graph, 3);
            // 创建Output
            AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(_m_graph, "Animation", _m_animator);
            // 让混合器链接上Output
            playableOutput.SetSourcePlayable(_m_mixer);
        }

        public override void OnDiscard()
        {
            // 清理这个对象的所有协程
            SCTaskHelper.instance.KillAllCoroutines(this);
            _m_graph.Destroy();
        }

        public override void OnResume()
        {
            _m_graph.Play();
        }

        public override void OnSuspend()
        {
            _m_graph.Stop();
        }




        public void DestoryNode(AnimationNodeBase node)
        {
            if (node != null)
            {
                _m_graph.Disconnect(_m_mixer, node.InputPort);
                node.PushPool();
            }
        }

        private void StartTransitionAniamtion(float fixedTime)
        {
            if (!string.IsNullOrEmpty(_m_transitionCoroutineId))
            {
                SCTaskHelper.instance.KillCoroutine(_m_transitionCoroutineId);
            }
            _m_transitionCoroutineId = SCTaskHelper.instance.CreateCoroutine(this,TransitionAniamtion(fixedTime));
        }

        // 动画过渡
        private IEnumerator TransitionAniamtion(float fixedTime)
        {
            // 交换端口号
            int temp = _m_inputPort0;
            _m_inputPort0 = _m_inputPort1;
            _m_inputPort1 = temp;

            // 硬切判断
            if (fixedTime == 0)
            {
                _m_mixer.SetInputWeight(_m_inputPort1, 0);
                _m_mixer.SetInputWeight(_m_inputPort0, 1);
            }

            // 当前的权重
            float currentWeight = 1;
            float speed = 1 / fixedTime;

            while (currentWeight > 0)
            {
                // 权重在减少
                currentWeight = Mathf.Clamp01(currentWeight - Time.deltaTime * speed);
                _m_mixer.SetInputWeight(_m_inputPort1, currentWeight);  // 减少
                _m_mixer.SetInputWeight(_m_inputPort0, 1 - currentWeight); // 增加
                yield return null;
            }
            _m_transitionCoroutineId = "";
        }

        /// <summary>
        /// 播放单个动画
        /// </summary>
        public void PlaySingleAniamtion(AnimationClip animationClip, float speed = 1, bool refreshAnimation = false, float transitionFixedTime = 0.25f)
        {
            SingleAnimationNode singleAnimationNode = null;
            if (_m_currentNode == null) // 首次播放
            {
                singleAnimationNode = SCPoolMgr.instance.GetObject<SingleAnimationNode>();
                singleAnimationNode.Init(_m_graph, _m_mixer, animationClip, speed, _m_inputPort0);
                _m_mixer.SetInputWeight(_m_inputPort0, 1);
            }
            else
            {
                SingleAnimationNode preNode = _m_currentNode as SingleAnimationNode; // 上一个节点

                // 相同的动画
                if (!refreshAnimation && preNode != null && preNode.GetAnimationClip() == animationClip) return;
                // 销毁掉当前可能被占用的Node
                DestoryNode(_m_previousNode);
                singleAnimationNode = SCPoolMgr.instance.GetObject<SingleAnimationNode>();
                singleAnimationNode.Init(_m_graph, _m_mixer, animationClip, speed, _m_inputPort1);
                _m_previousNode = _m_currentNode;
                StartTransitionAniamtion(transitionFixedTime);
            }
            this._m_speed = speed;
            _m_currentNode = singleAnimationNode;
            if (_m_graph.IsPlaying() == false) _m_graph.Play();
        }


        /// <summary>
        /// 播放混合动画
        /// </summary>
        public void PlayBlendAnimation(List<AnimationClip> clips, float speed = 1, float transitionFixedTime = 0.25f)
        {
            BlendAnimationNode blendAnimationNode = SCPoolMgr.instance.GetObject<BlendAnimationNode>();
            // 如果是第一次播放，不存在过渡
            if (_m_currentNode == null)
            {
                blendAnimationNode.Init(_m_graph, _m_mixer, clips, speed, _m_inputPort0);
                _m_mixer.SetInputWeight(_m_inputPort0, 1);
            }
            else
            {
                DestoryNode(_m_previousNode);
                blendAnimationNode.Init(_m_graph, _m_mixer, clips, speed, _m_inputPort1);
                _m_previousNode = _m_currentNode;
                StartTransitionAniamtion(transitionFixedTime);
            }
            this._m_speed = speed;
            _m_currentNode = blendAnimationNode;
            if (_m_graph.IsPlaying() == false) _m_graph.Play();
        }

        /// <summary>
        /// 播放混合动画
        /// </summary>
        public void PlayBlendAnimation(AnimationClip clip1, AnimationClip clip2, float speed = 1, float transitionFixedTime = 0.25f)
        {
            BlendAnimationNode blendAnimationNode = SCPoolMgr.instance.GetObject<BlendAnimationNode>();
            // 如果是第一次播放，不存在过渡
            if (_m_currentNode == null)
            {
                blendAnimationNode.Init(_m_graph, _m_mixer, clip1, clip2, speed, _m_inputPort0);
                _m_mixer.SetInputWeight(_m_inputPort0, 1);
            }
            else
            {
                DestoryNode(_m_previousNode);
                blendAnimationNode.Init(_m_graph, _m_mixer, clip1, clip2, speed, _m_inputPort1);
                _m_previousNode = _m_currentNode;
                StartTransitionAniamtion(transitionFixedTime);
            }
            this._m_speed = speed;
            _m_currentNode = blendAnimationNode;
            if (_m_graph.IsPlaying() == false) _m_graph.Play();
        }

        public void SetBlendWeight(List<float> weightList)
        {
            (_m_currentNode as BlendAnimationNode).SetBlendWeight(weightList);
        }
        public void SetBlendWeight(float clip1Weight)
        {
            (_m_currentNode as BlendAnimationNode).SetBlendWeight(clip1Weight);
        }


    }
}
