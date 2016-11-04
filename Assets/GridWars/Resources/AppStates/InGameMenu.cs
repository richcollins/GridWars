﻿using UnityEngine;
using System.Collections.Generic;

public class InGameMenu {
	public PlayingGameState playingGameState;
	public Player player;
	public MenuAnchor menuPlacement;

	public bool hasFocus {
		get {
			return menu.hasFocus;
		}
	}

	public void Show() {
		App.shared.ResetMenu();

		Reset();
	}

	public void Hide() {
		if (menu != null) {
			menu.Destroy();
			menu = null;
			player.inGameMenu = null;
			player = null;
		}
	}

	public void ReadInput () {
		concedeItem.ReadInput();

		if (hotkeysItem != null) {
			hotkeysItem.ReadInput();
		}

		if (previousCameraItem != null) {
			previousCameraItem.ReadInput();
		}

		if (nextCameraItem != null) {
			nextCameraItem.ReadInput();
		}

		if (firstPersonCameraItem != null) {
			firstPersonCameraItem.ReadInput();
		}

		if (inputs.toggleMenu.WasPressed && !App.shared.cameraController.isInFirstPersonMode) {
			if (menu.hasFocus) {
				menu.LoseFocus();
			}
			else {
                MonoBehaviour.FindObjectOfType<CameraController>().menuHasFocus = true;
				menu.SelectNextItem();
			}
		}
	}

	UIMenu menu;
	InGameMenuItem concedeItem;
	InGameMenuItem hotkeysItem;
	InGameMenuItem nextCameraItem;
	InGameMenuItem previousCameraItem;
	InGameMenuItem firstPersonCameraItem;

	PlayerInputs inputs {
		get {
			return player.inputs;
		}
	}

	void Reset() {
		if (menu != null) {
			menu.Destroy();
		}

		menu = UI.Menu();

		string title;

		//TODO: detect which player conceded in shared screen pvp
		if (App.shared.battlefield.isAiVsAi) { //AIvAI
			title = "Quit";
		}
		else {
			title = "Concede";
		}

		concedeItem = new InGameMenuItem();
		concedeItem.title = title;
		concedeItem.inGameMenu = this;
		concedeItem.playerAction = inputs.concede;
		concedeItem.menuAction = HandleConcede;
		menu.AddItem(concedeItem.menuItem);

		if (!App.shared.battlefield.isAiVsAi) {
			hotkeysItem = new InGameMenuItem();
			hotkeysItem.title = "Hotkeys";
			hotkeysItem.inGameMenu = this;
			hotkeysItem.playerAction = inputs.toggleHotkeys;
			hotkeysItem.menuAction = HandleHotkeys;
			menu.AddItem(hotkeysItem.menuItem);
		}

		if (inputs.nextCamera.Bindings.Count > 0) {
			/*
			previousCameraItem = new InGameMenuItem();
			previousCameraItem.inGameMenu = this;
			previousCameraItem.title = "< Camera";
			previousCameraItem.menuAction = HandlePreviousCamera;
			menu.AddItem(previousCameraItem.menuItem);
			*/

			nextCameraItem = new InGameMenuItem();
			nextCameraItem.inGameMenu = this;
			nextCameraItem.playerAction = inputs.nextCamera;
			nextCameraItem.title = "Camera";
			nextCameraItem.menuAction = HandleCamera;
			menu.AddItem(nextCameraItem.menuItem);
		}

		menu.SetOrientation(MenuOrientation.Horizontal);
		menu.SetAnchor(menuPlacement);
		menu.SetBackground(Color.black, 0);
		menu.selectsOnShow = false;
		menu.inputs = inputs;
		menu.Show();
	}


	void HandleConcede() {
		menu.Destroy();

		menu = UI.Menu();
		menu.AddItem(UI.MenuItem("Confirm", ReallyConcede));
		menu.AddItem(UI.MenuItem("Cancel", Reset), true);
		menu.SetOrientation(MenuOrientation.Horizontal);
		menu.SetAnchor(menuPlacement);
		menu.SetBackground(Color.black, 0);
		menu.selectsOnShow = true;
		menu.inputs = inputs;
		menu.Show();
	}

	void ReallyConcede() {
		var state = new PostGameState();

		if (App.shared.battlefield.isAiVsAi) {
			state.victoriousPlayer = null;
		}
		else {
			state.victoriousPlayer = player.opponent;
		}
			
		if (App.shared.battlefield.isInternetPVP) {
			App.shared.Log("ConcedeEvent.Send", this);
			ConcedeEvent.Create(Bolt.GlobalTargets.Others, Bolt.ReliabilityModes.ReliableOrdered).Send();
		}

		playingGameState.TransitionTo(state);
	}

	//TODO: different for each player?
	void HandleHotkeys() {
		App.shared.prefs.keyIconsVisible = !App.shared.prefs.keyIconsVisible;
        foreach (Tower _tower in GameObject.FindObjectsOfType<Tower>()) {
            _tower.UpdateHotKeys();
        }
	}

	void HandleCamera() {
		App.shared.cameraController.NextPosition();
	}
}

public class InGameMenuItem {
	public InGameMenu inGameMenu;
	public string title;
	public InControl.PlayerAction playerAction;
	public System.Action menuAction;

	public UIButton menuItem {
		get {
			if (_menuItem == null) {
				_menuItem = UI.MenuItem(title, menuAction);
			}

			return _menuItem;
		}
	}

	public void ReadInput() {
		if (playerAction.WasPressed) {
			menuAction();
		}
	}

	UIButton _menuItem;
}