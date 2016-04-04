using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardObjectMove 
{
	public BoardObject BoardObject { get; private set; }
	public List<Point3> Moves { get; private set; }

	public BoardObjectMove(BoardObject boardObject, List<Point3> moves)
	{
		BoardObject = boardObject;
		Moves = moves;
	}
}