﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Nine;

namespace NetworkGame {

public class NetworkGame : MonoBehaviour {

    const int WIDTH = 8;
    const int HEIGHT = 8;
    const float CAPTURE_DISTANCE = 0.05f;

    const float PLAYER_SPEED = 1f;
    const float ENEMY_MIN_SPEED = 0.5f;
    const float ENEMY_MAX_SPEED = 0.5f;

    const int START_ENEMY_COUNT = 10;
    const int MORE_ENEMIES_PER_STAGE = 2;

    const float SECONDS_BETWEEN_TEXT = 3;

    static int[] LENGTHS = {1, 1, 1, 4};

    public Text textbox;

    int num_enemies = START_ENEMY_COUNT;

    int turns;
    int stage = 1;

    /*Grid grid;*/
    GraphMatrix graph;
    GraphRenderer graphRenderer;

    Player player;
    List<Enemy> enemies;

    /*** TODO: GEM ***/
    Coord gemPosition;
    bool gemPickedUp = false;

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
                /*PlayTurn2();*/
                yield return null;
            } while (!(PlayerIsDead() || WonLevel()));
            //RenderBoard();
            ShowResult();
            if (WonLevel()) {
                yield return StartCoroutine(ScheherazadeSpeaks());
                IncreaseDifficulty();
            } else {
                ResetDifficulty();
            }
        }
    }

    /***** SETUP *****/

    int RandomLength() {
        return LENGTHS[Random.Range(0, LENGTHS.Length)];
    }

    void Setup() {
        /** Reset: if there are any renderers in the scene, destroy them **/
        foreach (var ur in GameObject.FindObjectsOfType<UnitRenderer>()) {
            Destroy(ur.gameObject);
        }
        foreach (var ur in GameObject.FindObjectsOfType<RectRenderer>()) {
            Destroy(ur.gameObject);
        }

        var safeZone = new HashSet<Coord>();
        var occupied = new HashSet<Coord>();

        /** Setup Graph **/
        graph = new GraphMatrix();
        foreach (var e in Edge.EdgesBetweenCoords(new Coord(0, 0), new Coord(WIDTH, HEIGHT))) {
            graph.AddPath(new Path(e, RandomLength()));
        }

        /** Create Safe Zone (Town) **/
        var centerOfTown = Coord.RandomCoord(WIDTH-1, HEIGHT-1).MovedBy(1, 1);
        var bottomLeft = centerOfTown.MovedBy(-1, -1);
        var topRight = centerOfTown.MovedBy(1, 1);

        foreach (var c in Edge.EdgesBetweenCoords(bottomLeft, topRight)) {
            graph.GetPath(c).allowedUnitType = UnitType.Player;
            safeZone.Add(c.p1);
            safeZone.Add(c.p2);

            occupied.Add(c.p1);
            occupied.Add(c.p2);
        }

        /** Gem class **/


        /** Create Gem: possible to be run multiple times **/
        gemPosition = Coord.RandomCoord(WIDTH+1, HEIGHT+1, occupied, true);

        ShapeGOFactory.InstantiateRect(
                new RectProperty(
                    center:gemPosition.ToVector(),
                    width: 0.2f,
                    height: 0.2f,
                    color: Color.red,
                    angle: 45,
                    layer: -2
                ));

        /** Create Units **/
        player = new Player(safeZone.GetRandomElement<Coord>(), PLAYER_SPEED);
        enemies = new List<Enemy>();
        for (int i=0; i<num_enemies; i++) {
            var ene = new Enemy(Coord.RandomCoord(WIDTH+1, HEIGHT+1, occupied, true),
                    Random.Range(ENEMY_MIN_SPEED, ENEMY_MAX_SPEED));
            enemies.Add(ene);
        }

        /** Create Unit Renderers **/
        new GameObject().AddComponent<UnitRenderer>().unit = player;
        foreach (var e in enemies) {
            new GameObject().AddComponent<UnitRenderer>().unit = e;
        }


        /** Render Graph **/
        graphRenderer = new GraphRenderer();
        graphRenderer.RenderGraph(graph);
    }

    void IncreaseDifficulty() {
        num_enemies += MORE_ENEMIES_PER_STAGE;
        gemPickedUp = false;
        stage++;
    }

    void ResetDifficulty() {
        num_enemies = START_ENEMY_COUNT;
        turns = 0;
        gemPickedUp = false;
        stage = 1;
    }


    /***** PLAY LOGIC *****/
    IEnumerator PlayTurn() {
        /** Move Enemies **/
        foreach (var e in enemies) {
            e.Chase(player, graph, Time.deltaTime, GemPickedUp());
        }

        /** Move Player **/
        if (player.RestingAtVertex()) {
			while (DirectionUtil.FromInput() == Direction.None) {
				yield return null;
			}
            turns++;
            player.MoveToward(DirectionUtil.FromInput());
        }
        player.Move(graph, Time.deltaTime);
    }

    void PlayTurn2() {
        /** Move Enemies **/
        foreach (var e in enemies) {
            e.Chase(player, graph, Time.deltaTime, GemPickedUp());
        }

        /** Move Player **/
        if (player.RestingAtVertex()) {
            turns++;
            player.MoveToward(DirectionUtil.FromInput());
        }
        player.Move(graph, Time.deltaTime);
    }


    /***** SCHEHERAZADE *****/
    IEnumerator ScheherazadeSpeaks() {
        textbox.text = "";
        yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
        foreach (var line in DialogueSystem.DialogueForStage(stage)) {
            textbox.text = line;
            yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
        }
        textbox.text = "";
    }

    /***** PROPERTIES - GAME STATUS *****/
    bool PlayerIsDead() {
        foreach (Enemy e in enemies) {
            if (e != null && Approx(e.position, player.position)) {
                return true;
            }
        }

        /*HAX*/
        if (num_enemies+3 >= (WIDTH+1) * (HEIGHT+1)) {
            return true;
        }

        return false;
    }

    bool WonLevel() {
        if (!PlayerIsDead() && GemPickedUp() && PlayerInSafeZone()) {
            return true;
        }
        return false;
    }

    bool GemPickedUp() {
        if (!gemPickedUp && Approx(player.position, gemPosition.ToVector())) {
            gemPickedUp = true;
        }
        return gemPickedUp;
    }

    bool PlayerInSafeZone() {
        if (player.edge.isVertex) {
            foreach (var path in graph.GetAdjacentPaths(player.edge.p1)) {
                if (path.allowedUnitType == UnitType.Player) {
                    return true;
                }
            }
        } else {
            var path = graph.GetPath(player.edge);
            if (path != null && path.allowedUnitType == UnitType.Player) {
                return true;
            }
        }
        return false;
    }

    bool Approx(Vector2 p1, Vector2 p2) {
        return Vector2.Distance(p1, p2) < CAPTURE_DISTANCE;
    }


    /***** RENDERING *****/
    void ShowResult() {
        Debug.Log("Survived for " + turns + " turns");
    }

}


