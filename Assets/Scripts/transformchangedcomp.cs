using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class transformchangedcomp : MonoBehaviour
{
    private Vector3 position;
    private Vector3 rotation;
    private Vector3 scale;
    public List<Tuple<GameObject, bool, Byte>> dicri;
    float meta = -1, glos = -1;
    private Material[] materialist;
    private float[] metalist;
    private float[] gloslist;
    private UnityEngine.Color[] colorlist;
    private string[] reTypelist;
    private Byte materialnum;
    UnityEngine.Color color;
    UnityEngine.Color defaultcol;
    private Material[] mata;
    string result = "N", reType = "N";
    private bool changed = false;

    private void Start()
    {
        position = gameObject.transform.position;
        rotation = gameObject.transform.localEulerAngles;
        scale = gameObject.transform.localScale;
        if (gameObject.GetComponent<MeshRenderer>())
        {
            materialist = gameObject.GetComponent<MeshRenderer>().materials;
            metalist = new float[materialist.Length];
            gloslist = new float[materialist.Length];
            colorlist = new UnityEngine.Color[materialist.Length];
            reTypelist = new string[materialist.Length];
            for (Byte i=0; i<Convert.ToByte(materialist.Length); i++) {
                Material materialprop = materialist[i];
                if (materialprop.HasProperty("_Metallic"))
                {
                    metalist[i] = materialprop.GetFloat("_Metallic");
                }
                else {
                    metalist[i] = -1;
                }
                if (materialprop.HasProperty("_Glossiness"))
                {
                    gloslist[i] = materialprop.GetFloat("_Glossiness");
                }
                else if (materialprop.HasProperty("_Smoothness"))
                {
                    gloslist[i] = materialprop.GetFloat("_Smoothness");
                }
                else
                {
                    gloslist[i] = -1;
                }
                if (materialprop.HasProperty("_Color"))
                {
                    colorlist[i] = materialprop.GetColor("_Color");
                }
                else
                {
                    colorlist[i] = defaultcol;
                }
                result = materialprop.GetTag("RenderType", false, "N");
                if (result != "N")
                {
                    reTypelist[i] = result;
                }
                else
                {
                    reTypelist[i] = "N";
                }
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
                mata = gameObject.GetComponent<MeshRenderer>().materials;
                for(Byte i = 0; i < Convert.ToByte(mata.Length); i++)
                {
                    if (reType != "N")
                    {
                        result = mata[i].GetTag("RenderType", false, "N");
                        if (reTypelist[i] != result)
                        {
                            Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool,Byte>(gameObject, true, i);
                            dicri.Add(temp);
                            reTypelist[i] = result;
                            changed = true;
                        }
                    }
                    if (metalist[i] != -1 && metalist[i] != mata[i].GetFloat("_Metallic"))
                    {
                        if (!changed)
                        {
                            Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                            dicri.Add(temp);
                            changed = true;
                        }
                        metalist[i] = mata[i].GetFloat("_Metallic");
                    }
                    if (gloslist[i] != -1)
                    {
                        if (mata[i].HasProperty("_Glossiness") && gloslist[i] != mata[i].GetFloat("_Glossiness"))
                        {
                            if (!changed)
                            {
                                Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                                dicri.Add(temp);
                                changed = true;
                            }
                            gloslist[i] = mata[i].GetFloat("_Glossiness");
                        }
                        else if (mata[i].HasProperty("_Smoothness") && gloslist[i] != mata[i].GetFloat("_Smoothness"))
                        {
                            if (!changed)
                            {
                                Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                                dicri.Add(temp);
                                changed = true;
                            }
                            gloslist[i] = mata[i].GetFloat("_Smoothness");
                        }
                    }
                    if (colorlist[i] != defaultcol && colorlist[i] != mata[i].GetColor("_Color"))
                    {
                        if (!changed)
                        {
                            Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                            dicri.Add(temp);
                            changed = true;
                        }
                        colorlist[i] = mata[i].GetColor("_Color");
                    }
                    if (!changed)
                    {
                        Tuple<GameObject, bool,Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, false,0);
                        dicri.Add(temp);
                    }
                    changed = false;
                }
            }
            else
                {
                    Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool,Byte>(gameObject, false,0);
                    dicri.Add(temp);
                }
                changed = false;
            }
        else if (gameObject.GetComponent<MeshRenderer>())
        {
            mata = gameObject.GetComponent<MeshRenderer>().materials;
            for (Byte i = 0; i < Convert.ToByte(mata.Length); i++)
            {
                if (reType != "N")
                {
                    result = mata[i].GetTag("RenderType", false, "N");
                    if (reTypelist[i] != result)
                    {
                        Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                        dicri.Add(temp);
                        reTypelist[i] = result;
                        changed = true;
                    }
                }
                if (metalist[i] != -1 && metalist[i] != mata[i].GetFloat("_Metallic"))
                {
                    if (!changed)
                    {
                        Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                        dicri.Add(temp);
                        changed = true;
                    }
                    metalist[i] = mata[i].GetFloat("_Metallic");
                }
                if (gloslist[i] != -1)
                {
                    if (mata[i].HasProperty("_Glossiness") && gloslist[i] != mata[i].GetFloat("_Glossiness"))
                    {
                        if (!changed)
                        {
                            Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                            dicri.Add(temp);
                            changed = true;
                        }
                        gloslist[i] = mata[i].GetFloat("_Glossiness");
                    }
                    else if (mata[i].HasProperty("_Smoothness") && gloslist[i] != mata[i].GetFloat("_Smoothness"))
                    {
                        if (!changed)
                        {
                            Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                            dicri.Add(temp);
                            changed = true;
                        }
                        gloslist[i] = mata[i].GetFloat("_Smoothness");
                    }
                }
                if (colorlist[i] != defaultcol && colorlist[i] != mata[i].GetColor("_Color"))
                {
                    if (!changed)
                    {
                        Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, true, i);
                        dicri.Add(temp);
                        changed = true;
                    }
                    colorlist[i] = mata[i].GetColor("_Color");
                }
                if (!changed)
                {
                    Tuple<GameObject, bool, Byte> temp = new Tuple<GameObject, bool, Byte>(gameObject, false, 0);
                    dicri.Add(temp);
                }
                changed = false;
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