using System;
using System.Collections;
using UnityEngine;

public class OnEnteringGoal : MonoBehaviour {
    public GameObject GoalInfoText;
    public GameObject SecondSpawnArea;
    public GameObject FirstSpawnArea;


    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Pedestrian")){

            if (SecondSpawnArea)
            {
                //SecondSpawnArea.SetActive(true);
                //ScenarioControl.Instance.EndCurrentScenario();
                //ScenarioControl.Instance.goalSpawnTransition = true;

                //SecondSpawnArea.SetActive(false);
                ScenarioControl.Instance.scenarioCompleted = true;
                FirstSpawnArea.SetActive(true);
                //ScenarioControl.Instance.elapsedTime = 0;
                ScenarioControl.Instance.GetComponent<ScenarioControl>().UpdateGeneralTimerView();
                ScenarioControl.Instance.GetComponent<ScenarioControl>().UpdateBonusTimerView();
                //ScenarioControl.Instance.goalSpawnTransition = false;
            }
        }
    }
}