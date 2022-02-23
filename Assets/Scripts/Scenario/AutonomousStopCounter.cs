using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonomousStopCounter : MonoBehaviour
{
    private void OnTriggerEnter(Collider obj)
    {
        if (ScenarioControl.Instance.autonomous && obj.CompareTag("Pedestrian"))
        {
            ScenarioControl.Instance.usedAutonomousHolds += 1;
        }
    }
}
