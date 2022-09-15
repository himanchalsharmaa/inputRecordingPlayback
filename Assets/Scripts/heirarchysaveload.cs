using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using TMPro;
using System;
using System.Globalization;
using System.IO.Pipes;
using System.Threading.Tasks;
using static UnityEngine.GraphicsBuffer;
using System.Xml;
using System.Linq;
using static GLTFast.Schema.AnimationChannel;
using System.Collections.Concurrent;
using System.Drawing;
using System.Collections.Specialized;
using UnityEditor;
//COMMAND+R is replace all occurences
public class heirarchysaveload : MonoBehaviour
{

    public GameObject nestedObject;
    public GameObject toChange;
    public GameObject toChangeParent;
    private Transform allParent;
    public TMP_Text nestedInfo;
    public TMP_Text loadinfo;
    private bool record = false;
    private float timer = 0.0f;
    public float timetocapture = 4.0f;
    private float elapsed = 0.0f;
    private BinaryWriter binarywriter;
    private BinaryReader binaryReader;
    private FileStream fileStream;
    private ConcurrentQueue<string> infostring;
    private List<Tuple<GameObject, bool, int,string>> objectstracked;
    private Dictionary<string,string> spawnedRuntime;
    public Dictionary<string, GameObject> aname=new Dictionary<string, GameObject>();
    private List<GameObject> spawnedTrack;
    private int  depth = 0;
    private int spawned = 0;
    private bool loaded = true;
    private bool callquits = false;
    private Task diskwrite;
    private Task<bool> diskread;

