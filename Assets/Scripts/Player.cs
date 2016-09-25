using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Nine;

namespace NetworkGame {

public class Player : Unit {

    public int gemsCarrying;

    Textbox speechTextbox;

    public Player(float x, float y, float speed, Textbox textbox): this(new Coord(x, y), speed, textbox) {}
    public Player(Vector2 c, float speed, Textbox textbox): this(new Coord(c.x, c.y), speed, textbox) {}
    public Player(Coord c, float speed, Textbox textbox) : base(c, speed) {
        speechTextbox = textbox;
    }

    public void MoveToward(Direction dir) {
        destination = FindNextDestination(origin, dir);
    }

    public override void Move(GraphMatrix graph, float deltaTime) {
        // atrocious. sigh.
        if (origin == destination) return;

        var cachedOrigin = origin; // copy

        base.Move(graph, deltaTime);

        if (origin == destination) {
            graph.GetPath(new Edge(cachedOrigin, destination)).length+=1.5f;
        }
    }

    public void EncounterNewDay(int day) {
        switch (day) {
            case 0:
                speechTextbox.Speak(
                        "Scheherazade stands in the middle of the courtyard.",
                        "\"It's morning. I must go to sleep.\"");
                break;

            case 1:
                speechTextbox.Speak(
                        "You return to scherazade, with a trinket on your palm.",
                        "She puts it on reluctantly.",
                        "The following night, Scheherazade begins the story of the sage Devan and King Yunan.",
                        "But the morning overtakes her, and she lapses into silence.");
                break;

            default:
                break;
        }
    }

    public void EncounterEnemy(Enemy enemy) {
    }

    public void EncounterLandmark(LandmarkType landmarkType) {
    }
}

}
