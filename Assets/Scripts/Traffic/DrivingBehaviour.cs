using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = System.Random;

public class DrivingBehaviour : MonoBehaviour
{
    public GameObject despawnPoint;
    public GameObject nextCar;
    public float distanceToNext;

    public bool manualHold;

    public const float MaxDrivingSpeed = 13.89f;
    private const float Acceleration = 0.55f;
    public float currentDrivingSpeed;
    public float currentDistanceToNext = 0f;
    private bool _playerDetected;
    private bool _vehicleDetected;
    public List<Collider> colliders = new List<Collider>();
    public bool leadingCar = false;
    public float chanceForHold = 0f;
    private bool _test;

    GameObject baseLights;
    GameObject headLights;

    private void Start()
    {
        currentDrivingSpeed = MaxDrivingSpeed;
        chanceForHold = ((float)new Random().Next(1, 100)) / 100;

        Transform lights = transform.Find("Lights");
        if (lights != null)
        {
            baseLights = lights.gameObject;
            baseLights.SetActive(false);
        }

        lights = transform.Find("headlights");
        if(lights != null)
        {
            headLights = lights.gameObject;
            headLights.SetActive(false);
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        SetPlayerDetected();

        transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        var parent = transform.parent;
        transform.position =
            new Vector3(transform.position.x, parent.transform.position.y, parent.transform.position.z);

        var nextCarSpeed = 0.0f;
        var nextCarIsNull = true;

        if (nextCar != null)
        {
            nextCarIsNull = false;
            currentDistanceToNext = nextCar.transform.position.x - transform.position.x;
            var script = nextCar.GetComponent<DrivingBehaviour>();
            nextCarSpeed = script.currentDrivingSpeed;
            _vehicleDetected = script._vehicleDetected || script._playerDetected || script.manualHold;
        }

        if (!_playerDetected && !manualHold && _vehicleDetected)
        {
            var distanceDifference = currentDistanceToNext - distanceToNext;

            if (distanceDifference > 15.0f)
            {
                distanceDifference = 15.0f;
            }
            else if (distanceDifference < -3.0f)
            {
                distanceDifference = -3.0f;
            }

            var accelerationFactor = (float) (-1.0f / 324.0f * Math.Pow((distanceDifference - 15.0f), 2.0f) + 1.0f);

            currentDrivingSpeed = MaxDrivingSpeed * accelerationFactor;

            if (currentDrivingSpeed < nextCarSpeed)
            {
                currentDrivingSpeed = nextCarSpeed;
            }
        }
        else if ((_playerDetected || manualHold) && currentDrivingSpeed > 0.0f)
        {
            currentDrivingSpeed -= Acceleration;
        }
        else if (!_playerDetected && !_vehicleDetected && !manualHold && currentDrivingSpeed < MaxDrivingSpeed &&
                 (nextCarIsNull || currentDistanceToNext >= distanceToNext - 2))
        {
            currentDrivingSpeed += Acceleration;
            if (!nextCarIsNull && currentDrivingSpeed > nextCarSpeed)
            {
                currentDrivingSpeed = nextCarSpeed;
            }
        }
        else if (currentDistanceToNext - distanceToNext < -1)
        {
            currentDrivingSpeed -= Acceleration;
        }

        //enable lights
        if (_playerDetected || manualHold)
        {
            if (baseLights != null) baseLights.SetActive(true);
            if(manualHold && headLights != null) headLights.SetActive(true);
        }
        else
        {
            if (baseLights != null) baseLights.SetActive(false);
            if (!manualHold && headLights != null) headLights.SetActive(false);

        }

        if (currentDrivingSpeed < 0)
        {
            currentDrivingSpeed = 0.0f;
        }
        else if (currentDrivingSpeed > MaxDrivingSpeed)
        {
            currentDrivingSpeed = MaxDrivingSpeed;
        }

        transform.position += transform.forward * currentDrivingSpeed * Time.fixedDeltaTime;

        if (transform.position.x > despawnPoint.transform.position.x)
        {
            Destroy(transform.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            var obj = contact.otherCollider;
            if (obj.CompareTag("Pedestrian") && !ScenarioControl.Instance.goalSpawnTransition) //if (obj.CompareTag("Pedestrian") && ScenarioControl.Instance.SpawnInfoText && !ScenarioControl.Instance.goalSpawnTransition)
            {
                    ScenarioControl.Instance.playerIsDead = true;
            }
        }
    }

    private void OnTriggerEnter(Collider obj)
    {
        if ((ScenarioControl.Instance.autonomous && obj.CompareTag("Pedestrian")) || obj.CompareTag("CrossingNPC"))
        {
            colliders.Add(obj);
        }
    }

    private void OnTriggerExit(Collider obj)
    {
        if ((ScenarioControl.Instance.autonomous && obj.CompareTag("Pedestrian") && colliders.Contains(obj)) || obj.CompareTag("CrossingNPC"))
        {
            colliders.Remove(obj);
        }
    }

    private void SetPlayerDetected()
    {
        foreach (Collider collider in colliders)
        {
            if (nextCar == null || nextCar.transform.position.x > collider.transform.position.x)
            {
                _playerDetected = true;
                return;
            }
        }

        _playerDetected = false;
    }
}