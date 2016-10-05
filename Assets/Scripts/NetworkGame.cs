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
    const int WIDTH = 10;
    const int HEIGHT = 10;
    const float CAPTURE_DISTANCE = 0.1f;
    static float[] LENGTHS = {1, 1, 1, 1};

    /** Units **/
    const float PLAYER_SPEED = 1f;
    const float ENEMY_MIN_SPEED = 1f;
    const float ENEMY_MAX_SPEED = 1f;
    const int START_ENEMY_COUNT = 0;
    const int MORE_ENEMIES_PER_day = 0;

    /** Dialogue **/
    const float SECONDS_BETWEEN_TEXT = 5;

    /** Additional Game States **/
    const int NUMBER_OF_DAYS = 4;
    const int NUMBER_OF_GEMS = 3;
    const int NUMBER_OF_STEPS_PER_DAY = 21;


    /***** PUBLIC: VARIABLES *****/
    public Text statusTextbox;
    public Teleprompter teleprompter;


    /***** PRIVATE: VARIABLES *****/
    Town town;

    /** Game status **/
    int num_enemies = START_ENEMY_COUNT;
    int steps;
    int day;

    /** Graph **/
    GraphMatrix graph;
    GraphRenderer graphRenderer;

    /** Units **/
    Player player;
    List<Enemy> enemies;

    /** Town **/
    List<Artifact> artifacts;
    List<RectRenderer> artifactRenderers;


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
                UpdateStatusBoard();
            } while (!(PlayerIsDead() || WonLevel()));

            player.EncounterNewTile(NUMBER_OF_STEPS_PER_DAY - steps);
            ShowResult();

            if (WonLevel()) {
                MadeItBack();
                UpdateStatusBoard();
                player.EncounterNewDay(day+1);
                //yield return StartCoroutine(ScheherazadeSpeaks());
                IncreaseDifficulty();
            } else {
                UpdateStatusBoard();
                ResetDifficulty();
                player.EncounterNewDay(day+1);
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
        artifacts = new List<Artifact>();
        artifactRenderers = new List<RectRenderer>();

        var gemPosition = Coord.RandomCoord(WIDTH+1, HEIGHT+1, occupied, true);
        var cupPosition = Coord.RandomCoord(WIDTH+1, HEIGHT+1, occupied, true);
        var arrowPosition = Coord.RandomCoord(WIDTH+1, HEIGHT+1, occupied, true);

        artifacts.Add(new Artifact(gemPosition, ArtifactType.Gem));
        artifacts.Add(new Artifact(cupPosition, ArtifactType.Cup));
        artifacts.Add(new Artifact(arrowPosition, ArtifactType.Arrow));

        foreach (var artifact in artifacts) {
            artifactRenderers.Add(ShapeGOFactory.InstantiateRect(
                    new RectProperty(
                        center: artifact.position.ToVector(),
                        width: 0.2f,
                        height: 0.2f,
                        color: Color.red,
                        angle: 45,
                        layer: -2
                    )));
        }

        /* Create Units */
        player = new Player(Town.CoordsForLandmark(town.residence).GetRandomElement<Coord>(), PLAYER_SPEED, teleprompter);
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
            ModifyVisibility();
            player.EncounterLandmark(PlayerPositionToLandmark());
            player.EncounterNewTile(NUMBER_OF_STEPS_PER_DAY - steps);
            graphRenderer.RenderGraph(graph);
			while (DirectionUtil.FromInput() == Direction.None || teleprompter.displaying) {
                if (DirectionUtil.FromInput() != Direction.None) teleprompter.DisplayImmediately();
				yield return null;
			}
            steps++;
            player.MoveToward(DirectionUtil.FromInput());
        }
        player.Move(graph, Time.deltaTime);

        PickUpArtifact();
    }

    void ModifyVisibility() {
        // for each adjacent 3x3, make it visible
        // for each 5-3x5-3, make it grayed
        // rerender graph
        foreach (var path in graph.GetAdjacentPaths(player.origin)) {
            foreach (var path2 in graph.GetAdjacentPaths(path.edge.p1)) {
                if (path2.visibility != Visibility.Revealed) path2.visibility = Visibility.Grayed;
            }
            foreach (var path2 in graph.GetAdjacentPaths(path.edge.p2)) {
                if (path2.visibility != Visibility.Revealed) path2.visibility = Visibility.Grayed;
            }
        }
        foreach (var path in graph.GetAdjacentPaths(player.origin)) {
            path.visibility = Visibility.Revealed;
        }
    }

    void PlayTurn2() {
        /** Move Enemies **/
        foreach (var e in enemies) {
            e.Chase(player, graph, Time.deltaTime, GemPickedUp());
        }

        /** Move Player **/
        if (player.RestingAtVertex()) {
            steps++;
            player.MoveToward(DirectionUtil.FromInput());
        }
        player.Move(graph, Time.deltaTime);
    }

    void PickUpArtifact() {
        for (int i = 0; i < artifacts.Count; i++) {
            var artifact = artifacts[i];
            if (Approx(artifact.position.ToVector(), player.position)) {
                artifacts.RemoveAt(i);
                var renderer = artifactRenderers[i];
                Destroy(renderer.gameObject);
                artifactRenderers.RemoveAt(i);
                player.inventory.Add(artifact);
                return;
            }
        }
    }

    /***** SCHEHERAZADE *****/
    IEnumerator ScheherazadeSpeaks() {
        /*
        speechTextbox.text = "";
        yield return new WaitForSeconds(0.4f);
        foreach (var line in DialogueSystem.DialogueForStage(day)) {
            speechTextbox.text = line;
            yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
        }
        speechTextbox.text = "";
        */
        yield return null;
    }

    void UpdateStatusBoard() {
        var location = "Path";
        var landmark = PlayerPositionToLandmark();
        if (landmark != LandmarkType.None) {
            location = landmark.ToString();
        }

        var inventory = "Carrying ";
        if (player.inventory.Count == 0) {
            inventory += "Nothing";
        }
        else {
            foreach (var artifact in player.inventory) {
                inventory += artifact.type + " ";
            }
        }

        var totalGemsCollected = "Artifacts Brought Back: " + town.gemsCollected;

        var daysLeft = string.Format("{0} days until the end of the world", NUMBER_OF_DAYS-day);

        var stepsLeft = string.Format("{0} steps until the end of the day", NUMBER_OF_STEPS_PER_DAY-steps);

        statusTextbox.text = string.Format("{0}\n{1}\n{2}\nLocation: {3}\n{4}\n",
                daysLeft, stepsLeft, inventory, location, totalGemsCollected);
    }


    /***** END GAME LOGIC *****/
    void MadeItBack() {
        town.gemsCollected += player.inventory.Count;
    }

    void IncreaseDifficulty() {
        SharedReset();
        num_enemies += MORE_ENEMIES_PER_day;
        day++;
    }

    void ResetDifficulty() {
        SharedReset();
        num_enemies = START_ENEMY_COUNT;
        day++;
    }

    void SharedReset() {
        player.NewDay();
        steps = 0;
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
        if ((!PlayerIsDead() && GemPickedUp() && PlayerInLandmark(LandmarkType.Residence))
            || NUMBER_OF_STEPS_PER_DAY - steps == 0) {
            return true;
        }
        return false;
    }

    bool GemPickedUp() {
        return player.inventory.Count > 0;
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
    float RandomLength() {
        return LENGTHS[Random.Range(0, LENGTHS.Length)];
    }

    bool Approx(Vector2 p1, Vector2 p2) {
        return Vector2.Distance(p1, p2) < CAPTURE_DISTANCE;
    }


    /***** RENDERING *****/
    void ShowResult() {
        Debug.Log("Survived for " + steps + " steps");
    }

}

}
