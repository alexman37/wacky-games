using UnityEngine;
using System.Collections;

public class LoadAssetBundle
{
    public delegate void callback0<T>(T anyArg);

    // Load an entire asset bundle
    public static IEnumerator LoadBundle<T>(string assetBundleName, callback0<T[]> callback) where T : UnityEngine.Object
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);

        //Load designated AssetBundle (word group)
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

        //Load the text file proper
        AssetBundleRequest asset = assetBundle.LoadAllAssetsAsync<T>();
        yield return asset;

        //Retrieve the object
        T[] raw = new T[asset.allAssets.Length];
        for(int i = 0; i < raw.Length; i++)
        {
            raw[i] = asset.allAssets[i] as T;
        }

        // Call callback method supplied as arg
        callback(raw);

        assetBundle.Unload(false);
        Debug.Log("Successfully loaded and used assetbundle of name " + assetBundleName);
    }



    // Load a specific object from an asset bundle
    public static IEnumerator LoadAssetObject<T>(string assetBundleName, string objectNameToLoad, callback0<T> callback) where T : UnityEngine.Object
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);

        //Load designated AssetBundle (word group)
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

        //Load the text file proper
        AssetBundleRequest asset = assetBundle.LoadAssetAsync<T>(objectNameToLoad);
        yield return asset;

        //Retrieve the object
        T raw = asset.asset as T;

        // Do something with it
        callback(raw);

        assetBundle.Unload(false);
        Debug.Log("Successfully loaded and used object " + objectNameToLoad + " from assetbundle of name " + assetBundleName);
    }
}
