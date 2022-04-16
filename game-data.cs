using System.IO;
using System.Reflection;

[Flags]
public enum TypeFlags{

    None = 0,
    Player = 1 << 0,
    Direction = 1 << 1,
    Container = 1 << 2,
    Visible = 1 << 3,
    CanTake = 1 << 4,
    TryTake = 1 << 5,
    Actor = 1 << 6,
    Weapon = 1 << 7

}
public class GameData{

    public string[] articles;
    public string[] conjunctions;
    public WordSynonymPair[] prepositions;
    public WordSynonymPair[] verbs;
    public string[] knownWords;
    public Object[] objects;
    public int[,] objectWordTable;
    public Syntax[] syntaxes;

    public GameData(){

        

    }

    #region Verb Subroutines

        #region Directions
            public bool VGo(){

                string direction = objects[Parser.directObjectID].name;

                switch(direction){

                    case "north":
                        VGoNorth();
                        break;
                    case "northeast":
                        VGoNorthEast();
                        break;
                    case "east":
                        VGoEast();
                        break;
                    case "southeast":
                        VGoSouthEast();
                        break;
                    case "south":
                        VGoSouth();
                        break;
                    case "southwest":
                        VGoSouthWest();
                        break;
                    case "west":
                        VGoWest();
                        break;
                    case "northwest":
                        VGoNorthWest();
                        break;
                    case "up":
                        VGoUp();
                        break;
                    case "down":
                        VGoDown();
                        break;
                    default:
                        GameF.Print("You should specify a direction.");
                        break;

                }
                return true;

            }
            public bool VGoNorth(){

                MovePlayer(0);
                return true;

            }
            public bool VGoNorthEast(){

                MovePlayer(1);
                return true;

            }
            public bool VGoEast(){

                MovePlayer(2);
                return true;

            }
            public bool VGoSouthEast(){

                MovePlayer(3);
                return true;

            }
            public bool VGoSouth(){

                MovePlayer(4);
                return true;

            }
            public bool VGoSouthWest(){

                MovePlayer(5);
                return true;

            }
            public bool VGoWest(){

                MovePlayer(6);
                return true;

            }
            public bool VGoNorthWest(){

                MovePlayer(7);
                return true;

            }
            public bool VGoUp(){

                MovePlayer(8);
                return true;

            }
            public bool VGoDown(){

            MovePlayer(9);
            return true;

        }
        #endregion
        public bool VInventory(){

            int playerID = GameF.SearchForObject(player);
            int[] heldObjects = GameF.GetHeldObjects(playerID, TypeFlags.CanTake);
            if(heldObjects.Length == 0){

                GameF.Print("You are empty handed.");
                return true;

            }
            else{

                GameF.Print("You are holding:");
                foreach(int i in heldObjects){

                    objects[i].holderID = playerID;
                    GameF.Print("   A " + objects[i].description  + ".");

                }
                return true;

            }
            
        }
        public bool VTake(){

            return true;

        }
        public bool VLook(){

            int currentLocation = GameF.GetObjectHolder(player);
            string description = objects[currentLocation].description;
            GameF.Print(description);
            if(objects[currentLocation].subroutine != null){

                Parser.verbID = Parser.GetWSPairIndex("look", verbs);
                objects[currentLocation].subroutine();

            }
            return true;

        }
        public bool VLookAround(){

            if(InRoom(Parser.directObjectID)){

                VLook();

            }
            return true;

        }
        public bool VLookAt(){

            int currentLocation = GameF.GetObjectHolder(player);
            int objectLocation = objects[Parser.directObjectID].holderID;

            if(Parser.directObjectID == currentLocation){

                if(objects[currentLocation].subroutine != null)return objects[currentLocation].subroutine();
                else return VLook();

            }
            if(!InRoom(Parser.directObjectID)){

                return true;

            }
            if(objects[Parser.directObjectID].subroutine != null && objects[Parser.directObjectID].subroutine())return true;
            else{

                GameF.Print("There's nothing special about the " + objects[Parser.directObjectID].description);
                return true;

            }

        }
        public bool VFight(){

            GameF.Print("You fight the " + Parser.directObjectInput + " with the " + Parser.indirectObjectInput + ".");
            return true;

        }

    #endregion
    #region Object Subroutines
        public bool PlayerF(){

            if(GameF.CompareVerb("look")){

                GameF.Print("That's difficult unless your eyes are prehensible.");
                return true;

            }
            return false;

        }
        public bool SunnyF(){

            if(GameF.CompareVerb("look")){

                GameF.Print("Sunny is a golden cocker spaniel.");
                return true;

            }
            return false;

        }
        public bool BedroomF(){

            if(GameF.CompareVerb("look")){

                GameF.Print("You are in Kyri's bedroom.");
                return true;

            }
            return false;

        }

    #endregion
    #region Other
        
        public void StartGame(){

            VLook();

        }
        private int[] GetPossibleDirections(){

            return objects[GameF.GetObjectHolder(player)].travelTable;

        }
        private void MovePlayer(int direction){

            int[] travelTable = GetPossibleDirections();
            if(travelTable[direction] == -1){

                GameF.Print("You can't go that way.");
                return;

            }
            else{

                objects[GameF.SearchForObject(player)].holderID = travelTable[direction];

            }
            VLook();

        } 
        private bool InRoom(int id){

            int currentLocation = GameF.GetObjectHolder(player);

            if(objects[Parser.directObjectID].holderID == currentLocation)return true;
            else GameF.Print("You can't see any " + Parser.directObjectInput + " here!");
            return false;

        }

    #endregion
    #region Macros

        string player = "me";
        string[] stupid = new string[]{

            "What a concept!",
            "You can't be serious."

        };

    #endregion

}

#region 

    public class Container : ObjectClass{

        public int capacity {get; set;}
        public int[] held {get; set;}

    }

    public class Weapon : ObjectClass{

        public float damage {get; set;}

    }

#endregion