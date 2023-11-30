using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    public Transform sight_bar_ref;
    public TMPro.TMP_Text text;

    private Transform track;
    private float sight_progress;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (track is not null)
        {
            transform.position = MainCamera.Instance.WorldToScreenPoint(track.position);
        }
    }

    public void SetTrack(Transform target)
    {
        track = target;
    }

    public float SightProgress
    {
        get { return sight_progress; }
        set { sight_progress = Mathf.Clamp(value, 0, 1);
            var sc = sight_bar_ref.localScale;
            sc.x = sight_progress;
            sight_bar_ref.localScale = sc;
        }
    }

    public string TextInfo
    {
        get { return text.text; }
        set { text.text = value; }
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
