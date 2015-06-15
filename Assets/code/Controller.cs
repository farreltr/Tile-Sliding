using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
	public static float STOPPED = 0.0f;
	private Vector2 direction;	
	private Animator animator;
	public static Vector2 RIGHT = new Vector2 (1.0f, 0.0f);
	public static Vector2 LEFT = new Vector2 (-1.0f, 0.0f);
	public static Vector2 UP = new Vector2 (0.0f, 1.0f);
	public static Vector2 DOWN = new Vector2 (0.0f, -1.0f);
	public static Vector2 STOP = new Vector2 (0.0f, 0.0f);
	public Vector3 startDirection;
	new string name;
	private Vector3 endPosition;
	private bool moving = false;
	private float duration = 50f; //at duration = 100 there is a stall at the center tile. Probably due to approximation
	private int nodeLayerIndex;

	// Pledge Algorithm
	//private bool rightSensor;
	private bool leftSensor;
	private bool frontSensor;

	private bool isPledge = false;
	private int pledgeCounter = 0;
	private Vector3 nextNodePosition;
	
	
	void Start ()
	{
		animator = this.GetComponent<Animator> ();
		direction = startDirection;
		nodeLayerIndex = 1 << LayerMask.NameToLayer ("Node");
	}
	
	void Update ()
	{
		UpdateAnimation ();
		UpdateCurrentLocation ();
		if (moving) {
			MoveToNextNode ();
		} else {
			DoSense ();
			if (isPledge) {
				MovePledge ();
			} else {
				MoveNormally ();
			}
		}
	}

	private void UpdateCurrentLocation ()
	{
		RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector3.up, 0.01f, Tile.tileIndex);
		transform.parent = hit.collider.transform;
	}

	private void MoveToNextNode ()
	{
		if (!Mathf.Approximately (gameObject.transform.position.magnitude, endPosition.magnitude)) {
			gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, endPosition, 1 / (duration * (Vector3.Distance (gameObject.transform.position, endPosition))));
		} else {
			moving = false;
		}
	}

	private void MoveNormally ()
	{
		if (frontSensor) {
			MoveForward ();
		} else {
			TurnRight (); // obstacle is now on left
			isPledge = true;
		}
	}

	private void MovePledge ()
	{
		if (leftSensor) { // obstacle is not on my left
			TurnLeft ();
			MoveForward ();

		} else { // obstacle is on my left

			if (frontSensor) {
				MoveForward ();
			} else {
				TurnRight ();
			}
		}
		isPledge = pledgeCounter != 0;
	}

	private void DoSense ()
	{
		RaycastHit2D hit = Physics2D.Raycast (transform.position, direction, 0.1f, nodeLayerIndex);
		if (hit.collider == null) {
			ChangeDirection ();
			hit.collider.enabled = true;
		}
		transform.parent = hit.collider.transform;
		hit.collider.enabled = false;
		leftSensor = LookLeft ();
		//rightSensor = LookRight ();
		frontSensor = LookForward ();
		hit.collider.enabled = true;
	}

	private bool LookLeft ()
	{
		Vector3 left = GetLeftDirection ();
		RaycastHit2D hit = Physics2D.Raycast (transform.position, left, 0.8f, nodeLayerIndex);
		if (hit.collider == null)
			return false;
		Node node = hit.transform.gameObject.GetComponent<Node> ();
		if (node.GetTileType () == Node.Type.Path) {
			return true;
		}
		return false;
	}

	private bool LookRight ()
	{
		RaycastHit2D hit = Physics2D.Raycast (transform.position, GetRightDirection (), 0.8f, nodeLayerIndex);
		if (hit.collider == null)
			return false;
		Node node = hit.transform.gameObject.GetComponent<Node> ();
		if (node.GetTileType () == Node.Type.Path) {
			return true;
		}
		return false;
	}

	private bool LookForward ()
	{
		RaycastHit2D hit = Physics2D.Raycast (transform.position, direction, 0.8f, nodeLayerIndex);
		if (hit.collider == null)
			return false;
		Node node = hit.transform.gameObject.GetComponent<Node> ();
		if (node.GetTileType () == Node.Type.Path) {
			nextNodePosition = node.transform.position;
			return true;
		}
		return false;
	}

	
	void UpdateAnimation ()
	{
		if (direction.y > 0) {
			animator.SetInteger ("direction", 2);
		} else if (direction.y < 0) {
			animator.SetInteger ("direction", 0);
		} else if (direction.x > 0) {
			animator.SetInteger ("direction", 3);
		} else if (direction.x < 0) {
			animator.SetInteger ("direction", 1);
		}
	}
	
	private void ChangeDirection ()
	{
		direction = -1 * direction;
	}
	
	private void Stop ()
	{
		direction = STOP;
		transform.Translate (direction);
	}

	private void MoveForward ()
	{
		DoSense ();
		endPosition = nextNodePosition;
		endPosition.y += 0.2f;
		moving = true;
	}
	
	private void TurnRight ()
	{
		direction = GetRightDirection ();
		pledgeCounter++;
	}

	private Vector3 GetRightDirection ()
	{
		if (direction == RIGHT) {
			return DOWN;
		} else if (direction == LEFT) {
			return UP;
		} else if (direction == UP) {
			return RIGHT;
		} else if (direction == DOWN) {
			return LEFT;
		}
		return STOP;
	}
	
	private void TurnLeft ()
	{
		direction = GetLeftDirection ();
		pledgeCounter--;
		
	}

	private Vector3 GetLeftDirection ()
	{
		if (direction == RIGHT) {
			return UP;
		} else if (direction == LEFT) {
			return DOWN;
		} else if (direction == UP) {
			return LEFT;
		} else if (direction == DOWN) {
			return RIGHT;
		}
		return STOP;
	}
	
}