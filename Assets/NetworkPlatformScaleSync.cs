using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlatformScaleSync : NetworkBehaviour {

    [SyncVar] public float roomSizeX;

    //ustalenie skali gdy obiekt jest tworzony
    public override void OnStartClient()
    {
        transform.localScale = new Vector2(roomSizeX / GetComponent<SpriteRenderer>().sprite.bounds.size.x, transform.localScale.y);
    }
}
