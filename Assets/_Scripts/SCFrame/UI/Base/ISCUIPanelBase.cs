using UnityEngine;

namespace SCFrame.UI
{
    public interface ISCUIPanelBase
    {
        GameObject GetGameObject();

        void ShowPanel();
        void HidePanel();
        //void ResetPanel();
    }
}
