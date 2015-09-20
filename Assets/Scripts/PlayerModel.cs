using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerModel : MonoBehaviour 
{
	public float movementSpeed = 5.0f;
	public float jumpSpeed = 5.0f;
	public float jumpHeight = 5.0f;

	public int attackCombo1;
	public int attackCombo2;
	public int attackCombo3;
	public int attackCombo4;
	public int suicideCombo;

	private List<InputCombo[]> movesList;

	// Use this for initialization
	void Start () {
		movesList = new List<InputCombo[]>();
		movesList.Insert(0, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.LoPunch});
		movesList.Insert(1, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.HiPunch});
		movesList.Insert(2, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.LoKick});
		movesList.Insert(3, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.HiKick});
		movesList.Insert(4, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.LoPunch});
		movesList.Insert(5, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.HiPunch});
		movesList.Insert(6, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.LoKick});
		movesList.Insert(7, new InputCombo[]{InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.HiKick});
		movesList.Insert(8, new InputCombo[]{InputCombo.Back, InputCombo.Down & InputCombo.Back, InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.LoPunch});
		movesList.Insert(9, new InputCombo[]{InputCombo.Back, InputCombo.Down & InputCombo.Back, InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.HiPunch});
		movesList.Insert(10, new InputCombo[]{InputCombo.Back, InputCombo.Down & InputCombo.Back, InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.LoKick});
		movesList.Insert(11, new InputCombo[]{InputCombo.Back, InputCombo.Down & InputCombo.Back, InputCombo.Down, InputCombo.Down & InputCombo.Forward, InputCombo.Forward, InputCombo.HiKick});
		movesList.Insert(12, new InputCombo[]{InputCombo.Forward, InputCombo.Down & InputCombo.Forward, InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.LoPunch});
		movesList.Insert(13, new InputCombo[]{InputCombo.Forward, InputCombo.Down & InputCombo.Forward, InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.HiPunch});
		movesList.Insert(14, new InputCombo[]{InputCombo.Forward, InputCombo.Down & InputCombo.Forward, InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.LoKick});
		movesList.Insert(15, new InputCombo[]{InputCombo.Forward, InputCombo.Down & InputCombo.Forward, InputCombo.Down, InputCombo.Down & InputCombo.Back, InputCombo.Back, InputCombo.HiKick});

		return;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
