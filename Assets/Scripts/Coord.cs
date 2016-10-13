using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

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

    public Coord Plus(Coord c) {
        return MovedBy(c.x, c.y);
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

    public List<Coord> AdjacentCoords(
            int maxX=int.MaxValue, // exclusive
            int maxY=int.MaxValue, // exclusive
            bool includeDiagonal=false,
            bool includeSelf=false) {

        var coords = new List<Coord>();
        if (includeDiagonal) {
            for (int i = -1; i<=1; i++) {
                for (int j = -1; j<=1; j++) {
                    coords.Add(MovedBy(i, j));
                }
            }
        } else {
            coords.Add(AdjacentCoord(Direction.Up));
            coords.Add(AdjacentCoord(Direction.Down));
            coords.Add(AdjacentCoord(Direction.Left));
            coords.Add(AdjacentCoord(Direction.Right));
            coords.Add(this);
        }

        if (!includeSelf) {
            coords.Remove(this);
        }

        return coords.FindAll(delegate(Coord c) {
            return c.Bounded(maxX, maxY);
        });
    }

    public IEnumerable<Coord> NearbyCoords(
            int xDist,
            int yDist,
            int maxX=int.MaxValue, // exclusive
            int maxY=int.MaxValue, // exclusive
            bool includeSelf=false) {
        for (int i=x-xDist; i<=x+xDist; i++) {
            for (int j=y-yDist; j<=y+yDist; j++) {
                var c = new Coord(i, j);
                if (c.Bounded(maxX, maxY) && (includeSelf || c != this)) {
                    yield return c;
                }
            }
        }
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

    public bool Bounded(int width, int height) {
        return x >= 0 && y >= 0 && x < width && y < height;
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


// coordinate => landmark
// landmark => coordinate? is this needed? otherwise we just - but how do we detect overlaps?
// just by looking through the tiles. not sure if we really need the reverse mapping
// now so let it go.
public struct Selection {
    public Coord bottomLeft;
    public Coord topRight;

    public Selection(Coord bl, Coord tr) {
        bottomLeft = bl;
        topRight = tr;
    }

    public static Selection RandomSelection(int width, int height, int maxX, int maxY) {
        var bottomLeft = Coord.RandomCoord(maxX-width, maxY-height);
        var topRight = bottomLeft.MovedBy(width, height);
        return new Selection(bottomLeft, topRight);
    }

    // linq?
    public IEnumerable<Coord> Coords() {
        for (int i=bottomLeft.x; i<=topRight.x; i++) {
            for (int j=bottomLeft.y; j<=topRight.y; j++) {
                yield return new Coord(i, j);
            }
        }
    }

    public Coord RandomCoord() {
        return Coord.RandomCoord(width, height).Plus(bottomLeft);
    }

    public int width {
        get {
            return topRight.x - bottomLeft.x;
        }
    }

    public int height {
        get {
            return topRight.y - bottomLeft.y;
        }
    }

    public override string ToString() {
        return string.Format("Selection: {0}, {1}", bottomLeft, topRight);
    }

}
}
