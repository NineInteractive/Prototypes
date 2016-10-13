using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class StoryMachine {
    Player player;
    World world;
    Teleprompter tele;

    // TODO remove teleprompter dependency
    public StoryMachine(Player player, World world, Teleprompter tele) {
        this.player = player;
        this.world = world;
        this.tele = tele;
    }

    public IEnumerator StoryForDay(int day) {
        var text = new string[]{};
        switch (day) {
            case 0:
                text = new[]
                {"Scheherazade stands in the middle of the courtyard.",
                 "\"It's morning. I must go to sleep.\"",
                 "She won't look at you."};
                break;

            case 1:
                text = new[]
                {"Scheherazade stands in the middle of the courtyard.",
                 "\"It's morning. I must go to sleep.\"",
                 "She won't look at you."};
                break;

            case 2:
                break;

            case 3:
                break;

            default:
                break;
        }
        yield return DisplayText(text);
    }

    public IEnumerator StoryForTurn(
            Tile tile,
            Tile[] visibleTiles,
            Artifact artifactCollected,
            int stepsLeft) {
        yield return DisplayText(TaleForTilesVisible(visibleTiles));
        yield return DisplayText(TaleForOccupyingTile(tile));
        yield return DisplayText(TaleForCollectedArtifact(artifactCollected));
        yield return DisplayText(TaleForNumberOfStepsLeft(stepsLeft));
    }

    public IEnumerator DisplayText(string[] text) {
        yield return tele.DisplayLines(text);
    }

    /***** PRIVATE: Story Fragments *****/

    string[] TaleForTilesVisible(Tile[] visibleTiles) {
        /*
        switch (tileType) {
            case TileType.Cave:
                return new[] {"The cave is empty."};
                break;

            case TileType.Tower:
                return new[] {"The cave is empty."};
                break;

            case TileType.Library:
                return new[] {"The cave is empty."};
                break;

            case TileType.Castle:
                return new[] {"The cave is empty."};
                break;

            case TileType.Arch:
                return new[] {"The cave is empty."};
                break;

            case TileType.Beacon:
                return new[] {"The cave is empty."};
                break;

            case TileType.Path:
                break;

            default:
                break;
        }
        */
        return null;
    }

    string[] TaleForOccupyingTile(Tile tile) {
        switch (tile.type) {
            case TileType.Cave:
                return new[] {"The cave is empty."};
                break;

            case TileType.Tower:
                return new[] {"The cave is empty."};
                break;

            case TileType.Library:
                return new[] {"The cave is empty."};
                break;

            case TileType.Castle:
                return new[] {"The cave is empty."};
                break;

            case TileType.Arch:
                return new[] {"The cave is empty."};
                break;

            case TileType.Beacon:
                return new[] {"The cave is empty."};
                break;

            case TileType.Path:
                break;

            default:
                break;
        }
        return null;
    }

    string[] TaleForCollectedArtifact(Artifact art) {
        if (art == null) return null;
        switch (art.type) {
            case ArtifactType.Gem:
            case ArtifactType.Cup:
            case ArtifactType.Bow:
            case ArtifactType.Mirror:
            case ArtifactType.PocketKnife:
            case ArtifactType.Arrow:
                break;
        }
        return null;
    }

    string[] TaleForNumberOfStepsLeft(int stepsLeft) {
        switch (stepsLeft) {
            case 20:
                return new[]
                {"You head out with a little bit of hope."};
                break;

            case 15:
                return new[]
                {"The sun is directly above you. You feel dizzy."};
                break;

            case 10:
                return new[]
                {"Afternoon. Here you try to live out another day, discover something new to bring."};
                break;

            case 5:
                return new[]
                {"The day is almost over, the sky is turning bright orange."};
                break;

            case 0:
                return new[]
                {"It's time to return home."};
                break;

            default:
                break;
        }
        return null;
    }


}

}
