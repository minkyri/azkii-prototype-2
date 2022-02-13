public static class Parser{

    public static void Parse(string input){

        string cleanInput = CleanInput(input.ToLower());
        string[] commands = SplitActions(cleanInput);

        //GameF.Print(commands.Length.ToString());
        for(int i = 0; i < commands.Length; i++){

            string[] command = commands[i].Split(' ');
            string verb = "";
            string directObject = "";
            string preposition = "";
            string indirectObject = "";

            

            // GameF.Print("ACTION: " + (i+1).ToString());
            // GameF.Print("   verb: " + verb);
            // GameF.Print("   direct object: " + directObject);
            // GameF.Print("   preposition: " + preposition);
            // GameF.Print("   indirect object: " + indirectObject);

        }

        foreach(string s in commands)GameF.Print(s);

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

}