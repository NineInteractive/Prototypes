using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class WorldRenderer : MonoBehaviour {

    const float TILE_SCALE = 1f;

    public Dictionary<Coord, RectRenderer> tileRendererDict = new Dictionary<Coord, RectRenderer>();
    public Dictionary<Artifact, RectRenderer> artifactRendererDict = new Dictionary<Artifact, RectRenderer>();

    public WorldRenderer() { }

    public void RenderWorld(World world) {
        // render tiles
        foreach (var pair in world.tiles) {
            var coord = pair.Key;
            var tile = pair.Value;
            if (!tileRendererDict.ContainsKey(coord)) {
                var renderer = ShapeGOFactory.InstantiateRect(RectPropertyFromTile(tile));
                renderer.transform.parent = transform;
                tileRendererDict[coord] = renderer;
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
    }

    /*

    public void RerenderPath(Path path) {
        RectRenderer rend;
        if (edgeRendererDict.TryGetValue(path.edge, out rend)) {
            rend.property = RectPropertyFromPath(path);
        }
    }
    */

    RectProperty RectPropertyFromTile(Tile tile) {
        Vector2 center = tile.position.ToVector() * TILE_SCALE;
        float length = TILE_SCALE * 0.5f;
        float width = TILE_SCALE * 0.5f;

        var color = Color.white;
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

        switch (tile.visibility) {
            case Visibility.Hidden:
                color.a = 0;
                break;
            case Visibility.Grayed:
                if (tile.type == TileType.Blocked) {
                    color.a = 1;
                } else {
                    color.a = 0.03f;
                }
                break;
            case Visibility.Revealed:
                if (tile.type == TileType.Blocked) {
                    color.a = 1;
                } else {
                    color.a = 1;
                }
                break;
        }

        return (new RectProperty(
                    center: center, height: length, width: width, color: color
        ));
    }

    RectProperty RectPropertyFromArtifact(Artifact art) {
        Vector2 center = art.position.ToVector() * TILE_SCALE;
        float length = TILE_SCALE * 0.2f;
        float width = TILE_SCALE * 0.2f;

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


