using System.Reflection;
using System.Runtime.Serialization;

public class Game{

    private static Game instance;
    public bool isFinished = false;
    public int textSpeed = 15;
    public GameData data;

    private Game(){}
    public static Game GetInstance()
    {

        //makes sure that only one instance of Game can ever exist (Singleton class)
        if (instance == null)
        {

            instance = new Game();
            instance.data = new GameData();
            instance.LoadGame();

        }
        return instance;

    }
    public bool CheckForSpecialActions(string input){

        switch(input){

            case "save":

                SaveGame();
                return true;

            case "restart":

                ResartGame();
                return true;

            case "exit":

                ExitGame();
                return true;

            case "load":

                LoadGame();
                return true;

            default:

                return false;

        }

    }
    public void LoadGame(){

        //lets the user choose between loading a game data from csv files, or loading it from an xml file

        string[] loadChoice = GameF.Input("Would you like to load from csv files, or load from a saved xml game?").ToLower().Split(' ');
        string loadFolderLocation;

        bool start = loadChoice.Contains("csv");
        bool load = loadChoice.Contains("save") || loadChoice.Contains("xml") || loadChoice.Contains("saved");

        if((start && load) || (!start && !load)){

            GameF.Print("You did not select a valid option.");
            LoadGame();
            return;

        }
        else if(load){

            string fileName = GameF.Input("Enter name of save game file.");
            loadFolderLocation = Directory.GetCurrentDirectory() + @"\saves\" + fileName + ".xml";
            LoadGameFromXML(loadFolderLocation);

            MethodInfo returnMethodInfo = instance.data.GetType().GetMethod("ReturnToGame");
            if(returnMethodInfo != null){

                Action returnMethod = (Action)Delegate.CreateDelegate(typeof(Action), instance.data, returnMethodInfo);
                returnMethod();

            }

        }
        else{

            loadFolderLocation = GameF.Input("Enter folder location containing data tables.");
            LoadGameFromCSV(loadFolderLocation);
            
            MethodInfo startMethodInfo = instance.data.GetType().GetMethod("StartGame");
            if(startMethodInfo != null){

                Action startMethod = (Action)Delegate.CreateDelegate(typeof(Action), instance.data, startMethodInfo);
                startMethod();

            }

        }

    }
    public void SaveGame(){

        //saves the game to the saves folder as an xml file, by serializing the GameData instance

        string fileName = GameF.Input("Enter name for save game.");

        try{

            string saveFolderLocation = Directory.GetCurrentDirectory() + @"\saves\" + fileName + ".xml";
            SaveGameToXML(saveFolderLocation);

        }
        catch(Exception){

            GameF.Print("There was an error saving.");
            SaveGame();

        }

        //checks to see if the game's data includes a method that specifies what happens upon returning to the game after saving
        MethodInfo returnMethodInfo = instance.data.GetType().GetMethod("ReturnToGame");
        if(returnMethodInfo != null){

            Action returnMethod = (Action)Delegate.CreateDelegate(typeof(Action), instance.data, returnMethodInfo);
            returnMethod();

        }

    }
    public void ResartGame(){

        //lets the player restart the game

        string[] loadChoice = GameF.Input("Would you like to restart?").ToLower().Split(' ');

        bool yes = loadChoice.Contains("y") || loadChoice.Contains("yes") || loadChoice.Contains("again");
        bool no = loadChoice.Contains("n") || loadChoice.Contains("no") || loadChoice.Contains("stop");

        if((yes && no) || (!yes && !no)){

            GameF.Print("You did not select a valid option.");
            ResartGame();
            return;

        }
        else if(no){

            return;

        }
        else{

            instance = null;
            GetInstance();

        }

    }
    public void ExitGame(){

        //lets the player exit the game

        string[] loadChoice = GameF.Input("Are you sure you want to exit?").ToLower().Split(' ');

        bool yes = loadChoice.Contains("y") || loadChoice.Contains("yes") || loadChoice.Contains("again");
        bool no = loadChoice.Contains("n") || loadChoice.Contains("no") || loadChoice.Contains("stop");

        if((yes && no) || (!yes && !no)){

            GameF.Print("You did not select a valid option.");
            ExitGame();
            return;

        }
        else if(no){

            MethodInfo returnMethodInfo = instance.data.GetType().GetMethod("ReturnToGame");
            if(returnMethodInfo != null){

                Action returnMethod = (Action)Delegate.CreateDelegate(typeof(Action), instance.data, returnMethodInfo);
                returnMethod();

            }
            return;

        }
        else{

            System.Environment.Exit(0);

        }

    }
    public void FinishGame(){

        ResartGame();
        ExitGame();

    }
    private void LoadGameFromCSV(string loadFolderLocation){

        //loads the game data properties from csv files

        Type dataClassType = data.GetType();

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

            //make sure there won't be any exceptions thrown when reading the files
            string filePath = loadFolderLocation + "/" + fileName + fileType;
            if(!File.Exists(filePath)){

                GameF.Print("Missing file: " + fileName + fileType);
                LoadGame();
                return;

            }
            else if(GameF.IsFileLocked(new FileInfo(filePath))){

                GameF.Print(fileName + fileType + " is open, please close it before importing a game.\n");
                LoadGame();
                return;

            }

        }

