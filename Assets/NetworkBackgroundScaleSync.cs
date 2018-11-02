using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkBackgroundScaleSync : NetworkBehaviour {

    [SyncVar] public float scaleX;
    [SyncVar] public float scaleY;

    
    public void SetScale(float x, float y)
    {
        scaleX = x;
        scaleY = y;
    }

    public override void OnStartClient()
    {
        transform.localScale = new Vector2(scaleX,scaleY);
    }

}
