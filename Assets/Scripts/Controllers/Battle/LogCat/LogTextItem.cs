using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuperScrollView
{   
    public class LogTextItem : MonoBehaviour
    {    
        public TextMeshProUGUI mLogText;
        LogTextItemData mItemData;
        int mItemDataIndex = -1;
        
        public int ItemIndex
        {
            get
            {
                return mItemDataIndex;
            }
            set
            {
                mItemDataIndex = value;
            }
        }      
         
        System.Action<int> mOnClickItemCallBack;
        public void Init(System.Action<int> OnClickItemCallBack = null)
        {
            ClickEventListener listener = ClickEventListener.Get(gameObject);
            listener.SetClickEventHandler(OnButtonClicked);
            mOnClickItemCallBack = OnClickItemCallBack;   
        }

        void OnButtonClicked(GameObject obj)
        {
            if(mOnClickItemCallBack != null)
            {
                mOnClickItemCallBack(mItemDataIndex);
            }
        }

        public void SetItemData(LogTextItemData itemData, int itemIndex)
        {
            mItemData = itemData;
            mItemDataIndex = itemIndex;
            mLogText.text = itemData.mLogText;
            mLogText.GetComponent<ContentSizeFitter>().SetLayoutVertical();
            Vector2 size = mLogText.GetComponent<RectTransform>().rect.size;
            RectTransform tf = gameObject.GetComponent<RectTransform>();
            float y = size.y;
           /* if (y < 75)
            {
                y = 75;
            }*/
            tf.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
        }

    }    
}
