using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class WorldRenderer {

    const float TILE_SCALE = 1f;

    public Dictionary<Coord, RectRenderer> tileRendererDict;

    public void RenderWorld(World world) {
        if (tileRendererDict != null) {
            foreach (var pair in world.tiles) {
                var coord = pair.Key;
                var tile = pair.Value;
                tileRendererDict[coord].property = RectPropertyFromTile(tile);
            }

            return;
        }

        tileRendererDict = new Dictionary<Coord, RectRenderer>();
        foreach (var pair in world.tiles) {
            var coord = pair.Key;
            var tile = pair.Value;
            var rectRend = ShapeGOFactory.InstantiateRect(RectPropertyFromTile(tile));
            tileRendererDict[coord] = rectRend;
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
        float length = TILE_SCALE * 0.8f;
        float width = TILE_SCALE * 0.8f;

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
                color.a = 0.03f;
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
                    center: center, height: length, width: width, color: color
        ));
    }
}
}