    private void Awake()
    { //To set FPS to 60
      
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    private async void LateUpdate()
    {
        if (record)
        {
            if (timer <= (timetocapture + 0.06f))
            {
                foreach (var item in objectstracked)
                {
                    GameObject obj = item.Item1;
                    string path = "" + obj.transform.GetSiblingIndex();
                    while (obj.transform.parent != allParent)
                    {
                        if (obj.transform.parent == null)
                        {
                            Debug.Log("Object outside " + allParent.name);
                            break;
                        }
                        depth += 1;
                        obj = obj.transform.parent.gameObject;
                        path = obj.transform.GetSiblingIndex() + "," + path;
                    }
                    depth += 1;
                    path = depth + "," + path;

                    string activity = ";";
                    if (item.Item3 >= 0)
                    {
                        if (item.Item1.activeSelf)
                        {
                            activity = activity + "1;";
                        }
                        else
                        {
                            activity = activity + "0;";
                        }

                        //TRS And Material Logic Begins

                        string posi = item.Item1.transform.position.x + "," + item.Item1.transform.position.y + "," + item.Item1.transform.position.z + ";";
                        string roti = item.Item1.transform.rotation.x + "," + item.Item1.transform.rotation.y + "," + item.Item1.transform.rotation.z + "," + item.Item1.transform.rotation.w + ";";
                        string scaly = item.Item1.transform.localScale.x + "," + item.Item1.transform.localScale.y + "," + item.Item1.transform.localScale.z + ";";
                        string matvalues = "";
                        if (item.Item2)
                        {
                            matvalues = matvalues + "1," + item.Item3 + ";";
                            if (item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].HasProperty("_Metallic"))
                            {
                                matvalues = matvalues + "1" + "," + item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].GetFloat("_Metallic") + ";";
                            }
                            else
                            {
                                matvalues = matvalues + "0;";
                            }
                            if (item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].HasProperty("_Glossiness"))
                            {
                                matvalues = matvalues + "1" + "," + item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].GetFloat("_Glossiness") + ";";
                            }
                            else if (item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].HasProperty("_Smoothness"))
                            {
                                matvalues = matvalues + "1" + "," + item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].GetFloat("_Smoothness") + ";";
                            }
                            else
                            {
                                matvalues = matvalues + "0;";
                            }
                            if (item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].HasProperty("_Color"))
                            {
                                UnityEngine.Color col = item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].GetColor("_Color");
                                matvalues = matvalues + "1," + col.r + "," + col.g + "," + col.b + "," + col.a + ";";
                            }
                            else
                            {
                                matvalues = matvalues + "0;";
                            }
                            string result = item.Item1.GetComponent<MeshRenderer>().materials[item.Item3].GetTag("RenderType", false, "N");
                            if (result != "N")
                            {
                                if (result == "Opaque")
                                {
                                    matvalues = matvalues + "1,1;";
                                }
                                if (result == "Transparent")
                                {
                                    matvalues = matvalues + "1,2;";
                                }
                            }
                            else
                            {
                                matvalues = matvalues + "0;";
                            }
                        }
                        else
                        {
                            matvalues = "0;";
                        }
                        string towrite = timer + activity +posi + roti + scaly + matvalues;
                        infostring.Enqueue(path);
                        infostring.Enqueue(towrite);
                        depth = 0;

                        // Logic Ends
                    }
                    else if (item.Item3 == -1)
                    {
                        activity = activity + "-1;";
                        depth = 0;
                        infostring.Enqueue(path);
                        infostring.Enqueue(timer+activity);
                    }
                    else if(item.Item3== -2)
                    {
                        activity = activity + "-2;";
                        depth = 0;
                        infostring.Enqueue(path);
                        infostring.Enqueue(timer+activity+item.Item4);
                    }

                }
                elapsed += Time.deltaTime;
                objectstracked.Clear();

                if (elapsed >= 1f)
                {
                    elapsed = elapsed % 1f;
                    nestedInfo.text = "" + timer;
                }
                if (infostring.Count >= 3000) // Couldn't see difference between 1k,2k,4k need HELP!
                {
                    if (diskwrite.IsCompleted)
                    {
                        diskwrite = Task.Factory.StartNew(() => writetodisk(infostring.Count));
                    }
                }
                timer += Time.deltaTime;
            }
            else
            {
                if (infostring.Count > 1)
                {
                    if (diskwrite.IsCompleted)
                    {
                        diskwrite = Task.Factory.StartNew(() => writetodisk(infostring.Count));
                        await diskwrite;
                    }
                    else
                    {
                        await diskwrite;
                        diskwrite = Task.Factory.StartNew(() => writetodisk(infostring.Count));
                        await diskwrite;
                    }

                }
                timer = 0;
                record = false;
                nestedInfo.text = "Done Recording";
                binarywriter.Close();
                fileStream.Close();
                objectstracked.Clear();
                spawnedRuntimeWrite();
                spawnedRuntime.Clear();
            }

        }

    }
    private void writetodisk(int runcount)
    {
        for (int i = 0; i < (runcount / 2); i++)
        {
            string a, b;
            infostring.TryDequeue(out a);
            string[] temp1 = a.Split(',');
            for (int j = 0; j < UInt16.Parse(temp1[0]) + 1; j++)
            {
                binarywriter.Write(UInt16.Parse(temp1[j]));
            }
            infostring.TryDequeue(out b);
            string[] temp2 = b.Split(';');
            binarywriter.Write(float.Parse(temp2[0]));
            if (temp2[1] == "-1")
            {
                binarywriter.Write(SByte.Parse(temp2[1]));
            }
            else if (temp2[1] == "-2")
            {
                binarywriter.Write(SByte.Parse(temp2[1]));
                string[] path2 = temp2[2].Split(',');
                for (int j = 0; j < UInt16.Parse(path2[0]) + 1; j++)
                {
                    binarywriter.Write(UInt16.Parse(path2[j]));
                }
            }
            else
            {
                binarywriter.Write(SByte.Parse(temp2[1]));
                string[] posarr = temp2[2].Split(',');
                binarywriter.Write(float.Parse(posarr[0]));
                binarywriter.Write(float.Parse(posarr[1]));
                binarywriter.Write(float.Parse(posarr[2]));
                string[] rotarr = temp2[3].Split(',');
                binarywriter.Write(float.Parse(rotarr[0]));
                binarywriter.Write(float.Parse(rotarr[1]));
                binarywriter.Write(float.Parse(rotarr[2]));
                binarywriter.Write(float.Parse(rotarr[3]));
                string[] scalearr = temp2[4].Split(',');
                binarywriter.Write(float.Parse(scalearr[0]));
                binarywriter.Write(float.Parse(scalearr[1]));
                binarywriter.Write(float.Parse(scalearr[2]));
                string[] matnum = temp2[5].Split(',');
                if (matnum[0] == "1")
                {
                    binarywriter.Write(true);
                    binarywriter.Write(SByte.Parse(matnum[1]));

                    string[] metal = temp2[6].Split(',');
                    if (metal[0] == "1")
                    {
                        binarywriter.Write(true);
                        binarywriter.Write(float.Parse(metal[1]));
                    }
                    else
                    {
                        binarywriter.Write(false);
                    }
                    string[] glos = temp2[7].Split(',');
                    if (glos[0] == "1")
                    {
                        binarywriter.Write(true);
                        binarywriter.Write(float.Parse(glos[1]));
                    }
                    else
                    {
                        binarywriter.Write(false);
                    }
                    string[] col = temp2[8].Split(',');
                    if (col[0] == "1")
                    {
                        binarywriter.Write(true);
                        binarywriter.Write(float.Parse(col[1]));
                        binarywriter.Write(float.Parse(col[2]));
                        binarywriter.Write(float.Parse(col[3]));
                        binarywriter.Write(float.Parse(col[4]));
                    }
                    else
                    {
                        binarywriter.Write(false);
                    }
                    string[] rend = temp2[9].Split(',');
                    if (rend[0] == "1")
                    {
                        binarywriter.Write(true);
                        binarywriter.Write(Convert.ToSByte(rend[1]));
                    }
                    else
                    {
                        binarywriter.Write(false);
                    }

                }
                else
                {
                    binarywriter.Write(false);
                }
            }
            
        }
    }
    IEnumerator playroutine(ConcurrentQueue<string> loadstring)
    {
        loadinfo.text = "STARTING LOAD";
        while (true)
        {
            if (timer <= timetocapture)
            {
                if (callquits)
                {
                    timer = 0;
                    binaryReader.Close();
                    fileStream.Close();
                    loadinfo.text = "done playing with exception";
                    callquits = false;
                    break;
                }
                elapsed += Time.deltaTime;

                if (elapsed >= 1f)
                {
                    elapsed = elapsed % 1f;
                    loadinfo.text = "" + timer;
                }
                loadingobj(binaryReader, aname, loadstring);
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
                aname.Clear();
                binaryReader.Close();
                fileStream.Close();
                callquits = false;
                loadinfo.text = "done playing";
                break;
            }
            yield return null;
        }
    }
    private void spawnedRuntimeWrite()
    {
        string snaploc = Application.persistentDataPath + "/runtimespawn.txt";
        fileStream = File.Open(snaploc, FileMode.Create);
        BinaryWriter binaryWriter = new BinaryWriter(fileStream);
        string[] keys = new string[spawnedRuntime.Count];
        string[] values = new string[spawnedRuntime.Count];
        spawnedRuntime.Keys.CopyTo(keys, 0);
        spawnedRuntime.Values.CopyTo(values, 0);
        for (int i = 0; i < spawnedRuntime.Count; i++)
        {
            binaryWriter.Write(keys[i] + ";" + values[i]);
            Debug.Log(keys[i]+";"+ values[i]);
        }
        binaryWriter.Close();
        fileStream.Close();
    }
    public void storingobj()
    {
        string snaploc = Application.persistentDataPath + "/snapshot.bin";
        fileStream = File.Open(snaploc, FileMode.Create);
        binarywriter = new BinaryWriter(fileStream);
        elapsed = 1f;
        objectstracked = new List<Tuple<GameObject, bool, int,string>>();
        infostring = new ConcurrentQueue<string>();
        spawnedRuntime = new Dictionary<string,string>();
        allParent = nestedObject.transform.parent;
        countObjectstracked(nestedObject, "/", "", objectstracked, binarywriter, nestedObject); // To Attach my script recursively to each child and send objectstrackd to each
        binarywriter.Close();
        fileStream.Close();
        if (File.Exists(Application.persistentDataPath + "/storeloc.bin"))
        {
            nestedInfo.text = "File exists: overwritten";
        }
        string filename = Application.persistentDataPath + "/storeloc.bin";
        fileStream = File.Open(filename, FileMode.Create);
        binarywriter = new BinaryWriter(fileStream);
        diskwrite = Task.Factory.StartNew(() => writetodisk(infostring.Count));
        record = true;
    }

    public async void callload()
    {
        GameObject tempo = Instantiate(toChange);
        tempo.transform.parent = toChangeParent.transform;
        tempo.SetActive(true);
        if (File.Exists(Application.persistentDataPath + "/storeloc.bin"))
        {
            if (File.Exists(Application.persistentDataPath + "/snapshot.bin"))
            {
                string snaploc = Application.persistentDataPath + "/snapshot.bin";
                fileStream = File.Open(snaploc, FileMode.Open);
                BinaryReader binaryreader = new BinaryReader(fileStream);
                aname = new Dictionary<string, GameObject>();
                allParent = nestedObject.transform.parent;
                snapReader(binaryreader);
                binaryreader.Close();
                fileStream.Close();
            }
            string filename = Application.persistentDataPath + "/storeloc.bin";
            fileStream = File.Open(filename, FileMode.Open);
            binaryReader = new BinaryReader(fileStream);
            ConcurrentQueue<string> loadstring = new ConcurrentQueue<string>();
            spawnedRuntime = new Dictionary<string, string>();
            try
            {
                diskread = Task.Factory.StartNew(() => loadfunction(binaryReader, loadstring));
                await diskread;
                loaded = diskread.Result;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            if (loaded)
            {
                StartCoroutine(playroutine(loadstring));
            }
        }
        else
        {
            loadinfo.text = "No load exists";
        }
    }
    public void loadingobj(BinaryReader reader, Dictionary<string, GameObject> aname, ConcurrentQueue<string> loadstring)
    {
        float timothy1 = 0, timothy2;
        bool oncy = true;
        while (true)
        {
            if (loadstring.Count < 500) //Leaves 4-5 seconds for thread to complete
            {
                if (diskread.IsCompleted)
                {
                    loaded = diskread.Result;
                }

                if (loaded)
                { 
                diskread = Task.Factory.StartNew(() => loadfunction(binaryReader, loadstring));
                loaded = false;
                }
            }
            if (!loaded)
            {
                if (loadstring.Count == 0)
                {
                    break;
                }
            }
            string line, toread;
            loadstring.TryDequeue(out line);
            GameObject go;
            Transform t;
            string[] posi = line.Split(',');
            t = allParent.GetChild(Int16.Parse(posi[1]));
            if (aname.TryGetValue(line, out go))
            {
                // Nothing
            }
            else
            {
                for (int x = 2; x < posi.Count(); x++)
                {
                    if (Int16.Parse(posi[x]) < t.childCount)
                    {
                        t = t.GetChild(Int16.Parse(posi[x]));
                    }
                    else
                    {
                        Debug.Log("Can't find gameObject");
                        //GameObject temp=Instantiate(go);
                        //spawned += 1;
                        //temp.transform.parent = t;
                        //t = t.GetChild(Int16.Parse(posi[x]));
                    }
                }
                go = t.gameObject;
                aname.Add(line, go);
            }
            loadstring.TryDequeue(out toread);
            string[] transforms = toread.Split(';');
            if (oncy)
            {
                timothy1 = float.Parse(transforms[0], CultureInfo.InvariantCulture.NumberFormat);
                oncy = false;
            }
            else
            {
                timothy2 = float.Parse(transforms[0], CultureInfo.InvariantCulture.NumberFormat);
                if (timothy2 != timothy1)
                {
                    break;
                }
            }
            int activity = Int16.Parse(transforms[1]);
            if (activity >= 0)
            {
                if (activity == 1 && !go.activeSelf)
                {
                    go.SetActive(true);
                }
                else if(activity==0 && go.activeSelf)
                {
                    go.SetActive(false);
                }
                string[] posarr = transforms[2].Split(',');
                string[] rotarr = transforms[3].Split(',');
                string[] scalearr = transforms[4].Split(',');
                Vector3 pos = new Vector3(float.Parse(posarr[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(posarr[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(posarr[2], CultureInfo.InvariantCulture.NumberFormat));
                Quaternion rot = new Quaternion(float.Parse(rotarr[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(rotarr[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(rotarr[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(rotarr[3], CultureInfo.InvariantCulture.NumberFormat));
                Vector3 scale = new Vector3(float.Parse(scalearr[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(scalearr[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(scalearr[2], CultureInfo.InvariantCulture.NumberFormat));
                go.transform.position = pos;
                go.transform.rotation = rot;
                go.transform.localScale = scale;
                string[] matnum = transforms[5].Split(',');
                Material gomat;
                if (matnum[0] == "1")
                {
                    gomat = go.GetComponent<MeshRenderer>().materials[Byte.Parse(matnum[1])];
                    string[] metal = transforms[6].Split(',');
                    if (metal[0] == "1")
                    {
                        gomat.SetFloat("_Metallic", float.Parse(metal[1], CultureInfo.InvariantCulture.NumberFormat));
                    }
                    string[] gloss = transforms[7].Split(',');
                    if (gloss[0] == "1")
                    {
                        if (gomat.HasProperty("_Glossiness"))
                        {
                            gomat.SetFloat("_Glossiness", float.Parse(gloss[1], CultureInfo.InvariantCulture.NumberFormat));
                        }
                        else
                        {
                            gomat.SetFloat("_Smoothness", float.Parse(gloss[1], CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }
                    string[] col = transforms[8].Split(',');
                    if (col[0] == "1")
                    {
                        UnityEngine.Color color = new UnityEngine.Color(float.Parse(col[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(col[2], CultureInfo.InvariantCulture.NumberFormat)
                            , float.Parse(col[3], CultureInfo.InvariantCulture.NumberFormat), float.Parse(col[4], CultureInfo.InvariantCulture.NumberFormat));
                        gomat.SetColor("_Color", color);
                    }
                    string[] rend = transforms[9].Split(',');
                    if (rend[0] == "1")
                    {
                        if (rend[0] == "1")
                        {
                            gomat.SetOverrideTag("RenderType", "Opaque");
                        }
                        else
                        {
                            gomat.SetOverrideTag("RenderType", "Transparent");
                        }
                    }
                }
            }
            else if(activity == -1)
            {
                Destroy(go);
            }
            else if(activity == -2)
            {
                string[] path2 = transforms[2].Split(',');
            }
             
        }
    }
    
    private bool loadfunction(BinaryReader binaryReader, ConcurrentQueue<string> loadstring)
    {
        bool once = true;
        bool twice = false;
        for (int i = 0; i < 2000; i++)
        {
            if (once)
            {
                if (binaryReader.PeekChar() != -1)
                {
                    UInt16 deep = binaryReader.ReadUInt16();
                    string liny = "" + deep;
                    for (int j = 0; j < deep; j++)
                    {
                        liny = liny + "," + binaryReader.ReadUInt16();
                    }
                    loadstring.Enqueue(liny);
                    twice = false;
                    once = false;
                }
                else
                {
                    //PeekChar is supposedly unreliable but try catch blocks are extremely slow when done 1000s of times
                    return false;
                }
            }
            if (twice)
            {
                string timer = binaryReader.ReadSingle() + ";";
                SByte activity = binaryReader.ReadSByte();
                if (activity >= 0)
                {
                    string acti = activity + ";";
                    string posi = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                    string roti = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                    string scali = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                    string matvalues = "";
                    if (binaryReader.ReadBoolean())
                    {
                        matvalues = matvalues + "1," + binaryReader.ReadSByte() + ";";

                        if (binaryReader.ReadBoolean())
                        {
                            matvalues = matvalues + "1," + binaryReader.ReadSingle() + ";";
                        }
                        else
                        {
                            matvalues = matvalues + "0;";
                        }
                        if (binaryReader.ReadBoolean())
                        {
                            matvalues = matvalues + "1," + binaryReader.ReadSingle() + ";";
                        }
                        else
                        {
                            matvalues = matvalues + "0;";
                        }
                        if (binaryReader.ReadBoolean())
                        {
                            matvalues = matvalues + "1," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                        }
                        else
                        {
                            matvalues = matvalues + "0;";
                        }
                        if (binaryReader.ReadBoolean())
                        {
                            SByte type = binaryReader.ReadSByte();
                            if (type == 1)
                            {
                                matvalues = matvalues + "1,1;";
                            }
                            else if (type == 2)
                            {
                                matvalues = matvalues + "1,2;";
                            }
                        }
                        else
                        {
                            matvalues = matvalues + "0;";
                        }

                    }
                    else
                    {
                        matvalues = matvalues + "0;";
                    }

                    string liny = timer + acti + posi + roti + scali + matvalues;
                    loadstring.Enqueue(liny);
                }
                else if(activity == -1)
                {
                    string acti = activity + ";";
                    string liny = timer + acti;
                    loadstring.Enqueue(liny);
                }
                else if(activity == -2)
                {
                    UInt16 deep = binaryReader.ReadUInt16();
                    string path2 = "" + deep;
                    for (int j = 0; j < deep; j++)
                    {
                        path2 = path2 + "," + binaryReader.ReadUInt16();
                    }
                    loadstring.Enqueue(path2);
                }
                
                twice = false;
                once = true;
            }
            if (!once)
            {
                twice = true;
            }
        }
        return true;
    }
    private void countObjectstracked(GameObject gameObject, string indent, string parentName, List<Tuple<GameObject, bool, int,string>> objectstracked, BinaryWriter binarywriter, GameObject allParent)
    {
        if (gameObject.transform.childCount > 0)
        {
            if (parentName != "")
            {
                if (!parentName.EndsWith("/"))
                    parentName = parentName + "/";
            }
            parentName = parentName + gameObject.name;
        }
        else
        {
            parentName = parentName + gameObject.name;
        }

        if (parentName.EndsWith("/"))
        {
            transformchangedcomp tfc = gameObject.AddComponent<transformchangedcomp>();
            tfc.dicri = objectstracked;
            snapWriter(gameObject, gameObject, binarywriter, allParent);
        }
        else
        {
            transformchangedcomp tfc = gameObject.AddComponent<transformchangedcomp>();
            tfc.dicri = objectstracked;
            snapWriter(gameObject, gameObject, binarywriter, allParent);
        }

        foreach (Transform child in gameObject.transform)
        {
            if (!parentName.EndsWith("/"))
                parentName = parentName + "/";
            countObjectstracked(child.gameObject, indent, parentName, objectstracked, binarywriter, allParent);
        }
    }
    private void snapWriter(GameObject obj, GameObject obj1, BinaryWriter binarywriter, GameObject allparent)
    {
        string path = "" + obj1.transform.GetSiblingIndex();
        int depth = 1;
        while (obj1 != allparent) //obj.transform.parent != allparent && 
        {
            depth += 1;
            obj1 = obj1.transform.parent.gameObject;
            path = obj1.transform.GetSiblingIndex() + "," + path;
        }
        path = depth + "," + path;
        string[] temp1 = path.Split(',');
        for (int j = 0; j < UInt16.Parse(temp1[0]) + 1; j++)
        {
            binarywriter.Write(UInt16.Parse(temp1[j]));
        }
        if (obj.GetComponent<MeshRenderer>())
        {
            Material[] mata = obj.GetComponent<MeshRenderer>().materials;
            binarywriter.Write(true);
            binarywriter.Write(Convert.ToByte(mata.Length));
            for (int i = 0; i < mata.Length; i++) 
            {
                if (mata[i].HasProperty("_Metallic"))
                {
                    binarywriter.Write(true);
                    binarywriter.Write(mata[i].GetFloat("_Metallic"));
                }
                else
                {
                    binarywriter.Write(false);
                }
                if (mata[i].HasProperty("_Glossiness"))
                {
                    binarywriter.Write(true);
                    binarywriter.Write(mata[i].GetFloat("_Glossiness"));
                }
                else if (mata[i].HasProperty("_Smoothness"))
                {
                    binarywriter.Write(true);
                    binarywriter.Write(mata[i].GetFloat("_Smoothness"));
                }
                else
                {
                    binarywriter.Write(false);
                }
                if (mata[i].HasProperty("_Color"))
                {
                    UnityEngine.Color coli = mata[i].GetColor("_Color");
                    binarywriter.Write(true);
                    binarywriter.Write(coli.r);
                    binarywriter.Write(coli.g);
                    binarywriter.Write(coli.b);
                    binarywriter.Write(coli.a);
                }
                else
                {
                    binarywriter.Write(false);
                }
                string result = mata[i].GetTag("RenderType", false, "N");
                if (result != "N")
                {
                    if (result == "Opaque")
                    {
                        binarywriter.Write(true);
                        binarywriter.Write(Convert.ToSByte(1));
                    }
                    if (result == "Transparent")
                    {
                        binarywriter.Write(true);
                        binarywriter.Write(Convert.ToSByte(2));
                    }
                }
                else
                {
                    binarywriter.Write(false);
                }
            }
        }
        else
        {
            binarywriter.Write(false);
        }
    }
    private void snapReader(BinaryReader binaryreader)
    {
        while (binaryreader.BaseStream.Position != binaryreader.BaseStream.Length)
        {
            GameObject go;
            UInt16 deep = binaryreader.ReadUInt16();
            string liny = "" + deep;
            for (int j = 0; j < deep; j++)
            {
                liny = liny + "," + binaryreader.ReadUInt16();
            }
            string[] posi = liny.Split(',');
            Transform t = allParent.GetChild(Int16.Parse(posi[1]));
            if (aname.TryGetValue(liny, out go))
            {
                // Nothing
            }
            else
            {
                for (int x = 2; x < posi.Count(); x++)
                {
                    t = t.GetChild(Int16.Parse(posi[x]));
                }
                go = t.gameObject;
                aname.Add(liny, go);
            }
            if (binaryreader.ReadBoolean())
            {
                Byte matcount = binaryreader.ReadByte();
                for (Byte i=0;i<matcount;i++)
                {
                    Material gomat = go.GetComponent<MeshRenderer>().materials[i];
                    if (binaryreader.ReadBoolean())
                    {
                        gomat.SetFloat("_Metallic", binaryreader.ReadSingle());
                    }
                    if (binaryreader.ReadBoolean())
                    {
                        if (gomat.HasProperty("_Glossiness"))
                        {
                            gomat.SetFloat("_Glossiness", binaryreader.ReadSingle());
                        }
                        else
                        {
                            gomat.SetFloat("_Smoothness", binaryreader.ReadSingle());
                        }
                    }
                    if (binaryreader.ReadBoolean())
                    {
                        UnityEngine.Color color = new UnityEngine.Color(binaryreader.ReadSingle(), binaryreader.ReadSingle(), binaryreader.ReadSingle(), binaryreader.ReadSingle());
                        gomat.SetColor("_Color", color);
                    }
                    if (binaryreader.ReadBoolean())
                    {
                        SByte type = binaryreader.ReadSByte();
                        if (type == 1)
                        {
                            gomat.SetOverrideTag("RenderType", "Opaque");
                        }
                        else
                        {
                            gomat.SetOverrideTag("RenderType", "Transparent");
                        }
                    }
                }
                
            }
        }
    }
    public GameObject instantiateRecorded(GameObject go,Vector3 pos,Quaternion rot, Dictionary<string, GameObject> indextoGameObjects)
    {
        if (!spawnedRuntime.ContainsKey(go.name)) {
            GameObject insta = Instantiate(go, pos, rot);
            insta.transform.parent = nestedObject.transform;
            transformchangedcomp tfc = insta.AddComponent<transformchangedcomp>();
            tfc.dicri = objectstracked;
            spawnedRuntime.Add(go.name,"" +insta.transform.GetSiblingIndex());
            indextoGameObjects.Add("" + insta.transform.GetSiblingIndex(),go);
            return insta;
        }
        else
        {
            return null;
        }
    }
    public bool changeParentTransform(GameObject toChange, GameObject toChangeTo, Dictionary<string, GameObject> indextoGameObjects)
    {
        Transform trans1 = toChange.transform;
        string path1 = "";
        int dep = 0;
        while (trans1.parent.transform != nestedObject.transform.parent.transform)
        {
            if (trans1.parent == null)
            {
                Debug.Log("901: Object outside the tracked parent: " + nestedObject.name);
                break;
            }
            dep += 1;
            path1 = trans1.GetSiblingIndex() + "," + path1;
            trans1 = trans1.parent.gameObject.transform;
        }
        path1 = dep + "," + path1;
        GameObject go;
        Transform trans2 = toChangeTo.transform;
        string path2 = "";
        dep = 1;
        while (trans2.parent.transform != nestedObject.transform.parent.transform)
        {
            if (trans2.parent == null)
            {
                Debug.Log("922: Object outside " + nestedObject.name);
                break;
            }
            dep += 1;
            path2 = trans2.GetSiblingIndex() + "," + path2;
            trans2 = trans2.parent.gameObject.transform;
        }
        path2 = dep + "," + path2;
        if(trans1.parent != null && trans2.parent != null)
        {
            if(indextoGameObjects.TryGetValue(path1,out go))
            {
                path2 = path2 + "," + toChangeTo.transform.childCount;
                indextoGameObjects.Add(path2, go);
                indextoGameObjects.Remove(path1);
                Tuple<GameObject, bool, int, string> temp = new Tuple<GameObject, bool, int, string>(toChange, false, -2, path2);
                objectstracked.Add(temp);
                toChange.transform.parent = toChangeTo.transform;
                Debug.Log("Parent changed and tracking..");
            }
            else
            {
                Debug.Log("963: Not found Object by key Path1");
            }
        }
        else if (trans2.parent == null)
        {
            Debug.Log("Changing the transform but no longer tracking after changing");
            toChange.transform.parent = toChangeTo.transform;
            if (indextoGameObjects.ContainsKey(path1))
            {
                indextoGameObjects.Remove(path1);
            }
        }
        if (trans2.parent != null && trans1.parent == null)
        {
            path2 = path2 + "," + toChangeTo.transform.childCount;
            Tuple<GameObject, bool, int, string> temp = new Tuple<GameObject, bool, int, string>(toChange, false, -2, path2);
            objectstracked.Add(temp);
            toChange.transform.parent = toChangeTo.transform;
            indextoGameObjects.Add(path2, toChange);
            Debug.Log("Parent changed and tracking..");
        }
        return true;
    }
    public bool destroyRecorded(GameObject go)
    {
        Tuple<GameObject, bool, int, string> temp = new Tuple<GameObject, bool, int, string>(go, false, -1,"");
        objectstracked.Add(temp);
        Destroy(go);
        return true;
    }
}
//private void DumpGameObject(GameObject gameObject, StreamWriter writer, string indent, string parentName,List<string> infostring,float timer)
//{
//    if (gameObject.transform.childCount > 0)
//    {
//        if (parentName != "")
//        {
//            if (!parentName.EndsWith("/"))
//                parentName = parentName + "/";
//        }
//        parentName = parentName + gameObject.name;
//    }
//    else
//    {
//        parentName = parentName + gameObject.name;
//    }

//    if (parentName.EndsWith("/"))
//    {
//        string posi = ";"+gameObject.transform.position.x + "," + gameObject.transform.position.y + "," + gameObject.transform.position.z + ";";
//        string roti = gameObject.transform.rotation.x + "," + gameObject.transform.rotation.y + "," + gameObject.transform.rotation.z + "," + gameObject.transform.rotation.w + ";";
//        string scaly = gameObject.transform.localScale.x + "," + gameObject.transform.localScale.y + "," + gameObject.transform.localScale.z ;
//        string towrite =timer + posi + roti + scaly+timer;
//        infostring.Add(parentName + gameObject.name);
//        infostring.Add(towrite);
//    }
//    else
//    {
//        infostring.Add(parentName);
//        string posi = ";" + gameObject.transform.position.x + "," + gameObject.transform.position.y + "," + gameObject.transform.position.z + ";";
//        string roti = gameObject.transform.rotation.x + "," + gameObject.transform.rotation.y + "," + gameObject.transform.rotation.z + "," + gameObject.transform.rotation.w + ";";
//        string scaly = gameObject.transform.localScale.x + "," + gameObject.transform.localScale.y + "," + gameObject.transform.localScale.z ;
//        string towrite =timer + posi + roti + scaly;
//        infostring.Add(towrite);
//    }

//    foreach (Transform child in gameObject.transform)
//    {
//        if (!parentName.EndsWith("/"))
//            parentName = parentName + "/";
//            DumpGameObject(child.gameObject, writer, indent, parentName, infostring,timer);
//    }
//}