        string[][] lines;

        #region Read Articles

            lines = GameF.ReadCSV(loadFolderLocation + "/articles" + fileType);
            List<string> articleList = new List<string>{};
            for(int i = 1; i < lines.Length; i++){

                string article = Parser.CleanInput(lines[i][0]);
                articleList.Add(article);

            }
            data.articles = articleList.ToArray();

        #endregion
        #region Read Conjunctions

            lines = GameF.ReadCSV(loadFolderLocation + "/conjunctions" + fileType);
            List<string> conjunctionList = new List<string>{};
            for(int i = 1; i < lines.Length; i++){

                string conjunction = Parser.CleanInput(lines[i][0]);
                conjunctionList.Add(conjunction);

            }
            data.conjunctions = conjunctionList.ToArray();
        
        #endregion
        #region Read Prepositions

            lines = GameF.ReadCSV(loadFolderLocation + "/prepositions" + fileType);
            List<WordSynonymPair> prepositionList = new List<WordSynonymPair>{};
            int prepositionIndexCounter = 0;

            for(int i = 1; i < lines.Length; i++){

                int currentPrepositionIndex = prepositionIndexCounter;
                prepositionList.Add(new WordSynonymPair(Parser.CleanInput(lines[i][0]), currentPrepositionIndex));
                prepositionIndexCounter ++;

                if(lines[i].Length > 1 && lines[i][1] != ""){

                    string[] synonyms = lines[i][1].Split(',');

                    for(int b = 0; b < synonyms.Length; b++){

                        string synonym = Parser.CleanInput(synonyms[b]);
                        prepositionList.Add(new WordSynonymPair(synonym, currentPrepositionIndex));
                        prepositionIndexCounter ++;

                    }

                }
                
            }
            data.prepositions = prepositionList.ToArray();

        #endregion
        #region Read Verbs

            lines = GameF.ReadCSV(loadFolderLocation + "/verbs" + fileType);
            List<WordSynonymPair> verbList = new List<WordSynonymPair>{};
            int verbIndexCounter = 0;

            for(int i = 1; i < lines.Length; i++){

                if(lines[i].Length > 1){

                    string[] synonyms = lines[i][1].Split(',');

                    int currentVerbIndex = verbIndexCounter;
                    verbList.Add(new WordSynonymPair(Parser.CleanInput(lines[i][0]), currentVerbIndex));
                    verbIndexCounter ++;

                    for(int b = 0; b < synonyms.Length; b++){

                        string synonym = Parser.CleanInput(synonyms[b]);
                        verbList.Add(new WordSynonymPair(synonym, currentVerbIndex));
                        verbIndexCounter ++;

                    }

                }
                
            }
            data.verbs = verbList.ToArray();            

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
                Func<bool> subroutine;

