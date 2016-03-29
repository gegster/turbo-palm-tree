using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BoardUtils 
{
	public static readonly int MAX_ADJACENT_ITEMS = 6;

	public static BoardObject[,,] CreateBoardObjects(int[,,] objectTypes, int[,,] objectColours, Board board)
	{
		var boardObjects = new BoardObject[objectTypes.GetLength(0), objectTypes.GetLength(1), objectTypes.GetLength(2)];
		for (int z = 0; z < objectTypes.GetLength(2); ++z)
		{
			for (int y = 0; y < objectTypes.GetLength(1); ++y)
			{
				for (int x = 0; x < objectTypes.GetLength(0); ++x)
				{
					var type = (BoardObject.BOType)objectTypes [x, y, z];
					var colour = (BoardObject.BOColour) objectColours [x, y, z];
					var boardObject = CreateBoardObject (x, y, z, type, colour, board.gameObject.transform);
					boardObjects [x, y, z] = boardObject;
				}
			}
		}
		return boardObjects;
	}

	public static BoardObject[,,] CreateOuterBoardObjects(int[,,] objectTypes, int[,,] objectColours, Board board)
	{
		var boardObjects = new BoardObject[objectTypes.GetLength(0), objectTypes.GetLength(1), objectTypes.GetLength(2)];
		for (int z = 0; z < objectTypes.GetLength(2); ++z)
		{
			for (int y = 0; y < objectTypes.GetLength(1); ++y)
			{
				for (int x = 0; x < objectTypes.GetLength(0); ++x)
				{
					if ( (z == 0)
						|| ( x == objectTypes.GetLength (0) - 1 )
						|| ( y == objectTypes.GetLength (1) - 1 ))
					{
						var type = (BoardObject.BOType)objectTypes [x, y, z];
						var colour = (BoardObject.BOColour) objectColours [x, y, z];
						var boardObject = CreateBoardObject (x, y, z, type, colour, board.gameObject.transform);
						boardObjects [x, y, z] = boardObject;
					}
				}
			}
		}
		return boardObjects;
	}

	public static bool HasEmptySlots(BoardObject[,,] boardObjects)
	{
		return GetEmptySlots (boardObjects).Count > 0;
	}

	public static List<BoardObject> GetEmptySlots(BoardObject[,,] boardObjects)
	{
		var emptyBoardObjects = new List<BoardObject> ();

		for (int z = 0; z < boardObjects.GetLength (2); ++z)
		{
			for (int y = 0; y < boardObjects.GetLength (1); ++y)
			{
				for (int x = 0; x < boardObjects.GetLength (0); ++x)
				{
					var boardObject = boardObjects [x, y, z];
					if (boardObject != null)
					{
						if( boardObject.BoardObjectType == BoardObject.BOType.EMPTY )
						{
							emptyBoardObjects.Add (boardObjects [x, y, z]);
						}
					}
				}
			}
		}

		return emptyBoardObjects;
	}

	private static BoardObject CreateBoardObject(int x, int y, int z, BoardObject.BOType type, BoardObject.BOColour colour, Transform parent)
	{
		var boardObjectFactory = Component.FindObjectOfType<BoardObjectFactory> ();

		var realColour = BoardObject.GetRealColour (colour);

		type = BoardObject.BOType.CUBE;   // hack to get same object type

		var boardObject = boardObjectFactory.CreateBoardObject (type, realColour);
		boardObject.Colour = colour;
		boardObject.GridPositionView.Set (x, y, z);
		boardObject.GridPositionModel.Set (x, y, z);
		boardObject.transform.position = new Vector3 (x, y, z);
		boardObject.transform.SetParent (parent, false);

		return boardObject;
	}

	public static int[,,] GetRandomValues(int sizeX, int sizeY, int sizeZ, int numTypes)
	{
		int[,,] randomValues = new int[sizeX, sizeY, sizeZ];
		for (int z = 0; z < sizeZ; ++z)
		{
			for (int y = 0; y < sizeY; ++y)
			{
				for (int x = 0; x < sizeX; ++x)
				{
					randomValues [x, y, z] = UnityEngine.Random.Range (0, numTypes);
				}
			}
		}
		return randomValues;
	}

	public static int[,,] RemoveMatches(int[,,] values, int matchSize, int maxValue)
	{
		List<int> availableValues = new List<int> (maxValue);
		for (int i = 0; i < maxValue; ++i)
		{
			availableValues.Add (i);
		}

		for (int z = 0; z < values.GetLength(2); ++z)
		{
			for (int y = 0; y < values.GetLength(1); ++y)
			{
				for (int x = 0; x < values.GetLength(0); ++x)
				{
					Match<int> match = GetMatchingItems<int> (x, y, z, values);
					if( match.HasMatchOfAtLeast(matchSize) )
					{
						var adjacentValues = GetAdjacentItems<int> (x, y, z, values);
						var value = availableValues.Find (element => adjacentValues.Contains (element) == false);
						values [x, y, z] = value;
					}
				}
			}
		}
		return values;
	}

	public static Match<T> GetMatchingItems<T>(Point3 gridPosition, T[,,] items)
	{
		return GetMatchingItems<T> (gridPosition.X, gridPosition.Y, gridPosition.Z, items);
	}

	public static Match<T> GetMatchingItems<T>(int x, int y, int z, T[,,] items)
	{
		int[] directionValues = new int[] { -1, 1 };
		int[] indices = new int[] { x, y, z };
		int[] dimensions = new int[] {
			items.GetLength (0),
			items.GetLength (1),
			items.GetLength (2)
		};
		var item = items [x, y, z];

		var match = new Match<T> (new int[] { x, y, z }, item);

		for( int bound = 0; bound < 3; ++bound)
		{
			for (int direction = 0; direction < 2; ++direction)
			{
				bool hasMatchingAdjacentItem = false;
				int indexIncrement = 1;
				do
				{
					hasMatchingAdjacentItem = false;

					int adjacentIndex = indices [bound] + (indexIncrement * directionValues [direction]);
					int xIndex = bound == 0 ? adjacentIndex : x;
					int yIndex = bound == 1 ? adjacentIndex : y;
					int zIndex = bound == 2 ? adjacentIndex : z;

					if (adjacentIndex >= 0 && adjacentIndex < dimensions [bound])
					{
						var adjacentItem = items [xIndex, yIndex, zIndex];
						if (item.Equals (adjacentItem))
						{
							match.MatchItems[bound].Add(adjacentItem);
							match.MatchOffsets [bound].Add (indexIncrement * directionValues [direction]);
							hasMatchingAdjacentItem = true;
							indexIncrement++;
						}
					}
				}
				while(hasMatchingAdjacentItem);
			}
		}

		return match;
	}



	private static List<T> GetAdjacentItems<T>(int x, int y, int z, T[,,] items)
	{
		List<T> adjacentValues = new List<T>(MAX_ADJACENT_ITEMS);

		int[] directionValues = new int[] { -1, 1 };
		int[] indices = new int[] { x, y, z };
		int[] dimensions = new int[] {
			items.GetLength (0),
			items.GetLength (1),
			items.GetLength (2)
		};

		for( int bound = 0; bound < 3; ++bound)
		{
			for (int direction = 0; direction < 2; ++direction)
			{
				int offset = 1;
				int adjacentIndex = indices [bound] + (offset * directionValues[direction]);
				int xIndex = bound == 0 ? adjacentIndex : x;
				int yIndex = bound == 1 ? adjacentIndex : y;
				int zIndex = bound == 2 ? adjacentIndex : z;

				if (adjacentIndex >= 0 && adjacentIndex < dimensions [bound])
				{
					var adjacentItem = items [xIndex, yIndex, zIndex];
					adjacentValues.Add (adjacentItem);
				}
			}
		}

		return adjacentValues;
	}

	public static List<Board.BoardFace> GetFace(Point3 position, Point3 extents)
	{
		var faces = new List<Board.BoardFace> ();

		if (position.Z == 0)
		{
			faces.Add (Board.BoardFace.X_Y);
		}
		if (position.X == extents.X - 1)
		{
			faces.Add (Board.BoardFace.Z_Y);
		}
		if (position.Y == extents.Y - 1)
		{
			faces.Add (Board.BoardFace.Z_NX);
		}

		return faces;
	}
}
