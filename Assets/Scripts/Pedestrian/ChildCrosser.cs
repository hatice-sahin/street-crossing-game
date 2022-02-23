using System;
using UnityEngine;

public class ChildCrosser : MonoBehaviour {
	public float speed = 5;
	public float startFollowDistance = 3;
	public float stopFollowDistance = 1;
	public Transform mother;

	private bool following = false;
	private Animator animator;

	private void Start() {
		animator = GetComponent<Animator>();
	}

	private void FixedUpdate()
	{
		var canCross = mother.GetComponent<ProsocialCrosser>().canCross;
		var rotation = transform.rotation;
		
		if (canCross){
			transform.rotation = new Quaternion(0, rotation.y, 0, rotation.w);
		}
		else
		{
			transform.rotation = new Quaternion(0, 0, 0, rotation.w);
		}
		
		Vector2 childPos = new Vector2(transform.position.x, transform.position.z);
		Vector2 motherPos = new Vector2(mother.position.x, mother.position.z);
		if (canCross && !following && Vector2.Distance(childPos, motherPos) >= startFollowDistance) {
			following = true;
			animator.SetTrigger("StartRunning");
			animator.ResetTrigger("StopWalking");
		}

		if (following && Vector2.Distance(childPos, motherPos) <= stopFollowDistance) {
			following = false;
			animator.ResetTrigger("StartRunning");
			animator.SetTrigger("StopWalking");
		}

		if (following) {
			transform.LookAt(mother);
			transform.position += speed * Time.fixedDeltaTime * transform.forward;
		}
	}

	public void ResetPedestrian() {
		if (animator == null) return;
		Vector3 pos = mother.position;
		var dir = mother.rotation;
		pos.x -= 1;
		transform.position = pos;
		transform.rotation = dir;
		following = false;
		animator.ResetTrigger("StartRunning");
		animator.SetTrigger("StopWalking");
	}
}
