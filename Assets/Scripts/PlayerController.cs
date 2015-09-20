using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;	// for Flags

/// <summary>
/// The player controller will be the go-between between player input, its underlying data in PlayerModel, and the GameController.
/// It handles events from the input controller, and passes events to the GameController.
/// It also handles player state.
/// </summary>
public class PlayerController : MonoBehaviour, GameStateSubscriber, PlayerInputEventHandler
{
	[SerializeField]
	private float maxSpeed;	// TODO - Move this to the Model

	[SerializeField]
	private Sprite idleSprite;
	[SerializeField]
	private Sprite movingForwardSprite;
	[SerializeField]
	private Sprite movingBackwardSprite;
	[SerializeField]
	private Sprite jumpingSprite;
	[SerializeField]
	private Sprite fallingSprite;
	[SerializeField]
	private Sprite crouchingSprite;

	#region Lifecycle Methods

	void Awake()
	{
		GameController.sharedInstance.SubscribeToGameStateChanges(this);
		collider = GetComponent<BoxCollider2D>();
		model = GetComponent<PlayerModel>();
		rigidBody = GetComponent<Rigidbody2D>();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		LimitTopSpeedIfNeeded();
	}

	#endregion Lifecycle Methods

	#region Input Event Handler Methods

	public void ReceivedInputCombos(List<InputCombo> inputCombos)
	{
		foreach (InputCombo input in inputCombos) {
			if ((input & InputCombo.Back) == InputCombo.Back) {
				DidReceiveMovementCommand(Vector2.left);	// TODO - Change this to `.right` depending on which way the player is facing.
			}
			if ((input & InputCombo.Forward) == InputCombo.Forward) { 
				DidReceiveMovementCommand(Vector2.right);	// TODO - Change this to `.left` depending on which way the player is facing.
			}
			if ((input & InputCombo.Up) == InputCombo.Up) {
				DidReceiveJumpCommand();
			}
			if ((input & InputCombo.Down) == InputCombo.Down) {
				DidReceiveCrouchCommand();
			}
			if ((input & InputCombo.None) == InputCombo.None) {
				DidReceiveNoInput();
			}
			// TODO - Handle other inputs
		}
	}

	private void DidReceiveMovementCommand(Vector2 direction)
	{
		float movementSpeed = model.movementSpeed;
		MoveInDirection(direction, movementSpeed);
		playerState |= PlayerState.Moving;
	}

	private void DidReceiveJumpCommand()
	{
		float jumpHeight = model.jumpHeight;
		float jumpSpeed = model.jumpSpeed;
		Jump(jumpHeight, jumpSpeed);
		playerState |= PlayerState.Jumping;
	}

	private void DidReceiveCrouchCommand()
	{
		playerState |= PlayerState.Crouching;
	}

	private void DidReceiveNoInput()
	{
		// The player didn't do anything this frame. If they were jumping, they are now falling. If they were doing anything else, they are now idle.
		if ((playerState & PlayerState.Jumping) == PlayerState.Jumping) {
			playerState = PlayerState.Falling;
		}
		else {
			playerState = PlayerState.Idle;
		}
	}
	// DidReceiveAttackCommand(attackType)
		// attack = model.attackOfType(attackType)
		// doAttack(attack), which will trigger the animation, and apply any of the movement vectors contained within `attack`
		// playerState |= .Attacking
	// 

	#endregion Input Event Handler Methods

	#region Movement Methods

	private void MoveInDirection(Vector2 direction, float movementSpeed) 
	{
		// Apply a force in the given direction to create the movement.
		rigidBody.AddForce(direction.normalized * movementSpeed);
	}

	private void Jump(float jumpHeight, float jumpSpeed) {
			// lerp sprite position at speed
	}
	// void Attack(Attack attack) {
	//		// do attack animation
	//		// apply attack.movement
	//		// at end of attack, remove .Attack state
	// }

	private void LimitTopSpeedIfNeeded()
	{
		// Limit top speed.
		if (rigidBody.velocity.x > maxSpeed) {
			rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
		}
		else if (rigidBody.velocity.x < -maxSpeed) {
			rigidBody.velocity = new Vector2(-maxSpeed, rigidBody.velocity.y);
		}
		if (rigidBody.velocity.y > maxSpeed) {
			rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxSpeed);
		}
		else if (rigidBody.velocity.y < -maxSpeed) {
			rigidBody.velocity = new Vector2(rigidBody.velocity.x, -maxSpeed);
		}
	}

	#endregion Movement Methods

	#region Collision Methods

	void OnTriggerEnter(Collider other)
	{
		// String otherPlayerName = model.otherPlayerName
		// if (other.tag == otherPlayerName) {
		//		remove .Attacking state
		//		playerState |= .Hitting
		// }
	}

	private BoxCollider2D collider;

	#endregion Collision Methods

	#region PlayerState Management

	// Use bit-wise comparisons for state checking. This lets us have combinations of state, like "Moving AND Jumping" or "Attacking AND GettingHit"
	private PlayerState playerState {
		get {
			return playerState;
		}
		set {
			// States toward the bottom will override earlier states. So put the important ones to the bottom.
			if (playerState == PlayerState.Idle) {
				// TODO
				spriteRenderer.sprite = idleSprite;
			}
			if (playerState == PlayerState.Dying) {
				// TODO
			}
			if ((playerState & PlayerState.Moving) == PlayerState.Moving) { // "if state includes Moving state (but maybe other states, too)"
				// TODO
				// TODO - If moving forward...
				spriteRenderer.sprite = movingForwardSprite;
				// TODO - Else if moving backward...
//				spriteRenderer.sprite = movingBackwardSprite;
			}
			if ((playerState & PlayerState.Jumping) == PlayerState.Jumping) {
				// TODO
				spriteRenderer.sprite = jumpingSprite;
			}
			if ((playerState & PlayerState.Attacking) == PlayerState.Attacking) {
				// TODO
			}
			if ((playerState & PlayerState.Hitting) == PlayerState.Hitting) {
				// TODO
			}
			if ((playerState & PlayerState.GettingHit) == PlayerState.GettingHit) {
				// TODO
			}
			if ((playerState & PlayerState.Falling) == PlayerState.Falling) {
				// TODO
				spriteRenderer.sprite = fallingSprite;
			}
			if ((playerState & PlayerState.Crouching) == PlayerState.Crouching) {
				// TODO
				spriteRenderer.sprite = crouchingSprite;
			}
		}
	}

	#endregion PlayerState Management

	#region GameStateSubscriber Methods

	public void SetState(GameState newState)
	{
		switch (newState) {
		case GameState.Starting:
			// TODO - Reset health, position, etc.
			break;
		default:
			break;
		}
	}

	#endregion GameStateSubscriber Methods

	private PlayerModel model;
	private Rigidbody2D rigidBody;
	private SpriteRenderer spriteRenderer;
}

public interface PlayerControllerEventHandler
{
	// TODO
	// PlayerHitOtherPlayerWithAttack(currentPlayerName, otherPlayerName, currentAttack)
}

[Flags]
enum PlayerState
{
	None = 0,
	Idle = 1,
	Moving = 2,
	Jumping = 4,
	Attacking = 8,
	Hitting = 16,
	GettingHit = 32,
	Dying = 64,
	Falling = 128,
	Crouching = 256,
	FacingLeft = 512,
	FacingRight = 1024
}
