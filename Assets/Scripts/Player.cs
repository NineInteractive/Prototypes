using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Nine;

namespace NetworkGame {

public class Player : Unit {

    public List<Artifact> inventory = new List<Artifact>();

    public Player(float x, float y, float speed): this(new Coord(x, y), speed) {}
    public Player(Vector2 c, float speed): this(new Coord(c.x, c.y), speed) {}
    public Player(Coord c, float speed) : base(c, speed) { }

    public void MoveToward(World world, Direction dir) {
        var target = FindNextDestination(origin, dir);

        if (world.tiles.ContainsKey(target) && !world.tiles[target].Impassable) {
            destination = target;
        }
    }

    public override void Move(World world, float deltaTime) {
        // atrocious. sigh.
        if (origin == destination) return;

        var cachedOrigin = origin; // copy

        base.Move(world, deltaTime);

        /* do something?
        if (origin == destination) {
            //graph.GetPath(new Edge(cachedOrigin, destination)).length+=1.5f;
        }
        */
    }

    string InventorySummary() {
        if (inventory.Count > 0) {
            return "You bring Scheherazade nothing.\nShe doesn't have much to say tonight.";
        }

        var summary = "You bring Scherazade ";
        var artifactToString = new List<string>();

        if (InventoryContains(ArtifactType.Gem)) {
            artifactToString.Add("a gem");
        }

        if (InventoryContains(ArtifactType.Cup)) {
            artifactToString.Add("a cup");
        }

        if (InventoryContains(ArtifactType.Arrow)) {
            artifactToString.Add("an arrow");
        }

        summary += string.Join(", ", artifactToString.ToArray()) + ".";
        return summary;
    }

    bool InventoryContains(ArtifactType type) {
        foreach (var artifact in inventory) {
            if (artifact.type == type) return true;
        }
        return false;
    }
}

}
