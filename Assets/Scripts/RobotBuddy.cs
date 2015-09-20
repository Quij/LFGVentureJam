using UnityEngine;
using System.Collections;

public class RobotBuddy : MonoBehaviour {

	[SerializeField]
	private Sprite[] sprites;
	[SerializeField]
	private float timeBetweenFrames;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Time.time - timeAtSpriteFrameSwap > timeBetweenFrames) {
			if (spriteNumber == 0) { 
				spriteNumber = 1; 
			}
			else {
				spriteNumber = 0;
			}
			spriteRenderer.sprite = sprites[spriteNumber];
			timeAtSpriteFrameSwap = Time.time;
		}
	}

	private float timeAtSpriteFrameSwap;
	private SpriteRenderer spriteRenderer;
	private int spriteNumber = 0;
}
