using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollListener_ : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RecyclableScrollList_ recyclableScrollList;
    public float threshold = .5f; // How close to the bottom the user needs to scroll before the next page is loaded. Adjust this as needed.
    // Start is called before the first frame update
    void Update()
    {
        if (scrollRect.normalizedPosition.y <= threshold && !recyclableScrollList.IsLoading())
        {
            recyclableScrollList.LoadNextPage();
        }
    }
}
