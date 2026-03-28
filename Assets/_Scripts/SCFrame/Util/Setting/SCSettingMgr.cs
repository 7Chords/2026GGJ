using UnityEngine;

namespace SCFrame
{
    public class SCSettingMgr : Singleton<SCSettingMgr>
    {
        private const string LanguagePrefKey = "SCSetting.LanguageType";

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

        public override void OnInitialize()
        {
            int stored = PlayerPrefs.GetInt(LanguagePrefKey, (int)ELanguageType.zh_CN);
            data.languageType = (ELanguageType)stored;
        }
    }
}
