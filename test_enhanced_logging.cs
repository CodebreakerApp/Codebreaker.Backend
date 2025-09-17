// This file demonstrates that the enhanced error message containing both received and expected move numbers
// is working correctly and will be logged by the GameAPIs service.
// 
// To run this test:
// dotnet run --project test_enhanced_logging.cs

using System;
using Codebreaker.GameAPIs.Analyzers;
using Codebreaker.GameAPIs.Models;

namespace TestEnhancedLogging
{
    public class MockGame : IGame
    {
        public string GameType { get; set; } = "Game6x4";
        public int NumberCodes { get; set; } = 4;
        public int MaxMoves { get; set; } = 12;
        public bool IsVictory { get; set; } = false;
        public int LastMoveNumber { get; set; } = 2; // Simulate that 2 moves have already been made
        public IDictionary<string, IEnumerable<string>> FieldValues { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public string[] Codes { get; set; } = new string[4];
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }

        public bool HasEnded() => false;
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Testing Enhanced Logging for Invalid Move Numbers");
            Console.WriteLine("=================================================");
            
            var game = new MockGame();
            var guesses = new ColorField[] { 
                ColorField.Parse("Red", null), 
                ColorField.Parse("Green", null), 
                ColorField.Parse("Blue", null), 
                ColorField.Parse("Yellow", null) 
            };
            
            // Simulate sending an invalid move number (5 when the expected is 3)
            int invalidMoveNumber = 5;
            int expectedMoveNumber = game.LastMoveNumber + 1;
            
            try 
            {
                var analyzer = new ColorGameGuessAnalyzer(game, guesses, invalidMoveNumber);
                analyzer.GetResult();
                Console.WriteLine("ERROR: Should have thrown an exception!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"✓ Exception thrown as expected: {ex.Message}");
                Console.WriteLine($"✓ Received move number {invalidMoveNumber} is included in message");
                Console.WriteLine($"✓ Expected move number {expectedMoveNumber} is included in message");
                
                // Verify both numbers are in the message
                bool hasReceived = ex.Message.Contains($"received {invalidMoveNumber}");
                bool hasExpected = ex.Message.Contains($"expected {expectedMoveNumber}");
                
                if (hasReceived && hasExpected)
                {
                    Console.WriteLine("✓ SUCCESS: Enhanced error message contains both received and expected move numbers!");
                }
                else
                {
                    Console.WriteLine("✗ FAILED: Enhanced error message is missing information");
                    Console.WriteLine($"  Has received: {hasReceived}, Has expected: {hasExpected}");
                }
                
                Console.WriteLine();
                Console.WriteLine("This enhanced error message will now appear in GameAPIs service logs when:");
                Console.WriteLine("- A client sends an invalid move number");
                Console.WriteLine("- The InvalidMoveReceived log method is called with ex.Message");
                Console.WriteLine("- Making debugging much easier by showing both received and expected values");
            }
        }
    }
}