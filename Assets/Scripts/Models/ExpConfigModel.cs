using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 经验值配置数据模型实现
/// </summary>
namespace SeerGoGame.Models
{
    public class ExpConfigModel : AbstractModel, IExpConfigModel
    {
        /// <summary>
        /// 经验值配置列表
        /// </summary>
        public List<ExpConfig> ExpConfigs { get; set; }

        protected override void OnInit()
        {
            ExpConfigs = new List<ExpConfig>();
            // 模型初始化时加载数据

        }

        public async Task LoadExpConfigAsync()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            string fullPath = $"{path}/PetExp";

            var result = await JsonResourceLoadManager.LoadConfigAsync<ExpConfigWrapper>(fullPath);

            if (result.Success && result.Data?.expConfigs != null)
            {
                ExpConfigs = result.Data.expConfigs;
            }
        }

    }
}
