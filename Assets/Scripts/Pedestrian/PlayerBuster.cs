using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class PlayerBuster : MonoBehaviour
{
	private void OnTriggerEnter(Collider other) {
		if (other.tag == "Pedestrian" && ScenarioControl.Instance.NPC_police && ScenarioControl.Instance.scenarioIsRunning) {
            bool busted = ScenarioControl.Instance.bustingEnabled; //new Random().Next(100) > 0;
			if (ScenarioControl.Instance.urgent) {
				if (ScenarioControl.Instance.elapsedTime < ScenarioControl.UrgentTaskTime) {
					ScenarioControl.Instance.playerBusted = busted;
				}
			} else {
				if (ScenarioControl.Instance.elapsedTime < ScenarioControl.NormalTaskTime) {
					ScenarioControl.Instance.playerBusted = busted;
				}
			}
		} 
	}
}
