# CodeBreaker.Bot Benchmarks

This project provides comprehensive performance benchmarks for the CodeBreaker.Bot algorithms. It measures execution time and memory consumption of the core algorithms used for playing Codebreaker games.

## Overview

The benchmarks evaluate the performance of:

### Core Algorithm Methods
- **HandleBlackMatches**: Filters possible values based on exact position matches (black pegs)
- **HandleWhiteMatches**: Filters based on correct color/wrong position matches (white pegs)
- **HandleBlueMatches**: Filters based on partial matches (specific to Game5x5x4)
- **HandleNoMatches**: Filters when no colors match the selection
- **SelectPeg**: Extracts individual peg values from integer representation
- **IntToColors**: Converts integer representation to color names

### Initialization Methods
- **InitializePossibleValues**: Creates initial possible values lists for different game types
- **Memory-intensive list operations**: Sorting, reducing, and managing large collections

### Game Scenarios
- **Early game**: Initial moves with large possibility spaces
- **Mid-game**: Progressive filtering with mixed match types
- **Late game**: High-precision filtering with small possibility spaces
- **Complete game simulation**: Full game progression scenarios

## Game Types Tested

- **Game6x4**: 6 colors, 4 positions (traditional Mastermind)
- **Game8x5**: 8 colors, 5 positions
- **Game5x5x4**: 25 shape+color combinations, 4 positions

## Benchmark Categories

### 1. AlgorithmBenchmarks
Core algorithm performance with different list sizes:
- Full lists (1,000+ values)
- Reduced lists (20-200 values)
- Various game types and match scenarios

### 2. InitializationBenchmarks
One-time setup operations:
- Possible values generation
- List creation and sorting
- Memory allocation patterns

### 3. GameScenarioBenchmarks
Realistic gameplay simulations:
- Progressive game states
- Combined operation sequences
- Best/worst-case filtering scenarios

## Running the Benchmarks

### Prerequisites

- .NET 9.0 SDK
- Windows, macOS, or Linux environment

### Quick Start

1. **Build the project**:
   ```bash
   cd src/services/bot/CodeBreaker.Bot.Benchmarks
   dotnet build -c Release
   ```

2. **Run all benchmarks**:
   ```bash
   dotnet run -c Release
   ```

3. **Run specific categories**:
   ```bash
   # Run only algorithm benchmarks
   dotnet run -c Release -- --filter "*AlgorithmBenchmarks*"
   
   # Run only Game6x4 benchmarks
   dotnet run -c Release -- --filter "*Game6x4*"
   
   # Run only memory-intensive benchmarks
   dotnet run -c Release -- --filter "*Memory*"
   ```

### Advanced Options

1. **Export results to different formats**:
   ```bash
   # Export to CSV
   dotnet run -c Release -- --exporters csv
   
   # Export to JSON
   dotnet run -c Release -- --exporters json
   
   # Export to HTML
   dotnet run -c Release -- --exporters html
   ```

2. **Run specific benchmark methods**:
   ```bash
   # Run only black matches benchmarks
   dotnet run -c Release -- --filter "*BlackMatches*"
   
   # Run only initialization benchmarks
   dotnet run -c Release -- --filter "*Initialization*"
   ```

3. **Memory profiling**:
   ```bash
   # Run with detailed memory analysis
   dotnet run -c Release -- --memory
   ```

## Understanding the Results

### Key Metrics

- **Mean**: Average execution time
- **Error**: Half of the 99.9% confidence interval
- **StdDev**: Standard deviation of measurements
- **Median**: Middle value of all measurements
- **Allocated**: Memory allocated during execution
- **Gen 0/1/2**: Garbage collection counts

### Typical Performance Expectations

| Operation | List Size | Expected Range |
|-----------|-----------|----------------|
| HandleBlackMatches | 1,000+ values | 10-100 μs |
| HandleWhiteMatches | 1,000+ values | 50-500 μs |
| HandleNoMatches | 1,000+ values | 5-50 μs |
| SelectPeg | Single value | < 1 μs |
| IntToColors | Single value | 1-5 μs |
| InitializePossibleValues | N/A | 1-10 ms |

### Memory Usage Patterns

- **Game6x4 initialization**: ~50-100 KB
- **Game8x5 initialization**: ~200-500 KB
- **Large list filtering**: Proportional to input size
- **String conversions**: Additional overhead for color names

## Interpreting Results for Optimization

### Performance Baselines

Use these benchmarks to:

1. **Establish baselines** before implementing algorithm changes
2. **Compare alternative implementations** of the same functionality
3. **Identify bottlenecks** in real game scenarios
4. **Monitor regression** when making code changes

### Common Optimization Targets

Based on the benchmarks, focus optimization efforts on:

1. **HandleWhiteMatches**: Often the most expensive operation
2. **Large list operations**: When possibility space is still large
3. **Memory allocations**: Frequent list creation and destruction
4. **Game8x5 scenarios**: Larger search spaces require more processing

### Red Flags

Watch for:
- **Execution times > 1ms** for individual filtering operations
- **Memory allocations > 1MB** for single operations
- **High GC pressure** (frequent Gen 1/2 collections)
- **Inconsistent timing** (high standard deviation)

## Benchmark Configuration

The benchmarks use BenchmarkDotNet's default configuration with:
- **SimpleJob**: Reasonable number of iterations for accurate results
- **MemoryDiagnoser**: Tracks memory allocations and GC behavior
- **RankColumn**: Shows relative performance ranking

## Troubleshooting

### Common Issues

1. **"No benchmarks found"**: Ensure you're running in Release configuration
2. **Inconsistent results**: Run on a dedicated machine without other heavy processes
3. **Out of memory**: Reduce the size of test data if running on constrained environments

### Performance Tips

1. **Close unnecessary applications** before running benchmarks
2. **Use Release configuration** for accurate performance measurements
3. **Run multiple times** to ensure consistency
4. **Consider thermal throttling** on laptops during long benchmark runs

## Contributing

When adding new benchmarks:

1. Follow the existing naming conventions
2. Use appropriate benchmark categories
3. Include memory diagnostics for operations that allocate
4. Add realistic test scenarios that represent actual usage
5. Document expected performance characteristics

## Example Output

```
|                                Method |      Mean |    Error |   StdDev |    Median | Allocated |
|-------------------------------------- |----------:|---------:|---------:|----------:|----------:|
|   HandleBlackMatches_Game6x4_FullList |  45.23 μs | 0.891 μs | 1.024 μs |  45.12 μs |   1.95 KB |
|      HandleNoMatches_Game6x4_FullList |  12.67 μs | 0.234 μs | 0.219 μs |  12.71 μs |   1.23 KB |
| InitializePossibleValues_Game6x4      |   3.45 ms | 0.068 ms | 0.064 ms |   3.43 ms |  52.3 KB |
```

This output shows that black match handling takes about 45 microseconds on average for a full Game6x4 list, while initializing the possible values takes about 3.5 milliseconds but only happens once per game.