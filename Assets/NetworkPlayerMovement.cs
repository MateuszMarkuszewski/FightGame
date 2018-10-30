using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkPlayerMovement : NetworkBehaviour {

    public PlayerControler PC;

    private void Start()
    {
        if (hasAuthority)
        {
            PC = GetComponent<PlayerControler>();
        }
    }

    //komenda do serwera z prośbą o bieg postaci
    [Command]
    public void CmdMove(float way)
    {
        PC.Move(way);
    }
    //komenda do serwera z prośbą o skok postaci
    [Command]
    public void CmdJump()
    {
        PC.Jump();
    }
    //komenda do serwera z prośbą o atak z powietrza postaci
    [Command]
    public void CmdDropAttack()
    {
        PC.DropAttack();
    }

    [Command]
    public void CmdDash()
    {
        //StartCoroutine(PC.Dash(0.1f, dashWay));
    }
    /*
    //
    [Command]
    public void CmdComeDown(Collider2D collider)
    {
        PC.ComeDown(collider);
    }*/

    //
    [Command]
    public void CmdBasicAttack()
    {
        PC.BasicAttack();
    }

    //
    [Command]
    public void CmdThrow()
    {
        PC.Throw();
    }

    //
    [Command]
    public void CmdTakeWeapon(GameObject weapon)
    {
        PC.TakeWeapon(weapon);
    }


}
