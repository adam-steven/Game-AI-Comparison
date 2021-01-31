using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFiniteStateMachine : EnemyStateMachine {

	/* PUBLIC ACCESS VARIABLES */
	public float patrolSpeed = 1f;			// Base patrol speed
	public float maxSpeed = 8f;				// Base maximum move speed
	public float triggerRange = 7.5f;		// Range from enemy to player to trigger from 'Patrol' to 'Chase' state
	public GameObject groundCheck;			// Object that checks for the ground
	public LayerMask whatIsGround;			// Describes what the ground is (what counts as the ground)
	public GameObject stateBanner;			// GameObject that represents the State of the AI

	// Textures for the StateBanner Gameobject. Maybe replace with a dynamic loading method
	public Sprite chaseSprite;				
	public Sprite patrolSprite;
	public Sprite deadSprite;
	public Sprite waitSprite;
	public Sprite examineSprite;

	/* PRIVATE VARIABLES */
	private bool facingRight = true;				
	private Animator anim;
	private bool movementAllowed = true;
	private float speedMultiplier = 1f;				// Speed multiplier changes dependant on state
	private bool grounded = false;		
	private float groundRadius = 0.2f;				// Check for range to check for ground
	private SpriteRenderer bannerSpriteRenderer;	// Renderer for the banner (2D UI element)
	private GameObject player;						// Handle on the player GameObject so we can get their location

	// Use this for initialization
	void Start () {
		// Initialise the starting objects
		anim = GetComponent<Animator>();
		player = GameObject.Find("Player");
		bannerSpriteRenderer = stateBanner.GetComponent<SpriteRenderer>();
	}

	public void StopBots()
	{
		// Stop the bots from being able to move and put them into the 'Wait' state
		// There is a lot of stuff to stop but it covers all the bases
		// Alternatively, make it go back into 'Patrol' mode
		maxSpeed = 0f;
		patrolSpeed = 0f;
		triggerRange = 0f;
		movementAllowed = false;
		Rigidbody2D rigidBodyComp = this.GetComponent<Rigidbody2D>();
		rigidBodyComp.velocity = new Vector2(0f, rigidBodyComp.velocity.y);
		anim.SetFloat("speed", 0f);
		ChangeState(State.Wait);
	}

	// Called at set interval each time. Good for physics
	void FixedUpdate()
	{
		// Do normal movement
		BaseMovementAndAnimation();

		// Extra movement details for specific states
		switch(aiState)
		{
		case State.Chase:
			ChasePlayer(3f);
			break;
		case State.Patrol:
			PerformPatrol();
			break;
		default:
			break;
		}
	}

	// Called once when we swap to a new state
	protected override void StartNewState(State currentState)
	{
		// Depending on the state, use the appropriate banner sprite
		switch(currentState)
		{
		case State.Patrol:
			bannerSpriteRenderer.sprite = patrolSprite;
			break;
		case State.Chase:
			bannerSpriteRenderer.sprite = chaseSprite;
			break;
		case State.Dead:
			Dead();
			bannerSpriteRenderer.sprite = deadSprite;
			break;
		case State.Wait:
			bannerSpriteRenderer.sprite = waitSprite;
			break;
		default:
			break;	
		}
	}

	private void BaseMovementAndAnimation()
	{
		// Get the attached Rigidbody component
		// This is used for almost all physiscs calculations
		Rigidbody2D rigidBodyComp = GetComponent<Rigidbody2D>();

		// Check if we are touching the ground. This makes a check from the 'groundCheck' GameObject for a specific 
		// radius looking for GameObjects in the entire scene that match those in the 'whatIsGround' LayerMask
		grounded = Physics2D.OverlapCircle(groundCheck.transform.position, groundRadius, whatIsGround);

		// Update the animation (falling has a specific animation)
		anim.SetBool("ground", grounded);

		// Set the speed variable of the animation (used for normal movement but its cheaper to assign without checking)
		anim.SetFloat("vSpeed", rigidBodyComp.velocity.y);

		// Flip the Enemy if we need to
		if((facingRight && (rigidBodyComp.velocity.x < 0f)) || (!facingRight && (rigidBodyComp.velocity.x > 0f)))
		{
			FlipTransform();
		}

		// Perform Movement if we can
		if(movementAllowed)
		{
			PerformMovement(isState(State.Chase) ? maxSpeed : patrolSpeed);
		}

		// When we fall out of the world, change to the 'Dead' state
		if(rigidBodyComp.position.y <= -5.5f)
		{
			ChangeState(State.Dead);
		}
	}
		
	void PerformPatrol()
	{
		// Create an offset for calculating if the AI needs to turn around
		Vector3 offset = new Vector3(patrolSpeed, 0f);

		// Check if the ground is going to be where it will move to - Flip movement if its near the edge
		bool grounded = Physics2D.OverlapCircle(groundCheck.transform.position + offset, groundRadius, whatIsGround);
		if(!grounded)
		{
			FlipMovement();
		}

		// If the player enters the trigger range - Change to CHASE state
		if((this.transform.position - player.transform.position).magnitude < triggerRange)
		{
			ChangeState(State.Chase);
		}
	}

	void ChasePlayer(float newSpeedMultiplier)
	{
		// Flip movement if the player moves behind them
		float horizontalDifference = this.transform.position.x - player.transform.position.x;
		if(((maxSpeed > 0f) && (horizontalDifference > 0f)) || ((maxSpeed < 0f) && (horizontalDifference < 0f)))
		{
			speedMultiplier = newSpeedMultiplier;
			FlipMovement();
		}

		// If the player leaves the triggerRange (this is extended by 50% when in CHASE state) return to PATROL state
		if((this.transform.position - player.transform.position).magnitude > triggerRange*1.5f)
		{
			ChangeState(State.Patrol);
		}
	}

	void FlipMovement()
	{
		// Flip the patrol and maximum speeds
		patrolSpeed *= -1;
		maxSpeed *= -1;
	}

	void FlipTransform()
	{ 
		speedMultiplier = 1f;

		// Flip the object so it faces the opposite direction
		facingRight = !facingRight;
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;

		// Flip the banner so it faces forwards
		Vector3 bannerScale = stateBanner.transform.localScale;
		bannerScale.x *= -1;
		stateBanner.transform.localScale = bannerScale;
	}

	void PerformMovement(float speed)
	{
		// Calculate new velocity and clamp it to maximum values
		float absSpeed = Mathf.Abs(speed);
		Rigidbody2D rigidBodyComp = GetComponent<Rigidbody2D>();
		float newXVelocity = rigidBodyComp.velocity.x + speed*speedMultiplier*Time.fixedDeltaTime;
		rigidBodyComp.velocity = new Vector2(Mathf.Clamp(newXVelocity, -absSpeed, absSpeed), rigidBodyComp.velocity.y);

		// Update the moving animation
		anim.SetFloat("speed", speed/maxSpeed);
	}

	private void Dead()
	{
		// Perform death animation
		anim.SetBool("dead", true);

		// Stop gravity affecting enemy
		this.GetComponent<Rigidbody2D>().gravityScale = 0f;

		// Remove the collider
		Destroy(this.GetComponent<Collider2D>());

		// Stop movement
		movementAllowed = false;
	}
}