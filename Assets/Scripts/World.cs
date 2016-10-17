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
    public Visibility visibility; // TODO known?

    public Tile(Coord pos, TileType type=TileType.Path, Visibility vis=Visibility.Hidden) {
        position = pos;
        this.type = type;
        visibility = vis;
    }

    public bool Impassable {
        get {
            return (int)type <= (int)TileType.Gate;
        }
    }

    public bool Rendered {
        get {
            return type != TileType.Void;
        }
    }
}

public enum TileType {
    // TODO landmark tjjjjjype?
    // Impassable Terrain
	Void=0, River=1, Cliff=2, Wall=3, Gate=4,
    // General Region
	Path=5, Plains=6, Hill=7, Mountain=8,
    // Landmark
	Palace=9, Library=10, Tower=11, Beacon=12, Arch=13, Cave=14
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
    public const int WIDTH = 15;
    public const int HEIGHT = 15;

    // Impassable: Void=0 River=1 Cliff=2 Wall=3 Gate=4
    // Region: Path=5 Plains=6 Hill=7 Mountain=8
    // Landmark: Palace=9 Library=10 Tower=11 Beacon=12 Arch=13 Cave=14

    public int[,] map = new int[,] {
		{0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},
		{2,0,8,8,8,0,12,8,8,0,0,8,8,1,0},
		{2,8,8,0,8,0,8,0,8,5,5,8,8,1,0},
		{2,8,0,8,8,8,8,0,1,5,0,0,1,1,0},
		{2,8,8,14,1,1,1,1,1,5,1,1,1,1,0},
		{3,5,1,1,0,0,5,5,5,5,0,1,0,1,0},
		{4,5,5,5,5,5,9,0,0,5,5,1,7,1,0},
		{3,0,5,0,0,0,5,7,7,0,5,7,13,1,0},
		{3,6,5,0,6,0,5,5,5,7,7,0,7,1,0},
		{3,6,6,6,6,5,5,0,0,7,10,7,7,1,0},
		{3,0,11,0,6,1,5,7,7,7,7,0,7,1,0},
		{3,6,6,6,6,1,5,1,7,0,7,0,0,1,0},
		{3,6,1,1,1,1,4,1,1,1,7,1,1,1,0},
		{3,6,1,0,0,0,0,0,0,0,4,0,0,0,0},
		{1,1,1,0,0,0,0,0,0,0,0,0,0,0,0}
    };

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
        castle = new Selection(new Coord(6, 8), new Coord(6,8));
        /*
        LandmarkAtCoords(centerCoord.NearbyCoords(0, 0, includeSelf:true), TileType.Palace);

        // place randomized blocks of various sizes
        var blockCount = numberOfBlocks.RandomValue();

        for (int i = 0; i < blockCount; i++) {
            CreateLandmark(blockSize.RandomValue(), blockSize.RandomValue(), TileType.Void);
        }

        // place other landmarks
        CreateLandmark(1, 1, TileType.Cave);
        CreateLandmark(2, 1, TileType.Library);
        CreateLandmark(1, 2, TileType.Tower);
        */

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

    void LandmarkAtCoords(IEnumerable<Coord> coords, TileType type) {
        foreach (var c in coords) {
            tiles[c].type = type;
        }
    }

    /***** CONSTRUCTOR ******/
    public World() {
        for (int i = 0; i < WIDTH; i++) {
            for (int j = 0; j < HEIGHT; j++) {
                var c = new Coord(i, j);
                tiles[c] = new Tile(c, (TileType)map[i,HEIGHT-j-1]);
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
