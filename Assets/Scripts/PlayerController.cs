﻿using UnityEngine;
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
	[SerializeField]
	private Sprite attackSpriteHighPunch;
	[SerializeField]
	private Sprite attackSpriteLowPunch;
	[SerializeField]
	private Sprite attackSpriteLowKick;
	[SerializeField]
	private Sprite attackSpriteHighKick;

	[SerializeField]
	private AudioSource audioSource;
	[SerializeField]
	private AudioClip soundJump;
	[SerializeField]
	private AudioClip soundHighPunch;
	[SerializeField]
	private AudioClip soundLowPunch;
	[SerializeField]
	private AudioClip soundHighKick;
	[SerializeField]
	private AudioClip soundLowKick;

	#region Lifecycle Methods

	void Awake()
	{
		collider = GetComponent<BoxCollider2D>();
		model = GetComponent<PlayerModel>();
		rigidBody = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		centerline = GameObject.FindGameObjectWithTag("Centerline");
		standardGravityScale = rigidBody.gravityScale;
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
		
		if (Time.time - timeAtLastAttackInput > currentAttackDuration && StateContainsFlag(playerState, PlayerState.Attacking)) {
			playerState = RemoveFlagFromState(playerState, PlayerState.Attacking);
			rigidBody.gravityScale = standardGravityScale;
			rigidBody.isKinematic = false;
			rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
			Debug.Log("Left Attacking state");
		}

		UpdateFacingDirection();
	}

	#endregion Lifecycle Methods

	#region Input Event Handler Methods

	public void ReceivedInputCombos(List<InputCombo> inputCombos)
	{
		InputCombo input = inputCombos[0];
		if (StateContainsFlag(input, InputCombo.Back)) {
			DidReceiveMovementCommand(Vector2.left);
		}
		if (StateContainsFlag(input, InputCombo.Forward)) {
			DidReceiveMovementCommand(Vector2.right);
		}
		if (StateContainsFlag(input, InputCombo.Up)) {
			DidReceiveJumpCommand();
		}
		if (StateContainsFlag(input, InputCombo.Down)) {
			DidReceiveCrouchCommand();
		}

		// TEMP - TODO: Replace this with the actual attack combos.
		if (StateContainsFlag(input, InputCombo.HiKick)) {
			DidReceiveAttackCombo(AttackCombo.HiKick);
		}
		if (StateContainsFlag(input, InputCombo.LoKick)) {
			DidReceiveAttackCombo(AttackCombo.LoKick);
		}
		if (StateContainsFlag(input, InputCombo.HiPunch)) {
			DidReceiveAttackCombo(AttackCombo.HiPunch);
		}
		if (StateContainsFlag(input, InputCombo.LoPunch)) {
			DidReceiveAttackCombo(AttackCombo.LoPunch);
		}

		// AttackCombo attackCombo = model.ComboFromInputs(inputCombos);
		// if (attackCombo != AttackCombo.None) {
		// 	DidReceiveAttackCombo(attackCombo);
		// }

		// TODO - Handle other inputs
	}

	private void DidReceiveMovementCommand(Vector2 direction)
	{
		if (isAllowedToMove) {
			float movementSpeed = model.movementSpeed;
			playerState |= PlayerState.Moving;
			MoveInDirection(direction, movementSpeed);
		}
	}

	private void DidReceiveJumpCommand()
	{
		if (isAllowedToJump) {
			float jumpHeight = model.jumpHeight;
			float jumpSpeed = model.jumpSpeed;
			playerState |= PlayerState.Jumping;
			Jump(jumpHeight, jumpSpeed);
			timeAtLastJump = Time.time;

			audioSource.clip = soundJump;
			audioSource.Play();
		}
	}

	private void DidReceiveCrouchCommand()
	{
		if (isGrounded) {
			playerState |= PlayerState.Crouching;
		}		
	}

	private void DidReceiveAttackCombo(AttackCombo attackCombo)
	{
		if (!isAllowedToAttack) {
			return;
		}

		currentAttack = attackCombo;
		currentAttackDuration = 0.5f;	// a default attack length. TODO - Grab this from the model.
		playerState |= PlayerState.Attacking;

		// Halt all movement on initiating an attack.
		rigidBody.velocity = new Vector2(0.0f, 0.0f);
		rigidBody.isKinematic = false;
		rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

		// While attacking, the player isn't susceptible to gravity!
		rigidBody.gravityScale = 0.0f;

		// Add movement to attacks that require it.
		switch (attackCombo) {
		case AttackCombo.LoPunch:
			audioSource.clip = soundLowPunch;
			audioSource.Play();
			currentAttackDuration = 1.5f;
			spriteRenderer.sprite = jumpingSprite;
			StartCoroutine(HandleMovementsForLoPunch());
			break;
		case AttackCombo.LoKick:
			audioSource.clip = soundLowKick;
			audioSource.Play();
			currentAttackDuration = 1.5f;
			if (!isGrounded) {
				spriteRenderer.sprite = fallingSprite;
			}
			StartCoroutine(HandleMovementsForLoKick());
			break;
		case AttackCombo.HiPunch:
			audioSource.clip = soundHighPunch;
			audioSource.Play();
			break;
		case AttackCombo.HiKick:
			audioSource.clip = soundHighKick;
			audioSource.Play();
			break;
		default:
			break;
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

	private IEnumerator HandleMovementsForLoPunch()
	{
		rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
		
		// Quickly move upwards a few feet, then apply force down and forward.
		MoveInDirection(Vector2.up, model.movementSpeed * 16);
		yield return new WaitForSeconds(0.15f);
		spriteRenderer.sprite = attackSpriteLowPunch;
		MoveInDirection(Vector2.down, model.movementSpeed * 24);	// TODO - Grab a separate attack speed property from model.
		if (facingDirection == PlayerFacingDirection.FacingRight) {
			MoveInDirection(Vector2.right, model.movementSpeed * 5);
		}
		else {
			MoveInDirection(Vector2.left, model.movementSpeed * 5);
		}
	}

	private IEnumerator HandleMovementsForLoKick()
	{
		rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

		MoveInDirection(Vector2.down, model.movementSpeed * 16);
		yield return new WaitForSeconds(0.25f);
		spriteRenderer.sprite = attackSpriteLowKick;
		rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
		yield return new WaitForSeconds(0.25f);
		spriteRenderer.sprite = idleSprite;
		rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	private AttackCombo currentAttack;
	private float currentAttackDuration = 0.5f;
	private float timeAtLastAttackInput;

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

	private PlayerFacingDirection _facingDirection = PlayerFacingDirection.FacingRight;
	private PlayerFacingDirection facingDirection {
		get {
			return _facingDirection;
		}
		set {
			_facingDirection = value;
			if (_facingDirection == PlayerFacingDirection.FacingRight) {
				spriteRenderer.transform.localScale = new Vector3(1, 1, 1);	// "Flip" the sprite without affecting the orientation of its transform.
			}
			else {
				spriteRenderer.transform.localScale = new Vector3(-1, 1, 1);	// "Flip" the sprite without affecting the orientation of its transform.
			}
		}
	}
	private void UpdateFacingDirection()
	{
		// If we're attacking, don't flip mid-animation.
		if (playerState == PlayerState.Attacking) {
			return;
		}

		if (centerline.transform.position.x - this.transform.position.x > 0) {
			// Then centerline is to the right of the fighter.
			if (facingDirection != PlayerFacingDirection.FacingRight) {
				facingDirection = PlayerFacingDirection.FacingRight;
			}
		}
		else {
			// Then centerline is to the left of the fighter.
			if (facingDirection != PlayerFacingDirection.FacingLeft) {
				facingDirection = PlayerFacingDirection.FacingLeft;
			}
		}
	}

	private Sprite SpriteForCurrentAttack()
	{
		switch (currentAttack) {
		case AttackCombo.HiPunch:
			return attackSpriteHighPunch;
		case AttackCombo.LoPunch:
			return attackSpriteLowPunch;
		case AttackCombo.HiKick:
			return attackSpriteHighKick;
		case AttackCombo.LoKick:
			return attackSpriteLowKick;
		default:
			return idleSprite;
		}
	}

	private float standardGravityScale;

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
				if (facingDirection == PlayerFacingDirection.FacingRight) {
					if (rigidBody.velocity.x >= 0) {
						newSprite = movingForwardSprite;
					}
					else {
						newSprite = movingBackwardSprite;
					}
				}
				else { // player FacingLeft
					if (rigidBody.velocity.x >= 0) {
						newSprite = movingBackwardSprite;
					}
					else {
						newSprite = movingForwardSprite;
					}
				}
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
			if (StateContainsFlag(value, PlayerState.Attacking)) {
				Debug.Log("Entered Attacing state");
				newState = RemoveIncompatibleFlagsFromState(newState);
				newSprite = SpriteForCurrentAttack();
				timeAtLastInput = Time.time;
				timeAtLastAttackInput = Time.time;
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

			// Can't attack and move at the same time.
			if (StateContainsFlag(finalState, PlayerState.Attacking)) {
				finalState = RemoveFlagFromState(finalState, PlayerState.Moving);
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

	// Allow movement if we're not attacking.
	private bool isAllowedToMove {
		get {
			return !StateContainsFlag(playerState, PlayerState.Attacking);
		}
	}

	// Allow attacking if we're not already attacking.
	private bool isAllowedToAttack {
		get {
			return !StateContainsFlag(playerState, PlayerState.Attacking);
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
	private GameObject centerline;
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

enum PlayerFacingDirection
{
	FacingRight,
	FacingLeft
}

// TEMP - To be replaced with the Model's version of this
enum AttackCombo
{
	HiPunch,
	LoPunch,
	HiKick,
	LoKick
}
