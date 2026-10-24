using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;

namespace SeerGoGame.Interfaces
{
    public interface IPetInfoSystem : ISystem
    {
        MonsterData GetMonsterById(int id);
        MonsterData GetMonsterByName(string name);
        List<MonsterData> GetMonstersByType(int type);
        List<MonsterData> GetEvolutionChain(int monsterId);
        List<LearnableMove> GetLearnableMoves(int monsterId);
        List<MonsterData> GetMonstersByEvolution(int evolvesFrom);
        bool CanMonsterEvolve(int monsterId, int currentLevel);
        MonsterData GetFirstEvolution(int monsterId);
    }
}