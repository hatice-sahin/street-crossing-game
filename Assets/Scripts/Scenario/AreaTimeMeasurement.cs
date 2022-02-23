using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTimeMeasurement : MonoBehaviour
{
    public bool playerInArea = false;
    public float playerInAreaTime = 0.0f;
    public float firstEnterTime = 0.0f;
    public float firstExitTime = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerInArea)
        {
            playerInAreaTime += Time.fixedDeltaTime;
        }
    }
    
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Pedestrian") && col.name == "Player")
        {
            if (firstEnterTime <= 0.0f)
            {
                firstEnterTime = ScenarioControl.Instance.elapsedTime; //how long participant spends time on the street.
                Debug.Log(gameObject.name+" Entered" + firstEnterTime);

            }
            playerInArea = true;
        }
    }

    public void setEnterTimeToNow()
    {
        Debug.Log("asdfasdfdasdfasdf");
        if (firstEnterTime <= 0.0f)
        {
            firstEnterTime = ScenarioControl.Instance.elapsedTime; //how long participant spends time on the street.
            Debug.Log(gameObject.name + " Entered set manually" + firstEnterTime);
        }
    }
    
    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Pedestrian") && col.name == "Player")
        {
            if (firstExitTime <= 0.0f)
            {
                firstExitTime = ScenarioControl.Instance.elapsedTime;
                Debug.Log(gameObject.name + "Exited" + firstExitTime);

            }
            playerInArea = false;
        }
    }

    public void ResetTimers()
    {
        Debug.Log("TImer Reset");
        playerInArea = false;
        playerInAreaTime = 0.0f;
        firstEnterTime = 0.0f;
        firstExitTime = 0.0f;
    }
}
