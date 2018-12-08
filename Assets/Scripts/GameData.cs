using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData{

    //klawisz obudowany w klasę aby w słowniku(skrypt OptionsMenu) były przechowywane referencje
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

    //null oznacza rozgrywke sieciową, true- przeciwko botowi, false- lokalnie 2 graaczy
    public static bool? ai;
    public static float? sizeMap;
    //gra sieciowa jest wstrzymana dopóki drugi gracz się nie połączy 
    public static bool secondClientConnected = false;
    //przechowują klawisze dla graczy, podczas rozgrywki lokalnej p1-gracz 1, p2-gracz 2
    //podczas sieciowej obaj kożystają z lokalnej wersji p1
    public static Controls p1 = new Controls();
    public static Controls p2 = new Controls();
}