public class GraphMatrix {
    public const float NO_CONNECTION = -1;

    public readonly Dictionary<Edge, Path> edgeToPath = new Dictionary<Edge, Path>();

    public void AddPath(Path path) {
        edgeToPath[path.edge] = path;
        edgeToPath[path.edge.Reverse()] = path;
    }

    public float GetLength(Edge edge) {
        Path p;
        if (!edgeToPath.TryGetValue(edge, out p)) {
            Debug.LogError("Edge not found: " + edge);
            return 0;
        }
        return p.length;
    }

    public Path GetPath(Edge edge) {
        Path p;

        if (!edgeToPath.TryGetValue(edge, out p)) {
            //Debug.Log("Edge not found: " + edge);
        }

        return p;
    }

    public Path GetPath(int x1, int y1, int x2, int y2) {
        return GetPath(new Edge(x1, y1, x2, y2));
    }

    public Path[] GetAdjacentPaths(Coord coord) {
        var paths = new List<Path>();
        foreach (var edge in coord.AdjacentEdges()) {
            var p = GetPath(edge);
            if (p != null) paths.Add(p);
        }

        return paths.ToArray();
    }

    public int Count {
        get {
            return edgeToPath.Count;
        }
    }
}

public class GraphRenderer {

    const float LINE_WIDTH_SCALE = 0.15f;
    const float LINE_LENGTH_SCALE = 1f;

    public Dictionary<Edge, RectRenderer> edgeRendererDict;

    public void RenderGraph(GraphMatrix mat) {
        edgeRendererDict = new Dictionary<Edge, RectRenderer>();
        foreach (var pair in mat.edgeToPath) {
            // draw edge
            var edge = pair.Key;
            var path = pair.Value;

            Vector2 center = edge.Midpoint()*LINE_LENGTH_SCALE;
            float length = LINE_LENGTH_SCALE; // only connected to adjacent vertices
            float width = LINE_WIDTH_SCALE / path.length;
            float angle = edge.orientation == Orientation.Vertical ? 90 : 0;

            var color = Color.white;
            if (path.allowedUnitType == UnitType.Player) {
                color = Color.yellow;
            }

            ShapeGOFactory.InstantiateShape(new RectProperty(
                        center: center, height: length, width: width, angle: angle, color: color
            ));
        }
    }
}

}
