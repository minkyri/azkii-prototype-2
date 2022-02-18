using System.IO;
using System.Reflection;

[Flags]
public enum TypeFlags{

    None = 0,
    Weapon = 1 << 0,
    Container = 1 << 1,
    Tangible = 1 << 2,
    Actor = 1 << 3

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

        LoadGame();

    }

    private void LoadGame(){

        string loadFolderLocation = @GameF.Input("Enter folder to load game from.");

        string[] filesToCheckFor = new string[]{

            "articles",
            "conjunctions",
            "objects",
            "prepositions",
            "syntax",
            "verbs"

        };
        string fileType = ".csv";

        foreach(string fileName in filesToCheckFor){

            string filePath = loadFolderLocation + "/" + fileName + fileType;
            if(!File.Exists(filePath)){

                GameF.Print("Missing file: " + fileName + fileType);
                LoadGame();
                return;

            }
            else if(GameF.IsFileLocked(new FileInfo(filePath))){

                GameF.Print(fileName + fileType + " is open, please close it before importing a game.");
                LoadGame();
                return;

            }

        }

        string[][] lines;

        #region Read Articles

            lines = GameF.ReadCSV(loadFolderLocation + "/articles" + fileType);
            List<string> articleList = new List<string>{};
            for(int i = 1; i < lines.Length; i++){

                articleList.Add(lines[i][0]);

            }
            articles = articleList.ToArray();

        #endregion
        #region Read Conjunctions

            lines = GameF.ReadCSV(loadFolderLocation + "/conjunctions" + fileType);
            List<string> conjunctionList = new List<string>{};
            for(int i = 1; i < lines.Length; i++){

                conjunctionList.Add(lines[i][0]);

            }
            conjunctions = conjunctionList.ToArray();
        
        #endregion
        #region Read Prepositions

            lines = GameF.ReadCSV(loadFolderLocation + "/prepositions" + fileType);
            List<WordSynonymPair> prepositionList = new List<WordSynonymPair>{};
            int prepositionIndexCounter = 0;

            for(int i = 1; i < lines.Length; i++){

                int currentPrepositionIndex = prepositionIndexCounter;
                prepositionList.Add(new WordSynonymPair(lines[i][0], currentPrepositionIndex));
                prepositionIndexCounter ++;

                if(lines[i].Length > 1 && lines[i][1] != ""){

                    string[] synonyms = lines[i][1].Split(',');

                    for(int b = 0; b < synonyms.Length; b++){

                        prepositionList.Add(new WordSynonymPair(synonyms[b], currentPrepositionIndex));
                        prepositionIndexCounter ++;

                    }

                }
                
            }
            prepositions = prepositionList.ToArray();

        #endregion
        #region Read Verbs

            lines = GameF.ReadCSV(loadFolderLocation + "/verbs" + fileType);
            List<WordSynonymPair> verbList = new List<WordSynonymPair>{};
            int verbIndexCounter = 0;

            for(int i = 1; i < lines.Length; i++){

                if(lines[i].Length > 1){

                    string[] synonyms = lines[i][1].Split(',');

                    int currentVerbIndex = verbIndexCounter;
                    verbList.Add(new WordSynonymPair(lines[i][0], currentVerbIndex));
                    verbIndexCounter ++;

                    for(int b = 0; b < synonyms.Length; b++){

                        verbList.Add(new WordSynonymPair(synonyms[b], currentVerbIndex));
                        verbIndexCounter ++;

                    }

                }
                
            }
            verbs = verbList.ToArray();            

        #endregion
        #region Read Objects

            lines = GameF.ReadCSV(loadFolderLocation + "/objects" + fileType);
            List<Object> objectList = new List<Object>{};
            List<string> knownWordsList = new List<string>{};
            int knownWordsIndexCounter = 0;
            List<int[]> objectWordList = new List<int[]>{};

            for(int i = 1; i < lines.Length; i++){

                string name;
                string description;
                int holder;
                string[] objectAdjectives;
                string[] objectSynonyms;
                TypeFlags flags = TypeFlags.None;
                Action subroutine;
                int[] travelTable = new int[10];

                name = lines[i][1];
                description = lines[i][2];
                holder = int.Parse(lines[i][3]);
                //el mong us
                objectAdjectives = lines[i][4].Split(',');
                objectSynonyms = lines[i][5].Split(',');
                
                if(name != ""){

                    knownWordsList.Add(name);
                    objectWordList.Add(new int[2]{i-1, knownWordsIndexCounter});
                    knownWordsIndexCounter++;

                }
                
                foreach(string adjective in objectAdjectives){

                    if(!knownWordsList.Contains(adjective) && adjective != ""){
                        
                        knownWordsList.Add(adjective);
                        objectWordList.Add(new int[2]{i-1, knownWordsIndexCounter});
                        knownWordsIndexCounter++;

                    }   

                }
                foreach(string synonym in objectSynonyms){

                    if(!knownWordsList.Contains(synonym) && synonym != ""){
                        
                        knownWordsList.Add(synonym);
                        objectWordList.Add(new int[2]{i-1, knownWordsIndexCounter});
                        knownWordsIndexCounter++;

                    }

                }

                for(int a = 0; a < travelTable.Length; a++){

                    string travel = lines[i][6 + a];
                    if(travel == "") travelTable[a] = -1;
                    else travelTable[a] = int.Parse(travel);

                }

                string[] enumStrings = lines[i][16].Split(',');
                foreach(string enumString in enumStrings){

                    if(enumString != ""){
                        
                        if(Enum.TryParse(typeof(TypeFlags), enumString, true, out object result)){

                            flags |= (TypeFlags)result;

                        }
                        else{

                            GameF.Print("The bit flag " + enumString + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/objects" + fileType + " does not exist.");
                            LoadGame();
                            return;

                        }

                    }

                }
                
                if(lines[i][17] != ""){

                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(lines[i][17]);
                    if(theMethod == null){

                        GameF.Print("The subroutine " + lines[i][17] + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/objects" + fileType + " does not exist.");
                        LoadGame();
                        return;

                    }
                    subroutine = (Action)Delegate.CreateDelegate(typeof(Action), this, theMethod);
                    //subroutine();
                    //GameF.Print(lines[i][17]);

                }
                else{

                    subroutine = null;

                }

                objectList.Add(new Object(

                    name,
                    description,
                    holder,
                    flags,
                    travelTable,
                    subroutine

                ));

            }
            
            objects = objectList.ToArray();
            knownWords = knownWordsList.ToArray();
            
            objectWordTable = new int[objectWordList.Count,2];
            for(int i = 0; i < objectWordList.Count; i++){

                objectWordTable[i,0] = objectWordList[i][0];
                objectWordTable[i,1] = objectWordList[i][1];

            }

        #endregion
        #region Read Syntax

            lines = GameF.ReadCSV(loadFolderLocation + "/syntax" + fileType);
            List<Syntax> syntaxList = new List<Syntax>{};

            for(int i = 1; i < lines.Length; i++){

                string verb;
                string preposition1;
                TypeFlags directObjectFlags = TypeFlags.None;
                string preposition2;
                TypeFlags indirectObjectFlags = TypeFlags.None;
                Action[] subroutines;

                verb = lines[i][0];
                preposition1 = lines[i][1];
                
                string[] enumStrings = lines[i][2].Split(',');
                foreach(string enumString in enumStrings){

                    if(enumString != ""){
                        
                        if(Enum.TryParse(typeof(TypeFlags), enumString, true, out object result)){

                            directObjectFlags |= (TypeFlags)result;

                        }
                        else{

                            GameF.Print("The bit flag " + enumString + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                            LoadGame();
                            return;

                        }

                    }

                }

                preposition2 = lines[i][3];

                enumStrings = lines[i][4].Split(',');
                foreach(string enumString in enumStrings){

                    if(enumString != ""){
                        
                        if(Enum.TryParse(typeof(TypeFlags), enumString, true, out object result)){

                            indirectObjectFlags |= (TypeFlags)result;

                        }
                        else{

                            GameF.Print("The bit flag " + enumString + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                            LoadGame();
                            return;

                        }

                    }

                }

                string[] actionStrings = lines[i][5].Split(',');
                List<Action> actionList = new List<Action>{};
                foreach(string actionString in actionStrings){

                    if(actionString != ""){

                        Type thisType = this.GetType();
                        MethodInfo theMethod = thisType.GetMethod(actionString);
                        if(theMethod == null){

                            GameF.Print("The subroutine " + actionString + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                            LoadGame();
                            return;

                        }
                        actionList.Add((Action)Delegate.CreateDelegate(typeof(Action), this, theMethod));

                    }
                    else{

                        actionList.Add(null);

                    }

                }
                subroutines = actionList.ToArray();
                
                int verbID = Parser.GetWSPairIndex(verb, verbs);
                int preposition1ID = Parser.GetWSPairIndex(preposition1, prepositions);
                int preposition2ID = Parser.GetWSPairIndex(preposition2, prepositions);

                //GameF.Print(verbID.ToString() + " " + preposition1ID.ToString() + " " + preposition2ID.ToString());

                if(verbID == -1 && verb != ""){

                    GameF.Print("The verb " + verb + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                    LoadGame();
                    return;

                }
                if(preposition1ID == -1 && preposition1 != ""){

                    GameF.Print("The preposition " + preposition1 + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                    LoadGame();
                    return;

                }
                if(preposition2ID == -1 && preposition2 != ""){

                    GameF.Print("The preposition " + preposition2 + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                    LoadGame();
                    return;

                }

                syntaxList.Add(new Syntax(

                    verbID,
                    preposition1ID,
                    directObjectFlags,
                    preposition2ID,
                    indirectObjectFlags,
                    subroutines

                ));

            }
            
            syntaxes = syntaxList.ToArray();
            //GameF.Print(syntaxes.Length.ToString());

        #endregion
        if(articles.Length == 0){

            GameF.Print("Game data does not specify any articles!");
            LoadGame();
            return;

        }
        else if(conjunctions.Length == 0){

            GameF.Print("Game data does not specify any conjunctions!");
            LoadGame();
            return;

        }
        else if(prepositions.Length == 0){

            GameF.Print("Game data does not specify any prepositions!");
            LoadGame();
            return;

        }
        else if(verbs.Length == 0){

            GameF.Print("Game data does not specify any verbs!");
            LoadGame();
            return;

        }
        else if(objects.Length == 0){

            GameF.Print("Game data does not specify any objects!");
            LoadGame();
            return;

        }
        else if(syntaxes.Length == 0){

            GameF.Print("Game data does not specify any syntax!");
            LoadGame();
            return;

        }

    }

    private void OldLoad(){

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
            new WordSynonymPair("fight", 7)

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

            GameF.Print("yello");

        }
        public void Fight(){

            GameF.Print("You are fighting the " + Game.GetInstance().data.objects[Parser.directObjectID].description + " " +
                Game.GetInstance().data.prepositions[Parser.preposition2ID].word +  " your " + Game.GetInstance().data.objects[Parser.indirectObjectID].description + ".");

        }

    #endregion
    #region Object Subroutines

        public void BlueKeyF(){

            if(Parser.GetWSPairIndex("look", verbs) == Parser.verbID){

                GameF.Print("You are looking at the blue key.");

            }

        }
        public void RedKeyF(){

            if(Parser.GetWSPairIndex("look", verbs) == Parser.verbID){

                GameF.Print("You are looking at the red key.");

            }

        }

    #endregion

}




