using SuperScrollView;
using UnityEngine;

/// <summary>
/// 赛尔号战斗日志管理器
/// </summary>
public class LogTextListViewScript : MonoBehaviour
{
    [Header("UI References")]
    public LoopListView2 mLoopListView;

    [Header("Settings")]
    public int mMaxLogCount = 100;  // 最大日志数量

    private DataSourceMgr<LogTextItemData> mDataSourceMgr;
    private int mLogIdCounter = 0;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        // 初始化数据源
        mDataSourceMgr = new DataSourceMgr<LogTextItemData>(0);
        mLoopListView.InitListView(mDataSourceMgr.TotalItemCount, OnGetItemByIndex);

        // 添加初始日志（严格按照图片内容）
       /* AddPetEnterLog("伊优", true);     // 第1张图第1行
        AddPetEnterLog("火炎贝", false);  // 第1张图第2行

        AddSkillUseLog("贝尔", "撞击", "正常", false);           // 第2张图第1行
        AddSkillUseLog("小火猴", "火花", "正常", true);         // 第2张图第2行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行
        AddStateChangeSkillUseLog("贝尔", "摇尾巴", "防御降低了", "烧伤:2回合", false); // 第2张图第3行*/
    }

    /// <summary>
    /// 添加精灵登场日志 
    /// 我方/对方：白色
    /// 精灵名字：紫色
    /// </summary>
    public void AddPetEnterLog(string petName, bool isPlayer)
    {
        string side = isPlayer ? "我方" : "对方";
        string petColor = isPlayer ? LogColors.White : LogColors.LightPurple;
        string content = $"<color={LogColors.White}>{side}</color><color={petColor}>【{petName}】</color><color={LogColors.White}>登场了！</color>";
        AddLog(content);
    }

    /// <summary>
    /// 添加技能使用日志
    /// 精灵名字：
    /// 技能名字：黄色
    /// 状态标签：绿色
    /// 状态详情：绿色
    /// </summary>
    public void AddSkillUseLog(string petName, string skillName, string state = "正常", bool isPlayer = true)
    {
        // 判断精灵颜色：敌方红色，我方蓝色
        string petColor = isPlayer ? LogColors.PlayerPet : LogColors.EnemyPet;

        // 严格按照第2张图格式
        string content = $"<color={petColor}>【{petName}】使用了</color><color={LogColors.SkillName}>{skillName}，</color><color={LogColors.StateValue}>【状态】:{state}</color>";

        AddLog(content);
    }

    /// <summary>
    /// 添加状态变化技能使用日志
    /// 精灵名字
    /// 技能名字：白色
    /// 状态变化描述：白色
    /// 状态标签：绿色
    /// 状态详情：紫色
    /// </summary>
    public void AddStateChangeSkillUseLog(string petName, string skillName, string stateChange, string state, bool isPlayer = true)
    {
        // 判断精灵颜色
        string petColor = isPlayer ? LogColors.PlayerPet : LogColors.EnemyPet;

        string content = $"<color={petColor}>【{petName}】使用了</color><color={LogColors.SkillName}>{skillName}，</color><color={LogColors.LightPurple}>{stateChange}</color><color={LogColors.StateValue}>【状态】:{state}</color>";

        AddLog(content);
    }

    /// <summary>
    /// 添加日志到数据源
    /// </summary>
    private void AddLog(string content)
    {
        LogTextItemData logData = new LogTextItemData();
        logData.Init(mLogIdCounter++, content);

        // 添加到数据源
        mDataSourceMgr.AppendData(logData);

        // 如果超过最大数量，移除最早的日志
        if (mDataSourceMgr.TotalItemCount > mMaxLogCount)
        {
            mDataSourceMgr.RemoveData(0);
        }

        // 刷新列表
        mLoopListView.SetListItemCount(mDataSourceMgr.TotalItemCount, false);
        mLoopListView.RefreshAllShownItem();

        // 自动滚动到底部
        mLoopListView.MovePanelToItemIndex(mDataSourceMgr.TotalItemCount - 1, 0);
    }

    /// <summary>
    /// 清空所有日志
    /// </summary>
    public void ClearAllLogs()
    {
        mDataSourceMgr.SetDataTotalCount(0);
        mLoopListView.SetListItemCount(0, false);
        mLogIdCounter = 0;
    }

    /// <summary>
    /// ListView项目获取回调
    /// </summary>
    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= mDataSourceMgr.TotalItemCount)
        {
            return null;
        }

        LogTextItemData itemData = mDataSourceMgr.GetItemDataByIndex(index);
        if (itemData == null)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem("ItemPrefab");
        LogTextItem itemScript = item.GetComponent<LogTextItem>();

        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
            itemScript.Init();
        }

        itemScript.SetItemData(itemData, index);
        return item;
    }

    /// <summary>
    /// 外部添加日志的公共方法
    /// </summary>
    public void AddBattleLog(string petName, string skillName, string state = "正常", bool isPlayer = true, bool isPetEnter = false, string stateChange = null)
    {
        if (isPetEnter)
        {
            AddPetEnterLog(petName, isPlayer);
        }
        else if (stateChange != null)
        {
            AddStateChangeSkillUseLog(petName, skillName, stateChange, state, isPlayer);
        }
        else
        {
            AddSkillUseLog(petName, skillName, state, isPlayer);
        }
    }
}