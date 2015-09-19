using UnityEngine;
using System.Collections;
using System;	// for Flags

/// <summary>
/// The player controller will be the go-between between player input, its underlying data in PlayerModel, and the GameController.
/// It handles events from the input controller, and passes events to the GameController.
/// It also handles player state.
/// </summary>
public class PlayerController : MonoBehaviour, GameStateSubscriber 
{
	#region Lifecycle Methods

	void Awake()
	{
		GameController.sharedInstance.SubscribeToGameStateChanges(this);
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	#endregion Lifecycle Methods

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
			if ((playerState & PlayerState.Dying) == PlayerState.Dying) {
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
			;
		default:
			break;
		}
	}

	#endregion GameStateSubscriber Methods
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
	Dying = 64
}
