using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AvatarsLibrary : MonoBehaviour {

	public static UMATextRecipe defaultRecipe;

	public List<UMATextRecipe> assetRecipes;
	
	public UMATextRecipe GetRecipe(int id) {
		return assetRecipes[id];
	}
	
	void Start() {
		defaultRecipe = defaultRecipe ?? assetRecipes.Count > 0? assetRecipes[0] : null;
	}


}
