using UnityEngine;

namespace Nine {
    public enum Direction {
        None, Up, Right, Down, Left
    }

    public static class DirectionUtil {
        public static Direction FromInput() {
            if (Input.GetMouseButtonDown(0)) {
                var mp = Input.mousePosition;
                var dv = mp - new Vector3(320, 390);
                if (Mathf.Abs(dv.x) > Mathf.Abs(dv.y)) {
                    if (dv.x > 0) {
                        return Direction.Right;
                    } else {
                        return Direction.Left;
                    }
                } else {
                    if (dv.y > 0) {
                        return Direction.Up;
                    } else {
                        return Direction.Down;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                return Direction.Up;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                return Direction.Down;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                return Direction.Left;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                return Direction.Right;
            }
            return Direction.None;
        }
    }

    public enum Orientation {
        Vertical, Horizontal
    }
}
