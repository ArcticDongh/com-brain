using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTimeUI : MonoBehaviour
{
    public void Save()
    {
        FW.TimelineManager.Save();
    }

    public void Load()
    {
        FW.TimelineManager.Load(-1);
    }
}
