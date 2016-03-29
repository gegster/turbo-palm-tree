using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Match<T>
{
	public List<List<int>> MatchOffsets { get; set; }
	public List<List<T>> MatchItems { get; set; }
	public int[] RootPosition { get; private set; }
	public T RootItem { get; private set; }

	public Match()
	{
		MatchOffsets = new List<List<int>> ();
		MatchItems = new List<List<T>> ();
	}

	public Match(int[] position, T item)
	{
		RootPosition = position;
		RootItem = item;

		int dimensions = position.Length;
		MatchOffsets = new List<List<int>> (dimensions);
		MatchItems = new List<List<T>> (dimensions);
		for (int i = 0; i < dimensions; ++i)
		{
			MatchOffsets.Add(new List<int> ());
			MatchItems.Add(new List<T> ());
		}
	}
		
	public bool HasMatchOfAtLeast(int matchSize)
	{
		for (int i = 0; i < MatchOffsets.Count; ++i)
		{
			// rather than store a zero offset for each dimension we just reduce the match size by one
			if (MatchOffsets [i].Count >= matchSize-1) // 
			{
				return true;
			}
		}

		return false;
	}

	public List<int> GetMatchOffsetsAsFlatList()
	{
		var flatList = new List<int>();
		MatchOffsets.ForEach (element => flatList.AddRange (element));
		return flatList;
	}

	public List<T> GetMatchItemsAsFlatList()
	{
		var flatList = new List<T>();
		flatList.Add (RootItem);
		MatchItems.ForEach (element => flatList.AddRange (element));
		return flatList;
	}
}