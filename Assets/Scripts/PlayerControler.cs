using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: rgdbody.getPoint do AI

public class PlayerControler : MonoBehaviour {

    public bool AI = false;
    //fizyka postaci
    public float heroSpeed;
    public float jumpForce;
    public int throwSpeed;
    private bool ragdoll = false;
    //klawisze
    public KeyCode left;
    public KeyCode right;
    public KeyCode jump;
    public KeyCode down;
    public KeyCode attack;
    public KeyCode throwWeapon;
    //testery
    public Transform groundTester;
    public LayerMask layersToTest;
    public Transform throwPoint;
    public TrailRenderer[] trailEffect;
    private float horizontalMove;

    public float maxHealth;
    private float currentHealth;
    public Image healthBar;
    public Image weaponDurability;

    Animator anim;
    bool dirToRight = true;
    Rigidbody2D rgdBody;
    ComboManager comboManager;
    Rigidbody2D[] rigs;

    private float radius = 0.1f;
    private bool onTheGround;
    private bool onTheWall;
    private bool keysEnable = true;
    private bool attackEnable = true;

    //zmienne służące do ataków
    private GameObject weapon = null;
    private int combo = 1;
    private int maxcombo = 2;

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
    }
    private void FixedUpdate()
    {

        /*
        if (comboManager.DoubleClick(left) || comboManager.DoubleClick(right))
        {
            Debug.Log("dash");
            StartCoroutine(Dash(0.1f, dirToRight ? 1 : -1));
            StartCoroutine(DisableKeys(0.1f));
        }*/
        
        if (Input.GetKeyDown(attack) && keysEnable && horizontalMove != 0)
        {
            //rgdBody.velocity = new Vector2(10f, 0f);
 
            //rgdBody.AddForce(new Vector2( 20000f, 0f), ForceMode2D.Impulse);

        }
    }




    void Update () {

        if(ragdoll == true)
        {
            transform.position = new Vector3(limbs[1].ik.position.x,transform.position.y, transform.position.z);
        }

        if (comboManager.DoubleClick(left))
        {
            StartCoroutine(Dash(0.1f, -1));
        }
        if (comboManager.DoubleClick(right))
        {
            StartCoroutine(Dash(0.1f, 1));
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
                if (Input.GetKeyDown(attack) && !Input.GetKeyDown(throwWeapon) && attackEnable)
                {
                    BasicAttack();
                }

                /// skakanie
                if (Input.GetKeyDown(jump) && keysEnable)
                {
                    Jump();
                }


                Move(horizontalMove);
            }
            //postać w powierzu
            else
            {
                //odbijanie sie od platformy
                if (Input.GetKeyDown(jump) && keysEnable && Physics2D.OverlapCircle(groundTester.position, radius, layersToTest))
                {
                    horizontalMove = keysEnable ? ((Input.GetKey(left) ? -1 : 0) + (Input.GetKey(right) ? 1 : 0)) : 0;
                    Jump();
                }

                //stała predkosc w locie
                Move(horizontalMove);

                //atak z powietrza
                if (Input.GetKeyDown(attack) && !Input.GetKeyDown(throwWeapon) && keysEnable)
                {
                    DropAttack();
                }
            }
            //tu moge zapisac wektor https://www.youtube.com/watch?v=EOSjfRuh7x4
            //Debug.Log(rgdBody.velocity);

            if (Input.GetKeyDown(throwWeapon) && Input.GetKeyDown(attack) && (weapon != null) && keysEnable)
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
            ChangeState();
        }
    }

    /// <summary>
    /// Funkcje odpowiedzialne za kontrole postaci:
    /// </summary>
    /// 



    public void Move(float way)
    {
        rgdBody.velocity = new Vector2(way * heroSpeed, rgdBody.velocity.y);
    }

    //wykonanie podstawowowego ataku
    public void BasicAttack()
    {
        combo = comboManager.Step(combo, maxcombo);
        if (combo == maxcombo) StartCoroutine(DisableAttack(1f));
        try
        {
            anim.SetTrigger(weapon.name + "-attack" + combo);
        }
        catch (NullReferenceException)
        {
            anim.SetTrigger("NoWeapon-attack" + combo);
        }
    }

    //skok
    public void Jump()
    {
        rgdBody.velocity = new Vector2(rgdBody.velocity.x, jumpForce);
        anim.SetTrigger("jump");
    }

    //zejscie z platformy
    public void ComeDown(Collider2D collider)
    {
        Physics2D.IgnoreCollision(collider, gameObject.GetComponent<Collider2D>());
        StartCoroutine(ReturnCollision(collider, gameObject.GetComponent<Collider2D>()));
    }

    //rzut podniesioną bronią
    void Throw()
    {
        Debug.Log("throw");
        anim.SetTrigger("throw");
    }

    //podniesienie broni
    void TakeWeapon(GameObject w)
    {
        if (weapon == null)
        {
            weapon = w;
            weapon.GetComponent<WeaponControler>().durabilityImage = weaponDurability;
            weapon.GetComponent<WeaponControler>().player = gameObject;
            weaponDurability.gameObject.SetActive(true);
            weapon.SendMessage("HandleWeapon", gameObject.transform);
            maxcombo = weapon.GetComponent<WeaponControler>().maxcombo;
            anim.Rebind();
            anim.SetBool(weapon.name, true);
            anim.SetBool("NoWeapon", false);
        }
    }

    void DropAttack()
    {
        anim.SetTrigger("dropAttack");
        rgdBody.velocity = new Vector2(0f, -20f);
    }

    IEnumerator Dash(float dashTime, float way)
    {
        StartCoroutine(DisableKeys(dashTime));
        foreach (TrailRenderer trail in trailEffect)
        {
            trail.Clear();
            trail.gameObject.SetActive(true);
        }
        Debug.Log("dash");
        anim.SetBool("dash", true);
        horizontalMove = way;
        while (0 <= dashTime)
        {
            dashTime -= Time.deltaTime;
            rgdBody.velocity = new Vector2(way * 30f, 0f);
            yield return null;
        }
        foreach (TrailRenderer trail in trailEffect)
        {
            trail.gameObject.SetActive(false);
        }
        rgdBody.velocity = new Vector2(0f, 0f);
        anim.SetBool("dash", false);
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
        if (weapon == null && Input.GetKeyDown(throwWeapon) && !Input.GetKeyDown(attack) && collision.gameObject.tag == "Interaction")
        {
            TakeWeapon(collision.transform.parent.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall" && anim.GetBool("InAir"))
        {
            anim.SetBool("wallstay",true);
            if (Input.GetKeyDown(jump))
            {
                anim.SetBool("wallstay", false);
                Debug.Log("jump");
                horizontalMove = collision.contacts[0].normal.x;
                Jump();
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
        if(Physics2D.OverlapCircle(groundTester.position, radius, layersToTest) && anim.GetBool("InAir") && rgdBody.velocity.y == 0)
        {
            anim.SetBool("InAir", false);
        }else if(!Physics2D.OverlapCircle(groundTester.position, radius, layersToTest) && !anim.GetBool("InAir"))
        {
            anim.SetBool("InAir", true);
        }
    }

    /// w objekcie postaci zmienia wartosc Transform>Scale>x aby odwrócić sprite w drugą stronę
    void Flip()
    {
        dirToRight = !dirToRight;
        Vector3 heroScale = gameObject.transform.localScale;
        heroScale.x *= -1;
        gameObject.transform.localScale = heroScale;
    }

    //zmniejsza HP
    void DealDamage(int dmg)
    {
        currentHealth = currentHealth - dmg;
        healthBar.fillAmount = currentHealth / maxHealth;
        Debug.Log(gameObject.name);
        Debug.Log(currentHealth);
    }

    //event wywoływany przez animacje
    void ThrowEvent(float angle)
    {
        DropWeapon();
        weapon.SendMessage("ClearUI");
        weapon.GetComponent<Rigidbody2D>().AddForce(new Vector2(throwSpeed * transform.localScale.x, angle), ForceMode2D.Impulse);
        weapon.transform.rotation = Quaternion.Euler(0, 0, (dirToRight ? -1f : 1f) * 90 - (dirToRight ? -angle : angle));
        weapon.transform.Find("AttackCollider").gameObject.SetActive(true);
        weapon.GetComponent<Collider2D>().isTrigger = false;
        DistachWeapon();
    }

    //gameobject broni jest odczepiana od rodzica 
    void DropWeapon()
    {
        weaponDurability.gameObject.SetActive(false);
        weapon.transform.parent = null;
        anim.Rebind();
        weapon.transform.position = throwPoint.transform.position;     
        weapon.GetComponent<Collider2D>().enabled = true;
        weapon.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        maxcombo = 2;
        weapon.layer = 10; 
    }

    void DistachWeapon()
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
            
            anim.enabled = false;
            rgdBody.bodyType = RigidbodyType2D.Kinematic;
            foreach (Rigidbody2D rig in rigs)
            {
                rig.bodyType = RigidbodyType2D.Dynamic;
            }
            limbs[0].ik.parent = null;
            ragdoll = true;
        }
        else if (ragdoll == true)
        {
            //zmieniam najpierw na static żeby zatrzymać siły działające na objekty
            limbs[0].ik.SetParent(gameObject.transform);

            foreach (Rigidbody2D rig in rigs)
            {
                rig.bodyType = RigidbodyType2D.Static;
                rig.bodyType = RigidbodyType2D.Kinematic;
            }

            StartCoroutine(StandUp(limbs));

            rgdBody.bodyType = RigidbodyType2D.Dynamic;

            //anim.enabled = true;

            ragdoll = false;
        }
    }

}