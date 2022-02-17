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
    public Action subroutine;
    public Object(string _name, string _description, int _holderID, TypeFlags _flags, int[] _travelTable, Action _subroutine){

        name = _name;
        description = _description;
        holderID = _holderID;
        flags = _flags;
        travelTable = _travelTable;
        subroutine = _subroutine;

    }

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
    public Action[] subroutines;

    public Syntax(

        int _verbID, 
        int _preposition1ID, 
        TypeFlags _directObjectFlags, 
        int _preposition2ID, 
        TypeFlags _indirectObjectFlags, 
        Action[] _subroutines

    )
    {

        verbID = _verbID;
        preposition1ID = _preposition1ID;
        directObjectFlags = _directObjectFlags;
        preposition2ID = _preposition2ID;
        indirectObjectFlags = _indirectObjectFlags;
        subroutines = _subroutines;

    }

}
