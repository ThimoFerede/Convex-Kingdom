using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ConvexKingdom;
using Util.Geometry.Polygon;
using Util.DataStructures.VerticalDecomposition;

public class UI : MonoBehaviour
{
    public const int TOWERS_PER_SIDE = 10;
    private bool isRedTurn = true;
    private List<Tower> blueTowers = new List<Tower>();
    private List<Tower> redTowers = new List<Tower>();
    private List<Road> roads = new List<Road>();
    private List<GameObject> canvasTowers = new List<GameObject>();
    private List<GameObject> canvasLines = new List<GameObject>();
    // private int counter = 0;
    private List<Tower> convexHullBlue = new List<Tower>();
    private List<Tower> convexHullRed = new List<Tower>();
    private List<Tower> availableTowers = new List<Tower>();
    private List<int> capturedTowersIndex = new List<int>();
    private VerticalDecomposition blueVD = new VerticalDecomposition(new Polygon2D());
    private VerticalDecomposition redVD = new VerticalDecomposition(new Polygon2D());
    private int blueScore = 0;
    private int redScore = 0;

    private List<Vector2> resources = new List<Vector2>();

    private DrawObjects drawObj;
    private MapGenerator mapGenerator;

    bool isStarted = false;
    bool isFinished = false;
    

    // Start is called before the first frame update
    void Start(){
        GameObject mCanvas = GameObject.Find("Canvas");
        drawObj = new DrawObjects(mCanvas, availableTowers, redTowers, blueTowers, roads, resources);
        mapGenerator = new MapGenerator(availableTowers, redTowers, blueTowers, roads, resources);
        Button btnObject = mCanvas.GetComponentInChildren<Button>();
        btnObject.enabled = true;
        if(!isStarted){
            mapGenerator.PlaceTowersAtRandom(isFinished);
            drawObj.UpdateCanvas(convexHullRed, convexHullBlue);
            isStarted = true;
        }
    }

    // Update is called once per frame
    void Update(){
        
    }

    public void ReturnToMenu() {
        SceneManager.LoadScene("Menu");
    }

     public void GoToInstructions() {
        SceneManager.LoadScene("Instructions");
    }

    public void PlaceTower() {
        SelectTowerOnMap();
    }

    private bool IsInvalidCoordinate(Vector3 mousePos) {
        // If there is already a tower on this position
        // var sreenMousePos = Camera.main.WorldToScreenPoint(mousePos, 0);

        Tower tower1 = this.redTowers.Find(o => IsMouseInRect(mousePos, o.GetScreenPosition(), Tower.width, Tower.height));
        if (tower1 != null) return true;
        
        Tower tower2 = this.blueTowers.Find(o => IsMouseInRect(mousePos, o.GetScreenPosition(), Tower.width, Tower.height));
        if (tower2 != null) return true;

        GameObject canvas = GameObject.Find("Canvas");
        Button btnObject = canvas.GetComponentInChildren<Button>();
        if (IsMouseInRect(mousePos, btnObject.transform.position, 300, 180)) return true;

        return false;
    }


    private bool IsMouseInRect(Vector3 mousePos, Vector3 center, int width, int height){
        return (mousePos.x >= center.x - width/2 && mousePos.x <= center.x + width/2) && 
        (mousePos.y >= center.y - height/2 && mousePos.y <= center.y + height/2); 
    }

    public void SelectTowerOnMap() {
        Vector2 mousePos = Input.mousePosition;

        Tower selectedTower = availableTowers.Find(o => o.GetX() > mousePos.x - 40 && o.GetX() < mousePos.x + 40 && o.GetY() > mousePos.y - 40 && o.GetY() < mousePos.y + 40);

        if (selectedTower != null) {
            Vector2 coordinates = Camera.main.ScreenToWorldPoint(new Vector2(selectedTower.GetX(), selectedTower.GetY()));

            var player = isRedTurn ? Player.Red : Player.Blue;
            if (!ClaimTower(player, selectedTower)) return;

            isRedTurn = !isRedTurn;

            while(availableTowers.Count>0 && IsThereLoop()){
                var randomTower = availableTowers[Random.Range(0, availableTowers.Count)];
                availableTowers.Remove(randomTower);
                player = isRedTurn ? Player.Red : Player.Blue;
                ClaimTower(player, randomTower, false);

                isRedTurn = !isRedTurn;
            }
        }

        ShowScores();
    }


