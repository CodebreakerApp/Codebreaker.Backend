# CodeBreaker.Bot Benchmarks

This project provides comprehensive performance benchmarks for the CodeBreaker algorithms, comparing both the binary-based (`CodeBreaker.Bot`) and string-based (`CodeBreaker.BotWithString`) implementations. It measures execution time and memory consumption of the core algorithms used for playing Codebreaker games.

## Overview

The benchmark suite includes four main categories of benchmarks:

1. **AlgorithmBenchmarks** - Original binary algorithm performance tests
2. **GameScenarioBenchmarks** - Realistic gameplay simulation tests
3. **InitializationBenchmarks** - One-time setup operation tests
4. **ComparisonBenchmarks** - Direct comparisons between binary and string implementations

## Algorithm Implementations Compared

### Binary Implementation
- **Data representation**: `int` with bit manipulation
- **Color handling**: Bit masks and shifts
- **Algorithm complexity**: Bit operations
- **Memory efficiency**: Compact representation
- **API compatibility**: Requires conversion to/from strings

### String Implementation  
- **Data representation**: `string[]` arrays
- **Color handling**: Direct string comparison
- **Algorithm complexity**: Simple array operations
- **Readability**: Higher (string operations)
- **API compatibility**: Direct compatibility with Games API

## Game Types Tested

- **Game6x4**: 6 colors, 4 positions (traditional Mastermind)
- **Game8x5**: 8 colors, 5 positions
- **Game5x5x4**: 25 shape+color combinations, 4 positions

## Benchmark Categories

### 1. AlgorithmBenchmarks
Core binary algorithm performance with different list sizes:
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

### 4. ComparisonBenchmarks (NEW)
Direct performance comparisons between implementations:
- **Black/White/No matches filtering** - Core game logic performance
- **Peg selection operations** - Individual element access
- **Initialization performance** - Setup time comparison
- **Memory usage patterns** - Memory allocation analysis
- **Color conversion operations** - Data transformation costs

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
   # Run only comparison benchmarks
   dotnet run -c Release -- --filter "*Comparison*"
   
   # Run only algorithm benchmarks
   dotnet run -c Release -- --filter "*AlgorithmBenchmarks*"
   
   # Run only Game6x4 benchmarks
   dotnet run -c Release -- --filter "*Game6x4*"
   
   # Run only binary vs string comparisons for black matches
   dotnet run -c Release -- --filter "*BlackMatches*"
   
   # Run memory-intensive benchmarks
   dotnet run -c Release -- --filter "*Memory*"
   ```

4. **Quick dry run for testing**:
   ```bash
   dotnet run -c Release -- --filter "*Comparison*" -j Dry
   ```

### Specific Comparison Examples

```bash
# Compare initialization performance
dotnet run -c Release -- --filter "*Initialization*"

# Compare black matches filtering
dotnet run -c Release -- --filter "*BlackMatches*"

# Compare memory usage
dotnet run -c Release -- --filter "*Memory*"

# Compare peg selection operations
dotnet run -c Release -- --filter "*PegSelection*"

# Compare white matches filtering  
dotnet run -c Release -- --filter "*WhiteMatches*"

# Compare no matches filtering
dotnet run -c Release -- --filter "*NoMatches*"
```

### Advanced Options

```bash
# Generate detailed reports
dotnet run -c Release -- --exporters html json

# Run with memory profiling
dotnet run -c Release -- --memory

# Compare different .NET versions (if available)
dotnet run -c Release -- --runtimes net8.0 net9.0

