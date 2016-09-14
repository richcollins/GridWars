﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

/*
 *  This is a singleton. Access like this:
 * 
 *  App.shared.ResourcePathForUnitType(typeof(Tank))
 * 
 */

public class App : MonoBehaviour {
    [Range(.01f, 5)]
    public float gameSpeed = 1;
	private static App _shared;
	public AssemblyCSharp.TimerCenter timerCenter;
	public int timeCounter = 0;
	public AssemblyCSharp.StepCache stepCache;
	private List <GameObject> _destroyQueue;
	public bool debug = false;
	public List <Soundtrack> soundtracks;
	public Prefs prefs;

	private bool _isProcessingDestroyQueue = false;

	public static App shared {
		get {
			if (_shared == null) {
				GameObject go = new GameObject();
				go.name = "App";
				_shared = go.AddComponent<App>();
			}
			return _shared;
		}
	}

	// --- Game Loop -------------------

	public void Start() {
		prefs = new Prefs();

		timerCenter = new AssemblyCSharp.TimerCenter();
		stepCache = new AssemblyCSharp.StepCache();
		_destroyQueue = new List<GameObject>();
		soundtracks = new List<Soundtrack>();

		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 0;

		Wreckage.SetupLayerCollisions();

		SoundtrackNamed("Wagner_Ride_of_the_Valkyries"); // preload

	}

	public void FixedUpdate() {
		timerCenter.Step();
		stepCache.Step();
		timeCounter++;
	}

	// --- Finding Paths --------------------
	// should really have the Types themselve know how to find themselves but
	// class methods don't have access to the type on which they were called -
	// they only know the type in which they are declared

	public string ResourcePathForUnitType(System.Type type) {
		List <string> pathComponents = new List<string>();

		while (type != typeof(GameUnit)) {
			pathComponents.Add(type.Name);
			type = type.BaseType;
		}

		pathComponents.Add("GameUnit"); // add GameUnit
		pathComponents.Reverse();
		return string.Join("/", pathComponents.ToArray());
	}

	public string PrefabPathForUnitType(System.Type type) {
		string path = ResourcePathForUnitType(type);
		return path + "/Prefabs/" + type.Name;
	}

	public string SoundPathForUnitType(System.Type type) {
		string path = ResourcePathForUnitType(type);
		string soundPath = path + "/Sounds/";
		return soundPath;
	}

	public AudioClip SoundNamedForUnitType(string name, System.Type type) {
			string path = ResourcePathForUnitType(type);
			string soundPath = path + "/Sounds/" + name;
			return Resources.Load<AudioClip>(soundPath);
	}


	// --- Destroying Objects -----------

	void LateUpdate() {
        #if UNITY_EDITOR
        Time.timeScale = Mathf.Clamp(gameSpeed, 0.01f, 5);
        #endif
		ProcessDestroyQueue();
	}


	public void AddToDestroyQueue(GameObject obj) {
		//ThrowIfQueuedToDestroyOrAlreadyDestroyed(obj);

		if(!_destroyQueue.Contains(obj)) {
			_destroyQueue.Add(obj);
		}
	}


	public void ProcessDestroyQueue() {
		if (_isProcessingDestroyQueue) {
			throw new System.InvalidOperationException("circular call of ProcessDestroyQueue");
		}

		_isProcessingDestroyQueue = true;

		foreach(GameObject obj in _destroyQueue) {
			if (obj == null) { 
				//throw new System.InvalidOperationException("found already destroyed gameobject in destroy queue");
				print("WARNING: found already destroyed gameobject in destroy queue");
			} else {
				Destroy(obj);
			}
		}

		_destroyQueue.Clear();
		_isProcessingDestroyQueue = false;
	}

	public void ThrowIfQueuedToDestroyOrAlreadyDestroyed(GameObject obj) {
		if (App.shared.HasQueuedToDestroy(obj)) {
			throw new System.InvalidOperationException("has queued to destroy");
		}

		if (obj == null) { // && !ReferenceEquals(obj, null)) {
			throw new System.InvalidOperationException("null game object");
		}
	}

	public bool HasQueuedToDestroy(GameObject obj) {
		return _destroyQueue.Contains(obj);
	}

	public void ImmediateDestory(GameObject obj) {
		Destroy(obj);
	}

	public void Log(object message, UnityEngine.Object context = null) {
		if (debug) {
			Debug.Log(message, context);
		}
	}

	public Soundtrack SoundtrackNamed(string trackName) {
		var tracks = soundtracks.Where(t => t.trackName == trackName).ToList(); 
		if (tracks.Count() > 0) {
			return tracks[0];
		}
		var track = gameObject.AddComponent<Soundtrack>();
		track.SetTrackName(trackName);
		soundtracks.Add(track);
		return track;
	}
}


/*
public class ATest : MonoBehaviour {
	public static void ClassTest() {
		print("type = " + MethodBase.GetCurrentMethod().ReflectedType.GetType().Name);
	}
}

public class BTest : ATest {
}
*/