using UMA;
using UnityEngine;
using System.Collections;
using UMA.PowerTools;


namespace UMA.PowerTools
{
	[ExecuteInEditMode]
	public class UMAEditorAvatar : MonoBehaviour
	{
#if UNITY_EDITOR
		public Animator animator;
		public UMAGeneratorThreaded umaGenerator;
		public UMAContext context;
		public bool destroyParent;

		void Awake()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.update += new UnityEditor.EditorApplication.CallbackFunction(Update);
#endif
		}

		void OnDestroy()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= new UnityEditor.EditorApplication.CallbackFunction(Update);
#endif
		}

		private float lastFrameTime = -1;
		public void Update()
		{
			if (animator != null)
			{
				var currentFrameTime = Time.realtimeSinceStartup;
				if (lastFrameTime != -1)
				{
					animator.Update(currentFrameTime - lastFrameTime);
				}
				lastFrameTime = currentFrameTime;
			}
		}

		public bool Show(UMARecipeBase recipe, RuntimeAnimatorController animationController, System.Action<UMAData> callback)
		{
			umaGenerator = GetGenerator();
			if (umaGenerator == null) return false;
			var oldRunSingleThreaded = umaGenerator.runSingleThreaded;
			umaGenerator.runSingleThreaded = true;
			umaGenerator.Awake();

			var umaData = gameObject.AddComponent<UMAData>();
			umaData.umaRecipe = new UMAData.UMARecipe();
			if (callback != null)
			{
				umaData.OnCharacterUpdated += callback;
			}

			context = UMAContext.FindInstance();
			context.ValidateDictionaries();
			recipe.Load(umaData.umaRecipe, context);
			umaData.umaGenerator = umaGenerator;

			var umaChild = Instantiate(umaData.umaRecipe.raceData.racePrefab) as GameObject;
			umaChild.transform.parent = transform;
			umaChild.transform.localPosition = Vector3.zero;
			umaChild.transform.localRotation = Quaternion.identity;
			UMAData newUMA = umaChild.GetComponentInChildren<UMAData>();
			newUMA.SetupOnAwake();
			umaData.Assign(newUMA);
			newUMA.animator = null;
			newUMA.firstBake = true;
			DestroyImmediate(newUMA);

			umaData.Dirty(true, true, true);
			umaData.firstBake = true;
			umaData.animationController = animationController;
			while (!umaGenerator.IsIdle())
			{
				umaGenerator.Work();
			}
			lastFrameTime = -1;

			umaData.myRenderer.hideFlags = gameObject.hideFlags;
			umaData.firstBake = true;
			animator = umaData.animator;
			umaData.animator = null;
			DestroyImmediate(umaData);
			if (gameObject == umaGenerator.gameObject)
			{
				DestroyImmediate(umaGenerator);
			}
			else
			{
#if UNITY_EDITOR
				umaGenerator.runSingleThreaded = oldRunSingleThreaded;
				if (!UnityEditor.EditorApplication.isPlaying && umaGenerator.textureMergePrefab != null)
				{
					var textureMerger = umaGenerator.transform.FindChild(umaGenerator.textureMergePrefab.name + "(Clone)");
					if (textureMerger != null)
					{
						DestroyImmediate(textureMerger.gameObject);
					}
				}
#endif
			}
			return true;
		}

		private UMAGeneratorThreaded GetGenerator()
		{
			var uma = GameObject.Find("UMA");
			if (uma != null)
			{
				var generators = uma.GetComponentsInChildren<UMAGeneratorThreaded>();
				if (generators.Length > 0) return generators[0];
				var umagenerators = uma.GetComponentsInChildren<UMAGenerator>();
				if (umagenerators.Length > 0)
				{
					var umaGenerator = umagenerators[0];
					var res = gameObject.AddComponent<UMAGeneratorThreaded>();
					res.fitAtlas = umaGenerator.fitAtlas;
					res.textureMerge = umaGenerator.textureMerge;
					res.convertRenderTexture = umaGenerator.convertRenderTexture;
					res.convertMipMaps = umaGenerator.convertMipMaps;
					res.atlasResolution = umaGenerator.atlasResolution;
					res.textureNameList = umaGenerator.textureNameList;
					res.textureMergePrefab = umaGenerator.textureMergePrefab;
					res.runSingleThreaded = true;
					return res;
				}
				Debug.LogError("Unable to find a suitable UMAGenerator under the UMA GameObject", uma);
			}
			else
			{
				Debug.LogError("Unable to find a UMA GameObject to search for a suitable UMAGenerator");
			}
			return null;
		}

		public void Hide()
		{
			if (destroyParent)
			{
				DestroyImmediate(transform.parent.gameObject);
			}
			else
			{
				DestroyImmediate(gameObject);
			}
		}
#endif
	}
}
