using UnityEngine;
using System.Collections;
using Nine;

namespace NetworkGame {

public class Unit {
    const float SPEED_MULTIPLIER = 5;
    const float VERTEX_MARGIN = 0.01f;

    public Vector2 position;
    public Direction direction;
    public float speed;

    public Unit(float x, float y, float speed) : this(new Vector2(x, y), speed) { }

    public Unit(Vector2 v, float speed) {
        position = v;
        this.speed = speed;
    }

    public bool AtVertex() {
        var dist = Vector2.Distance(position, NearestVertex());
        Debug.Log(dist);
        return dist < VERTEX_MARGIN;
    }

    public void Move(GraphMatrix graph, float deltaTime) {
        var edge = Edge.EdgeForPosAndDir(position, direction);
        var len = graph.GetLength(edge);
        if (len == 0) return;
        var displacement = deltaTime * speed / len * SPEED_MULTIPLIER;

        switch (direction) {
            case Direction.Up:
                position.y = Mathf.Min((int)position.y + 1, position.y + displacement);
                break;
            case Direction.Right:
                position.x = Mathf.Min((int)position.x + 1, position.x + displacement);
                break;
            case Direction.Down:
                position.y = Mathf.Max((int)position.y - 1, position.y - displacement);
                break;
            case Direction.Left:
                position.x = Mathf.Max((int)position.x - 1, position.x - displacement);
                break;
            default:
                break;
        }
    }

    Vector2 NearestVertex() {
        return new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
    }

    public override string ToString() {
        return string.Format("{0}: Pos={1} Dir={2} Speed={3}", GetType(), position, direction, speed);
    }
}

public class Player : Unit {
    public Player(float x, float y, float speed): base(x, y, speed) {}
    public Player(Vector2 c, float speed): base(c, speed) {}
}

public class Enemy : Unit {
    public float speed;

    public Enemy(float x, float y, float speed): base(x, y, speed) {}
    public Enemy(Vector2 c, float speed): base(c, speed) {}

    public void Chase(Vector2 target, GraphMatrix graph, float deltaTime) {
        if (AtVertex()) {
            direction = FaceTarget(target);
            Debug.Log("!At vertex: " + this);
        }
        Debug.Log(target + " " + position);
        Move(graph, deltaTime);
    }

    Direction FaceTarget(Vector2 target) {
        if (target.x > position.x) {
            return Direction.Right;
        }
        if (target.x < position.x) {
            return Direction.Left;
        }
        if (target.y > position.y) {
            return Direction.Up;
        }
        if (target.y < position.y) {
            return Direction.Down;
        }
        return Direction.None;
    }
}
}
