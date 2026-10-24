namespace QFramework
{
    public class AudioSearchKeys : IPoolType, IPoolable
    {
        public string AssetBundleName;

        public string AssetName;

        public void OnRecycled()
        {
            AssetBundleName = null;
            AssetName = null;
        }

        public bool IsRecycled { get; set; }
        
        public static AudioSearchKeys Allocate() => SafeObjectPool<AudioSearchKeys>.Instance.Allocate();

        public void Recycle2Cache() => SafeObjectPool<AudioSearchKeys>.Instance.Recycle(this);
        
        public override string ToString() => $"AudioSearchKeys AssetName:{AssetName} AssetBundleName:{AssetBundleName}";
    }
}