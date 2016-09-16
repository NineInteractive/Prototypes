using UnityEngine;
using System.Collections;

namespace NetworkGame {


public class UnitRenderer : MonoBehaviour {

    public Unit unit;

    RectRenderer shapeRenderer;

    void Start() {
        var color = Color.blue;
        if (unit is Player) {
            color = Color.red;
        }

        shapeRenderer = ShapeGOFactory.InstantiateRect(new RectProperty(width: 0.2f, height: 0.2f, color: color, layer: -1));
    }

    void Update() {
        if (unit == null) return;

        shapeRenderer.property.center = unit.position;
    }
}

}
