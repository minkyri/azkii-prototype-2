using System.IO;
using System.Reflection;

public class Game{

    private static Game instance;
    public bool isFinished = false;
    public GameData data;

    private Game(){}
    
    public static Game GetInstance()
    {

        if (instance == null)
        {

            instance = new Game();
            instance.data = new GameData();
            instance.LoadGame();
            instance.data.StartGame(); //should remove afterwards?

        }
        return instance;

    }

    public void LoadGame(){

        //string loadFolderLocation = @"F:\school\computer-science\cs-project\solutions\azkii-prototype-2\data-tables\example";
        string loadFolderLocation = GameF.Input("Enter folder to load game from.");
        Type dataClassType = this.data.GetType();

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
            data.articles = articleList.ToArray();

        #endregion
        #region Read Conjunctions

            lines = GameF.ReadCSV(loadFolderLocation + "/conjunctions" + fileType);
            List<string> conjunctionList = new List<string>{};
            for(int i = 1; i < lines.Length; i++){

                conjunctionList.Add(lines[i][0]);

            }
            data.conjunctions = conjunctionList.ToArray();
        
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
                    verbList.Add(new WordSynonymPair(lines[i][0], currentVerbIndex));
                    verbIndexCounter ++;

                    for(int b = 0; b < synonyms.Length; b++){

                        verbList.Add(new WordSynonymPair(synonyms[b], currentVerbIndex));
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
                TypeFlags flags = TypeFlags.None;
                Func<bool> subroutine;
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

                    if(adjective != ""){
                        
                        knownWordsList.Add(adjective);
                        objectWordList.Add(new int[2]{i-1, knownWordsIndexCounter});
                        knownWordsIndexCounter++;

                    }

                }
                foreach(string synonym in objectSynonyms){

                    if(synonym != ""){
                        
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

                    MethodInfo theMethod = dataClassType.GetMethod(lines[i][17]);
                    if(theMethod == null){

                        GameF.Print("The subroutine " + lines[i][17] + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/objects" + fileType + " does not exist.");
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
                    flags,
                    travelTable,
                    subroutine

                ));

            }
            
            objectList.Add(new Object(

                "it",
                "",
                0,
                TypeFlags.None,
                GameF.isolated,
                null

            ));
            knownWordsList.Add("it");
            objectWordList.Add(new int[]{objectList.Count-1,knownWordsList.Count-1});

            data.objects = objectList.ToArray();
            data.knownWords = knownWordsList.ToArray();
            
            data.objectWordTable = new int[objectWordList.Count,2];
            for(int i = 0; i < objectWordList.Count; i++){

                data.objectWordTable[i,0] = objectWordList[i][0];
                data.objectWordTable[i,1] = objectWordList[i][1];

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
                Func<bool> subroutine;

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

                // string[] actionStrings = lines[i][5].Split(',');
                // List<Action> actionList = new List<Action>{};
                // foreach(string actionString in actionStrings){

                //     if(actionString != ""){

                //         MethodInfo theMethod = dataClassType.GetMethod(actionString);
                //         if(theMethod == null){

                //             GameF.Print("The subroutine " + actionString + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                //             LoadGame();
                //             return;

                //         }
                //         actionList.Add((Action)Delegate.CreateDelegate(typeof(Action), this, theMethod));

                //     }
                //     else{

                //         actionList.Add(null);

                //     }

                // }
                // subroutines = actionList.ToArray();

                string actionString = lines[i][5];               
                MethodInfo theMethod = dataClassType.GetMethod(actionString);
                if(theMethod == null){

                    GameF.Print("The subroutine " + lines[i][5] + " specified on line " + (i+1).ToString() + " of " + loadFolderLocation + "/syntax" + fileType + " does not exist.");
                    LoadGame();
                    return;

                }
                subroutine = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), data, theMethod);

                int verbID = Parser.GetWSPairIndex(verb, data.verbs);
                int preposition1ID = Parser.GetWSPairIndex(preposition1, data.prepositions);
                int preposition2ID = Parser.GetWSPairIndex(preposition2, data.prepositions);

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
                //First note on README.md

                syntaxList.Add(new Syntax(

                    verbID,
                    preposition1ID,
                    directObjectFlags,
                    preposition2ID,
                    indirectObjectFlags,
                    subroutine

                ));

            }
            
            data.syntaxes = syntaxList.ToArray();
            //GameF.Print(syntaxes.Length.ToString());

