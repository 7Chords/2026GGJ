public class SCMsgConst
{
    #region 玩家输入相关 10
    public const int TBS_ATTACK_INPUT = 1000;
    public const int TBS_DEFEND_INPUT = 1001;
    public const int TBS_SKILL_INPUT = 1002;
    public const int TBS_ITEM_INPUT = 1003;
    public const int TBS_SWITCH_TO_UP_INPUT = 1004;
    public const int TBS_SWITCH_TO_DOWN_INPUT = 1005;
    public const int TBS_SWITCH_TO_LEFT_INPUT = 1006;
    public const int TBS_SWITCH_TO_RIGHT_INPUT = 1007;
    public const int TBS_CONFIRM_INPUT = 1008;

    public const int TBS_MOUSE_CLICK_ENEMY_INPUT = 1009;
    public const int TBS_MOUSE_CLICK_PLAYER_INPUT = 1010;
    public const int ESC_INPUT = 1011;
    public const int MOUSE_RIGHT_INPUT = 1012;
    public const int TBS_DETAIL_INPUT = 1013;

    public const int OW_CONFIRM_INPUT = 1014;
    public const int OW_OPTION_INPUT = 1015;
    public const int OW_SWITCH_TO_UP_INPUT = 1016;
    public const int OW_SWITCH_TO_DOWN_INPUT = 1017;
    public const int OW_SWITCH_TO_LEFT_INPUT = 1018;
    public const int OW_SWITCH_TO_RIGHT_INPUT = 1019;
    public const int OW_INTERACT_INPUT = 1020;

    #endregion


    #region TBS回合制战斗相关 20
    public const int TBS_GAME_START = 2000;
    public const int TBS_GAME_FINISH = 2001;

    public const int TBS_TURN_CHG = 2002;//回合制战斗 - 回合轮转
    public const int TBS_ACTOR_ACTION_END = 2003;//回合制战斗 - 角色行动结束

    public const int TBS_TURN_MGR_WORK = 2004;
    public const int TBS_ACTOR_MGR_WORK = 2005;
    public const int TBS_EFFECT_MGR_WORK = 2006;
    public const int TBS_COMP_MGR_WORK = 2007;

    public const int TBS_TURN_MGR_REST = 2008;
    public const int TBS_ACTOR_MGR_REST = 2009;
    public const int TBS_EFFECT_MGR_REST = 2010;
    public const int TBS_COMP_MGR_REST = 2011;

    public const int TBS_GAME_RESULT = 2012;

    public const int TBS_ACTOR_ATTACK = 2013;
    public const int TBS_ACTOR_DEFENCE = 2014;
    public const int TBS_ACTOR_SKILL = 2015;

    public const int TBS_ACTOR_SKILL_HIGHTLIGHT_UP = 2016;//回合制战斗 - 选择技能高光上移
    public const int TBS_ACTOR_SKILL_HIGHTLIGHT_DOWN = 2017;//回合制战斗 - 选择技能高光下移
    public const int TBS_ACTOR_SKILL_MOUSE_HIGHLIGHT = 2018;//回合制战斗 - 鼠标选择技能高光

    public const int TBS_ACTOR_CHG = 2019;//回合制战斗 - 角色轮转
    //public const int TBS_ACTOR_TARGET_HIGHLIGHT_LEFT = 2020;//回合制战斗 - 选择目标高光左移
    //public const int TBS_ACTOR_TARGET_HIGHLIGHT_RIGHT = 2021;//回合制战斗 - 选择目标高光右移
    public const int TBS_ACTOR_TARGET_MOUSE_HIGHLIGHT = 2022;//回合制战斗 - 鼠标选择高光
    public const int TBS_ACTOR_SKILL_CONFIRM = 2023;//回合制战斗 - 技能确认
    public const int TBS_SELECT_SINGLE_ENEMY_TARGET_CHG = 2024;//回合制战斗 - 选择单个敌方目标 改变

    public const int TBS_ACTOR_INFO_CHG = 2025;//回合制战斗 - 敌人目标信息改变（血量/属性...)
    public const int TBS_ACTOR_CONFIRM_RELEASE = 2026;//回合制战斗 - 确认释放（技能/道具）

    public const int TBS_TRAM_ACTION_END = 2027;//回合制战斗 - 队伍行动结束
    public const int TBS_TURN_CHG_SHOW_END = 2028;//回合制战斗 - 回合切换面板显示结束
    public const int TBS_ACTOR_DIE = 2029;//回合制战斗 - 角色死亡
    public const int TBS_SELECT_ENEMY_ALL_OR_SINGLE_STATE_SWITCH = 2030;//回合制战斗 - 选择敌方目标 单体/全体切换
    public const int TBS_ALL_PLAYER_ACTOR_DIE = 2031;//回合制战斗 - 所有我方角色死亡
    public const int TBS_ALL_ENEMY_ACTOR_DIE = 2032;//回合制战斗 - 所有敌方角色死亡
    public const int TBS_ENEMY_ACTOR_REMOVE_FROM_LIST = 2033;//回合制战斗 - 敌方角色从队列中移除

    public const int TBS_ACTOR_ITEM_HIGHTLIGHT_UP = 2034;//回合制战斗 - 选择道具高光上移
    public const int TBS_ACTOR_ITEM_HIGHTLIGHT_DOWN = 2035;//回合制战斗 - 选择道具高光下移
    public const int TBS_ACTOR_ITEM_MOUSE_HIGHLIGHT = 2036;//回合制战斗 - 鼠标选择道具高光
    public const int TBS_ACTOR_ITEM_CONFIRM = 2037;//回合制战斗 - 道具确认
    public const int TBS_ACTOR_ITEM = 2038;
    public const int TBS_SELECT_SINGLE_PLAYER_TARGET_CHG = 2039;//回合制战斗 - 选择单个玩家角色目标 改变
    public const int TBS_ACTOR_GET_BUFF = 2040;//回合制战斗 - 角色获得buff
    public const int TBS_ACTOR_REMOVE_BUFF = 2041;//回合制战斗 - 角色移除buff
    public const int TBS_DETAIL_SELECT_UP = 2042;//回合制战斗 - 角色详情面板选择角色上移
    public const int TBS_DETAIL_SELECT_DOWN = 2043;//回合制战斗 - 角色详情面板选择角色下移
    public const int TBS_DETAIL_SELECT_CLICK = 2044;//回合制战斗 - 角色详情面板选择查看角色
    public const int TBS_ACTOR_READY_CONTROL = 2045;//回合制战斗 - 角色准备好操作（相机和UI显示正确）
    #endregion

    #region OpenWorld大世界相关 30
    public const int OW_ITEM_MOUSE_HIGHLIGHT = 3001;
    public const int OW_CHARACTER_SELECT_CLICK = 3002;//
    public const int OW_PURCHASE_ITEM = 3003;//大世界 - 购买物品
    public const int OW_ITEM_CONFIRM = 3004;
    public const int OW_STORE_CONFIRM = 3005;
    public const int OW_OPTION_CONFIRM = 3006;
    public const int OW_DIALOG_CONFIRM = 3007;
    public const int OW_COMMON_TWO_BTN_CONFIRM = 3008;

    public const int OW_ITEM_HIGHLIGHT_UP = 3009;
    public const int OW_ITEM_HIGHLIGHT_DOWN = 3010;
    public const int OW_STORE_HIGHLIGHT_UP = 3011;
    public const int OW_STORE_HIGHLIGHT_DOWN = 3012;
    public const int OW_OPTION_HIGHLIGHT_UP = 3013;
    public const int OW_OPTION_HIGHLIGHT_DOWN = 3014;
    public const int OW_CHARACTER_HIGHLIGHT_UP = 3015;
    public const int OW_CHARACTER_HIGHLIGHT_DOWN = 3016;
    public const int OW_COMMON_TWO_BTN_HIGHLIGHT_LEFT=3017;
    public const int OW_COMMON_TWO_BTN_HIGHLIGHT_RIGHT = 3018;

    #endregion

    #region 系统相关 99
    public const int GAME_START = 9901;
    public const int GAME_END = 9902;
    public const int UI_NODE_CHG = 9903;//ui节点变化事件

    #endregion
}
