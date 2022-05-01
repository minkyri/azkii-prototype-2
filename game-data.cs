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

    //Everything below will be different for different text adventures

    //Object subroutines are marked with an F at the end, for example: ControlPanelF
    //Verb subroutines are marked with a V at the end, for example: LookV
    //These routines should return true if they have written to the console, and false if not (in most cases)

    #region Custom Variables

        [DataMember] string player = "me";
        [DataMember] string doors = "doors";
        [DataMember] bool scatScrubInEastToilet = false;
        [DataMember] bool scatScrubInWestToilet = false;
        [DataMember] bool isHulk = false;
        [DataMember] string[] stupid = new string[]{

            "What a concept!",
            "You can't be serious.",
            "You're kidding...right?"

        };
        [DataMember] bool timerStarted = false;
        [DataMember] DateTime timeStarted;
        [DataMember] DateTime exitTime;
        [DataMember] TimeSpan timeChange;
        [DataMember] TimeSpan timeLeft = TimeSpan.FromMinutes(30);

    #endregion
    #region Verb Subroutines

        public bool GoV(){

            string direction = objects[Parser.directObjectID].name;

            switch(direction){

                case "north":
                    GoNorthV();
                    break;
                case "northeast":
                    GoNorthEastV();
                    break;
                case "east":
                    GoEastV();
                    break;
                case "southeast":
                    GoSouthEastV();
                    break;
                case "south":
                    GoSouthV();
                    break;
                case "southwest":
                    GoSouthWestV();
                    break;
                case "west":
                    GoWestV();
                    break;
                case "northwest":
                    GoNorthWestV();
                    break;
                case "up":
                    GoUpV();
                    break;
                case "down":
                    GoDownV();
                    break;
                default:
                    
                    if(GameF.TryGetObjectClass<Door>(Parser.directObjectID, out Door doorObject)){

                        GetInV();
                        return true;

                    }
                    int playerHolder = GameF.GetObjectHolder(player);
                    if(playerHolder == Parser.directObjectID){

                        GameF.Print("You're already in the " + objects[Parser.directObjectID].description);
                        return true;

                    }
                    if(GameF.TryGetObjectClass<Room>(playerHolder, out Room roomObject)){

                        int[] travelTable = roomObject.GetTravelTable();
                        for(int i = 0; i < travelTable.Length; i++){

                            if(Parser.directObjectID == travelTable[i]){

                                Travel(i);
                                return true;

                            }

                        }

                    }

                    GameF.Print("You should supply a direction!");
                    break;

            }

            return true;

        }
        public bool GoNorthV(){

            Travel(0);
            return true;

        }
        public bool GoNorthEastV(){

            Travel(1);
            return true;

        }
        public bool GoEastV(){

            Travel(2);
            return true;

        }
        public bool GoSouthEastV(){

            Travel(3);
            return true;

        }
        public bool GoSouthV(){

            Travel(4);
            return true;

        }
        public bool GoSouthWestV(){

            Travel(5);
            return true;

        }
        public bool GoWestV(){

            Travel(6);
            return true;

        }
        public bool GoNorthWestV(){

            Travel(7);
            return true;

        }
        public bool GoUpV(){

            Travel(8);
            return true;

        }
        public bool GoDownV(){

            Travel(9);
            return true;

        }
        public bool InventoryV(){

            int playerID = GameF.SearchForObject(player);
            Dictionary<int, int> hierarchy = GameF.GetObjectHierarchy(playerID);

            if(hierarchy.Count == 0){

                GameF.Print("You are not carrying anything.");

            }
            else{

                GameF.Print("You are carrying:");
                DisplayObjectTree(playerID, hierarchy, true, null);

            }

            return true;

        }
        public bool LookV(){

            int currentLocation = GameF.GetObjectHolder(player);

            if(currentLocation == -1){

                GameF.Print("I don't know what you did, but you are literally nowhere...");

            }
            else if(GameF.TryGetObjectClass<Room>(currentLocation, out Room currentRoom)){

                GameF.Print(objects[currentLocation].description);

                if(!currentRoom.visited || GameF.CompareVerb("look")){

                    Parser.verbID = GameF.GetWSPairIndex("look", verbs);
                    Parser.preposition1ID = GameF.GetWSPairIndex("at", prepositions);
                    objects[currentLocation].subroutine();

                }

            }
            else if(objects[currentLocation].subroutine != null){

                Parser.verbID = GameF.GetWSPairIndex("look", verbs);
                Parser.preposition1ID = GameF.GetWSPairIndex("at", prepositions);
                objects[currentLocation].subroutine();

            }

            Dictionary<int, int> hierarchy = GameF.GetObjectHierarchy(currentLocation); 

            int[] doorIDs = GameF.GetHeldObjects(GameF.SearchForObject(doors));

            if(doorIDs != null && doorIDs.Length != 0){

                for(int i = 0; i < doorIDs.Length; i++){

                    if(GameF.TryGetObjectClass<Door>(doorIDs[i], out Door doorObject)){

                        if(
                            
                            (doorObject.roomIn == currentLocation || doorObject.roomOut == currentLocation)

                        ){

                            Dictionary<int, int> doorHierarchy = GameF.GetObjectHierarchy(doorIDs[i]);
                            hierarchy.Add(doorIDs[i], 0);
                            foreach(KeyValuePair<int, int> kvp in doorHierarchy){

                                hierarchy.Add(kvp.Key, kvp.Value+1);

                            }

                        }

                    }

                }

            }

            if(hierarchy.Count == 0){

                GameF.Print("There is nothing here.");

            }
            else{

                DisplayObjectTree(currentLocation, hierarchy, false, null);

            }

            return true;

        }
        public bool LookAtV(){

            Dictionary<int, int> hierarchy = GameF.GetObjectHierarchy(Parser.directObjectID); 

            if(hierarchy.Count == 0 || (hierarchy.Count == 1 && hierarchy.ContainsKey(GameF.SearchForObject(player)))){

                GameF.Print("There's nothing special about the " + objects[Parser.directObjectID].description + ".");

            }
            else{

                GameF.Print("The " + objects[Parser.directObjectID].description + " contains:");
                DisplayObjectTree(Parser.directObjectID, hierarchy, true, null);

            }

            return true;

        }
        public bool LookInsideV(){

            Dictionary<int, int> hierarchy = GameF.GetObjectHierarchy(Parser.directObjectID); 

            if(hierarchy.ContainsKey(GameF.SearchForObject(player))){

                GameF.Print("You're in it!");

            }
            else if(GameF.TryGetObjectClass<Open>(Parser.directObjectID, out Open openObject) && !openObject.open){

                GameF.Print("The " + objects[Parser.directObjectID].description + " is closed!");

            }
            else if(GameF.TryGetObjectClass<InContainer>(Parser.directObjectID, out InContainer inContainerObject)){

                int heldObjects = GameF.GetHeldObjects(Parser.directObjectID).Length;
                if(heldObjects == 0 || (heldObjects == 1 && hierarchy.ContainsKey(GameF.SearchForObject(player)))){

                    GameF.Print("The " + Parser.directObjectInput + " is empty.");

                }
                else{

                    GameF.Print("The " + objects[Parser.directObjectID].description + " contains:");
                    DisplayObjectTree(Parser.directObjectID, hierarchy, true, typeof(InContainer));

                }

            }
            else{

                GameF.Print("You won't find anything in the " + objects[Parser.directObjectID].description + ".");

            }
            
            return true;

        }
        public bool LookOnV(){

            Dictionary<int, int> hierarchy = GameF.GetObjectHierarchy(Parser.directObjectID); 

            // else if(GameF.TryGetObjectClass<Open>(Parser.directObjectID, out Open openObject) && !openObject.open){

            //     GameF.Print("The " + objects[Parser.directObjectID].description + " is closed!");

            // }
            if(GameF.TryGetObjectClass<OnContainer>(Parser.directObjectID, out OnContainer onContainerObject)){

                int heldObjects = GameF.GetHeldObjects(Parser.directObjectID).Length;
                if(heldObjects == 0 || (heldObjects == 1 && hierarchy.ContainsKey(GameF.SearchForObject(player)))){

                    GameF.Print("There's nothing on the " + Parser.directObjectInput);

                }
                else{

                    GameF.Print("Sitting on the  " + objects[Parser.directObjectID].description + " is:");
                    DisplayObjectTree(Parser.directObjectID, hierarchy, true, typeof(OnContainer));

                }

            }
            else{

                GameF.Print("You won't find anything on the " + Parser.directObjectInput);

            }

            return true;

        }
        public bool FlushV(){

            GameF.Print("You can't flush the " + objects[Parser.directObjectID].description + ".");
            return true;

        }
        public bool OpenV(){

            if(!GameF.TryGetObjectClass<Open>(Parser.directObjectID, out Open openObject)){

                GameF.Print("You can't open a " + objects[Parser.directObjectID].description + ".");

            }
            else if(openObject.locked){

                GameF.Print("The " + objects[Parser.directObjectID].description + " is locked!");

            }
            else if(openObject.open){

                GameF.Print("The " + objects[Parser.directObjectID].description + " is already open!");

            }
            else{

                openObject.open = true;
                GameF.Print("You opened the " + objects[Parser.directObjectID].description + ".");

                Dictionary<int, int> hierarchy = GameF.GetObjectHierarchy(Parser.directObjectID);

                int playerID = GameF.SearchForObject(player);
                bool playerInside = hierarchy.ContainsKey(playerID);
                int min = 0;
                if(playerInside)min = 1;
                
                if(GameF.GetHeldObjects(Parser.directObjectID).Length > min){

                    if(GameF.TryGetObjectClass<InContainer>(Parser.directObjectID, out InContainer inObject)){

                        GameF.Print("The " + objects[Parser.directObjectID].description + " contains:");
                        DisplayObjectTree(Parser.directObjectID, hierarchy, true, typeof(InContainer));

                    }
                    if(GameF.TryGetObjectClass<OnContainer>(Parser.directObjectID, out OnContainer onObject)){

                        GameF.Print("Sitting on the " + objects[Parser.directObjectID].description + " is:");
                        DisplayObjectTree(Parser.directObjectID, hierarchy, true, typeof(OnContainer));

                    }

                }

            }
            return true;

        }
        public bool CloseV(){

            if(!GameF.TryGetObjectClass<Open>(Parser.directObjectID, out Open openObject)){

                GameF.Print("You can't close a " + objects[Parser.directObjectID].description);

            }
            else if(!openObject.open){

                GameF.Print("The " + objects[Parser.directObjectID].description + " is already closed!");

            }
            else{

                openObject.open = false;
                GameF.Print("You closed the " + objects[Parser.directObjectID].description + ".");

            }
            return true;

        }
        public bool TakeV(){

            int playerID = GameF.SearchForObject(player);

            if(IsAccessible(Parser.directObjectID, playerID)){

                GameF.Print("You already have the " + objects[Parser.directObjectID].description);

            }
            else if(GameF.TryGetObjectClass<CanTake>(Parser.directObjectID, out CanTake canTake)){

                GameF.MoveObject(Parser.directObjectID, playerID);
                GameF.Print("You took the " + objects[Parser.directObjectID].description);

            }
            else if(GameF.TryGetObjectClass<TryTake>(Parser.directObjectID, out TryTake tryTake)){

                GameF.Print("It is securely anchored.");

            }
            else{

                GameF.Print(stupid[GameF.random.Next(0, stupid.Length)]);

            }

            return true;

        }
        public bool TakeAllV(){

            int playerID = GameF.SearchForObject(player);
            int playerHolder = GameF.GetObjectHolder(player);
            Dictionary<int, int> hierarchy = GameF.GetObjectHierarchy(playerHolder);

            int[] allObjects = new int[hierarchy.Count];
            int takeCount = 0;

            for(int i = 0; i < hierarchy.Count; i++){

                int id = hierarchy.ElementAt(i).Key;

                if(IsAccessible(id, playerHolder) && !Has(id, false)){

                    if(GameF.TryGetObjectClass<CanTake>(id, out CanTake canTake)){

                        GameF.MoveObject(id, playerID);
                        GameF.Print(objects[id].description + ": Taken.");
                        takeCount++;

                    }
                    else if(GameF.TryGetObjectClass<TryTake>(id, out TryTake tryTake)){

                        GameF.Print(objects[id].description + ": It is securely anchored.");
                        takeCount++;

                    }

                }

            }

            if(takeCount == 0){

                GameF.Print("There is nothing to take.");

            }

            return true;

        }
        public bool CheckV(){

            GameF.Print("It's doing just fine.");
            return true;

        }
        public bool PushV(){

            GameF.Print("Pushing the " + objects[Parser.directObjectID].description + " doesn't seem to do anything.");
            return true;

        }
        public bool SwipeV(){

            GameF.Print("Swiping the " + objects[Parser.directObjectID].description + " against the " + objects[Parser.indirectObjectID].description + " doesn't seem to work.");
            return true;

        }
        public bool ThrowV(){

            bool droppedOnHead = GameF.Chance(15);
            if(droppedOnHead){

                GameF.Print("What luck! You threw the " + objects[Parser.directObjectID].description + " up into the air, and hit you square on the head, " + 
                "killing you instantly. Next time don't throw things about like an idiot!");
                Death();

            }
            else{

                GameF.MoveObject(Parser.directObjectID, GameF.GetObjectHolder(player));
                GameF.Print("You threw the " + objects[Parser.directObjectID].description + ".");

            }
            
            return true;

        }
        public bool ThrowAtV(){

            bool droppedOnHead = GameF.Chance(15);
            if(droppedOnHead){

                GameF.Print("What luck! You threw the " + objects[Parser.directObjectID].description + " up into the air, and hit you square on the head, " + 
                "killing you instantly. Next time don't throw things about like an idiot!");

            }
            else{

                GameF.MoveObject(Parser.directObjectID, GameF.GetObjectHolder(player));
                GameF.Print("You threw the " + objects[Parser.directObjectID].description + ".");

            }
            //put on the ground?
            return true;

        }
        public bool BreakV(){

            GameF.Print("Seems a bit unnecessary don't you think?");
            return true;

        }
        public bool PourV(){

            if(Parser.directObjectID == GameF.SearchForObject("scatscrub")){

                GameF.Print("The ScatScrub 3000 burns a hole straight through the " + objects[Parser.directObjectID].description + ".");

            }
            else{

                GameF.Print("You can't pour that.");

            }
            return true;

        }
        public bool GetInV(){

            int currentLocation = GameF.GetObjectHolder(player);
            if(currentLocation == Parser.directObjectID){

                GameF.Print("You are already in the " + objects[Parser.directObjectID].description + ".");
                return true;

            }
            if(GameF.TryGetObjectClass<Open>(Parser.directObjectID, out Open openObject)){

                if(openObject.locked){

                    GameF.Print("Its locked!");
                    return true;

                }
                if(!openObject.open){

                    GameF.Print("Its closed!");
                    return true;

                }

            }
            if(GameF.TryGetObjectClass<GetIn>(Parser.directObjectID, out GetIn getInObject)){

                GameF.Print("You get into the " + objects[Parser.directObjectID].description + ".");
                GameF.MoveObject(GameF.SearchForObject(player), Parser.directObjectID);
                LookV();
                return true;

            }
            if(GameF.TryGetObjectClass<Door>(Parser.directObjectID, out Door doorObject)){

                GameF.Print("You go through the " + objects[Parser.directObjectID].description + "\n");
                if(doorObject.roomIn == currentLocation){

                    GameF.MoveObject(GameF.SearchForObject(player), doorObject.roomOut);

                }
                if(doorObject.roomOut == currentLocation){

                    GameF.MoveObject(GameF.SearchForObject(player), doorObject.roomIn);

                }
                
                LookV();
                return true;

            }

            GameF.Print("You hit your head on the " + objects[Parser.directObjectID].description + " as you attempt this ridiculous feat.");
            return true;

        }
        public bool GetOutV(){

            if(objects[GameF.SearchForObject(player)].holderID != Parser.directObjectID){

                GameF.Print("You are not in the " + objects[Parser.directObjectID].description + ".");
                return true;

            }

            if(GameF.TryGetObjectClass<Open>(Parser.directObjectID, out Open openObject)){

                if(openObject.locked){

                    GameF.Print("Its locked!");
                    return true;

                }
                if(!openObject.open){

                    GameF.Print("Its closed!");
                    return true;

                }

            }

            int holder = objects[Parser.directObjectID].holderID;
            if(holder != -1 && GameF.TryGetObjectClass<GetIn>(Parser.directObjectID, out GetIn getInObject)){

                GameF.MoveObject(GameF.SearchForObject(player), holder);
                GameF.Print("You get out of the " + objects[Parser.directObjectID].description + "\n");
                LookV();

            }
            else{

                GameF.Print("You can't get out of the " + objects[Parser.directObjectID].description);

            }

            return true;

        } 
        public bool UnscrewV(){

            GameF.Print("You can't remove the " + objects[Parser.directObjectID].description + " with the " + objects[Parser.indirectObjectID].description);
            return true;

        }
        public bool DropV(){

            GameF.MoveObject(Parser.directObjectID, GameF.GetObjectHolder(player));
            GameF.Print("You dropped the " + objects[Parser.directObjectID].description + ".");
            return true;

        }
        public bool PutInV(){

            if(!GameF.TryGetObjectClass<InContainer>(Parser.indirectObjectID, out InContainer container) && IsAccessible(Parser.indirectObjectID, GameF.GetObjectHolder(player))){

                GameF.Print("You can't put things into the " + objects[Parser.indirectObjectID].description + ".");

            }
            else {
                
                GameF.MoveObject(Parser.directObjectID, Parser.indirectObjectID);
                GameF.Print("You put the " + objects[Parser.directObjectID].description + " into the " + objects[Parser.indirectObjectID].description + ".");

            }

            return true;

        }
        public bool PutOnV(){

            if(!GameF.TryGetObjectClass<OnContainer>(Parser.indirectObjectID, out OnContainer container) && IsAccessible(Parser.indirectObjectID, GameF.GetObjectHolder(player))){

                GameF.Print("There is no good surface on the " + objects[Parser.indirectObjectID].description + " to put things on.");

            }
            else {
                
                GameF.MoveObject(Parser.directObjectID, Parser.indirectObjectID);
                GameF.Print("You put the " + objects[Parser.directObjectID].description + " on the " + objects[Parser.indirectObjectID].description + ".");

            }

            return true;

        }
        public bool EatV(){

            GameF.Print("I don't think the " + objects[Parser.directObjectID].description + " would agree with you.");
            return true;

        }
        public bool ReadV(){

            GameF.Print("There is nothing on the " + objects[Parser.directObjectID].description + "to read.");
            return true;

        }
        public bool FindV(){

            int playerHolder = GameF.GetObjectHolder(player);
            int playerID = GameF.SearchForObject(player);

            if(playerID == Parser.directObjectID){

                GameF.Print("You're around here somewhere...");

            }
            else if(playerHolder == Parser.directObjectID){

                GameF.Print("You're inside the " + objects[Parser.directObjectID].description + "!");

            }
            else if(objects[Parser.directObjectID].holderID == playerID){

                GameF.Print("You have it!");

            }
            else if(IsAccessible(Parser.directObjectID, playerHolder)){

                int holderID = objects[Parser.directObjectID].holderID;
                if(objects[Parser.directObjectID].holderID == playerHolder || objects[Parser.directObjectID].holderID == GameF.SearchForObject(doors)){

                    GameF.Print("It's right here!");

                }
                else if(GameF.TryGetObjectClass<OnContainer>(holderID, out OnContainer onObject)){

                    GameF.Print("It's on the " + objects[holderID].description + ".");

                }
                else{

                    GameF.Print("It's in the " + objects[holderID].description + ".");

                }
                
            }
            else{

                GameF.Print("You can't see any " + Parser.directObjectInput + " here.");

            }

            return true;

        }
        public bool TouchV(){

            GameF.Print("Fiddling with the " + objects[Parser.directObjectID].description + " doesn't seem to do anything.");
            return true;

        }
        public bool FixV(){

            GameF.Print("This has no effect.");
            return true;

        }
        public bool ShakeV(){

            GameF.Print("Shaking the " + objects[Parser.directObjectID].description + " doesn't seem to accomplish anything.");
            return true;

        }
        public bool WashV(){

            GameF.Print("Washing the " + objects[Parser.directObjectID].description + " " + prepositions[Parser.preposition2ID].word + " the " + objects[Parser.indirectObjectID].description + " doesn't seem to do anything.");
            return true;

        }
        public bool DigV(){

            GameF.Print("You can't dig there.");
            return true;

        }
        public bool UnlockV(){

            if(GameF.TryGetObjectClass<Open>(Parser.directObjectID, out Open openObject)){

                if(!openObject.locked){

                    GameF.Print("The " + objects[Parser.directObjectID].description + " is not locked.");

                }
                else{

                    GameF.Print("The " + objects[Parser.indirectObjectID].description + " does not unlock the " + objects[Parser.directObjectID].description + ".");

                }

                return true;

            }

            GameF.Print("The " + objects[Parser.directObjectID].description + " does not have a lock on it.");

            return true;

        }

    #endregion
    #region Object Subroutines
        
        #region Rooms

            public bool PlayerF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("That's going to be difficult unless your eyes are prehensible.");
                    return true;

                }
                else if(GameF.CompareVerb("take")){

                    GameF.Print("How romantic!");
                    return true;

                }
                return false;

            }
            public bool CryoRoomF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    // GameF.Print("Everything is dark apart from the white light emitted from the cryo pods either side of the room. " + 
                    // "The walls, floors and ceiling are all metallic and various pipes and wires run along the corners of the room.");

                    GameF.Print("You are standing in a dark room with two cryogenic pods. The pod you woke up in is red, and there is another blue pod that sits opposite it. There is a hallway to the south.");
                    return true;

                }
                return false;

            }
            public bool WestHallwayF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("You are standing in a hallway that leads north, east and west. Your bare feet are cold from the metallic floor.");

                    return true;

                }
                return false;

            }
            public bool WestBedroomF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("This is a relativly small bedroom with futuristic-looking bed and set of drawers. North of this room is a bathroom, and to the east is a hallway.");
                    return true;

                }
                return false;

            }
            public bool WestBathroomF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("You are standing in a bathroom that smells like it has been recently used...");
                    return true;

                }
                return false;

            }
            public bool ControlRoomF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("You are standing in a large open space with a huge control panel in the center. It is all dark except for the various flickering lights of the control panel. There are exits on all four sides of the room.");
                    GameF.Print("");
                    ControlPanelF();

                    if(!timerStarted){

                        GameF.Print("Above the control panel is a large screen with a timer reading 30:00." + "\n" + 
                        "As you take a step forward, the timer begins counting down...\nIt would probably be a good idea to check on it every now and again...");

                        timeStarted = DateTime.Now;
                        timerStarted = true;

                    }
                    else{

                        Parser.verbID = GameF.GetWSPairIndex("check", verbs);
                        TimerF();

                    }

                    return true;

                }
                return false;

            }
            public bool GreenhouseF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("You are in a small greenhouse-like enviroment, with some plants nearby.");
                    return true;

                }
                return false;

            }
            public bool EastHallwayF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("This hallway is almost identical to the west hallway, except that the window has been boarded up, and there are planks nailed to it.");
                    return true;

                }
                return false;

            }
            public bool CargoBayF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("Strangely, there is hardly any cargo here except for one large cargo box sits in the centre of the room.");
                    return true;

                }
                return false;

            }
            public bool EastBedroomF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("You are standing in a bedroom similar to the west bedroom, with a bed and a set of drawers.");
                    return true;

                }
                return false;

            }
            public bool EastBathroomF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("This bathroom is identical to the west bathroom, except for a small vent on the east wall of the room");

                    VentF();

                    return true;

                }
                return false;

            }
            public bool JanitorsOfficeF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The room you are standing in is extremely different from the previous rooms you have been in. " + 
                    "The walls are made of brick instead of metal, and the floor is made from a grey concrete. It looks nothing like a spaceship room...");
                    return true;

                }
                return false;

            }

        #endregion
        #region Objects

            public bool FirstPodF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    if(GameF.GetObjectHolder(player) == GameF.SearchForObject("first pod")){

                        GameF.Print("You seem to be in some sort of cryogenic pod. There is no window on the pod.");

                    }
                    else{

                        GameF.Print("This pod is painted red and is lying almost vertically against the wall, and is just tall enough to fit you in. " +  
                        "Strangely, there are no wires or pipes running through it...");
                        
                    }
                    return true;
                    
                }
                if(GameF.CompareVerb("touch")){

                    GameF.Print("The pod feels like cheap plastic...");
                    return true;

                }
                return false;

            }
            public bool SecondPodF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    if(GameF.GetObjectHolder(player) == GameF.SearchForObject("second pod")){

                        GameF.Print("You are in a cryogenic pod. There is no window on the pod.");

                    }
                    else{

                        GameF.Print("This pod is painted blue and is lying flat on the ground.");
                        
                    }
                    return true;

                }
                if(GameF.CompareVerb("touch")){

                    GameF.Print("The pod feels like cheap plastic...");
                    return true;

                }
                return false;

            }
            public bool WindowF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1("outside") || GameF.ComparePreposition1(""))){

                    GameF.Print("The window is large, circular and extrudes outwards. You take a closer look and see the distant gleaming of stars...AND EARTH! YOU'RE IN SPACE!");
                    return true;

                }
                if(GameF.CompareVerb("open")){

                    GameF.Print("Look outside the window and you'll see why that's a dumb idea...");
                    return true;

                }
                return false;

            }
            public bool WestBedF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The bed's frame is bright white and metallic, and the pillows and matress are greyish in colour.");
                    return true;

                }
                return false;

            }
            public bool WestDrawersF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The drawers are metallic and match the white-grey colours of the bed next to it");
                    return true;

                }
                return false;

            }
            public bool WestToiletF(){

                GameF.TryGetObjectClass<Open>(GameF.SearchForObject("west loo"), out Open openObject);
                if(GameF.CompareVerb("look")){

                    if(GameF.ComparePreposition1("in")){

                        if(openObject != null && openObject.open){

                            if(openObject.open){

                                GameF.Print("The inside of the toilet appears to have some...stains.");
                                return false;

                            }
                            else{

                                GameF.Print("You'll have to open it first.");
                                return true;

                            }
                        
                        }

                    }
                    if(GameF.GetObjectHolder(player) == GameF.SearchForObject("west dirty toilet")){

                        GameF.Print("Lets just say it's not a particularly clean toilet...");
                        return true;

                    }

                }
                if(GameF.CompareVerb("touch")){

                    GameF.Print("OH NOW YOU'VE GONE AND GOT POO ALL OVER YOUR HANDS!!");
                    return true;

                }
                if(GameF.CompareVerb("flush")){

                    int westToilet = GameF.SearchForObject("west toilet");
                    int eastToilet = GameF.SearchForObject("east toilet");
                    bool powerCoreInPipes = GameF.GetObjectHolder("blue power core") == -1;
                    int[] held = GameF.GetHeldObjects(westToilet);

                    if(scatScrubInWestToilet && powerCoreInPipes){

                        GameF.Print("The ScatScrub3000 in the toilet begins dissolving everything in it's path. You hear something drop in the pipes and get flushed away...");
                        GameF.MoveObject(GameF.SearchForObject("blue power core"), eastToilet);
                        scatScrubInWestToilet = false;
                        scatScrubInEastToilet = false;

                    }

                    if(held.Length > 0){

                        foreach(int id in held){

                            GameF.MoveObject(id, eastToilet);

                        }

                        if(held.Contains(GameF.SearchForObject(player))){

                            if(powerCoreInPipes)GameF.Print("You flush the toilet and start spinning as you get sucked into the pipes below. As you get sucked in, you see a blue device stuck on the pipes. You hit your head on a few pipes before emerging from another toilet.");
                            else GameF.Print("You flush the toilet and start spinning as you get sucked into the pipes below. You hit your head on a few pipes before emerging from another toilet.");
                            LookV();

                        }
                        else{

                            GameF.Print("You flushed everything down the toilet.");

                        }

                    }
                    else{

                        GameF.Print("You flushed the toilet.");

                    }

                    return true;

                }
                return false;

            }
            public bool WestCabinetF(){

                return false;

            }
            public bool WestSinkF(){

                return false;

            }
            public bool TimerF(){

                if((GameF.CompareVerb("look") || GameF.CompareVerb("check"))){

                    GameF.Print("The timer reads " + timeLeft.Minutes + ":" + timeLeft.Seconds + ".");
                    return true;
                    
                }
                return false;

            }
            public bool ControlPanelF(){

                int controlPanelID = GameF.SearchForObject("control panel");

                if((GameF.CompareVerb("put") || GameF.CompareVerb("throw")) && Parser.indirectObjectID == controlPanelID){

                    if(

                        Parser.directObjectID == GameF.SearchForObject("red power core") ||
                        Parser.directObjectID == GameF.SearchForObject("green power core") ||
                        Parser.directObjectID == GameF.SearchForObject("blue power core")
                        
                    ){

                        GameF.Print("You plug the " + objects[Parser.directObjectID].description + " into the control panel.");
                        GameF.MoveObject(Parser.directObjectID, controlPanelID);
                        return true;

                    }
                    else{

                        GameF.Print("The " + objects[Parser.directObjectID].description + " doesn't fit in any of the slots of the control panel.");
                        return true;

                    }

                }
                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    int[] held = GameF.GetHeldObjects(controlPanelID);
                    bool redHere = held.Contains(GameF.SearchForObject("red power core"));
                    bool greenHere = held.Contains(GameF.SearchForObject("green power core"));
                    bool blueHere = held.Contains(GameF.SearchForObject("blue power core"));

                    if(!(redHere && greenHere && blueHere)){

                        GameF.Print("The north, east and west faces of the control panel each have a round slot, " + 
                        "presumably for some kind drive to be placed in them.");

                    }
                    
                    return true;

                }
                return false;

            }
            public bool TomatoPlantF(){

                if(GameF.CompareVerb("dig")){

                    GameF.Print("You begin digging in the tomato plant bed, and find a lockbox key!");
                    objects[GameF.SearchForObject("lockbox key")].holderID = GameF.SearchForObject(player);

                    return true;

                }
                return false;

            }
            public bool CompostHeapF(){

                if(GameF.CompareVerb("dig")){

                    GameF.Print("You begin digging in the compost heap, but find nothing.");
                    return true;

                }
                return false;

            }
            public bool BoardedWindowF(){

                if(GameF.CompareVerb("break") && Parser.indirectObjectID == GameF.SearchForObject("hammer")){

                    GameF.Print("You swing the sledgehammer at the wooden planks on the wall. It makes a small dent. You swing again, and again, and again. Chunks of wood fly everywhere. There is light! You keep hacking away as more and more light comes through. You go for one final and powerful swing, but trip over and go head first through the wall.");
                    GameF.Print("You feel dazed. You open your eyes and look around. You are in some sort of reception area, and a banner over a door in the corner reads \"Escape World London\". There is a woman behind the desk, and she is looking at you angrily.");
                    GameF.Print("RECEPTIONIST WOMAN: YOU IDIOT. Breaking through the wall with a sledgehammer is NOT PART OF THE ESCAPE ROOM!");
                    GameF.Print("She calls the police, and you feel the grip of the sledgehammer in your hand...");
                    Win();

                }
                return false;

            }
            public bool CargoBoxF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The cargo box is large and made of metal. It is white with black stripes running across it. There doesn't seem to be any " + 
                    "way to open it.");
                    return true;

                }
                if(GameF.CompareVerb("break")){

                    if(!isHulk){

                        GameF.Print("You are not strong enough to break the cargo box.");
                    
                    }
                    else{

                        GameF.Print("HUULLKKKKK SMAAASSSHHHHH!!!!");
                        GameF.Print("You use your insane strength as the hulk to completely obliterate the box. You found a green power core inside!");
                        objects[GameF.SearchForObject("green power core")].holderID = GameF.SearchForObject(player);
                        objects[GameF.SearchForObject("cargo box")].holderID = -1;

                    }

                    return true;

                }
                return false;

            }
            public bool EastBedF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The bed's frame is dark, black and metallic, and the pillows and matress are white in colour.");
                    return true;

                }
                return false;

            }
            public bool EastDrawersF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The drawers are metallic and match the colours of the bed next to it");
                    return true;

                }
                return false;

            }
            public bool EastToiletF(){

                GameF.TryGetObjectClass<Open>(GameF.SearchForObject("east loo"), out Open openObject);
                if(GameF.CompareVerb("look")){

                    if(GameF.ComparePreposition1("in")){

                        if(openObject != null && openObject.open){

                            if(openObject.open){

                                GameF.Print("Looks like someone didn't flush...");
                                return false;

                            }
                            else{

                                GameF.Print("You'll have to open it first.");
                                return true;

                            }
                        
                        }

                    }
                    if(GameF.GetObjectHolder(player) == GameF.SearchForObject("east dirty toilet")){

                        GameF.Print("Lets just say it's not a particularly clean toilet...");
                        return true;

                    }

                }
                if(GameF.CompareVerb("touch")){

                    GameF.Print("OH NOW YOU'VE GONE AND GOT POO ALL OVER YOUR HANDS!!");
                    return true;

                }
                if(GameF.CompareVerb("flush")){

                    int westToilet = GameF.SearchForObject("west toilet");
                    int eastToilet = GameF.SearchForObject("east toilet");
                    bool powerCoreInPipes = GameF.GetObjectHolder("blue power core") == -1;
                    int[] held = GameF.GetHeldObjects(eastToilet);

                    if(scatScrubInEastToilet && powerCoreInPipes){

                        GameF.Print("The ScatScrub3000 in the toilet begins dissolving everything in it's path. You hear something drop in the pipes and get flushed away...");
                        GameF.MoveObject(GameF.SearchForObject("blue power core"), westToilet);
                        scatScrubInWestToilet = false;
                        scatScrubInEastToilet = false;

                    }

                    if(held.Length > 0){

                        foreach(int id in held){

                            GameF.MoveObject(id, westToilet);

                        }

                        if(held.Contains(GameF.SearchForObject(player))){

                            GameF.Print("You flush the toilet and start spinning as you get sucked into the pipes below. As you get sucked in, you see a blue device stuck on the pipes. You hit your head on a few pipes before emerging from another toilet.");
                            LookV();

                        }
                        else{

                            GameF.Print("You flushed everything down the toilet.");

                        }

                    }
                    else{

                        GameF.Print("You flushed the toilet.");

                    }

                    return true;

                }

                return false;

            }
            public bool EastCabinetF(){

                return false;

            }
            public bool EastSinkF(){

                return false;

            }
            public bool VentF(){

                int ventID = GameF.SearchForObject("sussy vent");

                bool squareScrew = GameF.GetObjectHolder("square bolt") == ventID;
                bool triangleScrew = GameF.GetObjectHolder("triangle bolt") == ventID;
                bool starScrew = GameF.GetObjectHolder("star bolt") == ventID;

                if(!squareScrew && !triangleScrew && !starScrew){

                    if(GameF.TryGetObjectClass<Open>(ventID, out Open opentObject)){

                        opentObject.locked = false;
                        opentObject.open = true;

                    }

                }

                if(GameF.CompareVerb("look")){

                    

                    if(squareScrew && triangleScrew && starScrew){

                        GameF.Print("The vent is bolted to the wall with three screws, each with a different shape. One screw is sqaure, the other triangular, and the last " + 
                        "has a star shape.");

                    }
                    else{

                        GameF.Print("The vent is made of metal and just wide enough to fit you in.");

                    }
                    return true;

                }
                if((GameF.CompareVerb("put") || GameF.CompareVerb("throw")) && Parser.indirectObjectID == ventID){

                    if(

                        Parser.directObjectID == GameF.SearchForObject("square bolt") ||
                        Parser.directObjectID == GameF.SearchForObject("triangle bolt") ||
                        Parser.directObjectID == GameF.SearchForObject("star bolt")
                        
                    ){

                        GameF.Print("You literally just took it off. Why on earth would you want to put it back on??");
                        return true;

                    }
                    else{

                        GameF.Print("It won't fit.");
                        return true;

                    }

                }
                return false;

            }
            public bool TableF(){

                return false;

            }
            public bool SledgehammerF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The sledgehammer has a sturdy wooden handle with a metallic hammerhead on the end, painted red.");
                    return true;

                }
                return false;

            }
            public bool WestBayDoorF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The bay door is large and made of metal.");
                    return true;

                }
                return false;

            }
            public bool NorthBayDoorF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The bay door is large and made of metal.");
                    return true;

                }
                return false;

            }
            public bool BlastDoorF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("By far the largest door in the ship, the blast door takes up the whole south wall of the control room.");
                    return true;

                }
                return false;

            }
            public bool SkeletonF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The skeleton doesn't seem human. It has a portly appearance, but the top half of it's body is missing, with a single bone sticking upwards from it's body.");
                    return true;

                }
                if(GameF.CompareVerb("touch")){

                    GameF.Print("The bones of the skeleton feels like plastic.");
                    return true;

                }
                return false;

            }
            public bool BrokenKeycardF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("There seems to be a photo on the keycard, but its difficult to see the whole image because the card is in pieces.");
                    return true;

                }
                return false;

            }
            public bool KeycardF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The photo on the card shows an image of some creature wearing a red, full-body spacesuit with a visor.");
                    return true;

                }
                return false;

            }
            public bool GlueF(){

                if((GameF.CompareVerb("glue") || GameF.CompareVerb("fix")) && Parser.directObjectID == GameF.SearchForObject("broken keycard")){

                    GameF.Print("You apply the glob of glue to the edges of the keycard pieces and stick them together. This isn't going to hold for long.");
                    objects[GameF.SearchForObject("glue glob")].holderID = -1;
                    objects[GameF.SearchForObject("broken keycard")].holderID = -1;
                    objects[GameF.SearchForObject("fixed keycard")].holderID = GameF.SearchForObject(player);
                    return true;

                }
                return false;

            }
            public bool HulkBulkF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The box of HulkBulk has a stretched, low quality image of the Hulk. The box reads: " + "\n" + @"HULK BULK. BECOME THE HULK");
                    return true;

                }
                return false;

            }
            public bool LockBoxF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The lockbox rusty but blue in colour and has a small keyhole.");
                    return true;

                }
                if((GameF.CompareVerb("unlock") || GameF.CompareVerb("open")) && GameF.ComparePreposition2("using") && Parser.indirectObjectID == GameF.SearchForObject("lockbox key")){

                    if(GameF.TryGetObjectClass<Open>(GameF.SearchForObject("blue lockbox"), out Open openObject)){

                        if(openObject.locked){

                            GameF.Print("You unlocked the lockbox. Inside is a screwdriver with a star-shaped tip!");
                            openObject.locked = false;
                            return true;

                        }

                    }
                    
                }
                return false;

            }
            public bool StarScrewdriverF(){

                return false;

            }
            public bool SquareScrewdriverF(){

                return false;

            }
            public bool TriangleScrewdriverF(){

                return false;

            }
            public bool CompostClumpF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("Its literatlly just a ball of mud.");
                    return true;

                }
                if(GameF.CompareVerb("wash") && Parser.indirectObjectID == GameF.SearchForObject("east sink") && Parser.indirectObjectID == GameF.SearchForObject("west sink")){

                    GameF.Print("You turn the tap on and begin rinsing the clump of mud. A screwdriver with a triangular tip is revealed!");
                    objects[GameF.SearchForObject("triangular screwdriver")].holderID = GameF.SearchForObject(player);
                    objects[GameF.SearchForObject("compost clump")].holderID = -1;
                    return true;

                }
                return false;

            }
            public bool TomatoF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The tomato is red and made of plastic.");
                    return true;

                }
                return false;

            }
            public bool PotatoPlantF(){

                if(GameF.CompareVerb("dig")){

                    GameF.Print("You begin digging in the potato plant bed, and find nothing.");
                    return true;

                }
                return false;

            }
            public bool PotatoF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The potato is yellowy-brown and made of plastic. There seems to be something inside it.");
                    return true;

                }
                if(GameF.CompareVerb("take")){

                    GameF.Print("As you pick up the potato, you feel something move about inside it.");
                    return false;

                }
                if(GameF.CompareVerb("shake")){

                    GameF.Print("Something rattles about inside...");
                    return true;

                }
                if(GameF.CompareVerb("throw") || GameF.CompareVerb("drop")){

                    GameF.Print("You throw the potato with all you've got at the wall and it shatters into pieces. A red power core rolls out of it!");
                    objects[GameF.SearchForObject("plastic potato")].holderID = -1;
                    objects[GameF.SearchForObject("red power core")].holderID = GameF.GetObjectHolder(player);
                    return true;

                }
                return false;

            }
            public bool PowerCoreF(){

                int controlPanelID = GameF.SearchForObject("control panel");

                bool redCore = GameF.GetObjectHolder("red power core") == controlPanelID;
                bool greenCore = GameF.GetObjectHolder("green power core") == controlPanelID;
                bool blueCore = GameF.GetObjectHolder("blue power core") == controlPanelID;

                if(redCore && greenCore && blueCore){

                    GameF.Print("With the last power core in place, the control panel lights up brightly and begins whirring loudly. The lights come on! You hear the bay doors behind you open. You turn around and walk towards the light coming from the bay doors...");
                    GameF.Print("\nYou cover your eyes because of the blinding lights, and as you get to the other side of the door, you slowly lift your hands away from your eyes...You look around...You seem to be in some sort of reception area. There is a woman at the front desk...");
                    GameF.Print("\nWOMAN: Did you enjoy your escape room experience? Would you like a picture?");
                    Win();

                }

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("UNLIMITED POWAAA!");
                    return true;

                }
                return false;

            }
            public bool ScatScrubF(){

                if(GameF.CompareVerb("look") && (GameF.ComparePreposition1("at") || GameF.ComparePreposition1(""))){

                    GameF.Print("The bottle of ScatScrub 3000 has an image of some feces with a red cross over it");
                    return true;
                    
                }
                else if(Parser.directObjectID == GameF.SearchForObject("scatscrub") && GameF.CompareVerb("pour")){

                    if(Parser.indirectObjectID == GameF.SearchForObject("west toilet")){

                        scatScrubInWestToilet = true;
                        GameF.Print("You pour some ScatScrub 3000 into the toilet.");
                        return true;

                    }
                    else if(Parser.indirectObjectID == GameF.SearchForObject("east toilet")){

                        scatScrubInEastToilet = true;
                        GameF.Print("You pour some ScatScrub 3000 into the toilet.");
                        return true;

                    }
                    GameF.Print("The ScatScrub 3000 burns a hole straight through the " + objects[Parser.directObjectID].description + ".");
                    return true;

                }
                return false;

            }
            public bool ResignationLetterF(){

                if(GameF.CompareVerb("look") || GameF.CompareVerb("read")){

                    GameF.Print("Deer Mr. Wong," + "\n" + 
                    "I have had enough. No more will I endure the late nights cleaning feceas from the walls. " + 
                    "\nNo more will I endure the constant leaking of urine from the toilets above my office." + 
                    "\nNot only am I quitting, but I have rigged place with C4 explosives that the timer will set off!.");
                    return true;

                }
                return false;

            }
            public bool WestKeycardScannerF(){

                if((GameF.CompareVerb("swipe") && Parser.directObjectID == GameF.SearchForObject("fixed keycard"))){

                    if(GameF.TryGetObjectClass<Open>(GameF.SearchForObject("west bay door"), out Open openObject)){

                        if(openObject.locked){

                            GameF.Print("You swipe the card against the scanner, and the bay doors swing open! Unfortunately, the keycard breaks into pieces.");
                    
                            openObject.locked = false;
                            openObject.open = true;

                            objects[GameF.SearchForObject("broken keycard")].holderID = GameF.SearchForObject(player);
                            objects[GameF.SearchForObject("fixed keycard")].holderID = -1;
                            return true;

                        }                    
                        else{

                            GameF.Print("Nothing happens.");

                        }

                    }
                    
                    return true;

                }
                return false;

            }
            public bool NorthKeycardScannerF(){

                if((GameF.CompareVerb("swipe") && Parser.directObjectID == GameF.SearchForObject("fixed keycard"))){

                    if(GameF.TryGetObjectClass<Open>(GameF.SearchForObject("north bay door"), out Open openObject)){

                        if(openObject.locked){

                            GameF.Print("You swipe the card against the scanner, and the bay doors swing open! Unfortunately, the keycard breaks into pieces.");
                    
                            openObject.locked = false;
                            openObject.open = true;

                            objects[GameF.SearchForObject("broken keycard")].holderID = GameF.SearchForObject(player);
                            objects[GameF.SearchForObject("fixed keycard")].holderID = -1;

                            return true;

                        }                    
                        else{

                            GameF.Print("Nothing happens.");

                        }

                    }

                }
                return false;

            }
            public bool GreenPillF(){

                if(GameF.CompareVerb("look")){

                    GameF.Print("The capsule is completely green and looks very suspicious.");
                    return true;
                    
                }
                if(GameF.CompareVerb("eat") || GameF.CompareVerb("drink")){

                    GameF.Print("You swallow the capsule...YOU HAVE TURNED INTO A HUGE GREEN BEAST. YOU ARE THE HULK.");
                    isHulk = true;

                    objects[GameF.SearchForObject("green pill")].holderID = -1;

                    return true;
                    
                }
                return false;

            }
            public bool SquareScrewF(){

                int id = GameF.SearchForObject("square bolt");
                if(GameF.CompareVerb("look")){

                    GameF.Print("The square screw can only be unscrewed with a matching sqaure shaped screwdriver.");
                    return true;
                    
                }
                if(GameF.CompareVerb("unscrew") && Parser.directObjectID == id && Parser.indirectObjectID == GameF.SearchForObject("square screwdriver")){

                    int ventID = GameF.SearchForObject("sussy vent");

                    bool squareScrew = GameF.GetObjectHolder("square bolt") == ventID;
                    bool triangleScrew = GameF.GetObjectHolder("triangle bolt") == ventID;
                    bool starScrew = GameF.GetObjectHolder("star bolt") == ventID;

                    if(squareScrew){

                        GameF.Print("You successfully unscrewed the square screw from the vent!");
                        objects[id].holderID = GameF.SearchForObject(player);
                        squareScrew = false;

                    }
                    else{

                        GameF.Print("There is no square screw on the vent!");

                    }

                    if(!squareScrew && !triangleScrew && !starScrew){

                        if(GameF.TryGetObjectClass<Open>(ventID, out Open opentObject)){

                            opentObject.locked = false;
                            opentObject.open = true;

                        }

                    }

                    for(int i = 0; i < objects[id].classes.Length; i++){

                        if(objects[id].classes[i].GetType() == typeof(TryTake)){

                            objects[id].classes[i] = new CanTake();

                        }

                    }
                    return true;
                    
                }
                return false;

            }
            public bool TriangleScrewF(){

                int id = GameF.SearchForObject("triangle bolt");
                if(GameF.CompareVerb("look")){

                    GameF.Print("The triangle screw can only be unscrewed with a matching triangle shaped screwdriver.");
                    return true;

                }
                if(GameF.CompareVerb("unscrew") && Parser.directObjectID == id && Parser.indirectObjectID == GameF.SearchForObject("triangle screwdriver")){

                    int ventID = GameF.SearchForObject("sussy vent");

                    bool squareScrew = GameF.GetObjectHolder("square bolt") == ventID;
                    bool triangleScrew = GameF.GetObjectHolder("triangle bolt") == ventID;
                    bool starScrew = GameF.GetObjectHolder("star bolt") == ventID;

                    if(triangleScrew){

                        GameF.Print("You successfully unscrewed the triangle screw from the vent!");
                        objects[id].holderID = GameF.SearchForObject(player);
                        triangleScrew = false;

                    }
                    else{

                        GameF.Print("There is no triangle screw on the vent!");

                    }

                    if(!squareScrew && !triangleScrew && !starScrew){

                        if(GameF.TryGetObjectClass<Open>(ventID, out Open opentObject)){

                            opentObject.locked = false;
                            opentObject.open = true;

                        }

                    }

                    for(int i = 0; i < objects[id].classes.Length; i++){

                        if(objects[id].classes[i].GetType() == typeof(TryTake)){

                            objects[id].classes[i] = new CanTake();

                        }

                    }
                    return true;
                    
                }
                return false;

            }
            public bool StarScrewF(){

                int id = GameF.SearchForObject("star bolt");
                if(GameF.CompareVerb("look")){

                    GameF.Print("The star screw can only be unscrewed with a matching star shaped screwdriver.");
                    return true;
                    
                }
                if(GameF.CompareVerb("unscrew") && Parser.directObjectID == id && Parser.indirectObjectID == GameF.SearchForObject("star screwdriver")){

                    int ventID = GameF.SearchForObject("sussy vent");

                    bool squareScrew = GameF.GetObjectHolder("square bolt") == ventID;
                    bool triangleScrew = GameF.GetObjectHolder("triangle bolt") == ventID;
                    bool starScrew = GameF.GetObjectHolder("star bolt") == ventID;

                    if(starScrew){

                        GameF.Print("You successfully unscrewed the star screw from the vent!");
                        objects[id].holderID = GameF.SearchForObject(player);
                        starScrew = false;

                    }
                    else{

                        GameF.Print("There is no star screw on the vent!");

                    }

                    if(!squareScrew && !triangleScrew && !starScrew){

                        if(GameF.TryGetObjectClass<Open>(ventID, out Open opentObject)){

                            opentObject.locked = false;
                            opentObject.open = true;

                        }

                    }

                    for(int i = 0; i < objects[id].classes.Length; i++){

                        if(objects[id].classes[i].GetType() == typeof(TryTake)){

                            objects[id].classes[i] = new CanTake();

                        }

                    }
                    return true;
                    
                }
                return false;

            }

        #endregion

    #endregion
    #region Flags

        public bool Any(int id, bool showMessage){
            
            return true;

        }
        public bool InRoom(int id, bool showMessage){

            int playerHolder = GameF.GetObjectHolder(player);
            bool condition = IsAccessible(id, playerHolder) || playerHolder == id; 

            if(!condition){

                if(GameF.TryGetObjectClass<Door>(id, out Door doorObject)){

                    if(
                        
                        (doorObject.roomIn == playerHolder || doorObject.roomOut == playerHolder)

                    ){

                        condition = true;

                    }

                }

            }

            string message = "You can't see any " + Parser.currentInput + " here!";
            return GameF.EvaluateFlag(condition, showMessage, message);

        }
        public bool IsDirection(int id, bool showMessage){

            bool condition = GameF.TryGetObjectClass<Direction>(id, out Direction directionObject);
            string message = "You must specify a direction!";
            
            return GameF.EvaluateFlag(condition, showMessage, message);

        }
        public bool Has(int id, bool showMessage){

            bool condition = IsAccessible(id, GameF.SearchForObject(player));
            string message = "You don't have the " + Parser.currentInput + "!";

            return GameF.EvaluateFlag(condition, showMessage, message);

        }

    #endregion 
    #region Object Classes

        public class Room : ObjectClass{

            public int north { get; set; }
            public int northeast { get; set; }
            public int east { get; set; }
            public int southeast { get; set; }
            public int south { get; set; }
            public int southwest { get; set; }
            public int west { get; set; }
            public int northwest { get; set; }
            public int up { get; set; }
            public int down { get; set; }
            public bool visited = false;

            public int[] GetTravelTable(){

                return new int[10]{

                    north,
                    northeast,
                    east,
                    southeast,
                    south,
                    southwest,
                    west,
                    northwest,
                    up,
                    down

                };

            }

        }
        public class Door : ObjectClass{

            public int roomIn { get; set; }
            public int roomOut { get; set; }

        }
        public class Open : ObjectClass{

            public bool open { get; set; }
            public bool locked { get; set; }

        }
        public class CanTake : ObjectClass{}
        public class TryTake : ObjectClass{}
        public class Container : ObjectClass{

            public int capacity { get; set; }
            public int[] held { get; set; }

        }
        public class OnContainer : Container{}
        public class InContainer : Container{}
        public class GetIn : ObjectClass{}
        public class Direction : ObjectClass{}

    #endregion
    #region Custom Functions

        public void BeforeCommandParsed(){

            if(timerStarted){

                timeLeft = TimeSpan.FromMinutes(30) - (DateTime.Now - timeStarted) + timeChange;
                if(timeLeft.TotalSeconds <= 0){

                    GameF.Print("BANG! EVERYTHING EXPLODES! YOU DIED!");
                    GameF.Print("The timer probably went off! Be quicker next time!");
                    Death();

                }

            } 

            exitTime = DateTime.Now;

        }
        public void StartGame(){

            GameF.Print("You wake up.\nIts cold. Its dark. You seem to be in some sort of pod.");

        }
        public void ReturnToGame(){

            LookV();
            timeChange = DateTime.Now - exitTime;

        }
        public int PrioritiseObjects(int[] similar){

            List<int> accepted = new List<int>{};
            int best = -1;

            foreach(int i in similar){

                if(IsDirection(i, false) || Has(i, false) || InRoom(i, false)){

                    accepted.Add(i);

                }

            }

            if(accepted.Count == 1){

                return accepted[0];

            }
            if(accepted.Count != 0)best = accepted[0];

            accepted = new List<int>{};
            foreach(int i in similar){

                if(IsDirection(i, false)){

                    accepted.Add(i);

                }

            }
            if(accepted.Count == 1){

                return accepted[0];

            }
            if(accepted.Count != 0)best = accepted[0];

            accepted = new List<int>{};
            foreach(int i in similar){

                if(Has(i, false)){

                    accepted.Add(i);

                }

            }
            if(accepted.Count == 1){

                return accepted[0];

            }
            if(accepted.Count != 0)best = accepted[0];

            accepted = new List<int>{};
            foreach(int i in similar){

                if(InRoom(i, false)){

                    accepted.Add(i);

                }

            }
            if(accepted.Count == 1){

                return accepted[0];

            }
            if(accepted.Count != 0)best = accepted[0];

            return best;

        }
        public void Travel(int direction){

            int playerHolder = GameF.GetObjectHolder(player);
            if(!GameF.TryGetObjectClass<Room>(playerHolder, out Room currentRoom)){

                if(playerHolder != -1)GameF.Print("You can't move because you're inside the " + objects[playerHolder].description);
                else GameF.Print("You can't move because you're not in a room!");
                return;

            }

            int[] travelTable = currentRoom.GetTravelTable();

            if(travelTable[direction] == -1){

                GameF.Print("You can't go that way.");
                return;

            }

            int[] doorIDs = GameF.GetHeldObjects(GameF.SearchForObject(doors));
            if(doorIDs != null && doorIDs.Length != 0){

                for(int i = 0; i < doorIDs.Length; i++){

                    if(GameF.TryGetObjectClass<Door>(doorIDs[i], out Door doorObject)){

                        if(
                            
                            (doorObject.roomIn == playerHolder && doorObject.roomOut == travelTable[direction]) || 
                            (doorObject.roomIn == travelTable[direction] && doorObject.roomOut == playerHolder)

                        ){

                            if(GameF.TryGetObjectClass<Open>(doorIDs[i], out Open openObject) && !openObject.open){

                                GameF.Print("You can't go that way because the " + objects[doorIDs[i]].description + " is closed!");
                                return;

                            }

                            GameF.MoveObject(GameF.SearchForObject(player), travelTable[direction]);
                            LookV();
                            currentRoom.visited = true;
                            return;

                        }

                    }

                }

            }

            GameF.MoveObject(GameF.SearchForObject(player), travelTable[direction]);
            LookV();
            currentRoom.visited = true;
            return;

        }
        public void DisplayObjectTree(int objectID, Dictionary<int, int> hierarchy, bool showFirstIndent, Type showType){

            string prefix = "There is a ";
            string suffix = " here";
            int closedIndent = 0;
            bool closedIndentLocked = false;

            foreach(KeyValuePair<int, int> kvp in hierarchy){

                int multiplier = kvp.Value;
                if(!showFirstIndent){

                    multiplier-=1;
                    if(multiplier < 0)multiplier = 0;

                }
                
                if(multiplier == closedIndent){

                    closedIndentLocked = false;

                }

                if(kvp.Key != GameF.SearchForObject(player)){

                    string indent = String.Concat(Enumerable.Repeat("   ", multiplier));

                    if(multiplier > 0 || showFirstIndent){

                        prefix = "A ";
                        suffix = "";

                    }
                    else{

                        prefix = "There is a ";
                        suffix = " here";

                    }

                    if(showType != null && GameF.TryGetObjectClass<Container>(kvp.Key, out Container containerObject)){
                        
                        Type thisType = null;
                        if(GameF.TryGetObjectClass<OnContainer>(kvp.Key, out OnContainer onContainerObject))thisType = typeof(OnContainer);
                        else if(GameF.TryGetObjectClass<InContainer>(kvp.Key, out InContainer inContainerObject))thisType = typeof(InContainer);

                        if(showType != thisType){
                            
                            closedIndent = multiplier;
                            closedIndentLocked = true;

                        }

                    }    

                    if(GameF.TryGetObjectClass<Open>(kvp.Key, out Open openObject)){

                        if(openObject.open){

                            if(!closedIndentLocked) GameF.Print(indent + prefix + objects[kvp.Key].description + suffix + ", which is open.");

                        }
                        else{

                            if(!closedIndentLocked){
                                
                                GameF.Print(indent + prefix + objects[kvp.Key].description + suffix + ", which is closed.");
                                if(!GameF.TryGetObjectClass<OnContainer>(kvp.Key, out OnContainer onObject)){

                                    closedIndent = multiplier;
                                    closedIndentLocked = true;

                                }
                                
                            }

                        }

                    }
                    else{

                        if(!closedIndentLocked) GameF.Print(indent + prefix + objects[kvp.Key].description + suffix);

                    }
                    if(GameF.GetHeldObjects(kvp.Key).Length != 0 && !closedIndentLocked){

                        if(GameF.TryGetObjectClass<OnContainer>(kvp.Key, out OnContainer onContainerObject)){

                            GameF.Print(indent + "Sitting on the " + objects[kvp.Key].description + " is: ");

                        }
                        else{

                            GameF.Print(indent + "The " + objects[kvp.Key].description + " contains: ");

                        }

                    }

                }
                else{

                    closedIndent = multiplier;
                    closedIndentLocked = true;

                }
                
            }
            
        }
        public bool IsAccessible(int findChild, int parentID){

            if(parentID == -1 || findChild == -1)return false;

            int[] held = GameF.GetHeldObjects(parentID);

            int[] doorIDs = GameF.GetHeldObjects(GameF.SearchForObject(doors));
            List<int> heldDoorIDs = new List<int>{};

            if(doorIDs != null && doorIDs.Length != 0){

                for(int i = 0; i < doorIDs.Length; i++){

                    if(GameF.TryGetObjectClass<Door>(doorIDs[i], out Door doorObject)){

                        if(
                            
                            (doorObject.roomIn == parentID || doorObject.roomOut == parentID)

                        ){

                            heldDoorIDs.Add(doorIDs[i]);

                        }

                    }

                }

            }

            int[] totalHeld = new int[held.Length + heldDoorIDs.Count];
            for(int h = 0; h < held.Length; h++){

                totalHeld[h] = held[h];

            }
            for(int d = 0; d < heldDoorIDs.Count; d++){

                totalHeld[held.Length + d] = heldDoorIDs[d];

            }


            if(GameF.TryGetObjectClass<Open>(parentID, out Open openObject)){

                if(!openObject.open && !GameF.TryGetObjectClass<OnContainer>(parentID, out OnContainer onObject)){

                    return false;

                }

            }
            if(totalHeld.Contains(findChild)){

                return true;

            }
            else{

                foreach(int id in totalHeld){

                    if(IsAccessible(findChild, id))return true;

                }

            }

            return false;

        }
        public void Win(){

            GameF.Print("\nYOU WIN");
            Game.GetInstance().FinishGame();

        }
        public void Death(){

            GameF.Print("\nYOU DIED");
            Game.GetInstance().FinishGame();

        }

    #endregion

}


