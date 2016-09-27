using Nine;

namespace NetworkGame {

public enum ArtifactType {
    Gem, Cup, Arrow
}

public class Artifact {
    public Coord position;
    public ArtifactType type;

    public Artifact(Coord position, ArtifactType type) {
        this.position = position;
        this.type = type;
    }
}

}
