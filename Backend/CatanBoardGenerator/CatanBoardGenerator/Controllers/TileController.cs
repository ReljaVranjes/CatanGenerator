using CatanBoardGenerator.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CatanBoardGenerator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TileController : ControllerBase
    {
        public List<string> TileTypes = new List<string> { "Field", "Field", "Field", "Field", "Mountain", "Mountain", "Mountain", "Hill", "Hill", "Hill", "Desert", "Forest", "Forest", "Forest", "Forest", "Pasture", "Pasture", "Pasture", "Pasture" };
        public List<int> TileDiceNumbers = new List<int> { 2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12};
        public List<string> Players = new List<string> { "Red", "Blue", "White", "Orange" };
        Dictionary<int,List<int>> SurroundingTiles = new Dictionary<int, List<int>>()
        { 
            {   0 ,  [1,3,4]            },
            {   1 ,  [0,2,4,5]          },
            {   2 ,  [1,5,6]            },
            {   3 ,  [0,4,7,8]          },
            {   4 ,  [0,1,3,5,8,9]      },
            {   5 ,  [1,2,4,6,9,10]     },
            {   6 ,  [2,5,10,11]        },
            {   7 ,  [3,8,12]           },
            {   8 ,  [3,4,7,9,12,13]    },
            {   9 ,  [4,5,8,10,13,14]   },
            {   10 , [5,6,9,11,14,15]   },
            {   11 , [6,10,15]          },
            {   12 , [7,8,13,16]        },
            {   13 , [8,9,12,14,16,17]  },
            {   14 , [9,10,13,15,17,18] },
            {   15 , [10,11,14,18]      },
            {   16 , [12,13,17]         },
            {   17 , [13,14,16,18]      },
            {   18 , [14,15,17]         }
        };
        List<Tile> Tiles = new List<Tile>();


        [HttpGet(Name = "GetBoard")]
        public IActionResult GetBoard([FromQuery] bool sixAndEightCanTouch, [FromQuery] bool sameNumbers, [FromQuery] bool sameResources)
        {
            Console.WriteLine(sixAndEightCanTouch);
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

            if(!sixAndEightCanTouch) { Remove6and8AsNeighbours(Tiles); }


            return Ok(Tiles);
        }

        //Positions where 6 or 8 can be moved so there is no collision
        private List<int> GetAvailablePositions6and8(List<Tile> tiles)
        {
            var availablePositions = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

            foreach (var tile in tiles)
            {
                //Remove the availability for desert tile
                if (tile.DiceNumber == 0)
                {
                    availablePositions[tile.Id] = -1;
                }

                if (tile.DiceNumber == 6 || tile.DiceNumber == 8)
                {
                    availablePositions[tile.Id] = -1;

                    foreach (int value in SurroundingTiles[tile.Id])
                    {
                        availablePositions[value] = -1;
                    }
                }
            }

            /*foreach (int a in availablePositions)
            {
                if (a != -1)
                {
                    Console.Write("AVAILABLE: " + a + "\n");
                }
            }
            Console.Write("***************************************\n");*/
            return availablePositions.Where(x => x != -1).ToList();
        }

        //Frequency of red number surrounding red number
        private Dictionary<int,List<int>> GetSurroundings6and8(List<Tile> tiles)
        {
            var positionsToChange = new Dictionary<int, List<int>>();

            foreach (var tile in tiles)
            {
                if (tile.DiceNumber == 6 || tile.DiceNumber == 8)
                {
                    // Ensure there's a list for the current tile ID
                    if (!positionsToChange.ContainsKey(tile.Id))
                    {
                        positionsToChange[tile.Id] = new List<int>();
                    }

                    foreach (int surroundingTileId in SurroundingTiles[tile.Id])
                    {
                        if (tiles[surroundingTileId].DiceNumber == 6 || tiles[surroundingTileId].DiceNumber == 8)
                        {
                            positionsToChange[tile.Id].Add(surroundingTileId);
                        }
                    }
                }
            }
            /*foreach (var kvp in positionsToChange)
            {
                Console.Write($"Key {kvp.Key}: ");
                Console.WriteLine(string.Join(", ", kvp.Value));
            }
            Console.Write("******************************\n");*/
            return positionsToChange;
        }

        private bool Are6and8Neighbours(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if(tile.DiceNumber == 6 || tile.DiceNumber == 8)
                {
                    foreach(int surroundingTileId in SurroundingTiles[tile.Id])
                    {
                        if (tiles[surroundingTileId].DiceNumber == 6 || tiles[surroundingTileId].DiceNumber == 8)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void Remove6and8AsNeighbours(List<Tile> tiles)
        {
            Random random = new Random();

            while (!Are6and8Neighbours(tiles))
            {
                var availablePositions = GetAvailablePositions6and8(tiles);
                var positionsToChange = GetSurroundings6and8(tiles);

                //Find the red tile with most red tiles around
                var TileWithMostSurroundings = positionsToChange
                .OrderByDescending(kvp => kvp.Value.Count)
                .FirstOrDefault();

                var randomNum = random.Next(availablePositions.Count);

                //Switch the most frequent red tile with some random tile's dice number
                var sixOrEight = tiles[TileWithMostSurroundings.Key].DiceNumber;
                var randomDiceNumber = tiles[availablePositions[randomNum]].DiceNumber;

                tiles[TileWithMostSurroundings.Key].DiceNumber = randomDiceNumber;
                tiles[availablePositions[randomNum]].DiceNumber = sixOrEight;

            }


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
