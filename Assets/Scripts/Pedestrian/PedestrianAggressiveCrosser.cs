using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianAggressiveCrosser : MonoBehaviour {
	public float initialHoldDelay = 5f;
	public float speed = 2f;
	public TrafficControl traffic;
	public GameObject spawnPoint;
	public GameObject targetPoint;
	public GameObject endTargetPoint;
	private float initialHoldWaitTimer = 0f;
	private bool moveStarted = false;
	private bool finishedCrossing = false;
	private bool done = false;
	private bool dead = false;
	private bool walkAnimSet = false;
	private Animator animator;
	private Transform currentTarget;

	private void Awake() {
		animator = GetComponent<Animator>();
		currentTarget = targetPoint.transform;
		transform.position = spawnPoint.transform.position;
		transform.rotation = spawnPoint.transform.rotation;

	}

	private void Update() {
		var carOnHold = ScenarioControl.Instance.traffic.carOnHold;

		if (carOnHold != null && !finishedCrossing) {
			var carOnHoldScript = carOnHold.GetComponent<DrivingBehaviour>();
			if ((carOnHoldScript.nextCar != null &&
			     carOnHoldScript.nextCar.transform.position.x <= (transform.position.x + 15)) ||
			    carOnHoldScript.currentDrivingSpeed > 0) {
				return;
			}

			moveStarted = true;
		}
	}

	private void FixedUpdate() {
		if (done) {
			return;
		}
		var rotation = transform.rotation;

		if (moveStarted)
		{
			transform.rotation = new Quaternion(0, rotation.y, 0, rotation.w);
		}
		else
		{
			transform.rotation = new Quaternion(0, 0, 0, rotation.w);
		}

		if (ScenarioControl.Instance.scenarioIsRunning) {
			initialHoldWaitTimer += Time.fixedDeltaTime;
		}
		
		if (initialHoldWaitTimer >= initialHoldDelay && !finishedCrossing) {
			traffic.ManualHold(gameObject, true, true);
		}

		if (!animator.enabled) dead = true;
		if (moveStarted && !dead) {
			if (!walkAnimSet) {
				animator.SetTrigger("StartWalking");
				animator.ResetTrigger("StopWalking");
				walkAnimSet = true;
			}

			MoveToCurrentTarget();
		}

		Vector2 pedestrianPos = new Vector2(transform.position.x, transform.position.z);
		Vector2 targetPos = new Vector2(currentTarget.position.x, currentTarget.position.z);
		if (Vector2.Distance(pedestrianPos, targetPos) <= speed * Time.fixedDeltaTime) {
			if (finishedCrossing) {
				EndReached();
			} else if (moveStarted){
				TargetReached();
			}
		}
	}

	private void MoveToCurrentTarget() {
		transform.LookAt(currentTarget);
		var pos = transform.position;
		pos += transform.forward * Time.fixedDeltaTime * speed;
		transform.position = pos;
	}

	private void TargetReached() {
		Debug.Log("Pedestrian arrived at target destination");
		finishedCrossing = true;
		traffic.ManualHold(gameObject, false, true);
		currentTarget = endTargetPoint.transform;
	}

	private void EndReached() {
		Debug.Log("Pedestrian arrived at end destination");
		animator.SetTrigger("StopWalking");
		animator.ResetTrigger("StartWalking");
		traffic.ManualHold(gameObject, false, true);
		walkAnimSet = false;
		done = true;
	}

	public void ResetPosition() {
		if (animator == null) return;
		transform.position = spawnPoint.transform.position;
		transform.rotation = spawnPoint.transform.rotation;
		animator.enabled = true;
		animator.SetTrigger("StopWalking");
		animator.ResetTrigger("StartWalking");
		initialHoldWaitTimer = 0f;
		moveStarted = false;
		finishedCrossing = false;
		dead = false;
		walkAnimSet = false;
		done = false;
		currentTarget = targetPoint.transform;
	}
}