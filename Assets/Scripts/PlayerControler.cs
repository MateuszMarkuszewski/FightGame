using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public KeyCode attack;
    public KeyCode throwWeapon;
    //testery
    public Transform groundTester;
    public Transform wallTester;
    public LayerMask layersToTest;
    public Transform throwPoint;

    public int health;

    Animator anim;
    bool dirToRight = true;
    Rigidbody2D rgdBody;
    ComboManager comboManager;
    Rigidbody2D[] rigs;

    private float radius = 0.1f;
    private bool onTheGround;
    private bool onTheWall;
    private GameObject weapon;

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
        if( ragdoll == true)
        {
            transform.position = new Vector3(limbs[1].ik.position.x,transform.position.y, transform.position.z);

        }
        //czy postać dotyka ziemi
        onTheGround = Physics2D.OverlapCircle(groundTester.position, radius, layersToTest);
        //onTheWall = (Physics2D.OverlapBox(wallTester.position, new Vector2(0.9f, 0.0f), 0.0f, layersToTest) && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)));
        //onTheWall = Physics2D.OverlapBox(wallTester.position, new Vector2(0.9f, 0.0f), 0.0f, layersToTest);
        onTheWall = Physics2D.OverlapCircle(wallTester.position, radius, layersToTest);
        float horizontalMove = ((Input.GetKey(left) ? -1 : 0) + (Input.GetKey(right) ? 1 : 0));
        rgdBody.velocity = new Vector2(horizontalMove * heroSpeed, rgdBody.velocity.y);
        /*
        if (comboManager.DoubleClick(left) || comboManager.DoubleClick(right))
        {
            rgdBody.velocity = new Vector2(horizontalMove * 10 * heroSpeed, rgdBody.velocity.y);
        }
        */
///atak
        if (Input.GetKeyDown(attack))
        {
            ChangeState();
            //anim.SetTrigger("attack");
        }
        
/// skakanie
        if(Input.GetKeyDown(jump) && (onTheGround || onTheWall))
        {
            rgdBody.AddForce(new Vector2(0f, jumpForce));
            anim.SetTrigger("jump");
        }

        anim.SetFloat("speed", Mathf.Abs(horizontalMove));

/// odwracanie sprita postaci w lewo
        if(horizontalMove < 0 && dirToRight)
        {
            Flip();
        }

/// odwracanie sprita postaci w prawo
        if (horizontalMove > 0 && !dirToRight)
        {
            Flip();
        }
        
///rzut bronią
        if (Input.GetKeyDown(throwWeapon) && (weapon != null))
        {
            Throw();
        }
///sprawdza czy postac zyje
        if (health <= 0)
        {
            gameObject.SetActive(false);
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
        weapon.SetActive(true);
        transform.Find(weapon.name).parent = null;
        weapon.transform.rotation = Quaternion.Euler(0, 0, (dirToRight ? -1f : 1f) * 90f);
        weapon.transform.position = throwPoint.transform.position;
        weapon.GetComponent<Rigidbody2D>().velocity = new Vector2(throwSpeed * transform.localScale.x, 0);
       //weapon.GetComponent<Rigidbody2D>().AddForceAtPosition(new Vector2(throwSpeed * transform.localScale.x, 0), throwPoint.transform.position, ForceMode2D.Impulse);
        weapon = null;
    }

    void TakeWeapon(GameObject w)
    {
        weapon = w;
    }

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