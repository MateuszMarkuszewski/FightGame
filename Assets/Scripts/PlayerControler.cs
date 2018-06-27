using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

//TODO: rgdbody.getPoint do AI

public class PlayerControler : MonoBehaviour {

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
    public Transform wallTester;
    public LayerMask layersToTest;
    public Transform throwPoint;
    private float horizontalMove;

    public int health;

    Animator anim;
    bool dirToRight = true;
    Rigidbody2D rgdBody;
    ComboManager comboManager;
    Rigidbody2D[] rigs;

    private float radius = 0.1f;
    private bool onTheGround;
    private bool onTheWall;

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

    void Update () {

        if(ragdoll == true)
        {
            transform.position = new Vector3(limbs[1].ik.position.x,transform.position.y, transform.position.z);

        }
        //czy postać dotyka ziemi
        //onTheWall = (Physics2D.OverlapBox(wallTester.position, new Vector2(0.9f, 0.0f), 0.0f, layersToTest) && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)));
        //onTheWall = Physics2D.OverlapBox(wallTester.position, new Vector2(0.9f, 0.0f), 0.0f, layersToTest);
        onTheWall = Physics2D.OverlapCircle(wallTester.position, radius, layersToTest);



        /*
        if (comboManager.DoubleClick(left) || comboManager.DoubleClick(right))
        {
            rgdBody.velocity = new Vector2(horizontalMove * 10 * heroSpeed, rgdBody.velocity.y);
        }
        */

        //postać na ziemi
        if (IsGrounded() || rgdBody.velocity.y == 0)
        {
            horizontalMove = ((Input.GetKey(left) ? -1 : 0) + (Input.GetKey(right) ? 1 : 0));
            anim.SetFloat("speed", Mathf.Abs(horizontalMove));
            ///atak
            if (Input.GetKeyDown(attack))
            {
                combo = comboManager.Step(combo, maxcombo);
                try
                {
                    Debug.Log(weapon.name + "-anim" + combo);
                    anim.SetTrigger(weapon.name + "-anim" + combo);
                }
                catch (NullReferenceException)
                {
                    anim.SetTrigger("NoWeapon-anim" + combo);
                }
            }

            /// skakanie
            if (Input.GetKeyDown(jump))
            {
                rgdBody.velocity = new Vector2(rgdBody.velocity.x, jumpForce);
                //rgdBody.AddForce(new Vector2(0f, jumpForce));
                anim.SetTrigger("jump");
            }
       
            ///rzut bronią
            if (Input.GetKeyDown(throwWeapon) && (weapon != null))
            {
                Throw();
            }
            rgdBody.velocity = new Vector2(horizontalMove * heroSpeed, rgdBody.velocity.y);

        }
        //postać w powierzu
        else
        {
            rgdBody.velocity = new Vector2(horizontalMove * heroSpeed, rgdBody.velocity.y);
        }
        //rgdBody.AddForce(new Vector2(horizontalMove * heroSpeed, rgdBody.velocity.y));
        //tu moge zapisac wektor https://www.youtube.com/watch?v=EOSjfRuh7x4
        //Debug.Log(rgdBody.velocity);


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
        if (health <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall" && !IsGrounded())
        {
            if (Input.GetKeyDown(jump))
            {
                Debug.Log("jump");
                horizontalMove = collision.contacts[0].normal.x;
                rgdBody.velocity = new Vector2(rgdBody.velocity.x, jumpForce);
                anim.SetTrigger("jump");
            }
        }
        if (collision.gameObject.tag == "Platform")
        {
            if (Input.GetKeyDown(down))
            {
                Physics2D.IgnoreCollision(collision.collider, gameObject.GetComponent<Collider2D>());
                StartCoroutine(ReturnCollision(collision.collider, gameObject.GetComponent<Collider2D>()));
            }
        }
    }

    IEnumerator ReturnCollision(Collider2D coll1, Collider2D coll2)
    {
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(coll1, coll2, false);
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundTester.position, radius, layersToTest);
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
        health = health - dmg;
        Debug.Log(gameObject.name);
        Debug.Log(health);
    }

    //rzut podniesioną bronią
    void Throw()
    {
        anim.SetTrigger("throw");
        DropWeapon();
        weapon.transform.rotation = Quaternion.Euler(0, 0, (dirToRight ? -1f : 1f) * 90f);
        weapon.transform.position = throwPoint.transform.position;
        weapon.GetComponent<Rigidbody2D>().velocity = new Vector2(throwSpeed * transform.localScale.x, 0);
       //weapon.GetComponent<Rigidbody2D>().AddForceAtPosition(new Vector2(throwSpeed * transform.localScale.x, 0), throwPoint.transform.position, ForceMode2D.Impulse);
        weapon = null;
    }

    //funkcja wywolywana przez PickUpControler przy zetknieciu z postacią
    void TakeWeapon(GameObject w)
    {
        if(weapon == null)
        {
            weapon = w;
            weapon.SendMessage("HandleWeapon", gameObject.transform);
            maxcombo = weapon.GetComponent<WeaponControler>().maxcombo;
            anim.Rebind();
        }    
    }

    //gameobject broni jest odczepiana od rodzica 
    void DropWeapon()
    {
        weapon.GetComponent<Collider2D>().enabled = true;
        transform.Find(weapon.name).parent = null;
        maxcombo = 2;
        weapon.layer = 10;
        //anim.Rebind();
    }


    ///////////////////////////////////////////////////////ragdoll
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