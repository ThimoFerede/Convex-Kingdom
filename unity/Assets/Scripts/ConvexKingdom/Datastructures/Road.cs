using UnityEngine;

namespace ConvexKingdom {
public class Road {
    public Tower tower1;
    public Tower tower2;
    public Road (Tower tower1, Tower tower2)
    {
        if (tower1 == null || tower2 == null)
            throw new System.ArgumentNullException();
        this.tower1 = tower1;
        this.tower2 = tower2;
    }
}
}
