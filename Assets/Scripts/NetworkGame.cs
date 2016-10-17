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

    const int fogDistance = 2;
    const int revealDistance = 1;

    /** Units **/
    const float PLAYER_SPEED = 1f;
    const float ENEMY_MIN_SPEED = 1f;
    const float ENEMY_MAX_SPEED = 1f;
    const int START_ENEMY_COUNT = 0;
    const int MORE_ENEMIES_PER_day = 0;


    /** Additional Game States **/
    const int NUMBER_OF_DAYS = 4;
    const int NUMBER_OF_GEMS = 3;
    const int NUMBER_OF_STEPS_PER_DAY = 20;


    /***** PUBLIC: VARIABLES *****/
    public Text statusTextbox;
    public Teleprompter sideTele;
    public Teleprompter fullScreenTele;
    public WorldRenderer worldRenderer;


    /***** PRIVATE: VARIABLES *****/
    World world;
    public StoryMachine story;

    /** Game status **/
    int steps;
    int day;

    /** Units **/
    Player player;

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

            yield return StartCoroutine(story.StoryForDay(day));

            ModifyVisibility();
            worldRenderer.RenderWorld(world);

            do {
                yield return StartCoroutine(PlayTurn());
                steps++;

                var artifact = PickUpArtifact();
                ModifyVisibility();
                worldRenderer.RenderWorld(world);

                yield return StartCoroutine(
                        story.StoryForTurn(
                            tile:PlayerPositionToTile(),
                            visibleTiles:world.NearbyTiles(player.origin, fogDistance, fogDistance).ToArray(),
                            artifactCollected: artifact,
                            stepsLeft: NUMBER_OF_STEPS_PER_DAY-steps));
            } while (!IsEndOfDay());

            //yield return StartCoroutine(ScheherazadeSpeaks());
            day++;
            steps = 0;
        }
    }

    /***** SETUP *****/
    void Setup() {
        /* Reset: if there are any renderers in the scene, destroy them */
        foreach (var ur in GameObject.FindObjectsOfType<UnitRenderer>()) {
            Destroy(ur.gameObject);
        }

        /* Create Town */
        if (world == null) {
            world = new World();
            world.GenerateWorld();
        }

        /* Create Units */
        player = new Player(world.centerCoord, PLAYER_SPEED);

        /* Create Unit Renderers */
        new GameObject().AddComponent<UnitRenderer>().unit = player;

        /* Render Graph */
        worldRenderer.RenderWorld(world);

        story.SetStoryObjects(player, world);
    }

    IEnumerator NightScene() {
        // Fade to black
        // Use full-screen sideTele to display text
        yield return StartCoroutine(story.StoryForDay(day));
    }


    /***** PLAY LOGIC *****/
    IEnumerator PlayTurn() {
        /** Move Player **/
        while (DirectionUtil.FromInput() == Direction.None) {
            yield return null;
        }
        player.MoveToward(DirectionUtil.FromInput());

        while (!player.RestingAtVertex()) {
            player.Move(world, Time.deltaTime);
            yield return null;
        }
    }

    void ModifyVisibility() {
        foreach (var tile in world.NearbyTiles(player.origin, fogDistance, fogDistance)) {
            if (tile.visibility != Visibility.Revealed) tile.visibility = Visibility.Grayed;
        }

        foreach (var tile in world.NearbyTiles(player.origin, revealDistance, revealDistance)) {
            tile.visibility = Visibility.Revealed;
        }
    }

    Artifact PickUpArtifact() {
        for (int i = 0; i < world.visibleArtifacts.Count; i++) {
            var artifact = world.visibleArtifacts[i];

            if (player.RestingAtVertex() && artifact.position == player.origin) {
                world.visibleArtifacts.RemoveAt(i);
                player.inventory.Add(artifact);
                return artifact;
            }
        }
        return null;
    }

    /***** SCHEHERAZADE *****/
    void UpdateStatusBoard() {
        /*
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

        /*
        var totalGemsCollected = "Artifacts Brought Back: " + town.gemsCollected;

        var daysLeft = string.Format("{0} days until the end of the world", NUMBER_OF_DAYS-day);

        var stepsLeft = string.Format("{0} steps until the end of the day", NUMBER_OF_STEPS_PER_DAY-steps);

        statusTextbox.text = string.Format("{0}\n{1}\n{2}\nLocation: {3}\n{4}\n",
                daysLeft, stepsLeft, inventory, location, totalGemsCollected);
                */
    }


    /***** END GAME LOGIC *****/
    void MadeItBack() {
        //town.gemsCollected += player.inventory.Count;
    }

    /***** PROPERTIES - GAME STATUS *****/
    bool PlayerIsDead() {
        return false;
    }

    bool WonLevel() {
        if ((!PlayerIsDead() && GemPickedUp() && PlayerInLandmark(TileType.Castle))
            || NUMBER_OF_STEPS_PER_DAY - steps == 0) {
            return true;
        }
        return false;
    }

    bool IsEndOfDay() {
        return NUMBER_OF_STEPS_PER_DAY - steps == 0;
    }

    bool GemPickedUp() {
        return player.inventory.Count > 0;
    }

    bool PlayerInLandmark(TileType ltype) {
        return ltype == PlayerPositionToLandmark();
    }

    TileType PlayerPositionToLandmark() {
        return world.tiles[player.origin].type;
    }

    Tile PlayerPositionToTile() {
        return world.tiles[player.origin];
    }

    IEnumerable<Tile> VisibleTiles() {
        return world.AdjacentTiles(player.origin);
    }

    bool PlayerInSafeZone() {
        return true;
        /*
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
        */
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
