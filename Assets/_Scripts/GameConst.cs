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
        public const string PREFAB_BATTLE_PART = "prefab_battle_part";
        public const string PREFAB_TOOLTIP_BUFF_ITEM = "prefab_tooltip_buff_item";
        public const string PREFAB_PART_BUFF_ITEM = "prefab_part_buff_item";
        public const string PREFAB_POP_TIP = "prefab_pop_tip";
        public const string PREFAB_TOOLTIP = "prefab_tooltip";
        public const string PREFAB_BUFF_SIDE_ITEM = "prefab_buff_side_item";
        public const string PREFAB_INTRO_TIP = "prefab_intro_tip";
        #endregion

        #region TAG & LAYER
        public const string FACE_GRID_TAG = "FaceGrid";
        #endregion

        #region Mat
        public const string MAT_UI_OUTLINE = "mat_ui_outline";
        #endregion

        #region Map icons (Resources sprite names)
        public const string SPR_ICON_NODE_PLAYER = "spr_icon_node_player";
        #endregion

        #region TIP

        public const float TOOLTIP_SHOW_ON_LEFT_THRESHOLD = 0.5f;
        public const float TOOLTIP_SHOW_ON_UP_THRESHOLD = 0.5f;
        public const float TOOLTIP_SHOW_Y_OFFSET_SCREEN_RATIO = -0.25f;
        public const float TOOLTIP_SHOW_X_OFFSET_SCREEN_RATIO = 0.15f;
        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_COMBINE = 0.15f;
        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_COMBINE = 0.2f;

        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_BATTLE = 0.5f;
        public const float SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_BATTLE = 0.2f;

        /// <summary> 伤害飘字（damagenum）与效果飘字（effect text）队列中，两条之间的间隔（秒）。 </summary>
        public const float UIFLYOUT_STAGGER_INTERVAL = 0.5f;

        #endregion

        #region GAME
        public const int DRAW_CARD_COUNT_PER_TURN = 3;
        public const int BUSY_CARD_MAX_COUNT = 6;

        public const int BUFF_LAYER_MAX = 99;

        /// <summary> Primary keys in buff.txt; keep in sync with table and effect handlers. </summary>
        public const long BUFF_ID_BLEED = 100001;
        public const long BUFF_ID_FAT = 100002;
        public const long BUFF_ID_BURN = 100003;
        public const long BUFF_ID_STRONG = 100004;
        public const long BUFF_ID_MOLD = 100007;
        public const long BUFF_ID_BREEDING_MASS = 100008;
        public const long BUFF_ID_HEAL_MASS = 100009;
        public const long BUFF_ID_ATTACK_MASS = 100010;

        public const int EVENT_BLOOD_2_PART_ROLL_MIN_CURRENT_HEALTH = 30;

        public const float DELAY_START_TIME = 0.5f;
        public const float DELAY_EFFECT_TIME = 0.5f;
        public const float DELAY_END_TIME = 0.5f;
        public const float DELAY_ACTIVE_BUFF_TIME = 0.75f;

        /// <summary> One run spans this many map floors; boss on the last floor opens run victory (panel_win). </summary>
        public const int RUN_TOTAL_FLOORS = 2;
        #endregion
    }
}
