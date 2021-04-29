using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Vuforia;
using System.IO;
using System.Collections.Generic;

public class SampleBehaviour : MonoBehaviour
{
    public bool loadAsset;
    protected string myAsset = "pikachu";
    protected ImageTarget imageTarget;
    protected TrackableBehaviour mTrackableBehaviour;
    protected Transform objTrf;
    protected bool visibility = false;

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
        visibility = state;
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (state)
        {
            Start();
        }
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
            string cachePath = Caching.defaultCache.path;
            Debug.Log(cachePath + "\n" + Caching.defaultCache + "\n" + Application.dataPath);
            /*FileInfo[] files = new DirectoryInfo(cachePath).GetFiles();
            string str = "";
            foreach (FileInfo file in files)
            {
                str = str + ", " + file.Name;
                Debug.Log(str);
            }*/
            GameObject obj = bundle.LoadAsset<GameObject>(myAsset);

            /*Debug.Log("Type: " + mTrackableBehaviour.GetType() + "\n" + "Component: " + mTrackableBehaviour.GetComponent<GameObject>());
            GameObject targetObject = mTrackableBehaviour.GetComponent<GameObject>();*/
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            Vector3 scale = transform.localScale;
            Debug.Log("Position: " + position + "\nRotation: " + rotation + "\nScale: " + scale);
            /*imageTarget = (ImageTarget)FindObjectOfType(typeof(ImageTarget));
            Debug.Log("Image Target: " + imageTarget);*/
            objTrf = Instantiate(obj).transform;
            objTrf.position = position;
            objTrf.rotation = rotation;
            objTrf.localScale = scale;
            objTrf.parent = mTrackableBehaviour.transform;
            if (visibility)
            {
                objTrf.gameObject.SetActive(true);

            }
            else
            {
                objTrf.gameObject.SetActive(false);
            }
            bundle.Unload(false);
        }
    }

    IEnumerator DownloadAndCacheAssetBundle(string uri, string manifestBundlePath)
    {
        //Load the manifest
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestBundlePath);
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        //Create new cache
        string today = DateTime.Today.ToLongDateString();
        Directory.CreateDirectory(today);
        Cache newCache = Caching.AddCache(today);

        //Set current cache for writing to the new cache if the cache is valid
        if (newCache.valid)
            Caching.currentCacheForWriting = newCache;

        //Download the bundle
        Hash128 hash = manifest.GetAssetBundleHash("bundleName");
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, hash, 0);
        yield return request.SendWebRequest();
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);

        //Get all the cached versions
        List<Hash128> listOfCachedVersions = new List<Hash128>();
        Caching.GetCachedVersions(bundle.name, listOfCachedVersions);

        if (!AssetBundleContainsAssetIWantToLoad(bundle))     //Or any conditions you want to check on your new asset bundle
        {
            //If our criteria wasn't met, we can remove the new cache and revert back to the most recent one
            Caching.currentCacheForWriting = Caching.GetCacheAt(Caching.cacheCount);
            Caching.RemoveCache(newCache);

            for (int i = listOfCachedVersions.Count - 1; i > 0; i--)
            {
                //Load a different bundle from a different cache
                request = UnityWebRequestAssetBundle.GetAssetBundle(uri, listOfCachedVersions[i], 0);
                yield return request.SendWebRequest();
                bundle = DownloadHandlerAssetBundle.GetContent(request);

                //Check and see if the newly loaded bundle from the cache meets your criteria
                if (AssetBundleContainsAssetIWantToLoad(bundle))
                    break;
            }
        }
        else
        {
            //This is if we only want to keep 5 local caches at any time
            if (Caching.cacheCount > 5)
                Caching.RemoveCache(Caching.GetCacheAt(1));     //Removes the oldest user created cache
        }
    }

    bool AssetBundleContainsAssetIWantToLoad(AssetBundle bundle)
    {
        return (bundle.LoadAsset<GameObject>("MyAsset") != null);     //this could be any conditional
    }
}