                name = lines[i][3].Split(',')[0];
                description = lines[i][2];
                
                if(lines[i][1] == "")holder = -1;
                else if(!int.TryParse(lines[i][1], out holder)){

                    GameF.Print("The object holder \"" + lines[i][1] + "\" on line " + (i+1).ToString() + " of " + loadFolderLocation + "/objects" + fileType + " is not a valid object id.");
                    LoadGame();
                    return;

                }
                else if(holder < 0){

                    GameF.Print("The object holder \"" + lines[i][1] + "\" on line " + (i+1).ToString() + " of " + loadFolderLocation + "/objects" + fileType + " is not a valid object id.");
                    LoadGame();
                    return;


                }

                //el mong us
                objectAdjectives = lines[i][4].Split(',');
                objectSynonyms = lines[i][3].Split(',').Skip(1).ToArray();

                if(name != ""){

                    knownWordsList.Add(Parser.CleanInput(name));
                    objectWordList.Add(new int[2]{i-1, knownWordsIndexCounter});
                    knownWordsIndexCounter++;

                }
                
                foreach(string adjective in objectAdjectives){

                    if(adjective != ""){
                        
                        knownWordsList.Add(Parser.CleanInput(adjective));
                        objectWordList.Add(new int[2]{i-1, knownWordsIndexCounter});
                        knownWordsIndexCounter++;

                    }

                }
                foreach(string synonym in objectSynonyms){

                    if(synonym != ""){
                        
                        knownWordsList.Add(Parser.CleanInput(synonym));
                        objectWordList.Add(new int[2]{i-1, knownWordsIndexCounter});
                        knownWordsIndexCounter++;

                    }

                }

                // for(int a = 0; a < travelTable.Length; a++){

                //     string travel = lines[i][6 + a];
                //     if(travel == "") travelTable[a] = -1;
                //     else travelTable[a] = int.Parse(travel);

                // }

                // string[] enumStrings = lines[i][16].Split(',');
                // foreach(string enumString in enumStrings){

                //     if(enumString != ""){
                        
                //         if(Enum.TryParse(typeof(TypeFlags), enumString, true, out object result)){

                //             flags |= (TypeFlags)result;

                //         }
                //         else{

                //             GameF.Print("The bit flag " + enumString + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/objects" + fileType + " does not exist.");
                //             LoadGame();
                //             return;

                //         }

                //     }

                // }
                
