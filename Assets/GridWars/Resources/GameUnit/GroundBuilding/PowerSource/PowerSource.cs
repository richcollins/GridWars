﻿using UnityEngine;
using System.Collections.Generic;

public class PowerSource : GroundBuilding {
	public Vector3 bounds;

	public float power {
		get {
			return powerSourceState.power;
		}

		set {
			powerSourceState.power = value;
		}
	}

	public float maxPower = 20f;
	public float generationRate = 2f;

	public float trackSpacing = 1.0f;

	public GameObject segmentPrefab;
	public float baseSegmentWidth = 10f;
	public float baseSegmentLength = 10f;
	public int segmentCount = 20;

	public GameObject prefab;

	IPowerSourceState powerSourceState {
		get {
			return entity.GetState<IPowerSourceState>();
		}
	}

	float trackLength {
		get {
			return bounds.x;
		}
	}

	float trackWidth {
		get {
			return bounds.z;
		}
	}

	List<GameObject>segments;

	protected override void Awake () {
		base.Awake();
		bounds = new Vector3(0f, 1.0f, 2.5f);
	}

	public override void Attached() {
		base.Attached();
		if (BoltNetwork.isClient) {
			Setup();
		}
	}

	public void Setup() {
		bounds = new Vector3(player.fortress.bounds.x, bounds.y, bounds.z);

		var segmentLength = (trackLength - trackSpacing*(segmentCount - 1))/segmentCount;

		segments = new List<GameObject>();
		for (var i = 0; i < segmentCount; i ++) {
			var segment = Instantiate<GameObject>(segmentPrefab);
			segment.transform.parent = transform;
			segment.transform.localRotation = Quaternion.identity;

			var offset = -trackLength/2 + segmentLength/2 + i*(segmentLength + trackSpacing);
			var segmentWidthScale = trackWidth/baseSegmentWidth;
			var segmentLengthScale = segmentLength/baseSegmentLength;

			segment.transform.localPosition = new Vector3(offset, 0.1f, 0);
			segment.transform.localScale = new Vector3(segmentLengthScale, segment.transform.localScale.y, segmentWidthScale);

			player.Paint(segment.gameObject);

			segment.SetActive(false);
			segments.Add(segment);
		}

		player.fortress.powerSource = this;
	}

	public override void SimulateOwner() {
		base.SimulateOwner();
		power = Mathf.Min(power + Time.fixedDeltaTime*generationRate, maxPower);
	}

	void Update() {
		var activeSegmentCount = segmentCount*power/maxPower;

		for (var i = 0; i < segmentCount; i ++) {
			segments[i].SetActive((i + 1) <= activeSegmentCount);
		}
	}

	void OnDrawGizmos() {
		/*
		if (player != null) {
			Gizmos.color = player.color;
		}
		Gizmos.DrawCube(transform.position, new Vector3(trackWidth, 1, trackLength));
		*/
	}
}