using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class GraphMatrix {
    public const float NO_CONNECTION = -1;

    public readonly Dictionary<Edge, Path> edgeToPath = new Dictionary<Edge, Path>();

    public void AddPath(Path path) {
        edgeToPath[path.edge] = path;
        edgeToPath[path.edge.Reverse()] = path;
    }

    public float GetLength(Edge edge) {
        Path p;
        if (!edgeToPath.TryGetValue(edge, out p)) {
            Debug.LogError("Edge not found: " + edge);
            return 0;
        }
        return p.length;
    }

    public Path GetPath(Edge edge) {
        Path p;

        if (!edgeToPath.TryGetValue(edge, out p)) {
            //Debug.Log("Edge not found: " + edge);
        }

        return p;
    }

    public Path GetPath(int x1, int y1, int x2, int y2) {
        return GetPath(new Edge(x1, y1, x2, y2));
    }

    public Path[] GetAdjacentPaths(Coord coord) {
        var paths = new List<Path>();
        foreach (var edge in coord.AdjacentEdges()) {
            var p = GetPath(edge);
            if (p != null) paths.Add(p);
        }

        return paths.ToArray();
    }

    public int Count {
        get {
            return edgeToPath.Count;
        }
    }
}

}
