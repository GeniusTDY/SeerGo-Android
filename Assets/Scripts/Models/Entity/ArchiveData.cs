// Model/ArchiveData.cs
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

[Serializable]
[XmlRoot("gameData")]
public class ArchiveData
{
    [XmlElement("version")] public string Version = "lite_0.0.1";
    [XmlElement("gender")] public bool Gender;
    [XmlElement("nickname")] public string Nickname = "Player";
    [XmlElement("firstLoginDate")] public DateTime FirstLoginDate = DateTime.Now;
    [XmlElement("lastLoginDate")] public DateTime LastLoginDate = DateTime.Now;
    [XmlElement("achievement")] public int Achievement;
    [XmlElement("petTotalExp")] public int PetTotalExp = 0;

    [XmlArray("petBag")]
    [XmlArrayItem("Pet")]
    public List<PetData> PetBag = new List<PetData>();

    [XmlArray("petStorage")]
    [XmlArrayItem("Pet")]
    public List<PetData> PetStorage = new List<PetData>();

    [XmlArray("pvpPetTeam")]
    [XmlArrayItem("Pet")]
    public List<PetData> PvpPetTeam = new List<PetData>();

    [XmlArray("itemStorage")]
    [XmlArrayItem("Item")]
    public List<ItemData> ItemStorage = new List<ItemData>();

    [XmlArray("mailStorage")]
    [XmlArrayItem("Mail")]
    public List<MailData> MailStorage = new List<MailData>();

    [XmlArray("missionStorage")]
    [XmlArrayItem("Mission")]
    public List<MissionData> MissionStorage = new List<MissionData>();

    [XmlArray("activityStorage")]
    [XmlArrayItem("Activity")]
    public List<ActivityData> ActivityStorage = new List<ActivityData>();

    [XmlArray("battleRecordStorage")]
    public List<BattleRecordData> BattleRecordStorage = new List<BattleRecordData>();

    [XmlArray("rewardBuffs")]
    [XmlArrayItem("RewardBuff")]
    public List<RewardBuffData> RewardBuffs = new List<RewardBuffData>();

    [XmlElement("settingsData")] public SettingsData SettingsData = new SettingsData();
    [XmlElement("yiteRogueData")] public YiteRogueData YiteRogueData = new YiteRogueData();

    /// <summary>
    /// 打印存档数据的完整信息
    /// </summary>
    public void Print()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== 存档数据 ===");
        sb.AppendLine($"版本: {Version}");
        sb.AppendLine($"性别: {(Gender ? "男" : "女")}");
        sb.AppendLine($"昵称: {Nickname}");
        sb.AppendLine($"首次登录: {FirstLoginDate}");
        sb.AppendLine($"最后登录: {LastLoginDate}");
        sb.AppendLine($"成就点数: {Achievement}");
        sb.AppendLine($"经验分配器: {PetTotalExp}");

        sb.AppendLine($"宠物背包数量: {PetBag.Count}");
        for (int i = 0; i < PetBag.Count; i++)
        {
            sb.AppendLine($"宠物[{i}]:");
            PetBag[i].Print(sb, "  ");
        }

        sb.AppendLine($"宠物仓库数量: {PetStorage.Count}");
        sb.AppendLine($"PVP队伍数量: {PvpPetTeam.Count}");

        sb.AppendLine($"物品数量: {ItemStorage.Count}");
        for (int i = 0; i < ItemStorage.Count; i++)
        {
            sb.Append($"  [{i}] ");
            ItemStorage[i].Print(sb);
        }

        sb.AppendLine($"邮件数量: {MailStorage.Count}");
        sb.AppendLine($"任务数量: {MissionStorage.Count}");
        sb.AppendLine($"活动数量: {ActivityStorage.Count}");
        sb.AppendLine($"战斗记录数量: {BattleRecordStorage.Count}");

        sb.AppendLine("设置数据:");
        SettingsData.Print(sb, "  ");

        sb.AppendLine("异特 Roguelike 数据:");
        YiteRogueData.Print(sb, "  ");

        UnityEngine.Debug.Log(sb.ToString());
    }

    /// <summary>
    /// 打印存档数据的简要信息
    /// </summary>
    public void PrintSummary()
    {
        SeerGoLogger.Log($"存档概要 | 玩家: {Nickname} | 性别: {(Gender ? "男" : "女")} | 宠物: {PetBag.Count}只 | 物品: {ItemStorage.Count}种 | 成就: {Achievement}");
    }
}

[Serializable]
public class PetData
{
    [XmlElement("id")] public int Id;

