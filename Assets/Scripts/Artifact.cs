using Nine;

namespace NetworkGame {

public enum ArtifactType {
    Gem, Cup, Bow, Mirror, PocketKnife, Arrow
}

public class Artifact {
    public Coord position;
    public ArtifactType type;
    // if false, the artifact isn't drawn on the screen
    // used for cases where you can pick up an artifact at a landmark
    public bool rendered = true;

    public Artifact(Coord position, ArtifactType type, bool rendered = true) {
        this.position = position;
        this.type = type;
        this.rendered = rendered;
    }
}

}
