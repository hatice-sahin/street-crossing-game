using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnteringSecondSpawn : MonoBehaviour
{
    public GameObject GoalInfoText;
    public GameObject SecondSpawnArea;
    public GameObject FirstSpawnArea;
    public GameObject SpawnInfoText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pedestrian"))
        {
            if (SecondSpawnArea)
            {
                //GoalInfoText.SetActive(false);
                SpawnInfoText.SetActive(true);
                SecondSpawnArea.SetActive(false);
                ScenarioControl.Instance.scenarioCompleted = true;
                FirstSpawnArea.SetActive(true);
                ScenarioControl.Instance.elapsedTime = 0;
                ScenarioControl.Instance.GetComponent<ScenarioControl>().UpdateGeneralTimerView();
                ScenarioControl.Instance.GetComponent<ScenarioControl>().UpdateBonusTimerView();
                ScenarioControl.Instance.goalSpawnTransition = false;
            }
        }
    }
}
