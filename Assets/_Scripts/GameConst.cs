using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class GameConst
    {
        #region UI RES NAME
        public const string PREFAB_FACE_PART = "prefab_face_part";
        public const string PREFAB_FACE_PART_PREVIEW = "prefab_face_part_preview";
        #endregion


        #region TAG & LAYER
        public const string FACE_GRID_TAG = "FaceGrid";
        #endregion

        #region TIP

        public const float TOOLTIP_SHOW_ON_LEFT_THRESHOLD = 0.7f;
        public const float TOOLTIP_SHOW_Y_OFFSET_SCREEN_RATIO = -0.1f;
        public const float TOOLTIP_SHOW_X_OFFSET_SCREEN_RATIO = 0.1f;
        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_X = 0.3f;
        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_Y = 0.2f;

        #endregion

        #region GAME
        public const int INIT_ENEMY_PART_COUNT = 3;
        #endregion
    }
}