    [XmlElement("basic")] public PetBasic Basic = new PetBasic();
    [XmlElement("feature")] public PetFeature Feature = new PetFeature();
    [XmlElement("exp")] public PetExp Exp = new PetExp();
    [XmlElement("talent")] public PetTalent Talent = new PetTalent();
    [XmlElement("skills")] public PetSkills Skills = new PetSkills();
    [XmlElement("record")] public PetRecord Record = new PetRecord();
    [XmlElement("ui")] public PetUI UI = new PetUI();
    [XmlElement("currentStatus")] public PetStatus CurrentStatus = new PetStatus();

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}ID: {Id}");
        Basic.Print(sb, indent + "  ");
        Feature.Print(sb, indent + "  ");
        Exp.Print(sb, indent + "  ");
        Talent.Print(sb, indent + "  ");
        CurrentStatus.Print(sb, indent + "  ");
        Skills.Print(sb, indent + "  ");

        // 添加记录信息打印
        sb.AppendLine($"{indent}  战斗记录:");
        sb.AppendLine($"{indent}    胜利: {Record.WinFightNum} 失败: {Record.LoseFightNum}");

        // 添加UI信息打印
        sb.AppendLine($"{indent}  UI信息: 基础ID={UI.BaseId}, 皮肤数量={UI.SpecialSkinList.Count}");
    }
}

[Serializable]
public class PetBasic
{
    [XmlAttribute("id")] public int Id;
    [XmlElement("name")] public string Name;
    [XmlElement("gender")] public int Gender;// 0雌，1雄，2无性别
    [XmlElement("personality")] public int Personality;
    [XmlElement("attributes")] public int Attributes;
    [XmlElement("height")] public int Height;
    [XmlElement("weight")] public int Weight;
    [XmlElement("getPetDate")] public string GetPetDate ="";

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}基础信息:");
        sb.AppendLine($"{indent}  名称: {Name}");
        sb.AppendLine($"{indent}  性别: {Gender}");
        sb.AppendLine($"{indent}  性格: {Personality}");
        sb.AppendLine($"{indent}  属性: {Attributes}");
        sb.AppendLine($"{indent}  身高: {Height}cm");
        sb.AppendLine($"{indent}  体重: {Weight}kg");
        sb.AppendLine($"{indent}  获得日期: {GetPetDate}");
    }
}

[Serializable]
public class PetFeature
{
    [XmlAttribute("id")] public int Id;
    [XmlElement("hasEmblem")] public bool HasEmblem;
    [XmlElement("featureId")] public int FeatureId;
    [XmlElement("emblemId")] public int EmblemId;
    [XmlArray("afterwardBuffIds")]
    [XmlArrayItem("int")] public List<int> AfterwardBuffIds = new List<int>();

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}特性信息:");
        sb.AppendLine($"{indent}  是否有徽章: {HasEmblem}");
        sb.AppendLine($"{indent}  特性ID: {FeatureId}");
        sb.AppendLine($"{indent}  徽章ID: {EmblemId}");
        sb.AppendLine($"{indent}  后续BuffID: {string.Join(", ", AfterwardBuffIds)}");
    }
}

[Serializable]
public class PetExp
{
    [XmlAttribute("id")] public int Id;
    [XmlElement("level")] public int Level;
    [XmlElement("growthType")] public int GrowthType;
    [XmlElement("fixedMaxLevel")] public int FixedMaxLevel;
    [XmlElement("currentExp")] public int CurrentExp;

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}经验信息:");
        sb.AppendLine($"{indent}  等级: {Level}");
        sb.AppendLine($"{indent}  经验类型: {GrowthType}");
        sb.AppendLine($"{indent}  固定最大等级: {FixedMaxLevel}");
        sb.AppendLine($"{indent}  当前经验: {CurrentExp}");
    }
}

[Serializable]
public class PetTalent
{
    [XmlAttribute("id")] public int Id;
    [XmlElement("iv")] public int IV;
    [XmlElement("ev")] public PetEV EV = new PetEV();
    [XmlElement("evStorage")] public int EVStorage;

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}天赋信息:");
        sb.AppendLine($"{indent}  个体值: {IV}");
        sb.AppendLine($"{indent}  EV存储: {EVStorage}");
        EV.Print(sb, indent + "  ");
    }
}

