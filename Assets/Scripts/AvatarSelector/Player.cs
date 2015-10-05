using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public const string TAG_UMA_AVATAR = "UMAAvatar";
	
	public bool avatarVisible = true;
	
	public float cameraRotation;
	private Locomotion _cacheLocomotion;
	
	[SerializeField]
	private Locomotion.MovementStyle _movementStyle;
	public Locomotion.MovementStyle movementStyle {
		get {
			return _movementStyle;
		}
		set {
			Locomotion.movementStyle = _movementStyle = value;
		}
	}
	
	[SerializeField]
	public FollowAvatar.FollowStyle followStyle;

	static public Player Instance {
		get {
			RefreshPlayer();
			return _player;
		}
	}

	public GameObject UMAAvatar {
		get {
			RefreshUMAAvatar();

			// Si no existe el UMAAvatar devolvemos el gameObject del Player
			return _umaAvatar ?? Instance.gameObject;
		}
		set {
			if (_umaAvatar == null) {
				RefreshUMAAvatar();
			}
			if (_umaAvatar != null) {
				Destroy(_umaAvatar);
			} else {
				Debug.LogWarning("Player avatar is not initialized.");
			}

			_umaAvatar = value;
			_umaAvatar.tag = TAG_UMA_AVATAR;
			_umaAvatar.transform.SetParent(transform);
			_umaAvatar.transform.position = transform.position;
			_cacheLocomotion = null;
			Locomotion.movementStyle = _movementStyle;

			//_umaAvatar.AddComponent<AudioListener>();
		}
	}

	public Locomotion Locomotion {
		get {
			if (_cacheLocomotion == null) {
				_cacheLocomotion = UMAAvatar.GetComponent<Locomotion>();
			}
			return _cacheLocomotion;
		}
	}

	void Start() {
		RefreshPlayer();
		RefreshUMAAvatar();

		// Por defecto, mantenemos al player desactivado
		if (_player != null) {
			_player.gameObject.SetActive(false);
		}
		if (_umaAvatar != null) {
			Locomotion.movementStyle = movementStyle;
		}
	}

	private static void RefreshPlayer() {
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		if (playerObj != null) {
			_player = playerObj.GetComponent<Player>();
		}
	}
	
	private void RefreshUMAAvatar() {
		GameObject avatarObj = GameObject.FindGameObjectWithTag(TAG_UMA_AVATAR);
		if (avatarObj != null) {
			_umaAvatar = avatarObj;
			_umaAvatar.GetComponentInChildren<Renderer>().enabled = avatarVisible;
		}
	}
	
	private static Player _player;
	private GameObject _umaAvatar;
}
