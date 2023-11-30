using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : UniqueMono<MainCamera>
{
    public float smooth_value = 0.2f;

    private Camera camera_ref;
    private float z_axis_backup;
    // Start is called before the first frame update
    void Start()
    {
        camera_ref = GetComponent<Camera>();
        z_axis_backup = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        var pos = Vector3.Lerp(transform.position, PlayerControl.Instance.transform.position, smooth_value);
        pos.z = z_axis_backup;
        transform.position = pos;
    }

    public Vector3 ScreenToWorldPoint(Vector3 pos)
    {
        return camera_ref.ScreenToWorldPoint(pos);
    }

    public Vector3 WorldToScreenPoint(Vector3 pos)
    {
        return camera_ref.WorldToScreenPoint(pos);
    }
}
