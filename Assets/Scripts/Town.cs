using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nine;

namespace NetworkGame {

public class Town {
    /***** PUBLIC: VARIABLES *****/
    /** Landmarks **/
    public List<Edge> residence = Edge.EdgesBetweenCoords(new Coord(1, 1), new Coord(3, 3));
    public List<Edge> library = Edge.EdgesBetweenCoords(new Coord(5, 5), new Coord(7, 6));
    public List<Edge> hill = Edge.EdgesBetweenCoords(new Coord(4, 7), new Coord(5, 8));
    public List<Edge> cave = Edge.EdgesBetweenCoords(new Coord(1, 6), new Coord(2, 7));


    /** Other game states **/
    public int gemsCollected = 0;
    public int datesLeft = 10;

    /***** PRIVATE: VARIABLES *****/
    HashSet<Coord> _safeZone;


    /***** PUBLIC: STATIC METHODS *****/
    public static HashSet<Coord> CoordsForLandmark(List<Edge> landmark) {
        var set = new HashSet<Coord>();
        foreach (var edge in landmark) {
            set.Add(edge.p1);
            set.Add(edge.p2);
        }
        return set;
    }


    /***** PUBLIC: METHODS *****/
    public HashSet<Coord> safeZone {
        get {
            if (_safeZone == null) {
                _safeZone = new HashSet<Coord>();
                foreach (var edge in residence.Concat(library).Concat(hill)) {
                    _safeZone.Add(edge.p1);
                    _safeZone.Add(edge.p2);
                }
            }
            return _safeZone;
        }
    }

    public HashSet<Coord> CreateBaseOccupied() {
        var occupied = new HashSet<Coord>();
        foreach (var edge in residence.Concat(library).Concat(hill).Concat(cave)) {
            occupied.Add(edge.p1);
            occupied.Add(edge.p2);
        }
        return occupied;
    }

    public void ApplyToGraph(GraphMatrix graph) {
        _ApplyToGraph(graph, residence, LandmarkType.Residence, UnitType.Player);
        _ApplyToGraph(graph, library, LandmarkType.Library, UnitType.Player);
        _ApplyToGraph(graph, hill, LandmarkType.Hill, UnitType.Player);
        _ApplyToGraph(graph, cave, LandmarkType.Cave, UnitType.Enemy);
    }


    /***** PRIVATE: METHODS *****/
    void _ApplyToGraph(GraphMatrix graph, List<Edge> edges, LandmarkType ltype, UnitType allowed) {
        foreach (var e in edges) {
            var p = graph.GetPath(e);
            p.landmarkType = ltype;
            p.allowedUnitType = allowed;
        }
    }
}

}
