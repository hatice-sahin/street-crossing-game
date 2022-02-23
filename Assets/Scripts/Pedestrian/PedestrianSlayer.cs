using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianSlayer : MonoBehaviour
{
    Animator animator;
    private CapsuleCollider collider;
    public float destroyDelay = 10f;
    private float timer = 0;
    private bool isDead = false;

	private void Awake() {
		animator = GetComponent<Animator>();
		collider = GetComponent<CapsuleCollider>();
	}

	private void FixedUpdate() {
		if (isDead) {
			timer += Time.fixedDeltaTime;
			if (destroyDelay <= timer) {
				Destroy(gameObject);
			}
		}
	}

	private void OnCollisionEnter(Collision other) {
		if (!other.collider.CompareTag("Vehicle") || other.collider.isTrigger || isDead) return;

		animator.enabled = false;
		isDead = true;
		collider.enabled = false;
	}

	private void OnTriggerEnter(Collider other) {
		
	}


}
