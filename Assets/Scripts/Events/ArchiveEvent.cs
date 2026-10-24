// ´æµµÊÂ¼þ
namespace SeerGoGame.Events
{
    public struct ArchiveEvent
    {
        public string Type; // "Save" »ò "Load"
        public bool Success;
        public string SaveName;
    }
}