using UnityEngine;

namespace Nine {

public struct Coord : System.IEquatable<Coord> {
    public int x;
    public int y;

    public Coord(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public Coord(Vector2 v) : this((int)v.x, (int)v.y) { }

    public static Coord RandomCoord(int maxX, int maxY) {
        return new Coord(
                Random.Range(0, maxX),
                Random.Range(0, maxY));
    }

    public Vector2 ToVector() {
        return new Vector2(x, y);
    }

    public Coord MovedBy(int dx, int dy) {
        return new Coord(x+dx, x+dy);
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
    public Coord p1;
    public Coord p2;


    /***** PUBLIC: STATIC *****/
    public static Edge EdgeForPosAndDir(Vector2 position, Direction dir) {
        var x1 = (int)Mathf.Floor(position.x);
        var x2 = (int)Mathf.Ceil(position.x);
        var y1 = (int)Mathf.Floor(position.y);
        var y2 = (int)Mathf.Ceil(position.y);
        Debug.Log(string.Format("Edge Vals: {0} {1} {2} {3}", x1, y1, x2, y2));

        /* take care of cases where pos lies on a vertex */
        switch (dir) {
            case Direction.Up:
                y2 = y1 + 1;
                break;
            case Direction.Down:
                y1 = y2 - 1;
                break;
            case Direction.Left:
                x1 = x2 - 1;
                break;
            case Direction.Right:
                x2 = x1 + 1;
                break;
        }
        return new Edge(x1, y1, x2, y2);
    }


    /***** INITIALIZER *****/
    public Edge(Coord p1, Coord p2) {
        this.p1 = p1;
        this.p2 = p2;
    }

    public Edge(int x1, int y1, int x2, int y2) : this(new Coord(x1, y1), new Coord(x2, y2)) {
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
