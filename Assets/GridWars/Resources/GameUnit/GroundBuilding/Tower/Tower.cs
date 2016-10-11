﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Tower : GroundBuilding {

	public GameObject iconPlacement;

	//public Mesh theMesh;
	[HideInInspector]
	public KeyCode attemptQueueUnitKeyCode = KeyCode.None;
    public string unitKeyMap = "None";
	public bool npcModeOn {
		get {
			return player.npcModeOn;
		}
	}

	//public GameObject topComponent;
	//public GameObject baseComponent;
	private GameUnit iconUnit;

	public static Vector3 size {
		get {
			/*
			Mesh mesh = GameUnit.Load<Tower>().gameObject.GetComponent<Mesh>();
			return mesh.bounds.size;
			*/

			//float y = GameUnit.Load<Tower>().GetComponent<Collider>().bounds.size.y;

			return new Vector3(5f, 1.5f, 5f);
		}
	}
		
	public string unitPrefabPath {
		get {
			return entity.GetState<ITowerState>().unitPrefabPath;
		}

		set {
			entity.GetState<ITowerState>().unitPrefabPath = value;
		}
	}

	GameObject _unitPrefab;
	public GameObject unitPrefab {
		get {
			if (_unitPrefab == null) {
				_unitPrefab = Resources.Load<GameObject>(unitPrefabPath);
				_unitPrefab.GetComponent<GameUnit>().Awake();
			}
			return _unitPrefab;
		}
	}

	// NetworkedGameUnit

	public override void ClientInit() {
		shouldDestroyColliderOnClient = false;
		base.ClientInit();
	}

	public override void ServerAndClientInit() {
		base.ServerAndClientInit();

		prefs = App.shared.prefs; //perf opt

		if (App.shared.testEndOfGameMode) {
			hitPoints = 1f;
		}

		keyIcon.SetActive(false);
	}

	public override void ServerJoinedGame() {
		base.ServerJoinedGame();

		isStaticUnit = true;

		releaseZones = new List<ReleaseZone>();
		var concurrency = Mathf.Ceil(player.powerSource.maxPower / unitPrefab.GameUnit().powerCost);
		var unitSize = unitPrefab.GetComponent<BoxCollider>().size;
		var unitWidth = unitSize.x;
		var unitLength = unitSize.z;
		var unitSpacing = unitWidth/4;
		var launchZoneWidth = concurrency*(unitWidth + unitSpacing) - unitSpacing;

		for (var i = 0; i < concurrency; i ++) {
			var releaseZone = this.CreateChild<ReleaseZone>();
			releaseZone.size = unitSize;
			releaseZone.transform.localPosition = new Vector3(-launchZoneWidth/2 + unitWidth/2 + i*(unitWidth+unitSpacing), 0.0f, 0f);

			releaseZone.transform.Translate(Vector3.Scale(
				new Vector3(0f, size.y/2 + unitSize.y/2 + unitSpacing, size.z/2 + unitLength/2 + unitSpacing),
				unitPrefab.GetComponent<GameUnit>().launchDirection
			));

			releaseZones.Add(releaseZone);
		}

		tag = "Player" + player.playerNumber;

		entity.AddEventCallback<AttemptQueueUnitEvent>(AttemptQueueUnit);
	}

	public override void ServerAndClientJoinedGame() {
		base.ServerAndClientJoinedGame();

		iconUnit = Instantiate(unitPrefab).GetComponent<GameUnit>();
		iconUnit.BecomeIcon();

		iconObject = iconUnit.gameObject;
		iconObject.transform.SetParent(transform);
		iconObject.transform.localPosition = new Vector3(0f, iconPlacement.transform.position.y, 0f);
		iconObject.transform.localRotation = Quaternion.identity;
		player.Paint(iconObject);

        unitKeyMap = iconUnit.GetComponent<GameUnit>().GetType().ToString() + player.localNumber;
        attemptQueueUnitKeyCode = unitKeyMap.GetKey();
		//Keys.data.TryGetValue(iconUnit.GetComponent<GameUnit>().GetType().ToString() + player.localNumber, out attemptQueueUnitKeyCode); //assigns KeyCode from string - dictionary is editable for remapping keys

        keyIcon.GetComponentInChildren<TextMesh>().text = attemptQueueUnitKeyCode.ToString().FormatForKeyboard();

		if (playerNumber == 2) {
			keyIcon.transform.Rotate(new Vector3(0, 0, 180));
		}

		//Debug.Log(player.playerNumber + ": " + gameUnit.GetType() + ": " + attemptQueueUnitKeyCode.ToString());
	}

	public override void ServerFixedUpdate () {
		//base.ServerFixedUpdate(); TODO: extract another class from GameUnit so we don't have to perform this perf opt.

		//NpcStep();

		if (queueSize > 0) {
			ReleaseUnits();
		}

	}

	public float EffectivenessEstimate() {
		float a = CountOfEnemyUnitsWeCanCounter();
		float b = CountOfEnemyUnitsThatCounterUs();

		//float desireToRelease = a * a / (1 + b);
		float cost = gameUnit.powerCost / player.powerSource.maxPower;
		float desireToRelease = 2f * (1.5f * a - b) / cost;
		return desireToRelease;
	}

	public void NpcStep () {
		if (npcModeOn) {
			if (player.powerSource.PowerRatio() > 0.3f) {
				if (Random.value < 0.001f * EffectivenessEstimate()) {
					SendAttemptQueueUnit();
				} else if (player.powerSource.IsAtMax()) {
					LaunchWithChance(0.002f);
				}
			} 
		}
	}

	public void LaunchWithChance(float chance) { // chance out of 1
		if (Random.value < chance) {
			SendAttemptQueueUnit();
		}
	}

	public override void ServerAndClientUpdate() {
		base.ServerAndClientUpdate();

		if (canQueueUnit) {
			//Paint();
			ShowHud(true);
			//keyIcon.SetActive((attemptQueueUnitKeyCode != KeyCode.None) && App.shared.prefs.keyIconsVisible);

		}
		else {
			//PaintAsDisabled();
            ShowHud(false);
		}

		keyIcon.SetActive(attemptQueueUnitKeyCode != KeyCode.None && player.isLocal && prefs.keyIconsVisible);
	}


	public override void ServerLeftGame() {
		base.ServerLeftGame();

		if (player != null) {
			player.fortress.TowerDied(this);
		}
	}

	// HUD

	GameObject iconObject;
	public GameObject keyIcon;
	Prefs prefs;

    public void ShowHud(bool b = true) {
        foreach (Renderer renderer in iconObject.GetComponentsInChildren<Renderer>()) {
            renderer.enabled = b;
        }
	}

	/*public void HideHud() {
		if (!hudIsHidden) {
			keyIcon.SetActive(false);
			foreach (Renderer renderer in iconObject.GetComponentsInChildren<Renderer>()) {
				renderer.enabled = false;
			}
			hudIsHidden = true;
		}
	}*/


	public override void Think() {
		// doesn't need to pick targets
	}

	public void AttemptQueueUnit(AttemptQueueUnitEvent e) {
		AttemptQueueUnit();
	}

	public void AttemptQueueUnit() {
		if (canQueueUnit) {
			QueueUnit();
		}
	}

	float lastProductionTime = 0f;
	//int releaseLocationIndex = 0;
	int queueSize = 0;
	List<ReleaseZone> releaseZones;

	bool canQueueUnit {
		get {
			return hasCooledDown && hasEnoughPower;
		}
	}

	bool hasCooledDown {
		get {
			return Time.time >= lastProductionTime + gameUnit.cooldownSeconds;
		}
	}

	bool hasEnoughPower {
		get {
			return player.powerSource.power >= gameUnit.powerCost;
		}
	}

	GameUnit gameUnit {
		get {
			return unitPrefab.GetComponent<GameUnit>();
		}
	}

	public void OnMouseDown() {
		if (!npcModeOn) {
			SendAttemptQueueUnit();
		}
	}

	public int CountOfEnemyUnitsWeCanCounter() {
		int total = 0;

		foreach(var counterType in iconUnit.CountersTypes()) {
			int count = player.EnemyObjectsOfType(counterType).Count;
			total += count;
		}

		return total;
	}

	public int CountOfEnemyUnitsThatCounterUs() {
		int total = 0;

		foreach(GameUnit unit in EnemyUnits()) {
			if (unit != null && unit.CountersTypes().Contains(iconUnit.GetType())) {
				total += 1;
			}
		}

		return total;
	}

	public override void QueuePlayerCommands() {
		base.QueuePlayerCommands();


        if (unitKeyMap == "None") {
            return;
        }
        if (!npcModeOn && unitKeyMap.Pressed()) {
			SendAttemptQueueUnit();
		}
	}

	public void SendAttemptQueueUnit() {
		if (entity.hasControl) {
			AttemptQueueUnitEvent.Create(entity).Send();
		}
	}

	void QueueUnit() {
		queueSize ++;
		player.powerSource.power -= gameUnit.powerCost;
		lastProductionTime = Time.time;
	}

	void ReleaseUnits() {
		while (queueSize > 0 && unobstructedReleaseZone != null) {
			var releaseZone = unobstructedReleaseZone;

			var unit = unitPrefab.GameUnit().Instantiate();
			unit.player = player;

			unit.transform.position = releaseZone.transform.position;
				
			unit.transform.rotation = transform.rotation;

			queueSize --;
		}
	}

	ReleaseZone unobstructedReleaseZone {
		get {
			var shuffledZones = releaseZones.OrderBy(a => UnityEngine.Random.value);

			foreach (var rz in shuffledZones) {
				if (!rz.isObstructed) {
					return rz;
				}
			}
			return null;
		}
	}
}