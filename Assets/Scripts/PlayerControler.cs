using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerControler : NetworkBehaviour {

    ////statystyki
    public int playerNum;
    public float heroSpeed;
    public float jumpForce;
    public int throwSpeed;
    public float maxHealth;
    [SyncVar(hook = "OnHealthChange")]
    public float currentHealth;
    ////klawisze
    public KeyCode left;
    public KeyCode right;
    public KeyCode jumpKey;
    public KeyCode down;
    public KeyCode attack;
    public KeyCode takeWeapon;
    ////testery
    public Transform groundTester;
    public LayerMask layersToTest;   
    private float radius = 0.1f;
    ////GUI
    public Image healthBar;
    public Image weaponDurability;
    ////komponenty
    public Animator anim;
    public Rigidbody2D rgdBody;
    public ComboManager comboManager;
    public Rigidbody2D[] rigs;
    public TrailRenderer[] trailEffect;
    public NetworkPlayerMovement networkPC;
    ////stany
    private bool canJump = false;
    public int dashWay = 0;
    public bool drop = false;
    public bool jump = false;
    //na potrzeby wyłączenia manewrów w pewnych okolicznościach
    private bool keysEnable = true;
    public bool attackEnable = true;
    //zmienna opisująca czy avatar został przygotowany do rozgrywki
    public bool networkSetUpDone;
    //czy sprite obrucony w prawo
    bool dirToRight = true;
    //przyjmuje wartości -1,0,1 w zależności od kierunku poruszania
    [SyncVar] public float horizontalMove;
    public bool ragdoll = false;
    //wyłącza możliwość kierowania postacią bota
    public bool AI = false;  
    public bool getDownFromPlatform = false;
    //platforma na której gracz stoi (w komendzie do serwera nie można przekazywać komponentów, więc w ten sposób)
    Collider2D platformBehind;
    ////walka
    public Transform throwPoint;
    //trzymana broń
    public GameObject weapon = null;
    //sekwencja ataków bez broni
    private int combo = 1;
    private int maxcombo = 2;
    private float lastAttackTime = 0;
    private float timeBetweenAttack = 0.3f;
    //siła w którą zostanie avatar popchnięty w chwili śmierci
    [SyncVar]private Vector2 ragdollForce;

    //zmienne opisujące ogłuszenie
    //funkcjonalność niedostępna w tej wersji gry z powodu niezadowalających sposobów na wstawanie postaci
    private struct Stun
    {
        public static float dmgToStun = 50f;
        public static float timeForDmg = 4f;
        public static float stunTime = 2f;
        public float lastTime;
        public float lastHealth;
    }
    private Stun stunControler;

    //struktura przechowująca kości i ik 
    //używana m.in. przy wstawaniu postaci (podnoszenie postaci niedostępna w tej wersji gry)
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


    IEnumerator WaitForAvatarSetUp()
    {
        yield return new WaitWhile(() => !networkSetUpDone);
        RigsSetUp();
    }

    void RigsSetUp()
    {
        rigs = GetComponentsInChildren<Rigidbody2D>();
        Transform[] iks = GetComponentInChildren<Transform>().Find("Skeleton").gameObject.GetComponentsInChildren<Transform>();
        limbs = new Limb[iks.Length];
        for (int i = 0; i < iks.Length; i++)
        {
            limbs[i] = new Limb(iks[i]);
        }
    }

    public override void OnStartClient()
    {
        currentHealth = maxHealth;
        StartCoroutine(WaitForAvatarSetUp());
    }

    //wykonywana przy każdej aktualizacji fizyki
    //wywoływane są tu manewry wpływające na Rigidbody2D
    private void FixedUpdate()
    {
        if (hasAuthority)
        {
            Move(horizontalMove);
            networkPC.CmdMove(horizontalMove);
            if (jump)
            {
                networkPC.CmdJump();
                Jump();
            }
            if (drop)
            {
                networkPC.CmdDropAttack();
                DropAttack();
            }
            if (dashWay != 0)
            {
                networkPC.CmdDash(0.1f, dashWay);
                StartCoroutine(Dash(0.1f, dashWay));
            }
        }
    }

    //wywoływana co klatkę
    void Update () {
        //uniemożliwienie poruszania się pokonanej postaci
        if (ragdoll)
        {
            return;
        }
        //na potrzeby synchronizacji animacji wykonywane na kazdej kopii
        canJump = Physics2D.OverlapCircle(groundTester.position, radius, layersToTest);
        IsGrounded();

        if (hasAuthority)
        {
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
            if (AI == false)
            {
                //postać na ziemi
                if (!anim.GetBool("InAir"))
                {
                    horizontalMove = keysEnable ? ((Input.GetKey(left) ? -1 : 0) + (Input.GetKey(right) ? 1 : 0)) : 0;
                    
                    ///atak
                    if (Input.GetKeyDown(attack) && !Input.GetKeyDown(takeWeapon))
                    {
                        networkPC.CmdBasicAttack();
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
                //rzut bronią
                if (Input.GetKeyDown(takeWeapon) && Input.GetKeyDown(attack) && (weapon != null) && keysEnable)
                {
                    networkPC.CmdThrow();
                    Throw();
                }
            }
        }
        ///sprawdza czy postac zyje
        if (currentHealth <= 0)
        {
            Die();
            GameManager.deadEvent.Invoke(playerNum);
        }
        /// odwracanie sprita postaci w lewo
        if (dirToRight && horizontalMove < 0)
        {
            Flip();
        }
        /// odwracanie sprita postaci w prawo
        if (!dirToRight && horizontalMove > 0)
        {
            Flip();
        }
        if (!anim.GetBool("InAir")) anim.SetFloat("speed", Mathf.Abs(horizontalMove));
    }


    //// Funkcje odpowiedzialne za kontrole postaci:

    public void Move(float way)
    {
       rgdBody.velocity = new Vector2(way * heroSpeed, rgdBody.velocity.y);       
    }

    //wykonanie podstawowowego ataku
    public void BasicAttack()
    {
        if (attackEnable)
        {
            //pomiędzy atakami jest przerwa pozwalająca skończyć animację
            if ( Time.time - lastAttackTime >= timeBetweenAttack)
            {
                //wybierany krok w sekwencji ataków
                combo = comboManager.Step(combo, maxcombo);
                //po wykonaniu całej sekwencji możliwość ataku zostaje na jakiś czas wyłączona
                if (combo == maxcombo) StartCoroutine(DisableAttack(1f));
                try
                {
                    anim.SetTrigger(weapon.name + "-attack" + combo);
                }
                catch
                {
                    anim.SetTrigger("NoWeapon-attack" + combo);
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
    public void ComeDown()
    {
        Physics2D.IgnoreCollision(platformBehind, GetComponent<Collider2D>());
        StartCoroutine(ReturnCollision(platformBehind, GetComponent<Collider2D>()));
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
            WeaponControler WC = weapon.GetComponent<WeaponControler>();
            //ustawienie inferfejsu broni
            WC.durabilityImage = weaponDurability;
            weaponDurability.gameObject.SetActive(true);
            weaponDurability.sprite = weapon.GetComponent<SpriteRenderer>().sprite;
            //dostosowanie broni
            WC.HandleWeapon(gameObject.transform);
            maxcombo = WC.maxcombo;
            //rebind poniewaz bron nie jest nieodłącznym elementem postaci i Animator niezawiera referencji do niej
            anim.Rebind();
            anim.SetBool(weapon.name, true);
            anim.SetBool("NoWeapon", false);
        }
    }
    
    //atak z powietrza
    public void DropAttack()
    { 
        rgdBody.velocity = new Vector2(0f, -20f);
        anim.SetTrigger("dropAttack");
        drop = false;
    }

    //zryw
    public IEnumerator Dash(float dashTime, float way)
    {
        //way działa analogicznie do horizontalMove
        float startTime = Time.time;
        //aby zryw został wykonanay raz
        dashWay = 0;
        //ograniczenie kontroli
        StartCoroutine(DisableKeys(dashTime));
        //dodatkowe efekty graficzne
        foreach (TrailRenderer trail in trailEffect)
        {
            trail.Clear();
            trail.gameObject.SetActive(true);
        }
        anim.SetBool("dash", true);
        //kontynuowanie/zmiana ruchu gdy postać w powietrzu
        horizontalMove = way;
        while (Time.time - startTime <= dashTime)
        {
            rgdBody.velocity = new Vector2(way * 50f, 0f);
            yield return null;
        }
        anim.SetBool("dash", false);
        foreach (TrailRenderer trail in trailEffect)
        {
            trail.gameObject.SetActive(false);
        }
    }

    //// Wykrywanie kolizji:

    private void OnTriggerStay2D(Collider2D collision)
    {
        //podnoszenie broni, czeka na kolizje z PickUpTrigger
        if (weapon == null && Input.GetKeyDown(takeWeapon) && !Input.GetKeyDown(attack) && collision.gameObject.tag == "Interaction")
        {
            if (hasAuthority)
            {
                networkPC.CmdTakeWeapon(collision.transform.parent.gameObject);
                TakeWeapon(collision.transform.parent.gameObject);
            }

        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //"złapanie" się ściany
        if (collision.gameObject.tag == "Wall" && anim.GetBool("InAir"))
        {
            anim.SetBool("wallstay",true);
            if (Input.GetKeyDown(jumpKey) && hasAuthority)
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
            platformBehind = collision.collider;
            if (Input.GetKeyDown(down) && keysEnable && hasAuthority)
            {
                networkPC.CmdComeDown();
                ComeDown();
            }
        }
    }

    //// Funkcje ustawiające/zmieniające stan postaci:

    //śmierć avatara
    public void Die()
    {
        if (weapon != null) DropWeapon();
        if (!ragdoll) ChangeState();
    }

    //ograniczenie możliwości kontroli
    IEnumerator DisableKeys(float time)
    {
        keysEnable = false;
        yield return new WaitForSeconds(time);
        keysEnable = true;     
    }

    //ograniczenie możliwości ataku
    IEnumerator DisableAttack(float time)
    {
        attackEnable = false;
        yield return new WaitForSeconds(time);
        attackEnable = true;
    }

    //po zejściu z platformy, przywraca kolizję
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

    // w objekcie postaci zmienia wartosc Transform>Scale>x aby odwrócić sprite w drugą stronę
    void Flip()
    {
        dirToRight = !dirToRight;
        gameObject.transform.localScale = new Vector2(-gameObject.transform.localScale.x, gameObject.transform.localScale.y);
    }

    //zmniejsza ilość życia, wykonywane tylko na serwerze
    public void ReduceHealth(int dmg)
    {
        anim.SetTrigger("TakeHit");
        currentHealth = currentHealth - dmg;
        Debug.Log("health - " + currentHealth);
    }

    //nieobecne w ostatecznej wersji gry
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

    //zapisanie siły która będzie działać na postać w przypadku śmierci
    public void SaveForce(Vector2 f)
    {
        ragdollForce = f;
    }

    //event wywoływany przez animacje, rzuca bronią
    void ThrowEvent(float angle)
    {
        DropWeapon();
        weapon.GetComponent<WeaponControler>().AddToArena();
        //nadanie obiektowi siły
        weapon.GetComponent<Rigidbody2D>().AddForce(new Vector2(throwSpeed * transform.localScale.x, angle), ForceMode2D.Impulse);
        //zwrucenie broni ostrzem w stronę w którą zwrucony jest avatar
        weapon.transform.rotation = Quaternion.Euler(0, 0, (dirToRight ? -1f : 1f) * 90 - (dirToRight ? -angle : angle));
        weapon.transform.Find("AttackCollider").gameObject.SetActive(true);
        DetachWeapon();
    }

    //gameobject broni jest odczepiana od rodzica 
    public void DropWeapon()
    {
        weapon.GetComponent<WeaponControler>().Clear();
        weaponDurability.gameObject.SetActive(false);
        weapon.transform.parent = null;
        anim.Rebind();
        weapon.transform.position = throwPoint.transform.position;    
        //zmiana stanu broni, może zaatakować lecz jeszcze nie można jej podnieść
        weapon.GetComponent<Collider2D>().enabled = true;
        weapon.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        maxcombo = 2;
        weapon.layer = 10;
    }

    //wykonywane na końcu "odczepienia" broni gdy już zostały zrealizowane wszystkie modyfikacje
    public void DetachWeapon()
    {
        weapon = null;
    }

    //// Funkcje odpowiedzialne za ragdoll:

    //nieobecne w ostatecznej wersji gry
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

    //nieobecne w ostatecznej wersji gry
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

    //uśmiercenie avatara
    public void ChangeState()
    {
        if (ragdoll == false)
        {
            //wyłączenie kinematyki odwrotnej, obiekty za nią odpowiadające przeszkadzają w swobodnym upadku
            limbs[1].ik.gameObject.SetActive(false);
            anim.enabled = false;

            //umożliwia wpływ fizyki silnika na każdą część ciała avatara
            foreach (Rigidbody2D rig in rigs)
            {
                rig.bodyType = RigidbodyType2D.Dynamic;
            }

            limbs[0].ik.parent = null;
            //wyrzut avatara
            limbs[0].ik.GetComponentInChildren<Rigidbody2D>().AddForce(ragdollForce * 10000, ForceMode2D.Force);
            
            ragdoll = true;
        }
        /*podnoszenie postaci
        else if (ragdoll == true)
        {
            limbs[0].ik.SetParent(gameObject.transform);
            limbs[1].ik.gameObject.SetActive(true);

            foreach (Rigidbody2D rig in rigs)
            {
                rig.bodyType = RigidbodyType2D.Kinematic;
            }

            //StartCoroutine(StandUp(limbs));
            rgdBody.bodyType = RigidbodyType2D.Dynamic;
            anim.enabled = true;

            foreach (Limb limb in limbs)
            {
                limb.ik.transform.localPosition = limb.orgPosition;
            }
            ragdoll = false;
        }*/
    }

    //przyłączenie częsci avatara odpadającej w chwili śmierci
    public void AssingSkeleton()
    {
        limbs[0].ik.SetParent(gameObject.transform);
    }

    //synchronizacja zdrowia
    void OnHealthChange(float health)
    {
        currentHealth = health;
        if(networkSetUpDone)healthBar.fillAmount = currentHealth / maxHealth;
    }

}