using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class LoadAssetBundle : MonoBehaviour {

	public Button button;
	public Slider progressBar;
	public string uri;

	private void Start()
	{
	#if UNITY_EDITOR
		uri = "http://beta.biathlonmania.com/assetBundle/Windows/cats";
	#elif UNITY_ANDROID
		uri = "http://beta.biathlonmania.com/assetBundle/Android/cats";
	#elif UNITY_IOS
		uri = "http://beta.biathlonmania.com/assetBundle/iOS/cats";
	#else
		uri = "http://beta.biathlonmania.com/assetBundle/Windows/cats";
	#endif
	}

	public void OnClick()
	{
		StartCoroutine(LoadFromWebAndSave(uri));
	}

	IEnumerator LoadFromWebAndSave(string uri)
	{
		string bundleName = "cats";
		string path = Path.Combine(Application.persistentDataPath, "AssetData");
		path = Path.Combine(path, bundleName);

		if(File.Exists(path))
		{
			yield return StartCoroutine(LoadFromStorage(path, bundleName));
		}
		else
		{
			UnityWebRequest www = UnityWebRequest.Get(uri);
			DownloadHandler handle = www.downloadHandler;
			StartCoroutine(ShowProgress(www));
			yield return www.Send();

			if(www.isNetworkError || www.isHttpError)
			{
				Debug.LogError(www.error);
				yield return null;
			}
			else
			{
				Save(handle.data, path);
				yield return StartCoroutine(LoadFromStorage(path, bundleName));
			}
		}	
	}

	IEnumerator LoadFromStorage(string path, string bundleName)
	{
		AssetBundle myLoadedAssetBundle = CheckIfBundleIsLoaded(bundleName);
		if(myLoadedAssetBundle == null)
		{
			AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(path);
			yield return bundle;
			myLoadedAssetBundle = bundle.assetBundle;
			if(myLoadedAssetBundle == null)
			{
				Debug.Log("Failed to load AssetBundle!");
				yield return null;
			}
		}

		AssetBundleRequest request = myLoadedAssetBundle.LoadAssetAsync<Sprite>("cat");
		yield return request;

		Sprite catSpriteSmall = request.asset as Sprite;
		GetComponent<Image>().sprite = catSpriteSmall;
	}

	private AssetBundle CheckIfBundleIsLoaded(string bundleName)
	{
		AssetBundle[] bundles = Resources.FindObjectsOfTypeAll(typeof(AssetBundle)) as AssetBundle[];
		for(int i = 0; i < bundles.Length; i++)
		{
			if(bundleName.Equals(bundles[i].name))
			{
				return bundles[i];
			}
		}
		return null;
	}

	private IEnumerator ShowProgress(UnityWebRequest www)
	{
		button.interactable = false;
		while(!www.isDone)
		{
			progressBar.value = www.downloadProgress;
			yield return new WaitForSeconds(0.1f);
		}
		progressBar.value = 0;
		Debug.Log("Done");
		button.interactable = true;
	}

	private void Save(byte[] data, string path)
	{
		if(!Directory.Exists(Path.GetDirectoryName(path)))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
		}

		try
		{
			File.WriteAllBytes(path, data);
			Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
		}
		catch(Exception e)
		{
			Debug.LogWarning("Failed To Save Data" + e.Message);
		}
	}

	public void ClearImage()
	{
		GetComponent<Image>().sprite = null;
	}

	#region NotUsedAnymore

	public void LoadAssetRemote()
	{
		StartCoroutine(LoadFromWeb(uri));
	}

	IEnumerator LoadFromWeb(string uri)
	{
		UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(uri);
		yield return www.SendWebRequest();

		if(www.isNetworkError || www.isHttpError)
		{
			Debug.LogError(www.error);
		}
		else
		{
			AssetBundle catBundle = DownloadHandlerAssetBundle.GetContent(www);
			Sprite catSpriteSmall = catBundle.LoadAsset<Sprite>("cat_big");
			GetComponent<Image>().sprite = catSpriteSmall;
		}
	}

	public void LoadAssetLocal()
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

	#endregion
}
