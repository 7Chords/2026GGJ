using SCFrame;

namespace GameCore.RefData
{
    public class GoodsEffectObj : _AEffectObjBase
    {
        public long goodsId;
        public int goodsAmount;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 2)
                return;
            goodsId = SCCommon.ParseInt(strArr[0]);
            goodsAmount = SCCommon.ParseInt(strArr[1]);

        }

        protected override string OnSerialise()
        {
            string str = goodsId + ":" + goodsAmount;
            return str;
        }
    }
}

