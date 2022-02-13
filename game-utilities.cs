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

    

}