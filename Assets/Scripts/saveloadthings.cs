using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;

public class saveloadthings : MonoBehaviour
{
    public TMP_Text saveinfo;
    public TMP_Text loadinfo; 
    private GameObject gameObject;
    private List<GameObject> gameObjload = new List<GameObject>();

    public void datasaver(GameObject gameObject,int i)
    {
 
        float[] pos = { gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z };
        float[] rot = { gameObject.transform.rotation.x, gameObject.transform.rotation.y, gameObject.transform.rotation.z, gameObject.transform.rotation.w };
        float[] scale = { gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z };
        BinaryFormatter bf = new BinaryFormatter();
        string localPathtrans = Application.persistentDataPath+"/"+gameObject.name+".txt";
        saveinfo.text = "opening filestream"+ Application.persistentDataPath;
        FileStream stream = new FileStream(localPathtrans, FileMode.Create);
        TransformInfo transformInfo = new TransformInfo(gameObject);
        saveinfo.text = "writting to file stream ";
        bf.Serialize(stream, transformInfo);
        /*saveinfo.text = "trying to save prefabs";
        SaveasPrefab(gameObject, localPathpref); */
        stream.Close(); 
        saveinfo.text = "done saving, stream closed.Objects saved: "+i;
    }
    public static void SaveasPrefab(GameObject gameObject, string localPath)
    {
       /*
        bool prefabSuccess;
        PrefabUtility.SaveAsPrefabAsset(gameObject, localPath, out prefabSuccess);
        if (prefabSuccess == true)
            Debug.Log("Prefab was saved successfully");
        else
            Debug.Log("Prefab failed to save" + prefabSuccess); */ 
    }

    public void Savefunction()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            
            i = i + 1;
            datasaver(child.gameObject,i);
        }
    }

    public void loadfunction() {
        //string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath);
        int j = 0;
        foreach (Transform child in transform)
        {
            string name = child.gameObject.name;
            
            loadinfo.text = "loading info of: " + name;
            BinaryFormatter bf = new BinaryFormatter();
            string path1 = Application.persistentDataPath + "/" + name + ".txt";
            FileStream stream1 = new FileStream(path1, FileMode.Open);              
            loadinfo.text = "deserializing transform info";
            TransformInfo transforminfo = bf.Deserialize(stream1) as TransformInfo;
            stream1.Close();
            Vector3 position;
            position.x = transforminfo.posx;
            position.y = transforminfo.posy;
            position.z = transforminfo.posz;
            Quaternion rotation;
            rotation.x = transforminfo.rotx;
            rotation.y = transforminfo.roty;
            rotation.z = transforminfo.rotz;
            rotation.w = transforminfo.rotw;
            Vector3 localscale;
            localscale.x = transforminfo.scalex ;
            localscale.y = transforminfo.scaley;
            localscale.z = transforminfo.scalez ;
            loadinfo.text = "setting position of: "+name;
            child.gameObject.transform.position = position;
            loadinfo.text = "setting rotation of: " + name;
            child.gameObject.transform.rotation = rotation;
            loadinfo.text = "setting scale of: " + name;
            child.gameObject.transform.localScale = localscale;
            j = j + 1;
            //child.gameObject.transform.childCount > 0;
            loadinfo.text = "Done. Objects loaded: "+j;
            Debug.Log("loaded: " + name);

        }

        /*
        foreach (string file in files)
            {
                //loadinfo.text = "going through each file in directory";
                if (Path.GetExtension(file) == ".txt")
                {
                    
                    GameObject gameObject = Resources.Load(Path.GetFileNameWithoutExtension(file), typeof(GameObject)) as GameObject;
                    if(gameObject == null) { saveinfo.text = "EVEN GAMEOBJ NULL"; }
                    BinaryFormatter bf = new BinaryFormatter();
                    string path1 = Application.persistentDataPath + "/" + Path.GetFileNameWithoutExtension(file) + ".txt";
                    FileStream stream1 = new FileStream(path1, FileMode.Open);              
                    loadinfo.text = "deserializing transform info";
                    TransformInfo transforminfo = bf.Deserialize(stream1) as TransformInfo;
                    stream1.Close();
                        Vector3 position;
                        position.x = transforminfo.posx;
                        position.y = transforminfo.posy;
                        position.z = transforminfo.posz;
                        Quaternion rotation;
                        rotation.x = transforminfo.rotx;
                        rotation.y = transforminfo.roty;
                        rotation.z = transforminfo.rotz;
                        rotation.w = transforminfo.rotw;
                        Vector3 localscale;
                        localscale.x = transforminfo.scalex / 2.2f;
                        localscale.y = transforminfo.scaley / 2.2f;
                        localscale.z = transforminfo.scalez / 2.2f;
                        gameObject.transform.localScale = localscale;
                        loadinfo.text = "Instantiating saved object";
                        GameObject gameObject1 = Instantiate(gameObject, position, rotation);
                        loadinfo.text = "Done loading object";
                        Debug.Log(localscale + ":" + gameObject1.transform.localScale);
                        Debug.Log("Instantiated This:" + Path.GetFileNameWithoutExtension(file));
                    
                } 
            }*/
       

    }


}
