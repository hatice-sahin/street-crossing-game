using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GazeReticleController : MonoBehaviour {
	public Color idleColor = Color.white;
	public Color activeColor = Color.white;
	public Sprite idle;
	public Sprite active;
	public SpriteRenderer sr;
	public VRInputModule inputModule;

	private void FixedUpdate() {
		UpdateReticle();
	}

	private void UpdateReticle() {
		PointerEventData data = inputModule.GetData();
		
		if (data.pointerCurrentRaycast.gameObject != null && data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<Button>() != null) {
			sr.sprite = active;
			sr.color = activeColor;
		}
		else {
			sr.sprite = idle;
			sr.color = idleColor;
		}
	}
}
