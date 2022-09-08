using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class transformchangedcomp : MonoBehaviour
{
    private Vector3 position;
    private Vector3 rotation;
    private Vector3 scale;
    public List<Tuple<GameObject, bool>> dicri;
    float meta = -1, glos = -1;
    UnityEngine.Color color;
    UnityEngine.Color defaultcol;
    Material mata;
    string result = "N", reType = "N";
    private bool changed = false;

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
            result = materialprop.GetTag("RenderType", false, "N");
            if (result != "N")
            {
                reType = result;
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
                if (reType!="N")
                {
                    result = mata.GetTag("RenderType", false, "N");
                    if (reType != result)
                    {
                        Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                        dicri.Add(temp);
                        reType = result;
                        changed = true;
                    }
                }
                if (meta != -1 && meta != mata.GetFloat("_Metallic"))
                {
                    if (!changed)
                    {
                        Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                        dicri.Add(temp);
                        changed = true;
                    }
                    meta = mata.GetFloat("_Metallic");
                }
                if (glos != -1 && glos != mata.GetFloat("_Glossiness"))
                {
                    if (!changed)
                    {
                        Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                        dicri.Add(temp);
                        changed = true;
                    }
                    glos = mata.GetFloat("_Glossiness");
                }
                if (color != defaultcol && color != mata.GetColor("_Color"))
                {
                    if (!changed)
                    {
                        Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                        dicri.Add(temp);
                        changed = true;
                    }
                    color = mata.GetColor("_Color");
                }
                if(!changed)
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
            changed = false;
        }
        else if (gameObject.GetComponent<MeshRenderer>())
        {
            mata = gameObject.GetComponent<MeshRenderer>().material;
            if (reType != "N")
            {
                result = mata.GetTag("RenderType", false, "N");
                if (reType != result)
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                    dicri.Add(temp);
                    reType = result;
                    changed = true;
                }
            }
            if (meta != -1 && meta != mata.GetFloat("_Metallic"))
            {
                if (!changed)
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                    dicri.Add(temp);
                    changed = true;
                }
                meta = mata.GetFloat("_Metallic");
            }
            if (glos != -1 && glos != mata.GetFloat("_Glossiness"))
            {
                if (!changed)
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                    dicri.Add(temp);
                    changed = true;
                }
                glos = mata.GetFloat("_Glossiness");
            }
            if (color != defaultcol && color != mata.GetColor("_Color"))
            {
                if (!changed)
                {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool>(gameObject, true);
                    dicri.Add(temp);
                }
                color = mata.GetColor("_Color");
            }
            changed = false;
        }
    }

    public static bool IsThereChangeInGameObject(Transform currentTransform, bool activeSelf, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (currentTransform.gameObject.activeSelf.Equals(activeSelf) && currentTransform.position.Equals(position) && currentTransform.eulerAngles.Equals(rotation) && currentTransform.localScale.Equals(scale))
            return false;
        return true;
    }


}