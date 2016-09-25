using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class NetworkGame : MonoBehaviour {
    /***** CONSTS, STATIC VARS *****/
    /** Map **/
    const int WIDTH = 8;
    const int HEIGHT = 8;
    const float CAPTURE_DISTANCE = 0.05f;
    static int[] LENGTHS = {1, 1, 1, 1};

    /** Units **/
    const float PLAYER_SPEED = 1f;
    const float ENEMY_MIN_SPEED = 0.5f;
    const float ENEMY_MAX_SPEED = 0.7f;
    const int START_ENEMY_COUNT = 16;
    const int MORE_ENEMIES_PER_day = 2;

    /** Dialogue **/
    const float SECONDS_BETWEEN_TEXT = 3;

    /** Additional Game States **/
    const int NUMBER_OF_DAYS = 11;
    const int NUMBER_OF_GEMS = 5;


    /***** PUBLIC: VARIABLES *****/
    public Text statusTextbox;
    public Text speechTextbox;
    public Textbox textbox;


    /***** PRIVATE: VARIABLES *****/
    Town town;

    /** Game status **/
    int num_enemies = START_ENEMY_COUNT;
    int turns;
    int day = 0;
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
            player.EncounterNewDay(day);
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
                //yield return StartCoroutine(ScheherazadeSpeaks());
                IncreaseDifficulty();
                player.EncounterNewDay(day);
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
        if (graph == null) {
            graph = new GraphMatrix();
            foreach (var e in Edge.EdgesBetweenCoords(new Coord(0, 0), new Coord(WIDTH, HEIGHT))) {
                graph.AddPath(new Path(e, RandomLength()));
            }

            town.ApplyToGraph(graph);
        }
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
        player = new Player(Town.CoordsForLandmark(town.residence).GetRandomElement<Coord>(), PLAYER_SPEED, textbox);
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
            e.Chase(player, graph, Time.deltaTime, GemPickedUp(), PlayerInSafeZone());
        }

        /** Move Player **/
        if (player.RestingAtVertex()) {
            graphRenderer.RenderGraph(graph);
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
        foreach (var line in DialogueSystem.DialogueForStage(day)) {
            speechTextbox.text = line;
            yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
        }
        speechTextbox.text = "";
    }

    void UpdateStatusBoard() {
        return;
        var location = "Path";
        var landmark = PlayerPositionToLandmark();
        if (landmark != LandmarkType.None) {
            location = landmark.ToString();
        }

        var inventory = "Gems Carrying: " + player.gemsCarrying;

        var totalGemsCollected = "Gems Brought Back: " + town.gemsCollected;

        var daysLeft = string.Format("{0} days until the end of the world", NUMBER_OF_DAYS-day);

        statusTextbox.text = string.Format("{0}\n{1}\n{2}\n{3}\n",
                daysLeft, location, totalGemsCollected, inventory);
    }


    /***** END GAME LOGIC *****/
    void MadeItBack() {
        town.gemsCollected += player.gemsCarrying;
    }

    void IncreaseDifficulty() {
        SharedReset();
        num_enemies += MORE_ENEMIES_PER_day;
        day++;
    }

    void ResetDifficulty() {
        SharedReset();
        num_enemies = START_ENEMY_COUNT;
        /*turns = 0;*/
        day++;
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

}
