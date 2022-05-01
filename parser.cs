using System.Reflection;

public static class Parser{

    public static int verbID;
    public static int preposition1ID = -1;
    public static int directObjectID = -1;
    public static int preposition2ID = -1;
    public static int indirectObjectID = -1;
    public static int syntaxID = -1;
    public static int itObjectPointer = -1;
    public static string verbInput = "";
    public static string preposition1Input = "";
    public static string preposition2Input = "";
    public static string directObjectInput = "";
    public static string indirectObjectInput = "";
    public static string currentInput = "";
    public static void Parse(string input){

        //removes the whitespace, punctuation and article words from the player's input, to make it more readable by the parser
        string cleanInput = RemoveArticles(CleanInput(input.ToLower()));
        
        //splits the players input into individual commands by conjunctions such as "and", "then"
        string[] commands = SplitActions(cleanInput);

        for(int i = 0; i < commands.Length; i++){

            string[] command = commands[i].Split(' ');

            //check if the player wants to save, load or exit the game
            if(Game.GetInstance().CheckForSpecialActions(commands[i]))return;

            //this is an optional subroutine that the game can have, and will be called everytime the user enters a command
            MethodInfo beforeCommandParsedInfo = Game.GetInstance().data.GetType().GetMethod("BeforeCommandParsed");
            if(beforeCommandParsedInfo != null){

                Action beforeCommandParsedMethod = (Action)Delegate.CreateDelegate(typeof(Action), Game.GetInstance().data, beforeCommandParsedInfo);
                beforeCommandParsedMethod();

            }

            verbID = -1;
            preposition1ID = -1;
            directObjectID = -1;
            preposition2ID = -1;
            indirectObjectID = -1;
            syntaxID = -1;

            //assume the first word is a verb
            verbID = GameF.GetWSPairIndex(command[0], Game.GetInstance().data.verbs);

            //return if no input is provided by the player
            if(commands[i] == "")
            {

                GameF.Print("You didn't enter anything.");
                return;

            }

            if(verbID == -1){

                if(
                    
                    GameF.GetWSPairIndex(command[0], Game.GetInstance().data.prepositions) != -1 ||
                    Game.GetInstance().data.knownWords.Contains(command[0])
                
                ){

                    GameF.Print("Your first word must be a verb!");

                }
                else{

                    GameF.Print("I don't know the word \"" + command[0] + "\".");

                }

                return;

            }

            verbInput = command[0];

            //this section tries to tokenize the command by prepositions, leaving an array of objects.
            bool foundAnObject = false;

            //there should only be a maximum of 2 objects referenced in the command
            string[] objectPhrases = new string[2]{"",""};
            int phrasePointer = 0;

            for(int a = 1; a < command.Length; a++){

                int tempPrepositionID = GameF.GetWSPairIndex(command[a], Game.GetInstance().data.prepositions);
                if(tempPrepositionID != -1){

                    if(preposition1ID == -1 && !foundAnObject){
                        
                        preposition1ID = tempPrepositionID;
                        preposition1Input = command[a];

                    }
                    else if(foundAnObject){

                        if(preposition2ID == -1){

                            preposition2ID = tempPrepositionID;
                            preposition2Input = command[a];

                        }
                        foundAnObject = false;
                        phrasePointer ++;

                    }
                    
                }
                else if(Game.GetInstance().data.knownWords.Contains(command[a])){
                
                    foundAnObject = true;
                    
                    //if there are more than two prepositions, then we get an array of more than two objects, which means that the player has specified too many objects
                    if(phrasePointer >= 2){

                        GameF.Print("There are too many nouns in that sentence!");
                        return;

                    }
                    else{

                        //the object phrases are constructed to give a phrase relating to a specific object - this could include the objects name and adjectives
                        objectPhrases[phrasePointer] += command[a] + " ";

                    }   

                }
                //there can only be one verb, which is at the beginning, so if another verb is detected, there are too many verbs and we return.
                else if(GameF.GetWSPairIndex(command[a], Game.GetInstance().data.verbs) != -1){

                    GameF.Print("You've used too many verbs!");
                    return;

                }
                //if the word doesn't show up in the prepositions, the verbs, the object names, or the object adjectives, then we do not know the word.
                else{

                    GameF.Print("I don't know the word \"" + command[a] + "\".");
                    return;

                }

            }

            for(int a = 0; a < objectPhrases.Length; a++){

                objectPhrases[a] =  CleanInput(objectPhrases[a]);

            }

            //we need to get the last object referenced so that "it" can be used to point to the last object referenced
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

            //now that we have our object phrases, we can begin scoring them to see which object best matches them, so we can populate the direct and indirect object identifiers
            if(objectPhrases[0] != ""){

                directObjectID = ScoreObjects(objectPhrases[0]);
                if(directObjectID != -1)itObjectPointer = directObjectID;
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
                if(indirectObjectID != -1)itObjectPointer = indirectObjectID;
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

            Syntax[] syntaxes = Game.GetInstance().data.syntaxes;
            Object[] objects = Game.GetInstance().data.objects;

            Func<bool> directObjectAction = null;
            Func<bool> indirectObjectAction = null;
            Func<bool> verbAction = null;

            bool foundSyntax = false;

            //now that we know our verb ID and preposition IDs, we can attempt to match it to a syntax, which will tell us how objects can be used
            for(int s = 0; s < syntaxes.Length && !foundSyntax; s++){

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

            //return when no syntax matches verb and prepositions
            if(!foundSyntax){

                GameF.Print("That sentence isn't one that I recognise.");
                return;

            }

            //check if the objects found abide by the flags specified in the syntax
            currentInput = directObjectInput;
            if(directObjectID != -1 && !CheckFlags(directObjectID, syntaxes[syntaxID].directObjectFlags)){

                return;

            }
            currentInput = indirectObjectInput;
            if(indirectObjectID != -1 && !CheckFlags(indirectObjectID, syntaxes[syntaxID].indirectObjectFlags)){

                return;

            }

            //try to get player to elaborate on their input before returning
            if(syntaxes[syntaxID].directObjectFlags.Length != 0 && directObjectID == -1){

                if(preposition1ID != -1){

                    GameF.Print("You need to specify what you want to " + verbInput + " " + preposition1Input + "!");
                    return;

                }
                GameF.Print("You need to specify what you want to " + verbInput + "!");
                return;

            }
            if(syntaxes[syntaxID].indirectObjectFlags.Length != 0 && indirectObjectID == -1){

                if(preposition1ID != -1){

                    GameF.Print("You need to tell me what you want to " + verbInput + " " + preposition1Input + " the " + directObjectInput + " " + preposition2Input + "!");
                    return;                    

                }
                GameF.Print("You need to tell me what you want to " + verbInput + " the " + directObjectInput + " " + preposition2Input + "!");
                return;

            }

            verbAction = syntaxes[syntaxID].subroutine;
            if(indirectObjectID != -1 && objects[indirectObjectID].subroutine != null){
                
                indirectObjectAction = objects[indirectObjectID].subroutine;

            }
            if(directObjectID != -1 && objects[directObjectID].subroutine != null){
                
                directObjectAction = objects[directObjectID].subroutine;

            }

            //finally, execute the subroutine of the indirect object, then that of the direct object and the same for the verb
            if(!ExecuteActions(indirectObjectAction, directObjectAction, verbAction)){

                GameF.Print("You can't do that!");

            }

            if(i < commands.Length-1)GameF.Print("");

        }

    }
    public static string CleanInput(string input){

        //removes punctuation and whitespace

        string[] splitInput = input.Split(' ');
        List<string> cleanSplitInput = new List<string>{};

        for(int i = 0; i < splitInput.Length; i++){

            string temp = "";

            //check each individual character, and only add to the return string if not any of the char types below
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

            if(splitInput[i] != "")cleanSplitInput.Add(temp);

        }

        return GameF.ArrayToSentence(cleanSplitInput.ToArray());

    }
    private static string RemoveArticles(string input){

        //removes articles from input
        //articles are specified in the articles.csv file

        if(Game.GetInstance().data == null)return input;

        string[] splitInput = input.Split(' ');
        List<string> cleanSplitInput = new List<string>{};

        for(int i = 0; i < splitInput.Length; i++){
 
            if(splitInput[i] != "" && !Game.GetInstance().data.articles.Contains(splitInput[i]))cleanSplitInput.Add(splitInput[i]);

        }

        return GameF.ArrayToSentence(cleanSplitInput.ToArray());

    }
    private static string[] SplitActions(string input){

        //split the users input into individual commands by tokenizing by conjunctions
        //conjunctions are specified in the conjunctions.csv file

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

            returnArray[i] = GameF.ArrayToSentence(commands[i].ToArray());

        }

        return returnArray;

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

            int[] objectIndexes = GameF.GetObjectIndexes(splitPhrase[b]);
            
            if(objectIndexes.Length == 1 && splitPhrase.Length > 1){

                if(singleObjectID == -1)singleObjectID = objectIndexes[0];
                else if(singleObjectID != -1 && objectIndexes[0] != singleObjectID){

                    return -1;

                }

            }

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

        List<int> acceptedObjects = new List<int>{};
        acceptedObjects.Add(objects[highestScoreIndex]);
        for(int j = 0; j < objectScores.Count; j++){

            if(objectScores[j] == highestScore && j != highestScoreIndex){

                acceptedObjects.Add(objects[j]);

            }

        }

        if(acceptedObjects.Count > 1){

            //GameF.Print("(" + Game.GetInstance().data.objects[objects[highestScoreIndex]] + ")");
            MethodInfo prioritiseMethodInfo = Game.GetInstance().data.GetType().GetMethod("PrioritiseObjects");
            if(prioritiseMethodInfo != null){

                Func<int[], int> prioritiseMethod = (Func<int[], int>)Delegate.CreateDelegate(typeof(Func<int[], int>), Game.GetInstance().data, prioritiseMethodInfo);
                int priority = prioritiseMethod(acceptedObjects.ToArray());
                if(priority != -1){

                    highscoreObject = priority;

                }
                else{

                    //return -2;

                }

            }

        }

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

        if(IO != null && IO()){

            return true;

        }
        if(DO != null && DO()){

            return true;

        }
        if(V != null && V()){

            return true;

        }
        return false;

    }

}