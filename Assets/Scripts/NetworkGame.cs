using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;

namespace NetworkGame {

public class NetworkGame : MonoBehaviour {

    const int WIDTH = 10;
    const int HEIGHT = 10;

    public Text renderer;

    int num_enemies = 1;

    int turns;

    Grid grid;

    Player player;
    List<Enemy> enemies;
    Coord gemPosition;

	// Use this for initialization
	void Start () {
        StartCoroutine(Play());
	}

	// Update is called once per frame
	void Update () {
	}

    IEnumerator Play() {
        // setup
        // turn
        // check death
        // print
        while (true) {
            Setup();
            do {
                //Debug.Log(">    Turn " + turns);
                RenderBoard();
                yield return StartCoroutine(PlayTurn());
                turns++;
                yield return null;
            } while (!(PlayerIsDead() || WonLevel()));
            RenderBoard();
            ShowResult();
            if (WonLevel()) {
                IncreaseDifficulty();
            } else {
                ResetDifficulty();
            }
        }
    }

    /***** SETUP *****/

    void Setup() {
        grid = new Grid(WIDTH, HEIGHT);

        gemPosition = Coord.RandomCoord(WIDTH, HEIGHT);

        player = new Player(0, 0); // init position?
        enemies = new List<Enemy>();
        for (int i=0; i<num_enemies; i++) {
            enemies.Add(new Enemy(Random.Range(1, WIDTH-1), Random.Range(1, HEIGHT-1),
                                   /*Random.Range(1.05f, 1.48f)));*/
                                   1));
        }
    }

    void IncreaseDifficulty() {
        num_enemies++;
    }

    void ResetDifficulty() {
        num_enemies = 1;
    }

    /***** PLAY LOGIC *****/

    IEnumerator PlayTurn() {
        while (DirectionUtil.FromInput() == Direction.None) {
            yield return null;
        }
        var dir = DirectionUtil.FromInput();
        player.Move(dir);
        foreach (var e in enemies) {
            e.Chase(player.position);
        }
        RemoveOverlappedEnemies();
    }

    void RemoveOverlappedEnemies() {
        var enemyPos = new Dictionary<Coord, List<Enemy>>();
        foreach (var enemy in enemies) {
            if (!enemyPos.ContainsKey(enemy.position)) {
                enemyPos[enemy.position] = new List<Enemy>();
            }
            enemyPos[enemy.position].Add(enemy);
        }

        foreach (var unitsOnSameCoord in enemyPos.Values) {
            if (unitsOnSameCoord.Count > 1) {
                foreach (var e in unitsOnSameCoord) {
                    enemies.Remove(e);
                }
            }
        }
    }

    /***** GAME STATUS *****/

    bool PlayerIsDead() {
        foreach (Enemy e in enemies) {
            if (e != null && e.position == player.position) {
                return true;
            }
        }
        return false;
    }

    bool WonLevel() {
        if (!PlayerIsDead() && (player.position == gemPosition)) {
            return true;
        }
        return false;
    }

    /***** RENDERING *****/

    void RenderBoard() {
        string output = "";
        for (int y = HEIGHT-1; y>=0; y--) {
            for (int x = 0; x<WIDTH; x++) {
                var here = new Coord(x, y);
                var c = CharForPosition(here);
                output += c;
            }
            output += '\n';
        }
        renderer.text = output;
    }

    char CharForPosition(Coord pos) {
        if (player.position == pos) {
            if (PlayerIsDead()) {
                return 'X';
            } else {
                return '@';
            }
        }

        foreach (var e in enemies) {
            if (e != null && e.position == pos) {
                return 'O';
            }
        }

        if (pos == gemPosition) {
            return '♢';
        }
        return '+';
    }

    void ShowResult() {
        Debug.Log("Survived for " + turns + " turns");
    }

}

public class Network {
    public void Blah () {
        // coord networked
        // draw network
        // take turns
    }
}

public struct Coord : System.IEquatable<Coord> {
    public int x;
    public int y;

    public Coord(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static Coord RandomCoord(int maxX, int maxY) {
        return new Coord(
                Random.Range(0, maxX),
                Random.Range(0, maxY));
    }

    public override int GetHashCode() {
        return 1000000*x + y;
    }

    public bool Equals(Coord other) {
        return x == other.x && y == other.y;
    }

	public static bool operator ==(Coord c1, Coord c2) {
		return c1.Equals(c2);
	}

	public static bool operator !=(Coord c1, Coord c2) {
	   return !c1.Equals(c2);
	}

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }
}

public class Unit {
    public Coord position;

    public Unit(int x, int y) {
        position = new Coord(x, y);
    }

    public Unit(Coord c) {
        position = c;
    }

    public void Move(Direction dir) {
        switch (dir) {
            case Direction.Up:
                position.y += 1;
                break;
            case Direction.Right:
                position.x += 1;
                break;
            case Direction.Down:
                position.y -= 1;
                break;
            case Direction.Left:
                position.x -= 1;
                break;
            default:
                break;
        }
    }
}

public class Player : Unit {
    public Player(int x, int y): base(x, y) {}
    public Player(Coord c): base(c) {}
}

public class Enemy : Unit {
    public float speed;

    float curMovement;

    public Enemy(int x, int y, float speed): base(x, y) {
        this.speed = speed;
        Debug.Log("Speed = " + speed);
    }

    public Enemy(Coord c, float speed): base(c) {
        this.speed = speed;
        Debug.Log("Speed = " + speed);
    }

    public void Chase(Coord target) {
        curMovement += speed;

        if (curMovement >= 1) {
            curMovement -= 1;
            if (target.x > position.x) {
                position.x++;
                return;
            }
            if (target.x < position.x) {
                position.x--;
                return;
            }
            if (target.y > position.y) {
                position.y++;
                return;
            }
            if (target.y < position.y) {
                position.y--;
                return;
            }
        }
    }
}

public class Grid {

    public int width;
    public int height;

    Coord[,] coords;

    public Grid(int width, int height) {
        this.width = width;
        this.height = height;

        coords = new Coord[width, height];

        for (int x=0; x<width; x++) {
            for (int y=0; y<height; y++) {
                coords[x, y] = new Coord(x, y);
            }
        }
    }
}
}
