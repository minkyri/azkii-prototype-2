[Flags]
public enum TypeFlags{

    None = 0,
    Weapon = 1 << 0,

}
public class GameData{

    public string[] articles;
    public string[] conjunctions;
    public WordSynonymPair[] prepositions;
    public WordSynonymPair[] verbs;
    public string[] knownWords;
    public Object[] objects;
    public int[,] objectWordTable;
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
        prepositions = new WordSynonymPair[]{

            new WordSynonymPair("under", 0),
            new WordSynonymPair("below", 0),
            new WordSynonymPair("underneath", 0),
            new WordSynonymPair("in", 3),
            new WordSynonymPair("through", 3),
            new WordSynonymPair("using", 5),
            new WordSynonymPair("with", 5),
            new WordSynonymPair("at", 7)

        };
        verbs = new WordSynonymPair[]{

            new WordSynonymPair("look", 0),
            new WordSynonymPair("examine", 0),
            new WordSynonymPair("throw", 2),
            new WordSynonymPair("stare", 0),
            new WordSynonymPair("launch", 2),
            new WordSynonymPair("destroy", 5),
            new WordSynonymPair("demolish", 5),
            new WordSynonymPair("kill", 7),
            new WordSynonymPair("fight", 7),
            new WordSynonymPair("kill", 7),

        };
        knownWords = new string[]{

            "old",
            "rusty",
            "iron",
            "sword",
            "box"

        };
        objects = new Object[]{

            new Object(

                "offscreen",
                "",
                0,
                TypeFlags.None,
                GameF.isolated,
                null

            ),
            new Object(

                "rooms",
                "",
                0,
                TypeFlags.None,
                GameF.isolated,
                null

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

                },
                null

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


                },
                null

            ),
            new Object(

                "sword",
                "rusty sword",
                3,
                TypeFlags.Weapon,
                GameF.isolated,
                null

            ),
            new Object(

                "box",
                "iron box",
                3,
                TypeFlags.None,
                GameF.isolated,
                null

            )

        };
        objectWordTable = new int[,]{

            {4, 0},
            {4, 2},
            {4, 3},
            {5, 0},
            {5, 1},
            {5, 2},
            {5, 4}

        };
        syntaxes = new Syntax[]{

            new Syntax(

                0,
                -1,
                TypeFlags.None,
                -1,
                TypeFlags.None,
                new Action[]{

                    Look

                }

            )

        };

    }

    #region Verb Subroutines

        public void Look(){

            GameF.Print("You are looking around");

        }

    #endregion
    #region Object Subroutines



    #endregion

}




