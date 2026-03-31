using GameCore.RefData;
using SCFrame;

namespace GameCore
{
    public class SCRefDataMgr : Singleton<SCRefDataMgr>
    {
        public SCRefDataList<StoreRefObj> storeRefList = new SCRefDataList<StoreRefObj>(StoreRefObj.assetPath, StoreRefObj.sheetName);
        public SCRefDataList<GoodsRefObj> goodsRefList = new SCRefDataList<GoodsRefObj>(GoodsRefObj.assetPath, GoodsRefObj.sheetName);
        public SCRefDataList<PartRefObj> partRefList = new SCRefDataList<PartRefObj>(PartRefObj.assetPath, PartRefObj.sheetName);
        public PlayerRefObj playerConfigRefObj = new PlayerRefObj(PlayerRefObj.assetPath, PlayerRefObj.sheetName);
        public SCRefDataList<EnemyRefObj> enemyRefList = new SCRefDataList<EnemyRefObj>(EnemyRefObj.assetPath, EnemyRefObj.sheetName);
        public SCRefDataList<EnemyPassiveRefObj> enemyPassiveRefList = new SCRefDataList<EnemyPassiveRefObj>(EnemyPassiveRefObj.assetPath, EnemyPassiveRefObj.sheetName);
        public SCRefDataList<PartLevelRefObj> partLevelRefList = new SCRefDataList<PartLevelRefObj>(PartLevelRefObj.assetPath, PartLevelRefObj.sheetName);
        public SCRefDataList<TrialRefObj> trialRefList = new SCRefDataList<TrialRefObj>(TrialRefObj.assetPath, TrialRefObj.sheetName);
        public SCRefDataList<TrialRewardRefObj> trialRewardRefList = new SCRefDataList<TrialRewardRefObj>(TrialRewardRefObj.assetPath, TrialRewardRefObj.sheetName);
        public SCRefDataList<BuffRefObj> buffRefList = new SCRefDataList<BuffRefObj>(BuffRefObj.assetPath, BuffRefObj.sheetName);
        public SCRefDataList<EventRefObj> eventRefList = new SCRefDataList<EventRefObj>(EventRefObj.assetPath, EventRefObj.sheetName);
        public SCRefDataList<EventDialogueRefObj> eventDialogueRefList = new SCRefDataList<EventDialogueRefObj>(EventDialogueRefObj.assetPath, EventDialogueRefObj.sheetName);
        public SCRefDataList<EventGetMoneyRefObj> eventGetMoneyRefList = new SCRefDataList<EventGetMoneyRefObj>(EventGetMoneyRefObj.assetPath, EventGetMoneyRefObj.sheetName);
        public SCRefDataList<EventGetPartRefObj> eventGetPartRefList = new SCRefDataList<EventGetPartRefObj>(EventGetPartRefObj.assetPath, EventGetPartRefObj.sheetName);
        public SCRefDataList<MapRefObj> mapRefList = new SCRefDataList<MapRefObj>(MapRefObj.assetPath, MapRefObj.sheetName);
        public SCRefDataList<EventBlood2PartRefObj> eventBlood2PartRefList = new SCRefDataList<EventBlood2PartRefObj>(EventBlood2PartRefObj.assetPath, EventBlood2PartRefObj.sheetName);
        public SCRefDataList<EventPart2PartRefObj> eventPart2PartRefList = new SCRefDataList<EventPart2PartRefObj>(EventPart2PartRefObj.assetPath, EventPart2PartRefObj.sheetName);
        public SCRefDataList<TextLanguageRefObj> textLanguageRefList = new SCRefDataList<TextLanguageRefObj>(TextLanguageRefObj.assetPath, TextLanguageRefObj.sheetName);

        public override void OnInitialize()
        {
            storeRefList.readFromTxt();
            goodsRefList.readFromTxt();
            partRefList.readFromTxt();
            playerConfigRefObj.readFromTxt();
            enemyRefList.readFromTxt();
            enemyPassiveRefList.readFromTxt();
            partLevelRefList.readFromTxt();
            trialRefList.readFromTxt();
            trialRewardRefList.readFromTxt();
            buffRefList.readFromTxt();
            eventRefList.readFromTxt();
            eventDialogueRefList.readFromTxt();
            eventGetMoneyRefList.readFromTxt();
            eventGetPartRefList.readFromTxt();
            mapRefList.readFromTxt();
            eventBlood2PartRefList.readFromTxt();
            eventPart2PartRefList.readFromTxt();
            textLanguageRefList.readFromTxt();
        }
    }
}
