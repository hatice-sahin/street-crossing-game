using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftWheelAnimation : MonoBehaviour
{
    private DrivingBehaviour parentScript;
    private readonly Vector3 _transformVector = new Vector3(-1000.0f, 0.0f, 0.0f);
    private void Start()
    {
        var parent = transform.parent.gameObject;
        if (parent.name == "car-passenger-wheels")
        {
            parent = parent.transform.parent.gameObject;
        }
        parentScript = parent.GetComponent<DrivingBehaviour>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.Rotate(_transformVector * Time.deltaTime * (parentScript.currentDrivingSpeed / DrivingBehaviour.MaxDrivingSpeed));
    }
}
