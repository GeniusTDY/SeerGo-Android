using System.Collections;
namespace SuperScrollView
{  
    public class LogTextItemData : ItemDataBase
    {
        public int mIndex;
        public string mLogText;

        public override void Init(int index, string logText)
        {
            mIndex = index;
            mLogText = logText;
        }
       
        public override void OnIndexChanged(int index, string logText)
        {
            mIndex = index;
            mLogText = logText;
        }

        public override bool IsFilterMatched(string filterStr)
        {
            return mLogText.Contains(filterStr);
        }
    }
}