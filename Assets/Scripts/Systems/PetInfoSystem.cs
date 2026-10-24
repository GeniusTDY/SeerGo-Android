using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 精灵系统实现 - 处理所有数据查询逻辑
/// </summary>
namespace SeerGoGame.Systems
{
    public class PetInfoSystem : AbstractSystem, IPetInfoSystem
    {
        private IPetInfoModel petInfoModel;

        protected override void OnInit()
        {
            petInfoModel = this.GetModel<IPetInfoModel>();
        }

        public MonsterData GetMonsterById(int id)
        {
            petInfoModel.MonsterDict.TryGetValue(id, out MonsterData monster);
            return monster;
        }

        public MonsterData GetMonsterByName(string name)
        {
            petInfoModel.NameToMonsterDict.TryGetValue(name, out MonsterData monster);
            return monster;
        }

        public List<MonsterData> GetMonstersByType(int type)
        {
            return petInfoModel.MonsterDict.Values.Where(m => m.Type == type).ToList();
        }

        public List<MonsterData> GetEvolutionChain(int monsterId)
        {
            var evolutionChain = new List<MonsterData>();
            var currentMonster = GetMonsterById(monsterId);

            if (currentMonster == null) return evolutionChain;

            // 向上追溯进化链（从初始形态开始）
            //  var ancestors = new List<MonsterData>();
            var current = currentMonster;

            /* while (current != null && current.EvolvesFrom != 0)
             {
                 var ancestor = GetMonsterById(current.EvolvesFrom);
                 if (ancestor != null)
                 {
                     ancestors.Insert(0, ancestor);
                     current = ancestor;
                 }
                 else
                 {
                     break;
                 }
             }

             evolutionChain.AddRange(ancestors);
             evolutionChain.Add(currentMonster);*/

            // 向下追溯进化链
            //   current = currentMonster;
            while (current != null && current.EvolvesTo != 0)
            {
                var next = GetMonsterById(current.EvolvesTo);
                if (next != null)
                {
                    evolutionChain.Add(next);
                    current = next;
                }
                else
                {
                    break;
                }
            }

            return evolutionChain;
        }

        // 获取初始形态（最上级进化前形态）
        public MonsterData GetFirstEvolution(int monsterId)
        {
            // 1. 获取当前怪物数据，做空值判断
            var currentMonster = GetMonsterById(monsterId);
            if (currentMonster == null) return null;

            const int maxLoopCount = 5;
            int currentLoopCount = 0;

            // 3. 循环查找最上级，满足两个终止条件之一即退出：
            //    a. 当前怪物无前置进化（EvolvesFrom == 0）
            //    b. 达到最大循环次数
            while (currentMonster.EvolvesFrom != 0 && currentLoopCount < maxLoopCount)
            {
                currentMonster = GetMonsterById(currentMonster.EvolvesFrom);

                if (currentMonster == null) break;

                currentLoopCount++;
            }

            return currentMonster;
        }


        public List<LearnableMove> GetLearnableMoves(int monsterId)
        {
            var monster = GetMonsterById(monsterId);
            return monster?.LearnableMoves ?? new List<LearnableMove>();
        }

        public List<MonsterData> GetMonstersByEvolution(int evolvesFrom)
        {
            return petInfoModel.MonsterDict.Values
                .Where(m => m.EvolvesFrom == evolvesFrom)
                .ToList();
        }

        public bool CanMonsterEvolve(int monsterId, int currentLevel)
        {
            var monster = GetMonsterById(monsterId);
            return monster != null && monster.EvolvesTo != 0 && currentLevel >= monster.EvolvingLv;
        }

    }
}