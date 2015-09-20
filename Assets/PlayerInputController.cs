using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerInputController : MonoBehaviour {
	private List<InputCombo> inputBuffer;
	private float downTime;
	private float frameDuration;
	private int bufferLimit;
	private InputCombo lastCombination;

	private PlayerInputEventHandler eventHandler;

	void Awake()
	{
		eventHandler = GetComponent<PlayerController>();
	}

	// Use this for initialization
	void Start () {
		inputBuffer = new List<InputCombo>();
		bufferLimit = 10;
		frameDuration = 0.3f;
		downTime = Time.time;
		lastCombination = InputCombo.None;

		return;
	}
	
	// Update is called once per frame
	void Update () {
		InputCombo currentCombination = InputCombo.None;
		float currentTime = Time.time;
		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");

		//multiply horizontalInput by -1 if opponent switched sides
		if(horizontalInput != 0) {
			if(horizontalInput < 0) {
				currentCombination |= InputCombo.Back;
			}
			else /*if(horizontalInput > 0)*/ {
				currentCombination |= InputCombo.Forward;
			}
		}

		if(verticalInput != 0) {
			if(verticalInput < 0) {
				currentCombination |= InputCombo.Down;
			}
			else /*if(verticalInput > 0)*/ {
				currentCombination |= InputCombo.Up;
			}
		}

		if(Input.GetButton("lo kick")) {
			currentCombination |= InputCombo.LoKick;
		}

		if(Input.GetButton("hi kick")) {
			currentCombination |= InputCombo.HiKick;
		}

		if(Input.GetButton("lo punch")) {
			currentCombination |= InputCombo.LoPunch;
		}

		if(Input.GetButton("hi punch")) {
			currentCombination |= InputCombo.HiPunch;
		}

		if(Input.GetButton("guard")) {
			currentCombination |= InputCombo.Guard;
		}

		if(Input.GetButton("dodge")) {
			currentCombination |= InputCombo.Dodge;
		}
		
		if(Input.GetButton("select")) {
			currentCombination |= InputCombo.Select;
		}
		
		if(Input.GetButton("start")) {
			currentCombination |= InputCombo.Start;
		}

		if(currentCombination != InputCombo.None) {
			if(currentCombination != lastCombination) {
				downTime = Time.time;
				lastCombination = currentCombination;
				inputBuffer.Insert(0, currentCombination);
			}
			else if((currentTime - downTime) >= frameDuration) {
				downTime = Time.time;
				lastCombination = currentCombination;
				inputBuffer.Insert(0, currentCombination);
			}

			if(inputBuffer.Count > bufferLimit) {
				inputBuffer.RemoveAt(bufferLimit);
			}

			Debug.Log(inputBuffer.Count);
			eventHandler.ReceivedInputCombos(inputBuffer);
		}

		return;
	}
}

[Flags]
public enum InputCombo{
	None = 0,
	Back = 1,
	Forward = 2,
	Up = 4,
	Down = 8,
	LoKick = 16,
	HiKick = 32,
	LoPunch = 64,
	HiPunch = 128,
	Guard = 256,
	Dodge = 512,
	Select = 1024,
	Start = 2048
}

interface PlayerInputEventHandler
{
	void ReceivedInputCombos(List<InputCombo> inputCombos);
}
