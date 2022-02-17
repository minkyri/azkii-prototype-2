GameF.Print("\nAZKII PROTOTYPE 2");

GameF.DisplayGameData();

while(!Game.GetInstance().isFinished){

    Parser.Parse(GameF.Input());
    
}