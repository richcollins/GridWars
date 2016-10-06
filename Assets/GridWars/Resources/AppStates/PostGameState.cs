﻿using UnityEngine;
using System.Collections;

public class PostGameState : NetworkDelegateState {
	public Player victoriousPlayer;

	bool requestedRematch;

	//AppState

	public override void EnterFrom(AppState state) {
		base.EnterFrom(state);

		network.networkDelegate = this;

		app.ResetMenu();
		menu.SetBackground(Color.black, 0);
		var title = "";
		if (battlefield.localPlayers.Count == 1) {
			if (victoriousPlayer.isLocal) {
				title = "Victory!";
			}
			else {
				title = "Defeat!";
			}
		}
		else {
			title = "Player " + victoriousPlayer.playerNumber + " is Victorious!";
		}

		menu.AddItem(UI.MenuItem(title, null, MenuItemType.ButtonTextOnly));
		if (battlefield.isInternetPVP) {
			menu.AddItem(UI.MenuItem("Request Rematch", RequestRematch));
		}
		menu.AddItem(UI.MenuItem("Leave Game", LeaveGame));

		menu.Show();
	}

	// Network

	public override void ZeusDisconnected() {
		base.ZeusDisconnected();

		ShowLostConnection();
	}

	public override void BoltShutdownCompleted() {
		base.BoltShutdownCompleted();

		network.networkDelegate = null;

		TransitionTo(new MainMenuState());
	}

	public override void Disconnected(BoltConnection connection) {
		base.Disconnected(connection);

		ShowLostConnection();
	}

	public override void ReceivedRematchRequest() {
		base.ReceivedRematchRequest();

		app.ResetMenu();
		menu.AddItem(UI.ActivityIndicator("Opponent Requests a Rematch"));
		menu.AddItem(UI.MenuItem("Accept", AcceptRematch));
		menu.AddItem(UI.MenuItem("Decline", LeaveGame));
		menu.Show();
	}

	public override void ReceivedAcceptRematch() {
		base.ReceivedAcceptRematch();

		if (BoltNetwork.isServer) {
			battlefield.SoftReset();
		}
		TransitionTo(new PlayingGameState());
	}

	// Menus

	void ShowLostConnection() {
		string prefix = "";
		if (requestedRematch) {
			prefix = "Opponent Declined. ";
		}

		app.ResetMenu();
		menu.AddItem(UI.ActivityIndicator(prefix + "Returning to Main Menu"));
		menu.Show();

		app.battlefield.HardReset();
		network.ShutdownBolt();
	}

	// LeaveGame

	void LeaveGame() {
		app.ResetMenu();
		menu.AddItem(UI.ActivityIndicator("RETURNING TO MAIN MENU"));
		menu.Show();

		if (BoltNetwork.isRunning) {
			app.battlefield.HardReset();
			network.ShutdownBolt();
		}
		else {
			BoltShutdownCompleted();
		}
	}

	//Rematch

	void RequestRematch() {
		RequestRematchEvent.Create(Bolt.GlobalTargets.Others, Bolt.ReliabilityModes.ReliableOrdered).Send();

		requestedRematch = true;

		app.ResetMenu();
		menu.AddItem(UI.ActivityIndicator("WAITING FOR RESPONSE"));
		menu.AddItem(UI.MenuItem("Cancel", LeaveGame));
		menu.Show();
	}

	void AcceptRematch() {
		AcceptRematchEvent.Create(Bolt.GlobalTargets.Others, Bolt.ReliabilityModes.ReliableOrdered).Send();

		if (BoltNetwork.isServer) {
			battlefield.SoftReset();
		}
		TransitionTo(new PlayingGameState());
	}
}