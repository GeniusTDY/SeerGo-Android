using QFramework;

using SeerGoGame.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 经验值配置数据模型接口
/// </summary>
namespace SeerGoGame.Interfaces
{
    public interface IExpConfigModel : IModel
    {
        /// <summary>
        /// 经验值配置列表
        /// </summary>
        List<ExpConfig> ExpConfigs { get; set; }

        Task LoadExpConfigAsync();

    }
}