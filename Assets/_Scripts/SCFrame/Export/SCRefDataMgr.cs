using GameCore.RefData;
using SCFrame;

namespace GameCore
{
    /// <summary>
    /// 配表数据管理器
    /// </summary>
    public class SCRefDataMgr : Singleton<SCRefDataMgr>
    {
        public SCRefDataList<StoreRefObj> storeRefList = new SCRefDataList<StoreRefObj>(StoreRefObj.assetPath, StoreRefObj.sheetName);
        public SCRefDataList<GoodsRefObj> goodsRefList = new SCRefDataList<GoodsRefObj>(GoodsRefObj.assetPath, GoodsRefObj.sheetName);
        public SCRefDataList<PartRefObj> partRefList = new SCRefDataList<PartRefObj>(PartRefObj.assetPath, PartRefObj.sheetName);

        public override void OnInitialize()
        {
            storeRefList.readFromTxt();
            goodsRefList.readFromTxt();
            partRefList.readFromTxt();
        }
    }
}
