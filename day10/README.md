# Advent of Code 2025 - Day 10: Factory

## Problem Overview

This challenge involves controlling a factory with buttons that affect multiple systems simultaneously. Each button press toggles indicator lights (Part 1) or adjusts joltage levels (Part 2) across multiple components.

### Part 1: Indicator Lights (Binary Toggle Problem)
- **Goal**: Achieve a specific pattern of binary indicator lights (on/off)
- **Constraint**: Each button toggles a specific set of lights
- **Objective**: Minimize the total number of button presses

### Part 2: Joltage Configuration (Integer Linear Programming)
- **Goal**: Reach exact integer joltage targets for multiple systems
- **Constraint**: Each button increments specific joltage counters by 1
- **Objective**: Minimize the total number of button presses

## Algorithm Choices

### Part 1: Gaussian Elimination over GF(2)

**Why this approach?**
- The problem is a system of linear equations over the binary field GF(2) (modulo 2 arithmetic)
- Each light can be on (1) or off (0), and each button press toggles specific lights
- The equation system: `Ax = b` where `x` is the button press vector (0 or 1)

**Algorithm Steps:**

1. **Matrix Construction**
   - Rows represent lights, columns represent buttons
   - `matrix[light][button] = 1` if button affects light
   - Augmented column stores target state

2. **Gaussian Elimination to RREF**
   - Perform row reduction using XOR operations (addition in GF(2))
   - Identify pivot columns (dependent variables) and free variables
   - Check for inconsistency (no solution exists)

3. **Minimize Hamming Weight**
   - Enumerate all `2^k` combinations of free variables (k = number of free variables)
   - For each combination, back-substitute to find dependent variables
   - Track minimum number of button presses (Hamming weight)

**Complexity:**
- Gaussian elimination: `O(n³)` where n = number of buttons
- Enumeration: `O(2^k)` where k = free variables (typically small)
- Overall: Fast for practical inputs (k ≤ 20)

### Part 2: Integer Linear Programming with Branch-and-Bound

**Why not use Part 1's approach?**
- Part 2 requires **exact integer targets**, not binary toggles
- Buttons can be pressed multiple times (0, 1, 2, ... times)
- This is an NP-hard Integer Linear Programming (ILP) problem

**Algorithm Steps:**

1. **Matrix Construction**
   - Rows represent joltage requirements, columns represent buttons
   - `matrix[req][button] = 1` if button affects requirement
   - Augmented column stores target joltage values
   - Use `i128` for numerical stability during elimination

2. **Fraction-Free Gaussian Elimination (Forward Pass Only)**
   - Perform forward elimination to create upper triangular form
   - Eliminates rows below pivot using: `row_new = row * pivot_val - pivot_row * factor`
   - This avoids division and maintains integer arithmetic
   - Stop after forward pass (don't reduce to RREF)

3. **Identify Free Variables**
   - Variables (buttons) without pivot positions are "free"
   - Can be set to any non-negative integer value
   - Dependent variables are determined by free variables

4. **Branch-and-Bound Search**
   - Enumerate combinations of free variable values
   - For each combination:
     - Back-substitute from bottom to top to solve for pivot variables
     - Check divisibility (solution must be integral)
     - Check non-negativity (can't press buttons negative times)
   - Track minimum total button presses across all valid solutions

**Key Optimizations:**

1. **Smart Search Limits**
   ```rust
   let limit = if free_vars.len() > 1 { 200 } else { 20000 };
   ```
   - Multiple free variables: Search 0-200 (prevents exponential blowup)
   - Single free variable: Search 0-20,000 (thorough search possible)
   - Balances completeness with performance

2. **Early Termination**
   - Stop immediately if a solution is not integral (fails divisibility check)
   - Stop if any variable would be negative
   - Prunes invalid branches early

3. **i128 Precision**
   - Prevents integer overflow during elimination
   - Maintains exact arithmetic throughout
   - Converts to i64 only for final result

4. **Forward-Only Elimination**
   - Faster than full RREF (Reduced Row Echelon Form)
   - Sufficient for back-substitution during search
   - Reduces matrix operations by ~50%

## Performance Results

For the full input (100 machines):

- **Part 1**: < 0.1 seconds
  - Total presses: **550**
  
- **Part 2**: ~2-3 seconds (release mode)
  - Total presses: **20,042**

## Implementation Notes

### Data Structures
- `Vec<Vec<i128>>`: Matrix representation for numerical stability
- `HashMap<usize, usize>`: Maps pivot columns to rows for efficient lookup
- `Vec<i64>`: Solution vectors and free variable values

### Numerical Stability
- Use `i128` during computation to prevent overflow
- Fraction-free elimination avoids floating-point errors
- All operations remain in integer domain
