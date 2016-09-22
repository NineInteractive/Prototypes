using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class Town {
    /***** PUBLIC: VARIABLES *****/
    /** Landmarks **/
    public List<Edge> residence = Edge.EdgesBetweenCoords(new Coord(1, 1), new Coord(3, 3));
    public List<Edge> library = Edge.EdgesBetweenCoords(new Coord(5, 5), new Coord(7, 6));
    public List<Edge> hill = Edge.EdgesBetweenCoords(new Coord(4, 7), new Coord(5, 8));
    public List<Edge> cave = Edge.EdgesBetweenCoords(new Coord(1, 6), new Coord(2, 7));


    /** Other game states **/
    public int gemsCollected = 0;
    public int datesLeft = 10;

    /***** PRIVATE: VARIABLES *****/
    HashSet<Coord> _safeZone;


    /***** PUBLIC: STATIC METHODS *****/
    public static HashSet<Coord> CoordsForLandmark(List<Edge> landmark) {
        var set = new HashSet<Coord>();
        foreach (var edge in landmark) {
            set.Add(edge.p1);
            set.Add(edge.p2);
        }
        return set;
    }


    /***** PUBLIC: METHODS *****/
    public HashSet<Coord> safeZone {
        get {
            if (_safeZone == null) {
                _safeZone = new HashSet<Coord>();
                foreach (var edge in residence.Concat(library).Concat(hill)) {
                    _safeZone.Add(edge.p1);
                    _safeZone.Add(edge.p2);
                }
            }
            return _safeZone;
        }
    }

    public HashSet<Coord> CreateBaseOccupied() {
        var occupied = new HashSet<Coord>();
        foreach (var edge in residence.Concat(library).Concat(hill).Concat(cave)) {
            occupied.Add(edge.p1);
            occupied.Add(edge.p2);
        }
        return occupied;
    }

    public void ApplyToGraph(GraphMatrix graph) {
        _ApplyToGraph(graph, residence, LandmarkType.Residence, UnitType.Player);
        _ApplyToGraph(graph, library, LandmarkType.Library, UnitType.Player);
        _ApplyToGraph(graph, hill, LandmarkType.Hill, UnitType.Player);
        _ApplyToGraph(graph, cave, LandmarkType.Cave, UnitType.Enemy);
    }


    /***** PRIVATE: METHODS *****/
    void _ApplyToGraph(GraphMatrix graph, List<Edge> edges, LandmarkType ltype, UnitType allowed) {
        foreach (var e in edges) {
            var p = graph.GetPath(e);
            p.landmarkType = ltype;
            p.allowedUnitType = allowed;
        }
    }
}


public class NetworkGame : MonoBehaviour {
    /***** CONSTS, STATIC VARS *****/
    /** Map **/
    const int WIDTH = 8;
    const int HEIGHT = 8;
    const float CAPTURE_DISTANCE = 0.05f;
    static int[] LENGTHS = {1, 1, 1, 4};

    /** Units **/
    const float PLAYER_SPEED = 1f;
    const float ENEMY_MIN_SPEED = 0.5f;
    const float ENEMY_MAX_SPEED = 0.5f;
    const int START_ENEMY_COUNT = 6;
    const int MORE_ENEMIES_PER_STAGE = 2;

    /** Dialogue **/
    const float SECONDS_BETWEEN_TEXT = 3;

    /** Additional Game States **/
    const int NUMBER_OF_DAYS = 11;
    const int NUMBER_OF_GEMS = 5;


    /***** PUBLIC: VARIABLES *****/
    public Text statusTextbox;
    public Text speechTextbox;


    /***** PRIVATE: VARIABLES *****/
    Town town;

    /** Game status **/
    int num_enemies = START_ENEMY_COUNT;
    int turns;
    int stage = 1;
    bool gemPickedUp = false;

    /** Graph **/
    GraphMatrix graph;
    GraphRenderer graphRenderer;

    /** Units **/
    Player player;
    List<Enemy> enemies;

    /** Town **/
    List<Coord> gemPositions;
    List<RectRenderer> gemRenderers;


    /***** INITIALIZERS *****/
	void Awake () {
        StartCoroutine(Play());
	}


    /***** MAIN LOGIC *****/


    IEnumerator Play() {
        while (true) {
            Setup();
            UpdateStatusBoard();
            do {
                yield return StartCoroutine(PlayTurn());
                /*PlayTurn2();*/
                yield return null;
                UpdateStatusBoard();
            } while (!(PlayerIsDead() || WonLevel()));
            ShowResult();
            if (WonLevel()) {
                MadeItBack();
                UpdateStatusBoard();
                yield return StartCoroutine(ScheherazadeSpeaks());
                IncreaseDifficulty();
            } else {
                UpdateStatusBoard();
                ResetDifficulty();
            }
        }
    }


    /***** SETUP *****/


