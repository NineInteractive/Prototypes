using UnityEngine;
using System.Collections.Generic;

namespace Nine {

public struct Coord : System.IEquatable<Coord> {
    /***** PUBLIC: VARIABLES *****/
    public int x;
    public int y;


    /***** CONSTRUCTORS *****/
    public Coord(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public Coord(float x, float y) {
        this.x = (int)x;
        this.y = (int)y;
    }

    public Coord(Vector2 v) : this(v.x, v.y) { }


    /***** STATIC: METHODS *****/
    public static Coord RandomCoord(int maxX, int maxY, ICollection<Coord> exclude, bool addToCollection=false) {
        var c = RandomCoord(maxX, maxY);
        while (exclude.Contains(c)) {
            c = RandomCoord(maxX, maxY);
        }
        if (addToCollection) exclude.Add(c);
        return c;
    }

    public static Coord RandomCoord(int maxX, int maxY) {
        return new Coord(
                Random.Range(0, maxX),
                Random.Range(0, maxY));
    }


    /***** PUBLIC: METHODS *****/
    public Vector2 ToVector() {
        return new Vector2(x, y);
    }

    public Coord MovedBy(int dx, int dy) {
        return new Coord(x+dx, y+dy);
    }

    public Coord AdjacentCoord(Direction dir) {
        Coord c = this;
        switch (dir) {
            case Direction.Up:
                c = MovedBy(0, 1);
                break;
            case Direction.Down:
                c = MovedBy(0, -1);
                break;
            case Direction.Left:
                c = MovedBy(-1, 0);
                break;
            case Direction.Right:
                c = MovedBy(1, 0);
                break;
            default:
                break;
        }

        return c;
    }

    public Edge AdjacentEdge(Direction dir) {
        return new Edge(this, AdjacentCoord(dir));
    }

    public Edge[] AdjacentEdges() {
        var edges = new Edge[4];
        edges[0] = AdjacentEdge(Direction.Up);
        edges[1] = AdjacentEdge(Direction.Right);
        edges[2] = AdjacentEdge(Direction.Down);
        edges[3] = AdjacentEdge(Direction.Left);
        return edges;
    }

    /***** PUBLIC: PROPERTIES *****/
    public bool isNoneNegative {
        get {
            return x >= 0 && y >= 0;
        }
    }

    /***** PUBLIC: IEquatable *****/
    public override int GetHashCode() {
        return 1000000*x + y;
    }

    public bool Equals(Coord other) {
        return x == other.x && y == other.y;
    }

	public static bool operator ==(Coord c1, Coord c2) {
		return c1.Equals(c2);
	}

	public static bool operator !=(Coord c1, Coord c2) {
	   return !c1.Equals(c2);
	}

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }
}

public struct Edge {
    public readonly Coord p1;
    public readonly Coord p2;

    /***** INITIALIZER *****/
    public Edge(Coord p1, Coord p2) {
        this.p1 = p1;
        this.p2 = p2;
    }

    public Edge(int x1, int y1, int x2, int y2) : this(new Coord(x1, y1), new Coord(x2, y2)) { }


    /***** PUBLIC: STATIC METHODS *****/
    public static List<Edge> EdgesBetweenCoords(Coord c1, Coord c2) {
        var edges = new List<Edge>();
        int width = Mathf.Abs(c2.x - c1.x);
        int height = Mathf.Abs(c2.y - c1.y);
        int left = Mathf.Min(c1.x, c2.x);
        int bottom = Mathf.Min(c1.y, c2.y);
        int right = left+width;
        int top = bottom+height;

        for (int x=left; x<right; x++) {
            for (int y=bottom; y<top; y++) {
                // north
                Edge north = new Edge(x, y, x, y+1);
                Edge east = new Edge(x, y, x+1, y);
                edges.Add(north);
                edges.Add(east);
            }
        }

        for (int x=left; x<right; x++) {
            Edge farNorth = new Edge(x, top, x+1, top);
            edges.Add(farNorth);
        }

        for (int y=bottom; y<top; y++) {
            Edge farEast = new Edge(right, y, right, y+1);
            edges.Add(farEast);
        }

        return edges;
    }

    /***** PUBLIC: METHODS *****/
    public Edge Reverse() {
        return new Edge(p2, p1);
    }

    public Orientation orientation {
        get {
            // TODO: this is just a hack
            if (p1.x != p2.x) {
                return Orientation.Vertical;
            } else {
                return Orientation.Horizontal;
            }
        }
    }

    public Vector2 Midpoint() {
        return new Vector2(p1.x+p2.x, p1.y+p2.y) / 2f;
    }

    public override string ToString() {
        return string.Format("Edge: {0}/{1}", p1, p2);
    }

    public bool isVertex {
        get {
            return p1 == p2;
        }
    }

    public bool isNoneNegative {
        get {
            return p1.isNoneNegative && p2.isNoneNegative;
        }
    }

    public override int GetHashCode() {
        unchecked {
            int hash = 17 * p1.GetHashCode() + 23 * p2.GetHashCode();
            return hash;
        }
    }
}

public class Grid {

    public int width;
    public int height;

    Coord[,] coords;
    Edge[,] edges; // width-1, height-1

    public Grid(int width, int height) {
        this.width = width;
        this.height = height;

        coords = new Coord[width, height];

        for (int x=0; x<width; x++) {
            for (int y=0; y<height; y++) {
                coords[x, y] = new Coord(x, y);
            }
        }
        // make horizontal edges
        // make vertical edges
        // look them up by the set of coords

    }
}

}
