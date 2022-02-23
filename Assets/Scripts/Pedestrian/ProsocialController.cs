using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProsocialController : MonoBehaviour {
	public ProsocialCrosser mother;
	public ChildCrosser child;

	public void ResetPedestrians() {
		mother.ResetPedestrian();
		child.ResetPedestrian();
	}
}