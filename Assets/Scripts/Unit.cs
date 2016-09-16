using UnityEngine;
using System.Collections;
using Nine;

namespace NetworkGame {

public class Unit {
    const float SPEED_MULTIPLIER = 9;
    const float VERTEX_MARGIN = 0.01f;

    public Vector2 position;
    public Direction direction;
    public float speed;

    protected Coord origin;
    protected Coord destination;

    public Unit(float x, float y, float speed) : this(new Coord(x, y), speed) { }

    public Unit(Vector2 v, float speed) : this(new Coord(v), speed) { }

    public Unit(Coord c, float speed) {
        origin = c;
        destination = origin;
        position = origin.ToVector();
        this.speed = speed;
    }

    public bool RestingAtVertex() {
        return origin == destination;
    }

    /*
     * Assumes direction has been set
     */
    public void Move(GraphMatrix graph, float deltaTime) {
        if (origin == destination) return;

        /** Calculate Displacement for Current Time Unit **/
        var edge = new Edge(origin, destination);
        var len = graph.GetLength(edge);
        var displacement = deltaTime * speed / len * SPEED_MULTIPLIER;

        /** Check Destination Arrival: If True, set origin = destination and pass **/
        if (Vector2.Distance(destination.ToVector(), position) < displacement) {
            origin = destination;
            position = destination.ToVector();
            return;
        }

        /** Ready to Move **/
        position += (destination.ToVector()-origin.ToVector()).normalized * displacement;
    }

    /***** PROTECTED: METHOD ***/

    protected static Coord FindNextDestination(Coord curr, Vector2 target) {
        Direction dir = Direction.None;
        var dy = target.y - curr.y;
        var dx = target.x - curr.x;

        if (Mathf.Abs(dy) > Mathf.Abs(dx)) {
            if (dy < 0) {
                dir = Direction.Down;
            } else {
                dir = Direction.Up;
            }
        } else {
            if (dx < 0) {
                dir = Direction.Left;
            } else {
                dir = Direction.Right;
            }
        }

        return FindNextDestination(curr, dir);
    }

    protected static Coord FindNextDestination(Coord curr, Direction dir) {
        Coord next;

        switch(dir) {
            case Direction.Up:
                next = curr.MovedBy(0, 1);
                break;
            case Direction.Down:
                next = curr.MovedBy(0, -1);
                break;
            case Direction.Left:
                next = curr.MovedBy(-1, 0);
                break;
            case Direction.Right:
                next = curr.MovedBy(1, 0);
                break;
            default:
                next = curr;
                break;
        }
        return next;
    }

    public override string ToString() {
        return string.Format("{0}: Pos={1} Origin={2} Dest={3} Speed={4}",
                GetType(), position, origin, destination, speed);
    }
}

public class Player : Unit {
    public Player(float x, float y, float speed): base(x, y, speed) {}
    public Player(Vector2 c, float speed): base(c, speed) {}
    public Player(Coord c, float speed) : base (c, speed) {}

    public void MoveToward(Direction dir) {
        destination = FindNextDestination(origin, dir);
    }
}

public class Enemy : Unit {
    public float speed;

    public Enemy(float x, float y, float speed): base(x, y, speed) {}
    public Enemy(Vector2 c, float speed): base(c, speed) {}
    public Enemy(Coord c, float speed) : base (c, speed) {}

    public void Chase(Vector2 target, GraphMatrix graph, float deltaTime) {
        if (RestingAtVertex()) {
            destination = FindNextDestination(origin, target);
        }
        Move(graph, deltaTime);
    }
}
}
