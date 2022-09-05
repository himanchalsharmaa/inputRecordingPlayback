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
            position = gameObject.transform.position;
            rotation = gameObject.transform.localEulerAngles;
            scale = gameObject.transform.localScale;
            if (materialprop != null)
            {
                Material mata = gameObject.GetComponent<MeshRenderer>().material;
                if (materialprop != mata)
                {
                    Tuple<GameObject, bool> temp =new Tuple<GameObject, bool> (gameObject,true);
                    dicri.Add(temp);
                    materialprop = mata;
                }
                else {
                    Tuple<GameObject, bool> temp = new Tuple<GameObject, bool> ( gameObject, false );
                    dicri.Add(temp);
                }
            }
            else
            {
                Tuple<GameObject, bool> temp = new Tuple<GameObject, bool> ( gameObject, false );
                dicri.Add(temp);
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
