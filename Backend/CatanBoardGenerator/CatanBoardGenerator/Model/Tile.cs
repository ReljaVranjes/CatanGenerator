namespace CatanBoardGenerator.Model
{
    public class Tile
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int DiceNumber { get; set; }

        public Tile(int id, string type, int diceNumber)
        {
            Id = id;
            Type = type;
            DiceNumber = diceNumber;
        }

        public override string ToString()
        {
            return $"Tile: {Id}, Type: {Type}, Dice: {DiceNumber}";
        }
    }

}
