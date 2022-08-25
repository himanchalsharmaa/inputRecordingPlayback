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
    
    private void Start()
    {
        position = gameObject.transform.position;
        rotation = gameObject.transform.localEulerAngles;
        scale = gameObject.transform.localScale;
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
