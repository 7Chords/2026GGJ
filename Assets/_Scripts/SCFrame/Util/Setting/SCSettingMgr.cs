using UnityEngine;

namespace SCFrame
{
    public class SCSettingMgr : Singleton<SCSettingMgr>
    {
        private const string LanguagePrefKey = "SCSetting.LanguageType";
        private const string BattleSpeed2xPrefKey = "SCSetting.BattleSpeed2x";
        private const float BattleDurationScaleFast = 1f / 1.5f;

        public SCSettingData data = new SCSettingData();

        public ELanguageType languageType
        {
            get => data.languageType;
            set
            {
                if (data.languageType == value)
                    return;
                data.languageType = value;
                PlayerPrefs.SetInt(LanguagePrefKey, (int)data.languageType);
                PlayerPrefs.Save();
            }
        }

        public bool battleSpeed2x
        {
            get => data.battleSpeed2x;
            set
            {
                if (data.battleSpeed2x == value)
                    return;
                data.battleSpeed2x = value;
                PlayerPrefs.SetInt(BattleSpeed2xPrefKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary> Multiplier for battle playback durations (1/1.5 when fast speed). </summary>
        public float battleDurationScale => battleSpeed2x ? BattleDurationScaleFast : 1f;

        public override void OnInitialize()
        {
            int stored = PlayerPrefs.GetInt(LanguagePrefKey, (int)ELanguageType.zh_CN);
            data.languageType = (ELanguageType)stored;
            data.battleSpeed2x = PlayerPrefs.GetInt(BattleSpeed2xPrefKey, 0) != 0;
        }

        public float ScaleBattleDuration(float duration)
        {
            return duration * battleDurationScale;
        }
    }
}
