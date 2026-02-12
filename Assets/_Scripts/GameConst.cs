using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class GameConst
    {
        #region UI RES NAME
        public const string PREFAB_PLAYER_FACE_PART = "prefab_player_face_part";
        public const string PREFAB_FACE_PART_PREVIEW = "prefab_face_part_preview";
        public const string PREFAB_TOOLTIP_GIRD = "prefab_tooltip_grid";
        public const string PREFAB_ENEMY_FACE_PART = "prefab_enemy_face_part";

        #endregion


        #region TAG & LAYER
        public const string FACE_GRID_TAG = "FaceGrid";
        #endregion

        #region TIP

        public const float TOOLTIP_SHOW_ON_LEFT_THRESHOLD = 0.7f;
        public const float TOOLTIP_SHOW_Y_OFFSET_SCREEN_RATIO = -0.25f;
        public const float TOOLTIP_SHOW_X_OFFSET_SCREEN_RATIO = 0.15f;
        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_X = 0.3f;
        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_Y = 0.2f;

        #endregion

        #region GAME
        public const int INIT_ENEMY_PART_COUNT = 3;
        public const int PLAYER_DRAW_CARD_COUNT = 3;
        public const int PLAYER_CARD_MAX_COUNT = 5;
        #endregion
    }
}
