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

    public Object(string _name, string _description, int _holderID, TypeFlags _flags, int[] _travelTable){

        name = _name;
        description = _description;
        holderID = _holderID;
        flags = _flags;
        travelTable = _travelTable;

    }

}
public class Syntax{

    int verbID;
    int preposition1ID;
    TypeFlags directObjectFlags;
    int preposition2ID;
    TypeFlags indirectObjectFlags;
    Action<int>[] subroutines;

    public Syntax(

        int _verbID, 
        int _preposition1ID, 
        TypeFlags _directObjectFlags, 
        int _preposition2ID, 
        TypeFlags _indirectObjectFlags, 
        Action<int>[] _subroutines

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
