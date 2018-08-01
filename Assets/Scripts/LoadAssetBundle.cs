using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class LoadAssetBundle : MonoBehaviour {

	private string uri = "http://beta.biathlonmania.com/assetBundle/cats";

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

	public void LoadAssetRemoteAndSave()
	{
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
		yield return www.Send();

		if(www.isNetworkError || www.isHttpError)
		{
			Debug.LogError(www.error);
		}
		else
		{
			string dataFileName = "catss";
			string tempPath = Path.Combine(Application.persistentDataPath, "AssetData");
			tempPath = Path.Combine(tempPath, dataFileName + ".unity3d");
			Save(handle.data, tempPath);			
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

	IEnumerable LoadAssetBundleFromStorage(string path)
	{
		AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(path);
		yield return bundle;

		AssetBundle myLoadedAssetBundle = bundle.assetBundle;
		if(myLoadedAssetBundle == null)
		{
			Debug.Log("Failed to load AssetBundle!");
			yield break;
		}

		AssetBundleRequest request = myLoadedAssetBundle.LoadAssetAsync<GameObject>("cat");
		yield return request;

		Sprite catSpriteSmall = request.asset as Sprite;
		GetComponent<Image>().sprite = catSpriteSmall;
	}
}
