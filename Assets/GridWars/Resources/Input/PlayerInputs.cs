﻿using InControl;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInputs : PlayerActionSet {
	public PlayerAction releaseChopper;
	public PlayerAction releaseTanker;
	public PlayerAction releaseTank;
	public PlayerAction releaseMobileSam;
	public PlayerAction toggleMenu;
	public PlayerAction concede;
	public PlayerAction toggleHotkeys;
	//public PlayerAction previousCamera;
	public PlayerAction nextCamera;
	public PlayerAction firstPersonCamera;
	public PlayerAction upItem;
	public PlayerAction downItem;
	public PlayerAction leftItem;
	public PlayerAction rightItem;
	public PlayerAction selectItem;
	public PlayerAction goBack;

	/*
	InputDevice _device;
	public InputDevice device {
		get {
			return _device;
		}

		set {
			_device = value;
			foreach(var action in Actions) {
				action.Device = _device;
			}
		}
	}
	*/

	public PlayerInputs() {
		releaseChopper = CreatePlayerAction("Chopper");
		releaseTanker = CreatePlayerAction("Tanker");
		releaseTank = CreatePlayerAction("Tank");
		releaseMobileSam = CreatePlayerAction("MobileSAM");

		toggleMenu = CreatePlayerAction("Toggle Menu");
		concede = CreatePlayerAction("Concede");
		toggleHotkeys = CreatePlayerAction("Hotkeys");

		//previousCamera = CreatePlayerAction("Previous Camera");
		nextCamera = CreatePlayerAction("Next Camera");

		firstPersonCamera = CreatePlayerAction("Unit Camera");

		upItem = CreatePlayerAction("Up Item");
		downItem = CreatePlayerAction("Down Item");
		leftItem = CreatePlayerAction("Left Item");
		rightItem = CreatePlayerAction("Right Item");
		selectItem = CreatePlayerAction("Select Item");
		goBack = CreatePlayerAction("Go Back");
	}

	public void AddControllerBindings() {
		releaseChopper.AddDefaultBinding(InputControlType.Action4);
		releaseTanker.AddDefaultBinding(InputControlType.Action2);
		releaseTank.AddDefaultBinding(InputControlType.Action3);
		releaseMobileSam.AddDefaultBinding(InputControlType.Action1);

		toggleMenu.AddDefaultBinding(InputControlType.Command);

		concede.AddDefaultBinding(InputControlType.LeftTrigger);
		toggleHotkeys.AddDefaultBinding(InputControlType.RightTrigger);

		//previousCamera.AddDefaultBinding(InputControlType.DPadLeft);
		nextCamera.AddDefaultBinding(InputControlType.DPadRight);
		firstPersonCamera.AddDefaultBinding(InputControlType.DPadDown);

		//previousItem.AddDefaultBinding(InputControlType.LeftStickLeft);
		//previousItem.AddDefaultBinding(InputControlType.RightStickLeft);
		upItem.AddDefaultBinding(InputControlType.LeftStickUp);
		upItem.AddDefaultBinding(InputControlType.RightStickUp);
		upItem.AddDefaultBinding(InputControlType.DPadUp);

		//nextItem.AddDefaultBinding(InputControlType.LeftStickRight);
		//nextItem.AddDefaultBinding(InputControlType.RightStickRight);
		downItem.AddDefaultBinding(InputControlType.LeftStickDown);
		downItem.AddDefaultBinding(InputControlType.RightStickDown);
		downItem.AddDefaultBinding(InputControlType.DPadDown);

		leftItem.AddDefaultBinding(InputControlType.LeftStickLeft);
		leftItem.AddDefaultBinding(InputControlType.RightStickLeft);
		leftItem.AddDefaultBinding(InputControlType.DPadLeft);

		//nextItem.AddDefaultBinding(InputControlType.LeftStickRight);
		//nextItem.AddDefaultBinding(InputControlType.RightStickRight);
		rightItem.AddDefaultBinding(InputControlType.LeftStickRight);
		rightItem.AddDefaultBinding(InputControlType.RightStickRight);
		rightItem.AddDefaultBinding(InputControlType.DPadRight);

		selectItem.AddDefaultBinding(InputControlType.Action1);
		goBack.AddDefaultBinding(InputControlType.Action2);
	}

	public void AddLocalPlayer1KeyBindings() {
		toggleMenu.AddDefaultBinding(Key.Escape);

		upItem.AddDefaultBinding(Key.UpArrow);
		downItem.AddDefaultBinding(Key.DownArrow);

		leftItem.AddDefaultBinding(Key.LeftArrow);
		rightItem.AddDefaultBinding(Key.RightArrow);

		selectItem.AddDefaultBinding(Key.Return);
		selectItem.AddDefaultBinding(Key.PadEnter);

		goBack.AddDefaultBinding(Key.Escape);

		concede.AddDefaultBinding(Key.Q);
		toggleHotkeys.AddDefaultBinding(Key.H);

		//previousCamera.AddDefaultBinding(Key.LeftArrow);
		nextCamera.AddDefaultBinding(Key.C);
		firstPersonCamera.AddDefaultBinding(Key.DownArrow);

		releaseChopper.AddDefaultBinding(Key.D);
		releaseTanker.AddDefaultBinding(Key.F);
		releaseTank.AddDefaultBinding(Key.J);
		releaseMobileSam.AddDefaultBinding(Key.K);
	}

	public void AddLocalPlayer2KeyBindings() {
		releaseChopper.AddDefaultBinding(Key.E);
		releaseTanker.AddDefaultBinding(Key.R);
		releaseTank.AddDefaultBinding(Key.U);
		releaseMobileSam.AddDefaultBinding(Key.I);
	}

	public string ControlDescription(DeviceBindingSource control) {
		//device.OnDetached = 
		//Debug.Log(device.Controls[0].Handle);
		return null;
	}
}

public static class PlayerInputsExtensions {
	public static Dictionary<string, string> ControlHandleNormalizations = new Dictionary<string, string>() {
		{ "cross", "X" },
		{ "triangle", "△" },
		{ "square", "▢" },
		{ "circle", "◯" }
	};

	public static string HotkeyDescription(this DeviceBindingSource self) {
		var handle = self.BoundTo.Device.GetControl(self.Control).Handle;
		foreach (var pair in PlayerInputsExtensions.ControlHandleNormalizations) {
			if (handle.ToLower().StartsWith(pair.Key)) {
				return pair.Value;
			}
		}
		return handle.Substring(0, 1);
	}

	public static string HotkeyDescription(this KeyBindingSource self) {
		return self.Control.ToString();
	}

	public static string HotkeyDescription(this BindingSource self) {
		if (self.inheritsFrom(typeof(KeyBindingSource))) {
			return (self as KeyBindingSource).HotkeyDescription();
		}
		else if (self.inheritsFrom(typeof(DeviceBindingSource))) {
			return (self as DeviceBindingSource).HotkeyDescription();
		}
		else {
			return "";
		}
	}
}