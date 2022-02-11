public static class Parser{

    private static string[] fillerWords = new string[]{

        "the",
        "an",
        "a"

    };

    public static void Parse(string input){

        input = input.ToLower();
        List<string> splitInput = CleanInput(input);

        //foreach(string s in splitInput)GameF.Print(s);

    }

    private static List<string> CleanInput(string input){

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

                    //GameF.Print("Removed: " + splitInput[i][c] + " = " + splitInput[i].Remove(c, 1));
                    // splitInput[i] = splitInput[i].Remove(c, 1);
                    temp += splitInput[i][c];

                }

            }

            if(temp != "" && !fillerWords.Contains(temp))cleanSplitInput.Add(temp);

        }

        return cleanSplitInput;

    }

}