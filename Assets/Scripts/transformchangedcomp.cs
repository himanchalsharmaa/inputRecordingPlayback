using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class transformchangedcomp : MonoBehaviour
{
    private bool once=true;
    private Vector3 position;
    private Vector3 rotation;
    private Vector3 scale;
    public List<GameObject> dicri;
    public string[] supportedsaves;
    Material materialprop;

    private void Start()
    {
        position = gameObject.transform.position;
        rotation = gameObject.transform.localEulerAngles;
        scale = gameObject.transform.localScale;
        if (gameObject.GetComponent<MeshRenderer>())
        {
            materialprop = gameObject.GetComponent<MeshRenderer>().material;
        }
        
    }
    void Update()
    {
        if (IsThereChangeInGameObject(gameObject.transform,gameObject.activeSelf,position,rotation,scale)) 
        {       
            addtodic(dicri);
            position = gameObject.transform.position;
            rotation = gameObject.transform.localEulerAngles;
            scale = gameObject.transform.localScale;
            //transform.hasChanged=false;
        }
        if (materialprop != null)
        {
            Material mata = gameObject.GetComponent<MeshRenderer>().material;
            if (materialprop != mata)                   //Don't know yet if material comparison goes through everything property by property
            {
                if (materialprop.HasProperty("_Metallic"))
                {
                    Debug.Log("metallic: "+ materialprop.GetFloat("_Metallic"));
                }
                if (materialprop.HasProperty("_Glossiness"))
                {
                    Debug.Log("Glossiness: " + materialprop.GetFloat("_Glossiness"));
                }
                if (materialprop.HasProperty("_Color"))
                {
                    Debug.Log("Color: " + materialprop.GetColor("_Color"));
                }
                //for (int i = 0; i < supportedsaves.Length; i++)
                //{

                //    if (mata.HasProperty(supportedsaves[i]))
                //    {
                //        Debug.Log(mata.GetFloat(supportedsaves[i]));
                //    }
                //}
                materialprop = mata;
            }
        }
    }

    public void addtodic(List<GameObject> dicri)
    {
        dicri.Add(gameObject);
    }
    public static bool IsThereChangeInGameObject(Transform currentTransform, bool activeSelf, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (currentTransform.gameObject.activeSelf.Equals(activeSelf) && currentTransform.position.Equals(position) && currentTransform.eulerAngles.Equals(rotation) && currentTransform.localScale.Equals(scale))
            return false;
        return true;
    }


}
