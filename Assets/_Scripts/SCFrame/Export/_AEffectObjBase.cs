using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    /// <summary>
    /// 配表效果对象 需要解析使用 
    /// 如配置一个buff效果 写一个类继承此抽象类 有id 持续回合
    /// </summary>
    public abstract class _AEffectObjBase
    {

        public virtual string Serialise()
        {
            return OnSerialise();
        }

        public virtual void Deserialize(string _str)
        {
            OnDeserialize(_str);
        }
        protected abstract string OnSerialise();
        protected abstract void OnDeserialize(string _str);
    }
}
