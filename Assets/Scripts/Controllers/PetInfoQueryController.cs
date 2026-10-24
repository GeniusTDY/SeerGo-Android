using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using UnityEngine;

public class PetInfoQueryController : BaseController
{
    private IPetInfoSystem monsterSystem;

    private void Start()
    {
        monsterSystem = this.GetSystem<IPetInfoSystem>();
        Debug.Log("MonsterQueryController: 控制器初始化完成");

        // 测试所有查询功能
        TestAllQueries();
    }

    public void DisplayMonsterInfo(int monsterId)
    {
        var monster = monsterSystem.GetMonsterById(monsterId);
        if (monster != null)
        {
            Debug.Log($"=== 精灵信息 ===");
            Debug.Log($"ID: {monster.ID}");
            Debug.Log($"名称: {monster.DefName}");
            Debug.Log($"类型: {monster.Type}");
            Debug.Log($"HP: {monster.HP}, 攻击: {monster.Atk}, 防御: {monster.Def}");
            Debug.Log($"进化等级: {monster.EvolvingLv}");
            Debug.Log($"可学习技能数: {monster.LearnableMoves?.Count ?? 0}");
        }
        else
        {
            Debug.LogWarning($"未找到ID为 {monsterId} 的精灵");
        }
    }

    public void DisplayEvolutionChain(int monsterId)
    {
        var evolutionChain = monsterSystem.GetEvolutionChain(monsterId);
        if (evolutionChain.Count > 0)
        {
            Debug.Log($"=== 进化链 ===");
            foreach (var monster in evolutionChain)
            {
                Debug.Log($"{monster.DefName} (Lv.{monster.EvolvingLv})");
            }
        }
        else
        {
            Debug.LogWarning($"未找到ID为 {monsterId} 的精灵进化链");
        }
    }

    public void DisplayMonstersByType(int type)
    {
        var monsters = monsterSystem.GetMonstersByType(type);
        Debug.Log($"=== 类型 {type} 的精灵 ===");
        foreach (var monster in monsters)
        {
            Debug.Log($"{monster.DefName} (ID: {monster.ID})");
        }
    }

    public void TestAllQueries()
    {
        Debug.Log("开始测试所有查询功能...");

        // 测试1: 获取所有精灵
      

        // 测试2: 按ID查询
        DisplayMonsterInfo(1); // 布布种子

        // 测试3: 按名称查询
        var monsterByName = monsterSystem.GetMonsterByName("布布花");
        if (monsterByName != null)
        {
            Debug.Log($"按名称查询: 找到 {monsterByName.DefName} (ID: {monsterByName.ID})");
        }

        // 测试4: 按类型查询
        DisplayMonstersByType(1); // 草系精灵

        // 测试5: 进化链查询
        DisplayEvolutionChain(3); // 布布花的进化链

        // 测试6: 可学习技能查询
        var learnableMoves = monsterSystem.GetLearnableMoves(1);
        Debug.Log($"布布种子可学习 {learnableMoves.Count} 个技能");

        // 测试7: 进化条件检查
        bool canEvolve = monsterSystem.CanMonsterEvolve(1, 20); // 布布种子在20级
        Debug.Log($"布布种子在20级是否可以进化: {canEvolve}");
    }

}