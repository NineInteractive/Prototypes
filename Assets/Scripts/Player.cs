using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Nine;

namespace NetworkGame {

public class Player : Unit {

    public List<Artifact> inventory = new List<Artifact>();

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
            //graph.GetPath(new Edge(cachedOrigin, destination)).length+=1.5f;
        }
    }

    public void NewDay() {
        inventory.Clear();
    }

    public void EncounterNewDay(int day) {
        switch (day) {
            case 0:
                Say("Scheherazade stands in the middle of the courtyard.",
                    "\"It's morning. I must go to sleep.\"");
                break;

            case 1:
                Say("You return to scherazade, with a trinket on your palm.",
                    "She puts it on reluctantly.",
                    "The following night, Scheherazade begins the story of the sage Devan and King Yunan.",
                    "But the morning overtakes her, and she lapses into silence.");
                break;

            default:
                break;
        }
    }

    public void EncounterEnemy(Enemy enemy) {
        Say("I greet the old king.", "He takes the gem away from you.");
    }

    public void EncounterLandmark(LandmarkType landmarkType) {
        switch (landmarkType) {
            case LandmarkType.Cave:
                Say("The cave is empty.");
                break;

            case LandmarkType.Hill:
                Say("From the hilltop, you can see the high balustrade of your palace.");
                break;

            case LandmarkType.Library:
                Say("This is a library. The old king refuses to enter.");
                break;

            case LandmarkType.Residence:
                Say("Back home.");
                break;

            case LandmarkType.None:
                if (Random.value < 0.05f) {
                    if (Random.value < 0.5f) {
                        Say("The sun is right above you.");
                    } else {
                        Say("You briefly forget about Scherazade.");
                    }
                }
                break;

            default:
                break;
        }
    }

    void Say(params string[] speech) {
        speechTextbox.Speak(speech);
    }
}

}
