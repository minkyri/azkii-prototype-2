using System.IO;
using System.Reflection;

[Flags]
public enum TypeFlags{

    None = 0,
    Player = 1 << 0,
    Direction = 1 << 1,
    Container = 1 << 2,
    Visible = 1 << 3,
    CanTake = 1 << 4

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
                int holder = -1;
                string[] objectAdjectives;
                string[] objectSynonyms;
                TypeFlags flags = TypeFlags.None;
                Action subroutine;
                int[] travelTable = new int[10];

                name = lines[i][1];
                description = lines[i][2];
                int.TryParse(lines[i][3], out holder);
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

    #region Verb Subroutines

        public void VGo(){

            string direction = objects[Parser.directObjectID].name;

            switch(direction){

                case "north":
                    VGoNorth();
                    break;
                case "northeast":
                    VGoNorthEast();
                    break;
                case "east":
                    VGoEast();
                    break;
                case "southeast":
                    VGoSouthEast();
                    break;
                case "south":
                    VGoSouth();
                    break;
                case "southwest":
                    VGoSouthWest();
                    break;
                case "west":
                    VGoWest();
                    break;
                case "northwest":
                    VGoNorthWest();
                    break;
                case "up":
                    VGoUp();
                    break;
                case "down":
                    VGoDown();
                    break;
                default:
                    GameF.Print("You can't go that way.");
                    break;

            }

        }
        public void VGoNorth(){

            MovePlayer(0);

        }
        public void VGoNorthEast(){

            MovePlayer(1);

        }
        public void VGoEast(){

            MovePlayer(2);

        }
        public void VGoSouthEast(){

            MovePlayer(3);

        }
        public void VGoSouth(){

            MovePlayer(4);

        }
        public void VGoSouthWest(){

            MovePlayer(5);

        }
        public void VGoWest(){

            MovePlayer(6);

        }
        public void VGoNorthWest(){

            MovePlayer(7);

        }
        public void VGoUp(){

            MovePlayer(8);

        }
        public void VGoDown(){

            MovePlayer(9);

        }
        public void VInventory(){

            int[] heldObjects = GameF.GetHeldObjects(GameF.SearchForObject("player"));
            if(heldObjects.Length == 0){

                GameF.Print("Your inventory is empty.");

            }
            else{

                GameF.Print("Your inventory contains:");
                foreach(int i in heldObjects){

                    GameF.Print(objects[i].description + ".");

                }

            }
            
        }
        public void VTake(){

            string preposition = "";

            if(Parser.preposition1ID != -1){

                preposition = prepositions[Parser.preposition1ID].word;

            }
            if(preposition == "up" || preposition == ""){

                PlaceObject(Parser.directObjectID, GameF.SearchForObject("player"));
                GameF.Print("Taken.");

            }
            else if(preposition == "all"){

                int[] surroundingObjects = GameF.GetHeldObjects(objects[GameF.SearchForObject("player")].holderID, TypeFlags.CanTake);

                for(int i = 0; i < surroundingObjects.Length; i++){

                    PlaceObject(surroundingObjects[i], GameF.SearchForObject("player"));
                    GameF.Print("Taken.");

                }

            }

        }
        public void VLook(){

            string description = objects[objects[GameF.SearchForObject("player")].holderID].description;
            //description = char.ToUpper(description[0]) + description.Substring(1);
            GameF.Print(description);
            //Display surrounding objects

        }
        public void VLookAround(){

            int directObjectIndex = Parser.directObjectID;
            int playerIndex = GameF.SearchForObject("player");
            int currentLocationIndex = objects[playerIndex].holderID;

            int[] surroundingObjects = GameF.GetHeldObjects(currentLocationIndex);
            if((surroundingObjects.Contains(directObjectIndex) || directObjectIndex == currentLocationIndex) && 
                objects[directObjectIndex].flags.HasFlag(objects[Parser.syntaxID].flags)){

                VLook();

            }
            else{

                GameF.Print("You can't see any " + Parser.directObjectInput + " here.");

            }

        }

    #endregion
    #region Object Subroutines
        public void PlayerF(){

            if(Parser.GetWSPairIndex("look", verbs) == Parser.verbID){

                GameF.Print("That's difficult unless your eyes are prehensible.");

            }

        }

    #endregion
    #region Other
        private int[] GetPossibleDirections(){

            Object[] objects = Game.GetInstance().data.objects;
            return objects[objects[GameF.SearchForObject("player")].holderID].travelTable;

        }
        private void MovePlayer(int direction){

            int[] travelTable = GetPossibleDirections();
            if(travelTable[direction] == -1){

                GameF.Print("You can't go that way.");
                return;

            }
            else{

                objects[GameF.SearchForObject("player")].holderID = travelTable[direction];

            }
            VLook();

        } 
        private void PlaceObject(int obj, int holder){

            objects[obj].holderID = holder;

        }

    #endregion

}