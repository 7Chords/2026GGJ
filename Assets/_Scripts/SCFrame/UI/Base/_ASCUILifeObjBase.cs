using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame.UI
{
    /// <summary>
    /// UI带生命周期的物体抽象基类
    /// 设计目的 如果UIObj的显隐和增删在node里面做的话 那么没办法控制动画与逻辑的时序
    /// 如果在panel里面做的话会占用onXXX 导致子类没办法提示override
    /// </summary>
    public abstract class _ASCUILifeObjBase : _ASCLifeObjBase
    {
        public sealed override void OnDiscard()
        {
            BeforeDiscard();
            AfterDiscard();
        }
        public abstract void BeforeDiscard();
        public abstract void AfterDiscard();
        public sealed override void OnInitialize()
        {
            BeforeInitialize();
            AfterInitialize();
        }
        public abstract void BeforeInitialize();
        public abstract void AfterInitialize();
        public sealed override void OnResume() { }

        public sealed override void OnSuspend() { }
    }
}
