﻿using UnityEngine;
using System.Collections;

public interface NetworkObjectDelegate {
	void MasterStart();
	void SlaveStart();
	void MasterFixedUpdate();
	void SlaveFixedUpdate();
}

public class NetworkObject : Bolt.EntityBehaviour {
	//public interface

	public NetworkObjectDelegate networkObjectDelegate {
		get {
			return GetComponent<NetworkObjectDelegate>();
		}
	}

	public virtual void MasterStart() {
		networkObjectDelegate.MasterStart();
	}

	public virtual void SlaveStart() {
		networkObjectDelegate.SlaveStart();
	}

	public virtual void MasterFixedUpdate() {
		networkObjectDelegate.MasterFixedUpdate();
	}

	public virtual void SlaveFixedUpdate() {
		networkObjectDelegate.SlaveFixedUpdate();
	}

	//internal interface

	public override void Attached() {
		base.Attached();

		if (BoltNetwork.isServer) {
			MasterStart();
		}
		SlaveStart();
	}

	public override void SimulateOwner() {
		base.SimulateOwner();

		MasterFixedUpdate();
	}

	void FixedUpdate() {
		SlaveFixedUpdate();
	}
}