                if(lines[i][5] != ""){

                    string methodName = Parser.CleanInput(lines[i][5]);
                    MethodInfo theMethod = dataClassType.GetMethod(methodName);
                    if(theMethod == null){

                        GameF.Print("The subroutine " + methodName + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/objects" + fileType + " does not exist.");
                        LoadGame();
                        return;

                    }
                    subroutine = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), data, theMethod);
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
                    subroutine

                ));

            }
            
            objectList.Add(new Object(

                "it",
                "",
                0,
                null

            ));
            knownWordsList.Add("it");
            objectWordList.Add(new int[]{objectList.Count-1,knownWordsList.Count-1});

            data.objects = objectList.ToArray();
            data.knownWords = knownWordsList.ToArray();
            
            // data.objectWordTable = new int[objectWordList.Count,2];
            // for(int i = 0; i < objectWordList.Count; i++){

            //     data.objectWordTable[i,0] = objectWordList[i][0];
            //     data.objectWordTable[i,1] = objectWordList[i][1];

            // }

            data.objectWordTable = new int[objectWordList.Count][];
            for(int i = 0; i < objectWordList.Count; i++){

                data.objectWordTable[i] = new int[2];
                data.objectWordTable[i][0] = objectWordList[i][0];
                data.objectWordTable[i][1] = objectWordList[i][1];

            }

        #endregion
        #region Read Syntax

            lines = GameF.ReadCSV(loadFolderLocation + "/syntax" + fileType);
            List<Syntax> syntaxList = new List<Syntax>{};

            for(int i = 1; i < lines.Length; i++){

                string verb;
                string preposition1;
                List<Func<int, bool, bool>> directObjectFlags = new List<Func<int, bool, bool>>{};
                string preposition2;
                List<Func<int, bool, bool>> indirectObjectFlags = new List<Func<int, bool, bool>>{};
                Func<bool> subroutine;

                verb = Parser.CleanInput(lines[i][0]);
                preposition1 = Parser.CleanInput(lines[i][1]);;
                
                string[] flagStrings = lines[i][2].Split(',');
                foreach(string flagString in flagStrings){

                    string cleanFlagString = Parser.CleanInput(flagString);

                    if(flagString != ""){
                        
                        MethodInfo flagMethod = dataClassType.GetMethod(cleanFlagString);
                        if(flagMethod == null){

                            GameF.Print("The flag subroutine \"" + cleanFlagString + "\" specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                            LoadGame();
                            return;

                        }
                        directObjectFlags.Add((Func<int, bool, bool>)Delegate.CreateDelegate(typeof(Func<int, bool, bool>), data, flagMethod));

                    }

                }

                preposition2 = Parser.CleanInput(lines[i][3]);

                flagStrings = lines[i][4].Split(',');
                foreach(string flagString in flagStrings){

                    string cleanFlagString = Parser.CleanInput(flagString);

                    if(flagString != ""){
                        
                        MethodInfo flagMethod = dataClassType.GetMethod(cleanFlagString);
                        if(flagMethod == null){

                            GameF.Print("The flag subroutine \"" + cleanFlagString + "\" specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                            LoadGame();
                            return;

                        }
                        indirectObjectFlags.Add((Func<int, bool, bool>)Delegate.CreateDelegate(typeof(Func<int, bool, bool>), data, flagMethod));

                    }

                }

                string actionString = Parser.CleanInput(lines[i][5]);               
                MethodInfo theMethod = dataClassType.GetMethod(actionString);
                if(theMethod == null){

                    GameF.Print("The subroutine \"" + actionString + "\" specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                    LoadGame();
                    return;

                }
                subroutine = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), data, theMethod);

                int verbID = GameF.GetWSPairIndex(verb, data.verbs);
                int preposition1ID = GameF.GetWSPairIndex(preposition1, data.prepositions);
                int preposition2ID = GameF.GetWSPairIndex(preposition2, data.prepositions);

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
                    directObjectFlags.ToArray(),
                    preposition2ID,
                    indirectObjectFlags.ToArray(),
                    subroutine

                ));

                // GameF.Print(verb + " : " + preposition1 + " : " + preposition2);
                // GameF.Print("   " + directObjectFlags.Count.ToString());
                // GameF.Print("   " + indirectObjectFlags.Count.ToString() + "\n");

            }
            
            data.syntaxes = syntaxList.ToArray();

        #endregion
        #region Read Classes

            //reflection is used here to try to find whether classes exist in game data

            string[] files = Directory.GetFiles(loadFolderLocation);
            Dictionary<int, List<ObjectClass>> objectClasses = new Dictionary<int, List<ObjectClass>>{};

            foreach(string file in files){

                string cutFileName = Path.GetFileNameWithoutExtension(file);
                string fileName = Path.GetFileName(file);
                if (cutFileName.Length > 2){

                    //take the string after the "c_" in the file name to be the name of the class
                    if(cutFileName.Substring(0, 2) == "c_" && Path.GetExtension(file) == ".csv"){

                        string classFileName = cutFileName.Substring(2);
                        string className = Parser.CleanInput(GameF.SnakeToPascal(classFileName)); 
                        Type classType = Type.GetType(typeof(GameData).FullName + "+" + className);
                        if(classType == null || !classType.IsSubclassOf(typeof(ObjectClass))){

                            GameF.Print("The class \"" + className + "\" specified in " + fileName + " does not exist.");
                            LoadGame();
                            return;

                        }                       

                        PropertyInfo[] propertyInfos = classType.GetProperties();
                        Dictionary<string, Type> propertyTypes = new Dictionary<string, Type>{};

                        foreach(PropertyInfo property in propertyInfos){

                            propertyTypes.Add(property.Name, property.PropertyType);

                        }

                        if(GameF.IsFileLocked(new FileInfo(file))){

                            GameF.Print(fileName + " is open, please close it before importing a game.");
                            LoadGame();
                            return;

                        }

                        lines = GameF.ReadCSV(file);

                        for(int i = 1; i < lines.Length; i++){
                            
                            string input = Parser.CleanInput(lines[i][0]);
                            object objectClass = Activator.CreateInstance(classType);
                            if(!int.TryParse(input, out int objectIndex) || objectIndex < 0 || objectIndex > (data.objects.Length-1)){

                                GameF.Print("The object index \"" + input + "\" specified on line " + (i+1).ToString() + " of " + fileName + " is not an existing object ID.");
                                LoadGame();
                                return;

                            }

                            for(int p = 1; p < lines[i].Length; p++){

                                string propertyName = Parser.CleanInput(lines[0][p]);
                                if(!propertyTypes.TryGetValue(propertyName, out Type propertyType)){

                                    GameF.Print("The property \"" + propertyName + "\" of the object type \"" + className + "\" specified on column " + (p+1).ToString() + " of " + fileName + " does not exist.");
                                    LoadGame();
                                    return;

                                }

                                object propertyValue;
                                if(propertyType.IsArray){
                                    
                                    string[] splitInput = lines[i][p].Split(',');
                                    Array values = Array.CreateInstance(propertyType.GetElementType(), splitInput.Length);
                                    
                                    for(int s = 0; s < splitInput.Length; s++){

                                        string propertyValueString = Parser.CleanInput(splitInput[s]);

                                        if(!GameF.TryCast(propertyValueString, propertyType.GetElementType(), out object value)){

                                            GameF.Print("The property value \"" + propertyValueString + "\" of the object type " + className + " specified on column " + (p+1).ToString() + ", line " + (i+1).ToString() + ", item " + (s+1).ToString() + ", of " + fileName + " is a " + propertyType.Name + ", which is an invalid property type.");
                                            LoadGame();
                                            return;

                                        }

                                        values.SetValue(value, s);

                                    }

                                    propertyValue = Convert.ChangeType(values, propertyType);

                                }
                                else if(propertyType.IsPrimitive){

                                    string propertyValueString = Parser.CleanInput(lines[i][p]);

                                    if(propertyType == typeof(bool)){

                                        propertyValueString = propertyValueString.ToLower();
                                        if(!bool.TryParse(propertyValueString, out bool b)){

                                            GameF.Print("The property value \"" + propertyValueString + "\" of the object type " + className + " specified on column " + (p+1).ToString() + ", line " + (i+1).ToString() + ", of " + fileName + " is a " + propertyType.Name + ", which is an invalid property type.");
                                            LoadGame();
                                            return;

                                        }

                                        propertyValue = b;

                                    }
                                    else if(propertyValueString == "" && propertyType == typeof(int)){

                                        propertyValue = -1;

                                    }
                                    else if(!GameF.TryCast(propertyValueString, propertyType, out propertyValue)){

                                        GameF.Print("The property value \"" + propertyValueString + "\" of the object type " + className + " specified on column " + (p+1).ToString() + ", line " + (i+1).ToString() + ", of " + fileName + " is a " + propertyType.Name + ", which is an invalid property type.");
                                        LoadGame();
                                        return;

                                    }

                                }
                                else{

                                    GameF.Print("The property \"" + Parser.CleanInput(lines[0][p]) + "\" of the object type " + className + " specified on column " + (p+1).ToString() + " of " + fileName + " is a " + propertyType.Name + ", which is an invalid property type.");
                                    LoadGame();
                                    return;

                                }

                                classType.GetProperty(propertyName).SetValue(objectClass, propertyValue);

                            }
                            
                            if(objectClasses.TryGetValue(objectIndex, out List<ObjectClass> classes)){

                                objectClasses[objectIndex].Add((ObjectClass)objectClass);

                            }
                            else{

                                objectClasses.Add(objectIndex, new List<ObjectClass>{(ObjectClass)objectClass});

                            }

                        }

                    }

                }

            }

            foreach(KeyValuePair<int, List<ObjectClass>> objectClass in objectClasses){

                data.objects[objectClass.Key].classes = objectClass.Value.ToArray();

            }

        #endregion   

        if(data.articles.Length == 0){

            GameF.Print("Game data does not specify any articles!");
            LoadGame();
            return;

        }
        else if(data.conjunctions.Length == 0){

            GameF.Print("Game data does not specify any conjunctions!");
            LoadGame();
            return;

        }
        else if(data.prepositions.Length == 0){

            GameF.Print("Game data does not specify any prepositions!");
            LoadGame();
            return;

        }
        else if(data.verbs.Length == 0){

            GameF.Print("Game data does not specify any verbs!");
            LoadGame();
            return;

        }
        else if(data.objects.Length == 0){

            GameF.Print("Game data does not specify any objects!");
            LoadGame();
            return;

        }
        else if(data.syntaxes.Length == 0){

            GameF.Print("Game data does not specify any syntax!");
            LoadGame();
            return;

        }

    }
    private void LoadGameFromXML(string loadFolderLocation){

        //loads an xml file from the specified location and assigns it to the game data instance

        data = GameF.DeserializeObject<GameData>(loadFolderLocation);
        Type dataClassType = data.GetType();

        foreach(KeyValuePair<int, string> kvp in data.objectSubroutineDictionary){

            MethodInfo theMethod = dataClassType.GetMethod(kvp.Value);
            if(theMethod == null){

                GameF.Print("The object subroutine \"" + kvp.Value + "\" specified in the loaded game file does not exist.");
                LoadGame();
                return;

            }
            data.objects[kvp.Key].subroutine = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), data, theMethod);

        }

        foreach(KeyValuePair<int, string> kvp in data.syntaxSubroutineDictionary){

            MethodInfo theMethod = dataClassType.GetMethod(kvp.Value);
            if(theMethod == null){

                GameF.Print("The syntax subroutine \"" + kvp.Value + "\" specified in the loaded game file does not exist.");
                LoadGame();
                return;

            }
            data.syntaxes[kvp.Key].subroutine = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), data, theMethod);

        }

        foreach(KeyValuePair<int, string[]> kvp in data.syntaxDirectObjectFlagDictionary){

            List<Func<int, bool, bool>> subroutines = new List<Func<int, bool, bool>>{};
            for(int i = 0; i < kvp.Value.Length; i++){

                MethodInfo theMethod = dataClassType.GetMethod(kvp.Value[i]);
                    if(theMethod == null){

                    GameF.Print("The direct object flag subroutine \"" + kvp.Value + "\" specified in the loaded game file does not exist.");
                    LoadGame();
                    return;

                }
                subroutines.Add((Func<int, bool, bool>)Delegate.CreateDelegate(typeof(Func<int, bool, bool>), data, theMethod));

            }
            data.syntaxes[kvp.Key].directObjectFlags = subroutines.ToArray();
            
        }

        foreach(KeyValuePair<int, string[]> kvp in data.syntaxIndirectObjectFlagDictionary){

            List<Func<int, bool, bool>> subroutines = new List<Func<int, bool, bool>>{};
            for(int i = 0; i < kvp.Value.Length; i++){

                MethodInfo theMethod = dataClassType.GetMethod(kvp.Value[i]);
                    if(theMethod == null){

                    GameF.Print("The indirect object flag subroutine \"" + kvp.Value + "\" specified in the loaded game file does not exist.");
                    LoadGame();
                    return;

                }
                subroutines.Add((Func<int, bool, bool>)Delegate.CreateDelegate(typeof(Func<int, bool, bool>), data, theMethod));

            }
            data.syntaxes[kvp.Key].indirectObjectFlags = subroutines.ToArray();
            
        }

    }
    private void SaveGameToXML(string saveFolderLocation){

        //takes the game data class instance and saves it to an xml file

        data.objectSubroutineDictionary = new Dictionary<int, string>{};
        for(int i = 0; i < data.objects.Length; i++){

            if(data.objects[i].subroutine != null){

                data.objectSubroutineDictionary.Add(i, data.objects[i].subroutine.Method.Name);

            }

        }

        data.syntaxSubroutineDictionary  = new Dictionary<int, string>{};
        for(int i = 0; i < data.syntaxes.Length; i++){

            if(data.syntaxes[i].subroutine != null){

                data.syntaxSubroutineDictionary.Add(i, data.syntaxes[i].subroutine.Method.Name);

            }

        }

        data.syntaxDirectObjectFlagDictionary  = new Dictionary<int, string[]>{};
        for(int i = 0; i < data.syntaxes.Length; i++){

            List<string> subroutineNames = new List<string>{};
            for(int a = 0; a < data.syntaxes[i].directObjectFlags.Length; a++){

                subroutineNames.Add(data.syntaxes[i].directObjectFlags[a].Method.Name);

            }
            data.syntaxDirectObjectFlagDictionary.Add(i, subroutineNames.ToArray());

        }

        data.syntaxIndirectObjectFlagDictionary  = new Dictionary<int, string[]>{};
        for(int i = 0; i < data.syntaxes.Length; i++){

            List<string> subroutineNames = new List<string>{};
            for(int a = 0; a < data.syntaxes[i].indirectObjectFlags.Length; a++){

                subroutineNames.Add(data.syntaxes[i].indirectObjectFlags[a].Method.Name);

            }
            data.syntaxIndirectObjectFlagDictionary.Add(i, subroutineNames.ToArray());

        }

        GameF.SerializeObject<GameData>(data, saveFolderLocation);

    }

}
public class Object{

