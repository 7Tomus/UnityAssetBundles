using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class LoadAssetBundle : MonoBehaviour {

	public Slider progressBar;
	public string uri;

	private string assetPath;

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

	public void LoadAssetRemote()
	{
		StartCoroutine(LoadFromWeb(uri));
	}

	public void OnClick1()
	{
	#if UNITY_EDITOR
			uri = "http://beta.biathlonmania.com/assetBundle/Windows/cats";
	#elif UNITY_ANDROID
			uri = "http://beta.biathlonmania.com/assetBundle/Android/cats";
	#else
			uri = "http://beta.biathlonmania.com/assetBundle/Windows/cats";
	#endif
		StartCoroutine(LoadFromWebAndSave(uri));
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

	IEnumerator LoadFromWebAndSave(string uri)
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
			string dataFileName = "cats";
			string tempPath = Path.Combine(Application.persistentDataPath, "AssetData");
			tempPath = Path.Combine(tempPath, dataFileName + ".unity3d");
			assetPath = tempPath;
			Save(handle.data, tempPath);
			yield return StartCoroutine(LoadAssetBundleFromStorage(tempPath));
		}
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

	IEnumerator LoadAssetBundleFromStorage(string path)
	{
		AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(path);
		yield return bundle;
		AssetBundle myLoadedAssetBundle = bundle.assetBundle;
		if(myLoadedAssetBundle == null)
		{
			Debug.Log("Failed to load AssetBundle!");
			yield return null;
		}

		AssetBundleRequest request = myLoadedAssetBundle.LoadAssetAsync<Sprite>("cat");
		yield return request;

		Sprite catSpriteSmall = request.asset as Sprite;
		GetComponent<Image>().sprite = catSpriteSmall;
	}

	private IEnumerator ShowProgress(UnityWebRequest www)
	{
		while(!www.isDone)
		{
			progressBar.value = www.downloadProgress;
			yield return new WaitForSeconds(0.1f);
		}
		progressBar.value = 1;
		Debug.Log("Done");
	}
}
