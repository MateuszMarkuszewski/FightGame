using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkPlayerMovement : NetworkBehaviour {

    public PlayerControler PC;
    /*
    private void Start()
    {
        if (hasAuthority)
        {
            PC = GetComponent<PlayerControler>();
        }
    }*/

    //komenda do serwera z prośbą o bieg postaci
    [Command]
    public void CmdMove(float way)
    {
       
        PC.horizontalMove = way;
        PC.Move(way);
    }
    //komenda do serwera z prośbą o skok postaci
    [Command]
    public void CmdJump()
    {
        RpcJump();
        //PC.Jump();
    }
    [ClientRpc]
    public void RpcJump()
    {
        if (!hasAuthority)
        {
            Debug.Log("rpcjump");
            PC.Jump();
        }
    }
    //komenda do serwera z prośbą o atak z powietrza postaci
    [Command]
    public void CmdDropAttack()
    {
        RpcDropAttack();
        //PC.DropAttack();
    }
    [ClientRpc]
    public void RpcDropAttack()
    {
        if (!hasAuthority)
        {
            Debug.Log("rpcdropattak");
            PC.DropAttack();
        }
    }

    //komenda do serwera z prośbą o zryw
    [Command]
    public void CmdDash(float time, int way)
    {
        RpcDash(time, way);
    }
    [ClientRpc]
    public void RpcDash(float time, int way)
    {
        if (!hasAuthority)
        {
            Debug.Log("rpcdash");
            StartCoroutine(PC.Dash(time, way));
        }
    }

    //komenda do serwera z prośbą o zejscie z platformy
    [Command]
    public void CmdComeDown()
    {
        //PC.ComeDown();
        RpcComeDown();
    }
    [ClientRpc]
    public void RpcComeDown()
    {
        if (!hasAuthority)
        {
            Debug.Log("rpccd");
            PC.ComeDown();
        }
    }

    //komenda do serwera z prośbą o podstawowoy atak
    [Command]
    public void CmdBasicAttack()
    {
        //PC.BasicAttack();
        RpcBasicAttack();
    }
    [ClientRpc]
    public void RpcBasicAttack()
    {
        if (!hasAuthority)
        {
            Debug.Log("rpbasicattak");
            PC.BasicAttack();
        }
    }


    //komenda do serwera z prośbą o rzut bronią
    [Command]
    public void CmdThrow()
    {
       // PC.Throw();
        RpcThow();
    }
    [ClientRpc]
    public void RpcThow()
    {
        if (!hasAuthority)
        {
            Debug.Log("rpcthrow");
            PC.Throw();
        }
    }

    //komenda do serwera z prośbą o podniesienie broni
    [Command]
    public void CmdTakeWeapon(GameObject weapon)
    {
        //PC.TakeWeapon(weapon);
        RpcTakeWeapon(weapon);
    }
    [ClientRpc]
    public void RpcTakeWeapon(GameObject weapon)
    {
        if (!hasAuthority)
        {
            Debug.Log("rpctake");
            PC.TakeWeapon(weapon);
        }
    }

    //komenda do serwera z prośbą o podniesienie broni
    [Command]
    public void CmdDetachWeapon()
    {
        PC.DetachWeapon();
    }
    //komenda do serwera z prośbą o podniesienie broni
    [Command]
    public void CmdDropWeapon()
    {
        PC.DropWeapon();
    }

    //pokonanie 
    [Command]
    public void CmdDie()
    {
       // PC.Die();
        RpcDie();
    }
    [ClientRpc]
    public void RpcDie()
    {
        if (!hasAuthority)
        {
            Debug.Log("die");
            PC.Die();
        }
    }
}
