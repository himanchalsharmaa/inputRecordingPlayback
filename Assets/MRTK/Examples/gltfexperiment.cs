using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using System;
using System.Globalization;
using GLTFast.Export;
using GLTFast;

public class gltfexperiment : MonoBehaviour
{
    public TMP_Text gltfexportinfo;
    public TMP_Text gltfimportinfo;
    public GameObject[] exportobject=new GameObject[1];
    public GameObject borngo;
    public async void gltfwrite()
    {
        var export = new GameObjectExport();
        export.AddScene(exportobject);

        // Async glTF export
        string path =Path.Combine(Application.persistentDataPath,"GlTFExport/gltfastCube.gltf");
        bool success = await export.SaveToFileAndDispose(path);

        if (!success)
        {
            Debug.LogError("Something went wrong exporting a glTF");
        }
        Debug.Log("Exported");
    }
    public async void gltfread()
    {
        gltfimportinfo.text = "Destroying GO and waiting for 3s";
        Destroy(exportobject[0]);
        gltfimportinfo.text = "Loading GO";
        var gltf = new GLTFast.GltfImport();
        var success = await gltf.Load("file://"+Application.persistentDataPath+"/GlTFExport/gltfastCube.gltf");
        if (success)
        {
            gltf.InstantiateMainScene(new GameObject("glTF").transform);
        }
        else {
            Debug.Log("Error: "+success);
                }
    }
    public async void LoadGltfBinaryFromMemory()
    {
        var filePath = Application.persistentDataPath+"/GlTFExport/gltfastCube.bin";
        byte[] data = File.ReadAllBytes(filePath);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(
            data,
            // The URI of the original data is important for resolving relative URIs within the glTF
            new Uri(filePath)
            );
        if (success)
        {
            success = gltf.InstantiateMainScene(transform);
        }
    }

}
