using UnityEngine;
using System.Collections;

public class SyncCameraTransform : MonoBehaviour {

	public GameObject cameraSynced;

	// Use this for initialization
	void Start () {
		if (cameraSynced == null && Camera.main != null) {
			cameraSynced = Camera.main.gameObject;
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (cameraSynced == null && Camera.main != null) {
			cameraSynced = Camera.main.gameObject;
		}
		if (cameraSynced != null) {
			cameraSynced.transform.position = transform.position;
			cameraSynced.transform.rotation = transform.rotation;
		}
	}
}
