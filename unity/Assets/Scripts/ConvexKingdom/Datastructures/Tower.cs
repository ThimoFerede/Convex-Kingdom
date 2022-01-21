using UnityEngine;

namespace ConvexKingdom {
public class Tower {
    private float x;
    private float y;

    public Vector3 position { get; private set;}

    public const int width = 75;
    public const int height = 100;

    public Tower(float X, float Y) {
        this.x = X;
        this.y = Y;
        this.position = new Vector3(X, Y, 0);
    }

    public Vector3 GetScreenPosition(){
        return Camera.main.WorldToScreenPoint(position);
    }

    public float GetX() {
        return this.x;
    }

    public float GetY() {
        return this.y;
    }
}
}
