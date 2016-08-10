﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tank : GroundVehicle {

	//public GameObject turret;
	public Weapon turretWeapon {
		get {
			return _t.FindDeepChild("turret").GetComponent<Weapon>();
		}
	}

	public override void Start () {
		base.Start();
		thrust = 850;
		rotationThrust = 60;
		turretWeapon.enabled = true;
		turretWeapon.owner = gameObject;
		turretWeapon.isFixed = false;
	}


	public override void pickTarget () {
		base.pickTarget();
		turretWeapon.target = target;
	}


	public override void FixedUpdate () {
		base.FixedUpdate();
		pickTarget ();
		steerTowardsNearestEnemy ();
	}

	void OnDisable() {
		turretWeapon.enabled = false;
	}
}