        #endregion
        #region Read Classes

            //check string after the '_p', and try to match it to an existing property class.
            //add an instance of this class with the specified values to the corresponding objects.

            string[] files = Directory.GetFiles(loadFolderLocation);
            Dictionary<int, List<ObjectClass>> objectClasses = new Dictionary<int, List<ObjectClass>>{};

            foreach(string file in files){

                string cutFileName = Path.GetFileNameWithoutExtension(file);
                string fileName = Path.GetFileName(file);
                if (cutFileName.Length > 2){

                    if(cutFileName.Substring(0, 2) == "c_" && Path.GetExtension(file) == ".csv"){

                        string classFileName = cutFileName.Substring(2);
                        string className = GameF.SnakeToPascal(classFileName);

                        Type classType = Type.GetType(className);
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

                            object objectClass = Activator.CreateInstance(classType);
                            if(!int.TryParse(lines[i][0], out int objectIndex) || objectIndex < 0 || objectIndex > (data.objects.Length-1)){

                                GameF.Print("The object index \"" + lines[i][0] + "\" specified on line " + (i+1).ToString() + " of " + fileName + " is not an existing object ID.");
                                LoadGame();
                                return;

                            }

                            for(int p = 1; p < lines[i].Length; p++){

                                string propertyName = lines[0][p];
                                if(!propertyTypes.TryGetValue(propertyName, out Type propertyType)){

                                    GameF.Print("The property \"" + lines[0][p] + "\" of the object type \"" + className + "\" specified on column " + (p+1).ToString() + " of " + fileName + " does not exist.");
                                    LoadGame();
                                    return;

                                }

                                object propertyValue;
                                if(propertyType.IsArray){
                                    
                                    string[] splitInput = lines[i][p].Split(',');
                                    Array values = Array.CreateInstance(propertyType.GetElementType(), splitInput.Length);
                                    
                                    for(int s = 0; s < splitInput.Length; s++){

                                        if(!GameF.TryCast(splitInput[s], propertyType.GetElementType(), out object value)){

                                            GameF.Print("The property value \"" + lines[i][p] + "\" of the object type " + className + " specified on column " + (p+1).ToString() + ", line " + (i+1).ToString() + ", item " + (s+1).ToString() + ", of " + fileName + " is a " + propertyType.Name + ", which is an invalid property type.");
                                            LoadGame();
                                            return;

                                        }

                                        values.SetValue(value, s);

                                    }

                                    propertyValue = Convert.ChangeType(values, propertyType);

                                }
                                else if(propertyType.IsPrimitive){

                                    if(!GameF.TryCast(lines[i][p], propertyType, out propertyValue)){

                                        GameF.Print("The property value \"" + lines[i][p] + "\" of the object type " + className + " specified on column " + (p+1).ToString() + ", line " + (i+1).ToString() + ", of " + fileName + " is a " + propertyType.Name + ", which is an invalid property type.");
                                        LoadGame();
                                        return;

                                    }

                                }
                                else{

                                    GameF.Print("The property \"" + lines[0][p] + "\" of the object type " + className + " specified on column " + (p+1).ToString() + " of " + fileName + " is a " + propertyType.Name + ", which is an invalid property type.");
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

}
public class Object{

    public string name;
    public string description;
    public int holderID;
    public int[] travelTable;
    public TypeFlags flags;
    public ObjectClass[] classes;
    public Func<bool> subroutine;
    public Object(string _name, string _description, int _holderID, TypeFlags _flags, int[] _travelTable, Func<bool> _subroutine){

        name = _name;
        description = _description;
        holderID = _holderID;
        flags = _flags;
        travelTable = _travelTable;
        subroutine = _subroutine;

    }

}
public class ObjectClass{



}
public class WordSynonymPair{

    public string word;
    public int synonym;

    public WordSynonymPair(string _word, int _synonym){

        word = _word;
        synonym = _synonym;

    }

}
public class Syntax{

    public int verbID;
    public int preposition1ID;
    public TypeFlags directObjectFlags;
    public int preposition2ID;
    public TypeFlags indirectObjectFlags;
    public Func<bool> subroutine;

    public Syntax(

        int _verbID, 
        int _preposition1ID, 
        TypeFlags _directObjectFlags, 
        int _preposition2ID, 
        TypeFlags _indirectObjectFlags, 
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

}