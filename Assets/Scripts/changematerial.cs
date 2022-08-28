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
        tochange.GetComponent<MeshRenderer>().sharedMaterial = materiallist[1];
        changeinfo.text = "change done";
    }
}
