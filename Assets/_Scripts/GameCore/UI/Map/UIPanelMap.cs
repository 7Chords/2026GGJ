using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMap : _ASCUIPanelBase<UIMonoMap>
    {
        public UIPanelMap(UIMonoMap _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        private GameObject _playerIconGO;

        public override void OnShowPanel()
        {
             UpdatePlayerIcon();
        }

        public override void AfterInitialize()
        {
            
        }

        public override void OnHidePanel()
        {
            
        }

        private void UpdatePlayerIcon()
        {
            var pos = GameModel.instance.playerMapPosition;
            if (pos.x == -1 || MapManager.instance.CurrentMapNodes == null) return; // Not started or invalid

            var targetNode = MapManager.instance.GetNode(pos.x, pos.y);
            if (targetNode != null)
            {
                if (_playerIconGO == null)
                {
                    // Created simple icon or load prefab
                    // Using "PlayerIcon" resource if exists, else create default image
                    //_playerIconGO = ResourcesHelper.LoadGameObject("PlayerIcon", targetNode.transform);
                    if (_playerIconGO == null)
                    {
                        // Fallback: Create simple Red Circle
                        _playerIconGO = new GameObject("PlayerIcon");
                        var img = _playerIconGO.AddComponent<UnityEngine.UI.Image>();
                        img.color = Color.green;
                        // Assuming standard sprite exists or just color block
                    }
                }
                
                // Parent to the Node so it moves with it
                _playerIconGO.transform.SetParent(targetNode.transform);
                _playerIconGO.transform.localPosition = Vector3.zero;
                _playerIconGO.transform.localScale = Vector3.one * 0.5f; // Small icon
                _playerIconGO.SetActive(true);
                
                // Ensure it draws on top
                _playerIconGO.transform.SetAsLastSibling();
            }
        }
        
        public override void BeforeDiscard()
        {
            
        }

        public override void AfterDiscard()
        {
            base.AfterDiscard();
        }
    }
}
