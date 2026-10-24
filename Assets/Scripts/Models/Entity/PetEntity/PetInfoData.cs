using System;
using System.Collections.Generic;

/// <summary>
/// 赛尔号精灵基础数据模型
/// 包含精灵的所有基本属性、进化信息和可学习技能
/// </summary>
namespace SeerGoGame.Models
{
    [Serializable]
    public class MonsterData
    {
        // ==================== 基础信息 ====================
        /// <summary>精灵唯一标识ID</summary>
        public int ID;

        /// <summary>精灵默认显示名称</summary>
        public string DefName;

        /// <summary>精灵种族类型 (1:草, 2:水, 3:火, 4:飞行, 5:电等)</summary>
        public int Type;

        /// <summary>成长类型 (影响属性成长曲线)</summary>
        public int GrowthType;

        // ==================== 基础战斗属性 ====================
        /// <summary>生命值</summary>
        public int HP;

        /// <summary>物理攻击力</summary>
        public int Atk;

        /// <summary>物理防御力</summary>
        public int Def;

        /// <summary>特殊攻击力</summary>
        public int SpAtk;

        /// <summary>特殊防御力</summary>
        public int SpDef;

        /// <summary>速度</summary>
        public int Spd;

        // ==================== 战斗收益相关 ====================
        /// <summary>被击败时提供的经验值</summary>
        public int YieldingExp;

        /// <summary>捕获概率 (数值越大越容易捕获)</summary>
        public int CatchRate;

        /// <summary>被击败时提供的努力值数组 [hp, atk, def, sp_atk, sp_def, spd]</summary>
        public int[] YieldingEV;

        // ==================== 进化系统 ====================
        /// <summary>进化前精灵ID (0表示最初形态)</summary>
        public int EvolvesFrom;

        /// <summary>进化后精灵ID (0表示最终形态)</summary>
        public int EvolvesTo;

        /// <summary>进化所需等级</summary>
        public int EvolvingLv;

        // ==================== 精灵特性标志 ====================
        /// <summary>是否可放生 (0:可放生, 1:不可放生)</summary>
        public int FreeForbidden;

        /// <summary>是否可作为主宠融合 (0:不可, 1:可)</summary>
        public int FuseMaster;

        /// <summary>是否可作为副宠融合 (0:不可, 1:可)</summary>
        public int FuseSub;

        /// <summary>性别 (0:无性别, 1:雄性, 2:雌性)</summary>
        public int Gender;

        /// <summary>精灵类别ID (所有第一形态为同一ID的精灵为一类)</summary>
        public int PetClass;

        // ==================== 战斗力系统参数 ====================
        /// <summary>形态系数 (用于战斗力计算，0-1代表普通，1代表超进化)</summary>
        public float FormParam;

        /// <summary>性格属性系数 (0-6代表不同性格类型)</summary>
        public int CharacterAttrParam;

        /// <summary>精灵档次系数 (0-5代表不同稀有度档次)</summary>
        public float GradeParam;

        /// <summary>是否附带特性 (0:无, 1:有)</summary>
        public int AddSeParam;

        // ==================== 特殊标识 ====================
        /// <summary>是否为大型精灵</summary>
        public bool IsLarge;

        /// <summary>是否为稀有精灵</summary>
        public bool IsRareMon;

        /// <summary>变异精灵ID (0:不能变异, 非0:可变异为指定ID)</summary>
        public int VariationID;

        // ==================== 技能系统 ====================
        /// <summary>可学习技能列表 (包含技能ID和学习等级)</summary>
        public List<LearnableMove> LearnableMoves;
    }

    /// <summary>
    /// 可学习技能数据模型
    /// </summary>
    [Serializable]
    public class LearnableMove
    {
        /// <summary>技能唯一标识ID</summary>
        public int ID;

        /// <summary>学习该技能所需的精灵等级</summary>
        public int LearningLv;
    }

    [Serializable]
    public class MonsterConfig
    {
        public List<MonsterData> Monsters;
    }
}