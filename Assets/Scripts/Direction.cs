using UnityEngine;

namespace Common {
    public enum Direction {
        None, Up, Right, Down, Left
    }

    public static class DirectionUtil {
        public static Direction FromInput() {
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
