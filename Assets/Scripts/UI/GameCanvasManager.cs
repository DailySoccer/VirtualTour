using UnityEngine;
using System.Collections;
//using UnityEngine.UI;

public class GameCanvasManager : CanvasManager 
{
	private MainManager _mainManagerInstance;

	private UnityEngine.UI.Toggle _soundsToggle;
	private UnityEngine.UI.Toggle _musicToggle;

	void Awake() {
		_mainManagerInstance = MainManager.Instance;
		
		// Get Language Settings
		switch (_mainManagerInstance.CurrentLanguage) {
		case "es":
			GameObject.Find("Toggle Spanish").GetComponent<UnityEngine.UI.Toggle>().isOn = true;
			break;
		case "en":
			GameObject.Find("Toggle English").GetComponent<UnityEngine.UI.Toggle>().isOn = true;
			break;
		}
		// Get Sound Settings
		_soundsToggle = GameObject.Find("Sounds").GetComponent<UnityEngine.UI.Toggle>();
		_soundsToggle.isOn = _mainManagerInstance.SoundEnabled;
		
		// Get Music Settings
		_musicToggle = GameObject.Find("Music").GetComponent<UnityEngine.UI.Toggle>();
		_musicToggle.isOn = _mainManagerInstance.MusicEnabled;

		_mainManagerInstance.OnMessagesUnreadedEvent += OnUnreadedMessages;
	}

	void OnEnable() {
		ShowScreen(currentGUIScreen);
	}

	public void SetLanguage(string lang) {
		_mainManagerInstance.ChangeLanguage(lang);
	}

	public void SetSoundsEnable() {
		_mainManagerInstance.SoundEnabled = _soundsToggle.isOn;
	}

	public void SetMusicEnable() {
		_mainManagerInstance.MusicEnabled = _musicToggle.isOn;
	}

	private void OnUnreadedMessages(int counter) {
		GameObject.FindGameObjectWithTag("ChatUIButton").GetComponent<BadgeAlert>().Count = counter;
	}
}
