using System.Collections.Generic;
using GameCore.RefData;

namespace GameCore.Helpers
{
    public static class EnemyPassiveIntroText
    {
        public static string ResolveTitle(EnemyPassiveRefObj row)
        {
            return row == null ? string.Empty : (row.passiveName ?? string.Empty);
        }

        public static string ResolveDesc(EnemyPassiveRefObj row)
        {
            if (row == null)
                return string.Empty;
            string d = row.passiveDesc ?? string.Empty;
            object[] args = BuildFormatArgs(row.paramList);
            if (args == null || args.Length == 0)
                return d;
            try
            {
                return string.Format(d, args);
            }
            catch
            {
                return d;
            }
        }

        static object[] BuildFormatArgs(List<float> paramList)
        {
            if (paramList == null || paramList.Count == 0)
                return null;
            var a = new object[paramList.Count];
            for (int i = 0; i < paramList.Count; i++)
                a[i] = paramList[i];
            return a;
        }
    }
}
