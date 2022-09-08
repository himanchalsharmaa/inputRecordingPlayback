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
    private BinaryWriter binarywriter;
    private BinaryReader binaryReader;
    private FileStream fileStream;
    private ConcurrentQueue<string> infostring;
    private List<Tuple<GameObject, bool>> objectstracked;
    private Dictionary<string, GameObject> aname;
    private int  depth = 0;
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
            if (timer <= (timetocapture + 0.05f))
            {
                foreach (var item in objectstracked)
                {
                    string posi = ";" + item.Item1.transform.position.x + "," + item.Item1.transform.position.y + "," + item.Item1.transform.position.z + ";";
                    string roti = item.Item1.transform.rotation.x + "," + item.Item1.transform.rotation.y + "," + item.Item1.transform.rotation.z + "," + item.Item1.transform.rotation.w + ";";
                    string scaly = item.Item1.transform.localScale.x + "," + item.Item1.transform.localScale.y + "," + item.Item1.transform.localScale.z + ";";
                    string matvalues = "";
                    if (item.Item2)
                    {
                        if (item.Item1.GetComponent<MeshRenderer>().material.HasProperty("_Metallic"))
                        {
                            matvalues = matvalues + "1" + "," + item.Item1.GetComponent<MeshRenderer>().material.GetFloat("_Metallic") + ";";
                        }
                        else
                        {
                            matvalues = matvalues + "0;";
                        }
                        if (item.Item1.GetComponent<MeshRenderer>().material.HasProperty("_Glossiness"))
                        {
                            matvalues = matvalues + "1" + "," + item.Item1.GetComponent<MeshRenderer>().material.GetFloat("_Glossiness") + ";";
                        }
                        else
                        {
                            matvalues = matvalues + "0;";
                        }
                        if (item.Item1.GetComponent<MeshRenderer>().material.HasProperty("_Color"))
                        {
                            UnityEngine.Color col = item.Item1.GetComponent<MeshRenderer>().material.GetColor("_Color");
                            matvalues = matvalues + "1," + col.r + "," + col.g + "," + col.b + "," + col.a + ";";
                        }
                        else
                        {
                            matvalues = matvalues + "0;";
                        }
                        string result = item.Item1.GetComponent<MeshRenderer>().material.GetTag("RenderType", false, "N");
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
                        matvalues = "0;0;0;0;";
                    }
                    string towrite = timer + posi + roti + scaly + matvalues;
                    GameObject obj = item.Item1;
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
            string[] metal = temp2[4].Split(',');
            if (metal[0] == "1")
            {
                binarywriter.Write(true);
                binarywriter.Write(float.Parse(metal[1]));
            }
            else
            {
                binarywriter.Write(false);
            }
            string[] glos = temp2[5].Split(',');
            if (glos[0] == "1")
            {
                binarywriter.Write(true);
                binarywriter.Write(float.Parse(glos[1]));
            }
            else
            {
                binarywriter.Write(false);
            }
            string[] col = temp2[6].Split(',');
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
            string[] rend = temp2[7].Split(',');
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
    public void storingobj()
    {
        string snaploc = Application.persistentDataPath + "/snapshot.bin";
        fileStream = File.Open(snaploc, FileMode.Create);
        binarywriter = new BinaryWriter(fileStream);
        elapsed = 1f;
        objectstracked = new List<Tuple<GameObject, bool>>();
        infostring = new ConcurrentQueue<string>();
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
                    t = t.GetChild(Int16.Parse(posi[x]));
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
            string[] metal = transforms[4].Split(',');
            if (metal[0] == "1")
            {
                go.GetComponent<MeshRenderer>().material.SetFloat("_Metallic", float.Parse(metal[1], CultureInfo.InvariantCulture.NumberFormat));
            }
            string[] gloss = transforms[5].Split(',');
            if (gloss[0] == "1")
            {
                go.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", float.Parse(gloss[1], CultureInfo.InvariantCulture.NumberFormat));
            }
            string[] col = transforms[6].Split(',');
            if (col[0] == "1")
            {
                UnityEngine.Color color = new UnityEngine.Color(float.Parse(col[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(col[2], CultureInfo.InvariantCulture.NumberFormat)
                    , float.Parse(col[3], CultureInfo.InvariantCulture.NumberFormat), float.Parse(col[4], CultureInfo.InvariantCulture.NumberFormat));
                go.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            }
            string[] rend = transforms[7].Split(',');
            if (rend[0] == "1")
            {
                if (rend[0] == "1")
                {
                    go.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Opaque");
                }
                else
                {
                    go.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Transparent");
                }
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
                string posi = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                string roti = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                string scali = binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + "," + binaryReader.ReadSingle() + ";";
                string matvalues = "";
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
                string liny = timer + posi + roti + scali + matvalues;

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
    private void countObjectstracked(GameObject gameObject, string indent, string parentName, List<Tuple<GameObject, bool>> objectstracked, BinaryWriter binarywriter, GameObject allParent)
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
            if (obj.GetComponent<MeshRenderer>().material.HasProperty("_Metallic"))
            {
                binarywriter.Write(true);
                binarywriter.Write(obj.GetComponent<MeshRenderer>().material.GetFloat("_Metallic"));
            }
            else
            {
                binarywriter.Write(false);
            }
            if (obj.GetComponent<MeshRenderer>().material.HasProperty("_Glossiness"))
            {
                binarywriter.Write(true);
                binarywriter.Write(obj.GetComponent<MeshRenderer>().material.GetFloat("_Glossiness"));
            }
            else
            {
                binarywriter.Write(false);
            }
            if (obj.GetComponent<MeshRenderer>().material.HasProperty("_Color"))
            {
                UnityEngine.Color coli = obj.GetComponent<MeshRenderer>().material.GetColor("_Color");
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
            string result = obj.GetComponent<MeshRenderer>().material.GetTag("RenderType", false, "N");
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
        else
        {
            binarywriter.Write(false);
            binarywriter.Write(false);
            binarywriter.Write(false);
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
                go.GetComponent<MeshRenderer>().material.SetFloat("_Metallic", binaryreader.ReadSingle());
            }
            if (binaryreader.ReadBoolean())
            {
                go.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", binaryreader.ReadSingle());
            }
            if (binaryreader.ReadBoolean())
            {
                UnityEngine.Color color = new UnityEngine.Color(binaryreader.ReadSingle(), binaryreader.ReadSingle(), binaryreader.ReadSingle(), binaryreader.ReadSingle());
                go.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            }
            if (binaryreader.ReadBoolean())
            {
                SByte type = binaryreader.ReadSByte();
                if (type == 1)
                {
                    go.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Opaque");
                }
                else
                {
                    go.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Transparent");
                }               
            }
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