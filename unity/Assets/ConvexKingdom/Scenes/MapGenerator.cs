using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ConvexKingdom;


public class MapGenerator: MonoBehaviour{
    private List<Tower> blueTowers = new List<Tower>();
    private List<Tower> redTowers = new List<Tower>();
    private List<Road> roads = new List<Road>();
    private List<Tower> availableTowers = new List<Tower>();
    private List<Vector2> resources = new List<Vector2>();
    
    public MapGenerator(List<Tower> availableTowers, List<Tower> redTowers, 
                        List<Tower> blueTowers, 
                        List<Road> roads, List<Vector2> resources): base()
    {
        this.availableTowers = availableTowers;
        this.redTowers = redTowers;
        this.blueTowers = blueTowers;
        this.roads = roads;
        this.resources = resources;
    }


    public void PlaceTowersAtRandom(bool isFinished) {
        if (isFinished) return;

        for (int i = 0; i < 2*UI.TOWERS_PER_SIDE; i++) {
            Vector2 randPos = Camera.main.ViewportToScreenPoint(new Vector2(Random.value * 0.8f + 0.1f, Random.value * 0.8f + 0.1f));

            // While there is a nearby tower get new coordinates
            while (hasNearbyTower(randPos.x, randPos.y)) {
                randPos = Camera.main.ViewportToScreenPoint(new Vector2(Random.value * 0.8f + 0.1f, Random.value * 0.8f + 0.1f));
            }
            availableTowers.Add(new Tower(randPos.x, randPos.y));
        }
        GenerateRandomResources();
        ConnectTowers();
    }
    private void ConnectTowers(){
        // modified MST with multiple edges crossing each cut
        roads.Clear();
        if (availableTowers.Count == 0) return;
        List<Tower> Unconnected = new List<Tower>(availableTowers);
        List<Tower> Connected = new List<Tower>();

        int roadPerCut = 4; // number of desired roads crossing a cut

        Connected.Add(availableTowers[0]);
        Unconnected.Remove(availableTowers[0]);
        List<Road> shortest_roads = new List<Road>();

        while (Connected.Count < availableTowers.Count) {
            Road shortest = null;
            float min_road_length = float.PositiveInfinity;
            for (int i = 0; i < Connected.Count; i++) {
                Tower t1 = Connected[i];
                Tower closest = null;
                float min_dist = float.PositiveInfinity;

                for (int j = 0; j < Unconnected.Count; j++) {
                    Tower t2 = Unconnected[j];
                    float dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(t1.GetX() - t2.GetX()),2) + Mathf.Pow(Mathf.Abs(t1.GetY() - t2.GetY()),2));

                    if (dist < min_dist)
                    {
                        closest = t2;
                        min_dist = dist;
                    }
                }

                if (min_dist < min_road_length)
                {
                    // second tower is the one in "Unconnected"
                    shortest = new Road(t1, closest);
                    min_road_length = min_dist;
                }
            }

            if (shortest != null) {
                shortest_roads.Add(shortest);
                // Debug.Log("Found " + shortest_roads.Count + " crossing cut (so far)");
                Unconnected.Remove(shortest.tower2);
                // Connected.Add(shortest.tower2);
                roads.Add(shortest);
            }
            if (shortest == null || shortest_roads.Count == roadPerCut) {
                if (shortest_roads.Count > 0) Connected.Add(shortest_roads[0].tower2); // mark first as connected
                for (int i = 1; i < shortest_roads.Count; i++) Unconnected.Add(shortest_roads[i].tower2); // mark others as unconnected
                shortest_roads.Clear();
            }
        }
    }


    private bool isPointInTower(float x, float y, Tower tower, Vector2 threshold){
        if (tower.GetX() > x - threshold.x && tower.GetX() < x + threshold.x) {
            if (tower.GetY() > y - threshold.y && tower.GetY() < y + threshold.y) {
                return true;
            }
        }
        return false;
    }

    private bool hasNearbyTower(float x, float y) {
        Vector2 threshold = Camera.main.ViewportToScreenPoint(new Vector2(0.05f, 0.05f));

        foreach (Tower tower in availableTowers) {
            if (isPointInTower(x, y, tower, threshold)) return true;
            if (tower.GetX() > x - 1 && tower.GetX() < x + 1) return true; // assure general positions
        }
        
        return false;
    }

    private void GenerateRandomResources(){
        Vector2 threshold = Camera.main.ViewportToScreenPoint(new Vector2(0.05f, 0.05f));
        for (int i = 0; i < 4*UI.TOWERS_PER_SIDE; i++) {
            Vector2 randPos = Camera.main.ViewportToScreenPoint(new Vector2(Random.value * 0.7f + 0.15f, Random.value * 0.7f + 0.15f));
            var insideTower = availableTowers.Exists(t => isPointInTower(randPos.x, randPos.y, t, threshold));
            if(insideTower){
                randPos.x -= Tower.width;
                randPos.y -= Tower.height;
            }
            resources.Add(randPos);
        }
    }
}