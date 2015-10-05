using UnityEngine;
using System.Collections;
using UMA;

[RequireComponent (typeof (UMADynamicAvatar))]
public class AvatarLoaderAutoConfig : MonoBehaviour {
	
	
	
	// Use this for initialization
	void Awake () {
		var dynAvatarAux = GetComponent<UMADynamicAvatar>();
		if (dynAvatarAux.context == null) {
			dynAvatarAux.context = GameObject.FindObjectOfType<UMAContext>();
		} 
		if (dynAvatarAux.umaGenerator == null) {
			dynAvatarAux.umaGenerator = GameObject.FindObjectOfType<UMAGenerator>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

