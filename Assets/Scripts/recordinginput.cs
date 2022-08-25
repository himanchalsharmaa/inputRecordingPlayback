using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
//using UnityEditor;
using System.IO;
using System.Threading;
//using UnityEngine.Events;

public class recordinginput : MonoBehaviour
{
    public TMP_Text saveinforec;
    public TMP_Text loadinforec;
    public TMP_Text playinforec;
    public TMP_Text populatemenu;
    public GameObject scrollingMenu;
    public GameObject scrollingMenuloc;
    public GameObject nMbuttonprefab;
    private bool populated = false;
    private bool reload = false;
    private GameObject scrollmenu;
    private Thread torunsave;
    //private UnityAction unityAction;
    private List<string> filenames = new List<string>();
    private InputRecordingService inputRecordingService;
    private InputPlaybackService inputPlaybackService;
   // private string path;
    //private IMixedRealityServiceRegistrar test;

    // Start is called before the first frame update
    void Start()
    {
        inputRecordingService = CoreServices.GetDataProvider<InputRecordingService>(CoreServices.InputSystem);
        //Debug.Log(inputRecordingService.UseBufferTimeLimit);
        inputRecordingService.UseBufferTimeLimit = false;
        //Debug.Log(inputRecordingService.UseBufferTimeLimit);
        inputPlaybackService = CoreServices.GetDataProvider<InputPlaybackService>(CoreServices.InputSystem);
        torunsave = new Thread(helpthread);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartRecordingIn()
    {
        inputRecordingService.Enable();
        if (inputRecordingService == null) { Debug.Log("inputRecordingService is null"); }
        saveinforec.text = "Starting Recording";
        inputRecordingService.StartRecording();
        saveinforec.text = "Now Recording";
    }
    public void StopRecordingIn()
    {

        inputRecordingService.StopRecording();
        loadinforec.text = "Stopping Recording";
        string path = Path.Combine(Application.persistentDataPath, "Animations");
        if (Directory.Exists(path))
        {
            loadinforec.text = "Saving Recording";
            torunsave.Start(path);
            reload = true;

            loadinforec.text = "Saved Recording@:";
            //Directory.Delete(path,true);

        }
        else
        {
            Directory.CreateDirectory(path);
            torunsave.Start(path);
        }
        

    }

    private void helpthread(object path)
    {

        inputRecordingService.SaveInputAnimation(path as string);
        //Debug.Log("COMPLETED at:"+path as string);
        inputRecordingService.Disable();
        return;

    }


    public void PlayRecording()
    {
        playinforec.text = "trying to load animation";
        string path = Path.Combine(Application.persistentDataPath, "Animations");
        string[] files = System.IO.Directory.GetFiles(path);
        foreach (string file in files)
        {
            filenames.Add(Path.GetFileName(file));
            //Debug.Log("" + Path.GetFileName(file));
           // //populatenearmenu(file);
           // if (inputPlaybackService.LoadInputAnimation(Application.persistentDataPath + "/Animations/" + Path.GetFileName(file)))
           //{  playinforec.text = "playing:"+ Path.GetFileName(file);
           //   inputPlaybackService.Play(); }
           // else {
           //     playinforec.text = "Error loading"; }
        }
        populatenearmenu(filenames);
    }
    public void populatenearmenu(List<string> files)
    {
        if(populated==false)
        { populatemenu.text = "inititalizing populator";
            //unityAction += shitfunction;
            if (reload == true)
            {
                Destroy(scrollmenu);
            }
        scrollmenu = Instantiate(scrollingMenu, scrollingMenuloc.transform.position, Quaternion.identity);
        GameObject parent = scrollmenu.transform.Find("ScrollingObjectCollection").transform.Find("Container").transform.Find("GridObjectCollection").gameObject;
        foreach (string file in files)
        {
            GameObject button = Instantiate(nMbuttonprefab);
            button.transform.SetParent(parent.transform);
            button.transform.Find("IconAndText").transform.Find("TextMeshPro").GetComponent<TMP_Text>().text = file;
            button.GetComponent<Interactable>().OnClick.AddListener(delegate { playfunction(button); });

        }
        parent.GetComponent<GridObjectCollection>().UpdateCollection();
        populated = true;
        Debug.Log("" + parent.name);
        }
        //dynamiclistpopulator dlp = transform.parent.transform.Find("Canvas").transform.Find("DynamicScrollPopulator").GetComponent<dynamiclistpopulator>();
        //dynamiclistpopulator dlp = new dynamiclistpopulator(); //CANT MAKE LIKE THIS
        
    }
    public void playfunction(GameObject button)
    {
       
        string filewithext = Application.persistentDataPath + "/Animations/" + button.transform.Find("IconAndText").transform.Find("TextMeshPro").GetComponent<TMP_Text>().text;
        Debug.Log("Exists: "+File.Exists(filewithext));
        if (inputPlaybackService.LoadInputAnimation(filewithext))
        {
            playinforec.text = "playing:" + filewithext;
            inputPlaybackService.Play();
        }
        else
        {
            playinforec.text = "Error loading";
        }
    }
}