    private bool IsThereLoop(){
        var towers = isRedTurn ? redTowers: blueTowers;
        if(towers.Count == 0){
            return false;
        }
        
        foreach(Tower tower in towers){
            bool possiblePath = false;
            foreach (Road r in roads) {
                if (
                    (r.tower1 == tower && availableTowers.Contains(r.tower2)) ||
                    (r.tower2 == tower && availableTowers.Contains(r.tower1)) 
                ){
                    possiblePath = true;
                    break;
                }
            }
            if(possiblePath){
               return false; 
            }
        }
        return true;
    }




    private void ShowScores() {
        Text tbTurn = GameObject.Find("Turn").GetComponent<Text>();
        Text tbWinner = GameObject.Find("Winner").GetComponent<Text>();
        tbWinner.text = "";

        var redArea = ConvexHullGen.CalculateConvexHullArea(convexHullRed, Camera.main);
        var blueArea = ConvexHullGen.CalculateConvexHullArea(convexHullBlue, Camera.main);


        if (redArea > blueArea) {
            tbWinner.text = "Player Red has larger area and "+redScore+" gold coins!";
            tbWinner.color = Color.red;
        } else if (redArea == blueArea) {
            tbWinner.text = "The areas are equal!";
            tbWinner.color = Color.green;
        } else {
            tbWinner.text = "Player Blue has larger area and "+blueScore+" gold coins!";
            tbWinner.color = Color.blue;
        }
        // tbTurn.text = "Game Over!";

        GameObject canvas = GameObject.Find("Canvas");
        Button btnObject = canvas.GetComponentInChildren<Button>();
        // Button btnRestart = btnObject.GetComponent<Button>();
        btnObject.GetComponentInChildren<Text>().text = "Go to menu";
        btnObject.enabled = true;
        // this.isFinished = true;

        UpdateVerticalDecomp(Player.Red);
        UpdateVerticalDecomp(Player.Blue);

        return;
    }

    private enum Player { Red, Blue }
    private bool ClaimTower(Player player, Tower tower, bool connectionNeeded = true) {
        if(connectionNeeded){
            bool connected = false;
            foreach (Road r in roads) {
                if (player == Player.Blue) {
                    if (blueTowers.Count == 0) {connected = true; break;}
                    if ((r.tower1 == tower && blueTowers.Contains(r.tower2)) || (r.tower2 == tower&& blueTowers.Contains(r.tower1))){
                        connected = true;
                        break;
                    }
                }
                if (player == Player.Red) {
                    if (redTowers.Count == 0) {connected = true; break;}
                    if ((r.tower1 == tower && redTowers.Contains(r.tower2)) || (r.tower2 == tower&& redTowers.Contains(r.tower1))){
                        connected = true;
                        break;
                    }
                }
            }
            if (!connected) return false;
        }

        if (availableTowers.Contains(tower)) availableTowers.Remove(tower);
        // TODO: claiming tower of other player

        if (player == Player.Blue) {
            // Add tower and update convexhull, vertical decomposition
            blueTowers.Add(tower);
            convexHullBlue = ConvexHullGen.GetConvexHull(blueTowers);
            UpdateVerticalDecomp(Player.Blue);

            // Capture encapsulated towers
            Debug.Log("Checking towers in blue convexhull...");
            bool change = false;
            List<Tower> capturedTowers = new List<Tower>();
            foreach (Tower t in redTowers) {
                if (blueVD.PointInPolygon(t.position)) {
                    Debug.Log("Tower captured!");
                    capturedTowers.Add(t);
                    change = true;
                }
            }
            foreach (Tower t in availableTowers) {
                if (blueVD.PointInPolygon(t.position)) {
                    Debug.Log("Tower captured!");
                    capturedTowers.Add(t);
                    change = true;
                }
            }
            foreach (Tower t in capturedTowers)
            {
                if (availableTowers.Contains(t)) availableTowers.Remove(t);
                else if (redTowers.Contains(t)) redTowers.Remove(t);
                blueTowers.Add(t);
            }

            // Update convexhull, vertical decomposition
            if (change) {
                convexHullRed = ConvexHullGen.GetConvexHull(redTowers);
                UpdateVerticalDecomp(Player.Red);
            }
        }

        else if (player == Player.Red) {
            // Add tower and update convexhull, vertical decomposition
            redTowers.Add(tower);
            convexHullRed = ConvexHullGen.GetConvexHull(redTowers);
            UpdateVerticalDecomp(Player.Red);

            // Capture encapsulated towers
            Debug.Log("Checking towers in red convexhull...");
            bool change = false;
            List<Tower> capturedTowers = new List<Tower>();
            foreach (Tower t in blueTowers) {
                if (redVD.PointInPolygon(t.position)) {
                    Debug.Log("Tower captured!");
                    capturedTowers.Add(t);
                    change = true;
                }
            }
            foreach (Tower t in availableTowers) {
                if (redVD.PointInPolygon(t.position)) {
                    Debug.Log("Tower captured!");
                    capturedTowers.Add(t);
                    change = true;
                }
            }
            foreach (Tower t in capturedTowers)
            {
                if (availableTowers.Contains(t)) availableTowers.Remove(t);
                else if (blueTowers.Contains(t)) blueTowers.Remove(t);
                redTowers.Add(t);
            }

            // Update convexhull, vertical decomposition
            if (change) {
                convexHullBlue = ConvexHullGen.GetConvexHull(blueTowers);
                UpdateVerticalDecomp(Player.Blue);
            }
        }
        UpdateScores();
        drawObj.UpdateCanvas(convexHullRed, convexHullBlue);
        return true;
    }
    private void UpdateScores() {
        Debug.Log("Updating score");
        blueScore = 0;
        redScore = 0;
        // Debug.Log("Checking resources in convexhulls...");
        foreach(var r in resources){
            if(blueVD.PointInPolygon(r)){
                // Debug.Log("blue score ++");
                blueScore++;
            }
            if(redVD.PointInPolygon(r)){
                // Debug.Log("red score ++");
                redScore++;
            }
        }
    }
    
