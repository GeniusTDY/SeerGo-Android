using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace SeerGoGame.Models
{
    [Serializable]
    public class MapNpcEntities
    {
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("resId")] public string ResId;
        [XmlAttribute("type")] public string Type;
        [XmlAttribute("sort")] public int Sort = 3;
        [XmlAttribute("polygonEnable")] public bool PolygonEnable = true;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("size")] public string Size;
        [XmlAttribute("pos")] public string Position;
        [XmlAttribute("rotation")] public string Rotation = "0,0,0";
        [XmlAttribute("color")] public string Color = "255,255,255,255";
        [XmlAttribute("namePos")] public string NamePosition = "0,0";
        [XmlAttribute("rate")] public int Rate = 100;// 出现概率

        public string description;
        public Dialog dialog;  // 新增dialog字段
        public string transport;
        public EventHandler eventHandler = new EventHandler();
        public Battle battle = new Battle();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Id: {Id}");
            sb.AppendLine($"ResId: {ResId}");
            sb.AppendLine($"Type: {Type}");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"Sort: {Sort}");
            sb.AppendLine($"PolygonEnable: {PolygonEnable}");
            sb.AppendLine($"Size: {Size}");
            sb.AppendLine($"Position: {Position}");
            sb.AppendLine($"Rotation: {Rotation}");
            sb.AppendLine($"Color: {Color}");
            sb.AppendLine($"NamePosition: {NamePosition}");
            sb.AppendLine($"Description: {description}");
            sb.AppendLine($"Dialog: {(dialog?.ToString() ?? "null")}");
            sb.AppendLine($"Transport: {transport}");
            sb.AppendLine($"EventHandler: {(eventHandler?.ToString() ?? "null")}");
            sb.AppendLine($"Battle: {(battle?.ToString() ?? "null")}");
            return sb.ToString();
        }
    }

    [Serializable]
    public class MapNpc : MapNpcEntities
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            sb.AppendLine($"GetBattleSummary(): {GetBattleSummary()}");
            sb.AppendLine($"GetEventHandlerInfo(): {GetEventHandlerInfo()}");
            sb.AppendLine($"GetDialogSummary(): {GetDialogSummary()}");
            return sb.ToString();
        }

        public string GetBattleSummary()
        {
            if (battle == null || battle.branch == null || battle.branch.enemy?.pet == null || battle.branch.enemy.pet.Count == 0)
            {
                return "无战斗配置";
            }

            var summary = new StringBuilder("战斗敌人: ");
            foreach (var pet in battle.branch.enemy.pet)
            {
                summary.Append($"宠物{pet.Id} (Lv.{pet.Level}) ");
            }
            return summary.ToString().Trim();
        }

        public string GetEventHandlerInfo()
        {
            if (eventHandler?.button == null)
            {
                return "无交互事件";
            }

            return $"交互事件: [{eventHandler.button.Type}] {eventHandler.button.action}";
        }

        public string GetDialogSummary()
        {
            if (dialog == null)
            {
                return "无对话框配置";
            }

            return $"对话框: {dialog.content} (选项: {dialog.functionHandler?.func?.Count ?? 0}个)";
        }
    }

    [Serializable]
    public class Dialog
    {
        [XmlElement("content")]
        public string content;

        [XmlElement("functionHandler")]
        public FunctionHandler functionHandler;

        public override string ToString()
        {
            return $"Dialog[Content={content}, FunctionHandler={(functionHandler?.ToString() ?? "null")}]";
        }
    }

    [Serializable]
    public class FunctionHandler
    {
        [XmlElement("func")]
        public List<Func> func = new List<Func>();

        public override string ToString()
        {
            if (func == null || func.Count == 0)
            {
                return "无功能处理";
            }

            var sb = new StringBuilder();
            sb.Append($"功能处理: ");
            foreach (var f in func)
            {
                sb.Append($"{f} ");
            }
            return sb.ToString().Trim();
        }
    }

    [Serializable]
    public class Func
    {
        [XmlAttribute("type")]
        public string type;

        [XmlElement("description")]
        public string description;

        [XmlElement("action")]
        public string action;

        [XmlElement("param")]
        public string param;

        public override string ToString()
        {
            return $"Func[Type={type}, Description={description}, Action={action}, Param={param}]";
        }
    }

    [Serializable]
    public class EventHandler
    {
        public Button button = new Button();

        public override string ToString()
        {
            return button?.ToString() ?? "Button: null";
        }
    }

    [Serializable]
    public class Button
    {
        [XmlAttribute("type")] public string Type;
        public string action;
        public string param;

        [XmlElement("reward")]
        public List<Reward> reward = new List<Reward>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Button[Type={Type}, Action={action}, Param={param}");
            if (reward != null && reward.Count > 0)
            {
                sb.Append(", Rewards=[");
                foreach (var r in reward)
                    sb.Append($"{r} ");
                sb.Append("]");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    [Serializable]
    public class Reward
    {
        [XmlAttribute("id")] public int Id;
        [XmlAttribute("num")] public int Num;
        [XmlAttribute("rate")] public int Rate;

        public override string ToString()
        {
            return $"Reward[Id={Id}, Num={Num}, Rate={Rate}%]";
        }
    }

    [Serializable]
    public class Battle
    {
        public Branch branch = new Branch();

        public override string ToString()
        {
            return branch?.ToString() ?? "Branch: null";
        }
    }

    [Serializable]
    public class Branch
    {
        [XmlAttribute("id")] public string Id = "default";

        public Settings settings = new Settings();
        public Enemy enemy = new Enemy();
        public WinHandler winHandler = new WinHandler();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"  Branch Id: {Id}");
            sb.AppendLine($"  Settings: {settings}");
            sb.AppendLine($"  Enemy: {enemy}");
            sb.AppendLine($"  WinHandler: {winHandler}");
            return sb.ToString();
        }
    }

    [Serializable]
    public class Settings
    {
        [XmlAttribute("count")] public int Count = 6;
        [XmlAttribute("capture")] public bool Capture = true;
        [XmlAttribute("mod")] public bool Mod = true;
        [XmlElement("mode")] public string mode = "";  // 新增mode字段

        public override string ToString()
        {
            return $"Settings[Count={Count}, Capture={Capture}, Mod={Mod}, Mode={mode}]";
        }
    }

    [Serializable]
    public class Enemy
    {
        [XmlAttribute("type")] public string Type = "NORMAL";  // NORMAL BOSS BRAVETOWER

        [XmlElement("pet")]
        public List<Pet> pet = new List<Pet>();

        public override string ToString()
        {
            if (pet == null || pet.Count == 0)
            {
                return "无宠物";
            }

            var sb = new StringBuilder();
            sb.Append("宠物列表: ");
            foreach (var p in pet)
            {
                sb.Append($"{p} ");
            }
            return sb.ToString().Trim();
        }
    }

    [Serializable]
    public class InitBuffData
    {
        [XmlAttribute("id")] public int Id;
        [XmlAttribute("duration")] public string Duration; // null = 使用BuffConfig中的默认值
        [XmlAttribute("effectValue")] public string EffectValue; // null = 使用BuffConfig中的默认值

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Buff[Id={Id}");
            if (!string.IsNullOrEmpty(Duration))
                sb.Append($", Duration={Duration}");
            if (!string.IsNullOrEmpty(EffectValue))
                sb.Append($", EffectValue={EffectValue}");
            sb.Append("]");
            return sb.ToString();
        }
    }

    [Serializable]
    public class InitBuffContainer
    {
        [XmlElement("buff")]
        public List<InitBuffData> Buffs = new List<InitBuffData>();

        public override string ToString()
        {
            if (Buffs == null || Buffs.Count == 0)
                return "无";
            return string.Join(", ", Buffs);
        }
    }

    [Serializable]
    public class Pet
    {
        [XmlAttribute("id")] public string Id = "-38";
        [XmlAttribute("level")] public int Level = 12;

        [XmlElement("loopSkill")] public string LoopSkill = "10050";
        [XmlElement("status")] public Status Status = new Status();
        [XmlElement("initBuff")] public InitBuffContainer InitBuff = new InitBuffContainer();
        [XmlElement("superSkill")] public string SuperSkill = "";  // 新增superSkill字段

        public List<InitBuffData> GetInitBuffList()
        {
            return InitBuff?.Buffs ?? new List<InitBuffData>();
        }

        public override string ToString()
        {
            var buffList = GetInitBuffList();
            var buffStr = buffList != null && buffList.Count > 0 ? string.Join(", ", buffList) : "无";

            return $"Pet[Id={Id}, Level={Level}, LoopSkill={LoopSkill}, SuperSkill={SuperSkill}, Status={Status}, InitBuff=[{buffStr}]]";
        }
    }

    [Serializable]
    public class Status
    {
        [XmlElement("hp")] public int Hp = 0;
        [XmlElement("spd")] public int Spd = 0;

        public override string ToString()
        {
            return $"Status[Hp={Hp}, Spd={Spd}]";
        }
    }

    [Serializable]
    public class WinHandler
    {
        [XmlElement("button")]
        public List<WinButton> button = new List<WinButton>();  // 改为列表

        public override string ToString()
        {
            if (button == null || button.Count == 0)
            {
                return "无获胜处理";
            }

            var sb = new StringBuilder();
            sb.Append("获胜按钮: ");
            foreach (var b in button)
            {
                sb.Append($"{b} ");
            }
            return sb.ToString().Trim();
        }
    }

    [Serializable]
    public class WinButton
    {
        [XmlAttribute("type")] public string Type = "auto";

        [XmlElement("action")] public string action = "setNpcParam";
        [XmlElement("param")] public List<string> param = new List<string>();
        [XmlElement("condition")] public string condition = "random[0~100]LTE30";

        public override string ToString()
        {
            var paramStr = param != null ? string.Join(", ", param) : "";
            return $"WinButton[Type={Type}, Action={action}, Params={paramStr}, Condition={condition}]";
        }
    }
}