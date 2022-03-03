public class Game{

    private static Game instance;
    public bool isFinished = false;
    public GameData data;

    private Game(){

        

    }
    
    public static Game GetInstance()
    {
        if (instance == null)
        {

            instance = new Game();
            instance.data = new GameData();
            instance.data.StartGame();

        }
        return instance;
    }

}
public class Object{

    public string name;
    public string description;
    public int holderID;
    public int[] travelTable;
    public TypeFlags flags;
    public Property[] properties;
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
public class Property{



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