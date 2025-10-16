# Investigation Summary: FieldValues Property Type Change

**Related Issue:** [#340 - EF Core 10 and SQL Server 2025 support with JSON updates](https://github.com/CodebreakerApp/Codebreaker.Backend/issues/340)

## Quick Reference

| Aspect | Current | Proposed |
|--------|---------|----------|
| **Property Type** | `IDictionary<string, IEnumerable<string>>` | `Dictionary<string, string[]>` |
| **EF Core Version** | 9.0.9 | Waiting for 10.x |
| **Files Affected** | 8+ core files | Same |
| **Breaking Change** | N/A | Yes - Major version bump |
| **Storage Format** | String or JSON | No change required |
| **Recommendation** | Keep current | Wait for EF Core 10 |

## Documents Created

### 1. [EFCore10-FieldValues-Investigation.md](./EFCore10-FieldValues-Investigation.md)
Comprehensive investigation report covering:
- Complete impact assessment
- Current implementation analysis for all 3 database providers
- EF Core 9 vs EF Core 10 considerations
- Detailed mapping strategy options
- Migration considerations
- Testing strategy
- Recommendations

**Key Sections:**
- Section 1: Current Implementation Analysis
- Section 2: Impact Assessment (8+ files affected)
- Section 3: EF Core JSON Support Investigation
- Section 4: Mapping Strategy Options (3 alternatives)
- Section 5-6: Code Samples and Proof of Concept
- Section 7: Database Migration Considerations
- Section 8: Recommendations

### 2. [POC-FieldValues-TypeChange.md](./POC-FieldValues-TypeChange.md)
Concrete code examples demonstrating implementation:
- Before/after code comparisons
- Complete configuration examples for all databases
- Updated factory methods
- Test updates
- Bot client changes
- Usage examples
- Backward compatibility considerations

## Executive Summary

### Current State
- Repository uses **EF Core 9.0.9** with custom value converters
- `FieldValues` property is `IDictionary<string, IEnumerable<string>>`
- Three database providers: SQL Server, PostgreSQL, Cosmos DB
- Each uses different storage approach (string format vs JSON)

### Findings

#### 1. Impact Assessment
**Files Requiring Changes:** 13+ files across multiple layers
- 2 model files (Game.cs, IGame.cs)
- 6 database configuration files
- 1 factory file
- 1 client library file
- 2 bot client files
- 3+ test files

**Change Complexity:** Moderate
- Type signature updates
- ValueComparer generic parameters
- Factory method dictionary initializations
- Test assertions

#### 2. EF Core Support Analysis
**EF Core 9 (Current):**
- Requires custom converters for `IDictionary<string, IEnumerable<string>>`
- Improved JSON support but not for interface + collection values
- `Dictionary<string, string[]>` may simplify configuration

**EF Core 10 (Future):**
- Expected to improve JSON support
- May natively support `Dictionary<string, string[]>`
- Should be tested before making breaking changes

#### 3. Storage Strategies

| Database | Current | Proposed | Migration Needed |
|----------|---------|----------|------------------|
| **SQL Server** | String format (`nvarchar(200)`) | Same or JSON | Optional |
| **PostgreSQL** | String format | Same or JSONB | Optional |
| **Cosmos DB** | JSON (native) | JSON (native) | No |

### Mapping Strategy Options

#### Option 1: Direct Property Type Change ⭐ Recommended
**Approach:** Change to `Dictionary<string, string[]>` in major version

**Pros:**
- Clean, straightforward implementation
- Better performance (arrays vs enumerables)
- Aligns with EF Core capabilities
- Future-proof

**Cons:**
- Breaking change for all consumers
- Requires major version bump (4.0.0)

**Recommendation:** Implement in version 4.0.0 after EF Core 10 is stable

#### Option 2: Keep Interface, Use Explicit Implementation
**Approach:** Maintain `IDictionary<string, IEnumerable<string>>` in interface

**Pros:**
- Backward compatible
- No breaking changes

**Cons:**
- More complex
- Doesn't simplify EF Core configuration

**Recommendation:** Not recommended - adds complexity without clear benefit

#### Option 3: Wait for EF Core 10 ⭐ Current Recommendation
**Approach:** Monitor EF Core 10 development

**Pros:**
- Can leverage native features
- Better understanding of capabilities
- Avoid premature optimization

**Cons:**
- Delays potential benefits

**Recommendation:** **This is the recommended immediate action**

## Key Takeaways

### 1. Breaking Change Scope
- **Moderate impact** - 8+ core files plus tests
- **Type change** is straightforward
- **Database schemas** can remain unchanged
- **Migration path** is clear for consumers

### 2. Storage Format Flexibility
- Current string-based storage in SQL Server/PostgreSQL works well
- No need to migrate to JSON columns immediately
- Cosmos DB already uses JSON (no changes needed)
- Can keep string format even after type change

### 3. Backward Compatibility
- Arrays implement `IEnumerable<string>` - many patterns still work
- LINQ operations continue to function
- `.Count()` becomes `.Length` for efficiency
- Explicit interface implementation can maintain full compatibility

### 4. EF Core Considerations
- **Current:** EF Core 9.0.9 requires custom converters
- **Future:** EF Core 10 may simplify configuration
- **Reality:** Even with EF Core 10, some conversion may be needed
- **Action:** Test preview releases before committing

## Recommendations

### Immediate Actions (Now)
1. ✅ **Document findings** - Completed
2. ✅ **Create POC code samples** - Completed
3. **Monitor EF Core 10 development**
4. **Track this investigation in issue #340**

### Short-term (EF Core 10 Preview)
1. **Test EF Core 10 preview packages**
2. **Validate JSON support for `Dictionary<string, string[]>`**
3. **Measure performance improvements**
4. **Update documentation based on findings**

### Long-term (Version 4.0.0)
1. **Implement property type change**
2. **Update all affected files**
3. **Run complete test suite**
4. **Create migration guide for consumers**
5. **Release as major version 4.0.0**

## Migration Path for Consumers

When version 4.0.0 is released:

```csharp
// Old code (v3.x)
IDictionary<string, IEnumerable<string>> fieldValues = game.FieldValues;

// New code (v4.0) - Option 1: Direct change
Dictionary<string, string[]> fieldValues = game.FieldValues;

// New code (v4.0) - Option 2: Keep working with IEnumerable
IEnumerable<string> colors = game.FieldValues["colors"]; // Still works!

// More efficient with arrays
int count = game.FieldValues["colors"].Length; // Use .Length instead of .Count()
```

## Questions Answered

### Q1: What needs to be changed?
**A:** 13+ files across model, database, factory, client, and test layers. See detailed list in investigation document.

### Q2: Can we keep the interface type but map differently?
**A:** Technically yes, but adds complexity without clear benefit. Not recommended.

### Q3: What about EF Core 10 JSON features?
**A:** EF Core 10 is not yet released. Should test preview releases to understand actual capabilities before making breaking changes.

### Q4: Do we need database migrations?
**A:** No for string-based storage. Optional migration to JSON columns for better queryability.

### Q5: What about Cosmos DB?
**A:** Minimal impact - only converter type signatures change. JSON structure remains identical.

### Q6: When should we make this change?
**A:** Wait for EF Core 10 stable release, test thoroughly, then implement in major version 4.0.0.

## Next Steps

1. **Create tracking issue** in GitHub for version 4.0.0 planning
2. **Set up monitoring** for EF Core 10 releases
3. **Prepare test branch** for EF Core 10 preview testing
4. **Document decision** in issue #340
5. **Schedule review** when EF Core 10 RC is available

---

## Related Links

- Main Investigation: [EFCore10-FieldValues-Investigation.md](./EFCore10-FieldValues-Investigation.md)
- Proof of Concept: [POC-FieldValues-TypeChange.md](./POC-FieldValues-TypeChange.md)
- Parent Issue: [#340](https://github.com/CodebreakerApp/Codebreaker.Backend/issues/340)
- EF Core What's New: https://learn.microsoft.com/en-us/ef/core/what-is-new/

---

**Investigation Status:** ✅ Complete  
**Recommendation:** Wait for EF Core 10, then implement in version 4.0.0  
**Document Version:** 1.0  
**Date:** October 15, 2025
