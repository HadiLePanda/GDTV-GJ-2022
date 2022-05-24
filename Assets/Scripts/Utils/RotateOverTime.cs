using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public Vector3 StartRotation;
    public Vector3 EndRotation;
    public float duration;
    Vector3 lastRotation;
    float endTime;
    // Start is called before the first frame update
    void Start()
    {
        lastRotation = StartRotation;
        endTime = Time.time + duration;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time >= endTime)
        {
            enabled = false;
        }
        transform.rotation = Quaternion.Euler( Vector3.Lerp(lastRotation, EndRotation, Time.time / endTime));
    }
}
