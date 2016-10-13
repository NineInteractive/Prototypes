using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class Tile {
    public TileType type;
    public Coord position;
    public Visibility visibility;

    public Tile(Coord pos, TileType type=TileType.Path, Visibility vis=Visibility.Hidden) {
        position = pos;
        this.type = type;
        visibility = vis;
    }
}

public enum TileType {
    // TODO landmark type?
    Path, Blocked, Castle, Cave, Library, Tower, Beacon, Arch
}

public class Landmark {
    public List<Coord> coords;
    public TileType type;

    public Landmark(Coord coord, TileType type=TileType.Path) {
        coords = new List<Coord>();
        coords.Add(coord);
        this.type = type;
    }

    public Landmark(List<Coord> coords, TileType type = TileType.Path) {
        this.coords = coords;
        this.type = type;
    }

    public void SetTiles(List<Tile> tiles) {
        foreach (var t in tiles) {
            t.type = type;
        }
    }
}

public class World {
    const int WIDTH = 20;
    const int HEIGHT = 20;

    /***** PUBLIC: VARIABLES *****/
    public IntRange numberOfBlocks = new IntRange(40, 40);
    public IntRange blockSize = new IntRange(0, 3);

    public Dictionary<Coord, Tile> tiles = new Dictionary<Coord, Tile>();

    /***** PRIVATE: VARIABLES *****/
    public Selection castle;

    /** Artifacts **/
    Dictionary<ArtifactType, Artifact> artifacts = new Dictionary<ArtifactType, Artifact>();
    public List<Artifact> visibleArtifacts = new List<Artifact>();
    public Dictionary<ArtifactType, Artifact> collectedArtifacts = new Dictionary<ArtifactType, Artifact>();

    /***** PUBLIC: STATIC METHODS *****/
    public void GenerateWorld() {
        // put castle in the center
        LandmarkAtCoords(centerCoord.AdjacentCoords(WIDTH, HEIGHT), TileType.Castle);
        castle = new Selection(centerCoord.MovedBy(-1, -1), centerCoord.MovedBy(1, 1));

        // place randomized blocks of various sizes
        var blockCount = numberOfBlocks.RandomValue();

        for (int i = 0; i < blockCount; i++) {
            CreateLandmark(blockSize.RandomValue(), blockSize.RandomValue(), TileType.Blocked);
        }

        // place other landmarks
        CreateLandmark(1, 1, TileType.Cave);
        CreateLandmark(2, 1, TileType.Library);
        CreateLandmark(1, 2, TileType.Tower);

        CreateArtifacts();
    }

    Selection CreateLandmark(int width, int height, TileType type) {
        Selection landmark;

        do {
            landmark = Selection.RandomSelection(width, height, WIDTH, HEIGHT);
        } while (Overlapped(landmark));

        foreach (var c in landmark.Coords()) {
            tiles[c].type = type;
        }

        return landmark;
    }

    void CreateArtifacts() {
        artifacts[ArtifactType.Gem] = new Artifact(new Coord(3, 7), ArtifactType.Gem);
        artifacts[ArtifactType.Cup] = new Artifact(new Coord(4, 8), ArtifactType.Cup);
        artifacts[ArtifactType.Bow] = new Artifact(new Coord(5, 10), ArtifactType.Bow);
        artifacts[ArtifactType.Mirror] = new Artifact(new Coord(8, 13), ArtifactType.Mirror);
        artifacts[ArtifactType.PocketKnife] = new Artifact(new Coord(13, 2), ArtifactType.PocketKnife);
        artifacts[ArtifactType.Arrow] = new Artifact(new Coord(5, 5), ArtifactType.Arrow);

        visibleArtifacts.Add(artifacts[ArtifactType.Cup]);
        visibleArtifacts.Add(artifacts[ArtifactType.Mirror]);
        visibleArtifacts.Add(artifacts[ArtifactType.Arrow]);
    }

    bool Overlapped(Selection sel) {
        foreach (var c in sel.Coords()) {
            if (tiles[c].type != TileType.Path) {
                return true;
            }
        }
        return false;
    }

    void LandmarkAtCoords(List<Coord> coords, TileType type) {
        foreach (var c in coords) {
            tiles[c].type = type;
        }
    }

    /***** CONSTRUCTOR ******/
    public World() {
        for (int i = 0; i < WIDTH; i++) {
            for (int j = 0; j < HEIGHT; j++) {
                var c = new Coord(i, j);
                tiles[c] = new Tile(c);
            }
        }
    }

    public Coord centerCoord {
        get {
            return new Coord(WIDTH/2, HEIGHT/2);
        }
    }

    public IEnumerable<Tile> AdjacentTiles(Coord coord) {
        foreach (var c in coord.AdjacentCoords(includeDiagonal:true, includeSelf:true)) {
            if (tiles.ContainsKey(c)) yield return tiles[c];
        }
    }

    public IEnumerable<Tile> NearbyTiles(Coord coord, int xdist, int ydist) {
        foreach (var c in coord.NearbyCoords(xdist, ydist, maxX:WIDTH, maxY: HEIGHT, includeSelf:true)) {
            if (tiles.ContainsKey(c)) yield return tiles[c];
        }
    }

    /*
    public override string ToString() {
    }
    */
}
}
