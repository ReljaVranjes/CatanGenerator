using CatanBoardGenerator.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatanBoardGenerator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TileController : ControllerBase
    {
        public List<string> TileTypes = new List<string> { "Field", "Field", "Field", "Field", "Mountain", "Mountain", "Mountain", "Hill", "Hill", "Hill", "Desert", "Forest", "Forest", "Forest", "Forest", "Pasture", "Pasture", "Pasture", "Pasture" };
        public List<int> TileDiceNumbers = new List<int> { 2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12};
        public List<string> Players = new List<string> { "Red", "Blue", "White", "Orange" };
        List<Tile> Tiles = new List<Tile>();


        [HttpGet(Name = "GetBoard")]
        public IActionResult GetBoard()
        {
            var random = new Random();

            var shuffledTypes = TileTypes.OrderBy(x => random.Next()).ToList();
            var shuffledDiceNumbers = TileDiceNumbers.OrderBy(x => random.Next()).ToList();

            for (int i = 0; i < 19; i++)
            {
                string type = shuffledTypes[i];
                int diceNumber = 0;

                if (type != "Desert" && shuffledDiceNumbers.Count > 0)
                {
                    diceNumber = shuffledDiceNumbers[0];
                    shuffledDiceNumbers.RemoveAt(0);
                }

                Tile tile = new Tile(i, type, diceNumber);
                Tiles.Add(tile);
            }


            return Ok(Tiles);
        }

        [HttpGet("OrderOfPlay")]
        public IActionResult GetOrderOfPlay()
        {
            var random = new Random();
            var shuffledPlayers = Players.OrderBy(x => random.Next()).ToList();

            return Ok(shuffledPlayers);
        }
    }
}
