﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GuiMapScreen : GUIPopUpScreen {
	public Button MapRoomIcon;

	public override void Awake () {
		base.Awake ();
	}

	public override void Start() {
		/*
		if (_map == null) {
			_map = new List<Button>();

			int X = 100;
			int Y = Screen.height - 200;

			foreach(string roomDefinitionKey in RoomManager.Instance.RoomDefinitions.Keys) {
				RoomDefinition roomDefinition = RoomManager.Instance.RoomDefinitions[roomDefinitionKey] as RoomDefinition;

				Button roomBtn = GameObject.Instantiate(MapRoomIcon);
				roomBtn.GetComponentInChildren<Text>().text = roomDefinition.Name;

				string roomKey = roomDefinitionKey;
				roomBtn.onClick.AddListener(() => HandleRoom(roomKey));
				
				roomBtn.transform.position = new Vector3(X, Y, 0);
				roomBtn.transform.SetParent(transform);
				
				_map.Add(roomBtn);

				X += 150;
				if (X + (MapRoomIcon.GetComponent<RectTransform>().rect.xMax / 2) > Screen.width) {
					Y -= 150;
					X = 100;
				}
			}
		}
		*/
	}

	public void HandleRoom(string roomKey) {
		Debug.LogWarning("HandleRoom: " + roomKey);
		RoomManager.Instance.GotoRoom(roomKey);
	}

	public override void Update () {
		base.Update ();
	}

	List<Button> _map;
}
