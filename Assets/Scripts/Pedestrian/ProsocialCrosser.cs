using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProsocialCrosser : MonoBehaviour {
	public float speed = 3f;
	public bool canCross = false;
	public TrafficControl traffic;
	public Transform spawn;
	public Transform target;
	public Transform endTarget;

	private Animator animator;
	private bool finishedCrossing = false;
	private bool done = false;
	private bool walkAnimSet = false;
	private Transform currentTarget;

	private void Start() {
		animator = GetComponent<Animator>();
		currentTarget = target;

		transform.position = spawn.position;
		transform.rotation = spawn.rotation;
	}

	private void FixedUpdate() {
		if (done) {
			return;
		}

		var rotation = transform.rotation;
		if (canCross) {
			transform.rotation = new Quaternion(0, rotation.y, 0, rotation.w);
		}
		else
		{
			transform.rotation = new Quaternion(0, 0, 0, rotation.w);
		}

		var bonusTime = 0;
		if (ScenarioControl.Instance.urgent)
		{
			bonusTime = TrafficControl.trafficTime - 5;
		}
		else
		{
			bonusTime = TrafficControl.trafficTime - 5;
		}

		if (!canCross && ScenarioControl.Instance.elapsedTime > bonusTime &&
		    traffic.currentCar.transform.position.x >= transform.position.x + 5)
		{
			canCross = true;
		}

		if (canCross) {
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
			} else {
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
		Debug.Log("Prosocial pedestrian arrived at target destination");
		finishedCrossing = true;
		currentTarget = endTarget;
	}

	private void EndReached() {
		Debug.Log("Prosocial pedestrian arrived at end destination");
		animator.SetTrigger("StopWalking");
		animator.ResetTrigger("StartWalking");
		walkAnimSet = false;
		done = true;
	}

	public void ResetPedestrian() {
		if (animator == null) return;
		animator.SetTrigger("StopWalking");
		animator.ResetTrigger("StartWalking");
		done = false;
		transform.position = spawn.position;
		transform.rotation = spawn.rotation;
		walkAnimSet = false;
		finishedCrossing = false;
		canCross = false;
		currentTarget = target;
	}
}