    private void UpdateVerticalDecomp(Player player) {
        List<Vector2> vertices = new List<Vector2>();
        List<Tower> towers = player == Player.Blue ? convexHullBlue : convexHullRed;
        foreach (Tower t in towers) {
            vertices.Add(t.position);
        }

        Polygon2D p = new Polygon2D(vertices);
        VerticalDecomposition vd = new VerticalDecomposition(p);
        if (player == Player.Blue) blueVD = vd;
        else if (player == Player.Red) redVD = vd;

        // draw trapezoids
        List<VerticalDecomposition.Trapezoid> trapezoids = vd.GetAllTrapezoids();
        foreach (VerticalDecomposition.Trapezoid t in trapezoids) {
            Color c = Color.white;
            if (!t.in_polygon) continue;
            int margin = 0; // = 5; for checking trapezoid overlap bugs

            Vector3 top_left =  new Vector3(t.left.x + margin,  t.top.Line.Y(t.left.x + margin)     - margin, 0);
            Vector3 top_right = new Vector3(t.right.x - margin, t.top.Line.Y(t.right.x - margin)    - margin, 0);
            Vector3 bot_right = new Vector3(t.right.x - margin, t.bottom.Line.Y(t.right.x - margin) + margin, 0);
            Vector3 bot_left =  new Vector3(t.left.x + margin,  t.bottom.Line.Y(t.left.x + margin)  + margin, 0);

            top_left = Camera.main.ScreenToWorldPoint(top_left, 0);
            top_right = Camera.main.ScreenToWorldPoint(top_right, 0);
            bot_right = Camera.main.ScreenToWorldPoint(bot_right, 0);
            bot_left = Camera.main.ScreenToWorldPoint(bot_left, 0);
            Vector3 center = Camera.main.ScreenToWorldPoint(t.center, 0);

            int dur = availableTowers.Count == 0 ? 10 : 1;

            // edges
            Debug.DrawLine(top_left, top_right, c, dur);
            Debug.DrawLine(top_right, bot_right, c, dur);
            Debug.DrawLine(bot_right, bot_left, c, dur);
            Debug.DrawLine(bot_left, top_left, c, dur);

            // center cross
            Debug.DrawLine(new Vector3(center.x + 0.05f, center.y), new Vector3(center.x - 0.05f, center.y), c, dur);
            Debug.DrawLine(new Vector3(center.x, center.y + 0.05f), new Vector3(center.x, center.y - 0.05f), c, dur);

            Vector3 delta = new Vector3(0, 0.05f, 0);
            foreach (VerticalDecomposition.Trapezoid rn in t.rightNeighbors) {
                Vector3 neighbor_center = Camera.main.ScreenToWorldPoint(rn.center, 0);
                Debug.DrawLine(center + delta, neighbor_center + delta, Color.red, dur);
            }
            foreach (VerticalDecomposition.Trapezoid ln in t.leftNeighbors) {
                Vector3 neighbor_center = Camera.main.ScreenToWorldPoint(ln.center, 0);
                Debug.DrawLine(center - delta, neighbor_center - delta, Color.blue, dur);
            }

            // Debug.Log("Drawing " + top_left.ToString() + " " + top_right.ToString() + " " + bot_right.ToString() + " " + bot_left.ToString());
        }
    }
}
