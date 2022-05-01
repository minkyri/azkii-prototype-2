using System.Runtime.Serialization;
using System.Xml;

public static class GameF{

    #region General Utilities
        public static Random random = new Random();
        public static void Print(string display){

            //prints message character by character for a more immersive feel
            for(int i = 0; i < display.Length; i++){

                Console.Write(display[i]);
                //if the player presses a key, immediately display the entire message
                if(!Console.KeyAvailable){
                    
                    Thread.Sleep(Game.GetInstance().textSpeed);

                }

            }
            Console.WriteLine();

        }
        public static string Input(string display){

            //take an input from the player while giving them a message. this could be used for questions
            Print(display);
            return Input();

        }
        public static string Input(){

            //take an input from the player
            Console.Write("\n>  ");
            return Console.ReadLine();

        }
        public static string SnakeToPascal(string snakeString){

            //used to convert from snake case to pascal case
            // on-container becomes OnContainer

            if(snakeString.Length == 0)return snakeString;

            char separator = '-';
            string pascalString = "";

            //first letter capitalised
            pascalString += char.ToUpper(snakeString[0]);
            bool lastWasSeparator = false;

            for(int i = 1; i < snakeString.Length; i++){

                if(lastWasSeparator){

                    if(snakeString[i] != separator){
                        
                        pascalString += char.ToUpper(snakeString[i]);
                        lastWasSeparator = false;

                    }

                }
                else{

                    if (snakeString[i] == separator)lastWasSeparator = true;
                    else {
                        
                        pascalString += snakeString[i];

                    }

                }

            }

            return pascalString;

        }
        public static bool TryCast(object input, Type type, out object result)
        {

            //used to safely cast an object to a type

            result = null;
            try
            {

                result = Convert.ChangeType(input, type);
                return true;
                
            }
            catch
            {

                return false;

            }

        }
        public static int[] GetIndexes(string s, string[] arr){

            //find all of the indexes for which a string appears in an array

            List<int> indexes = new List<int>{};

            for(int i = 0; i < arr.Length; i++){

                if(arr[i] == s)indexes.Add(i);

            }
            
            if(indexes.Count > 0)return indexes.ToArray();
            return new int[]{-1};

        }
        public static string ArrayToSentence(string[] array){

            //converts an array of strings to a single string, and puts spaces inbetween

            string returnString = "";

            if(array.Length == 0)return returnString;

            for(int i = 0; i < array.Length-1; i++)returnString += array[i] + " ";
            returnString += array[array.Length-1];
            return returnString;

        } 
        public static bool Chance(int percent){

            //calculates a percentage chance
            return random.Next(0, 100) < percent;

        }

    #endregion
    #region File Reading/Writing

        public static bool IsFileLocked(FileInfo file)
        {

            //check if file locked/open to prevent writing/reading while file still in memory
            try
            {
                using(FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {

                return true;

            }

            //file is not locked
            return false;
        }
        public static string[][] ReadCSV(string path){

            //gets the contents of a csv file

            List<string[]> lines = new List<string[]>{};

            using(var reader = new StreamReader(path)){

                while(!reader.EndOfStream){

                    lines.Add(SplitCsvLine(reader.ReadLine()));

                }

            }

            return lines.ToArray();

        }
        public static string[] SplitCsvLine(string line)
        {

            //splits line by commas, except for when quotes are used - quotes indicate a list inside a single cell
            List<string> result = new List<string>();
            string currentStr = "";
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++) // For each character
            {
                if (line[i] == '\"'){ // Quotes are closing or opening
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',') // Comma
                {

                    if (!inQuotes) // If not in quotes, end of current string, add it to result
                    {

                        result.Add(currentStr.ToString());
                        currentStr = "";

                    }
                    else{

                        currentStr += line[i];

                    }

                }
                else{

                    currentStr += line[i];

                }

            }

            result.Add(currentStr.ToString());
            return result.ToArray(); // Return array of all strings

        }
        public static void SerializeObject<T>(T objectInstance, string filePath){

            //serialize object to save as xml
            FileStream writer = new FileStream(filePath, FileMode.Create);
            DataContractSerializer ser = new DataContractSerializer(typeof(T), Game.GetInstance().data.GetType().GetNestedTypes());
            ser.WriteObject(writer, objectInstance);
            writer.Close();

        }
        public static T DeserializeObject<T>(string filePath){

            //deserialize xml contents to get an object instance
            FileStream fs = new FileStream(filePath, FileMode.Open);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(T), Game.GetInstance().data.GetType().GetNestedTypes());

            T objectInstance = (T)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();

            return objectInstance;

        }

