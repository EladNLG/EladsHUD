using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AutoBuild : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private static Dictionary<string, string> ModBundleMap = new Dictionary<string, string>(){
        {"EladsHUD","assets"} ///EDIT WITH NAME OF MOD ASSEMBLY AND NAME OF ASSET BUNDLE (CASE MATTERS)
    };

    [MenuItem("BuildTools/Build AssetBundles")]
    static void BuildAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        System.IO.DirectoryInfo di = new DirectoryInfo(assetBundleDirectory);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    public static string OnGeneratedCSProject(string path, string content)
    {
        foreach (var mod in ModBundleMap.Keys)
        {
            if (path.EndsWith($"{mod}.csproj"))
            {
                string newContent = "";
                bool Added = false;
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    if (!Added && line.Contains("<Compile Include="))
                    {
                        newContent += $"     <EmbeddedResource Include=\"Assets\\AssetBundles\\{ModBundleMap[mod]}\" />\n";
                        Added = true;
                    }
                    newContent += line + "\n";
                }
                return newContent;
            }
        }
        return content;
    }
}
