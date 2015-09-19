using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;	// for Flags

/// <summary>
/// The player controller will be the go-between between player input, its underlying data in PlayerModel, and the GameController.
/// It handles events from the input controller, and passes events to the GameController.
/// It also handles player state.
/// </summary>
public class PlayerController : MonoBehaviour, GameStateSubscriber 
{
	[SerializeField]
	private float maxSpeed;

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

	// TODO
	void DidReceiveRawCommands(List<InputCombo> inputs)
	{
		foreach (input in inputs) {
			
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
	// DidReceiveCrouchCommand()
		// do crouch animation
		// playerState |= .Crouching
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
			if (playerState == PlayerState.Idle) {
				// TODO
			}
			if (playerState == PlayerState.Dying) {
				// TODO
			}
			if ((playerState & PlayerState.Moving) == PlayerState.Moving) { // "if state includes Moving state (but maybe other states, too)"
				// TODO
			}
			if ((playerState & PlayerState.Jumping) == PlayerState.Jumping) {
				// TODO
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
			}
			if ((playerState & PlayerState.Crouching) == PlayerState.Crouching) {
				// TODO
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
	Crouching = 256
}

// TEMP - This wil live in PlayerInputController
[Flags]
enum InputCombo
{
	None = 0
}
