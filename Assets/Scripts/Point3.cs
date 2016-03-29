using UnityEngine;
using System.Collections;

public class Point3
{
	public static Point3 POS_X = new Point3 (1, 0, 0);
	public static Point3 POS_Y = new Point3 (0, 1, 0);
	public static Point3 POS_Z = new Point3 (0, 0, 1);
	public static Point3 NEG_X = new Point3 (-1, 0, 0);
	public static Point3 NEG_Y = new Point3 (0, -1, 0);
	public static Point3 NEG_Z = new Point3 (0, 0, -1);

	public int X { get; set; }
	public int Y { get; set; }
	public int Z { get; set; }

	public Point3()
	{
		X = 0;
		Y = 0;
		Z = 0;
	}

	public Point3(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Point3(Point3 pos)
	{
		X = pos.X;
		Y = pos.Y;
		Z = pos.Z;
	}

	public void Set(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public override bool Equals(System.Object obj) 
	{
		return obj is Point3 && this == (Point3)obj;
	}

	public override int GetHashCode() 
	{
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	}

	public static bool operator ==(Point3 a, Point3 b) 
	{
		return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
	}

	public static bool operator !=(Point3 a, Point3 b) 
	{
		return !(a == b);
	}
		
	public static Point3 operator -(Point3 a, Point3 b) 
	{
		return new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
	}

	public static Point3 operator +(Point3 a, Point3 b) 
	{
		return new Point3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}, {2}]", X, Y, Z);
	}
}