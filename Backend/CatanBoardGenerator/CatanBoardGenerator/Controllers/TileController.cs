using CatanBoardGenerator.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.PortableExecutable;

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
        public IActionResult GetBoard([FromQuery] bool sixAndEightCanTouch, [FromQuery] bool sameNumbers, [FromQuery] bool sameTypes)
        {
            Console.WriteLine("SAME TYPES: " + sameTypes);
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

            if (!sameNumbers) { RemoveTouchingNumbers(Tiles); }
            if (!sameTypes) { RemoveTouchingTypes(Tiles); }
            if (!sixAndEightCanTouch) { Remove6and8Touching(Tiles); }

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

        private bool Are6and8Touching(List<Tile> tiles)
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

        //Display board without 6 and 8 touching
        private void Remove6and8Touching(List<Tile> tiles)
        {
            Random random = new Random();

            while (!Are6and8Touching(tiles))
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

        private bool AreSameTypesTouching(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                foreach (var surroundingTileId in SurroundingTiles[tile.Id])
                {
                    if (tiles[surroundingTileId].Type == tile.Type)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //Get frequency of each tile touching its type
        private Dictionary<int, List<int>> GetTouchingTypes(List<Tile> tiles)
        {
            var positionsToChange = new Dictionary<int, List<int>>();

            foreach (var tile in tiles)
            {
                    foreach (var surroundingTileId in SurroundingTiles[tile.Id])
                    {
                        if (!positionsToChange.ContainsKey(tile.Id))
                        {
                            positionsToChange[tile.Id] = new List<int>();
                        }

                        if (tiles[surroundingTileId].Type == tile.Type)
                        {
                            positionsToChange[tile.Id].Add(surroundingTileId);
                        }
                    }
            }
            foreach (var kvp in positionsToChange)
            {
                Console.Write($"Key {kvp.Key}: {tiles[kvp.Key].Type} ");
                Console.WriteLine(string.Join(", ", kvp.Value));
            }
            Console.Write("******************************\n");
            return positionsToChange;
        }

        //For each type, get available positions
        private Dictionary<string, List<int>> GetAvailablePositionsForTypes(List<Tile> tiles)
        {
            var availablePositionsInd = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
            var availablePositionsDict = new Dictionary<string, List<int>>()
            {
                { "Field", new List<int>() },
                { "Mountain", new List<int>() },
                { "Hill", new List<int>() },
                { "Pasture", new List<int>() },
                { "Forest", new List<int>() },
            };


            var fieldPositions = Enumerable.Range(0, tiles.Count).Where(i => tiles[i].Type == "Field").ToList();
            var mountainPositions = Enumerable.Range(0, tiles.Count).Where(i => tiles[i].Type == "Mountain").ToList();
            var hillPositions = Enumerable.Range(0, tiles.Count).Where(i => tiles[i].Type == "Hill").ToList();
            var pasturePositions = Enumerable.Range(0, tiles.Count).Where(i => tiles[i].Type == "Pasture").ToList();
            var forestPositions = Enumerable.Range(0, tiles.Count).Where(i => tiles[i].Type == "Forest").ToList();

            //Add surrounding tiles of each type
            foreach (var tile in tiles) 
            { 
                if(tile.Type == "Field") { fieldPositions.AddRange(SurroundingTiles[tile.Id]); }
                else if (tile.Type == "Mountain") { mountainPositions.AddRange(SurroundingTiles[tile.Id]); }
                else if(tile.Type == "Hill") { hillPositions.AddRange(SurroundingTiles[tile.Id]); }
                else if(tile.Type == "Pasture") { pasturePositions.AddRange(SurroundingTiles[tile.Id]); }
                else if(tile.Type == "Forest") { forestPositions.AddRange(SurroundingTiles[tile.Id]); }
                else if(tile.Type == "Desert") { availablePositionsInd.RemoveAt(tile.Id); }
            }
             
            //Filter positions that are not surrounded by certain type
            availablePositionsDict["Field"] = availablePositionsInd.Except(fieldPositions).ToList();
            availablePositionsDict["Mountain"] = availablePositionsInd.Except(mountainPositions).ToList();
            availablePositionsDict["Hill"] = availablePositionsInd.Except(hillPositions).ToList();
            availablePositionsDict["Pasture"] = availablePositionsInd.Except(pasturePositions).ToList();
            availablePositionsDict["Forest"] = availablePositionsInd.Except(forestPositions).ToList();
            
            foreach (var kvp in availablePositionsDict)
            {
                Console.Write($"Key {kvp.Key}: ");
                Console.WriteLine(string.Join(", ", kvp.Value));
            }
            Console.Write("******************************\n");
            return availablePositionsDict;
        }

        //Display board without same types touching
        private void RemoveTouchingTypes(List<Tile> tiles)
        {
            Random random = new Random();

            while(!AreSameTypesTouching(tiles)) {
                var availablePositionsDict = GetAvailablePositionsForTypes(tiles);
                var touchingTypesTilesDict = GetTouchingTypes(tiles);

                var TileWithMostTouches = touchingTypesTilesDict
                .OrderByDescending(kvp => kvp.Value.Count)
                .FirstOrDefault();

                var MostTouchedType = tiles[TileWithMostTouches.Key].Type;
                Console.WriteLine("SELECTED : " + MostTouchedType);


                var availablePositions = availablePositionsDict[MostTouchedType];
                var randomAvaliableTile = availablePositions[random.Next(availablePositions.Count)];

                Console.WriteLine("CHANGING TO " + randomAvaliableTile);

                tiles[TileWithMostTouches.Key].Type = tiles[randomAvaliableTile].Type;
                tiles[randomAvaliableTile].Type = MostTouchedType;

                Console.WriteLine("SWITCHED " + TileWithMostTouches.Key + " AND " + randomAvaliableTile);
            }
        }

        private bool AreNumbersTouching(List<Tile> tiles) {
            foreach (var tile in tiles)
            {
                foreach (var surroundingTileId in SurroundingTiles[tile.Id])
                {
                    if (tiles[surroundingTileId].DiceNumber == tile.DiceNumber)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void RemoveTouchingNumbers(List<Tile> tiles)
        {
            Random random = new Random();

            while (!AreNumbersTouching(tiles))
            { 
                foreach(var tile in tiles)
                {
                    foreach (var surroundingTileId in SurroundingTiles[tile.Id])
                    {
                        if (tiles[surroundingTileId].DiceNumber == tile.DiceNumber)
                        {
                            var availablePositions = GetAvailablePositionsForTile(tile.Id);
                            var randomPosition = availablePositions[random.Next(availablePositions.Count)];
                            var diceNumber = tile.DiceNumber;

                            tile.DiceNumber = tiles[randomPosition].DiceNumber;
                            tiles[randomPosition].DiceNumber = diceNumber;
                        }
                    }
                }
            }
        }

        private List<int> GetAvailablePositionsForTile(int tileId) 
        { 
            var availablePositions = new List<int>() { 0,1,2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18};

            return availablePositions.Except(SurroundingTiles[tileId]).ToList();
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
