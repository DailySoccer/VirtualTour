﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetDefinition {
	public string Id;
	public int Version;

	public AssetDefinition(string id, int version) {
		Id = id;
		Version = version;
		// Debug.Log (string.Format("AssetDefinition: Id: {0} Version: {1}", Id, Version));
	}
	
	static public AssetDefinition LoadFromJSON(Hashtable jsonMap) {
		AssetDefinition assetDefinition = new AssetDefinition(
			jsonMap[KEY_ID] as string,
			System.Convert.ToInt32(jsonMap[KEY_VERSION])
			);
		return assetDefinition;
	}
	
	const string KEY_ID = "id";
	const string KEY_VERSION = "version";
}

public class DLCManager : MonoBehaviour {

	static public DLCManager Instance {
		get {
			GameObject mainManagerObj = GameObject.FindGameObjectWithTag("MainManager");
			if (mainManagerObj == null)
				return null;

			DLCManager dlcManager = mainManagerObj.GetComponent<DLCManager>();
			return dlcManager.enabled ? dlcManager : null;
		}
	}
	
	public TextAsset Assets;
	public string AssetsUrl;
	public Dictionary<string, AssetDefinition> AssetDefinitions = new Dictionary<string, AssetDefinition>();
	public Dictionary<string, AssetBundle> AssetResources = new Dictionary<string, AssetBundle>();

	string BaseUrl {
		get {
#if UNITY_ANDROID
			return AssetsUrl + "/Android/";
#elif UNITY_IOS
			return AssetsUrl + "/iOS/";
#else
			return AssetsUrl + "/";
#endif
		}
	}

	void Awake () {
		LoadDefinitions ();
	}

	void Start () {
	}
	
	void Update () {
	}

	public void ClearResources() {
		foreach(string key in AssetResources.Keys) {
			AssetBundle bundle = AssetResources[key];
			bundle.Unload(true);
		}
		AssetResources.Clear();
	}

	public IEnumerator LoadResource(string keyResource, System.Action<AssetBundle> callback = null) {
		if (!AssetDefinitions.ContainsKey(keyResource)) {
			yield break;
		}

		if (AssetResources.ContainsKey(keyResource)) {
			if (callback != null) callback(AssetResources[keyResource]);
			yield break;
		}

		// Wait for the Caching system to be ready
		while (!Caching.ready)
			yield return null;

		AssetDefinition definition = AssetDefinitions[keyResource];

		// Ignoramos las versiones 0...
		if (definition.Version > 0) {
			Debug.Log ("DLCManager: Loading... " + definition.Id);

			// Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
			using(WWW www = WWW.LoadFromCacheOrDownload (BaseUrl + definition.Id, definition.Version)) {
				yield return www;
				
				if (string.IsNullOrEmpty(www.error)) {
					AssetBundle bundle = www.assetBundle;
					bundle.LoadAllAssets();
					AssetResources[keyResource] = bundle;
					if (callback != null) callback(bundle);
				}
				else {
					Debug.LogError("WWW download had an error:" + www.error);
					// throw new Exception("WWW download had an error:" + www.error);
				}
			} // memory is freed from the web stream (www.Dispose() gets called implicitly)
		}
	}
	
	public IEnumerator CacheResources() {
		// Wait for the Caching system to be ready
		while (!Caching.ready)
			yield return null;

		foreach(string key in AssetDefinitions.Keys) {
			AssetDefinition definition = AssetDefinitions[key];

			// Ignoramos las versiones 0...
			if (definition.Version > 0) {
				Debug.Log ("DLCManager: Cache... " + definition.Id);

				// Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
				using(WWW www = WWW.LoadFromCacheOrDownload (BaseUrl + definition.Id, definition.Version)) {
					yield return www;

					if (string.IsNullOrEmpty(www.error)) {
						AssetBundle bundle = www.assetBundle;
						bundle.Unload(false);
					}
					else {
						Debug.LogError("WWW download had an error:" + www.error);
						// throw new Exception("WWW download had an error:" + www.error);
					}
				} // memory is freed from the web stream (www.Dispose() gets called implicitly)
			}
		}
	}
	
	private void LoadDefinitions() {
		#if UNITY_ANDROID
		string resourceName = "Android/assetbundles";
#elif UNITY_IOS
		string resourceName = "iOS/assetbundles";
#else
		string resourceName = "";
#endif

		TextAsset jsonFile = !string.IsNullOrEmpty(resourceName) ? Resources.Load<TextAsset>(resourceName) : null;
		if (jsonFile != null) {
			Debug.Log ("DLCManager: AssetBundles: " + resourceName);
		}
		else {
			jsonFile = Assets;
			Debug.Log ("DLCManager: AssetBundles: Default");
		}

		object json = JSON.JsonDecode(jsonFile.text);
		if (json is Hashtable) {
			Hashtable jsonMap = json as Hashtable;
			ArrayList assets = jsonMap[KEY_ASSETS] as ArrayList;
			foreach (object asset in assets) {
				if (asset is Hashtable) {
					AssetDefinition assetDefinition = AssetDefinition.LoadFromJSON(asset as Hashtable);
					AssetDefinitions.Add (assetDefinition.Id, assetDefinition);
				}
			}
		}
	}

	const string KEY_ASSETS = "assets";
}
