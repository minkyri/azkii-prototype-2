using System.Runtime.Serialization;

[DataContract]
public class GameData{

    #region DO NOT CHANGE

    [DataMember] public string[] articles;
    [DataMember] public string[] conjunctions;
    [DataMember] public WordSynonymPair[] prepositions;
    [DataMember] public WordSynonymPair[] verbs;
    [DataMember] public string[] knownWords;
    [DataMember] public Object[] objects;
    [DataMember] public int[][] objectWordTable;
    [DataMember] public Syntax[] syntaxes;

    [DataMember] public Dictionary<int, string> objectSubroutineDictionary;
    [DataMember] public Dictionary<int, string> syntaxSubroutineDictionary;
    [DataMember] public Dictionary<int, string[]> syntaxDirectObjectFlagDictionary;
    [DataMember] public Dictionary<int, string[]> syntaxIndirectObjectFlagDictionary;

    public GameData(){}

    #endregion

    #region Custom Variables

        [DataMember] string player = "me";
        [DataMember] string[] stupid = new string[]{

            "What a concept!",
            "You can't be serious."

        };

    #endregion
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

            int[] heldObjects = GameF.GetHeldObjects(player);
            if(heldObjects.Length == 0){

                GameF.Print("You are empty handed.");
                return true;

            }
            else{

                GameF.Print("You are holding:");
                foreach(int i in heldObjects){

                    objects[i].holderID = GameF.SearchForObject(player);
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

                Parser.verbID = GameF.GetWSPairIndex("look", verbs);
                objects[currentLocation].subroutine();

            }
            return true;

        }
        public bool VLookAround(){

            VLook();
            return true;

        }
        public bool VLookAt(){

            if(objects[Parser.directObjectID].subroutine != null && objects[Parser.directObjectID].subroutine())return true;
            else{

                GameF.Print("There's nothing special about the " + Parser.directObjectInput);
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
    #region Flags

        public bool Any(int id, bool showMessage){
            
            return true;

        }
        public bool InRoom(int id, bool showMessage){

            bool condition = objects[id].holderID == GameF.GetObjectHolder(player);
            string message = "You can't see any " + Parser.currentInput + " here!";
            return GameF.EvaluateFlag(condition, showMessage, message);

        }
        public bool IsRoom(int id, bool showMessage){

            bool condition = objects[id].holderID == 0;
            string message = "The " + Parser.currentInput + " is not a room.";
            
            return GameF.EvaluateFlag(condition, showMessage, message);

        }
        public bool Carried(int id, bool showMessage){

            bool condition = objects[id].holderID == GameF.SearchForObject(player);
            string message = "You don't have the " + Parser.currentInput + "!";
            
            return GameF.EvaluateFlag(condition, showMessage, message);

        }

    #endregion 
    #region Object Classes

        public class Container : ObjectClass{

            public int capacity {get; set;}
            public int[] held {get; set;}

        }

         public class Weapon : ObjectClass{

            public float damage {get; set;}

        }

    #endregion
    #region Other
        
        //Move to game functions!

        public void StartGame(){

            VLook();

        }
        public void ReturnToGame(){

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

    #endregion

}


