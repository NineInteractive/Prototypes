using UnityEngine;
using System.Collections.Generic;

namespace NetworkGame {
    public class DialogueSystem {
        static string[] DELIMITER = new string[]{"\n\n"};
        public static string[] DialogueForStage(int stage) {
            var dialogue = Resources.Load<TextAsset>("dialogue");
            string[] stageDialogue = dialogue.text.Split(DELIMITER, System.StringSplitOptions.None);

            if (stageDialogue.Length > stage) {
                return stageDialogue[stage].Split('\n');
            }
            return new string[]{};
        }
    }
}
