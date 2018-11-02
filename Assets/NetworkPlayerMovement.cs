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

    //komenda do serwera z prośbą o zryw
    [Command]
    public void CmdDash()
    {
        StartCoroutine(PC.Dash(0.1f, PC.dashWay));
    }

    //komenda do serwera z prośbą o zejscie z platformy
    [Command]
    public void CmdComeDown()
    {
        PC.ComeDown();
    }

    //komenda do serwera z prośbą o podstawowoy atak
    [Command]
    public void CmdBasicAttack()
    {
        PC.BasicAttack();
    }

    //komenda do serwera z prośbą o rzut bronią
    [Command]
    public void CmdThrow()
    {
        PC.Throw();
    }

    //komenda do serwera z prośbą o podniesienie broni
    [Command]
    public void CmdTakeWeapon(GameObject weapon)
    {
        PC.TakeWeapon(weapon);
    }

}
