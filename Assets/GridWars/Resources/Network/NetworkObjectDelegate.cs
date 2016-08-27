﻿using UnityEngine;
using System.Collections;

public interface NetworkObjectDelegate {
	void MasterSlaveStart();
	void MasterStart();
	void SlaveStart();
	void MasterFixedUpdate();
	void SlaveFixedUpdate();
}