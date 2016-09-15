using UnityEngine;

namespace Nine {

public struct Coord : System.IEquatable<Coord> {
    public int x;
    public int y;

    public Coord(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static Coord RandomCoord(int maxX, int maxY) {
        return new Coord(
                Random.Range(0, maxX),
                Random.Range(0, maxY));
    }

    public Vector2 ToVector() {
        return new Vector2(x, y);
    }

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
    public int x1;
    public int y1;
    public int x2;
    public int y2;

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

    public Edge(int x1, int y1, int x2, int y2) {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
    }

    public Edge Reverse() {
        return new Edge(x2, y2, x1, y1);
    }

    public Orientation orientation {
        get {
            // TODO: this is just a hack
            if (x1 != x2) {
                return Orientation.Vertical;
            } else {
                return Orientation.Horizontal;
            }
        }
    }

    public override string ToString() {
        return string.Format("Edge: ({0}, {1}) - ({2}, {3})", x1, y1, x2, y2);
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