    void Setup() {
        /* Reset: if there are any renderers in the scene, destroy them */
        foreach (var ur in GameObject.FindObjectsOfType<UnitRenderer>()) {
            Destroy(ur.gameObject);
        }
        foreach (var ur in GameObject.FindObjectsOfType<RectRenderer>()) {
            Destroy(ur.gameObject);
        }

        /* Create Town */
        if (town == null) town = new Town();

        /* Setup Graph */
        graph = new GraphMatrix();
        foreach (var e in Edge.EdgesBetweenCoords(new Coord(0, 0), new Coord(WIDTH, HEIGHT))) {
            graph.AddPath(new Path(e, RandomLength()));
        }

        town.ApplyToGraph(graph);
        var occupied = town.CreateBaseOccupied();

        /* Create Gem: possible to be run multiple times */
        gemPositions = new List<Coord>();
        gemRenderers = new List<RectRenderer>();
        for (int i = 0; i < NUMBER_OF_GEMS; i++) {
            gemPositions.Add(Coord.RandomCoord(WIDTH+1, HEIGHT+1, occupied, true));
            gemRenderers.Add(ShapeGOFactory.InstantiateRect(
                    new RectProperty(
                        center:gemPositions[i].ToVector(),
                        width: 0.2f,
                        height: 0.2f,
                        color: Color.red,
                        angle: 45,
                        layer: -2
                    )));
        }

        /* Create Units */
        player = new Player(Town.CoordsForLandmark(town.residence).GetRandomElement<Coord>(), PLAYER_SPEED);
        enemies = new List<Enemy>();
        for (int i=0; i<num_enemies; i++) {
            var ene = new Enemy(Coord.RandomCoord(WIDTH+1, HEIGHT+1, occupied, true),
                    Random.Range(ENEMY_MIN_SPEED, ENEMY_MAX_SPEED));
            enemies.Add(ene);
        }

        /* Create Unit Renderers */
        new GameObject().AddComponent<UnitRenderer>().unit = player;
        foreach (var e in enemies) {
            new GameObject().AddComponent<UnitRenderer>().unit = e;
        }


        /* Render Graph */
        graphRenderer = new GraphRenderer();
        graphRenderer.RenderGraph(graph);
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

        PickUpGem();
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

    void PickUpGem() {
        for (int i = 0; i < gemPositions.Count; i++) {
            var coord = gemPositions[i];
            if (Approx(coord.ToVector(), player.position)) {
                gemPositions.RemoveAt(i);
                var renderer = gemRenderers[i];
                Destroy(renderer.gameObject);
                gemRenderers.RemoveAt(i);
                player.gemsCarrying++;
                gemPickedUp = true;
                return;
            }
        }
    }

    /***** SCHEHERAZADE *****/
    IEnumerator ScheherazadeSpeaks() {
        speechTextbox.text = "";
        yield return new WaitForSeconds(0.4f);
        foreach (var line in DialogueSystem.DialogueForStage(stage)) {
            speechTextbox.text = line;
            yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
        }
        speechTextbox.text = "";
    }

    void UpdateStatusBoard() {
        var location = "Path";
        var landmark = PlayerPositionToLandmark();
        if (landmark != LandmarkType.None) {
            location = landmark.ToString();
        }

        var inventory = "Gems Carrying: " + player.gemsCarrying;

        var totalGemsCollected = "Gems Brought Back: " + town.gemsCollected;

        var daysLeft = string.Format("{0} days until the end of the world", NUMBER_OF_DAYS-stage);

        statusTextbox.text = string.Format("{0}\n{1}\n{2}\n{3}\n",
                daysLeft, location, totalGemsCollected, inventory);
    }


    /***** END GAME LOGIC *****/
    void MadeItBack() {
        town.gemsCollected += player.gemsCarrying;
    }

    void IncreaseDifficulty() {
        SharedReset();
        num_enemies += MORE_ENEMIES_PER_STAGE;
        stage++;
    }

    void ResetDifficulty() {
        SharedReset();
        num_enemies = START_ENEMY_COUNT;
        /*turns = 0;*/
        stage++;
    }

    void SharedReset() {
        player.gemsCarrying = 0;
        gemPickedUp = false;
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
        if (!PlayerIsDead() && GemPickedUp() && PlayerInLandmark(LandmarkType.Residence)) {
            return true;
        }
        return false;
    }

    bool GemPickedUp() {
        return gemPickedUp;
    }

    bool PlayerInLandmark(LandmarkType ltype) {
        if (player.edge.isVertex) {
            foreach (var path in graph.GetAdjacentPaths(player.edge.p1)) {
                if (path.landmarkType == ltype) {
                    return true;
                }
            }
        } else {
            var path = graph.GetPath(player.edge);
            if (path != null && path.landmarkType == ltype) {
                return true;
            }
        }
        return false;
    }

    LandmarkType PlayerPositionToLandmark() {
        if (player.edge.isVertex) {
            foreach (var path in graph.GetAdjacentPaths(player.edge.p1)) {
                if (path.landmarkType != LandmarkType.None) {
                    return path.landmarkType;
                }
            }
        } else {
            var path = graph.GetPath(player.edge);
            if (path != null) {
                return path.landmarkType;
            }
        }
        return LandmarkType.None;
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


    /***** HELPERS *****/
    int RandomLength() {
        return LENGTHS[Random.Range(0, LENGTHS.Length)];
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

    const float LINE_WIDTH_SCALE = 0.5f;
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
            //float width = LINE_WIDTH_SCALE / path.length;
            float width = 0.2f * path.length;
            float angle = edge.orientation == Orientation.Vertical ? 90 : 0;

            var color = Color.white;
            switch (path.landmarkType) {
                case LandmarkType.Cave:
                    color = Color.cyan;
                    break;
                case LandmarkType.Hill:
                    color = Color.magenta;
                    break;
                case LandmarkType.Library:
                    color = Color.green;
                    break;
                case LandmarkType.Residence:
                    color = Color.yellow;
                    break;
                default:
                    color = Color.white;
                    break;
            }

            ShapeGOFactory.InstantiateShape(new RectProperty(
                        center: center, height: length, width: width, angle: angle, color: color
            ));
        }
    }
}

}
