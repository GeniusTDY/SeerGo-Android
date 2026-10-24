using PixeLadder.SimpleTooltip;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipsManager : MonoBehaviour
{
    [Header("飞船功能室按钮")]
    public Button Btn_Captain;      // 船长室
    public Button Btn_Shiyanshi;    // 实验室
    public Button Btn_Ziliaoshi;    // 资料室
    public Button Btn_Liaowancang;  // 精灵仓
    public Button Btn_Liaowanlutai; // 精灵操作台
    public Button Btn_Donlishi;     // 动力室
    public Button Btn_Chuangsoncang;// 创造仓
    public Button Btn_Jixieshi;     // 机械室
    public Button Btn_JiaoGuang;    // 校光室
    public Button Btn_Famingshi;    // 发明室
    public Button Btn_Taikong;      // 太空站

    [Header("主界面UI按钮")]
    public Button Guide;
    public Button Mail;
    public Button Spt;
    public Button Sprite;
    public Button Task;
    public Button NoNo;
    public Button MiniMap;
    public Button Btn_Chat;      // 聊天
    public Button Btn_Emoj;      // 表情
    public Button Btn_Action;   // 动作
    public Button Btn_Shoot;    // 射击
    public Button Btn_PetsBag;  // 精灵背包
    public Button Btn_Home;     // 家园
    public Button Btn_Frends;   // 好友
    public Button Btn_Team;     // 队伍
    public Button Btn_Bag;      // 背包
    public Button Btn_Setting; // 设置
    public Button RemainingUses; // 道具剩余使用次数

    [Header("Canvas")]
    [SerializeField] private Canvas _WorldMapCanvas;
    [SerializeField] private Canvas _UICanvas;

    private void Start()
    {
        SetupSpaceshipTooltips();
        SetupMainUITooltips();
    }

    private void SetupSpaceshipTooltips()
    {
        // 使用元组数组简化
        var tooltips = new (Button btn, string tip)[]
        {
            (Btn_Captain, "船长室"),
            (Btn_Shiyanshi, "实验室"),
            (Btn_Ziliaoshi, "资料室"),
            (Btn_Liaowancang, "瞭望仓"),
            (Btn_Liaowanlutai, "瞭望露台"),
            (Btn_Donlishi, "动力室"),
            (Btn_Chuangsoncang, "传送仓"),
            (Btn_Jixieshi, "机械室"),
            (Btn_JiaoGuang, "教官室"),
            (Btn_Famingshi, "发明室"),
            (Btn_Taikong, "太空站")
        };

        foreach (var (btn, tip) in tooltips)
        {
            if (btn != null)
            {
                TooltipTrigger.AddTooltip(btn.gameObject, null, tip, Color.yellow, null, _WorldMapCanvas);
            }
        }
    }

    private void SetupMainUITooltips()
    {
        // 使用元组数组简化
        var tooltips = new (Button btn, string tip)[]
        {
            (Guide, "指南(暂未开放)"),
            (Mail, "邮件(暂未开放)"),
            (Spt, "SPT(暂未开放)"),
            (Sprite, "精灵(暂未开放)"),
            (Task, "任务(暂未开放)"),
            (NoNo, "NoNo(暂未开放)"),
            (MiniMap, "世界地图"),
            (Btn_Chat, "聊天(暂未开放)"),
            (Btn_Emoj, "表情(暂未开放)"),
            (Btn_Action, "动作(暂未开放)"),
            (Btn_Shoot, "射击(暂未开放)"),
            (Btn_PetsBag, "精灵背包"),
            (Btn_Home, "基地"),
            (Btn_Frends, "好友(暂未开放)"),
            (Btn_Team, "战队(暂未开放)"),
            (Btn_Bag, "物品背包"),
            (Btn_Setting, "设置"),
            (RemainingUses, "道具剩余使用次数")
        };

        foreach (var (btn, tip) in tooltips)
        {
            if (btn != null)
            {
                TooltipTrigger.AddTooltip(btn.gameObject, null, tip, Color.yellow, null, _UICanvas);
            }
        }
    }
}