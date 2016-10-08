using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class GraphRenderer {

    const float LINE_WIDTH_SCALE = 0.07f;
    const float LINE_LENGTH_SCALE = 1f;

    /* multiple edges can map onto same path */
    public Dictionary<Edge, RectRenderer> edgeRendererDict;

    public void RenderGraph(GraphMatrix mat) {
        if (edgeRendererDict != null) {
            foreach (var pair in mat.edgeToPath) {
                edgeRendererDict[pair.Key].property = RectPropertyFromPath(pair.Value);
            }

            return;
        }
        edgeRendererDict = new Dictionary<Edge, RectRenderer>();
        foreach (var pair in mat.edgeToPath) {
            // draw edge
            var edge = pair.Key;
            var path = pair.Value;

            var rectRend = ShapeGOFactory.InstantiateRect(RectPropertyFromPath(path));

            edgeRendererDict.Add(edge, rectRend);
        }
    }

    public void RerenderPath(Path path) {
        RectRenderer rend;
        if (edgeRendererDict.TryGetValue(path.edge, out rend)) {
            rend.property = RectPropertyFromPath(path);
        }
    }

    RectProperty RectPropertyFromPath(Path path) {
        var edge = path.edge;
        Vector2 center = edge.p1.ToVector() * LINE_LENGTH_SCALE;
        float length = LINE_LENGTH_SCALE; // only connected to adjacent vertices
        //float width = LINE_WIDTH_SCALE / path.length;
        float width = LINE_WIDTH_SCALE * path.length;
        width = length*0.8f;
        length = length * 0.8f;
        float angle = edge.orientation == Orientation.Vertical ? 90 : 0;

        var color = Color.white;
        switch (path.landmarkType) {
            case LandmarkType.Cave:
                color = Color.cyan;
                break;
            case LandmarkType.Hill:
                color = Color.magenta;
                break;
            case LandmarkType.Library:
                color = Color.green;
                break;
            case LandmarkType.Residence:
                color = Color.yellow;
                break;
            default:
                color = Color.white;
                break;
        }

        switch (path.visibility) {
            case Visibility.Hidden:
                color.a = 0;
                break;
            case Visibility.Grayed:
                color.a = 0.03f;
                break;
            case Visibility.Revealed:
                if (Random.value < 0.7f) {
                    color.a = 0;
                } else {
                    color.a = 1;
                }
                break;
        }

        return (new RectProperty(
                    center: center, height: length, width: width, angle: angle, color: color
        ));
    }
}
}

