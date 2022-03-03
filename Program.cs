Game.GetInstance();
GameF.DisplayGameData();

while(!Game.GetInstance().isFinished){

    Parser.Parse(GameF.Input());
    
}