    #endregion
    #region Game Functions
        public static void DisplayGameData(){

            Game.GetInstance();
            Print("ARTICLES");
            foreach (string article in Game.GetInstance().data.articles)
            {
                
                Print("    " + article);

            }
            Print("CONJUNCTIONS");
            foreach (string conjunction in Game.GetInstance().data.conjunctions)
            {
                
                Print("    " + conjunction);

            }
            Print("PREPOSITIONS");
            foreach (WordSynonymPair preposition in Game.GetInstance().data.prepositions)
            {
                
                Print("    " + preposition.word + "   synonym: " + Game.GetInstance().data.prepositions[preposition.synonym].word);

            }
            Print("VERBS");
            foreach (WordSynonymPair verb in Game.GetInstance().data.verbs)
            {
                
                Print("    " + verb.word + "   synonym: " + Game.GetInstance().data.verbs[verb.synonym].word);

            }

            Object[] objects = Game.GetInstance().data.objects;
            Print("OBJECTS");
            for(int i = 0; i < objects.Length; i++)
            {
                
                Print("    NAME: " + objects[i].name + 
                " DESCRIPTION:    " + objects[i].description + 
                "   HOLDER:    " + objects[objects[i].holderID].name);

            }

            Syntax[] syntaxes = Game.GetInstance().data.syntaxes;
            Print("SYNTAX");
            for(int i = 0; i < syntaxes.Length; i++)
            {
                
                string display = "";
                display += "    VERB: " + Game.GetInstance().data.verbs[syntaxes[i].verbID].word;
                if(syntaxes[i].preposition1ID != -1)display += " PREP1:    " + Game.GetInstance().data.prepositions[syntaxes[i].preposition1ID].word;
                if(syntaxes[i].preposition2ID != -1)display += "   PREP2:    " + Game.GetInstance().data.prepositions[syntaxes[i].preposition2ID].word;
                
                Print(display);

            }

            GameF.Print("KNOWN WORDS");
            string[] knownWords = Game.GetInstance().data.knownWords;
            foreach(string knownWord in knownWords){

                GameF.Print("   " + knownWord);

            }

            GameF.Print("OBJECT WORD TABLE");
            int[][] objectWordTable = Game.GetInstance().data.objectWordTable;
            for(int i = 0; i < objectWordTable.Length/objectWordTable.Rank; i++){

                GameF.Print("   " + objects[objectWordTable[i][0]].name + " : " + knownWords[objectWordTable[i][1]]);

            }

            GameF.Print("CLASSES");
            foreach(Object obj in objects){

                GameF.Print("   " + obj.name + ":");

                ObjectClass[] classes = obj.classes;
                if(classes != null){

                    foreach(ObjectClass c in classes){

                        System.Reflection.PropertyInfo[] properties = c.GetType().GetProperties();
        
                        GameF.Print("       " + c.GetType().Name + ":");

                        foreach(System.Reflection.PropertyInfo property in properties){

                            GameF.Print("           " + property.Name.ToString() + " : " + property.GetValue(c).ToString());

                        }

                    }
                    
                }
                
            }

        }
        public static int SearchForObject(string name){

            return Parser.ScoreObjects(name);

        }
        public static int GetObjectHolder(string name){

            int obj = SearchForObject(name);
            if(obj == -1)return -1;
            return Game.GetInstance().data.objects[obj].holderID;

        }
        public static int[] GetHeldObjects(int holderID){

            if(holderID == -1)return new int[]{};

            List<int> heldIDs = new List<int>{};
            Object[] objects = Game.GetInstance().data.objects;
            for(int i = 0; i < objects.Length; i++){

                if(objects[i].holderID == holderID){

                    heldIDs.Add(i);

                }

            }

            return heldIDs.ToArray();

        }
        public static int[] GetObjectIndexes(string word){
        
            if(Parser.itObjectPointer != -1 && word == "it"){

                return new int[]{Parser.itObjectPointer};

            }

            int[][] objectWordTable = Game.GetInstance().data.objectWordTable;
            List<int> objectIndexes = new List<int>{};

            for(int i = 0; i < objectWordTable.Length/objectWordTable.Rank; i++){

                foreach(int index in GameF.GetIndexes(word, Game.GetInstance().data.knownWords)){

                    if(index == objectWordTable[i][1]){

                        objectIndexes.Add(objectWordTable[i][0]);

                    }

                }

            }

            // for(int i = 0; i < objectIndexes.Count; i++){

            //     GameF.Print(Game.GetInstance().data.objects[objectIndexes[i]].description);

            // }

            return objectIndexes.ToArray();

        }
        public static int GetWSPairIndex(string word, WordSynonymPair[] WSArr){

            for(int i = 0; i < WSArr.Length; i++){

                if(WSArr[i].word == word){

                    return WSArr[i].synonym;

                }

            }

            return -1;

        }
        public static bool CompareVerb(string verb){

            return GetWSPairIndex(verb, Game.GetInstance().data.verbs) == Parser.verbID;

        }
        public static bool ComparePreposition1(string preposition){

            return GetWSPairIndex(preposition, Game.GetInstance().data.prepositions) == Parser.preposition1ID;

        }
        public static bool ComparePreposition2(string preposition){

            return GetWSPairIndex(preposition, Game.GetInstance().data.prepositions) == Parser.preposition2ID;

        }
        public static void MoveObject(int objectID, int toID){

            Object[] objects = Game.GetInstance().data.objects;
            if(objects.Length > objectID && objects.Length > toID){

                objects[objectID].holderID = toID;

            }

        }
        public static bool EvaluateFlag(bool condition, bool showMessage, string message){

            if(condition){

                return true;

            }
            if(showMessage){

                GameF.Print(message);

            }
            return false;

        }
        public static bool TryGetObjectClass<T>(int objectID, out T objectClass){

            Type classType = typeof(T);
            objectClass = default(T);

            Object obj = Game.GetInstance().data.objects[objectID];

            if(objectID == -1 || obj == null || obj.classes == null){

                return false;
                
            }

            foreach(ObjectClass c in obj.classes){

                if(c.GetType() == classType){

                    if(TryCast(c, classType, out object cast)){

                        objectClass = (T)cast;
                        return true;

                    }

                }

            }

            return false;

        }
        public static Dictionary<int, int> GetObjectHierarchy(int objectID){

            return GetObjectHierarchyRecursive(objectID, 0);

        }
        public static Dictionary<int, int> GetObjectHierarchyRecursive(int objectID, int indents){

            Dictionary<int, int> hierarchyDictionary = new Dictionary<int, int>{};
            
            if(indents != 0){

                hierarchyDictionary.Add(objectID, indents);

            }

            int[] held = GameF.GetHeldObjects(objectID);
            
            foreach(int heldID in held)
            {

                Dictionary<int, int> addition = GetObjectHierarchyRecursive(heldID, indents + 1);
                foreach(KeyValuePair<int, int> kvp in addition){

                    hierarchyDictionary.Add(kvp.Key, kvp.Value);

                }

            }

            return hierarchyDictionary;

        }

    #endregion

}

