namespace QFramework
{
    internal class ClipPrepareModeController
    {
        private IClipPrepareMode mPrepareMode;
        internal IClipPrepareMode PrepareMode => mPrepareMode ?? (mPrepareMode = ByLoaderAsync);
        internal readonly PrepareClipBySetUp BySetUp = new PrepareClipBySetUp();
        internal readonly PrepareClipByLoaderAsync ByLoaderAsync = new PrepareClipByLoaderAsync();
        internal readonly PrepareClipByLoaderSync ByLoaderSync = new PrepareClipByLoaderSync();

        internal void ChangePrepareMode(IClipPrepareMode mode)
        {
            if (PrepareMode == mode)
            {
                return;
            }

            PrepareMode?.UnPrepareClip();
            mPrepareMode = mode;
        }
    }
}