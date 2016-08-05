﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightTank : GameUnit {
	public override void Start () {
		base.Start();
		thrust = 200;
		rotationThrust = 5;
	}
		

	public override void FixedUpdate () {
		base.FixedUpdate();
		aimTowardsNearestEnemy ();
	}

}