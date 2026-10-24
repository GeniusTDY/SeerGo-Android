using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Dialog1LevelUp : Dialog1Base
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI petName;
    [SerializeField] private TextMeshProUGUI petContent;
    [SerializeField] private Image petImg;

    [Header("增加信息")]
    [SerializeField] private TextMeshProUGUI addLevel;
    [SerializeField] private TextMeshProUGUI addHp;
    [SerializeField] private TextMeshProUGUI addAtk;
    [SerializeField] private TextMeshProUGUI addDef;
    [SerializeField] private TextMeshProUGUI addSpAtk;
    [SerializeField] private TextMeshProUGUI addSpDef;
    [SerializeField] private TextMeshProUGUI addSpd;

    [Header("当前信息")]
    [SerializeField] private TextMeshProUGUI Level;
    [SerializeField] private TextMeshProUGUI Hp;
    [SerializeField] private TextMeshProUGUI Atk;
    [SerializeField] private TextMeshProUGUI Def;
    [SerializeField] private TextMeshProUGUI SpAtk;
    [SerializeField] private TextMeshProUGUI SpDef;
    [SerializeField] private TextMeshProUGUI Spd;

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Initialize(List<int> addNums, PetData petData, int expGained)
    {
        string resPath = $"Sprites/PetImage/Pet_{petData.Id}/1";
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(resPath);

        petName.text = petData.Basic.Name;
        petImg.sprite = sprite;

        string finalText = string.Format("赛尔精灵获得经验值：<color=#FF0000>{0}</color>成功升至：<color=#FF0000>{1}</color>级",
            expGained, petData.Exp.Level);
        petContent.text = finalText;

        // 新增种族值
        addLevel.text = "+" + addNums[6].ToString();
        addHp.text = "+" + addNums[0].ToString();
        addAtk.text = "+" + addNums[1].ToString();
        addDef.text = "+" + addNums[2].ToString();
        addSpAtk.text = "+" + addNums[3].ToString();
        addSpDef.text = "+" + addNums[4].ToString();
        addSpd.text = "+" + addNums[5].ToString();

        // 当前种族值
        Level.text = petData.Exp.Level.ToString();
        Hp.text = petData.CurrentStatus.Hp.ToString();
        Atk.text = petData.CurrentStatus.Atk.ToString();
        Def.text = petData.CurrentStatus.Def.ToString();
        SpAtk.text = petData.CurrentStatus.SpAtk.ToString();
        SpDef.text = petData.CurrentStatus.SpDef.ToString();
        Spd.text = petData.CurrentStatus.Spd.ToString();

        isClosing = false;
    }
}