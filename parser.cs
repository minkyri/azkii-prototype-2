public static class Parser{

    public static void Parse(string input){

        string cleanInput = CleanInput(input.ToLower());
        string[] commands = SplitActions(cleanInput);

        for(int i = 0; i < commands.Length; i++){

            string[] command = commands[i].Split(' ');
            int verbID;
            int preposition1ID = -1;
            int directObjectID = -1;
            int preposition2ID = -1;
            int indirectObjectID = -1;

            verbID = GetWSPairIndex(command[0], Game.GetInstance().data.verbs);

            if(verbID == -1){

                if(
                    
                    GetWSPairIndex(command[0], Game.GetInstance().data.prepositions) != -1 ||
                    Game.GetInstance().data.knownWords.Contains(command[0])
                
                ){

                    GameF.Print("Your first word must be a verb!");

                }
                else{

                    GameF.Print("I don't know the word \"" + command[0] + "\"");

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

                    GameF.Print("I don't know the word \"" + command[a] + "\"");

                }

            }

            directObjectID = ScoreObjects(objectPhrases[0]);
            if(directObjectID == -1){

                GameF.Print("There is no such object called " + objectPhrases[0]);
                return;

            }

            indirectObjectID = ScoreObjects(objectPhrases[1]);
            if(indirectObjectID == -1){

                GameF.Print("There is no such object called " + objectPhrases[1]);
                return;

            }

            GameF.Print("ACTION: " + (i+1).ToString());
            if(verbID != -1)GameF.Print("   verb: " + Game.GetInstance().data.verbs[verbID].word);
            if(preposition1ID != -1)GameF.Print("   preposition 1: " + Game.GetInstance().data.prepositions[preposition1ID].word);
            if(directObjectID != -1)GameF.Print("   direct object: " + Game.GetInstance().data.objects[directObjectID].name);
            if(preposition2ID != -1)GameF.Print("   preposition 2: " + Game.GetInstance().data.prepositions[preposition2ID].word);
            if(indirectObjectID != -1)GameF.Print("   indirect object: " + Game.GetInstance().data.objects[indirectObjectID].name);

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
        
        int[,] objectWordTable = Game.GetInstance().data.objectWordTable;
        List<int> objectIndexes = new List<int>{};

        for(int i = 0; i < objectWordTable.Length/objectWordTable.Rank; i++){

            if(GameF.GetIndex(word, Game.GetInstance().data.knownWords) == objectWordTable[i,1]){

                objectIndexes.Add(objectWordTable[i,0]);

            }

        }

        return objectIndexes.ToArray();

    }
    public static int ScoreObjects(string objectPhrase){

        string[] splitPhrase = CleanInput(objectPhrase).Split(" ");
        int singleObjectID = -1;
        List<int> objects = new List<int>{};
        List<int> objectScores = new List<int>{};

        for(int b = 0; b < splitPhrase.Length; b++){

            int[] objectIndexes = WordToObjectIndexes(splitPhrase[b]);

            if(objectIndexes.Length == 1 && singleObjectID == -1)singleObjectID = objectIndexes[0];
            else if(objectIndexes.Length == 1 && singleObjectID != -1 && objectIndexes[0] != singleObjectID){

                return -1;

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

        for(int i = 0; i < objectScores.Count; i++){

            if(objectScores[i] > highestScore){

                highestScore = objectScores[i];
                highscoreObject = objects[i];

            }

        }

        return highscoreObject;

    }

}