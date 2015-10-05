using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UMA.PowerTools
{
	public static class PowerToolsRuntime
	{
		public static Type GetPreferredRecipeFormat()
		{
			foreach (var format in UMARecipeBase.GetRecipeFormats())
			{
				if (format.FullName == "UMA.RecipeTools.BinaryRecipeFloat")
					return format;
			}
#if StripLitJson
			return null;
#else
			return typeof(UMATextRecipe);
#endif
		}

		public static GameObject SaveCharacterPrefab(string assetFolder, string name, UMAData umaData)
		{
#if UNITY_EDITOR
			SkinnedMeshRenderer myRenderer = umaData.myRenderer;
			string[] textureNameList = umaData.umaGenerator.textureNameList;
			GameObject umaRoot = umaData.umaRoot;
			Avatar avatar = umaData.animator.avatar;

			EnsureProjectFolder(assetFolder);

			var asset = ScriptableObject.CreateInstance(GetPreferredRecipeFormat()) as UMARecipeBase;
			UMAContext context = UMAContext.Instance != null ? UMAContext.Instance : GameObject.Find("UMAContext").GetComponent<UMAContext>(); // temporary hack till FindInstance is made public
			asset.Save(umaData.umaRecipe, context);
			AssetDatabase.CreateAsset(asset, assetFolder+"/"+name+"_recipe.asset");
			AssetDatabase.SaveAssets();

			var sharedMaterials = myRenderer.sharedMaterials;
			foreach (var mat in sharedMaterials)
			{
				foreach (var texName in textureNameList)
				{
					if (mat.HasProperty(texName))
					{
						Texture tex = mat.GetTexture(texName);
						if (tex != null)
						{
							Texture2D newTex = tex as Texture2D;
							if (newTex == null)
							{
								newTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false, false);
								RenderTexture.active = tex as RenderTexture;
								newTex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, true);
								RenderTexture.active = null;
								newTex.name = name + "_" + texName + "_" + mat.shader.name.Replace('/', '_');
								WriteAllBytes(GetAssetPath(assetFolder + "/" + newTex.name + ".png"), newTex.EncodeToPNG());
							}
							else
							{
								if( string.IsNullOrEmpty(AssetDatabase.GetAssetPath(newTex)))
								{
									WriteAllBytes(GetAssetPath(assetFolder + "/" + name + "_" + texName + "_" + mat.shader.name.Replace('/', '_') + ".png"), newTex.EncodeToPNG());
								}
								else
								{
									string destname = assetFolder + "/" + name + "_" + texName + "_" + mat.shader.name.Replace('/', '_') + ".png";
									if (AssetDatabase.GetAssetPath(newTex) != destname)
									{
										System.IO.File.Copy(GetAssetPath(AssetDatabase.GetAssetPath(newTex)), GetAssetPath(destname));
									}
								}
							}
						}
					}
				}
			}
			AssetDatabase.Refresh();
			var mesh = myRenderer.sharedMesh;
			var prefab = PrefabUtility.CreatePrefab(assetFolder + "/" + name + ".prefab", umaData.gameObject, ReplacePrefabOptions.ConnectToPrefab);
			avatar.name = name;
			AssetDatabase.AddObjectToAsset(avatar, prefab);
			AssetDatabase.AddObjectToAsset(mesh, prefab);
			var materials = myRenderer.sharedMaterials;
			foreach (var mat in materials)
			{
				foreach (var texName in textureNameList)
				{
					if (mat.HasProperty(texName))
					{
						mat.SetTexture(texName, AssetDatabase.LoadMainAssetAtPath(assetFolder + "/" + name + "_" + texName + "_" + mat.shader.name.Replace('/', '_') + ".png") as Texture);
					}
				}
				AssetDatabase.AddObjectToAsset(mat, prefab);
			}
			myRenderer.materials = materials;
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(prefab));
			prefab.GetComponentsInChildren<Animator>(true)[0].avatar = avatar;
			var prefabUmaData = prefab.GetComponentsInChildren<UMAData>(true)[0];
			prefabUmaData.umaRoot = null;
			UnityEngine.Object.DestroyImmediate(prefabUmaData, true);
			UnityEngine.Object.DestroyImmediate(prefab.GetComponentsInChildren<UMAAvatarBase>(true)[0], true);
			var renderer = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true)[0];
			renderer.sharedMaterials = materials;
			renderer.sharedMesh = mesh;
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(prefab));
			return prefab;
#else
			throw new NotImplementedException("SaveCharacterPrefab Cannot save a prefab outside of the Unity environment. This method only works in the editor!");
#endif
		}

		public static void SaveCharacterPrefab(UMAData umaData, string prefabName)
		{
#if UNITY_EDITOR
			EnsureProjectFolder("Assets/UMA/UMA_Generated/Complete");
			var assetFolder = AssetDatabase.GenerateUniqueAssetPath("Assets/UMA/UMA_Generated/Complete/" + prefabName);
			SaveCharacterPrefab(assetFolder, prefabName, umaData);
#else
			throw new NotImplementedException("SaveCharacterPrefab Cannot save a prefab outside of the Unity environment. This method only works in the editor!");
#endif
		}

		#region helper methods

		public static void EnsureProjectFolder(string folder)
		{
#if UNITY_EDITOR
			if (!System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + "/" + folder))
			{
				EnsureProjectFolder(System.IO.Path.GetDirectoryName(folder));
				AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(folder), System.IO.Path.GetFileName(folder));
			}
#else
			throw new NotImplementedException("EnsureProjectFolder: The concept of ensuring a project folder outside the Unity environment is flawed. This method only works in the editor!");
#endif
		}

		public static string GetAssetPath(string path)
		{
			return System.IO.Directory.GetCurrentDirectory() + "/" + path;
		}

		public static bool FolderExists(string path)
		{
			return System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + "/" + path);
		}

		public static bool FileExists(string path)
		{
			return System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + "/" + path);
		}

		public static void WriteAllBytes(string path, byte[] data)
		{
			using (var file = System.IO.File.Open(path, System.IO.FileMode.OpenOrCreate))
			{
				file.Write(data, 0, data.Length);
				file.SetLength(data.Length);
				file.Flush();
			}
		}
		#endregion
	}
}
