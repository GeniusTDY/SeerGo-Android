using System;
using System.Collections.Generic;

namespace SeerGoGame.Models
{
    /// <summary>
    /// 融合配方结果
    /// </summary>
    [Serializable]
    public class FusionRecipeResult
    {
        /// <summary>结果精灵ID</summary>
        public int Id;

        /// <summary>概率</summary>
        public int Rate;

        /// <summary>珠子ID</summary>
        public int BeadId;
    }

    /// <summary>
    /// 融合配方（主宠+副宠=结果）
    /// </summary>
    [Serializable]
    public class FusionRecipe
    {
        /// <summary>主宠ID（不可颠倒）</summary>
        public int MainId;

        /// <summary>副宠ID列表（一个主宠可对应多个副宠）</summary>
        public List<int> SubIds = new List<int>();

        /// <summary>结果列表</summary>
        public List<FusionRecipeResult> Results = new List<FusionRecipeResult>();

        /// <summary>
        /// 检查给定的副宠ID是否在副宠列表中
        /// </summary>
        public bool ContainsSubId(int subId)
        {
            return SubIds.Contains(subId);
        }
    }

    /// <summary>
    /// 融合公式（材料顺序=结果特性列表）
    /// </summary>
    [Serializable]
    public class FusionFormula
    {
        /// <summary>材料顺序（对应Center槽位1-4）</summary>
        public List<int> Order = new List<int>();

        /// <summary>符合公式时可获得的特性ID列表</summary>
        public List<int> Ids = new List<int>();
    }

    /// <summary>
    /// 融合配置数据包装类
    /// </summary>
    [Serializable]
    public class FusionConfigData
    {
        /// <summary>融合配方列表</summary>
        public List<FusionRecipe> Recipes = new List<FusionRecipe>();

        /// <summary>融合公式列表</summary>
        public List<FusionFormula> Formulas = new List<FusionFormula>();
    }
}
