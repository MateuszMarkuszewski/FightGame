using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControler : MonoBehaviour {

    //statystyki
    public float heroSpeed;
    public float jumpForce;
    public int throwSpeed;
    public float maxHealth;
    private float currentHealth;
    //klawisze
    public KeyCode left;
    public KeyCode right;
    public KeyCode jumpKey;
    public KeyCode down;
    public KeyCode attack;
    public KeyCode takeWeapon;
    //testery
    public Transform groundTester;
    public LayerMask layersToTest;
    public Transform throwPoint;
    private float radius = 0.1f;
    //GUI
    public Image healthBar;
    public Image weaponDurability;
    //komponenty
    public Animator anim;
    Rigidbody2D rgdBody;
    ComboManager comboManager;
    Rigidbody2D[] rigs;
    public TrailRenderer[] trailEffect;
    //stany
    private bool canJump = false;
    public int dashWay = 0;
    public bool drop = false;
    public bool jump = false;
    private bool keysEnable = true;
    public bool attackEnable = true;
    bool dirToRight = true;
    public float horizontalMove;
    private bool ragdoll = false;
    public bool AI = false;
    //walka
    public GameObject weapon = null;
    private int combo = 1;
    private int maxcombo = 2;
    private float lastAttackTime = 0;
    private float timeBetweenAttack = 0.3f;
    private Vector2 ragdollForce;
    private struct Stun
    {
        public static float dmgToStun = 50f;
        public static float timeForDmg = 4f;
        public static float stunTime = 2f;
        public float lastTime;
        public float lastHealth;
    }
    private Stun stunControler;
    Quaternion rotation;

    //struktura przechowująca kości i ik 
    //używana przy podnoszeniu postaci
    public struct Limb
    {
        public Vector3 orgPosition;
        public Transform ik;

        public Limb(Transform ik)
        {
            this.ik = ik;
            this.orgPosition = ik.localPosition;
        }
    }
    public Limb[] limbs;

    class ComplateCoroutines { public int num = 0; }
    
    void Start ()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rgdBody = GetComponent<Rigidbody2D>();
        comboManager = GetComponent<ComboManager>();
        rigs = GetComponentsInChildren<Rigidbody2D>();
        

        //tworze listę objektów limbs która służy do ustawiania postaci do pozycji stojącej
        Transform[] iks = GetComponentInChildren<Transform>().Find("Skeleton").gameObject.GetComponentsInChildren<Transform>();
        limbs = new Limb[iks.Length];
        for(int i=0; i<iks.Length; i++)
        {
            limbs[i] = new Limb(iks[i]);
        }
        foreach(Limb i in limbs)
        {
            Debug.Log(i.ik);
        }
        //StartCoroutine(CheckIfStuned());
    }

    private void FixedUpdate()
    {
        
        Move(horizontalMove);
        if (jump)
        {
            Jump();
        }
        if (drop)
        {
            DropAttack();
        }
        if (dashWay != 0)
        {
            StartCoroutine(Dash(0.1f, dashWay));
        }
    }

    void Update () {
        canJump = Physics2D.OverlapCircle(groundTester.position, radius, layersToTest);

        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeState();
        }

        if (ragdoll)
        {
            transform.position = new Vector2(limbs[7].ik.position.x, limbs[7].ik.position.y + (transform.position.y - GetComponent<CapsuleCollider2D>().bounds.min.y));            
        }

        if (Input.GetKeyDown(left))
        {
            if (comboManager.DoubleClick(left))
            {
                dashWay = -1;
            }
        }
        if (Input.GetKeyDown(right))
        {
            if (comboManager.DoubleClick(right))
            {
                dashWay = 1;
            }
        }
        IsGrounded();
        if(AI == false)
        {
            //postać na ziemi
            if (!anim.GetBool("InAir"))
            {
                horizontalMove = keysEnable ? ((Input.GetKey(left) ? -1 : 0) + (Input.GetKey(right) ? 1 : 0)) : 0;
                anim.SetFloat("speed", Mathf.Abs(horizontalMove));
                ///atak
                if (Input.GetKeyDown(attack) && !Input.GetKeyDown(takeWeapon) )
                {
                    BasicAttack();
                }
            }
            else
            {
                //atak z powietrza
                if (Input.GetKeyDown(attack) && !Input.GetKeyDown(takeWeapon) && keysEnable)
                {
                    drop = true;
                }
            }
            /// skakanie
            if (Input.GetKeyDown(jumpKey) && keysEnable && canJump)
            {
                jump = true;
            }

            if (Input.GetKeyDown(takeWeapon) && Input.GetKeyDown(attack) && (weapon != null) && keysEnable)
            {
                Throw();
            }
        }   
        
        /// odwracanie sprita postaci w lewo
        if (horizontalMove < 0 && dirToRight)
        {
            Flip();
        }
        /// odwracanie sprita postaci w prawo
        if (horizontalMove > 0 && !dirToRight)
        {
            Flip();
        }
        ///sprawdza czy postac zyje
        if (currentHealth <= 0)
        {
            if(weapon!=null)DropWeapon();
            if(!ragdoll)ChangeState();
            //Destroy(this);
        }
    }

    /// <summary>
    /// Funkcje odpowiedzialne za kontrole postaci:
    /// </summary>
    /// 



    public void Move(float way)
    {
       rgdBody.velocity = new Vector2(way * heroSpeed, rgdBody.velocity.y);       
       //rgdBody.AddForce(new Vector2(way * heroSpeed,0),ForceMode2D.Impulse);
    }

    //wykonanie podstawowowego ataku
    public void BasicAttack()
    {
        if (attackEnable)
        {
            if ( Time.time - lastAttackTime >= timeBetweenAttack)
            {
                combo = comboManager.Step(combo, maxcombo);
                if (combo == maxcombo) StartCoroutine(DisableAttack(1f));
                try
                {
                    anim.SetTrigger(weapon.name + "-attack" + combo);
                    //StartCoroutine(DisableKeys(timeBetweenAttack));
                }
                catch
                {
                    anim.SetTrigger("NoWeapon-attack" + combo);
                    //StartCoroutine(DisableKeys(timeBetweenAttack));
                }
                lastAttackTime = Time.time;
            }
        }
    }


    //skok
    public void Jump()
    {
        rgdBody.velocity = new Vector2(rgdBody.velocity.x, jumpForce);
        anim.SetTrigger("jump");
        jump = false;
    }

    //zejscie z platformy
    public void ComeDown(Collider2D collider)
    {
        Physics2D.IgnoreCollision(collider, gameObject.GetComponent<Collider2D>());
        StartCoroutine(ReturnCollision(collider, gameObject.GetComponent<Collider2D>()));
    }

    //rzut podniesioną bronią
    public void Throw()
    {
        anim.SetTrigger("throw");
    }

    //podniesienie broni
    public void TakeWeapon(GameObject w)
    {
        if (weapon == null)
        {
            weapon = w;
            //ustawienie inferfejsu
            weapon.GetComponent<WeaponControler>().durabilityImage = weaponDurability;
            weaponDurability.gameObject.SetActive(true);
            weaponDurability.sprite = weapon.GetComponent<SpriteRenderer>().sprite;
            //
            weapon.GetComponent<WeaponControler>().HandleWeapon(gameObject.transform);
                //SendMessage("HandleWeapon", gameObject.transform);
            maxcombo = weapon.GetComponent<WeaponControler>().maxcombo;
            //rebind poniewaz bron nie jest nieodłącznym elementem postaci i przed tym nie wie o jaki attackcollider chodzi
            anim.Rebind();
            anim.SetBool(weapon.name, true);
            anim.SetBool("NoWeapon", false);
        }
    }

     public void DropAttack()
    {
        anim.SetTrigger("dropAttack");
        rgdBody.velocity = new Vector2(0f, -20f);
        drop = false;
    }

    IEnumerator Dash(float dashTime, float way)
    {
        float startTime = Time.time;
        dashWay = 0;
        StartCoroutine(DisableKeys(dashTime));
        foreach (TrailRenderer trail in trailEffect)
        {
            trail.Clear();
            trail.gameObject.SetActive(true);
        }
        anim.SetBool("dash", true);
        horizontalMove = way;
        while (Time.time - startTime <= dashTime)
        {
            Debug.Log("dash");
            rgdBody.velocity = new Vector2(way * 50f, 0f);
            //rgdBody.AddForce(new Vector2(way * 10000f, 0f));
            yield return null;
        }
        anim.SetBool("dash", false);
        foreach (TrailRenderer trail in trailEffect)
        {
            trail.gameObject.SetActive(false);
        }
        //rgdBody.velocity = new Vector2(0f, 0f);
    }

    void Block()
    {
        anim.SetTrigger("block");
    }


    /// <summary>
    /// Wykrywanie kolizji:
    /// </summary>

    private void OnTriggerStay2D(Collider2D collision)
    {
        //podnoszenie broni, czeka na kolizje z PickUpTrigger
        if (weapon == null && Input.GetKeyDown(takeWeapon) && !Input.GetKeyDown(attack) && collision.gameObject.tag == "Interaction")
        {
            TakeWeapon(collision.transform.parent.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall" && anim.GetBool("InAir"))
        {
            anim.SetBool("wallstay",true);
            if (Input.GetKeyDown(jumpKey))
            {
                anim.SetBool("wallstay", false);
                horizontalMove = collision.contacts[0].normal.x;
                jump = true;
            }
        }
        else
        {
            anim.SetBool("wallstay",false);
        }
        //schodzenie z platformy
        if (collision.gameObject.tag == "Platform")
        {
            if ((Input.GetKeyDown(down) && keysEnable))
            {
                ComeDown(collision.collider);
            }
        }
        //if (collision.gameObject.tag == "Interaction") Debug.Log("seeeeeeee");
    }
    public void GroundTest(Collider2D collision)
    {

    }

/// <summary>
/// Funkcje ustawiające/zmieniające stan postaci:
/// </summary>

    IEnumerator DisableKeys(float time)
    {
        keysEnable = false;
        yield return new WaitForSeconds(time);
        keysEnable = true;     
    }

    IEnumerator DisableAttack(float time)
    {
        attackEnable = false;
        yield return new WaitForSeconds(time);
        attackEnable = true;
    }

    IEnumerator ReturnCollision(Collider2D coll1, Collider2D coll2)
    {
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(coll1, coll2, false);
    }

    //czy postać dotyka ziemi
    void IsGrounded()
    {
        if(canJump && anim.GetBool("InAir") && rgdBody.velocity.y == 0)
        {
            anim.SetBool("InAir", false);
        }else if(!canJump && !anim.GetBool("InAir"))
        {
            anim.SetBool("InAir", true);
        }
    }

    /// w objekcie postaci zmienia wartosc Transform>Scale>x aby odwrócić sprite w drugą stronę
    void Flip()
    {
        dirToRight = !dirToRight;
        gameObject.transform.localScale = new Vector2(-gameObject.transform.localScale.x, gameObject.transform.localScale.y);
    }

    //zmniejsza HP
    public void DealDamage(int dmg)
    {
        horizontalMove = 0;
        currentHealth = currentHealth - dmg;
        healthBar.fillAmount = currentHealth / maxHealth;
        anim.SetTrigger("TakeHit");

        if (Time.time - stunControler.lastTime > Stun.timeForDmg)
        {
            stunControler.lastTime = Time.time;
            stunControler.lastHealth = currentHealth;
        }
    }
    

    IEnumerator CheckIfStuned()
    {
        while(true)
        {
            if (stunControler.lastHealth - currentHealth >= Stun.dmgToStun && Time.time - stunControler.lastTime <= Stun.timeForDmg && !ragdoll)
            {
                ChangeState();
                stunControler.lastTime = Time.time;
            }
            else if(ragdoll && Time.time - stunControler.lastTime >= Stun.stunTime)
            {
                ChangeState();
            }
            yield return null;
        }
    }

    public void SaveForce(Vector2 f)
    {
        ragdollForce = f;
    }

    //event wywoływany przez animacje
    void ThrowEvent(float angle)
    {
        DropWeapon();
        
        weapon.GetComponent<Rigidbody2D>().AddForce(new Vector2(throwSpeed * transform.localScale.x, angle), ForceMode2D.Impulse);
        weapon.transform.rotation = Quaternion.Euler(0, 0, (dirToRight ? -1f : 1f) * 90 - (dirToRight ? -angle : angle));
        weapon.transform.Find("AttackCollider").gameObject.SetActive(true);
        //weapon.GetComponent<Collider2D>().isTrigger = false;
        DetachWeapon();
    }

    //gameobject broni jest odczepiana od rodzica 
    public void DropWeapon()
    {
        weaponDurability.gameObject.SetActive(false);
        weapon.transform.parent = null;
        weapon.GetComponent<WeaponControler>().Clear();
        //anim.Rebind();//nie potrzebne?
        weapon.transform.position = throwPoint.transform.position;     
        weapon.GetComponent<Collider2D>().enabled = true;
        weapon.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        maxcombo = 2;
        weapon.layer = 10; 
    }

    public void DetachWeapon()
    {
        weapon = null;
    }

  /// <summary>
  /// Funkcje odpowiedzialne za ragdoll:
  /// </summary>
  // TODO: przenieść do osobnego pliku
    IEnumerator MoveToOrgPosition(Vector3 source, float overTime, Limb childObject, ComplateCoroutines complated)
    {
        float startTime = Time.time;
        while (Time.time < startTime + overTime)
        {
            childObject.ik.localPosition = Vector3.Lerp(source, childObject.orgPosition, (Time.time - startTime) / overTime);
            yield return null;
        }
        complated.num++;
    }

    IEnumerator StandUp(Limb[] limbs)
    {
        ComplateCoroutines complated = new ComplateCoroutines();
        //TODO: bardziej naturalnie
        //TODO:Ienumerator zeby nie powtrzało akcji
        //podnoszenie kręgosłupa w pierwszej kolejnosci
        foreach (Limb limb in limbs)
        {
            if(limb.ik.name == "Spine")
            {
                yield return StartCoroutine(MoveToOrgPosition(limb.ik.localPosition, 0.3f, limb, complated));
            }
        }
        foreach (Limb limb in limbs)
        {
            StartCoroutine(MoveToOrgPosition(limb.ik.localPosition, 0.3f, limb, complated));
        }
        while (complated.num<=limbs.Length)
        {
            yield return null;
        }
        anim.enabled = true;
    }


    public void ChangeState()
    {
        //zmiana stanu postaci z ragdoll do normalnego i odwrotnie
        if (ragdoll == false)
        {
            //gdy nie wyłaczone ik to zle wstawanie
            limbs[1].ik.gameObject.SetActive(false);
            anim.enabled = false;

            foreach (Rigidbody2D rig in rigs)
            {
                rig.bodyType = RigidbodyType2D.Dynamic;
            }

            limbs[0].ik.parent = null;
            limbs[0].ik.GetComponentInChildren<Rigidbody2D>().AddForce(ragdollForce * 10000, ForceMode2D.Force);
            ragdoll = true;
        }
        else if (ragdoll == true)
        {
            //tu można animacje wstawania, tylko podnoszenie kregosłupa
            anim.enabled = true;
            anim.SetTrigger("standUp");
            //zmieniam najpierw na static żeby zatrzymać siły działające na objekty
            limbs[0].ik.SetParent(gameObject.transform);
            limbs[1].ik.gameObject.SetActive(true);

            foreach (Rigidbody2D rig in rigs)
            {
                rig.bodyType = RigidbodyType2D.Static;
                rig.bodyType = RigidbodyType2D.Kinematic;
            }

           // StartCoroutine(StandUp(limbs));
           // anim.enabled = true;
            rgdBody.bodyType = RigidbodyType2D.Dynamic;

            ragdoll = false;
        }
    }
}