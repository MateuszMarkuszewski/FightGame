using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControler : MonoBehaviour {

    //fizyka postaci
    public float heroSpeed;
    public float jumpForce;
    public int throwSpeed;
    private bool ragdoll = true;
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

    void Start ()
    {
        anim = GetComponent<Animator>();
        rgdBody = GetComponent<Rigidbody2D>();
        comboManager = GetComponent<ComboManager>();
        rigs = GetComponentsInChildren<Rigidbody2D>();
    }

    void Update () {
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
            anim.enabled = false;
            foreach(Rigidbody2D rig in rigs){
                rig.bodyType = RigidbodyType2D.Dynamic;
            }
          //  anim.SetTrigger("attack");
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

}