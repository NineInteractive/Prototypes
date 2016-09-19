using Nine;

namespace NetworkGame {
    public class Gem {
    }

    public class Path {
        public Edge edge;
        public float length;
        public UnitType allowedUnitType;

        public Path(Edge edge, float length, UnitType utype = UnitType.Unit) {
            this.edge = edge;
            this.length = length;
            this.allowedUnitType = utype;
        }
    }

    public enum UnitType {
        Unit, Player, Enemy
    }

}
