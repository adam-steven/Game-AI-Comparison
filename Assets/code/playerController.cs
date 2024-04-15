using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [HideInInspector]
    public int startingHealth;
    public int health;
    public int speed = 30;

    public GameObject sprite;
    private SpriteRenderer spriteRender;
    private Animator anim;

    public GameObject gun;
    public GameObject gunHoldPoint;

    [HideInInspector]
    public GameObject currentRoom;
    private int inWait = 0;
    public GameObject punchBullet;


    void Start()
    {
        anim = sprite.gameObject.GetComponent<Animator>();
        spriteRender = sprite.GetComponent<SpriteRenderer>();
        startingHealth = health;
    }

    void FixedUpdate()
    {
        spriteRender.color = Color.Lerp(Color.white, Color.red, (1f - ((float)health / (float)startingHealth)));
        if(health <= 0)
            Destroy(this.gameObject);


        //movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        transform.Translate(movement * speed * Time.deltaTime);

        //rotation
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Quaternion rot = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);
        sprite.transform.rotation = rot;
        sprite.transform.eulerAngles = new Vector3(0, 0, sprite.transform.eulerAngles.z);

        //animation
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            anim.SetBool("walking", true);
        else
            anim.SetBool("walking", false);

        if (Input.GetButtonDown("Drop"))
            gun = null;

        if (Input.GetButton("Fire1") && !gun)
        {
            anim.SetBool("punch", true);
            PunchPlayer();
        }
        else
        {
            anim.SetBool("punch", false);
        }

        if (!gun)
            anim.SetInteger("gun", 0);
        else
            anim.SetInteger("gun", gun.GetComponent<gunScript>().gunHoldNum);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("room shadow"))
        {
            currentRoom = collision.gameObject;
        }
    }

    IEnumerator Punch()
    {
        yield return new WaitForSeconds(0.3f);
        inWait = 2;
    }

    private void PunchPlayer()
    {
        if (inWait == 0)
        {
            StartCoroutine(Punch());
            inWait = 1;
        }
        else if (inWait == 2)
        {
            bulletScript shotBullet = Instantiate(punchBullet, sprite.transform.position, sprite.transform.rotation).GetComponent<bulletScript>();
            shotBullet.damage = 1;
            shotBullet.speed = 1;
            shotBullet.shake = 0;
            inWait = 0;
        }
    }
}