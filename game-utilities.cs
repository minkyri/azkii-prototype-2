public static class GameF{

    #region Macros

        public static int[] isolated = new int[]{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1};

    #endregion
    public static void Print(string display){

        Console.WriteLine(display);

    }
    public static string Input(string display){

        Console.WriteLine(display);
        Console.Write("\n>  ");
        return Console.ReadLine();

    }
    public static string Input(){

        Console.Write("\n>  ");
        return Console.ReadLine();

    }
    public static string SnakeToPascal(string snakeString){

        if(snakeString.Length == 0)return snakeString;

        char separator = '-';
        string pascalString = "";

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

        List<int> indexes = new List<int>{};

        for(int i = 0; i < arr.Length; i++){

            if(arr[i] == s)indexes.Add(i);

        }
        
        if(indexes.Count > 0)return indexes.ToArray();
        return new int[]{-1};

    }
    public static bool IsFileLocked(FileInfo file)
    {
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
        int[,] objectWordTable = Game.GetInstance().data.objectWordTable;
        for(int i = 0; i < objectWordTable.Length/objectWordTable.Rank; i++){

            GameF.Print("   " + objects[objectWordTable[i,0]].name + " : " + knownWords[objectWordTable[i,1]]);

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

        Object[] objects = Game.GetInstance().data.objects;
        for(int i = 0; i < objects.Length; i++){

            if(objects[i].name == name){

                return i;

            }

        }

        return -1;

    }
    public static int GetObjectHolder(string name){

        int obj = SearchForObject(name);
        if(obj == -1)return -1;
        return Game.GetInstance().data.objects[obj].holderID;

    }
    public static int[] GetHeldObjects(int holderID){

        List<int> heldIDs = new List<int>{};
        Object[] objects = Game.GetInstance().data.objects;
        for(int i = 0; i < objects.Length; i++){

            if(objects[i].holderID == holderID){

                heldIDs.Add(i);

            }

        }

        return heldIDs.ToArray();

    }
    public static int[] GetHeldObjects(int holderID, TypeFlags flags){

        List<int> heldIDs = new List<int>{};
        Object[] objects = Game.GetInstance().data.objects;
        for(int i = 0; i < objects.Length; i++){

            if(objects[i].holderID == holderID && objects[i].flags.HasFlag(flags)){

                heldIDs.Add(i);

            }

        }

        return heldIDs.ToArray();

    }
    public static bool CompareVerb(string verb){

        return Parser.GetWSPairIndex(verb, Game.GetInstance().data.verbs) == Parser.verbID;

    }
    public static bool ComparePreposition1(string preposition){

        return Parser.GetWSPairIndex(preposition, Game.GetInstance().data.prepositions) == Parser.preposition1ID;

    }
    public static bool ComparePreposition2(string preposition){

        return Parser.GetWSPairIndex(preposition, Game.GetInstance().data.prepositions) == Parser.preposition2ID;

    }

}