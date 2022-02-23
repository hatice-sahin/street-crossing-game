using UnityEngine;

public class OnExitingFirstSpawn : MonoBehaviour {

    //public GameObject SpawnInfoText;
    public GameObject RushHourInfoText;
    public GameObject FirstSpawnArea;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pedestrian"))
        {
            if (FirstSpawnArea)
            {
                //SpawnInfoText.SetActive(false);
                RushHourInfoText.SetActive(false);
                FirstSpawnArea.SetActive(false);
            }
            ScenarioControl.Instance.elapsedTime = 0; // timer resets when plahyer exits the spawn area. 
            ScenarioControl.Instance.scenarioIsRunning = true;
            ScenarioControl.Instance.spawnEntered = false;
            ScenarioControl.Instance.spawnExited = true;
        }
    }
}
