using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    public class PetAttributeCalcSystem : AbstractSystem, IPetAttributeCalcSystem
    {
        private IPersonalitySystem _personalitySystem;
        private IPetInfoSystem _petInfoSystem;
        protected override void OnInit()
        {
            _personalitySystem = this.GetSystem<IPersonalitySystem>();
            _petInfoSystem = this.GetSystem<IPetInfoSystem>();
        }

        /// <summary>
        /// 计算精灵能力值
        /// </summary>
        /// <param name="raceValue">种族值</param>
        /// <param name="individualValue">个体值(0-31)</param>
        /// <param name="effortValue">努力值(0-255)</param>
        /// <param name="level">等级(1-100)</param>
        /// <param name="personalityModifier">性格修正(0.9/1.0/1.1)</param>
        /// <param name="isHP">是否是体力计算</param>
        /// <returns>最终能力值</returns>
        public int CalculateStat(int raceValue, int individualValue,
                                       int effortValue, int level,
                                       float personalityModifier = 1.0f,
                                       bool isHP = false)
        {
            // 验证输入范围
            individualValue = Mathf.Clamp(individualValue, 0, 31);
            effortValue = Mathf.Clamp(effortValue, 0, 255);
            level = Mathf.Clamp(level, 1, 100);

            // 第一步：计算基础值（努力值每4点换算为1点能力）
            int baseValue = (raceValue * 2) + individualValue + (effortValue / 4);

            // 第二步：应用等级系数
            int levelAdjusted = (baseValue * level) / 100;

            if (isHP)
            {
                // 第三步(HP)：加上基础常数
                return levelAdjusted + 10 + level;
            }
            else
            {
                // 第三步(其他能力)：加上基础常数
                int baseStat = levelAdjusted + 5;

                // 第四步：应用性格修正并取整
                float finalStat = baseStat * personalityModifier;
                return Mathf.FloorToInt(finalStat);
            }
        }

        /// <summary>
        /// 批量计算精灵六维能力值
        /// </summary>
        public List<int> CalculateAllStats(PetData petData)
        {
            // 查询性格加成
            PersonalityData personalityData = _personalitySystem.GetPersonalityById(petData.Basic.Personality);
            MonsterData monsterData = _petInfoSystem.GetMonsterById(petData.Id);

            List<int> addNum = new List<int>();

            int Hp = CalculateStat(monsterData.HP, petData.Talent.IV,
                                       petData.Talent.EV.Hp, petData.Exp.Level,
                                       1.0f,
                                       true);
            int Atk = CalculateStat(monsterData.Atk, petData.Talent.IV,
                                 petData.Talent.EV.Atk, petData.Exp.Level,
                                 personalityData.modifiers.m_attack,
                                 false);
            int Def = CalculateStat(monsterData.Def, petData.Talent.IV,
                            petData.Talent.EV.Def, petData.Exp.Level,
                            personalityData.modifiers.m_defence,
                            false);

            int SpAtk = CalculateStat(monsterData.SpAtk, petData.Talent.IV,
                      petData.Talent.EV.SpAtk, petData.Exp.Level,
                      personalityData.modifiers.m_SA,
                      false);

            int SpDef = CalculateStat(monsterData.SpDef, petData.Talent.IV,
                      petData.Talent.EV.SpDef, petData.Exp.Level,
                      personalityData.modifiers.m_SD,
                      false);

            int Spd = CalculateStat(monsterData.Spd, petData.Talent.IV,
              petData.Talent.EV.Spd, petData.Exp.Level,
              personalityData.modifiers.m_speed,
              false);

            addNum.Add(Hp - petData.CurrentStatus.MaxHp);
            addNum.Add(Atk - petData.CurrentStatus.Atk);
            addNum.Add(Def - petData.CurrentStatus.Def);
            addNum.Add(SpAtk - petData.CurrentStatus.SpAtk);
            addNum.Add(SpDef - petData.CurrentStatus.SpDef);
            addNum.Add(Spd - petData.CurrentStatus.Spd);

            petData.CurrentStatus.Hp = Hp;
            petData.CurrentStatus.MaxHp = Hp;
            petData.CurrentStatus.Atk = Atk;
            petData.CurrentStatus.Def = Def;
            petData.CurrentStatus.SpAtk = SpAtk;
            petData.CurrentStatus.SpDef = SpDef;
            petData.CurrentStatus.Spd = Spd;

            return addNum;
        }

    }
}