using QFramework;
using SeerGoGame.Models;
using UnityEngine;

/// <summary>
/// 经验值配置系统接口
/// </summary>
namespace SeerGoGame.Interfaces
{
    public interface IExpConfigSystem : ISystem
    {
        /// <summary>
        /// 获取指定等级和类型的经验值
        /// </summary>
        /// <param name="level">等级</param>
        /// <param name="type">类型索引</param>
        /// <returns>经验值</returns>
        int GetExpValue(int level, int type);

        /// <summary>
        /// 根据等级获取经验值配置
        /// </summary>
        /// <param name="level">等级</param>
        /// <returns>经验值配置</returns>
        ExpConfig GetByLevel(int level);

        void InitializeData();

        // 精灵等级升级计算
        void CalcExp(PetData petData, int expGained, Transform transform);
    }
}