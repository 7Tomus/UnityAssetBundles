using UnityEngine;
using UnityEngine.UI;

public class LoadAssetBundle : MonoBehaviour {

	public void LoadAsset()
	{
		AssetBundle catBundle = AssetBundle.LoadFromFile(Application.dataPath + "/AssetBundles/StandaloneWindows/cats");
		if(catBundle == null)
		{
			Debug.Log("Cat bundle failed to load");
			return;
		}
		Sprite catSpriteSmall = catBundle.LoadAsset<Sprite>("cat_big");
		GetComponent<Image>().sprite = catSpriteSmall;
	}
}
