using Nine;

namespace NetworkGame {
    public class Gem {
    }

    public class Path {
        public Edge edge;
        public float length;
        public UnitType allowedUnitType;
        public LandmarkType landmarkType;

        public Path(Edge edge, float length, UnitType utype = UnitType.Unit, LandmarkType ltype = LandmarkType.None) {
            this.edge = edge;
            this.length = length;
            this.allowedUnitType = utype;
            this.landmarkType = ltype;
        }

		public override int GetHashCode() {
		  return edge.GetHashCode();
		}
    }

    public enum LandmarkType {
        None, Residence, Library, Hill, Cave
    }

    public enum UnitType {
        Unit, Player, Enemy
    }

}
