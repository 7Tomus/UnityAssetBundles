﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class LoadAssetBundle : MonoBehaviour {

	public Slider progressBar;
	public string uri;

	private void Awake()
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
		path = Path.Combine(path, bundleName + ".unity3d");

		if(File.Exists(path))
		{
			Debug.Log("Bundle is already in storage");
			yield return StartCoroutine(LoadFromStorage(path));
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
				yield return StartCoroutine(LoadFromStorage(path));
			}
		}	
	}

	IEnumerator LoadFromStorage(string path)
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
