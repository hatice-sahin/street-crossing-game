using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraColliderSyncer : MonoBehaviour
{
    //add these inside your class
    //Make sure you bind this in Unity by dragging the OVRPlayerController from the Hierarchy to the script
    public GameObject player;
    CharacterController character;
    public GameObject centerEyeAnchor;

    void Awake()
    {
        character = player.GetComponent<CharacterController>();
    }

    void Update()
    {
        character.center = centerEyeAnchor.transform.localPosition;
    }
}
