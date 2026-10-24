using QFramework;

namespace SeerGoGame.Interfaces
{
    public interface IAchievementSystem : ISystem
    {
        void CheckMapExplorationAchievement(int mapId);
        void CheckNpcInteractionAchievement(string npcId);
        void CheckMapChangeAchievement(int fromMapId, int toMapId);
    }
}
