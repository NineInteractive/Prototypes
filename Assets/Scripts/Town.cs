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
    Path, Blocked, Castle, Cave, Library, Tower
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

// coordinate => landmark
// landmark => coordinate? is this needed? otherwise we just - but how do we detect overlaps?
// just by looking through the tiles. not sure if we really need the reverse mapping
// now so let it go.
public struct Selection {
    public Coord bottomLeft;
    public Coord topRight;

    public Selection(Coord bl, Coord tr) {
        bottomLeft = bl;
        topRight = tr;
    }

    public static Selection RandomSelection(int width, int height, int maxX, int maxY) {
        var bottomLeft = Coord.RandomCoord(maxX-width, maxY-height);
        var topRight = bottomLeft.MovedBy(width, height);
        return new Selection(bottomLeft, topRight);
    }

    // linq?
    public IEnumerable<Coord> Coords() {
        for (int i=bottomLeft.x; i<=topRight.x; i++) {
            for (int j=bottomLeft.y; j<=topRight.y; j++) {
                yield return new Coord(i, j);
            }
        }
    }

    public Coord RandomCoord() {
        return Coord.RandomCoord(width, height).Plus(bottomLeft);
    }

    public int width {
        get {
            return topRight.x - bottomLeft.x;
        }
    }

    public int height {
        get {
            return topRight.y - bottomLeft.y;
        }
    }

    public override string ToString() {
        return string.Format("Selection: {0}, {1}", bottomLeft, topRight);
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
        // place gems
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

    public IEnumerable<Tile> GetAdjacentTiles(Coord coord) {
        foreach (var c in coord.AdjacentCoords(includeDiagonal:true, includeSelf:true)) {
            if (tiles.ContainsKey(c)) yield return tiles[c];
        }
    }

    /*
    public override string ToString() {
    }
    */
}
}
