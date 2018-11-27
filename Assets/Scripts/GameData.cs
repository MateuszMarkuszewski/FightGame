using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameData{

    public class Key
    {
        public KeyCode? key;
    }

    public class Controls
    {
        public Key attack = new Key();
        public Key up = new Key();
        public Key down = new Key();
        public Key right = new Key();
        public Key left = new Key();
        public Key pickUp = new Key();
    };

    public static bool? ai;
    public static float? sizeMap;
    public static bool secondClientConnected = false;
    public static Controls p1 = new Controls();
    public static Controls p2 = new Controls();
}
