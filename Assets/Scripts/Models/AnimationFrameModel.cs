using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// 动画帧模型实现
namespace SeerGoGame.Models
{
    public class AnimationFrameModel : AbstractModel, IAnimationFrameModel
    {
        public Dictionary<int, Dictionary<string, int>> AnimationFrames { get; private set; }

        protected override void OnInit()
        {
            AnimationFrames = new Dictionary<int, Dictionary<string, int>>();
        }

        public async Task LoadAnimationConfigAsync()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            string fullPath = $"{path}/petLabelInfo";

            var result = await JsonResourceLoadManager.LoadConfigAsync<AnimationFrameRootWrapper>(fullPath);

            if (result.Success && result.Data?.root?.list != null)
            {
                foreach (var listData in result.Data.root.list)
                {
                    if (listData.item != null)
                    {
                        var frameDict = new Dictionary<string, int>();
                        foreach (var itemData in listData.item)
                        {
                            if (!string.IsNullOrEmpty(itemData.lable))
                            {
                                frameDict[itemData.lable] = itemData.frame;
                            }
                        }
                        AnimationFrames[listData.id] = frameDict;
                    }
                }
                //SeerGoLogger.Log("攻击帧数数据加载成功。");
            }
        }

    }
}