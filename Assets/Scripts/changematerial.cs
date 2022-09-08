using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class changematerial : MonoBehaviour
{
    public GameObject tochange;
    public List<Material> materiallist;
    public TMP_Text changeinfo;

    public void colorchange()
    {
        changeinfo.text = "Beginning change";
        UnityEngine.Color col = new UnityEngine.Color(0,0,1,1);
        tochange.GetComponent<MeshRenderer>().material.SetColor("_Color",col);
        string res= tochange.GetComponent<MeshRenderer>().material.GetTag("RenderType", false, "N");
        Debug.Log(res);
        if (res == "Transparent")
        {
            tochange.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Opaque");
            Debug.Log("Set to Opaque");
        }
        else if (res == "Opaque")
        {
            tochange.GetComponent<MeshRenderer>().material.SetOverrideTag("RenderType", "Transparent");
            Debug.Log("Set to Transparent");
        }
        changeinfo.text = "change done";
    }
}
