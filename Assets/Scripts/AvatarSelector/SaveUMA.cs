using UnityEngine;
using System.Collections;

public class SaveUMA : MonoBehaviour {

	public bool save;
	public bool load;
	private int infoTime;
	public string info;

	// Update is called once per frame
	void Update () {
		
		if (load) {
			Load();
			//Debug.Log(GetRecipeBytes());
			
			load = false;
			info = "loaded";	
			infoTime = 200;
		} else if (save) {
			Debug.Log(GetRecipeString());
			//Debug.Log(GetRecipeBytes());
			
			save = false;
			info = "saved";	
			infoTime = 200;
		} else {
			infoTime -= 1;
			if (infoTime <= 0) {
				info = "";
			}
		}
	}
	
	public string GetRecipeString() {
		UMAContext context = GameObject.Find("UMAContext").GetComponent<UMAContext>();
		UMA.UMAData.UMARecipe ur = GetComponent<UMA.UMAData>().umaRecipe;
		UMATextRecipe utr = new UMATextRecipe();
		utr.Save(ur, context);
		return utr.recipeString;
	}
	
	public byte[] GetRecipeBytes() {
		UMAContext context = GameObject.Find("UMAContext").GetComponent<UMAContext>();
		UMA.UMAData.UMARecipe urb = GetComponent<UMA.UMAData>().umaRecipe;
		UMATextRecipe utr = new UMATextRecipe();
		utr.Save(urb, context);
		return utr.GetBytes();
	}
	
	public void Load() {
		UMAContext context = GameObject.Find("UMAContext").GetComponent<UMAContext>();
		UMA.UMAData recipeData = GetComponent<UMA.UMAData>();
		UMATextRecipe umaTextRecipe = ScriptableObject.CreateInstance("UMATextRecipe") as UMATextRecipe;
		
		umaTextRecipe.recipeString = LoadText();
		umaTextRecipe.Load(recipeData.umaRecipe, context);
		
		recipeData.Dirty(true, true, true);
	}
	
	public string LoadText() {
		return "{\"version\":2,\"packedSlotDataList\":null,\"slotsV2\":[{\"id\":\"FemaleEyes\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"EyeOverlay\",\"colorIdx\":0,\"rect\":[0,0,0,0]},{\"id\":\"EyeOverlayAdjust\",\"colorIdx\":1,\"rect\":[64,64,128,128]}]},{\"id\":\"FemaleHead_Head\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"FemaleHead01\",\"colorIdx\":2,\"rect\":[0,0,0,0]},{\"id\":\"FemaleEyebrow01\",\"colorIdx\":3,\"rect\":[384,256,256,64]},{\"id\":\"FemaleLipstick01\",\"colorIdx\":4,\"rect\":[480,203,64,32]},{\"id\":\"FemaleLongHair01\",\"colorIdx\":5,\"rect\":[0,0,0,0]}]},{\"id\":\"FemaleHead_Mouth\",\"scale\":100,\"copyIdx\":1,\"overlays\":null},{\"id\":\"FemaleHead_Eyes\",\"scale\":100,\"copyIdx\":1,\"overlays\":null},{\"id\":\"FemaleHead_Nose\",\"scale\":100,\"copyIdx\":1,\"overlays\":null},{\"id\":\"FemaleHead_ElvenEars\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"ElvenEars\",\"colorIdx\":2,\"rect\":[0,0,0,0]}]},{\"id\":\"FemaleEyelash\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"FemaleEyelash\",\"colorIdx\":6,\"rect\":[0,0,0,0]}]},{\"id\":\"FemaleTorso\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"FemaleBody02\",\"colorIdx\":2,\"rect\":[0,0,0,0]},{\"id\":\"FemaleUnderwear01\",\"colorIdx\":7,\"rect\":[0,512,1024,512]},{\"id\":\"FemaleJeans01\",\"colorIdx\":8,\"rect\":[0,0,0,0]}]},{\"id\":\"FemaleHands\",\"scale\":100,\"copyIdx\":7,\"overlays\":null},{\"id\":\"FemaleFeet\",\"scale\":100,\"copyIdx\":7,\"overlays\":null},{\"id\":\"FemaleInnerMouth\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"InnerMouth\",\"colorIdx\":0,\"rect\":[0,0,0,0]}]},{\"id\":\"FemaleLegs\",\"scale\":100,\"copyIdx\":7,\"overlays\":null},{\"id\":\"FemaleTshirt01\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"FemaleTshirt01\",\"colorIdx\":9,\"rect\":[0,0,0,0]}]},{\"id\":\"FemaleLongHair01\",\"scale\":100,\"copyIdx\":1,\"overlays\":null},{\"id\":\"FemaleLongHair01_Module\",\"scale\":100,\"copyIdx\":-1,\"overlays\":[{\"id\":\"FemaleLongHair01_Module\",\"colorIdx\":5,\"rect\":[0,0,0,0]}]}],\"colors\":null,\"fColors\":[{\"name\":null,\"colors\":[255,255,255,0,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[56,158,33,0,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[175,164,179,510,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[32,15,15,0,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[251,215,230,510,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[44,132,228,30,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[0,0,0,255,0,0,0,0]},{\"name\":null,\"colors\":[43,149,171,0,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[112,29,113,0,0,0,0,0,255,255,255,255,0,0,0,0]},{\"name\":null,\"colors\":[119,116,59,0,0,0,0,0,255,255,255,255,0,0,0,0]}],\"sharedColorCount\":0,\"race\":\"HumanFemale\",\"umaDna\":{},\"packedDna\":[{\"dnaType\":\"UMADnaHumanoid\",\"packedDna\":\"{\\\"height\\\":78,\\\"headSize\\\":130,\\\"headWidth\\\":142,\\\"neckThickness\\\":129,\\\"armLength\\\":125,\\\"forearmLength\\\":126,\\\"armWidth\\\":127,\\\"forearmWidth\\\":111,\\\"handsSize\\\":125,\\\"feetSize\\\":126,\\\"legSeparation\\\":125,\\\"upperMuscle\\\":77,\\\"lowerMuscle\\\":81,\\\"upperWeight\\\":102,\\\"lowerWeight\\\":124,\\\"legsSize\\\":149,\\\"belly\\\":102,\\\"waist\\\":79,\\\"gluteusSize\\\":113,\\\"earsSize\\\":113,\\\"earsPosition\\\":139,\\\"earsRotation\\\":120,\\\"noseSize\\\":85,\\\"noseCurve\\\":105,\\\"noseWidth\\\":156,\\\"noseInclination\\\":132,\\\"nosePosition\\\":88,\\\"nosePronounced\\\":146,\\\"noseFlatten\\\":80,\\\"chinSize\\\":121,\\\"chinPronounced\\\":151,\\\"chinPosition\\\":138,\\\"mandibleSize\\\":122,\\\"jawsSize\\\":116,\\\"jawsPosition\\\":127,\\\"cheekSize\\\":91,\\\"cheekPosition\\\":110,\\\"lowCheekPronounced\\\":180,\\\"lowCheekPosition\\\":190,\\\"foreheadSize\\\":82,\\\"foreheadPosition\\\":140,\\\"lipsSize\\\":178,\\\"mouthSize\\\":132,\\\"eyeRotation\\\":141,\\\"eyeSize\\\":201,\\\"breastSize\\\":123}\"},{\"dnaType\":\"UMADnaTutorial\",\"packedDna\":\"{\\\"eyeSpacing\\\":128}\"}]}";
	}
}
