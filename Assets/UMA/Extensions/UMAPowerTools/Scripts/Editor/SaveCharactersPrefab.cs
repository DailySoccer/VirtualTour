using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UMA.PowerTools
{
	public class UMASaveCharacters : Editor
	{
		public static bool FileExists(string path)
		{
			return System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + "/" + path);
		}

		[MenuItem("UMA/Power Tools/Save Character Prefabs")]
		static void SaveCharacterPrefabsMenuItem()
		{
			var newCharacters = new List<GameObject>();
			HashSet<UMAAvatarBase> saved = new HashSet<UMAAvatarBase>();
			foreach (var go in Selection.gameObjects)
			{
				var selectedTransform = go.transform;
				UMAAvatarBase avatar = selectedTransform.GetComponent<UMAAvatarBase>();
				while( avatar == null && selectedTransform.parent != null )
				{
					selectedTransform = selectedTransform.parent;
					avatar = selectedTransform.GetComponent<UMAAvatarBase>();
				}
				if (avatar != null && PrefabUtility.GetPrefabObject(avatar.umaData.umaRoot) == null)
				{
					if (saved.Add(avatar))
					{
						var path = EditorUtility.SaveFilePanelInProject("Save Avatar Prefab", avatar.name + ".prefab", "prefab", "Save Avatar Prefab", "Assets/UMA/UMA_Generated/Complete");
						if (FileExists(path))
						{
							Debug.LogWarning("Overwrite of prefabs not supported!");
						}
						else if (path.Length != 0)
						{
							newCharacters.Add(SaveCharacterPrefab(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path), avatar.umaData));
						}
					}
				}
			}
			if (newCharacters.Count > 0)
			{
				Selection.objects = newCharacters.ToArray();
			}
		}
		public static GameObject SaveCharacterPrefab(string assetFolder, string name, UMAData umaData)
		{
			return PowerToolsRuntime.SaveCharacterPrefab(assetFolder, name, umaData);
		}

		public static void SaveCharacterPrefab(UMAData umaData, string prefabName)
		{
			PowerToolsRuntime.SaveCharacterPrefab(umaData, prefabName);
		}
	}
}
