using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using Random = System.Random;

#pragma strict

public class TrafficControl : MonoBehaviour
{
    public GameObject spawnPoint;
    public GameObject despawnPoint;
    public int minCarDensity;
    public int maxCarDensity;

    private const float DistanceBetweenCars = 5.0f;
    private const float TimeToTriggerManualHoldCooldown = 0.5f;
    public float ChanceForManualHold = 1.0f;

    public GameObject currentCar;
    private GameObject _nextCar;
    public GameObject carOnHold;
    public GameObject lastCar;
    private bool _manualHold;
    private bool _nextCarReady;
    private int _carCounter;
    private int _nextDensity;
    private float _nextDistance;
    private bool _firstCarSpawned;
    private bool _initialized;
    private readonly List<GameObject> _CVs = new List<GameObject>();
    private readonly List<GameObject> _AVs = new List<GameObject>();
    private float _timeOfManualHold;
    private bool _npcIsTryingToCrossStreet = false;
    private bool _playerIsTryingToCrossStreet = false;

    public const int trafficTime = 6;

    private void Start()
    {
        var CVprefabs = Resources.LoadAll("CV Prefabs");
        foreach (var obj in CVprefabs)
        {
            if (obj is GameObject vehicle)
            {
                vehicle.tag = "Vehicle";
                _CVs.Add(vehicle);
            }
        }

        var AVprefabs = Resources.LoadAll("AV Prefabs");
        foreach (var obj in AVprefabs)
        {
            if (obj is GameObject vehicle)
            {
                vehicle.tag = "Vehicle";
                _AVs.Add(vehicle);
            }
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (ScenarioControl.Instance.gameOver)
        {
            return;
        }
        if (!ScenarioControl.Instance.transition && !_initialized)
        {
            SpawnInitialCars(18);
            GenerateNextDensity();
        } else if (!_initialized)
        {
            return;
        }

        /*var bonusTime = 0;
        if (ScenarioControl.Instance.urgent)
        {
            bonusTime = ScenarioControl.UrgentTaskTime;
        }
        else
        {
            bonusTime = ScenarioControl.NormalTaskTime;
        }*/
        if ((ScenarioControl.Instance.scenarioIsRunning || ScenarioControl.Instance.goalSpawnTransition) && ScenarioControl.Instance.elapsedTime > trafficTime - 5)
        {
            if (lastCar == null)
            {
                lastCar = currentCar;
            }
            else { 
                if (lastCar.transform.position.x > 1.5)
                {
                    ScenarioControl.Instance.logLastCarPassed();
                }
            }
            return;
        }
        if (ScenarioControl.Instance.goalSpawnTransition)
        {
            return;
        }

        if (!_nextCarReady)
        {
            var index = new Random().Next(_CVs.Count);
            var car = _CVs[index];
            var autonomousCarStopCounter = car.transform.Find("CountTrigger").gameObject;

            if (ScenarioControl.Instance.autonomous && (_carCounter == 0 || _carCounter == _nextDensity))
            {
                var AVindex = new Random().Next(_AVs.Count);
                car = _AVs[AVindex];
                autonomousCarStopCounter.SetActive(true);
                car.GetComponent<DrivingBehaviour>().leadingCar = true;
            }
            else if (_carCounter == 0 || _carCounter == _nextDensity)
            {
                autonomousCarStopCounter.SetActive(false);
                car.GetComponent<DrivingBehaviour>().leadingCar = true;
            } else {
                if (((float)new Random().Next(1, 100)) / 100 > 0.7)
                {
                    var AVindex = new Random().Next(_AVs.Count);
                    car = _AVs[AVindex];
                }
                autonomousCarStopCounter.SetActive(false);
                car.GetComponent<DrivingBehaviour>().leadingCar = false;
            }

            _nextCar = car;
            _nextCarReady = true;
            if (_firstCarSpawned)
            {
                CalculateNextDistance();
            }
        }

        if (!_firstCarSpawned || currentCar.transform.position.x - spawnPoint.transform.position.x >= _nextDistance)
        {
            SpawnObject(_nextCar, spawnPoint.transform.position);
            _firstCarSpawned = true;
            _carCounter++;

            _nextCarReady = false;
        }
    }

    private void SpawnObject(GameObject obj, Vector3 spawnPoint)
    {
        var vehicle = Instantiate(obj, spawnPoint, Quaternion.identity);
        //vehicle.tag = "Vehicle";
        vehicle.transform.rotation = transform.rotation;
        vehicle.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
        vehicle.transform.parent = transform;
        var script = vehicle.GetComponent<DrivingBehaviour>();
        script.despawnPoint = despawnPoint;

        if (_firstCarSpawned)
        {
            script.nextCar = currentCar;
            var distance = _nextDistance;
            if (_carCounter == 0)
            {
                distance -= 3.0f * DistanceBetweenCars;
            }

            script.distanceToNext = distance;
        }
        else if (!_firstCarSpawned && _initialized)
        {
            script.nextCar = currentCar;
            script.distanceToNext = vehicle.GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2 +
                                    currentCar.GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2 +
                                    DistanceBetweenCars;
        }

        currentCar = vehicle;
    }

    private void GenerateNextDensity()
    {
        _nextDensity = new Random().Next(minCarDensity, maxCarDensity);
    }

    private void CalculateNextDistance()
    {
        if (_carCounter >= _nextDensity)
        {
            _nextDistance = DistanceBetweenCars * 4;
            _carCounter = 0;
            GenerateNextDensity();
        }
        else
        {
            _nextDistance = DistanceBetweenCars;
        }

        _nextDistance += _nextCar.GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2;
        _nextDistance += currentCar.GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2;
    }

    public void ManualHold(GameObject player, bool hold, bool usedByNpc = false)
    {
        if (!usedByNpc && _npcIsTryingToCrossStreet || usedByNpc && _playerIsTryingToCrossStreet)
        {
            return;
        }
     
        if (hold && _firstCarSpawned && !_manualHold &&
            (ScenarioControl.Instance.timeUntilNextManualHold == 0 || usedByNpc))
        {

            var distance = player.transform.position.x - spawnPoint.transform.position.x;
            var car = currentCar;

            while (distance > 25.0f && car != null)
            {
                var script = car.GetComponent<DrivingBehaviour>();
                distance = player.transform.position.x - car.transform.position.x;

                if ((distance < 25.0f && distance > 10.0f) && script.currentDistanceToNext >= 15 &&
                    (script.chanceForHold <= ChanceForManualHold || usedByNpc))
                {
                    carOnHold = car;
                    _manualHold = true;
                    if (usedByNpc)
                    {
                        _npcIsTryingToCrossStreet = true;
                    }
                    else
                    {
                        ScenarioControl.Instance.usedManualHolds += 1;
                        _playerIsTryingToCrossStreet = true;
                    }
                    _timeOfManualHold = ScenarioControl.Instance.elapsedTime;
                    break;
                }

                car = script.nextCar;
            }

            if (_manualHold)
            {
                var script = carOnHold.GetComponent<DrivingBehaviour>();
                script.manualHold = true;
            }
        }
        else if (!hold && _manualHold)
        {
            var script = carOnHold.GetComponent<DrivingBehaviour>();
            script.manualHold = false;
            _manualHold = false;
            if (ScenarioControl.Instance.elapsedTime - _timeOfManualHold >= TimeToTriggerManualHoldCooldown &&
                !usedByNpc)
            {
                _playerIsTryingToCrossStreet = false;
                ScenarioControl.Instance.timeUntilNextManualHold = ScenarioControl.ManualHoldCooldown;
            }
            else
            {
                _npcIsTryingToCrossStreet = false;
            }
        }
    }

    private void SpawnInitialCars(int count, bool trafficCleared = true)
    {
        var random = new Random();
        var firstCar = currentCar;
        var lastCar = currentCar;
        
        currentCar = null;

        for (var i = 0; i < count; i++)
        {
            var index = random.Next(_CVs.Count);

            var car = _CVs[index];

            var vector = new Vector3(0, 0, 0);

            if (currentCar != null)
            {
                vector = new Vector3(car.GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2 +
                                     currentCar.GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2 +
                                     DistanceBetweenCars, 0, 0);
                var script = currentCar.GetComponent<DrivingBehaviour>();
                
                script.distanceToNext = vector.x;
                var newPosition = currentCar.transform.position + vector;
                if (!trafficCleared && lastCar.transform.position.x - (vector.x + 3 * DistanceBetweenCars) < newPosition.x)
                {
                    script.nextCar = lastCar;
                    script.distanceToNext = vector.x;
                    break;
                }
                SpawnObject(car, newPosition);
                script.nextCar = currentCar;
            }
            else
            {
                vector = new Vector3(DistanceBetweenCars * 5 + car.GetComponent<MeshFilter>().sharedMesh.bounds.size.z,
                    0, 0);
                SpawnObject(car, spawnPoint.transform.position + vector);
                firstCar = currentCar;
            }
        }

        currentCar = firstCar;
        _initialized = true;
    }

    public List<DrivingBehaviour> GetAllCars()
    {
        var carList = new List<DrivingBehaviour>();
        var currentCar = this.currentCar;
        
        while (currentCar != null)
        {
            carList.Add(currentCar.GetComponent<DrivingBehaviour>());
            currentCar = currentCar.GetComponent<DrivingBehaviour>().nextCar;
        }

        return carList;
    }

    public void ResetTraffic()
    {
        var carList = GetAllCars();
        foreach (var car in carList)
        {
            Destroy(car.transform.gameObject);
        }

        lastCar = null;
        currentCar = null;
        _nextCar = null;
        carOnHold = null;
        _manualHold = false;
        _nextCarReady = false;
        _carCounter = 0;
        _nextDensity = 0;
        _nextDistance = 0;
        _firstCarSpawned = false;
        _initialized = false;
        _npcIsTryingToCrossStreet = false;
        _playerIsTryingToCrossStreet = false;
    }
}