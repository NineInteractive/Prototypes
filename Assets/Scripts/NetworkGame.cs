using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Nine;

namespace NetworkGame {

public class NetworkGame : MonoBehaviour {

    const int WIDTH = 8;
    const int HEIGHT = 8;
    const int MIN_LENGTH = 1;
    const int MAX_LENGTH = 8;
    const float CAPTURE_DISTANCE = 0.05f;

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
	void Awake () {
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
                //RenderBoard();
                yield return StartCoroutine(PlayTurn());
                turns++;
                yield return null;
            } while (!(PlayerIsDead() || WonLevel()));
            //RenderBoard();
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
        /** Reset: if there are any renderers in the scene, destroy them **/
        foreach (var ur in GameObject.FindObjectsOfType<UnitRenderer>()) {
            Destroy(ur.gameObject);
        }
        foreach (var ur in GameObject.FindObjectsOfType<RectRenderer>()) {
            Destroy(ur.gameObject);
        }

        var occupied = new HashSet<Coord>();

        /** Create Gem: possible to be run multiple times **/
        gemPosition = Coord.RandomCoord(WIDTH, HEIGHT, occupied, true);

        ShapeGOFactory.InstantiateRect(
                new RectProperty(
                    center:gemPosition.ToVector(),
                    width: 0.2f,
                    height: 0.2f,
                    color: Color.green,
                    layer: -2
                ));

        /** Create Units **/
        player = new Player(Coord.RandomCoord(WIDTH, HEIGHT, occupied, true), 1); // init position?
        enemies = new List<Enemy>();
        for (int i=0; i<num_enemies; i++) {
            var ene = new Enemy(Coord.RandomCoord(WIDTH, HEIGHT, occupied, true), Random.Range(1.05f, 1.48f));
            enemies.Add(ene);
        }

        /** Create Renderers **/
        new GameObject().AddComponent<UnitRenderer>().unit = player;
        foreach (var e in enemies) {
            new GameObject().AddComponent<UnitRenderer>().unit = e;
        }

        /** Setup Graph **/
        graph = new GraphMatrix();
        for (int x=0; x<WIDTH; x++) {
            for (int y=0; y<HEIGHT; y++) {
                // north
                graph.SetLength(new Edge(x, y, x+1, y), Random.Range(MIN_LENGTH, MAX_LENGTH));
                // east
                graph.SetLength(new Edge(x, y, x, y+1), Random.Range(MIN_LENGTH, MAX_LENGTH));
            }
        }

        for (int x=0; x<WIDTH; x++) {
            graph.SetLength(new Edge(x, HEIGHT, x+1, HEIGHT), Random.Range(MIN_LENGTH, MAX_LENGTH));
        }

        for (int y=0; y<HEIGHT; y++) {
            graph.SetLength(new Edge(WIDTH, y, WIDTH, y+1), Random.Range(MIN_LENGTH, MAX_LENGTH));
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
        turns = 0;
    }


    /***** PLAY LOGIC *****/
    IEnumerator PlayTurn() {
        /** Move Enemies **/
        foreach (var e in enemies) {
            e.Chase(player.position, graph, Time.deltaTime);
        }

        /** Move Player **/
        if (player.RestingAtVertex()) {
			while (DirectionUtil.FromInput() == Direction.None) {
				yield return null;
			}
            player.MoveToward(DirectionUtil.FromInput());
        }
        player.Move(graph, Time.deltaTime);
    }

    /*
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
    */

    /***** GAME STATUS *****/

    bool PlayerIsDead() {
        foreach (Enemy e in enemies) {
            if (e != null && Approx(e.position, player.position)) {
                return true;
            }
        }
        return false;
    }

    bool WonLevel() {
        if (!PlayerIsDead() && Approx(player.position, gemPosition.ToVector())) {
            return true;
        }
        return false;
    }

    bool Approx(Vector2 p1, Vector2 p2) {
        return Vector2.Distance(p1, p2) < CAPTURE_DISTANCE;
    }

    /***** RENDERING *****/

    /*
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
    */

    void ShowResult() {
        Debug.Log("Survived for " + turns + " turns");
    }

}


public class GraphMatrix {
    public const float NO_CONNECTION = -1;

    public readonly Dictionary<Edge, float> lengthBetweenVertices = new Dictionary<Edge, float>();

    public float GetLength(Edge edge) {
        float len;
        if (!lengthBetweenVertices.TryGetValue(edge, out len)) {
            Debug.LogError("Edge not found: " + edge);
        }
        return len;
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
    const float LINE_LENGTH_SCALE = 1f;

    public Dictionary<Edge, RectRenderer> edgeRendererDict;

    public void RenderGraph(GraphMatrix mat) {
        edgeRendererDict = new Dictionary<Edge, RectRenderer>();
        foreach (var pair in mat.lengthBetweenVertices) {
            // draw edge
            var len = pair.Value;
            var edge = pair.Key;

            Vector2 center = edge.Midpoint()*LINE_LENGTH_SCALE;
            float length = LINE_LENGTH_SCALE; // only connected to adjacent vertices
            float width = len * LINE_WIDTH_SCALE;
            float angle = edge.orientation == Orientation.Vertical ? 90 : 0;

            ShapeGOFactory.InstantiateShape(new RectProperty(
                        center: center, height: length, width: width, angle: angle, color: Color.white
            ));
        }
    }
}

}
