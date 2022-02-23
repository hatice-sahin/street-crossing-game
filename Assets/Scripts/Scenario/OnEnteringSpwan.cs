using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnteringSpwan : MonoBehaviour
{
    public GameObject particleSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pedestrian"))
        {
            ScenarioControl.Instance.spawnEntered = true;
            ScenarioControl.Instance.spawnExited = false;
            particleSystem.SetActive(false);
        }
    }
}
