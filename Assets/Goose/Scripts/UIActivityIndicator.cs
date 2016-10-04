﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class UIActivityIndicator : UIMenuItem {
	string prefix;
	int maxDots = 10;
	int dotCount = 0;
	float showTime = 0f;

	public static UIActivityIndicator Instantiate() {
		GameObject go = MonoBehaviour.Instantiate(Resources.Load<GameObject>(UI.BUTTONPREFAB));
		UI.AssignToCanvas(go);
		Destroy(go.GetComponent<UIButton>());
		UIActivityIndicator indicator = go.AddComponent<UIActivityIndicator>();
		return indicator;
	}

	public Text SetText(string text) {
		prefix = text + "\n";
		return base.SetText(prefix);
	}

	void Awake() {
		matchesNeighborSize = false;
		isOutlined = false;
	}

	public override void Show() {
		base.Show();

		showTime = Time.time;
	}

	void Update () {
		var newDotCount = Mathf.FloorToInt(Time.time - showTime) % (maxDots + 1);

		if (dotCount != newDotCount) {
			dotCount = newDotCount;
			UpdateText();
		}

		//transform.GetComponentInChildren<Text> ().rectTransform.rotation = Quaternion.Euler (Vector3.zero);
		//transform.Rotate (transform.forward, Time.deltaTime * rotateSpeed);
	}

	void UpdateText() {
		var suffix = "";

		for (var i = 0; i < dotCount; i ++) {
			suffix += ".";
		}

		transform.GetComponentInChildren<Text>().text = prefix + suffix;
	}

}
