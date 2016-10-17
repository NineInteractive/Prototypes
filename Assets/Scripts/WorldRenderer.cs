using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class WorldRenderer : MonoBehaviour {

    public float tileScale = 1f;
    public float distBetweenTiles = 0.5f;
    public float tileSideLength = 0.8f;
    public float borderThickness = 0.005f;
    public float linkLength = 0.4f;

    public TextMesh captionPrefab;

    public Dictionary<Coord, RectRenderer> tileRendererDict = new Dictionary<Coord, RectRenderer>();
    public Dictionary<Coord, TextMesh> captionDict = new Dictionary<Coord, TextMesh>();
    public Dictionary<Artifact, RectRenderer> artifactRendererDict = new Dictionary<Artifact, RectRenderer>();

    bool rendered = false;

    public WorldRenderer() { }


    public void SetupPlayerRenderer(Player player) {
        /* Create Unit Renderers */
        new GameObject().AddComponent<UnitRenderer>().unit = player;
    }

    public void RenderWorld(World world) {
        // render tiles
        foreach (var pair in world.tiles) {
            var coord = pair.Key;
            var tile = pair.Value;
            if (!tileRendererDict.ContainsKey(coord)) {
                var renderer = ShapeGOFactory.InstantiateRect(RectPropertyFromTile(tile));
                renderer.transform.parent = transform;
                tileRendererDict[coord] = renderer;

                var caption = Instantiate<TextMesh>(captionPrefab);
                caption.transform.parent = renderer.transform;
                caption.transform.localPosition = new Vector3(tileSideLength/2f, -tileSideLength/2f, -5);
                caption.text = tile.type.ToString();
                if (tile.type== TileType.Blocked) {
                    caption.color = new Color32(0,0,0,0);
                }
                captionDict[coord] = caption;
            } else {
                tileRendererDict[coord].property = RectPropertyFromTile(tile);
            }
        }

        // render artifacts
        // TODO reuse renderer
        foreach (var renderer in artifactRendererDict.Values) {
            Destroy(renderer.gameObject);
        }
        artifactRendererDict.Clear();

        foreach (var artifact in world.visibleArtifacts) {
            var renderer = ShapeGOFactory.InstantiateRect(RectPropertyFromArtifact(artifact));
            artifactRendererDict[artifact] = renderer;
        }

        if (!rendered) {
            RenderNetwork(world);
        }

        rendered = true;
    }

    void RenderNetwork(World world) {
        for (int i=0; i < World.WIDTH; i++) {
            for (int j=0; j < World.HEIGHT; j++) {
                var c = new Coord(i, j);
                var cAbove = new Coord(i, j+1);
                var cRight = new Coord(i+1, j);

                Tile tile, tileAbove, tileRight;
                if (world.tiles.TryGetValue(c, out tile) && !tile.Blocked) {
                    if (world.tiles.TryGetValue(cAbove, out tileAbove) && !tileAbove.Blocked) {
                        var r = ShapeGOFactory.InstantiateRect(RectPropertyForLink(tile, tileAbove));
                        r.transform.parent = transform;
                    }
                    if (world.tiles.TryGetValue(cRight, out tileRight) && !tileRight.Blocked) {
                        var r = ShapeGOFactory.InstantiateRect(RectPropertyForLink(tile, tileRight));
                        r.transform.parent = transform;
                    }
                }
            }
        }
    }

    /*

    public void RerenderPath(Path path) {
        RectRenderer rend;
        if (edgeRendererDict.TryGetValue(path.edge, out rend)) {
            rend.property = RectPropertyFromPath(path);
        }
    }
    */

    Vector2 CoordToVector (Coord c) {
        return new Vector2(c.x * (tileScale + distBetweenTiles), c.y * (tileScale + distBetweenTiles));
    }

    RectProperty RectPropertyForLink(Tile tile1, Tile tile2) {

        var center = (CoordToVector(tile1.position)+CoordToVector(tile2.position)) / 2f;
        var angle = tile1.position.x == tile2.position.x ? 0 : 90;

        return (new RectProperty(
                    center: center, height: linkLength, width: borderThickness, angle: angle, color: Color.black
        ));
    }

    RectProperty RectPropertyFromTile(Tile tile) {
        Vector2 center = CoordToVector(tile.position);
        float length = tileSideLength;
        float width = tileSideLength;

        BorderProperty bp = new BorderProperty();

        if (tile.type != TileType.Blocked) {
            bp = new BorderProperty(style: BorderStyle.Solid, color: Color.black, thickness:borderThickness);
        }

        var color = Color.white;
        /*
        switch (tile.type) {
            case TileType.Cave:
                color = Color.cyan;
                break;
            case TileType.Tower:
                color = Color.magenta;
                break;
            case TileType.Library:
                color = Color.green;
                break;
            case TileType.Castle:
                color = Color.yellow;
                break;
            case TileType.Blocked:
                color = Color.black;
                break;
            default:
                color = Color.white;
                break;
        }
        */

        switch (tile.visibility) {
            case Visibility.Hidden:
                color.a = 0;
                break;
            case Visibility.Grayed:
                if (tile.type == TileType.Blocked) {
                    color.a = 0;
                } else {
                    color.a = 0.03f;
                }
                break;
            case Visibility.Revealed:
                if (tile.type == TileType.Blocked) {
                    color.a = 0;
                } else {
                    color.a = 1;
                }
                break;
        }

        return (new RectProperty(
                    center: center, height: length, width: width, color: color, border: bp
        ));
    }

    RectProperty RectPropertyFromArtifact(Artifact art) {
        Vector2 center = art.position.ToVector() * tileScale;
        float length = tileScale * 0.2f;
        float width = tileScale * 0.2f;

        var color = Color.red;
        /*Gem, Cup, Bow, Mirror, PocketKnife, Arrow
        switch (art.type) {
            case TileType.Cave:
                color = Color.cyan;
                break;
            case TileType.Tower:
                color = Color.magenta;
                break;
            case TileType.Library:
                color = Color.green;
                break;
            case TileType.Castle:
                color = Color.yellow;
                break;
            case TileType.Blocked:
                color = Color.black;
                break;
            default:
                color = Color.white;
                break;
        }
        */

        return (new RectProperty(
                    center: center, height: length, width: width, color: color
        ));
    }
}
}