# Group benchmarks by implementation type
dotnet run -c Release -- --filter "*Binary*"
dotnet run -c Release -- --filter "*String*"
```

## Understanding the Results

### Key Metrics to Watch

1. **Mean Execution Time**: Average time per operation
2. **Memory Allocation**: Bytes allocated during execution
3. **Gen 0/1/2 Collections**: Garbage collection pressure
4. **Ratio**: Relative performance between implementations
5. **Rank**: Performance ranking within the benchmark group

### Expected Performance Characteristics

#### Binary Implementation Advantages:
- **Memory efficiency**: Compact integer representation
- **Cache performance**: Better locality for large datasets
- **Arithmetic operations**: Fast bit manipulation
- **Less GC pressure**: Fewer object allocations

#### String Implementation Advantages:
- **API compatibility**: No conversion overhead with Games API
- **Code readability**: Easier to understand and maintain
- **Debugging**: More straightforward to inspect values
- **Type safety**: Less bit manipulation complexity

### Sample Comparison Output

```
| Method                                    | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated |
|------------------------------------------ |----------:|---------:|---------:|------:|-------:|----------:|
| Binary_HandleBlackMatches_Game6x4_FullList | 15.23 ms | 0.25 ms | 0.22 ms |  1.00 |  125.0 |    2.1 MB |
| String_HandleBlackMatches_Game6x4_FullList | 28.45 ms | 0.52 ms | 0.48 ms |  1.87 |  285.0 |    4.8 MB |
```

This shows:
- Binary implementation is ~1.87x faster
- String implementation uses ~2.3x more memory
- Both have predictable performance characteristics

### Performance Comparison Categories

The comparison benchmarks organize results by:
- **Operation type** (BlackMatches, WhiteMatches, NoMatches, etc.)
- **Game type** (Game6x4, Game8x5, Game5x5x4)
- **Implementation** (Binary vs String)
- **Data size** (FullList vs ReducedList)

## Interpreting Results for Optimization

### When to use Binary implementation:
- Large datasets (1000+ combinations)
- Memory-constrained environments
- Performance-critical paths
- Batch processing scenarios
- High-frequency operations

### When to use String implementation:
- API compatibility requirements
- Development/debugging scenarios
- Small to medium datasets
- Code maintainability priorities
- Direct integration with Games API

### Algorithm Performance Ranking (typical):

1. **SelectPeg** operations: Fastest (direct access)
2. **HandleNoMatches**: Fast (simple filtering)
3. **HandleBlackMatches**: Moderate (exact matching)
4. **HandleWhiteMatches**: Slower (complex matching logic)
5. **Initialization**: Slowest (generates all combinations)

## Benchmark Configuration

The benchmarks use BenchmarkDotNet's configuration with:
- **SimpleJob**: Reasonable number of iterations for accurate results
- **MemoryDiagnoser**: Tracks memory allocations and GC behavior
- **RankColumn**: Shows relative performance ranking
- **GroupBenchmarksBy**: Organizes results by logical categories

## Troubleshooting

### Common Issues

1. **"No benchmarks found"**: Ensure you're running in Release configuration
2. **Inconsistent results**: Run on a dedicated machine without other heavy processes
3. **Out of memory**: Reduce the size of test data if running on constrained environments
4. **Long execution times**: Use `-j Dry` for quick validation runs

### Performance Tips

1. **Close unnecessary applications** before running benchmarks
2. **Use Release configuration** for accurate performance measurements
3. **Run multiple times** to ensure consistency
4. **Consider thermal throttling** on laptops during long benchmark runs
5. **Use filters** to focus on specific comparisons

## Key Features

### Self-Contained Design
- No external package dependencies (except BenchmarkDotNet)
- Local copies of both binary and string algorithms
- Local GameType definitions
- Comprehensive test data generators
- Works without private Azure DevOps feeds

### Comprehensive Coverage
- 60+ comparison benchmarks available
- Tests different data sizes and scenarios
- Covers all major algorithm operations
- Includes initialization and memory stress tests

### Easy Comparison
- Side-by-side binary vs string results
- Clear performance ratios and rankings
- Memory allocation analysis
- Grouped by operation and game type

## Contributing

When adding new benchmarks:

1. Follow the existing naming convention: `{Implementation}_{Operation}_{GameType}_{Scenario}`
2. Use appropriate benchmark categories for organization
3. Include both binary and string variants for comparison
4. Test with different data sizes (full, reduced, small lists)
5. Document expected performance characteristics
6. Consider both time and memory implications

## Example Usage Scenarios

### Performance Analysis
```bash
# Quick performance comparison
dotnet run -c Release -- --filter "*BlackMatches*Game6x4*" -j Dry

# Detailed memory analysis
dotnet run -c Release -- --filter "*Memory*" --memory

# Full initialization comparison
dotnet run -c Release -- --filter "*Initialization*"
```

### Algorithm Selection
```bash
# Test specific game type performance
dotnet run -c Release -- --filter "*Game8x5*"

# Compare filtering operations
dotnet run -c Release -- --filter "*Matches*"

# Analyze peg operations
dotnet run -c Release -- --filter "*Peg*"
```

This comprehensive benchmark suite helps you make informed decisions about which algorithm implementation to use based on your specific performance requirements, memory constraints, and API compatibility needs.