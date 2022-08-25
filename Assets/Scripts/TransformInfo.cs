using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformInfo
{
    public float posx,posy,posz,rotx,roty,rotz,rotw,scalex,scaley,scalez;

    public TransformInfo(GameObject gameObject)
    {
        posx = gameObject.transform.position.x;
        posy = gameObject.transform.position.y;
        posz = gameObject.transform.position.z;
        rotx = gameObject.transform.rotation.x;
        roty = gameObject.transform.rotation.y;
        rotz = gameObject.transform.rotation.z;
        rotw = gameObject.transform.rotation.w;
        scalex = gameObject.transform.localScale.x;
        scaley = gameObject.transform.localScale.y;
        scalez = gameObject.transform.localScale.z;
    }
}
