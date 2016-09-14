using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;

namespace NetworkGame {

public class NetworkGame : MonoBehaviour {

    const int WIDTH = 10;
    const int HEIGHT = 10;
    const int MIN_LENGTH = 1;
    const int MAX_LENGTH = 8;

    public Text renderer;

    int num_enemies = 1;

    int turns;

    /*Grid grid;*/
    GraphMatrix graph;
    GraphRenderer graphRenderer;

    Player player;
    List<Enemy> enemies;
    Coord gemPosition;

	// Use this for initialization
	void Start () {
        StartCoroutine(Play());
	}

	// Update is called once per frame
	void Update () {
	}

    IEnumerator Play() {
        // setup
        // turn
        // check death
        // print
        while (true) {
            Setup();
            do {
                //Debug.Log(">    Turn " + turns);
                RenderBoard();
                yield return StartCoroutine(PlayTurn());
                turns++;
                yield return null;
            } while (!(PlayerIsDead() || WonLevel()));
            RenderBoard();
            ShowResult();
            if (WonLevel()) {
                IncreaseDifficulty();
            } else {
                ResetDifficulty();
            }
        }
    }

    /***** SETUP *****/

    void Setup() {
        // possible to be run multiple times
        gemPosition = Coord.RandomCoord(WIDTH, HEIGHT);

        player = new Player(0, 0); // init position?
        enemies = new List<Enemy>();
        for (int i=0; i<num_enemies; i++) {
            enemies.Add(new Enemy(Random.Range(1, WIDTH-1), Random.Range(1, HEIGHT-1),
                                   /*Random.Range(1.05f, 1.48f)));*/
                                   1));
        }

        // setup cost
        graph = new GraphMatrix();
        for (int x=0; x<WIDTH; x++) {
            for (int y=0; y<HEIGHT; y++) {
                // north
                graph.SetLength(new Edge(x, y, x+1, y), Random.Range(MIN_LENGTH, MAX_LENGTH));
                // east
                graph.SetLength(new Edge(x, y, x, y+1), Random.Range(MIN_LENGTH, MAX_LENGTH));
            }
        }

        // render
        graphRenderer = new GraphRenderer();
        graphRenderer.RenderGraph(graph);
    }

    void IncreaseDifficulty() {
        num_enemies++;
    }

    void ResetDifficulty() {
        num_enemies = 1;
    }

    /***** PLAY LOGIC *****/

    IEnumerator PlayTurn() {
        while (DirectionUtil.FromInput() == Direction.None) {
            yield return null;
        }
        var dir = DirectionUtil.FromInput();
        player.Move(dir);
        foreach (var e in enemies) {
            e.Chase(player.position);
        }
        RemoveOverlappedEnemies();
    }

    void RemoveOverlappedEnemies() {
        var enemyPos = new Dictionary<Coord, List<Enemy>>();
        foreach (var enemy in enemies) {
            if (!enemyPos.ContainsKey(enemy.position)) {
                enemyPos[enemy.position] = new List<Enemy>();
            }
            enemyPos[enemy.position].Add(enemy);
        }

        foreach (var unitsOnSameCoord in enemyPos.Values) {
            if (unitsOnSameCoord.Count > 1) {
                foreach (var e in unitsOnSameCoord) {
                    enemies.Remove(e);
                }
            }
        }
    }

    /***** GAME STATUS *****/

    bool PlayerIsDead() {
        foreach (Enemy e in enemies) {
            if (e != null && e.position == player.position) {
                return true;
            }
        }
        return false;
    }

    bool WonLevel() {
        if (!PlayerIsDead() && (player.position == gemPosition)) {
            return true;
        }
        return false;
    }

    /***** RENDERING *****/

    void RenderBoard() {
        string output = "";
        for (int y = HEIGHT-1; y>=0; y--) {
            for (int x = 0; x<WIDTH; x++) {
                var here = new Coord(x, y);
                var c = CharForPosition(here);
                output += c;
            }
            output += '\n';
        }
        renderer.text = output;
    }

