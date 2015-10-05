using UnityEngine;
using System.Collections;

public class GUIFadeScreen : UIScreen {

	public GameObject Loading;

	void OnEnable() {
	}

	void Start () {
	}
	
	void Update () {
		if (Loading != null) {
			Loading.transform.Rotate(0,10,0);
		}
	}
}
