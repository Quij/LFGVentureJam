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
	private float jumpDuration = 0.75f;	// TODO - Move this to player model.

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
		collider = GetComponent<BoxCollider2D>();
		model = GetComponent<PlayerModel>();
		rigidBody = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Use this for initialization
	void Start () 
	{
		GameController.sharedInstance.SubscribeToGameStateChanges(this);	
	}
	
	// Update is called once per frame
	void Update ()
	{
		LimitTopSpeedIfNeeded();
		
		if (Time.time - timeAtLastInput > 0.5f) {
			BecomeIdle();
		}

		if (!isGrounded && rigidBody.velocity.y < 0) {
			playerState |= PlayerState.Falling;
		}
	}

	#endregion Lifecycle Methods

	#region Input Event Handler Methods

	public void ReceivedInputCombos(List<InputCombo> inputCombos)
	{
		InputCombo input = inputCombos[0];
		if (StateContainsFlag(input, InputCombo.Back)) {
			DidReceiveMovementCommand(Vector2.left);	// TODO - Change this to `.right` depending on which way the player is facing.
		}
		if (StateContainsFlag(input, InputCombo.Forward)) {
			DidReceiveMovementCommand(Vector2.right);	// TODO - Change this to `.left` depending on which way the player is facing.
		}
		if (StateContainsFlag(input, InputCombo.Up)) {
			DidReceiveJumpCommand();
		}
		if (StateContainsFlag(input, InputCombo.Down)) {
			DidReceiveCrouchCommand();
		}
		// TODO - Handle other inputs
	}

	private void DidReceiveMovementCommand(Vector2 direction)
	{
		float movementSpeed = model.movementSpeed;
		playerState |= PlayerState.Moving;
		MoveInDirection(direction, movementSpeed);
	}

	private void DidReceiveJumpCommand()
	{
		if (isAllowedToJump) {
			float jumpHeight = model.jumpHeight;
			float jumpSpeed = model.jumpSpeed;
			playerState |= PlayerState.Jumping;
			Jump(jumpHeight, jumpSpeed);
			timeAtLastJump = Time.time;
		}
	}

	private void DidReceiveCrouchCommand()
	{
		if (isGrounded) {
			playerState |= PlayerState.Crouching;
		}		
	}

	private void BecomeIdle()
	{
		if (playerState != PlayerState.Idle) {
			if (isGrounded) {
				playerState = PlayerState.Idle;
			}
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
		rigidBody.AddForce(Vector2.up * jumpSpeed);
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

	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "Ground") {
			isGrounded = true;
			playerState = PlayerState.Idle;
		}
		
		// String otherPlayerName = model.otherPlayerName
		// if (other.tag == otherPlayerName) {
		//		remove .Attacking state
		//		playerState |= .Hitting
		// }
	}

	void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.tag == "Ground")
		{
			isGrounded = false;
		}
	}

	private BoxCollider2D collider;

	#endregion Collision Methods

	#region PlayerState Management

	// Use bit-wise comparisons for state checking. This lets us have combinations of state, like "Moving AND Jumping" or "Attacking AND GettingHit"
	private PlayerState _playerState;
	private PlayerState playerState {
		get {
			return _playerState;
		}
		set {
			PlayerState newState = value;
			Sprite newSprite = idleSprite;

			if (StateStrictlyContainsFlag(value, PlayerState.Idle)) { // If .Idle is the only flag...
				newSprite = idleSprite;
				newState = PlayerState.Idle;
			}
			if (StateContainsFlag(value, PlayerState.Moving)) {
				newState = RemoveIncompatibleFlagsFromState(newState);
				newSprite = movingForwardSprite;
				timeAtLastInput = Time.time;
			}
			if (StateContainsFlag(value, PlayerState.Jumping)) {
				newState = RemoveIncompatibleFlagsFromState(newState);
				isGrounded = false;
				newSprite = jumpingSprite;
				timeAtLastInput = Time.time;
			}
			if (StateContainsFlag(value, PlayerState.Falling)) {
				newState = RemoveIncompatibleFlagsFromState(newState);
				newSprite = fallingSprite;
				timeAtLastInput = Time.time;
			}
			if (StateContainsFlag(value, PlayerState.Crouching)) {
				newState = RemoveIncompatibleFlagsFromState(newState);
				newSprite = crouchingSprite;
				timeAtLastInput = Time.time;
			}

			_playerState = newState;
			spriteRenderer.sprite = newSprite;
		}
	}

	private bool StateContainsFlag(PlayerState state, PlayerState flag)
	{
		if ((state &= flag) == flag) {
			return true;
		}
		return false;
	}

	private bool StateContainsFlag(InputCombo state, InputCombo flag)
	{
		if ((state &= flag) == flag) {
			return true;
		}
		return false;
	}

	private PlayerState RemoveFlagFromState(PlayerState state, PlayerState flagToRemove)
	{
		return state &= ~flagToRemove;
	}

	// If we can remove the given flag and we're left with "None", then that flag was the only one present in `state`.
	private bool StateStrictlyContainsFlag(PlayerState state, PlayerState flag)
	{
		if (RemoveFlagFromState(state, flag) == PlayerState.None) {
			return true;
		}
		return false;
	}

	// All states are incompatible with Idle.
	// Moving is incompatible with Crouching.
	// Crouching is incompatible with Moving, Jumping, and Falling.
	// Jumping is incompatible with Falling and Crouching.
	// Falling is incompatible with Jumping and Crouching.
	// FacingLeft is incompatible with FacingRight (and vice versa).
	private PlayerState RemoveIncompatibleFlagsFromState(PlayerState state)
	{
		if (!StateStrictlyContainsFlag(state, PlayerState.Idle)) {
			PlayerState finalState = state;
			finalState = RemoveFlagFromState(finalState, PlayerState.Idle);	

			// Can't move and crouch at the same time.
			if (StateContainsFlag(finalState, PlayerState.Moving)) {
				finalState = RemoveFlagFromState(finalState, PlayerState.Crouching);
			}

			// Can't crouch and move, jump, or fall at the same time.
			if (StateContainsFlag(finalState, PlayerState.Crouching)) {
				finalState = RemoveFlagFromState(finalState, PlayerState.Moving);
				finalState = RemoveFlagFromState(finalState, PlayerState.Jumping);
				finalState = RemoveFlagFromState(finalState, PlayerState.Falling);
			}

			// Can't fall and jump or crouch at the same time.
			if (StateContainsFlag(finalState, PlayerState.Falling)) {
				finalState = RemoveFlagFromState(finalState, PlayerState.Jumping);
				finalState = RemoveFlagFromState(finalState, PlayerState.Crouching);
			}

			// Can't jump and fall or crouch at the same time.
			if (StateContainsFlag(finalState, PlayerState.Jumping)) {
				finalState = RemoveFlagFromState(finalState, PlayerState.Falling);
				finalState = RemoveFlagFromState(finalState, PlayerState.Crouching);
			}

			// Can't face left and right at the same time.
			if (StateContainsFlag(finalState, PlayerState.FacingLeft)) {
				finalState = RemoveFlagFromState(finalState, PlayerState.FacingRight);
			}
			if (StateContainsFlag(finalState, PlayerState.FacingRight)) {
				finalState = RemoveFlagFromState(finalState, PlayerState.FacingLeft);
			}

			return finalState;
		}
		else {
			// The state is just .Idle, so leave it at that.
			return state;
		}
	}

	private bool _isGrounded = true;
	private bool isGrounded {
		get {
			return _isGrounded;
		}
		set {
			_isGrounded = value;
		}
	}
	private float timeAtLastInput;
	private float timeAtLastJump;
	private bool isAllowedToJump {
		get {
			return isGrounded;
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

// TODO - Create an invisible centerline object, and do this.transform.lookAt(centerline). If that's negative, we're .FacingLeft. Otherwise we're .FacingRight. Eventually the centerline object will be the other player.
