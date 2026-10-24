using QFramework;
using System;
using System.Collections.Generic;

/// <summary>
/// 经验值配置数据模型
/// Level: 等级
/// Type_0: 类型0所需经验值
/// Type_2: 类型2所需经验值  
/// Type_3: 类型3所需经验值
/// Type_4: 类型4所需经验值
/// Type_5: 类型5所需经验值
/// </summary>
namespace SeerGoGame.Models
{
    [Serializable]
    public class ExpConfig
    {
        /// <summary>
        /// 等级
        /// </summary>
        public int Level;

        /// <summary>
        /// 类型0所需经验值
        /// </summary>
        public int Type_0;

        /// <summary>
        /// 类型2所需经验值
        /// </summary>
        public int Type_2;

        /// <summary>
        /// 类型3所需经验值
        /// </summary>
        public int Type_3;

        /// <summary>
        /// 类型4所需经验值
        /// </summary>
        public int Type_4;

        /// <summary>
        /// 类型5所需经验值
        /// </summary>
        public int Type_5;
    }

    /// <summary>
    /// JSON数组包装类
    /// </summary>
    [Serializable]
    public class ExpConfigWrapper
    {
        public List<ExpConfig> expConfigs;
    }
}