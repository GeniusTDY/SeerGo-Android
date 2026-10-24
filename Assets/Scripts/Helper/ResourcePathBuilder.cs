namespace SeerGoGame.Utilities
{
    /// <summary>
    /// 资源路径构建器 - 路径已迁移到外部 Resources/ 目录
    /// 
    /// 外部路径结构:
    ///   Resources/Audio/           - MP3音频 (通过 ExternalResourcesManager 异步加载)
    ///   Resources/Item/            - 物品图标PNG
    ///   Resources/Maps/bg/         - 地图背景PNG
    ///   Resources/Maps/Npc/        - NPC精灵PNG
    ///   Resources/Maps/Path/       - 地图路径遮罩PNG
    ///   Resources/PetFightImage/   - 战斗精灵PNG
    ///   Resources/PetImage/        - 精灵展示PNG
    /// </summary>
    public static class ResourcePathBuilder
    {
        // 外部资源路径根节点
        private const string MapsBgRoot = "Maps/bg";
        private const string MapsNpcRoot = "Maps/Npc";
        private const string MapsPathRoot = "Maps/Path";
        
        private const string ItemRoot = "Item";
        private const string PetImageRoot = "PetImage";
        private const string PetFightImageRoot = "PetFightImage";
        
        // 音频路径
        private const string AudioFightRoot = "Audio/Fight";
        private const string AudioMapsRoot = "Audio/Maps";

        // 内部Resources路径（未迁移的资源）
        private const string DataRoot = "Data/Maps";
        private const string WorldMapDataRoot = "Data/WorldMap";
        private const string PetsRoot = "Data/Pets";
        private const string SpritesFightButtonRoot = "Sprites/Fight/FuncButton";

        // == 地图相关 ==

        /// <summary>地图背景Sprite路径（外部）</summary>
        public static string GetMapBackgroundPath(int mapId)
        {
            return $"{MapsBgRoot}/{mapId}";
        }

        /// <summary>地图数据资源路径（内部Resources）</summary>
        public static string GetMapDataPath(int mapId)
        {
            return $"{DataRoot}/{mapId}";
        }

        /// <summary>世界地图数据资源路径（内部Resources）</summary>
        public static string GetWorldMapDataPath()
        {
            return $"{WorldMapDataRoot}/WorldMap";
        }

        /// <summary>NPC精灵路径（外部）</summary>
        public static string GetSpritesMapNpcPath()
        {
            return MapsNpcRoot;
        }

        /// <summary>地图路径遮罩路径（外部）</summary>
        public static string GetMapsPathRoot()
        {
            return MapsPathRoot;
        }

        // == 精灵相关 ==

        /// <summary>精灵信息数据路径（内部Resources）</summary>
        public static string GetPetsInfoDataPath()
        {
            return PetsRoot;
        }

        /// <summary>精灵展示图片路径（外部）</summary>
        public static string GetPetImagePath()
        {
            return PetImageRoot;
        }

        /// <summary>战斗精灵icon路径（外部）</summary>
        public static string GetPetFightImagePath()
        {
            return PetFightImageRoot;
        }

        // == UI相关 ==

        /// <summary>战斗按钮事件资源路径（内部Resources）</summary>
        public static string GetFightButtonImagePath()
        {
            return SpritesFightButtonRoot;
        }

        /// <summary>战斗精灵icon（旧名，兼容）</summary>
        public static string GetSpritesPetFightImagePath()
        {
            return PetFightImageRoot;
        }

        // == 物品相关 ==

        /// <summary>物品图标路径（外部）</summary>
        public static string GetItemPath()
        {
            return ItemRoot;
        }

        // == 音频相关 ==

        /// <summary>战斗音乐路径（外部）</summary>
        public static string GetAudioFightPath()
        {
            return AudioFightRoot;
        }

        /// <summary>地图背景音乐路径（外部）</summary>
        public static string GetAudioMapsPath()
        {
            return AudioMapsRoot;
        }

        /// <summary>构建精灵展示图片路径</summary>
        public static string GetPetImageResPath(int petId, int index = 1)
        {
            return $"{PetImageRoot}/Pet_{petId}/{index}";
        }

        /// <summary>构建战斗精灵图片路径</summary>
        public static string GetPetFightImageResPath(int petId, string subPath)
        {
            return $"{PetFightImageRoot}/Pet_{petId}/{subPath}";
        }

        /// <summary>构建物品图标路径</summary>
        public static string GetItemResPath(int itemId)
        {
            return $"{ItemRoot}/{itemId}";
        }

        /// <summary>构建地图BGM路径</summary>
        public static string GetBgmPath(string bgmName)
        {
            return $"{AudioMapsRoot}/{bgmName}";
        }

        /// <summary>构建战斗BGM路径</summary>
        public static string GetFightMusicPath(string musicName)
        {
            return $"{AudioFightRoot}/{musicName}";
        }
    }
}