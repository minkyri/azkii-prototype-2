public static class GameF{

    public static void Print(string display){

        Console.WriteLine(display);

    }

    public static string Input(string display){

        Console.WriteLine(display);
        Console.Write("\n>  ");
        return Console.ReadLine();

    }

}