[Serializable]
public class PetEV
{
    [XmlElement("atk")] public int Atk = 0;
    [XmlElement("spAtk")] public int SpAtk = 0;
    [XmlElement("def")] public int Def = 0;
    [XmlElement("spDef")] public int SpDef = 0;
    [XmlElement("spd")] public int Spd = 0;
    [XmlElement("hp")] public int Hp = 0;

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}努力值:");
        sb.AppendLine($"{indent}  攻击: {Atk} | 特攻: {SpAtk}");
        sb.AppendLine($"{indent}  防御: {Def} | 特防: {SpDef}");
        sb.AppendLine($"{indent}  速度: {Spd} | 生命: {Hp}");
    }
}

[Serializable]
public class PetSkills
{
    [XmlAttribute("id")] public int Id;
    [XmlArray("ownSkillId")]
    [XmlArrayItem("int")] public List<int> OwnSkillId = new List<int>();
    [XmlArray("normalSkillId")]
    [XmlArrayItem("skill")] public List<EquippedSkill> NormalSkill = new List<EquippedSkill>();
    [XmlElement("superSkillId")] public int SuperSkillId;

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}技能信息:");
        sb.AppendLine($"{indent}  拥有技能: {string.Join(", ", OwnSkillId)}");
        sb.AppendLine($"{indent}  普通技能: {string.Join(", ", NormalSkill)}");
        sb.AppendLine($"{indent}  超级技能: {SuperSkillId}");
    }
}

[Serializable]
public class EquippedSkill
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("pp")]
    public int currentPP;

    [XmlAttribute("maxPP")]
    public int maxPP;

    public EquippedSkill() { }

    public EquippedSkill(int id, int pp, int maxPP)
    {
        this.id = id;
        this.currentPP = pp;
        this.maxPP = maxPP;
    }

    public void Print(StringBuilder sb, string indent = "")
    {
      sb.AppendLine($"{indent}技能: {id},技能当前PP{currentPP},最大PP{maxPP}");
    }


}

[Serializable]
public class PetRecord
{
    [XmlElement("winFightNum")] public int WinFightNum;
    [XmlElement("loseFightNum")] public int LoseFightNum;
    [XmlElement("recordDict")] public string RecordDict = "";

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}战斗记录:");
        sb.AppendLine($"{indent}  胜利: {WinFightNum} 失败: {LoseFightNum}");
        if (!string.IsNullOrEmpty(RecordDict))
        {
            sb.AppendLine($"{indent}  记录字典: {RecordDict}");
        }
    }
}

[Serializable]
public class PetUI
{
    [XmlAttribute("id")] public int Id;
    [XmlAttribute("baseId")] public int BaseId;
    [XmlArray("specialSkinList")]
    public List<string> SpecialSkinList = new List<string>();

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}UI信息:");
        sb.AppendLine($"{indent}  基础ID: {BaseId}");
        sb.AppendLine($"{indent}  特殊皮肤: {string.Join(", ", SpecialSkinList)}");
    }
}

[Serializable]
public class PetStatus
{
    [XmlElement("atk")] public int Atk;
    [XmlElement("spAtk")] public int SpAtk;
    [XmlElement("def")] public int Def;
    [XmlElement("spDef")] public int SpDef;
    [XmlElement("spd")] public int Spd;
    [XmlElement("hp")] public int Hp;
    [XmlElement("maxHp")] public int MaxHp;

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}当前状态:");
        sb.AppendLine($"{indent}  攻击: {Atk} | 特攻: {SpAtk}");
        sb.AppendLine($"{indent}  防御: {Def} | 特防: {SpDef}");
        sb.AppendLine($"{indent}  速度: {Spd} | 当前生命: {Hp} | 最大生命: {MaxHp}");
    }
}

[Serializable]
public class ItemData
{
    [XmlAttribute("id")] public int Id;
    [XmlAttribute("num")] public int Num;
    [XmlAttribute("fusionpetid")] public int FusionPetId;
    [XmlAttribute("fusionfeatureid")] public int FusionFeatureId;

    public void Print(StringBuilder sb)
    {
        sb.AppendLine($"物品 ID:{Id} 数量:{Num} 融合宠物id:{FusionPetId} 融合宠物特性id:{FusionFeatureId}");
    }
}

[Serializable]
public class MailData
{
    [XmlElement("isRead")] public bool IsRead;
    [XmlElement("sender")] public string Sender = "";
    [XmlElement("title")] public string Title = "";
    [XmlElement("content")] public string Content = "";
    [XmlElement("date")] public DateTime Date = DateTime.Now;
    [XmlElement("gift")] public MailGift Gift = new MailGift();
}

[Serializable]
public class MailGift
{
    [XmlElement("item")] public List<MailItem> Items = new List<MailItem>();
}

