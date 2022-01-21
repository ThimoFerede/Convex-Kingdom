using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ConvexKingdom;


public class DrawObjects: MonoBehaviour{
    private GameObject mCanvas;
    private List<GameObject> canvasTowers = new List<GameObject>();
    private List<GameObject> canvasLines = new List<GameObject>();

    private List<Tower> blueTowers = new List<Tower>();
    private List<Tower> redTowers = new List<Tower>();
    private List<Road> roads = new List<Road>();
    private List<Tower> availableTowers = new List<Tower>();
    private List<Vector2> resources = new List<Vector2>();
    
    public DrawObjects(GameObject canvas, List<Tower> availableTowers, List<Tower> redTowers, 
                        List<Tower> blueTowers, 
                        List<Road> roads, List<Vector2> resources): base()
    {
        this.mCanvas = canvas;
        this.availableTowers = availableTowers;
        this.redTowers = redTowers;
        this.blueTowers = blueTowers;
        this.roads = roads;
        this.resources = resources;
    }


    
    private void DrawLines(List<Tower> towers, Color color) {
        if (towers.Count < 2) return;
        for (int i = 1; i < towers.Count; i++) {
            // Debug.Log(towers[i].GetX() + "   " + towers[i].GetY());
            DrawLine(towers[i-1], towers[i], i, color, -1);
        }
        DrawLine(towers[0], towers[towers.Count-1], towers.Count, color, -1);
    }


    private void DrawLine(Tower start, Tower end, int id, Color cl, float z, float width=0.1f){
        GameObject imgObject = new GameObject("Line"+(id).ToString());
        var lr = imgObject.AddComponent<LineRenderer>();
        RectTransform trans = imgObject.AddComponent<RectTransform>();
        trans.transform.SetParent(mCanvas.transform); // setting parent

        Vector3 startpos = start.position;
        startpos.z = z;
        Vector3 endpos = end.position;
        endpos.z = z;

        lr.SetPosition(0, startpos);
        lr.SetPosition(1, endpos);
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = cl;
        lr.endColor = cl;

        canvasLines.Add(imgObject);
    }

    private void DrawRoads() {
        foreach (Road r in roads) {
            Vector2 coordinates1 = Camera.main.ScreenToWorldPoint(new Vector2(r.tower1.GetX(), r.tower1.GetY()));
            Tower t1 = new Tower(coordinates1.x, coordinates1.y);
            Vector2 coordinates2 = Camera.main.ScreenToWorldPoint(new Vector2(r.tower2.GetX(), r.tower2.GetY()));
            Tower t2 = new Tower(coordinates2.x, coordinates2.y);
            DrawLine(t1, t2, 100  + roads.IndexOf(r), Color.gray, 0);
            DrawLine(t1, t2, 100 + roads.Count + roads.IndexOf(r), Color.black, 1, 0.12f);
        }
    }
    
    public void UpdateCanvas(List<Tower> convexHullRed, List<Tower> convexHullBlue)
    {
        // clear all tower sprites
        foreach (GameObject tower in canvasTowers)
        {
            Destroy(tower);
        }

        // place available, blue, red tower sprites
        int counter = 0;
        foreach (Tower tower in availableTowers) {
            GameObject imgObject = new GameObject("Tower"+(counter++).ToString());

            RectTransform trans = imgObject.AddComponent<RectTransform>();
            trans.transform.SetParent(mCanvas.transform); // setting parent
            trans.localScale = Vector3.one;
            trans.anchoredPosition = new Vector2(0f, 0f); // setting position, will be on center
            trans.sizeDelta= new Vector2(Tower.width, Tower.height); // custom size
            Image image = imgObject.AddComponent<Image>();
            image.transform.position = new Vector2(tower.GetX(), tower.GetY());
            image.transform.localScale = new Vector3(1, 1, .1f);
            image.sprite = Resources.Load<Sprite>("gray");
            canvasTowers.Add(imgObject);
        }
        foreach (Tower tower in blueTowers) {
            GameObject imgObject = new GameObject("Tower"+(counter++).ToString());

            RectTransform trans = imgObject.AddComponent<RectTransform>();
            trans.transform.SetParent(mCanvas.transform); // setting parent
            trans.localScale = Vector3.one;
            trans.anchoredPosition = new Vector2(0f, 0f); // setting position, will be on center
            trans.sizeDelta= new Vector2(Tower.width, Tower.height); // custom size
            Image image = imgObject.AddComponent<Image>();
            image.transform.position = new Vector2(tower.GetX(), tower.GetY());
            image.transform.localScale = new Vector3(1, 1, .1f);
            image.sprite = Resources.Load<Sprite>("blue");
            canvasTowers.Add(imgObject);
        }
        foreach (Tower tower in redTowers) {
            GameObject imgObject = new GameObject("Tower"+(counter++).ToString());

            RectTransform trans = imgObject.AddComponent<RectTransform>();
            trans.transform.SetParent(mCanvas.transform); // setting parent
            trans.localScale = Vector3.one;
            trans.anchoredPosition = new Vector2(0f, 0f); // setting position, will be on center
            trans.sizeDelta= new Vector2(Tower.width, Tower.height); // custom size
            Image image = imgObject.AddComponent<Image>();
            image.transform.position = new Vector2(tower.GetX(), tower.GetY());
            image.transform.localScale = new Vector3(1, 1, .1f);
            image.sprite = Resources.Load<Sprite>("red");
            canvasTowers.Add(imgObject);
        }

        // place resources
        foreach (var r in resources) {
            GameObject imgObject = new GameObject("Resource" + resources.IndexOf(r).ToString());

            RectTransform trans = imgObject.AddComponent<RectTransform>();
            trans.transform.SetParent(mCanvas.transform); // setting parent
            trans.localScale = Vector3.one;
            trans.anchoredPosition = new Vector2(0f, 0f); // setting position, will be on center
            trans.sizeDelta= new Vector2(10, 10); // custom size
            Image image = imgObject.AddComponent<Image>();
            image.transform.position = r;
            image.transform.localScale = new Vector3(1, 1, .1f);
            image.sprite = Resources.Load<Sprite>("gold");
        }

        // clear lines
        foreach (GameObject line in canvasLines)
        {
            Destroy(line);
        }
        canvasLines.Clear();

        // draw convexhulls
        List<Tower> convexHullBlueWorld = new List<Tower>();
        List<Tower> convexHullRedWorld = new List<Tower>();

        // Convert coordinates to world positions
        foreach(Tower tower in convexHullBlue) {
            Vector2 coordinates = Camera.main.ScreenToWorldPoint(new Vector2(tower.GetX(), tower.GetY()));
            convexHullBlueWorld.Add(new Tower(coordinates.x, coordinates.y));
        }

        foreach(Tower tower in convexHullRed) {
            Vector2 coordinates = Camera.main.ScreenToWorldPoint(new Vector2(tower.GetX(), tower.GetY()));
            convexHullRedWorld.Add(new Tower(coordinates.x, coordinates.y));
        }
        DrawRoads();
        DrawLines(convexHullBlueWorld, Color.blue);
        DrawLines(convexHullRedWorld, Color.red);
    }
}