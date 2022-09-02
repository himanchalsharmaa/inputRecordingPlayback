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
//COMMAND+R is replace all occurences
public class heirarchysaveload : MonoBehaviour
{
    public GameObject nestedObject;
    private Transform allParent;
    public TMP_Text nestedInfo;
    public TMP_Text loadinfo;
    private bool record = false;
    private float timer = 0.0f;
    public float timetocapture = 4.0f;
    private float elapsed = 0.0f;
    private bool playrecord = false;
    private BinaryWriter binarywriter;
    private BinaryReader binaryReader;
    private FileStream fileStream;
    private ConcurrentQueue<string> infostring ;
    private List<GameObject> objectstracked = new List<GameObject>();
    private string[] supportedsaves;
    private Dictionary<string, GameObject> aname;
    private int r,depth=0,j=0;
    private bool loaded = true;
    private bool callquits = false;
    private bool isrunning = false;
    private Task diskwrite;
    private Task<bool> diskread;

    private void Awake()
    { //To set FPS to 60
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    private void Start()
    {
        supportedsaves =new string[3] { "_Metallic", "_Glossiness","_Color" };
    }
    private void Update()
    {

    }
    private async void LateUpdate()
    {
        if (record)
        {
            if (timer <= (timetocapture + 0.05f))
            {
                foreach (var item in objectstracked)
                {
                    string posi = ";" + item.transform.position.x + "," + item.transform.position.y + "," + item.transform.position.z + ";";
                    string roti = item.transform.rotation.x + "," + item.transform.rotation.y + "," + item.transform.rotation.z + "," + item.transform.rotation.w + ";";
                    string scaly = item.transform.localScale.x + "," + item.transform.localScale.y + "," + item.transform.localScale.z;
                    string towrite = timer + posi + roti + scaly;
                    GameObject obj = item;
                    string path = "" + obj.transform.GetSiblingIndex();
                    while (obj.transform.parent != allParent)
                    {
                        depth += 1;
                        obj = obj.transform.parent.gameObject;
                        path = obj.transform.GetSiblingIndex() + "," + path;
                    }
                    depth += 1;
                    path = depth + "," + path;
                    infostring.Enqueue(path);
                    infostring.Enqueue(towrite);
                    depth = 0;
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
                  //  Debug.Log(infostring.Count);
                 //   Debug.Log(diskwrite.IsCompletedSuccessfully); // Editor was misbehaving and entering this twice for some reason
                    if(diskwrite.IsCompleted)
                    {
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
            }

        }

    }
    
    // GUID in meta files for texture change
    private void writetodisk(int runcount) {
        for (int i = 0; i < (runcount/2); i++)
        {
            string a,b;
            infostring.TryPeek(out a);
            string[] temp1 = a.Split(',');
            for (int j = 0; j < UInt16.Parse(temp1[0]) + 1; j++)
            {
                binarywriter.Write(UInt16.Parse(temp1[j]));
            }
            infostring.TryDequeue(out a);
            infostring.TryPeek(out b);
            string[] temp2 = b.Split(';');
            infostring.TryDequeue(out b);                               //It's either this or writting every1000 lines from a 9 times storing list
            binarywriter.Write(float.Parse(temp2[0]));
            string[] posarr = temp2[1].Split(',');
            binarywriter.Write(float.Parse(posarr[0]));
            binarywriter.Write(float.Parse(posarr[1]));
            binarywriter.Write(float.Parse(posarr[2]));
            string[] rotarr = temp2[2].Split(',');
            binarywriter.Write(float.Parse(rotarr[0]));
            binarywriter.Write(float.Parse(rotarr[1]));
            binarywriter.Write(float.Parse(rotarr[2]));
            binarywriter.Write(float.Parse(rotarr[3]));
            string[] scalearr = temp2[3].Split(',');
            binarywriter.Write(float.Parse(scalearr[0]));
            binarywriter.Write(float.Parse(scalearr[1]));
            binarywriter.Write(float.Parse(scalearr[2]));
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
    public async void storingobj()
    {
        if (File.Exists(Application.persistentDataPath + "/storeloc.bin"))
        {
            nestedInfo.text = "File exists: overwritten";
        }

        // writer = new StreamWriter(Application.persistentDataPath + "/storeloc.txt", false);
        string filename = Application.persistentDataPath + "/storeloc.bin";
        fileStream = File.Open(filename, FileMode.Create);
        binarywriter = new BinaryWriter(fileStream);
        elapsed = 1f;
        objectstracked = new List<GameObject>();
        infostring = new ConcurrentQueue<string>();
        countObjectstracked(nestedObject, "/", "", objectstracked); // To Attach my script recursively to each child and send objectstrackd to each
        allParent = nestedObject.transform.parent;
        diskwrite = Task.Factory.StartNew(() => writetodisk(infostring.Count));
        await diskwrite;
        record = true;
    }

    public async void callload()
    {

        if (File.Exists(Application.persistentDataPath + "/storeloc.bin"))
        {
            string filename = Application.persistentDataPath + "/storeloc.bin";
            fileStream = File.Open(filename, FileMode.Open);
            binaryReader = new BinaryReader(fileStream);
            ConcurrentQueue<string> loadstring = new ConcurrentQueue<string>();
            aname = new Dictionary<string, GameObject>();
            allParent = nestedObject.transform.parent;
            try
            {
                diskread = Task.Factory.StartNew(() => loadfunction(binaryReader, loadstring));
                await diskread;
                loaded = diskread.Result;
            }
            catch(Exception e)
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
    public void loadingobj(BinaryReader reader,Dictionary<string,GameObject> aname, ConcurrentQueue<string> loadstring)
    {
        float timothy1=0, timothy2;
        bool oncy=true;
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
                    try
                    {
                        diskread = Task.Factory.StartNew(() => loadfunction(binaryReader, loadstring));
                        loaded = false;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
            }
            if (!loaded)
            {
                    if (loadstring.Count==0)
                    {
                    break;
                    }
            }
            string line, toread;
            loadstring.TryPeek(out line);
            GameObject go;
            Transform t;
            string[] posi = line.Split(',');
            t=allParent.GetChild(Int16.Parse(posi[1]));
            if (aname.TryGetValue(line,out go))
                {
                    // Nothing
                }
            else
            {
                for(int x = 2; x < posi.Count(); x++)
                {
                    t=t.GetChild(Int16.Parse(posi[x]));
                }
                go = t.gameObject;
                aname.Add(line,go);
            }
            loadstring.TryDequeue(out line);
            loadstring.TryPeek(out toread);
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
            string[] posarr = transforms[1].Split(',');
            string[] rotarr = transforms[2].Split(',');
            string[] scalearr = transforms[3].Split(',');
            Vector3 pos = new Vector3(float.Parse(posarr[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(posarr[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(posarr[2], CultureInfo.InvariantCulture.NumberFormat));
            Quaternion rot = new Quaternion(float.Parse(rotarr[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(rotarr[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(rotarr[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(rotarr[3], CultureInfo.InvariantCulture.NumberFormat));
            Vector3 scale = new Vector3(float.Parse(scalearr[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(scalearr[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(scalearr[2], CultureInfo.InvariantCulture.NumberFormat));
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.transform.localScale = scale;
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
                if (binaryReader.PeekChar()!=-1)
                {
                    UInt16 deep = binaryReader.ReadUInt16();
                    string liny = "" + deep;
                    for (int j = 0; j < deep; j++)
                    {
                        liny = liny + "," + binaryReader.ReadUInt16();
                    }
                    if (liny != null)
                    {
                        loadstring.Enqueue(liny);
                    }
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
                string posi = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                string roti = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                string scali = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle();
                string liny = timer + posi + roti + scali;

                if (liny != null)
                {
                    loadstring.Enqueue(liny);
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
    private void countObjectstracked(GameObject gameObject,string indent,string parentName,List<GameObject> dicri)
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
            transformchangedcomp tfc= gameObject.AddComponent<transformchangedcomp>();
            tfc.dicri =objectstracked;
            tfc.supportedsaves = supportedsaves;
        }
        else
        {
            transformchangedcomp tfc = gameObject.AddComponent<transformchangedcomp>();
            tfc.dicri = objectstracked;
            tfc.supportedsaves = supportedsaves;
        }

        foreach (Transform child in gameObject.transform)
        {
            if (!parentName.EndsWith("/"))
                parentName = parentName + "/";
            countObjectstracked(child.gameObject, indent, parentName, dicri);
        }
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