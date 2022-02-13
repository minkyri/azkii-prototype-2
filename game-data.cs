[Flags]
public enum TypeFlags{

    None = 0,
    Weapon = 1 << 0,

}
public class GameData{

    public string[] articles;
    public string[] conjunctions;
    public Dictionary<string, int> prepositions;
    public Dictionary<string, int> verbs;
    public string[] knownWords;
    public Object[] objects;
    public int[,] wordObjectDictionary;
    public Syntax[] syntaxes;

    public GameData(){

        articles = new string[]{

            "a",
            "an",
            "the"

        };
        conjunctions = new string[]{

            "and",
            "then"

        };
        prepositions = new Dictionary<string, int>{

            {"under", 0},
            {"below", 0},
            {"underneath", 0},
            {"in", 3},
            {"through", 3},
            {"using", 5},
            {"with", 5},
            {"at", 7}

        };
        verbs = new Dictionary<string, int>{

            {"look", 0},
            {"examine", 0},
            {"throw", 2},
            {"stare", 0},
            {"launch", 2},
            {"destroy", 5},
            {"demolish", 5},
            {"kill", 7},
            {"fight", 7}

        };
        knownWords = new string[]{

            "old",
            "rusty",
            "iron",
            "sword"

        };
        objects = new Object[]{

            new Object(

                "offscreen",
                "",
                0,
                TypeFlags.None,
                GameF.isolated

            ),
            new Object(

                "rooms",
                "",
                0,
                TypeFlags.None,
                GameF.isolated

            ),
            new Object(

                "kitchen",
                "Kitchen",
                1,
                TypeFlags.None,
                new int[]{

                    3,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1

                }

            ),
            new Object(

                "field",
                "grassy field",
                1,
                TypeFlags.None,
                new int[]{

                    -1,
                    -1,
                    -1,
                    -1,
                    2,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1


                }

            ),
            new Object(

                "sword",
                "silver sword",
                3,
                TypeFlags.Weapon,
                GameF.isolated

            )

        };
        wordObjectDictionary = new int[,]{

            {4, 0},
            {4, 1},
            {4, 2},
            {4, 3}

        };
        syntaxes = new Syntax[]{

            new Syntax(

                0,
                -1,
                TypeFlags.None,
                -1,
                TypeFlags.None,
                new Action<int>[]{

                    Look

                }

            )

        };

    }

    #region Verb Subroutines

        public void Look(int objectID){

            //Call object's subroutine and give it the verb, maybe have verb as global variable?
            GameF.Print("You are looking at object " + objectID.ToString());

        }

    #endregion

}




