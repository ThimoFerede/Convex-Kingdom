using General.Menu;
using General.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Geometry.Polygon;
using Util.Algorithms.Polygon;
using Util.Geometry;

public class Controller : MonoBehaviour
{
	void Start()
	{
		// Construct map
	}

	void Update()
	{
		// Get user input and apply action
	}

	public void ClaimTower(Tower tower /*, player? */)
	{
		// Add to players kingdom
		// If unclaimed: (remove from other players kingdom)
		// Call UpdateKingdoms()
	}

	public void BattleForTower(Tower tower /*, attacker, defender? */)
	{
		// Chance based on score ratio
		// Call ClaimTower(...) for winner
	}
	
	private void UpdateKingdoms()
	{
		// Compute convexhull
		// Compute vertical decomposition
		// Compute scores (#resources within kingdom)
	}
}
