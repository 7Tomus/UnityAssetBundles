using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AssetBundleManager : MonoBehaviour
{
	string url = "http://beta.biathlonmania.com/assetBundle/Windows";
	public Button button;
	public Slider progressBar;

	#region WholeStuff
	public void LoadImage(string bundleName)
	{
		AssetBundle assetBundle = GetAssetBundle(bundleName, progressBar, button).Result;
		AssetBundleRequest request = assetBundle.LoadAssetAsync<Sprite>("cat");
		Sprite catSpriteSmall = request.asset as Sprite;
		GetComponent<Image>().sprite = catSpriteSmall;
	}


	private async Task<AssetBundle> GetAssetBundle(string bundleName, Slider loadingSlider = null, Button triggerButton = null)
	{
		#if UNITY_ANDROID
			url = "http://beta.biathlonmania.com/assetBundle/Android";
		#elif UNITY_IOS
			url = "http://beta.biathlonmania.com/assetBundle/IOS"
		#endif
		string localPath = Path.Combine(Application.persistentDataPath, "AssetData");
		localPath = Path.Combine(localPath, bundleName);
		if(File.Exists(localPath))
		{
			return LoadAssetBundleFromDisk(bundleName, localPath);
		}
		else
		{
			UnityWebRequest www = UnityWebRequest.Get(Path.Combine(url, bundleName));
			DownloadHandler handle = www.downloadHandler;
			
			if(loadingSlider != null && triggerButton != null)
			{
				StartCoroutine(ShowProgress(www, loadingSlider, triggerButton));
			}
			
			www.SendWebRequest();

			if(www.isNetworkError || www.isHttpError)
			{
				Debug.LogError(www.error);
				return null;
			}
			else
			{
				SaveAssetBundleToDisk(handle.data, localPath);
				return LoadAssetBundleFromDisk(bundleName, localPath);
			}
		}
	}
	private AssetBundle GetLoadedBundle(string bundleName)
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
	private IEnumerator ShowProgress(UnityWebRequest www, Slider assetBundleLoadingSlider, Button nextButton)
	{
		nextButton.interactable = false;
		while(!www.isDone)
		{
			assetBundleLoadingSlider.value = www.downloadProgress;
			yield return new WaitForSeconds(0.1f);
		}
		assetBundleLoadingSlider.value = 0;
		nextButton.interactable = true;
	}
	private void SaveAssetBundleToDisk(byte[] data, string path)
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
	private AssetBundle LoadAssetBundleFromDisk(string bundleName, string path)
	{
		AssetBundle assetBundle;
		assetBundle = GetLoadedBundle(bundleName);
		if(assetBundle == null)
		{
			AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(path);
			assetBundle = bundle.assetBundle;
			if(assetBundle == null)
			{
				Debug.Log("Failed to load AssetBundle!");
				return null;
			}
		}
		return assetBundle;
	}
	#endregion

	public void DownloadAssetBundle(string bundleName)
	{

	}

	private async Task TestDownloadAssetBundle(string bundleName)
	{
#if UNITY_ANDROID
			url = "http://beta.biathlonmania.com/assetBundle/Android";
#elif UNITY_IOS
			url = "http://beta.biathlonmania.com/assetBundle/IOS"
#endif

		string localPath = Path.Combine(Application.persistentDataPath, "AssetData");
		localPath = Path.Combine(localPath, bundleName);
		UnityWebRequest www = UnityWebRequest.Get(Path.Combine(url, bundleName));
		DownloadHandler handle = www.downloadHandler;

	}
}
