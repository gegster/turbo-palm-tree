using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class BoardObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
	public enum BOType { CAPSULE, CONE, CUBE, CYLINDER, DIAMOND, DOUGHNUT, SPHERE, EMPTY } ;
	public enum BOColour { RED, GREEN, BLUE, YELLOW, ORANGE, WHITE, PURPLE };
	private static readonly Color[] Colours = new Color[] {Color.red, Color.green, Color.blue, Color.yellow, new Color(1.0f, 0.35f, 0.0f), Color.white, new Color(0.5f, 0.0f, 0.5f) };
	public static Color GetRealColour(BOColour colour) { return Colours[(int)colour]; }

	public BoardObject.BOType BoardObjectType { get; set; }
	public BoardObject.BOColour Colour { get; set; }
	public GameObject Mesh { get; set; }
	public Point3 GridPositionView { get; set; }
	public Point3 GridPositionModel { get; set; }

	private Board mBoard;

	public BoardObject()
	{
		GridPositionView = new Point3 ();
		GridPositionModel = new Point3 ();
	}

	// Use this for initialization
	void Start () 
	{
		Mesh.transform.SetParent (gameObject.transform, false);
		mBoard = Component.FindObjectOfType<Board> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPointerUp (PointerEventData eventData)
	{
		//LogInfo ();
	}

	public void OnPointerDown (PointerEventData eventData)
	{
#if UNITY_STANDALONE
		mBoard.OnBoardObjectTouched (this);
		//Debug.Log ("OnPointerDown " + this.name);
#endif
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
#if UNITY_STANDALONE
		if (Input.GetMouseButton (0))
#endif
		{
			//Debug.Log ("OnPointerEnter " + this.name);
			mBoard.OnBoardObjectTouched (this);
		}
	}

	private void LogInfo()
	{
		Debug.Log (transform.position.ToString());
		Debug.Log (GridPositionView.X.ToString () + " " + GridPositionView.Y.ToString () + " " + GridPositionView.Z.ToString ());
		Debug.Log (GridPositionModel.X.ToString () + " " + GridPositionModel.Y.ToString () + " " + GridPositionModel.Z.ToString ());
	}

	public bool IsAdjacent(BoardObject other)
	{
		var xDiff = Mathf.Abs (GridPositionModel.X - other.GridPositionModel.X);
		var yDiff = Mathf.Abs (GridPositionModel.Y - other.GridPositionModel.Y);
		var zDiff = Mathf.Abs (GridPositionModel.Z - other.GridPositionModel.Z);

		return (xDiff + yDiff + zDiff) == 1;
	}

	public override bool Equals(System.Object obj) 
	{
		return obj != null && obj is BoardObject && this == (BoardObject)obj;
	}

	public static bool operator ==(BoardObject a, BoardObject b) 
	{
		if (System.Object.ReferenceEquals(a, b))
		{
			return true;
		}

		if ((object)a == null || (object)b == null)
		{
			return false;

		}

		return a.BoardObjectType == b.BoardObjectType && a.Colour == b.Colour;
	}

	public static bool operator !=(BoardObject a, BoardObject b) 
	{
		return !(a == b);
	}
}
