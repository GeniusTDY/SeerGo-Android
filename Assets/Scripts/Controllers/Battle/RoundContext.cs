using SeerGoGame.Models;
using System.Collections.Generic;

namespace SeerGoGame.Controllers.Battle
{
    /// <summary>
    /// 回合上下文 - 存储本回合的所有数据
    /// </summary>
    public class RoundContext
    {
        // 静态实例
        private static RoundContext _instance;

        // 静态属性，首次访问时自动创建实例
        public static RoundContext Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RoundContext();
                }
                return _instance;
            }
        }
        // 技能伤害计算次数
        public int PlayerSkillCaclDamageCount { get; set; } = 1;
        public int EnemySkillCaclDamageCount { get; set; } = 1;

        // 技能数据
        public SkillData PlayerSkill { get; set; }
        public SkillData EnemySkill { get; set; }

        // 技能先制
        public int PlayerSkillPriority { get; set; }
        public int EnemySkillPriority { get; set; }

        // 技能索引（用于PP扣减）
        public int PlayerSkillIndex { get; set; }
        public int EnemySkillIndex { get; set; }

        // 伤害数据
        public int PlayerDamage { get; set; }
        public int EnemyDamage { get; set; }

        // 技能威力
        public int PlayerSkillPower { get; set; }
        public int EnemySkillPower { get; set; }

        // 精灵属性
        public int PlayerType { get; set; }
        public int EnemyType { get; set; }

        // 命中状态
        public bool IsPlayerSkillHit { get; set; }
        public bool IsEnemySkillHit { get; set; }

        // 暴击状态
        public bool IsPlayerSkillCrit { get; set; }
        public bool IsEnemySkillCrit { get; set; }

        // 实际命中率
        public int PlayerSkillAccuracy { get; set; }
        public int EnemySkillAccuracy { get; set; }

        // 实际暴击率
        public int PlayerSkillCritRate { get; set; }
        public int EnemySkillCritRate { get; set; }

        // 精灵ID
        public int PlayerPetId { get; set; }
        public int EnemyPetId { get; set; }

        // 先制顺序
        public TurnPriority Priority { get; set; }

        // 先制双倍伤害
        public bool isPlayerFirstDoubleDamage { get; set; } = false;
        public bool isMonsterFirstDoubleDamage { get; set; } = false;


        // 技能连续使用次数
        public Dictionary<int, int> PlayerSkillUseCount { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> EnemySkillUseCount { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// 重置回合数据
        /// </summary>
        public void Reset()
        {
            PlayerSkill = null;
            EnemySkill = null;
            PlayerSkillPriority = 0;
            EnemySkillPriority = 0;
            PlayerSkillIndex = -1;
            EnemySkillIndex = -1;
            PlayerDamage = 0;
            EnemyDamage = 0;
            PlayerSkillPower = 0;
            EnemySkillPower = 0;
            PlayerSkillCaclDamageCount = 1;
            EnemySkillCaclDamageCount = 1;
            PlayerType = 0;
            EnemyType = 0;
            isPlayerFirstDoubleDamage = false;
            isMonsterFirstDoubleDamage = false;
            IsPlayerSkillHit = false;
            IsEnemySkillHit = false;
            IsPlayerSkillCrit = false;
            IsEnemySkillCrit = false;
            PlayerSkillAccuracy = 100;
            EnemySkillAccuracy = 100;
            PlayerSkillCritRate = 0;
            EnemySkillCritRate = 0;
            PlayerPetId = 0;
            EnemyPetId = 0;
        }

        /// <summary>
        /// 是否是道具回合（玩家无技能）
        /// </summary>
        public bool IsItemRound()
        {
            return PlayerSkill == null && EnemySkill != null;
        }

        /// <summary>
        /// 是否是切换回合（玩家无技能，敌方可能切换）
        /// </summary>
        public bool IsSwitchRound()
        {
            return PlayerSkill == null;
        }

        /// <summary>
        /// 清空所有连击计数
        /// </summary>
        public void ClearAllSkillCounts(bool isPlayer)
        {
            if (isPlayer)
                PlayerSkillUseCount.Clear();
            else
                EnemySkillUseCount.Clear();
        }

        /// <summary>
        /// 清除静态实例（场景切换时调用）
        /// </summary>
        public static void ClearStaticInstance()
        {
            if (_instance != null)
            {
                _instance.PlayerSkillUseCount?.Clear();
                _instance.EnemySkillUseCount?.Clear();
                _instance = null;
            }
        }
    }

    /// <summary>
    /// 先制优先级
    /// </summary>
    public enum TurnPriority
    {
        PlayerFirst,
        EnemyFirst
    }
}