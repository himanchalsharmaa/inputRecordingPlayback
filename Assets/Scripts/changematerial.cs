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
    private heirarchysaveload hrs;

    private void Awake()
    {
        hrs = nestedObj.GetComponent<heirarchysaveload>();
    }
    public void colorchange()
    {
        tochange.SetActive(true);
        GameObject temp=hrs.instantiateRecorded(go,goTRS.transform.position,goTRS.transform.rotation,hrs.aname);
        if (temp != null)
        {
            //temp.transform.parent = goTRS.transform;
            temp.AddComponent<ObjectManipulator>();
        }
        else
        {
            Debug.Log("Object NULL instantiated");
        }
        StartCoroutine(changecol());
    }
    IEnumerator changecol()
    {
        yield return new WaitForSeconds(3);
        UnityEngine.Color col = new UnityEngine.Color(0, 0, 1, 1);
        tochange.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", col);
        tochange.GetComponent<MeshRenderer>().materials[1].SetColor("_Color", col);
        changeinfo.text = "Beginning change";
        string res = tochange.GetComponent<MeshRenderer>().material.GetTag("RenderType", false, "N");
        if (res == "Transparent")
        {
            tochange.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Opaque");
        }
        else if (res == "Opaque")
        {
            tochange.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Transparent");
        }
        changeinfo.text = "change done";
        StartCoroutine(waitCor());
    }
    IEnumerator waitCor()
    {
        yield return new WaitForSeconds(3);
        hrs.destroyRecorded(tochange);
    }

}
