using UnityEngine;
using System.Collections;

namespace NetworkGame {


public class UnitRenderer : MonoBehaviour {

    public Unit unit;

    RectRenderer shapeRenderer;

    void Awake() {
        shapeRenderer = ShapeGOFactory.InstantiateRect(new RectProperty(width: 0.2f, height: 0.2f, color: Color.blue));
    }

    void Update() {
        if (unit == null) return;

        shapeRenderer.property.center = unit.position;
    }
}

}
