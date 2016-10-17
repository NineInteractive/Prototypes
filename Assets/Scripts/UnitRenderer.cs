using UnityEngine;
using System.Collections;

namespace NetworkGame {


public class UnitRenderer : MonoBehaviour {

    public Unit unit;

    RectRenderer shapeRenderer;

    void Start() {
        var color = Color.green;
        var width = 0.2f;
        var height = 0.2f;
        var angle = 0f;
        /*
        if (unit is Player) {
            //color = Color.green;
            height = 0.3f;
            //angle = 22.5f;
        }
        */

        shapeRenderer = ShapeGOFactory.InstantiateRect(new RectProperty(width: width, height: height, color: color, angle: angle, layer: -2));
    }

    void Update() {
        if (unit == null) return;

        shapeRenderer.property.center = unit.position * (1.5f);
        //return new Vector2(c.x * (tileScale + distBetweenTiles), c.y * (tileScale + distBetweenTiles));
    }
}

}