    public string name;
    public string description;
    public int holderID;
    public ObjectClass[] classes;
    [IgnoreDataMember] public Func<bool> subroutine;
    public Object(string _name, string _description, int _holderID, Func<bool> _subroutine){

        name = _name;
        description = _description;
        holderID = _holderID;
        subroutine = _subroutine;

    }
    public Object(){}

}
public class ObjectClass{}
public class WordSynonymPair{

    //used for verbs and prepositions
    public string word;
    public int synonym;

    public WordSynonymPair(string _word, int _synonym){

        word = _word;
        synonym = _synonym;

    }
    public WordSynonymPair(){}

}
public class Syntax{

    public int verbID;
    public int preposition1ID;

    //delegates cannot be serialized
    [IgnoreDataMember] public Func<int, bool, bool>[] directObjectFlags;
    public int preposition2ID;
    [IgnoreDataMember] public Func<int, bool, bool>[] indirectObjectFlags;
    [IgnoreDataMember] public Func<bool> subroutine;

    public Syntax(

        int _verbID, 
        int _preposition1ID, 
        Func<int, bool, bool>[] _directObjectFlags,
        int _preposition2ID, 
        Func<int, bool, bool>[] _indirectObjectFlags, 
        Func<bool> _subroutine

    )
    {

        verbID = _verbID;
        preposition1ID = _preposition1ID;
        directObjectFlags = _directObjectFlags;
        preposition2ID = _preposition2ID;
        indirectObjectFlags = _indirectObjectFlags;
        subroutine = _subroutine;

    }

    public Syntax(){}

}