[Serializable]
public class MailItem
{
    [XmlAttribute("id")] public int Id;
    [XmlAttribute("num")] public int Num;
}

[Serializable]
public class MissionData
{
    [XmlAttribute("id")] public int Id;
    [XmlAttribute("checkpoint")] public string Checkpoint = "";
    [XmlAttribute("done")] public bool Done;
}

[Serializable]
public class ActivityData
{
    [XmlAttribute("id")] public string Id = "";
    [XmlElement("data")] public ActivityDataContent Data = new ActivityDataContent();
}

[Serializable]
public class ActivityDataContent
{
    [XmlElement("entry")] public List<ActivityEntry> Entries = new List<ActivityEntry>();
}

[Serializable]
public class ActivityEntry
{
    [XmlElement("key")] public string Key = "";
    [XmlElement("value")] public string Value = "";
}

[Serializable]
public class BattleRecordData
{
    // 根据实际需求定义战斗记录结构
}

[Serializable]
public class SettingsData
{
    [XmlElement("useRobotAsPlayer")] public bool UseRobotAsPlayer;
    [XmlElement("BGMVolume")] public int BGMVolume = 10;
    [XmlElement("UIVolume")] public int UIVolume;
    [XmlElement("battleBGMVolume")] public int BattleBGMVolume = 10;
    [XmlElement("battleSEVolume")] public int BattleSEVolume;
    [XmlElement("battleAnimSpeed")] public int BattleAnimSpeed;
    [XmlElement("shakeWhenBigDamage")] public bool ShakeWhenBigDamage;
    [XmlElement("flashWhenBigDamage")] public bool FlashWhenBigDamage;
    [XmlElement("initMapId")] public int InitMapId;
    [XmlElement("ruleId")] public int RuleId;

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}使用机器人: {UseRobotAsPlayer}");
        sb.AppendLine($"{indent}音量 - BGM: {BGMVolume}, UI: {UIVolume}, 战斗BGM: {BattleBGMVolume}, 战斗音效: {BattleSEVolume}");
        sb.AppendLine($"{indent}战斗动画速度: {BattleAnimSpeed}");
        sb.AppendLine($"{indent}震动: {ShakeWhenBigDamage}, 闪光: {FlashWhenBigDamage}");
        sb.AppendLine($"{indent}初始地图: {InitMapId}, 规则: {RuleId}");
    }
}

[Serializable]
public class RewardBuffData
{
    [XmlAttribute("buffId")] public int BuffId;
    [XmlAttribute("multiplier")] public int Multiplier;
    [XmlAttribute("remainCount")] public int RemainCount;
    [XmlAttribute("buffType")] public string BuffType = ""; // "exp", "seerBean", "dropRate"
}

[Serializable]
public class YiteRogueData
{
    [XmlElement("mode")] public int Mode;
    [XmlElement("isEnd")] public bool IsEnd;
    [XmlArray("petBag")]
    [XmlArrayItem("Pet")] public List<PetData> PetBag = new List<PetData>();
    [XmlElement("buffStatus")] public PetStatus BuffStatus = new PetStatus();
    [XmlArray("buffIds")]
    [XmlArrayItem("int")] public List<int> BuffIds = new List<int>();
    [XmlArray("itemBag")]
    public List<ItemData> ItemBag = new List<ItemData>();
    [XmlElement("floor")] public int Floor;
    [XmlElement("nextPos")] public int NextPos;
    [XmlElement("trace")] public string Trace = "";
    [XmlElement("eventMap")] public string EventMap = "";
    [XmlElement("currentBraveTowerFloor")] public int CurrentBraveTowerFloor = 1;
    [XmlElement("historyBraveTowerFloor")] public int HistoryBraveTowerFloor = 1;
    [XmlElement("currentTrialTowerFloor")] public int CurrentTrialTowerFloor = 1;
    [XmlElement("historyTrialTowerFloor")] public int HistoryTrialTowerFloor = 1;
    [XmlElement("fightAnimSpeed")] public int FightAnimSpeed = 1;

    public void Print(StringBuilder sb, string indent = "")
    {
        sb.AppendLine($"{indent}模式: {Mode}, 是否结束: {IsEnd}");
        sb.AppendLine($"{indent}楼层: {Floor}, 下一个位置: {NextPos}");
        sb.AppendLine($"{indent}宠物数量: {PetBag.Count}, 物品数量: {ItemBag.Count}");
        sb.AppendLine($"{indent}Buff数量: {BuffIds.Count}");
    }
}