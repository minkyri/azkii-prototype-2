public static class Parser{

    public static int verbID;
    public static int preposition1ID = -1;
    public static int directObjectID = -1;
    public static int preposition2ID = -1;
    public static int indirectObjectID = -1;
    public static int syntaxID = -1;
    public static int itObjectPointer = -1;
    public static string directObjectInput = "";
    public static string indirectObjectInput = "";
    public static string currentInput = "";
    public static void Parse(string input){

        string cleanInput = CleanInput(input.ToLower());
        string[] commands = SplitActions(cleanInput);

        for(int i = 0; i < commands.Length; i++){

            string[] command = commands[i].Split(' ');
            verbID = -1;
            preposition1ID = -1;
            directObjectID = -1;
            preposition2ID = -1;
            indirectObjectID = -1;
            syntaxID = -1;

            verbID = GetWSPairIndex(command[0], Game.GetInstance().data.verbs);

            if(verbID == -1){

                if(
                    
                    GetWSPairIndex(command[0], Game.GetInstance().data.prepositions) != -1 ||
                    Game.GetInstance().data.knownWords.Contains(command[0])
                
                ){

                    GameF.Print("Your first word must be a verb!");

                }
                else{

                    GameF.Print("I don't know the word \"" + command[0] + "\".");

                }

                return;

            }

            bool foundAnObject = false;
            string[] objectPhrases = new string[2]{"",""};
            int phrasePointer = 0;

            for(int a = 1; a < command.Length; a++){

                int tempPrepositionID = GetWSPairIndex(command[a], Game.GetInstance().data.prepositions);
                if(tempPrepositionID != -1){

                    if(preposition1ID == -1 && !foundAnObject){
                        
                        preposition1ID = tempPrepositionID;

                    }
                    else if(foundAnObject){

                        if(preposition2ID == -1){

                            preposition2ID = tempPrepositionID;

                        }
                        foundAnObject = false;
                        phrasePointer ++;

                    }
                    
                }
                else if(Game.GetInstance().data.knownWords.Contains(command[a])){
                
                    foundAnObject = true;
                    
                    if(phrasePointer >= 2){

                        GameF.Print("There are too many nouns in that sentence!");
                        return;

                    }
                    else{

                        objectPhrases[phrasePointer] += command[a] + " ";

                    }   

                }
                else if(GetWSPairIndex(command[a], Game.GetInstance().data.verbs) != -1){

                    GameF.Print("You've used too many verbs!");
                    return;

                }
                else{

                    GameF.Print("I don't know the word \"" + command[a] + "\".");
                    return;

                }

            }

            for(int a = 0; a < objectPhrases.Length; a++){

                objectPhrases[a] =  CleanInput(objectPhrases[a]);

            }

            if(itObjectPointer != -1){

                string lastObjectInput = "";
                if(directObjectInput != ""){

                    lastObjectInput = directObjectInput;

                }
                if(indirectObjectInput != ""){

                    lastObjectInput = indirectObjectInput;

                }

                if(objectPhrases[0] == "it")objectPhrases[0] = lastObjectInput;
                if(objectPhrases[1] == "it")objectPhrases[1] = lastObjectInput;

            }
            
            directObjectInput = objectPhrases[0];
            indirectObjectInput = objectPhrases[1];

            if(objectPhrases[0] != ""){

                directObjectID = ScoreObjects(objectPhrases[0]);
                if(directObjectID == -1){

                    GameF.Print("There is no such object called " + objectPhrases[0] + ".");
                    return;

                }
                else if(directObjectID == -2){

                    GameF.Print("You need to be more specific than that.");
                    return;

                }
                else if(directObjectID == -3){

                    GameF.Print("I can't see what you're refering to.");
                    return;

                }

            }

            if(objectPhrases[1] != ""){

                indirectObjectID = ScoreObjects(objectPhrases[1]);
                if(indirectObjectID == -1){

                    GameF.Print("There is no such object called " + objectPhrases[1] + ".");
                    return;

                }
                else if(indirectObjectID == -2){

                    GameF.Print("You need to be more specific than that.");
                    return;

                }
                else if(indirectObjectID == -3){

                    GameF.Print("I can't see what you're refering to.");
                    return;

                }

            }

            // GameF.Print("ACTION: " + (i+1).ToString());
            // if(verbID != -1)GameF.Print("   verb: " + Game.GetInstance().data.verbs[verbID].word);
            // if(preposition1ID != -1)GameF.Print("   preposition 1: " + Game.GetInstance().data.prepositions[preposition1ID].word);
            // if(directObjectID != -1)GameF.Print("   direct object: " + Game.GetInstance().data.objects[directObjectID].name);
            // if(preposition2ID != -1)GameF.Print("   preposition 2: " + Game.GetInstance().data.prepositions[preposition2ID].word);
            // if(indirectObjectID != -1)GameF.Print("   indirect object: " + Game.GetInstance().data.objects[indirectObjectID].name);

            Syntax[] syntaxes = Game.GetInstance().data.syntaxes;
            Object[] objects = Game.GetInstance().data.objects;

            Func<bool> directObjectAction = null;
            Func<bool> indirectObjectAction = null;
            Func<bool> verbAction = null;

            bool foundSyntax = false;

            for(int s = 0; s < syntaxes.Length && !foundSyntax; s++){

                // string verb = "";
                // string prep1 = "";
                // string prep2 = "";

                // if(syntaxes[s].verbID != -1)verb = Game.GetInstance().data.verbs[syntaxes[s].verbID].word;
                // if(syntaxes[s].preposition1ID != -1)prep1 = Game.GetInstance().data.prepositions[syntaxes[s].preposition1ID].word;
                // if(syntaxes[s].preposition2ID != -1)prep2 = Game.GetInstance().data.prepositions[syntaxes[s].preposition2ID].word;

                // GameF.Print(

                //     verb + " : " + 
                //     prep1 + " : " + 
                //     prep2 + " : " + 
                //     (!((syntaxes[s].directObjectFlags.Length == 0 && directObjectID != -1) ||
                //     (syntaxes[s].indirectObjectFlags.Length == 0 && indirectObjectID != -1))).ToString()

                // );

                // GameF.Print(

                //     "     " + (syntaxes[s].directObjectFlags.Length == 0).ToString() + "\n"

                // );

                if(

                    syntaxes[s].verbID == verbID &&
                    syntaxes[s].preposition1ID == preposition1ID &&
                    syntaxes[s].preposition2ID == preposition2ID &&

                    !((syntaxes[s].directObjectFlags.Length == 0 && directObjectID != -1) ||
                      (syntaxes[s].indirectObjectFlags.Length == 0 && indirectObjectID != -1))

                ){

                    foundSyntax = true;
                    syntaxID = s;
                    
                }

            }

            if(!foundSyntax){

                GameF.Print("That sentence isn't one that I recognise.");
                return;

            }

            currentInput = directObjectInput;
            if(directObjectID != -1 && !CheckFlags(directObjectID, syntaxes[syntaxID].directObjectFlags)){

                return;

            }
            currentInput = indirectObjectInput;
            if(indirectObjectID != -1 && !CheckFlags(indirectObjectID, syntaxes[syntaxID].indirectObjectFlags)){

                return;

            }

            verbAction = syntaxes[syntaxID].subroutine;
            if(indirectObjectID != -1 && objects[indirectObjectID].subroutine != null){
                
                indirectObjectAction = objects[indirectObjectID].subroutine;

            }
            if(directObjectID != -1 && objects[directObjectID].subroutine != null){
                
                directObjectAction = objects[directObjectID].subroutine;

            }

            if(!ExecuteActions(indirectObjectAction, directObjectAction, verbAction)){

                GameF.Print("You can't do that!");

            }

            if(i < commands.Length-1)GameF.Print("");

        }

    }
    private static string CleanInput(string input){

        List<string> splitInput = input.Split(' ').ToList();
        List<string> cleanSplitInput = new List<string>{};

        for(int i = 0; i < splitInput.Count; i++){

            string temp = "";

            for(int c = 0; c < splitInput[i].Length; c++){
                
                if(!(

                    char.IsSeparator(splitInput[i][c]) ||
                    char.IsSymbol(splitInput[i][c]) ||
                    char.IsPunctuation(splitInput[i][c]) ||
                    char.IsWhiteSpace(splitInput[i][c])

                )){

                    temp += splitInput[i][c];

                }

            }

            if(temp != "" && !Game.GetInstance().data.articles.Contains(temp))cleanSplitInput.Add(temp);

        }

        //foreach(string s in cleanSplitInput)GameF.Print(s + "   len: " + s.Length.ToString());

        return ArrayToSentenceString(cleanSplitInput.ToArray());

    }
    private static string[] SplitActions(string input){

        string[] splitInput = input.Split(' ');
        List<List<string>> commands = new List<List<string>>{};

        int commandIndex = 0;
        commands.Add(new List<string>{});
        for(int i = 0; i < splitInput.Length; i++){

            if(Game.GetInstance().data.conjunctions.Contains(splitInput[i])){
                
                commandIndex ++;
                commands.Add(new List<string>{});

            }
            else commands[commandIndex].Add(splitInput[i]);
            
        }

        List<List<string>> tempList = new List<List<string>>{};
        foreach(List<string> l in commands)if(l.Count != 0)tempList.Add(l);
        commands = tempList;

        string[] returnArray = new string[commands.Count];
        for(int i = 0; i < returnArray.Length; i++){

            returnArray[i] = ArrayToSentenceString(commands[i].ToArray());

        }

        return returnArray;

    }
    private static string ArrayToSentenceString(string[] array){

        string returnString = "";

        if(array.Length == 0)return returnString;

        for(int i = 0; i < array.Length-1; i++)returnString += array[i] + " ";
        returnString += array[array.Length-1];
        return returnString;

    }
    public static int GetWSPairIndex(string word, WordSynonymPair[] WSArr){

        for(int i = 0; i < WSArr.Length; i++){

            if(WSArr[i].word == word){

                return WSArr[i].synonym;

            }

        }

        return -1;

    }
    public static int[] WordToObjectIndexes(string word){
        
        if(itObjectPointer != -1 && word == "it"){

            return new int[]{itObjectPointer};

        }

        int[,] objectWordTable = Game.GetInstance().data.objectWordTable;
        List<int> objectIndexes = new List<int>{};

        for(int i = 0; i < objectWordTable.Length/objectWordTable.Rank; i++){

            foreach(int index in GameF.GetIndexes(word, Game.GetInstance().data.knownWords)){

                if(index == objectWordTable[i,1]){

                    objectIndexes.Add(objectWordTable[i,0]);

                }

            }

        }

        // for(int i = 0; i < objectIndexes.Count; i++){

        //     GameF.Print(Game.GetInstance().data.objects[objectIndexes[i]].description);

        // }

        return objectIndexes.ToArray();

    }
    public static int ScoreObjects(string objectPhrase){

        string[] splitPhrase = objectPhrase.Split(" ");
        int singleObjectID = -1;
        List<int> objects = new List<int>{};
        List<int> objectScores = new List<int>{};

        for(int b = 0; b < splitPhrase.Length; b++){

            if(itObjectPointer == -1 && splitPhrase[b] == "it"){

                return -3;

            }

            //GameF.Print(splitPhrase[b] + "  len: " + splitPhrase[b].Length);
            int[] objectIndexes = WordToObjectIndexes(splitPhrase[b]);
            
            if(objectIndexes.Length == 1 && splitPhrase.Length > 1){

                if(singleObjectID == -1)singleObjectID = objectIndexes[0];
                else if(singleObjectID != -1 && objectIndexes[0] != singleObjectID){

                    return -1;

                }

            }

            // GameF.Print(splitPhrase[b].ToUpper() + ": ");
            // foreach(int i in objectIndexes){

            //     GameF.Print(i.ToString());

            // }

            for(int c = 0; c < objectIndexes.Length; c++){

                if(objects.Contains(objectIndexes[c])){
                    
                    objectScores[objects.IndexOf(objectIndexes[c])] += 1;

                }
                else{

                    objects.Add(objectIndexes[c]);
                    objectScores.Add(1);

                }

            }

        }

        int highestScore = 0;
        int highscoreObject = -1;
        int highestScoreIndex = -1;

        for(int i = 0; i < objectScores.Count; i++){

            if(objectScores[i] > highestScore){

                highestScore = objectScores[i];
                highscoreObject = objects[i];
                highestScoreIndex = i;

            }

        }

        for(int j = 0; j < objectScores.Count; j++){

            if(objectScores[j] == highestScore && j != highestScoreIndex){

                return -2;

            }

        }

        itObjectPointer = highscoreObject;
        return highscoreObject;

    }
    public static bool CheckFlags(int objectID, Func<int, bool, bool>[] flags){

        if(flags.Length == 0)return true;
        List<Func<int, bool, bool>> failedFlags = new List<Func<int, bool, bool>>{};

        foreach(Func<int, bool, bool> flag in flags){

            if(flag(objectID, false))return true;
            else failedFlags.Add(flag);

        }

        failedFlags[0](objectID, true);
        return false;

    }
    public static bool ExecuteActions(Func<bool> IO, Func<bool> DO, Func<bool> V){

        if(V != null && V()){

            return true;

        }
        if(IO != null && IO()){

            return true;

        }
        if(DO != null && DO()){

            return true;

        }
        return false;

    }

}