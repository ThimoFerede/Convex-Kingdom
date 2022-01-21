namespace ConvexKingdom{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using Util.Geometry;
	using Util.Geometry.DCEL;
	using Util.Geometry.Polygon;
	using Util.Geometry.Triangulation;
	using Util.Math;

	/// <summary>
	/// Construction of the Convex Hull
	/// Responsibility: Boris
	/// </summary>
	public static class ConvexHullGen
	{

		/// <summary>
		/// Creates Convex Hull.
		/// </summary>
		/// <returns> Convex Hull of the given points </returns>
		// public static ConvexHull GenerateConvexHull(/*...*/)
		// {
		// 	// TODO
		// 	return new ConvexHull();
		// }

	private static double getCross(Tower O, Tower A, Tower B)
    {
        return (A.GetX() - O.GetX()) * (B.GetY() - O.GetY()) - (A.GetY() - O.GetY()) * (B.GetX() - O.GetX());
    }

    public static List<Tower> GetConvexHull(List<Tower> points)
    {
        int n = points.Count(); 
        int k = 0;
        List<Tower> convexHull = new List<Tower>(new Tower[2*n]);

        // Handle degenerate list of points
        if (points == null || n <= 1)
            return points;

        points.Sort((a, b) =>
             a.GetX() == b.GetX() ? a.GetY().CompareTo(b.GetY()) : a.GetX().CompareTo(b.GetX()));

        // Calculate the lower hull
        for (int i = 0; i < n; ++i)
        {
            while (k >= 2 && getCross(convexHull[k - 2], convexHull[k - 1], points[i]) <= 0)
                k--;
            convexHull[k++] = points[i];
        }

        // Calculate the upper hull
        int threshold = k+1;
        for (int i = n - 2; i >= 0; i--)
        {
            while (k >= threshold && getCross(convexHull[k - 2], convexHull[k - 1], points[i]) <= 0)
                k--;
            convexHull[k++] = points[i];
        }

        return convexHull.Take(k - 1).ToList();
    }

    public static float CalculateConvexHullArea(List<Tower> towers, Camera transform) 
    {
        if(towers.Count < 3) return 0;
        
        List<Tower> towersWithLocalPos = new List<Tower>();

        // Transform them to local positions 
        foreach (Tower tower in towers) {
            var coordinates = transform.transform.InverseTransformDirection(new Vector2(tower.GetX(), tower.GetY()));
            Tower newTower = new Tower(coordinates[0], coordinates[1]);
            towersWithLocalPos.Add(newTower);
        }

        // Calculate area ^^
        towersWithLocalPos.Add(towersWithLocalPos[0]);
        var area = Math.Abs(towersWithLocalPos.Take(towersWithLocalPos.Count - 1)
            .Select((p, i) => (towersWithLocalPos[i + 1].GetX() - p.GetX()) * ((towersWithLocalPos[i + 1].GetY() + p.GetY())))
            .Sum() / 2);

        return area;
    }
		
		// Other functions:...
	}
}