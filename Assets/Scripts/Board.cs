using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Board : MonoBehaviour
{
	public delegate void OnItemSwapped (BoardObject a, BoardObject b);

	public delegate void OnMatchObjectsRemoved (Match<BoardObject> match);

	public delegate void OnBoardObjectsDropped ();

	public enum BoardState
	{
		IDLE,
		ANIMATING,
		CHECKING
	};

	public enum BoardFace
	{
		X_Y,
		Z_Y,
		Z_NX
	};

	public enum BoardAxis
	{
		X,
		Y,
		Z
	};

	public Vector3 gridSize;

	public int minMatchSize;
	public int levelId;
	public float swapTime;
	public float removeTime;
	public float moveTimePerGridTile;

	private BoardObject[,,] mBoardObjects;
	private BoardState mState;
	private Point3 mGridExtents;

	private List<BoardObject> mSwapCandidates = new List<BoardObject> ();
	private List<Match<BoardObject>> mValidMathes = new List<Match<BoardObject>> ();
	private Point3 mLastSwapDirection;
	private Dictionary<BoardFace, List<Point3>> mFaceDownDirections;

	// Use this for initialization
	void Start()
	{
		UnityEngine.Random.seed = levelId;

		mGridExtents = new Point3 ((int)gridSize.x, (int)gridSize.y, (int)gridSize.z);

		mFaceDownDirections = CreateFaceDownDirections ();

		int numBoardObjectTypes = Enum.GetValues (typeof(BoardObject.BOType)).Length - 1;
		int numBoardObjectColours = Enum.GetValues (typeof(BoardObject.BOColour)).Length;

		var objectTypes = BoardUtils.GetRandomValues ((int)gridSize.x, (int)gridSize.y, (int)gridSize.z, numBoardObjectTypes);
		objectTypes = BoardUtils.RemoveMatches (objectTypes, minMatchSize, numBoardObjectTypes);
		var objectColours = BoardUtils.GetRandomValues ((int)gridSize.x, (int)gridSize.y, (int)gridSize.z, numBoardObjectColours);
		objectColours = BoardUtils.RemoveMatches (objectColours, minMatchSize, numBoardObjectColours);
		mBoardObjects = BoardUtils.CreateOuterBoardObjects (objectTypes, objectColours, this);

		mState = BoardState.IDLE;
	}

	private Dictionary<BoardFace, List<Point3>> CreateFaceDownDirections()
	{
		Dictionary<BoardFace, List<Point3>> faceDownDirections = new Dictionary<BoardFace, List<Point3>> ();
		faceDownDirections.Add (BoardFace.X_Y, new List<Point3> { Point3.NEG_Y });
		faceDownDirections.Add (BoardFace.Z_Y, new List<Point3> { Point3.NEG_Y });
		faceDownDirections.Add (BoardFace.Z_NX, new List<Point3> { Point3.NEG_Z, Point3.NEG_X });
	
		return faceDownDirections;
	}

	private void SetState(BoardState newState)
	{
		mState = newState;
	}

	public void OnBoardObjectTouched(BoardObject boardObject)
	{
		if (mState != BoardState.IDLE)
		{
			return; // ignore
		}

		if (mSwapCandidates.Contains (boardObject) == false)
		{
			if (mSwapCandidates.Count > 0)
			{
				if (boardObject.IsAdjacent (mSwapCandidates [0]))
				{
					mSwapCandidates.Add (boardObject);
					SetState (BoardState.ANIMATING);
				}
				else
				{
					mSwapCandidates.Clear ();
					mSwapCandidates.Add (boardObject);
				}
			}
			else
			{
				mSwapCandidates.Add (boardObject);
			}
		}
	}

	void Update()
	{
		switch (mState)
		{
		case BoardState.IDLE:
			break;
		case BoardState.ANIMATING:
			UpdateAnimatingState ();
			break;
		case BoardState.CHECKING:
			UpdateCheckingState ();
			break;
		}
	}

	private void UpdateAnimatingState()
	{
		if (mSwapCandidates.Count >= 2)
		{
			SwapBoardObjectsInModel (mSwapCandidates [0], mSwapCandidates [1]);
			mLastSwapDirection = mSwapCandidates [1].GridPositionModel - mSwapCandidates [0].GridPositionModel;
			StartCoroutine (SwapBoardObjectsInViewCoroutine (mSwapCandidates [0], mSwapCandidates [1], OnBoardObjectsSwapped));
			mSwapCandidates.Clear ();
		}
	}

	private void UpdateCheckingState()
	{
		var emptySlots = BoardUtils.GetEmptySlots (mBoardObjects);

		if (emptySlots.Count > 0)
		{
			StartCoroutine (DropBoardObjectsCoroutine (emptySlots, OnEmptySlotsFilled));
		}
		else
		{
			SetState (BoardState.IDLE);
		}
	}

	private List<BoardObject> GetBoardObjectsInColumn(int x, int z)
	{
		var boardObjects = new List<BoardObject> ();
		for (int y = 0; y < mGridExtents.Y; ++y)
		{
			boardObjects.Add (mBoardObjects [x, y, z]);
		}

		return boardObjects;
	}

	List<BoardObject> GetFaceBoardObjects(BoardFace face)
	{
		var boardObjects = new List<BoardObject> ();
		int numRows = 0;
		int numColumns = 0;
		GetFaceColumnRowSize (face, ref numColumns, ref numRows);

		for (int row = 0; row < numRows; ++row)
		{
			for (int column = 0; column < numColumns; ++column)
			{
				var boardObject = GetFaceBoardObject (face, column, row);
				boardObjects.Add (boardObject);
			}
		}

		return boardObjects;
	}

	void GetFaceColumnRowSize(BoardFace face, ref int numColumns, ref int numRows)
	{
		switch (face)
		{
		default:
		case BoardFace.X_Y:
			numColumns = mGridExtents.X;
			numRows = mGridExtents.Y;
			break;
		case BoardFace.Z_Y:
			numColumns = mGridExtents.Z;
			numRows = mGridExtents.Y;
			break;
		case BoardFace.Z_NX:
			numColumns = mGridExtents.Z;
			numRows = mGridExtents.X;
			break;
		}
	}

	BoardObject GetFaceBoardObject(BoardFace face, int column, int row)
	{
		switch (face)
		{
		default:
		case BoardFace.X_Y:
			return mBoardObjects [column, row, 0];
		case BoardFace.Z_Y:
			return mBoardObjects [mGridExtents.X - 1, row, column];
		case BoardFace.Z_NX:
			return mBoardObjects [row, mGridExtents.Y - 1, column];
		}
	}

	private Point3 GetDropPosition(BoardObject boardObject, BoardFace face, ref List<Point3> moves)
	{
		var startingPosition = new Point3 (boardObject.GridPositionModel);
		var lastPosition = new Point3 (startingPosition);
		var delta = new Point3 ();
		var dropPosition = DropBoardObjectFrom (startingPosition, face, ref delta);
		while (dropPosition != lastPosition)
		{
			moves.Add (delta);
			lastPosition = dropPosition;
			dropPosition = DropBoardObjectFrom (lastPosition, face, ref delta);
		}

		return dropPosition;
	}

	IEnumerator DropBoardObjectsCoroutine(List<BoardObject> emptySlots, OnBoardObjectsDropped finishedCallback)
	{
		// update model positions first
		var boardObjectMoves = new List<BoardObjectMove>();
		var boardFaces = new BoardFace[] { BoardFace.X_Y, BoardFace.Z_Y, BoardFace.Z_NX }; 
		foreach (BoardFace face in boardFaces)
		{
			var boardObjects = GetFaceBoardObjects (face);
			foreach (var boardObject in boardObjects)
			{
				if (boardObject.BoardObjectType != BoardObject.BOType.EMPTY) 
				{
					var moves = new List<Point3> ();
					var dropPosition = GetDropPosition (boardObject, face, ref moves);
					if (dropPosition != boardObject.GridPositionModel) {
						var temp = mBoardObjects [dropPosition.X, dropPosition.Y, dropPosition.Z];
						mBoardObjects [dropPosition.X, dropPosition.Y, dropPosition.Z] = boardObject;
						mBoardObjects [boardObject.GridPositionModel.X, boardObject.GridPositionModel.Y, boardObject.GridPositionModel.Z] = temp; 

						boardObjectMoves.Add (new BoardObjectMove (boardObject, moves));
						boardObject.GridPositionModel = dropPosition;
					}
				}
			}
		}

		yield return new WaitForEndOfFrame ();

		var maxMoves = 0;
		Vector3[] startPositions = new Vector3[boardObjectMoves.Count];
		for(int i = 0; i < boardObjectMoves.Count; ++i) 
		{ 
			var boardObjectMove = boardObjectMoves [i];
			if(boardObjectMove.Moves.Count > maxMoves)
			{
				maxMoves = boardObjectMove.Moves.Count;
			}
		}
			
		var targetPosition = new Vector3 ();

		for( int j = 0; j < maxMoves; ++j)
		{
			for(int i = 0; i < boardObjectMoves.Count; ++i) 
			{ 
				var boardObjectMove = boardObjectMoves [i];
				startPositions[i] = boardObjectMove.BoardObject.transform.position;
				if(boardObjectMove.Moves.Count > maxMoves)
				{
					maxMoves = boardObjectMove.Moves.Count;
				}
			}

			var startTime = Time.time;
			var endTime = startTime + moveTimePerGridTile;
			while (Time.time <= endTime)
			{
				for (int i = 0; i < boardObjectMoves.Count; ++i)
				{
					var move = boardObjectMoves [i];
					if (move.Moves.Count <= j + 1) 
					{
						var boardObject = move.BoardObject;
						var t = (Time.time - startTime) / moveTimePerGridTile;
						Debug.Log (t);
						t = Mathf.Clamp01 (t);

						targetPosition.Set (boardObject.GridPositionView.X + move.Moves[j].X, boardObject.GridPositionView.Y + move.Moves[j].Y, boardObject.GridPositionView.Z + move.Moves[j].Z);
						boardObject.transform.position = Vector3.Lerp (startPositions [i], targetPosition, t);
					}
				}

				yield return new WaitForEndOfFrame ();
			}

			for (int i = 0; i < boardObjectMoves.Count; ++i)
			{
				var move = boardObjectMoves [i];
				if (boardObjectMoves [i].Moves.Count <= j + 1) 
				{
					var boardObject = boardObjectMoves [i].BoardObject;
					targetPosition.Set (boardObject.GridPositionView.X + move.Moves[j].X, boardObject.GridPositionView.Y + move.Moves[j].Y, boardObject.GridPositionView.Z + move.Moves[j].Z);
					boardObject.transform.position = targetPosition;
					boardObject.GridPositionView.Set((int)targetPosition.x, (int)targetPosition.y, (int)targetPosition.z);
				}
			}
		}

		if (finishedCallback != null)
		{
			finishedCallback ();
		}
	}

	private Point3 DropBoardObjectFrom(Point3 startPoint, BoardFace face, ref Point3 delta)
	{
		Point3 newPosition = TryDroppingBoardObjectDownFace (startPoint, face, ref delta);
		if (newPosition == startPoint)
		{
			newPosition = TryDroppingBoardObjectDownAdjacentFace (newPosition, face, ref delta);
		}

		return newPosition;
	}

	private Point3 TryDroppingBoardObjectDownFace(Point3 startPoint, BoardFace face, ref Point3 delta)
	{
		var downDirections = mFaceDownDirections [face];

		foreach (Point3 downDirection in downDirections)
		{
			var belowPosition = startPoint + downDirection;
			if (IsValidGridPosition (belowPosition) && IsGridPositionEmpty (belowPosition))
			{
				delta = downDirection;
				return belowPosition;
			}
		}

		return startPoint;
	}

	private bool IsValidGridPosition(Point3 gridPosition)
	{
		return (gridPosition.X >= 0 && gridPosition.X < mGridExtents.X) && (gridPosition.Y >= 0 && gridPosition.Y < mGridExtents.Y) && (gridPosition.Z >= 0 && gridPosition.Z < mGridExtents.Z);
	}

	private bool IsGridPositionEmpty(Point3 gridPosition)
	{
		Debug.Log (gridPosition.X + " " + gridPosition.Y + " " + gridPosition.Z);
		BoardObject slot = mBoardObjects [gridPosition.X, gridPosition.Y, gridPosition.Z];
		if (slot == null) 
		{
			int i = 0;
			++i;
		}
		return slot.BoardObjectType == BoardObject.BOType.EMPTY;
	}

	private Point3 TryDroppingBoardObjectDownAdjacentFace(Point3 startPoint, BoardFace face, ref Point3 delta)
	{
		return startPoint;
	}

	private void OnEmptySlotsFilled()
	{
		// TODO: check board for matches
		SetState (BoardState.IDLE);
	}

	IEnumerator RemoveMatchedBoardObjectsInViewCoroutine(Match<BoardObject> match, OnMatchObjectsRemoved finishedCallback)
	{
		var matchedBoardObjects = match.GetMatchItemsAsFlatList ();

		var startTime = Time.time;
		var endTime = startTime + removeTime;

		Vector3[] startScales = new Vector3[matchedBoardObjects.Count];
		Vector3 targetScale = Vector3.zero;

		for (int i = 0; i < matchedBoardObjects.Count; ++i)
		{
			startScales [i] = matchedBoardObjects [i].transform.localScale;
		}
			
		while (Time.time <= endTime)
		{
			var t = (Time.time - startTime) / swapTime;

			for (int i = 0; i < matchedBoardObjects.Count; ++i)
			{
				//startScales [i] = matchedBoardObjects [i].transform.localScale;
				matchedBoardObjects [i].transform.localScale = Vector3.Lerp (startScales [i], targetScale, t);
			}

			yield return new WaitForEndOfFrame ();
		}

		foreach (BoardObject boardObject in matchedBoardObjects)
		{
			mBoardObjects [boardObject.GridPositionView.X, boardObject.GridPositionView.Y, boardObject.GridPositionView.Z].BoardObjectType = BoardObject.BOType.EMPTY;
		}

		if (finishedCallback != null)
		{
			finishedCallback (match);
		}
	}

	private void OnMatchedBoardObjectsRemoved(Match<BoardObject> match)
	{
		mValidMathes.Remove (match);

		if (mValidMathes.Count == 0)
		{
			SetState (BoardState.CHECKING);
		}
	}

	private void SwapBoardObjectsInModel(BoardObject a, BoardObject b)
	{
		var tempGridPosition = a.GridPositionModel;
		a.GridPositionModel = b.GridPositionModel;
		b.GridPositionModel = tempGridPosition;
	}

	IEnumerator SwapBoardObjectsInViewCoroutine(BoardObject a, BoardObject b, OnItemSwapped finishedCallback)
	{
		var startTime = Time.time;
		var endTime = startTime + swapTime;

		var aStartPosition = a.transform.position;
		var bStartPosition = b.transform.position;

		while (Time.time <= endTime)
		{
			var t = (Time.time - startTime) / swapTime;

			a.transform.position = Vector3.Lerp (aStartPosition, bStartPosition, t);
			b.transform.position = Vector3.Lerp (bStartPosition, aStartPosition, t);

			yield return new WaitForEndOfFrame ();
		}

		a.transform.position = bStartPosition;
		b.transform.position = aStartPosition;

		var temp = a.GridPositionView;
		a.GridPositionView = b.GridPositionView;
		b.GridPositionView = temp;

		mBoardObjects [a.GridPositionView.X, a.GridPositionView.Y, a.GridPositionView.Z] = a;
		mBoardObjects [b.GridPositionView.X, b.GridPositionView.Y, b.GridPositionView.Z] = b;

		if (finishedCallback != null)
		{
			finishedCallback (a, b);
		}
	}

	private void OnBoardObjectsSwappedFailed(BoardObject a, BoardObject b)
	{
		SetState (BoardState.CHECKING);
	}

	private void OnBoardObjectsSwapped(BoardObject a, BoardObject b)
	{
		var swappedBoardObjects = new BoardObject[] { a, b };
		mValidMathes.Clear ();
		foreach (BoardObject swappedBoardObject in swappedBoardObjects)
		{
			var match = BoardUtils.GetMatchingItems<BoardObject> (swappedBoardObject.GridPositionModel, mBoardObjects);

			if (match.HasMatchOfAtLeast (minMatchSize))
			{
				mValidMathes.Add (match);
			}
		}

		if (mValidMathes.Count > 0)
		{
			foreach (var validMatch in mValidMathes)
			{
				StartCoroutine (RemoveMatchedBoardObjectsInViewCoroutine (validMatch, OnMatchedBoardObjectsRemoved));
			}
		}
		else
		{
			//StartCoroutine(SwapBoardObjectsCoroutine(a, b, OnBoardObjectsSwappedFailed));
			SetState (BoardState.CHECKING);
		}
	}
}
