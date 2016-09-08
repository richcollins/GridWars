﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chopper : AirVehicle {
	public float cruiseHeight = 12f;
	public float thrustHeight = 2f;

	public GameObject mainRotor;
	public GameObject tailRotor;

	Transform mainRotorTransform; // set in start

	[HideInInspector]
	public float damageRotation;

	public override void ServerAndClientJoinedGame() {
		base.ServerAndClientJoinedGame();
		mainRotor = _t.FindDeepChild("mainRotor").gameObject;
		tailRotor = _t.FindDeepChild("tailRotor").gameObject;
	}

	public override void ServerJoinedGame () {
		base.ServerJoinedGame();
		isRunning = true;

		cruiseHeight = 10f + Random.Range(-1.0f, 1.0f);

		mainRotorTransform = _t.FindDeepChild("mainRotorCenter");

		damageRotation = (Random.value - 0.5f) * 10f;
	}

	public float UpDesire() { // 0.0 to 1.0
		float diff = cruiseHeight - y ();
		return Mathf.Clamp(SmoothValue(diff)/2, 0f, 1f);
	}

	public float ForwardDesire() { // 0.0 to 1.0 
		if (!target) {
			return 0f;
		}
			
		if (!IsInStandoffRange()) {
			// don't tilt forward until we're facing the target
			float angleDiff = Mathf.Abs(YAngleToTarget());
			if (angleDiff < 30) {
				float diff = targetDistance() - standOffDistance;
				return Mathf.Clamp(diff, 0f, 1f);
			}
		}

		return 0f;
	}

	public float TiltRightDesire() { // -1.0 to 1.0
		Vector3 worldUp = new Vector3(0, 1, 0);
		float a = AngleBetweenOnAxis(_t.up, worldUp, _t.forward); // left is positive angle
		return Mathf.Clamp(a/10.0f, -1.0f, 1.0f)/5f;
	}

	public void  ApplyRotorLRThrust() { // z tilt control ------------------------------------------------------
		float upThrust = TotalUpThrust()/2f;

		float offset = 1f;
		Vector3 thrustPointLeft  = mainRotorTransform.position - mainRotorTransform.right * offset;
		Vector3 thrustPointRight = mainRotorTransform.position + mainRotorTransform.right * offset;

		float f = TiltRightDesire();

		Vector3 rotorUp = mainRotorTransform.up;
		Vector3 leftForce  = rotorUp * ((upThrust - f) / 2);
		Vector3 rightForce = rotorUp * ((upThrust + f) / 2);

		rigidBody().AddForceAtPosition(leftForce,  thrustPointLeft);
		rigidBody().AddForceAtPosition(rightForce, thrustPointRight);

		mainRotorTransform = _t.FindDeepChild("mainRotorCenter");


		//Debug.DrawLine(mainRotorTransform.position, mainRotorTransform.position + (mainRotorTransform.up * transform.rotation.eulerAngles.z / 10f), Color.blue); 
		//Debug.DrawLine(thrustPointLeft, thrustPointLeft + leftForce , Color.black); 
		//Debug.DrawLine(thrustPointRight, thrustPointRight + rightForce , Color.black); 
	}
				
	public float TotalUpThrust() {
		float upThrust = thrust * UpDesire(); // * 1.5f;

		if (IsHeavilyDamaged()) {
			upThrust *= Random.value;
		}

		return upThrust;
	}

	public void  ApplyRotorThrust() {
		// points around top rotor to apply force
		// a difference between the force applied to these 
		// causes chopper to tilt and then move forward or back

		float upThrust = TotalUpThrust();

		ApplyRotorLRThrust();

		// forward/backward control ---------------------------------------------------

		float offset = 1f;
		Vector3 mainRotorThrustPointBack  = mainRotorTransform.position + mainRotorTransform.forward * offset;
		Vector3 mainRotorThrustPointFront = mainRotorTransform.position - mainRotorTransform.forward * offset;

		Vector3 rotorUp = mainRotorTransform.up;
		float speed = ForwardSpeed();
		float desiredSpeed = ForwardDesire() * 4;
		float speedDiff = desiredSpeed - speed;
		float f = Mathf.Clamp(speedDiff, -upThrust, upThrust);

		Vector3 frontForce = rotorUp * ((upThrust + f) / 2);
		Vector3 backForce  = rotorUp * ((upThrust - f) / 2);

		rigidBody().AddForceAtPosition(frontForce, mainRotorThrustPointFront);
		rigidBody().AddForceAtPosition(backForce,  mainRotorThrustPointBack);

	
		//Debug.DrawLine(mainRotorThrustPointFront, mainRotorThrustPointFront + frontForce * 2.0f, Color.yellow); 
		//Debug.DrawLine(mainRotorThrustPointBack,  mainRotorThrustPointBack  + backForce  * 2.0f, Color.blue); 
	}

	public void SpinRotors() {
		// rotors don't look right except at certain speeds, so hard wire this
		float r = Random.value;
		float t = TotalUpThrust();
		Object_rotDY(mainRotor, 20f + t*r); //Mathf.Abs(upThrust*5.0f) + 20f);
		Object_rotDY (tailRotor, 20f+ 20f*r);
	}

	public override void ServerFixedUpdate () {
		base.ServerFixedUpdate();
		if (isRunning) {
			PickTarget();
			SteerTowardsTarget();
			ApplyRotorThrust();
		}
		RemoveIfOutOfBounds();
	}

	public override void ServerAndClientFixedUpdate () {
		base.ServerAndClientFixedUpdate();
		SpinRotors();
	}

	public override void OnCollisionEnter(Collision collision) {
		base.OnCollisionEnter(collision);

		// destroy on ground collision
		if (collision.collider.name == "BattlefieldPlane") {
			if (collision.relativeVelocity.magnitude > 2) {
				Die();
			}
		}
	}
}
