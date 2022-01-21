namespace Util.DataStructures.VerticalDecomposition
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Implementation of a vertial decomposition.
    /// </summary>
    public class VerticalDecomposition
    {
        public static float ROUNDING_ERROR = 0.1f;
        public Node root;
        private Trapezoid bbox;
        public VerticalDecomposition(Polygon2D polygon, float bbox_margin = 100)
        {
            if (polygon.VertexCount == 0) return;
            float xmin = float.NaN;
            float ymin = float.NaN;
            float xmax = float.NaN;
            float ymax = float.NaN;

            foreach (Vector2 v in polygon.Vertices) {
                if (v.x < xmin || float.IsNaN(xmin)) xmin = v.x;
                if (v.y < ymin || float.IsNaN(ymin)) ymin = v.y;
                if (v.x > xmax || float.IsNaN(xmax)) xmax = v.x;
                if (v.y > ymax || float.IsNaN(ymax)) ymax = v.y;
            }

            // Build bounding box
            // Margin for nicer visualization
            Vector2 topleft  = new Vector2(xmin - bbox_margin, ymax + bbox_margin);
            Vector2 topright = new Vector2(xmax + bbox_margin, ymax + bbox_margin);
            Vector2 botright = new Vector2(xmax + bbox_margin, ymin - bbox_margin);
            Vector2 botleft  = new Vector2(xmin - bbox_margin, ymin - bbox_margin);
            bbox = new Trapezoid(
                new LineSegment(topleft, topright),
                new LineSegment(botleft, botright),
                topleft, topright
            );
            root = new leafNode(bbox);

            Debug.Log("VD bbox: " + topleft.ToString() + " " + topright.ToString() + " " + botright.ToString() + " " + botleft.ToString());

            foreach (LineSegment l in polygon.Segments) {
                insertLineSegment(l);
            }
        }
        public bool PointInPolygon(Vector2 p)
        {
            if (root == null) return false;
            
            Trapezoid trapezoid = root.GetTrapezoid(p);
            Debug.Log("Query point" + p.ToString() + " in trapezoid " + trapezoid.ToString() + " in polygon? : " + trapezoid.in_polygon);
            return trapezoid.in_polygon;
        }
        public List<Trapezoid> GetAllTrapezoids()
        {
            if (root == null) return new List<Trapezoid>();
            return root.GetAllTrapezoids();
        }
        private void insertLineSegment(LineSegment s)
        {
            Debug.Log("Adding linesegment " + s.ToString() + " to VD");
            //  x------ ... ------x
            //  |     | ... |     |
            //  |   x----s----x   |
            //  |     | ... |     |
            //  x------ ... ------x
            List<Trapezoid> trapezoids_old = findInterectingTrapezoids(s);
            if (trapezoids_old.Count == 0) throw new Exception("no intersecting trapezoids!");
            // Debug.Log("Found " + trapezoids_old.Count + " intersecting trapezoids");

            Vector2 l = (s.Point1.x < s.Point2.x) ? s.Point1 : s.Point2;
            Vector2 r = (s.Point1.x >= s.Point2.x) ? s.Point1 : s.Point2;
            Trapezoid L_old = trapezoids_old[0];
            Trapezoid R_old = trapezoids_old[trapezoids_old.Count - 1];
            // Debug.Log("L_old = " + L_old.ToString());
            // Debug.Log("R_old = " + R_old.ToString());

            //  x------ ... ------|
            //  | L     ...     R |
            //  |   l----s----r   |
            //  |       ...       |
            //  |------ ... ------x

            // create new left most and right most trapezoids
            //  x------ ... ------|
            //  | L'|   ...   | R'|
            //  |   l----s----r   |
            //  |   |   ...   |   |
            //  |------ ... ------x
            Trapezoid L_new = new Trapezoid(L_old.top, L_old.bottom, L_old.left, l          );
            Trapezoid R_new = new Trapezoid(R_old.top, R_old.bottom, r,          R_old.right);

            // only works for single convex polygon:
            L_new.in_polygon = L_old.left != bbox.left;
            R_new.in_polygon = R_old.right != bbox.right;

            // link neighbors
            if (!L_new.AreaZero()) {
                foreach (Trapezoid t in L_old.leftNeighbors) {
                    makeNeighbors(t, L_new);
                }
            }
            if (!R_new.AreaZero()) {
                foreach (Trapezoid t in R_old.rightNeighbors) {
                    makeNeighbors(t, R_new);
                }
            }

            // make nodes
            leafNode L_node = new leafNode(L_new);
            leafNode R_node = new leafNode(R_new);

            if (L_old == R_old)
            {
                // Debug.Log("segment within single trapezoid");
                // create middle trapezoids
                //  x---------------|
                //  | L'|   T   | R'|
                //  |   l---s---r   |
                //  |   |   B   |   |
                //  |---------------x
                Trapezoid T = new Trapezoid(L_old.top, s,            l, r);
                Trapezoid B = new Trapezoid(s,         L_old.bottom, l, r);

                // only works for single convex polygon:
                T.in_polygon = T.top != bbox.top;
                B.in_polygon = B.bottom != bbox.bottom;

                // fix neighbors
                if (L_new.AreaZero()) {
                    foreach (Trapezoid t in L_old.leftNeighbors) {
                        if (t.topRight.y > l.y + ROUNDING_ERROR) makeNeighbors(t,T);
                        if (t.bottomRight.y < l.y - ROUNDING_ERROR) makeNeighbors(t,B);
                    }
                }
                else {
                    makeNeighbors(L_new, T);
                    makeNeighbors(L_new, B);
                }
                if (R_new.AreaZero()) {
                    foreach (Trapezoid t in R_old.rightNeighbors) {
                        if (t.topLeft.y > r.y + ROUNDING_ERROR) makeNeighbors(T,t);
                        if (t.bottomLeft.y < r.y - ROUNDING_ERROR) makeNeighbors(B,t);
                    }
                }
                else {
                    makeNeighbors(T, R_new);
                    makeNeighbors(B, R_new);
                }

                // make nodes
                Node T_node = new leafNode(T);
                Node B_node = new leafNode(B);
                Node new_node = new lineNode (B_node, T_node, s.Line);
                if (!L_new.AreaZero()) new_node = new pointNode(L_node,   new_node, l);
                if (!R_new.AreaZero()) new_node = new pointNode(new_node, R_node,   r);
                Node old_node = L_old.node;

                // update decision tree
                replaceNode(old_node, new_node);
                disconnectFromNeighbors(trapezoids_old);
                return;
            }

            // Debug.Log("segment crossing multiple trapezoids");
            
            // Example scenario for reference
            // From:
            //  x---------|------x
            //  |         |               x-|---------x--
            //  |         x-----|-----|-----x         |
            //  |    1    |  2  |  3  |  4  |    5    |
            //  |   l~ ~ ~|~ ~ ~|~ ~ ~|~ ~ ~|~ ~ ~r   |
            //  |         |     |     |     |         |
            // -|---------|-----x     x-----|---------|-
            //                  |     |
            //           x------|-----|---------x
            // To:
            //  x---|-----|------x
            //  |   |     |               x-|-----|---x--
            //  |   |  1  x-----------------x     |   |
            //  |   |     |              4  |  5  |   |
            //  |L' l~ ~ ~|~ ~ ~|~ ~ ~|~ ~ ~|~ ~ ~r R'|
            //  |   |        2  |     |        6  |   |
            // -|---|-----------x  3  x-----------|---|-
            //                  |     |
            //           x------|-----|---------x

            // construct upper and lower list of trapezoids
            List<Trapezoid> upper_trapezoids = new List<Trapezoid>();
            List<Trapezoid> lower_trapezoids = new List<Trapezoid>();

            // loop through trapezoids (left to right)
            foreach (Trapezoid t in trapezoids_old)
            {
                if (t == R_old) break;
                // construct trapezoid by selecting right point
                if (s.Line.PointAbove(t.right)) {
                    // Trapezoid 1 and 4 in illustrated example
                    Trapezoid upper_trapezoid;
                    if (upper_trapezoids.Count == 0) {
                        upper_trapezoid = new Trapezoid(t.top, s, l, t.right); // from l to right point of t
                    }
                    else {
                        upper_trapezoid = new Trapezoid(t.top, s, upper_trapezoids[upper_trapezoids.Count-1].right, t.right); // from prev right point to right point of t
                    }
                    upper_trapezoid.in_polygon = t.top != bbox.top; // only works for single convex polygon
                    Node n = new leafNode(upper_trapezoid);
                    upper_trapezoids.Add(upper_trapezoid);
                }
                else {
                    // trapezoid 2 and 3 in illustrated example
                    Trapezoid lower_trapezoid;
                    if (lower_trapezoids.Count == 0) {
                        lower_trapezoid = new Trapezoid(s, t.bottom, l, t.right); // from l to right point of t
                    }
                    else {
                        lower_trapezoid = new Trapezoid(s, t.bottom, lower_trapezoids[lower_trapezoids.Count-1].right, t.right); // from prev right point to right point of t
                    }
                    lower_trapezoid.in_polygon = t.bottom != bbox.bottom; // only works for single convex polygon
                    Node n = new leafNode(lower_trapezoid);
                    lower_trapezoids.Add(lower_trapezoid);
                }
            }

            // construct last two trapezoids: 5 and 6 in illustrated example
            Trapezoid last_upper_trapezoid;
            if (upper_trapezoids.Count > 0) last_upper_trapezoid = new Trapezoid(R_old.top, s, upper_trapezoids[upper_trapezoids.Count-1].right, r);
            else last_upper_trapezoid = new Trapezoid(R_old.top, s, l, r);
            last_upper_trapezoid.in_polygon = R_old.top != bbox.top; // only works for single convex polygon
            
            Trapezoid last_lower_trapezoid;
            if (lower_trapezoids.Count > 0) last_lower_trapezoid = new Trapezoid(s, R_old.bottom, lower_trapezoids[lower_trapezoids.Count-1].right, r);
            else last_lower_trapezoid = new Trapezoid(s, R_old.bottom, l, r);
            last_lower_trapezoid.in_polygon = R_old.bottom != bbox.bottom; // only works for single convex polygon
            
            Node last_upper_node = new leafNode(last_upper_trapezoid);
            Node last_lower_node = new leafNode(last_lower_trapezoid);
            upper_trapezoids.Add(last_upper_trapezoid);
            lower_trapezoids.Add(last_lower_trapezoid);

            // fix neighbors
            foreach (Trapezoid upper_trapezoid in upper_trapezoids) {
                int i = upper_trapezoids.IndexOf(upper_trapezoid);
                if (i == 0) {
                    // first upper trapezoid
                    if (L_new.AreaZero()) {
                        foreach (Trapezoid ln in L_old.leftNeighbors) {
                            if (ln.topRight.y > l.y + ROUNDING_ERROR) makeNeighbors(ln,upper_trapezoid);
                        }
                    }
                    else {
                        makeNeighbors(L_new, upper_trapezoid);
                    }
                }
                else makeNeighbors(upper_trapezoids[i-1], upper_trapezoid);
                if (i == upper_trapezoids.Count - 1) {
                    // last upper trapezoid
                    if (R_new.AreaZero()) {
                        foreach (Trapezoid rn in R_old.rightNeighbors) {
                            if (rn.topLeft.y > r.y + ROUNDING_ERROR) makeNeighbors(upper_trapezoid, rn);
                        }
                    }
                    else {
                        makeNeighbors(upper_trapezoid, R_new);
                    }

                }
            }
            foreach (Trapezoid lower_trapezoid in lower_trapezoids) {
                int i = lower_trapezoids.IndexOf(lower_trapezoid);
                if (i == 0) {
                    // first lower trapezoid
                    if (L_new.AreaZero()) {
                        foreach (Trapezoid ln in L_old.leftNeighbors) {
                            if (ln.bottomRight.y < l.y - ROUNDING_ERROR) makeNeighbors(ln,lower_trapezoid);
                        }
                    }
                    else {
                        makeNeighbors(L_new, lower_trapezoid);
                    }
                }
                else makeNeighbors(lower_trapezoids[i-1], lower_trapezoid);
                if (i == lower_trapezoids.Count - 1) {
                    // last lower trapezoid
                    if (R_new.AreaZero()) {
                        foreach (Trapezoid rn in R_old.rightNeighbors) {
                            if (rn.bottomLeft.y < r.y - ROUNDING_ERROR) makeNeighbors(lower_trapezoid, rn);
                        }
                    }
                    else {
                        makeNeighbors(lower_trapezoid, R_new);
                    }
                }
            }

            // update decision tree
            // left trapezoid
            Node new_left_node = new lineNode(lower_trapezoids[0].node, upper_trapezoids[0].node, s.Line);
            if (!L_new.AreaZero()) new_left_node = new pointNode(L_node, new_left_node, l);
            replaceNode(L_old.node, new_left_node);

            // right trapezoid
            Node new_right_node = new lineNode(lower_trapezoids[lower_trapezoids.Count-1].node, upper_trapezoids[upper_trapezoids.Count-1].node, s.Line);
            if (!R_new.AreaZero()) new_right_node = new pointNode(new_right_node, R_node, r);
            replaceNode(R_old.node, new_right_node);

            // intermediate trapezoids
            foreach (Trapezoid t in trapezoids_old)
            {
                if (t == R_old || t == L_old) continue;

                // find corresponding upper trapezoid
                Trapezoid upper = null;
                foreach (Trapezoid ut in upper_trapezoids){
                    if (t.right.x <= ut.right.x && t.left.x >= ut.left.x) {
                        upper = ut;
                        break; // should occure exactly once
                    }
                }

                // find correpsoning lower trapezoid
                Trapezoid lower = null;
                foreach (Trapezoid lt in lower_trapezoids){
                    if (t.right.x <= lt.right.x && t.left.x >= lt.left.x) {
                        lower = lt;
                        break; // should occure exactly once
                    }
                }

                // should not happen:
                if (upper == null || lower == null) throw new Exception("No lower/upper trapezoid found");

                Node splitter = new lineNode(lower.node, upper.node, s.Line);
                replaceNode(t.node, splitter);
            }

            disconnectFromNeighbors(trapezoids_old);
        }
        private bool replaceNode(Node original, Node new_node)
        {
            // Debug.Log("Replacing node in decision tree...");
            if (original == root) {
                // replace whole tree
                root = new_node;
                return true;
            }
            else {
                // replace subtree
                Node parent = original.parent;

                if (parent is pointNode){
                    pointNode p = parent as pointNode;
                    if (p.leftNode == original) {p.leftNode = new_node; return true;}
                    if (p.rightNode == original) {p.rightNode = new_node; return true;}
                }
                else if (parent is lineNode){
                    lineNode p = parent as lineNode;
                    if (p.bottomNode == original) {p.bottomNode = new_node; return true;}
                    if (p.topNode == original) {p.topNode = new_node; return true;}
                }
            }

            return false; // should not happen
        }
        // left to right
        private List<Trapezoid> findInterectingTrapezoids(LineSegment l)
        {
            // Debug.Log("Finding intersecting trapezoids...");
            List<Trapezoid> trapezoids = new List<Trapezoid>();

            // find left and right most trapezoid
            Vector2 leftPoint = (l.Point1.x < l.Point2.x) ? l.Point1 : l.Point2;
            Vector2 rightPoint = (l.Point1.x > l.Point2.x) ? l.Point1 : l.Point2;

            // bias towards middle of linesegment to avoid reporting "touching" trapezoids 
            Trapezoid leftTrapezoid = root.GetTrapezoid(leftPoint, HorDir.Right);
            Trapezoid rightTrapezoid = root.GetTrapezoid(rightPoint, HorDir.Left);

            // walk from left to right trapezoid and add to list
            Trapezoid t = leftTrapezoid;
            trapezoids.Add(leftTrapezoid);
            // Debug.Log("Found " + leftTrapezoid.ToString());

            while (t != rightTrapezoid)
            {
                if (t.rightNeighbors.Count == 0) throw new Exception("Traversing trough trapezoid with no right neighbor!"); 
                else if (t.rightNeighbors.Count == 1) t = t.rightNeighbors[0];
                else if (t.rightNeighbors.Count == 2) {
                    Trapezoid n1 = t.rightNeighbors[0];
                    Trapezoid n2 = t.rightNeighbors[1];
                    if (l.Y(t.right.x) > n1.bottom.Y(t.right.x) && l.Y(t.right.x) < n1.top.Y(t.right.x)) {
                        t = n1; // l passes through neighbor 1
                    }
                    else t = n2; // l passes through neighbor 2
                }
                else if (t.rightNeighbors.Count > 2) throw new Exception("Trapezoid with more than two right neighbors!");
                trapezoids.Add(t);
                // Debug.Log("Found " + leftTrapezoid.ToString());
            }

            return trapezoids;
        }
        private void makeNeighbors(Trapezoid left, Trapezoid right) {
            if (!left.hasRightEdge || !right.hasLeftEdge) return;
            left.rightNeighbors.Add(right);
            right.leftNeighbors.Add(left);
        }
        private void disconnectFromNeighbors(List<Trapezoid> trapezoids)
        {
            foreach (Trapezoid t in trapezoids) disconnectFromNeighbors(t);
        }
        private void disconnectFromNeighbors(Trapezoid t) {
            // removes relations FROM neighbor TO trapezoid
            foreach (Trapezoid n in t.leftNeighbors) n.rightNeighbors.Remove(t);
            foreach (Trapezoid n in t.rightNeighbors) n.leftNeighbors.Remove(t);
        }

        /// <summary>
        /// Class for trapezoids inside a vertical decomposition
        /// Trapezoids are linked to a (leaf) node in the decision tree.
        /// </summary>
        public class Trapezoid
        {
            public bool in_polygon = false;
            public LineSegment top; // top bound
            public LineSegment bottom; // bottom bound
            public Vector2 left; // left bound
            public Vector2 right; // right bound
            public Vector2 topRight {get {return new Vector2(right.x, top.Line.Y(right.x)); }}
            public Vector2 topLeft {get {return new Vector2(left.x, top.Line.Y(left.x)); }}
            public Vector2 bottomRight {get {return new Vector2(right.x, bottom.Line.Y(right.x)); }}
            public Vector2 bottomLeft {get {return new Vector2(left.x, bottom.Line.Y(left.x)); }}
            public Vector2 center {get {
                float x_center = 0.5f * (right.x + left.x);
                float y_center = 0.5f * (top.Line.Y(x_center) + bottom.Line.Y(x_center));
                return new Vector2(x_center, y_center); 
                }}
            public bool hasRightEdge {get {return topRight.y > bottomRight.y + ROUNDING_ERROR; }}
            public bool hasLeftEdge {get {return topLeft.y > bottomLeft.y + ROUNDING_ERROR; }}
            public leafNode node; // bidirectional link with node in decision tree
            public List<Trapezoid> leftNeighbors;
            public List<Trapezoid> rightNeighbors;
            public bool AreaZero() { return left.x == right.x; }
            public Trapezoid(LineSegment top, LineSegment bottom, Vector2 left, Vector2 right)
            {
                if (top == null || bottom == null || left == null || right == null)
                    throw new ArgumentNullException();
                leftNeighbors = new List<Trapezoid>();
                rightNeighbors = new List<Trapezoid>();
                this.top    = top;
                this.bottom = bottom;
                this.left   = left;
                this.right  = right;
                // Debug.Log("Constructing trapezoid " + this.ToString());
            }
            public override String ToString()
            {
                return "top:" + top.ToString() + " bottom:" + bottom.ToString() + " left:" + left.ToString() + " right:" + right.ToString();
            }
        };

        public enum HorDir {Left, Right}
        public enum VerDir {Up, Down}
        
        /// <summary>
        /// Base class for all nodes in decision tree.
        /// </summary>
        abstract public class Node
        {
            public abstract Trapezoid GetTrapezoid(Vector2 p, HorDir horizontal_bias = HorDir.Right, VerDir vertical_bias = VerDir.Up);
            public abstract List<Trapezoid> GetAllTrapezoids();
            public Node parent;
        };
        
        /// <summary>
        /// Class for point nodes (left or right)
        /// </summary>
        public class pointNode : Node
        {
            public Node leftNode;
            public Node rightNode;
            public Vector2 point; 
            public pointNode(Node leftNode, Node rightNode, Vector2 point)
            {
                // Debug.Log("Constructing point node...");
                if (leftNode == null || rightNode == null || point == null)
                    throw new ArgumentNullException();
                
                leftNode.parent  = this;
                rightNode.parent = this;
                this.leftNode    = leftNode;
                this.rightNode   = rightNode;
                this.point       = point;
            }

            public override Trapezoid GetTrapezoid(Vector2 p, HorDir horizontal_bias = HorDir.Right, VerDir vertical_bias = VerDir.Up)
            {
                float x = (horizontal_bias == HorDir.Right) ? p.x + ROUNDING_ERROR : p.x - ROUNDING_ERROR;
                if (x > point.x)
                    return rightNode.GetTrapezoid(p, horizontal_bias, vertical_bias);
                else
                    return leftNode.GetTrapezoid(p, horizontal_bias, vertical_bias);
            }
            public override List<Trapezoid> GetAllTrapezoids()
            {
                List<Trapezoid> trapezoids = leftNode.GetAllTrapezoids();
                trapezoids.AddRange(rightNode.GetAllTrapezoids());
                return trapezoids;
            }
        }
        
        /// <summary>
        /// Class for line nodes (bottom or top)
        /// </summary>
        public class lineNode : Node
        {
            public Node bottomNode;
            public Node topNode;
            public Line line; 
            public lineNode(Node bottomNode, Node topNode, Line line)
            {
                // Debug.Log("Constructing line node...");
                if (bottomNode == null || topNode == null || line == null)
                    throw new ArgumentNullException();
                
                bottomNode.parent = this;
                topNode.parent    = this;
                this.bottomNode   = bottomNode;
                this.topNode      = topNode;
                this.line         = line;
            }

            public override Trapezoid GetTrapezoid(Vector2 p, HorDir horizontal_bias = HorDir.Right, VerDir vertical_bias = VerDir.Up)
            {
                float y = (vertical_bias == VerDir.Up) ? p.y + ROUNDING_ERROR : p.y - ROUNDING_ERROR;
                if (y > line.Y(p.x))
                    return topNode.GetTrapezoid(p, horizontal_bias, vertical_bias);
                else
                    return bottomNode.GetTrapezoid(p, horizontal_bias, vertical_bias);
            }
            public override List<Trapezoid> GetAllTrapezoids()
            {
                List<Trapezoid> trapezoids = topNode.GetAllTrapezoids();
                trapezoids.AddRange(bottomNode.GetAllTrapezoids());
                return trapezoids;
            }
        }

        /// <summary>
        /// Class for leaf nodes.
        /// </summary>
        public class leafNode : Node
        {
            public Trapezoid trapezoid;
            public leafNode(Trapezoid trapezoid)
            {
                // Debug.Log("Constructing leave node...");
                if (trapezoid == null)
                    throw new ArgumentNullException();
                trapezoid.node = this; // create bidirectional link
                this.trapezoid = trapezoid;
            }

            public override Trapezoid GetTrapezoid(Vector2 p, HorDir horizontal_bias = HorDir.Right, VerDir vertical_bias = VerDir.Up)
            {
                return trapezoid;
            }
            public override List<Trapezoid> GetAllTrapezoids()
            {
                List<Trapezoid> trapezoids = new List<Trapezoid>();
                trapezoids.Add(trapezoid);
                return trapezoids;
            }
        };
    }
}
