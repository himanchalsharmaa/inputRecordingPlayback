using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Runtime.InteropServices;
using Microsoft.MixedReality.Toolkit.UI;




public class changematerial : MonoBehaviour
{
    public GameObject nestedObj;
    public GameObject tochange;
    public List<Material> materiallist;
    public TMP_Text changeinfo;
    public GameObject go;
    public GameObject goTRS;

    public void colorchange()
    {
        changeinfo.text = "Beginning change";
        UnityEngine.Color col = new UnityEngine.Color(0,0,1,1);
        tochange.GetComponent<MeshRenderer>().materials[0].SetColor("_Color",col);
        tochange.GetComponent<MeshRenderer>().materials[1].SetColor("_Color", col);
        // Debug.Log(Shader.Find("Mixed Reality Toolkit/Standard")); // MixedRealityToolkit/Standard
        //List<string> outnames = new List<string>();
        //tochange.GetComponent<MeshRenderer>().material.GetTexturePropertyNames(outnames);
        //for (int i = 0; i < outnames.Count; i++)
        //{
        //    Debug.Log(outnames[i]);
        //}
        string res= tochange.GetComponent<MeshRenderer>().material.GetTag("RenderType", false, "N");
        if (res == "Transparent")
        {
            tochange.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Opaque");
        }
        else if (res == "Opaque")
        {
            tochange.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Transparent");
        }
        changeinfo.text = "change done";
        heirarchysaveload hrs=nestedObj.GetComponent<heirarchysaveload>();
        GameObject temp=hrs.instantiateRecorded(go,goTRS.transform.position,goTRS.transform.rotation);
        if (temp != null)
        {
            temp.transform.parent = goTRS.transform;
            temp.AddComponent<ObjectManipulator>();
        }
        else
        {
            Debug.Log("Object NULL instantiated");
        }
    }
}
