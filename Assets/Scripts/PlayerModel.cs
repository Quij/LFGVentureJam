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

	public class MovesTree {
		private MoveNode root;
		public int lastAttackCombo;

		private class MoveNode {
			public List<MoveNode> children;
			public InputCombo inputCombo;
			public int attackCombo;

			public MoveNode(InputCombo inputInstance = InputCombo.None) {
				attackCombo = -1;
				inputCombo = inputInstance;
				children = new List<MoveNode>();

				return;
			}

			public int findChild(InputCombo inputInstance){
				for(int index = 0; index < children.Count; index++) {
					if(children[index].inputCombo == inputInstance) {
						return index;
					}
				}

				return -1;
			}

			public void addChild(InputCombo inputInstance, MovesTree tree = null) {
				children.Insert(0, new MoveNode(inputInstance));

				if(tree != null) {
					tree.lastAttackCombo++;
					children[0].attackCombo = tree.lastAttackCombo;
				}

				return;
			}

			public void printAttackCombos(String ascendants = "") {
				ascendants += "[" + inputCombo + "]";
				if(attackCombo >= 0) {
					ascendants += "*";
				}

				if(children.Count > 0) {
					for(int i = 0; i < children.Count; i++) {
						children[i].printAttackCombos(ascendants);
					}
				}
				else {
					Debug.Log("Move: " + ascendants);
				}

				return;
			}
		}

		public MovesTree() {
			root = new MoveNode();
			lastAttackCombo = -1;

			return;
		}

		public bool AddMove(InputCombo[] move) {
			MoveNode currentNode = root;
			int currentInput = 0;
			int childIndex = currentNode.findChild(move[currentInput]);

			//traverse tree until end
			while(childIndex >= 0) {
				currentNode = currentNode.children[childIndex];

				if((currentInput + 1) == move.Length) {
					break;
				}

				currentInput++;
				childIndex = currentNode.findChild(move[currentInput]);
			}

			//add missing new nodes to tree up until final node
			for(; currentInput < (move.Length - 1); currentInput++) {
				currentNode.addChild(move[currentInput]);
				childIndex = 0;//currentNode.findChild();
				currentNode = currentNode.children[childIndex];
			}

			//for the final node if move doesn't exist assign new attackCombo value and return true
			if(currentInput == (move.Length - 1)) {
				currentNode.addChild(move[currentInput], this);
				return true;
			}

			//else return false;
			return false;
		}

		public int findAttackComboNumber(InputCombo [] inputBuffer) {
			MoveNode currentNode = root;
			int i;
			int j;
			int start = (inputBuffer.Length - 1);
			bool reset = false;

			while(start > 0) {
				//read buffer backwards
				for(i = start; i >= 0; i--) {
					if(reset){
						currentNode = root;
					}
					else {
						reset = true;
					}

					//focus on each child in order
					for(j = 0; j < currentNode.children.Count; j++) {
						//compare child against buffer
						if(inputBuffer[i] == currentNode.children[j].inputCombo) {
							currentNode = currentNode.children[j];

							Debug.Log("[" + inputBuffer[i] + "]");

							if(i == 0) {
								Debug.Log("FOUND COMBO #" + currentNode.attackCombo);

								return currentNode.attackCombo;
							}

							reset = false;
						}
					}
				}

				start--;
			}

			return -1;
		}

		public void printTree() {
			MoveNode currentNode = root;

			Debug.Log("===Printing tree===");
				root.printAttackCombos();
			Debug.Log("===Finished printing tree===");

			return;
		}
	}

	private MovesTree movesTree;

	// Use this for initialization
	void Start () {
		movesTree = new MovesTree();

		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.LoPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.HiPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.LoKick});
		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.HiKick});
		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.LoPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.HiPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.LoKick});
		movesTree.AddMove(new InputCombo[]{InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.HiKick});
		movesTree.AddMove(new InputCombo[]{InputCombo.Back, InputCombo.Down | InputCombo.Back, InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.LoPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Back, InputCombo.Down | InputCombo.Back, InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.HiPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Back, InputCombo.Down | InputCombo.Back, InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.LoKick});
		movesTree.AddMove(new InputCombo[]{InputCombo.Back, InputCombo.Down | InputCombo.Back, InputCombo.Down, InputCombo.Down | InputCombo.Forward, InputCombo.Forward, InputCombo.HiKick});
		movesTree.AddMove(new InputCombo[]{InputCombo.Forward, InputCombo.Down | InputCombo.Forward, InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.LoPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Forward, InputCombo.Down | InputCombo.Forward, InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.HiPunch});
		movesTree.AddMove(new InputCombo[]{InputCombo.Forward, InputCombo.Down | InputCombo.Forward, InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.LoKick});
		movesTree.AddMove(new InputCombo[]{InputCombo.Forward, InputCombo.Down | InputCombo.Forward, InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back, InputCombo.HiKick});

		//Debug.Log(movesTree.findAttackComboNumber(new InputCombo[]{InputCombo.HiKick, InputCombo.Forward, InputCombo.Down | InputCombo.Forward, InputCombo.Down, InputCombo.Down | InputCombo.Back, InputCombo.Back}));
		//Debug.Log(movesTree.findAttackComboNumber(new InputCombo[]{InputCombo.HiKick, InputCombo.Forward, InputCombo.Down | InputCombo.Forward, InputCombo.Down}));
		//Debug.Log(movesTree.findAttackComboNumber(new InputCombo[]{InputCombo.Forward, InputCombo.Down | InputCombo.Forward, InputCombo.Down}));
		//movesTree.printTree();

		return;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
