using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class transformchangedcomp : MonoBehaviour
{
    private Vector3 position;
    private Vector3 rotation;
    private Vector3 scale;
    public List<Tuple<GameObject,bool>> dicri;
    float meta=-1, glos=-1;
    UnityEngine.Color color;
    UnityEngine.Color defaultcol;
    Material mata;

    private void Start()
    {
        position = gameObject.transform.position;
        rotation = gameObject.transform.localEulerAngles;
        scale = gameObject.transform.localScale;
        if (gameObject.GetComponent<MeshRenderer>())
        {
            Material materialprop = gameObject.GetComponent<MeshRenderer>().material;
            if (materialprop.HasProperty("_Metallic"))
            {
                meta = materialprop.GetFloat("_Metallic");
            }
            if (materialprop.HasProperty("_Glossiness"))
            {
                glos = materialprop.GetFloat("_Glossiness");
            }
            if (materialprop.HasProperty("_Color"))
            {
                color = materialprop.GetColor("_Color");
            }
        }
        
    }
    void Update()
    {

        if (IsThereChangeInGameObject(gameObject.transform, gameObject.activeSelf, position, rotation, scale))
        {
            position = gameObject.transform.position;
            rotation = gameObject.transform.localEulerAngles;
            scale = gameObject.transform.localScale;
            if (gameObject.GetComponent<MeshRenderer>())
            {
                mata = gameObject.GetComponent<MeshRenderer>().material;
                if (meta != -1 && meta != mata.GetFloat("_Metallic"))
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                    dicri.Add(temp);
                    meta = mata.GetFloat("_Metallic");
                }
                else if (glos != -1 && glos != mata.GetFloat("_Glossiness"))
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                    dicri.Add(temp);
                    glos = mata.GetFloat("_Glossiness");
                }
                else if (color != defaultcol && color != mata.GetColor("_Color"))
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                    dicri.Add(temp);
                    color = mata.GetColor("_Color");
                }
                else
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, false);
                    dicri.Add(temp);
                }
            }
            else
            {
                Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, false);
                dicri.Add(temp);
            }
        }
        if (gameObject.GetComponent<MeshRenderer>())
        {
            mata = gameObject.GetComponent<MeshRenderer>().material;
            if (meta != -1 && meta != mata.GetFloat("_Metallic"))
            {
                Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                dicri.Add(temp);
                meta = mata.GetFloat("_Metallic");
            }
            else if (glos != -1 && glos != mata.GetFloat("_Glossiness"))
            {
                Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                dicri.Add(temp);
                glos = mata.GetFloat("_Glossiness");
            }
            else if (color != defaultcol && color != mata.GetColor("_Color"))
            {
                Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                dicri.Add(temp);
                color = mata.GetColor("_Color");
            }
        }
    }

    public static bool IsThereChangeInGameObject(Transform currentTransform, bool activeSelf, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (currentTransform.gameObject.activeSelf.Equals(activeSelf) && currentTransform.position.Equals(position) && currentTransform.eulerAngles.Equals(rotation) && currentTransform.localScale.Equals(scale))
            return false;
        return true;
    }


}
