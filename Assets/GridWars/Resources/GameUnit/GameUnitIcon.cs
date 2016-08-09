﻿using UnityEngine;
using System.Collections;

using UnityEditor;

public class GameUnitIcon : MonoBehaviour {

	// Use this for initialization
	void Start () {
		/*
		var player = new Player();
		player.playerNumber = 1;
		GetComponent<GameUnit>().player = player;
		*/

		GetComponent<Collider>().enabled = false;
		GetComponent<Rigidbody>().useGravity = false;
		this.EachMaterial(m => {
			m.shader = Shader.Find("Hidden/VacuumShaders/The Amazing Wireframe/Physically Based/Transparent/Simple/Diffuse");

			m.SetColor("_V_WIRE_Color", Color.white);
			m.SetColor("_Color", new Color(0, 0, 0, 0));
		});
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.S)) {
			this.enabled = false;
			GetComponent<GameUnit>().enabled = true;
		}
	}
}
