using UnityEngine;
using System.Collections.Generic;

namespace NetworkGame {
    public class DialogueSystem {
        public static string[] DialogueForStage(int stage) {
            var d = new List<string>();

            switch (stage) {
                case 0:
                    break;
                case 1:
                    d.Add("Thankfully, Scheherazade wakes up.");
                    break;
                case 2:
                    d.Add("She begins the story of a young opal collector who yearned to amass enough township to free the game.");
                    break;

            }

            return d.ToArray();
        }
    }
}
