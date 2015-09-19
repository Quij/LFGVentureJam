using UnityEngine;
using System.Collections;
using System.Collections.Generic; // for Lists

public class GameController : MonoBehaviour {

	// Publicly-accessible instance of game controller. Using this and `SubscribeToGameStateChanges`, anyone can sign up for game-state-change updates.
	public static GameController sharedInstance { 
		get {
			if (_sharedInstance == null) {
				_sharedInstance = GameObject.FindObjectOfType<GameController>();
			}
			return _sharedInstance;
		}
	}

	private static GameController _sharedInstance;

	#region Life-cycle Methods

	void Awake () 
	{
		_sharedInstance = this;	// Initialize the shared instance.
	}

	#endregion Life-cycle Methods

	#region State Management

	public void SetState(GameState newState)
	{
		state = newState;
		
		foreach (GameStateSubscriber subscriber in gameStateSubscribers) {
			subscriber.SetState(newState);
		}
	}
	
	private GameState state {
		get {
			return state;
		}
		set {
			switch (value) {
			case GameState.Starting:
				// TODO
				break;
			case GameState.Playing:
				// TODO
				break;
			case GameState.Paused:
				// TODO
				break;
			case GameState.GameOverPlayerOneWins:
				// TODO
				break;
			case GameState.GameOverPlayerTwoWins:
				// TODO
				break;
			case GameState.Exiting:
				// TODO
				Application.Quit();
				break;
			default:
				break;
			}
		}
	}

	// Adds the caller to the list of subscribers who will be notified of game state changes.
	public void SubscribeToGameStateChanges(GameStateSubscriber subscriber)
	{
		gameStateSubscribers.Add(subscriber);
	}

	private List<GameStateSubscriber> gameStateSubscribers = new List<GameStateSubscriber>();
	
	#endregion
}

// An object implementing this interface will be notified by the GameController whenever the GameState changes.
public interface GameStateSubscriber
{
	void SetState(GameState newState);
}

public enum GameState {
	Starting,
	Paused,
	Playing,
	GameOverPlayerOneWins,
	GameOverPlayerTwoWins,
	Exiting
}
