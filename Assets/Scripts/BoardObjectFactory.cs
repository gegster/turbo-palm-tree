using UnityEngine;
using System.Collections;

public class BoardObjectFactory : MonoBehaviour 
{
	public BoardObject boardObjectResource;
	public GameObject[] boardObjectMeshResources;
	public Color[] typeColours;

	public BoardObject CreateBoardObject(BoardObject.BOType type, Color colour)
	{
		var boardObject = GameObject.Instantiate (boardObjectResource);
		boardObject.BoardObjectType = type;
		boardObject.Mesh = GameObject.Instantiate (boardObjectMeshResources [(int)type]);
		boardObject.Mesh.GetComponent<Renderer> ().material.color = colour;
		return boardObject;
	}
}
