using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkPlayerMovement : NetworkBehaviour {

    public PlayerControler PC;


    //komenda do serwera z prośbą o wywołanie biegu
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
    }

    //wywołanie skoku na instacji która nie ma praw do avatara
    [ClientRpc]
    public void RpcJump()
    {
        if (!hasAuthority)
        {
            PC.Jump();
        }
    }

    //komenda do serwera z prośbą o atak z powietrza postaci
    [Command]
    public void CmdDropAttack()
    {
        RpcDropAttack();
    }

    //wywołanie ataku z powietrza na instacji która nie ma praw do avatara
    [ClientRpc]
    public void RpcDropAttack()
    {
        if (!hasAuthority)
        {
            PC.DropAttack();
        }
    }

    //komenda do serwera z prośbą o zryw
    [Command]
    public void CmdDash(float time, int way)
    {
        RpcDash(time, way);
    }

    //wywołanie zrywu na instacji która nie ma praw do avatara
    [ClientRpc]
    public void RpcDash(float time, int way)
    {
        if (!hasAuthority)
        {
            StartCoroutine(PC.Dash(time, way));
        }
    }

    //komenda do serwera z prośbą o zejscie z platformy
    [Command]
    public void CmdComeDown()
    {
        RpcComeDown();
    }

    //wywołanie zejścia z platformy na instacji która nie ma praw do avatara
    [ClientRpc]
    public void RpcComeDown()
    {
        if (!hasAuthority)
        {
            PC.ComeDown();
        }
    }

    //komenda do serwera z prośbą o podstawowoy atak
    [Command]
    public void CmdBasicAttack()
    {
        RpcBasicAttack();
    }

    //wywołanie ataku na instacji która nie ma praw do avatara
    [ClientRpc]
    public void RpcBasicAttack()
    {
        if (!hasAuthority)
        {
            PC.BasicAttack();
        }
    }

    //komenda do serwera z prośbą o rzut bronią
    [Command]
    public void CmdThrow()
    {
        RpcThow();
    }

    //wywołanie rzutu na instacji która nie ma praw do avatara
    [ClientRpc]
    public void RpcThow()
    {
        if (!hasAuthority)
        {
            PC.Throw();
        }
    }

    //komenda do serwera z prośbą o podniesienie broni
    [Command]
    public void CmdTakeWeapon(GameObject weapon)
    {
        RpcTakeWeapon(weapon);
    }

    //wywołanie podniesienia broni na instacji która nie ma praw do avatara
    [ClientRpc]
    public void RpcTakeWeapon(GameObject weapon)
    {
        if (!hasAuthority)
        {
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
}
