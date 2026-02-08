using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Logic
{

    /// <summary>
    /// 部位逻辑基类 只做逻辑的东西 效果不在这里做
    /// </summary>
    public abstract class BasePartLogic
    {
        protected PartInfo partInfo;

        public virtual void Initialize(PartInfo _info)
        {
            partInfo = _info;
        }

        public virtual void OnGetHit(float damage) { }
        public virtual void OnPartBroken() { }
        public virtual void OnPartAction() { }
    }
}
