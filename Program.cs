using System;
using System.Collections.Generic;
using System.Linq;

/*
    TODO
    5) Report stats in nice table
        a) stats have % winner
        b) stats have average score
        c) stats have # of wins, losses, ties
    
    Extras
    - Make ReadLine safe
    - Reflection for iPicker creation
    - Parallelize Games and Sync results at end
*/

namespace ConsoleApplication
{

    public class Program
    {

        public static void Main(string[] args)
        {

            Console.WriteLine("Number of Iterations: ");
            long iterations = long.Parse(Console.ReadLine());
            List<iPicker> iPickers = new List<iPicker>() { new MostTiles(), new MostTilesHighest(), new MostTilesLowest(), new RandomTiles(), new HighestTiles(), new LowestTiles() };
            for (int i = 1; i <= iterations; i++)
            {
                List<Board> boards = new List<Board>(4 * iPickers.Count);

                foreach (iPicker picker in iPickers)
                {
                    boards.AddRange(new List<Board>(4) { new Board(9, picker), new Board(10, picker), new Board(11, picker), new Board(12, picker) });
                }

                Random random = new Random();

                do
                {
                    Dice dice = RollDice(random);

                    foreach (Board board in boards)
                    {
                        List<int> tilePick = board.Picker.PickedTiles(dice, board);
                        if (tilePick != null)
                        {
                            foreach (int tile in tilePick)
                            {
                                board.Tiles.Remove(tile);
                            }
                        }
                        else
                        {
                            board.Done = true;
                        }

                    }
                } while (boards.Any(b => !b.Done));

                Console.WriteLine();
                foreach (Board board in boards)
                {
                    //if (board.Score == 0){
                        Console.WriteLine($"Picker: {board.Picker.GetType().Name}   Score: {board.Score}    TileCount:{board.InitialTileCount}");
                    //}
                }
            }
            Console.ReadLine();
        }

        public static Dice RollDice(Random random)
        {
            int firstRoll = random.Next(1, 7);
            int secondRoll = random.Next(1, 7);
            return new Dice() { FirstRoll = firstRoll, SecondRoll = secondRoll };
        }
    }

    public class Board
    {

        public Board(int numberOfTiles, iPicker picker)
        {
            Tiles = GenerateTiles(numberOfTiles);
            Picker = picker;
            InitialTileCount = numberOfTiles;
        }

        public List<int> Tiles;

        public int Score { get { return Tiles.Sum(); } }

        public int InitialTileCount;

        public bool Done = false;

        public iPicker Picker;

        private List<int> GenerateTiles(int numberOfTiles)
        {
            List<int> tiles = new List<int>(numberOfTiles);

            for (int i = numberOfTiles; i > 0; i--)
            {
                tiles.Add(i);
            }

            return tiles;
        }

        public List<List<int>> GetAvailableSets(List<int> tiles)
        {

            List<List<int>> sets = new List<List<int>>();
            foreach (int tile in tiles)
            {
                sets.Add(new List<int>() { tile });

                RecursiveSetAdder(tiles.Where(t => t <= tile).ToList(), sets, new List<List<int>>() { new List<int>() { tile } });
            }

            return sets;
        }

        private static bool RecursiveSetAdder(List<int> tiles, List<List<int>> sets, List<List<int>> previousTierOfSets)
        {
            List<List<int>> nextTierOfSets = new List<List<int>>();
            foreach (List<int> set in previousTierOfSets)
            {
                foreach (int i in tiles.Where(n => n < set.Last()))
                {
                    if (tiles.Contains(i))
                    {
                        if (i > 1)
                        {
                            nextTierOfSets.Add(new List<int>(set) { i });
                        }
                        sets.Add(new List<int>(set) { i });
                    }
                }
            }

            return nextTierOfSets.Count > 0 ? RecursiveSetAdder(tiles, sets, nextTierOfSets) : false;
        }
    }

    public interface iPicker
    {

        List<int> PickedTiles(Dice dice, Board board);
    }

    public class HighestTiles : iPicker
    {

        public List<int> PickedTiles(Dice dice, Board board)
        {
            int diceTotal = dice.FirstRoll + dice.SecondRoll;

            List<List<int>> sets = board.GetAvailableSets(board.Tiles);

            return sets.Where(s => s.Sum() == diceTotal).OrderByDescending(s => s.Max()).FirstOrDefault();
        }
    }

    public class LowestTiles : iPicker
    {

        public List<int> PickedTiles(Dice dice, Board board)
        {
            int diceTotal = dice.FirstRoll + dice.SecondRoll;

            List<List<int>> sets = board.GetAvailableSets(board.Tiles);

            return sets.Where(s => s.Sum() == diceTotal).OrderBy(s => s.Min()).FirstOrDefault();
        }
    }

    public class MostTilesHighest : iPicker
    {

        public List<int> PickedTiles(Dice dice, Board board)
        {
            int diceTotal = dice.FirstRoll + dice.SecondRoll;

            List<List<int>> sets = board.GetAvailableSets(board.Tiles);

            List<List<int>> test = sets.Where(s => s.Sum() == diceTotal).ToList();

            return test.OrderByDescending(s => s.Count).ThenByDescending(s => s.Max()).FirstOrDefault();
        }
    }

    public class MostTilesLowest : iPicker
    {

        public List<int> PickedTiles(Dice dice, Board board)
        {
            int diceTotal = dice.FirstRoll + dice.SecondRoll;

            List<List<int>> sets = board.GetAvailableSets(board.Tiles);

            List<List<int>> test = sets.Where(s => s.Sum() == diceTotal).ToList();
            return test.OrderByDescending(s => s.Count).ThenBy(s => s.Min()).FirstOrDefault();
        }
    }
    public class MostTiles : iPicker
    {

        public List<int> PickedTiles(Dice dice, Board board)
        {
            int diceTotal = dice.FirstRoll + dice.SecondRoll;

            List<List<int>> sets = board.GetAvailableSets(board.Tiles);

            List<List<int>> test = sets.Where(s => s.Sum() == diceTotal).ToList();
            return test.OrderByDescending(s => s.Count).FirstOrDefault();
        }
    }

    public class RandomTiles : iPicker
    {

        private Random random = new Random();

        public List<int> PickedTiles(Dice dice, Board board)
        {
            int diceTotal = dice.FirstRoll + dice.SecondRoll;

            List<List<int>> sets = board.GetAvailableSets(board.Tiles);
            IEnumerable<List<int>> matchingSets = sets.Where(s => s.Sum() == diceTotal);

            return matchingSets.ElementAtOrDefault(random.Next(0, matchingSets.Count()));
        }
    }

    public struct Dice
    {

        public int FirstRoll;

        public int SecondRoll;
    }
}
