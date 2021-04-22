using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using System.Collections;
using Vuforia;

public class SampleBehaviour : MonoBehaviour
{
    public bool loadAsset;

    void Start()
    {
        if (loadAsset)
        {
            StartCoroutine(GetAssetBundle());
            Debug.Log("LOAD ASSETS");
        }
    }

    public void SetActive(bool state)
    {
        Debug.Log("Activated State: " + state);
        loadAsset = state;
        Start();
    }

    public IEnumerator GetAssetBundle()
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle("https://github.com/luk3Sky/unity-ar-test/raw/main/Unity%20Vuforia%20AR%20Project/Assets/AssetBundles/pikachu-Android");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            Debug.Log("LOADED BUNDLE: " + bundle);
            GameObject obj = bundle.LoadAsset<GameObject>("pikachu");
            Instantiate(obj);
        }
    }
}