    char CharForPosition(Coord pos) {
        if (player.position == pos) {
            if (PlayerIsDead()) {
                return 'X';
            } else {
                return '@';
            }
        }

        foreach (var e in enemies) {
            if (e != null && e.position == pos) {
                return 'O';
            }
        }

        if (pos == gemPosition) {
            return '♢';
        }
        return '+';
    }

    void ShowResult() {
        Debug.Log("Survived for " + turns + " turns");
    }

}

public class Network {
    public void Blah () {
        // coord networked
        // draw network
        // take turns
    }
}

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

public class Unit {
    public Coord position;

    public Unit(int x, int y) {
        position = new Coord(x, y);
    }

    public Unit(Coord c) {
        position = c;
    }

    public void Move(Direction dir) {
        switch (dir) {
            case Direction.Up:
                position.y += 1;
                break;
            case Direction.Right:
                position.x += 1;
                break;
            case Direction.Down:
                position.y -= 1;
                break;
            case Direction.Left:
                position.x -= 1;
                break;
            default:
                break;
        }
    }
}

public class Player : Unit {
    public Player(int x, int y): base(x, y) {}
    public Player(Coord c): base(c) {}
}

public class Enemy : Unit {
    public float speed;

    float curMovement;

    public Enemy(int x, int y, float speed): base(x, y) {
        this.speed = speed;
        Debug.Log("Speed = " + speed);
    }

    public Enemy(Coord c, float speed): base(c) {
        this.speed = speed;
        Debug.Log("Speed = " + speed);
    }

    public void Chase(Coord target) {
        curMovement += speed;

        if (curMovement >= 1) {
            curMovement -= 1;
            if (target.x > position.x) {
                position.x++;
                return;
            }
            if (target.x < position.x) {
                position.x--;
                return;
            }
            if (target.y > position.y) {
                position.y++;
                return;
            }
            if (target.y < position.y) {
                position.y--;
                return;
            }
        }
    }
}

/*
public class Edge {
    public float length;
    public Coord vertex1;
    public Coord vertex2;

    public HashSet<Coord> vertices;
}
*/

// Every node has a node id
//

public struct Edge {
    public int x1;
    public int y1;
    public int x2;
    public int y2;

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
}

public class GraphMatrix {
    public const float NO_CONNECTION = -1;

    public readonly Dictionary<Edge, float> lengthBetweenVertices = new Dictionary<Edge, float>();

    public float GetLength(Edge edge) {
        return lengthBetweenVertices[edge];
    }

    public void SetLength(Edge edge, float length) {
        lengthBetweenVertices[edge] = length;
        lengthBetweenVertices[edge.Reverse()] = length;
    }

    public int Count {
        get {
            return lengthBetweenVertices.Count;
        }
    }
}

public class GraphRenderer {

    const float LINE_WIDTH_SCALE = 0.1f;
    const float LINE_LENGTH_SCALE = 5f;

    public Dictionary<Edge, RectRenderer> edgeRendererDict;

    public void RenderGraph(GraphMatrix mat) {
        edgeRendererDict = new Dictionary<Edge, RectRenderer>();
        foreach (var pair in mat.lengthBetweenVertices) {
            // draw edge
            var len = pair.Value;
            var edge = pair.Key;

            // approx
            var boardCenter = new Vector2(5, 5);

            Vector2 center = (new Vector2((edge.x1+edge.x2)/2f, (edge.y1+edge.y2)/2f)-boardCenter)*LINE_LENGTH_SCALE;
            float length = LINE_LENGTH_SCALE; // only connected to adjacent vertices
            float width = len * LINE_WIDTH_SCALE;
            float angle = edge.orientation == Orientation.Vertical ? 0 : 90;

            ShapeGOFactory.InstantiateShape(new RectProperty(
                        center: center, height: length, width: width, angle: angle, color: Color.white
            ));
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

    /*
    public Edge EdgeBetween(Coord c1, Coord c2) {
        return new Edge;
    }
    */
}
}
