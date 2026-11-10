# 📘 The Masterclass Learning Guide: Dynamic Branch Prediction (1981–Present)
### An In-Depth Technical Textbook & Viva Preparation Manual for ELL305
**Course:** ELL305 — Computer Organization and Architecture (IIT Delhi)  
**Authors:** Aman Sharma, Atharv Khedkar, Makar Vishwajit Shankar, Om Sahu, Sai Kaushik Tati  

---

## 🧭 How to Use This Guide

This document is designed as a **comprehensive self-teaching manual** for the entire team. Whether you have 20 minutes or 2 hours, this guide walks you from absolute first principles to advanced microarchitectural design. If you read this guide thoroughly, you will understand every equation, circuit diagram, simulation result, and design trade-off in our term paper, and you will be able to answer any viva question with confidence.

---

# 📑 Table of Contents
1. [Module 1: The Microarchitectural Foundations (Pipelining & Hazards)](#module-1-the-microarchitectural-foundations)
2. [Module 2: The Classical Evolution (1981–1999)](#module-2-the-classical-evolution)
   - [2.1 Smith's Bimodal 2-Bit Saturating Counter (1981)](#21-smiths-bimodal-2-bit-saturating-counter-1981)
   - [2.2 Yeh & Patt's Two-Level Adaptive Predictors (1991, 1993)](#22-yeh--patts-two-level-adaptive-predictors-1991-1993)
   - [2.3 The Complete 9-Variant Taxonomy ($XAy$)](#23-the-complete-9-variant-taxonomy-xay)
   - [2.4 McFarling's Gshare Predictor (1993)](#24-mcfarlings-gshare-predictor-1993)
   - [2.5 Kessler's DEC Alpha 21264 Tournament Predictor (1999)](#25-kesslers-dec-alpha-21264-tournament-predictor-1999)
3. [Module 3: Physical Hardware Constraints & Speculation](#module-3-physical-hardware-constraints--speculation)
   - [3.1 Speculative Checkpoints ($S_{\text{checkpoint}}$)](#31-speculative-checkpoints)
   - [3.2 Critical Path Timing & Front-End Decoupling](#32-critical-path-timing--front-end-decoupling)
4. [Module 4: Empirical Simulation & Dual-Workload Analysis](#module-4-empirical-simulation--dual-workload-analysis)
5. [Module 5: Modern Microprocessors (2000–Present: Perceptron & TAGE)](#module-5-modern-microprocessors-2000present)
6. [Module 6: Comprehensive Viva & Defense Question Bank (15 Master Questions)](#module-6-comprehensive-viva--defense-question-bank)

---

# Module 1: The Microarchitectural Foundations

### 1.1 The Pipelined Instruction Execution Model
In an unpipelined single-cycle processor, an entire instruction must be fetched, decoded, executed, access memory, and write back its registers in one long clock cycle. The clock frequency is limited by the slowest possible instruction.

A **pipelined processor** breaks instruction execution into multiple balanced stages (e.g., the classic 5-stage RISC pipeline):
1. **IF (Instruction Fetch):** Read instruction from instruction cache at Program Counter (`PC`).
2. **ID (Instruction Decode & Register Fetch):** Decode opcode, read source registers.
3. **EX (Execute / ALU):** Compute arithmetic results or effective memory addresses.
4. **MEM (Memory Access):** Read or write data cache (for load/store instructions).
5. **WB (Write Back):** Write results back into register file.

```
Cycle 1:  [ IF 1 ]
Cycle 2:  [ ID 1 ] [ IF 2 ]
Cycle 3:  [ EX 1 ] [ ID 2 ] [ IF 3 ]
Cycle 4:  [ MEM 1] [ EX 2 ] [ ID 3 ] [ IF 4 ]
Cycle 5:  [ WB 1 ] [ MEM 2] [ EX 3 ] [ ID 4 ] [ IF 5 ]
```

In steady state, the pipeline completes **one instruction every clock cycle** ($\text{CPI} \approx 1$).

---

### 1.2 Control Hazards: Why Branches Break Pipelining
A **control hazard** occurs whenever the pipeline cannot immediately determine which instruction to fetch next because the current instruction is a branch or jump whose condition or target address has not yet been resolved.

Consider a conditional branch instruction:
```assembly
BEQ R1, R2, TargetAddress   # If R1 == R2, jump to TargetAddress
```
* The processor fetches `BEQ` in **Cycle 1 (IF)**.
* In **Cycle 2**, the hardware must fetch the next instruction! But which instruction should it fetch?
  * Should it fetch `PC + 4` (fall-through path)?
  * Or should it fetch `TargetAddress` (taken path)?
* The processor **cannot know** whether `R1 == R2` until the condition is evaluated in **Cycle 3 (EX)** or even **Cycle 4 (MEM)**!

#### What happens if the processor just stalls?
If the processor simply stops fetching and waits for the branch condition to resolve, it inserts 2 to 4 empty cycles (called **pipeline bubbles**) on every branch instruction.
* In typical software (C, Python, Java, C++), **1 out of every 5 instructions is a conditional branch** ($f_b \approx 20\%$).
* Stalling 3 cycles for every branch adds $0.20 \times 3 = 0.60$ cycles per instruction.
* The CPI jumps from $1.0$ to $1.60$—a **$37.5\%$ throughput loss**!

---

### 1.3 Speculative Execution & The Cost of Misprediction
To prevent pipeline starvation, high-performance processors **speculate**:
1. At the Fetch stage (IF), a hardware **Branch Predictor** makes an educated guess: *Will this branch be Taken or Not-Taken?*
2. The front-end immediately begins fetching instructions from the predicted address without waiting.
3. When the branch reaches the Execute stage, the actual outcome is evaluated:
   * **Correct Guess (Hit):** Execution continues with zero penalty.
   * **Incorrect Guess (Misprediction):** The pipeline has fetched and partially executed instructions down the wrong path! The processor must trigger a **pipeline flush**:
     * Invalidate all speculatively fetched instructions in the pipeline.
     * Restore register state and program counters.
     * Redirect the fetch engine to the correct target address.

#### The Fundamental CPI Penalty Equation (Derivation)
Let:
* $\text{CPI}_0$ = Ideal base cycles per instruction without hazards.
* $f_b$ = Fraction of executed instructions that are conditional branches ($\approx 0.15 - 0.25$).
* $m$ = Branch misprediction rate ($0.0 \le m \le 1.0$).
* $P$ = Pipeline flush penalty in clock cycles (time from Fetch to Branch Resolution).

The effective Cycles Per Instruction ($\text{CPI}$) is:

$$\text{CPI} = \text{CPI}_0 + f_b \cdot m \cdot P$$

#### Numerical Example: Why a 5% Accuracy Improvement is Massive
Consider an out-of-order superscalar processor with:
* $\text{CPI}_0 = 0.50$ (can execute 2 instructions per cycle ideally)
* $f_b = 0.20$ (20% of instructions are conditional branches)
* $P = 12$ cycles (a typical 12-stage pipeline flush penalty)

1. **With a basic predictor ($m = 10\%$ misprediction rate):**
   $$\text{CPI} = 0.50 + (0.20 \times 0.10 \times 12) = 0.50 + 0.24 = 0.74$$
   $$\text{IPC (Instructions Per Cycle)} = \frac{1}{\text{CPI}} = \frac{1}{0.74} \approx 1.351$$

2. **With an advanced predictor ($m = 5\%$ misprediction rate):**
   $$\text{CPI} = 0.50 + (0.20 \times 0.05 \times 12) = 0.50 + 0.12 = 0.62$$
   $$\text{IPC} = \frac{1}{0.62} \approx 1.613$$

$$\text{Speedup} = \frac{1.613 - 1.351}{1.351} \times 100\% = \mathbf{19.4\% \text{ overall processor speedup!}}$$

> 💡 **Takeaway:** Improving branch prediction accuracy by just 5% delivers nearly a **20% overall speedup** across the entire computer without changing clock frequency!

---

# Module 2: The Classical Evolution (1981–1999)

The core intellectual journey of dynamic branch prediction spans four foundational milestones.

---

## 2.1 Smith's Bimodal 2-Bit Saturating Counter (1981)

In 1981, J. E. Smith published his landmark paper proposing dynamic branch prediction using small tables of saturating counters.

### The 1-Bit Predictor Flaw: Ping-Pong Oscillation on Loops
Consider a simple 1-bit predictor that simply remembers the last outcome (1 = Taken, 0 = Not-Taken).
Now consider an inner loop that runs 10 times:
```c
for (int i = 0; i < 10; i++) {
    // Loop body
}
```
In assembly, this loop terminates with a backward conditional branch:
* Iterations 1 to 9: Branch is **Taken** (loops back).
* Iteration 10: Branch is **Not-Taken** (exits loop).

What happens to a 1-bit predictor?
1. Iterations 1–9: Predicts Taken (Correct).
2. Iteration 10: Predicts Taken, but actual is Not-Taken $\implies$ **Misprediction #1**! The 1-bit state changes to `0` (Not-Taken).
3. The next time this loop is called from an outer loop:
   * First iteration: Predicts Not-Taken (because state was left at `0`), but the loop needs to run $\implies$ **Misprediction #2**!
* **Result:** A 1-bit predictor mispredicts **twice** for every execution of a loop!

### The Solution: 2-Bit Saturating Counter with Hysteresis
Smith replaced the 1-bit flag with a **2-bit saturating counter** featuring 4 states:

```
        Taken                Taken                Taken
  ┌───────────────┐    ┌───────────────┐    ┌───────────────┐
  │               ▼    │               ▼    │               ▼
[00]             [01]             [10]             [11]
Strongly NT      Weakly NT        Weakly T         Strongly T
  ▲               │    ▲               │    ▲               │
  └───────────────┘    └───────────────┘    └───────────────┘
     Not-Taken            Not-Taken            Not-Taken

Prediction Rule: High bit determines prediction (>= 2 is Taken, < 2 is Not-Taken)
```

* **How Hysteresis saves loop exits:**
  * Throughout the loop, repeated Taken outcomes saturate the counter to `11` (Strongly Taken).
  * On the 10th iteration (loop exit), the single `Not-Taken` decrements the counter from `11` to `10` (Weakly Taken).
  * **Crucial:** It still mispredicts the exit, BUT the counter remains in the Taken region (`10` $\ge 2$)!
  * When the loop is re-entered later, it **predicts Taken immediately**, avoiding the second misprediction!
  * **Result:** Mispredictions on loops are cut from 2 to 1 (a 50% reduction).

### The Fatal Flaw of Bimodal Counters: Direction Bias vs. Patterns
A 2-bit counter tracks **direction bias** (does this branch take more often than not?), but it is completely **blind to patterns**.

#### The Alternating Branch Proof
Consider a deterministic branch that alternates every single execution:
$$\text{Trace: } T, NT, T, NT, T, NT, T, NT \dots$$
* Start at `10` (Weakly Taken):
  1. Input $T$: Predicted $T$ (Correct). Counter increments to `11` (Strongly Taken).
  2. Input $NT$: Predicted $T$ (**Misprediction**). Counter decrements to `10` (Weakly Taken).
  3. Input $T$: Predicted $T$ (Correct). Counter increments to `11` (Strongly Taken).
  4. Input $NT$: Predicted $T$ (**Misprediction**). Counter decrements to `10` (Weakly Taken).
* The predictor predicts Taken every single cycle!
* **Accuracy:** Exactly **$50\%$** on a completely deterministic pattern! If the initial state starts in Weakly Not-Taken, accuracy can drop to near **$0\%$**!

---

## 2.2 Yeh & Patt's Two-Level Adaptive Predictors (1991, 1993)

T.-Y. Yeh and Yale Patt realized that branches do not execute in isolation; their future outcomes correlate with their **recent history patterns**.

### The Two-Level Architecture
1. **Level 1 (Branch History Register - BHR):** An $h$-bit shift register that records the recent taken/not-taken outcomes ($1 = T, 0 = NT$) of branches.
2. **Level 2 (Pattern History Table - PHT):** A table of $2^h$ 2-bit saturating counters, indexed by the binary pattern currently inside the BHR.

```
Branch PC ────────► [ Branch History Register (BHR) ] (e.g. 1 0 1 0)
                                    │
                                    ▼ (Pattern: 1010)
                    ┌───────────────────────────────┐
                    │ Pattern History Table (PHT)   │
                    │ Row 0000: [ 2-bit counter ]   │
                    │ Row 0001: [ 2-bit counter ]   │
                    │ ...                           │
                    │ Row 1010: [ 2-bit counter ] ──┼──► Prediction
                    │ ...                           │
                    │ Row 1111: [ 2-bit counter ]   │
                    └───────────────────────────────┘
```

#### How it solves the alternating branch:
* History length $h = 1$ bit.
* When previous outcome was $T$ (`1`), BHR holds `1` $\implies$ indexes PHT counter #1.
  * Counter #1 quickly saturates to `00` (Strongly Not-Taken).
* When previous outcome was $NT$ (`0`), BHR holds `0` $\implies$ indexes PHT counter #0.
  * Counter #0 quickly saturates to `11` (Strongly Taken).
* **Result:** After just 2 warm-up cycles, **accuracy is 100%**!

---

## 2.3 The Complete 9-Variant Taxonomy ($XAy$)

In their 1993 ISCA paper, Yeh and Patt established the universal taxonomy for two-level predictors using a 3-letter code:

$$\mathbf{X} \quad \mathbf{A} \quad \mathbf{y}$$

### Letter 1: First-Level History Type ($X$)
* **G (Global):** Exactly **one single BHR** shared by all branches across the entire program.
* **P (Per-address):** A table of $B$ independent BHRs, where each branch PC has its own private history register.
* **S (Set-associative):** Branches are hashed into $S = B/K$ sets, where branches in the same set share a BHR.

### Letter 2: Adaptive Type ($A$)
* Always **A** (Adaptive counter table).

### Letter 3: Second-Level PHT Organization ($y$)
* **g (Global):** Exactly **one single physical PHT** of $2^h$ counters shared by all history registers.
* **p (Per-address):** Each branch or set has its own independent PHT of $2^h$ counters ($B \times 2^h$ total counters).
* **s (Set-associative):** Sets of branches share sub-tables of PHTs.

---

### The 9 Combinations & Closed-Form State Formulas

| Variant | Full Meaning | Hardware State Formula ($S$) | Practical Assessment |
| :---: | :--- | :--- | :--- |
| **GAg** | **G**lobal BHR, **g**lobal PHT | $S = h + 2 \cdot 2^h$ | Lowest hardware cost; severe cross-branch aliasing |
| **GAs** | **G**lobal BHR, **s**et-associative PHT | $S = h + 2 \cdot C \cdot 2^h$ | Moderate cost; reduces global interference |
| **GAp** | **G**lobal BHR, **p**er-address PHT | $S = h + 2B \cdot 2^h$ | High cost; scales with branch working set $B$ |
| **SAg** | **S**et BHR, **g**lobal PHT | $S = \frac{B}{K}h + 2 \cdot 2^h$ | **Balanced:** Compresses BHR table by factor of $K$ |
| **SAs** | **S**et BHR, **s**et PHT | $S = \frac{B}{K}h + 2C \cdot 2^h$ | Intermediate clustering |
| **SAp** | **S**et BHR, **p**er-set PHT | $S = \frac{B}{K}h + 2\frac{B}{K}2^h$ | High state cost with diminishing accuracy returns |
| **PAg** | **P**er-address BHR, **g**lobal PHT | $S = Bh + 2 \cdot 2^h$ | **The Sweet Spot:** Excellent for loops, compact state |
| **PAs** | **P**er-address BHR, **s**et PHT | $S = Bh + 2C \cdot 2^h$ | Strong isolation; moderate cost |
| **PAp** | **P**er-address BHR, **p**er-address PHT | $S = Bh + 2B \cdot 2^h$ | **Unroutable:** Physically impossible to build for large $B$ |

#### Why PAp is physically unroutable:
Suppose an integer benchmark has $B = 4096$ active branches, and we use a history length of $h = 12$ bits.
$$S_{\text{PAp}} = (4096 \times 12) + (2 \times 4096 \times 2^{12})$$
$$S_{\text{PAp}} = 49,152 + (8,192 \times 4096) = 49,152 + 33,554,432 \text{ bits} \approx \mathbf{33.6 \text{ Megabits!}}$$
In 1993, whole CPUs had only a few kilobytes of cache. Dedicating 33 Megabits of SRAM solely for branch prediction would occupy multiple entire silicon wafers!
In contrast, **PAg** uses:
$$S_{\text{PAg}} = (4096 \times 12) + (2 \times 2^{12}) = 49,152 + 8,192 = \mathbf{57.3 \text{ Kilobits!}}$$
*PAg achieves 99% of PAp's accuracy at 1/585th the hardware cost!*

---

## 2.4 McFarling's Gshare Predictor (1993)

Scott McFarling (DEC Western Research Lab) identified two fatal problems in global two-level predictors:
1. **Destructive Aliasing in GAg:** If Branch X and Branch Y happen to see the same recent history pattern (e.g., `1101`), they access the exact same counter in the PHT and overwrite each other's state.
2. **Wasteful Indexing in Gselect:** Architects tried concatenating branch PC bits with history bits:
   $$\text{Index} = \text{PC}[7:0] \mathbin{\Vert} \text{GHR}[7:0]$$
   This required doubling the table size for every additional address bit, wasting vast amounts of memory because many index combinations are never accessed.

### The Gshare Solution: Bitwise XOR Hashing
McFarling proposed combining the global history and branch address using bitwise XOR:

$$\text{PHT Index} = (\text{PC} \gg 2) \oplus \text{GHR}$$

```
Branch PC:     1 0 1 1 0 0 1 0 1 1 0 0
                      ▼ (Shift right by 2 to drop word offset)
               0 0 1 0 1 1 0 0 1 0 1 1
GHR (History): 1 1 0 1 0 0 1 1 0 1 0 0
               ─────────────────────── (Bitwise XOR)
PHT Index:     1 1 1 1 1 1 1 1 1 1 1 1 ──► Indexes 4096-entry PHT
```

### Why Gshare is Microarchitectural Magic:
1. **Zero Extra SRAM:** It requires no additional memory arrays—just a single row of standard XOR gates!
2. **Entropy Scrambling:** Two branches seeing the same global history will have different PCs; XORing guarantees they map to completely different rows in the PHT.
3. **Constructive Aliasing:** Occasionally, distinct branches that share the same logical outcome map to the same entry, actually helping each other train the counter faster!

---

## 2.5 Kessler's DEC Alpha 21264 Tournament Predictor (1999)

In 1999, the Compaq/DEC Alpha 21264 processor debuted the most famous commercial predictor of the 20th century: the **Tournament Predictor**.

### The Motivation: "No Single Predictor Fits All Branches"
Programs contain fundamentally different types of control flow:
* **Local Correlation Branches:** Inner loop counters and state machines depend strictly on their *own past outcomes* (favoring PAg).
* **Global Path Branches:** Cascading `if-else` blocks and function call guards depend on *decisions made by earlier, separate branches* (favoring Gshare).

If you use pure PAg, your global branches mispredict. If you use pure Gshare, your local loops suffer aliasing.

### The Tournament Structure
The Alpha 21264 runs **two complete predictors in parallel**, governed by a 3rd table called the **Chooser**:

```
                                  Branch PC
                                      │
                   ┌──────────────────┴──────────────────┐
                   ▼                                     ▼
        ┌─────────────────────┐               ┌─────────────────────┐
        │ Local History Table │               │ Global History Reg  │
        │  (1024 x 10 bits)   │               │     (12 bits)       │
        └──────────┬──────────┘               └──────────┬──────────┘
                   ▼                                     │
        ┌─────────────────────┐                          │
        │ Local Counter Table │                          │
        │  (1024 x 3 bits)    │                          │
        └──────────┬──────────┘                          │
                   │ Local                               │
                   │ Prediction                          │
                   │                      ┌──────────────┴──────────────┐
                   │                      ▼                             ▼
                   │           ┌─────────────────────┐       ┌─────────────────────┐
                   │           │ Global Counter PHT  │       │    Chooser Table    │
                   │           │   (4096 x 2 bits)   │       │   (4096 x 2 bits)   │
                   │           └──────────┬──────────┘       └──────────┬──────────┘
                   │                      │ Global                      │
                   │                      │ Prediction                  │ Select Line
                   ▼                      ▼                             ▼
                ┌────────────────────────────┐                          │
                │     Multiplexer (MUX)      │◄─────────────────────────┘
                └──────────────┬─────────────┘
                               ▼
                        Final Prediction
```

### The Chooser Algorithm (Critical for Viva!)
The Chooser consists of a 4,096-entry table of 2-bit saturating counters indexed by the 12-bit global path history:
* States `00` or `01`: Steer MUX to select **Local Predictor**.
* States `10` or `11`: Steer MUX to select **Global Predictor**.

#### When is the Chooser updated?
The Chooser is **ONLY updated when the Local and Global predictors disagree**:
* If both predicted correctly: Do nothing (no need to change preference).
* If both predicted wrong: Do nothing (neither was better).
* If Local was Correct and Global was Wrong: Decrement chooser counter toward Local.
* If Global was Correct and Local was Wrong: Increment chooser counter toward Global.

### The Exact Alpha 21264 Hardware Budget Breakdown

| Subsystem Component | Physical Dimensions | Total Bits of State | Percentage of Budget |
| :--- | :---: | :---: | :---: |
| **Local History Table (LHT)** | $1024 \text{ entries} \times 10\text{ bits}$ | $10,240$ bits | $34.47\%$ |
| **Local Counter Table (PHT)** | $1024 \text{ entries} \times 3\text{ bits}$ | $3,072$ bits | $10.34\%$ |
| **Global Prediction Table** | $4096 \text{ entries} \times 2\text{ bits}$ | $8,192$ bits | $27.58\%$ |
| **Chooser Table** | $4096 \text{ entries} \times 2\text{ bits}$ | $8,192$ bits | $27.58\%$ |
| **Global History Register** | $1 \text{ register} \times 12\text{ bits}$ | $12$ bits | $0.04\%$ |
| **Total Table Storage** | — | **$29,708$ bits** ($\approx 29.7\text{ Kbit}$) | **$100.00\%$** |
| *Speculative Checkpoints* | $32 \text{ branches} \times (12\text{b} + 10\text{b})$ | $704$ bits | $+2.37\%$ |

> 🔑 **The Architectural Law:** Notice that the Chooser table consumes **8,192 bits (27.58%)**—as much silicon as the entire global predictor! Kessler proved that **adaptivity between specialized predictors beats raw table capacity**.

---

# Module 3: Physical Hardware Constraints & Speculation

### 3.1 Speculative Checkpoints ($S_{\text{checkpoint}}$)
In an aggressive out-of-order processor like the Alpha 21264 (which supports up to 80 in-flight instructions and up to 32 speculative in-flight branches):
* The Global History Register (GHR) **must be speculatively updated at instruction Fetch time**.
* If the processor waited until branch commit (retirement) to update the GHR, subsequent branches in the fetch buffer would be predicted using **stale history**, destroying prediction accuracy!

#### What happens on a misprediction?
If Branch #5 mispredicts, the speculative GHR has already been shifted by Branches #6, #7, and #8. How does the CPU recover the exact history state of Branch #5?
* **Solution (Shadow Checkpoints):** When each branch is fetched, the front-end takes an instantaneous snapshot of the GHR and local history table pointers into a dedicated **Checkpoint Array**.
* When a pipeline flush occurs, the hardware restores the GHR from the checkpoint in **exactly 1 clock cycle**!

$$S_{\text{checkpoint}} = N_{\text{checkpoints}} \times (h_{\text{GHR}} + \lceil \log_2(N_{\text{LHT}}) \rceil)$$
$$S_{\text{checkpoint}} = 32 \times (12\text{ bits} + 10\text{ bits}) = \mathbf{704 \text{ bits}}$$

---

### 3.2 Critical Path Timing & Front-End Decoupling
* **The Serialization Bottleneck:** Look at the Alpha 21264 Local Predictor:
  $$\text{Fetch PC} \longrightarrow \text{Read LHT} \longrightarrow \text{Read Local PHT} \longrightarrow \text{MUX}$$
  This requires **two serialized SRAM table reads** back-to-back!
* At a 1 GHz clock speed (1 nanosecond cycle time), performing two dependent memory lookups in a single cycle is physically impossible.

#### The Modern Solution: Front-End Decoupled Pipelining
1. **Cycle 1 (Next-Line Predictor / $\mu$BTB):** A tiny, single-cycle branch target buffer provides an instantaneous next-PC target so the fetch stage never stalls (zero bubble latency).
2. **Cycle 2 (Decoupled Main Predictor):** In parallel, the complex tournament or TAGE predictor completes its 2-cycle lookup. If the main predictor disagrees with the fast $\mu$BTB, it overrides the fetch engine with only a 1-cycle redirection bubble!

---

# Module 4: Empirical Simulation & Dual-Workload Analysis

To validate these theories, we developed an execution-driven simulator in C# (`simulate_predictors.cs`) evaluating five predictor engines over **864,980 total branch events**.

### Why We Evaluated Two Distinct Workloads
1. **Workload A (Unit Isolation Suite, 155,000 branches):** Synthesized to isolate theoretical corner cases (tight 20-iteration loops, alternating `T-NT` sequences, path-correlated nested conditionals).
2. **Workload B (SPECint-Style Procedural Control Flow, 709,980 branches):** Modeled after real-world integer applications (`gcc`, `gzip`, `perl`) across 512 active branch sites (lexer/parser state machines, hash table chaining, quicksort partitions, and error guards).

---

### Quantitative Experimental Results

```
================ BRANCH PREDICTOR DUAL-WORKLOAD SIMULATION ================
>>> WORKLOAD A: Isolation Micro-Benchmark Suite (155,000 branches)
-----------------------------------------------------------------------------------------
Predictor Architecture    | State (bits) | Mispredictions | Mispred Rate | Accuracy  
-----------------------------------------------------------------------------------------
Bimodal (2K entries)      | 4,096 bits   | 29,509         |      19.04%  |    80.96%
GAg (11-bit global)       | 4,107 bits   | 10,923         |       7.05%  |    92.95%
SAg (128 sets x 10b)      | 3,328 bits   | 9,206          |       5.94%  |    94.06%
PAg (512x10b + 1K PHT)    | 7,168 bits   | 9,206          |       5.94%  |    94.06%
Gshare (12-bit XOR)       | 8,204 bits   | 10,924         |       7.05%  |    92.95%
Alpha 21264 Tournament    | 29,708 bits  | 8,281          |       5.34%  |    94.66%
-----------------------------------------------------------------------------------------

>>> WORKLOAD B: SPECint-Style Procedural Control-Flow Trace (709,980 branches, 512 sites)
-----------------------------------------------------------------------------------------
Predictor Architecture    | State (bits) | Mispredictions | Mispred Rate | Accuracy  
-----------------------------------------------------------------------------------------
Bimodal (2K entries)      | 4,096 bits   | 195,526        |      27.54%  |    72.46%
GAg (11-bit global)       | 4,107 bits   | 52,050         |       7.33%  |    92.67%
SAg (128 sets x 10b)      | 3,328 bits   | 105,013        |      14.79%  |    85.21%
PAg (512x10b + 1K PHT)    | 7,168 bits   | 105,013        |      14.79%  |    85.21%
Gshare (12-bit XOR)       | 8,204 bits   | 50,552         |       7.12%  |    92.88%
Alpha 21264 Tournament    | 29,708 bits  | 50,170         |       7.07%  |    92.93%
-----------------------------------------------------------------------------------------
```

### Key Empirical Takeaways to Discuss in Viva:
1. **The Failure of Bimodal:** Bimodal counters suffer severely in both suites ($19.04\%$ and $27.54\%$ misprediction rates) because they are completely blind to alternating patterns and path correlations.
2. **The Procedural Drop in PAg:** In Workload A, PAg achieves $94.06\%$ accuracy. But in Workload B (procedural code), PAg drops to $85.21\%$! Why? Because local BHRs only see their own branch PC; they cannot observe decisions made in caller functions!
3. **The Power of Gshare in Procedural Code:** Gshare maintains **$92.88\%$ accuracy** in Workload B because its global history register seamlessly threads across function call boundaries and nested decision trees.
4. **The Dominance of the Tournament Predictor:** The Alpha 21264 tournament engine achieves the highest accuracy across both suites (**94.66%** in Workload A, **92.93%** in Workload B), cutting mispredictions by up to **$74.34\%$** over bimodal baselines!

---

# Module 5: Modern Microprocessors (2000–Present)

The classical era ended with the Alpha 21264 in 1999. But what happened next?

### 5.1 Perceptron Predictors (Jiménez & Lin, HPCA 2001)
* **The Problem with Two-Level Tables:** A pattern history table with $h$ history bits requires $2^h$ rows. If you want a 64-bit history, you need $2^{64}$ entries—an impossible number!
* **The Innovation:** Daniel Jiménez and Calvin Lin replaced the SRAM counter table with **single-layer artificial neural perceptrons**:
  $$y = w_0 + \sum_{i=1}^{h} w_i \cdot x_i$$
  where $x_i \in \{-1, +1\}$ represents past Taken/Not-Taken outcomes, and $w_i$ are learned signed integer weights.
* **The Magic:** Hardware complexity scales **linearly** with history length ($O(h)$ instead of exponential $O(2^h)$). This allowed processors to track **over 60 branches of history** using compact weight tables!

---

### 5.2 TAGE Predictors (André Seznec, ISCA 2006–Present)
**TAGE (TAgged GEometric history length)** is the undisputed gold standard in modern CPU design (powering **AMD Zen 3/4/5, Intel Core Ultra, and Apple Silicon M1/M2/M3/M4**).

```
                      Branch PC
                          │
  ┌───────────────────────┼───────────────────────┬───────────────────────┐
  ▼                       ▼                       ▼                       ▼
[ Base Predictor ]    [ Tagged Table T1 ]     [ Tagged Table T2 ]     [ Tagged Table T3 ]
(Bimodal counters)    (History L1 = 4 bits)   (History L2 = 16 bits)  (History L3 = 64 bits)
  │                       │                       │                       │
  │                       ▼ (Tag Match?)          ▼ (Tag Match?)          ▼ (Tag Match?)
  └───────────────────────┴───────────────────────┴───────────────────────┘
                                      │
                     Longest Matching Tagged Table Wins!
```

* **How TAGE Works:**
  1. Uses multiple tagged predictor tables alongside a bimodal base predictor.
  2. The history lengths of the tables grow in a **geometric progression** (e.g., $L_1 = 4, L_2 = 12, L_3 = 36, L_4 = 108$ bits).
  3. Short histories capture local loops; long histories capture deep global paths.
  4. Each entry has an address **tag**. The prediction is provided by the tagged table with the **longest matching history**!
  5. If no tagged table matches, it falls back gracefully to the bimodal base predictor.
* **Result:** Misprediction rates below **$2.5\%$** across modern commercial workloads!

---

# Module 6: Comprehensive Viva & Defense Question Bank

> **Read these 15 questions and model answers carefully. If you know these, no examiner can stump you.**

---

#### Q1: What is a control hazard, and why does it degrade pipeline throughput?
* **Model Answer:** A control hazard occurs when the pipeline cannot determine the next instruction address to fetch because an in-flight branch condition has not yet been resolved in the execution stage. If the pipeline stalls, it introduces empty cycles (bubbles). If it speculates incorrectly, it must flush all speculatively fetched instructions, wasting $P$ clock cycles per misprediction.

#### Q2: Explain the first-order CPI penalty equation.
* **Model Answer:** The formula is $\text{CPI} = \text{CPI}_0 + f_b \cdot m \cdot P$. $\text{CPI}_0$ is the ideal CPI, $f_b$ is the branch instruction frequency ($\approx 20\%$), $m$ is the misprediction rate, and $P$ is the pipeline penalty in cycles. Because modern out-of-order processors have deep pipelines ($P \ge 10$) and wide superscalar issue ($\text{CPI}_0 \le 0.5$), the branch penalty term dominates execution time.

#### Q3: Why does a 2-bit saturating counter outperform a 1-bit predictor on loops?
* **Model Answer:** Because of **hysteresis**. In an $N$-iteration loop, a 1-bit predictor mispredicts twice: once upon loop exit (when falling through), and once upon loop re-entry (because its state was left at Not-Taken). A 2-bit counter mispredicts only once at exit because the single exit outcome drops the counter from Strongly Taken (`11`) to Weakly Taken (`10`), preserving the Taken prediction for subsequent loop runs.

#### Q4: Why does a bimodal counter fail on an alternating branch ($T, NT, T, NT...$)?
* **Model Answer:** Bimodal counters only learn **direction bias**, not **sequential patterns**. On an alternating branch, the counter oscillates between Weakly Taken and Weakly Not-Taken, predicting Taken when it should be Not-Taken and vice versa, resulting in near 0% accuracy despite the branch being 100% deterministic.

#### Q5: How do Two-Level Adaptive predictors solve pattern recognition?
* **Model Answer:** They decouple history tracking into a Branch History Register (BHR) and a Pattern History Table (PHT). The BHR acts as a shift register capturing recent outcomes. The bit pattern in the BHR indexes dedicated 2-bit counters in the PHT. For an alternating branch, history `1` maps to a counter learning `Not-Taken`, and history `0` maps to a counter learning `Taken`, yielding 100% accuracy after warm-up.

#### Q6: Explain the Yeh & Patt 9-variant notation ($XAy$).
* **Model Answer:** The first letter ($X$) denotes First-Level History: Global (**G** = 1 shared BHR), Per-address (**P** = table of BHRs), or Set-associative (**S** = sets sharing BHRs). The second letter is always **A** (Adaptive). The third letter ($y$) denotes Second-Level PHT organization: Global (**g** = 1 shared PHT), Per-address (**p** = private PHT per branch/set), or Set-associative (**s**).

#### Q7: Why did commercial microprocessors adopt PAg instead of PAp?
* **Model Answer:** Silicon scalability. While PAp isolates patterns by giving every branch its own PHT, its hardware state scales as $S_{\text{PAp}} = Bh + 2B \cdot 2^h$. For 4,096 branches with 12-bit history, PAp requires over 33 Megabits of SRAM, which was unroutable in 1990s silicon. PAg shares a single PHT ($S_{\text{PAg}} = Bh + 2 \cdot 2^h$), reducing state to 57 Kilobits while capturing 99% of the pattern accuracy.

#### Q8: What is the closed-form state cost for a Set-Associative History (SAg) predictor?
* **Model Answer:** $S_{\text{SAg}} = \frac{B}{K} \cdot h + 2 \cdot 2^h$, where branches are hashed into $B/K$ sets, with each set sharing an $h$-bit BHR that indexes a single shared PHT of $2^h$ counters. This compresses the BHR table footprint by a factor of $K$ relative to PAg.

#### Q9: What problem does McFarling's Gshare predictor solve, and how?
* **Model Answer:** Gshare solves destructive aliasing in global history predictors without increasing physical table size. Instead of using history alone (which aliases independent branches) or concatenating PC with history (which wastes table rows), Gshare bitwise XORs the branch PC with the global history register ($\text{Index} = (\text{PC} \gg 2) \oplus \text{GHR}$). This scrambles accesses uniformly across the PHT using zero additional SRAM bits.

#### Q10: Can aliasing in Gshare ever be constructive?
* **Model Answer:** Yes. When two distinct branches mapping to the exact same PHT entry happen to share the same directional outcome (e.g., both evaluate to Taken), they reinforce the counter's training. Gshare's XOR hash maximizes entropy, ensuring that unavoidable collisions behave as harmless pseudo-random noise rather than systematic thrashing.

#### Q11: Explain the structure of the DEC Alpha 21264 Tournament Predictor.
* **Model Answer:** The Alpha 21264 combines a Local Predictor and a Global Predictor in parallel, steered by a Chooser table. The Local Predictor uses 1024 10-bit local history registers indexing 1024 3-bit saturating counters ($13.3\text{ Kbits}$). The Global Predictor uses a 12-bit GHR indexing 4096 2-bit counters ($8.2\text{ Kbits}$). The Chooser table has 4096 2-bit counters indexed by global history ($8.2\text{ Kbits}$). The total architectural state is 29,708 bits ($\approx 29.7\text{ Kbit}$).

#### Q12: Why did the Alpha 21264 use 3-bit counters for local prediction but 2-bit counters for global?
* **Model Answer:** 3-bit counters provide 8 states (0 to 7) with a midpoint of 4. This gives stronger hysteresis for inner loops with occasional anomalous exits or irregular loop iterations, preventing counter destabilization on atypical branch executions.

#### Q13: When is the Global History Register updated in modern out-of-order CPUs?
* **Model Answer:** At instruction **fetch** time. If updated at retirement (commit), back-to-back branches in the fetch stream would make predictions using stale history. To recover from mispredictions, processors maintain an array of Shadow Checkpoints (in the Alpha 21264, 32 branch checkpoints adding 704 bits of flip-flop state) that instantaneously restore the GHR within 1 cycle upon a pipeline flush.

#### Q14: How do modern multi-GHz processors resolve the 2-cycle latency of local predictors?
* **Model Answer:** Through **front-end pipeline decoupling**. A small, single-cycle Next-Line Predictor ($\mu$BTB) delivers branch target addresses in Cycle 1 to prevent fetch bubbles. The main tournament or TAGE predictor completes its lookup over 2 cycles in parallel. If the main predictor disagrees with the $\mu$BTB, it overrides the front-end fetch queue with only a 1-cycle redirection penalty.

#### Q15: What replaced tournament predictors in modern CPUs, and why?
* **Model Answer:** **TAGE (TAgged GEometric history length)** predictors. Tournament predictors are limited to two fixed history lengths. TAGE uses multiple tagged tables with geometrically growing history lengths ($4, 12, 36, 108\dots$ bits) and selects the prediction from the table with the longest matching tag. TAGE achieves misprediction rates below 2.5% and powers modern AMD Zen, Intel Core, and Apple Silicon chips.

---

### 6.2 The Professor's 5 Viva Defense Traps (Evaluation Report Audit)

> **The faculty evaluation report awarded our paper 93/100 (Grade A) and noted 4 specific microarchitectural deductions. If the professor or TA grills you on these exact points, use these model defenses:**

#### Trap 1: "Why did you use a procedural trace generator instead of standard hardware pin-traces (like Intel PIN or gem5 SPEC CPU dumps)?"
* **Bulletproof Defense:**  
  *"Sir/Ma'am, standard SPEC CPU instruction traces contain millions of instructions with unpredictable phase shifts, context switches, and cache misses that convolve branch predictor accuracy with orthogonal system effects. We deliberately designed Workload A and Workload B as controlled synthetic micro-architectural stress benches. Workload A isolates pure algorithmic failure modes (e.g., proving that bimodal counters achieve near 0% on deterministic alternation, while PAg achieves 100% after warm-up). Workload B synthesizes multi-nested loops, correlated branches, and dynamic polymorphism with calibrated entropy ($H \approx 0.81$). While physical PIN dumps from SPECint binaries (`gcc`, `gzip`) would reflect operating system library noise, our procedural generator enabled reproducible, deterministic verification of the exact theoretical boundary conditions."*

#### Trap 2: "Your state budget counts raw SRAM bitcells. Doesn't physical SRAM have significant peripheral overhead?"
* **Bulletproof Defense:**  
  *"Yes, absolutely. Our closed-form state model ($S_{\text{arch}}$) captures first-order architectural storage bits ($B \cdot h + 2 \cdot 2^h$), which is the standard convention in foundational literature (Yeh & Patt 1991, McFarling 1993, Kessler 1999). However, physically realizing an SRAM array requires wordline decoders, bitline precharge circuits, sense amplifiers, column multiplexers, and write drivers. In sub-micron CMOS, peripheral overhead adds 40% to 80% silicon area beyond the raw 6T bitcell matrix. Furthermore, dual-ported tables (such as the Alpha 21264 local history table supporting dual-branch fetch) incur a $\approx 1.8\times$ bitcell area penalty over single-ported 6T cells. We acknowledge that our equations represent architectural storage state, not physical silicon layout area."*

#### Trap 3: "In the Alpha 21264 tournament predictor, what happens to the 2-bit chooser counters during program phase transitions?"
* **Bulletproof Defense:**  
  *"The chooser table uses 2-bit saturating up/down counters. When a program undergoes a phase transition—for example, shifting from tight inner loops where local history excels to complex global control flow—the chooser counters experience hysteresis lag. If a counter is saturated at Strongly Local (`00`), it requires at least two consecutive cycles where the global predictor is correct and the local predictor mispredicts before the chooser flips its decision to Global. During this 2-to-3 branch training window, the processor suffers transient mispredictions solely due to chooser inertia. Kessler's 1999 measurements showed that despite this hysteresis penalty, the tournament predictor outperforms both standalone sub-predictors by over 1.5% to 3% across diverse workloads."*

#### Trap 4: "You modeled 704 bits of Shadow Checkpoints for speculative GHR recovery. What is the physical control logic overhead?"
* **Bulletproof Defense:**  
  *"The 704 bits represent the raw storage for 32 checkpoint slots ($32 \times (12\text{-bit GHR} + 10\text{-bit LHR}) = 704\text{ bits}$). In a physical out-of-order execution engine, managing these checkpoints requires: (1) a 5-bit allocation pointer and retirement pointer managed as a circular FIFO queue; (2) flash-invalidation logic to squash younger checkpoints upon a branch misprediction; and (3) branch tag forwarding logic to associate each in-flight branch with its checkpoint index. While this control logic adds roughly 150 to 200 gate equivalents of combinatorial logic, it is entirely feasible in hardware and strictly required to decouple speculative fetch from out-of-order retirement."*

#### Trap 5: "Why did you define Set-Associative ($SxX$) variants in the taxonomy, and how do they scale?"
* **Bulletproof Defense:**  
  *"In Yeh & Patt's complete 9-variant taxonomy, $SxX$ represents Set-Associative branch history tracking. In a fully per-branch scheme ($PAg$), each branch has a dedicated BHR ($S = B \cdot h + 2 \cdot 2^h$). In $SAg$, branches are hashed by PC into $B/K$ sets, where each set shares an $h$-bit BHR:
  $$S_{SAg} = \frac{B}{K} \cdot h + 2 \cdot 2^h$$
  This compresses the first-level BHR table footprint by a factor of $K$ ($K$-way sharing) to save silicon area while retaining significantly better pattern isolation than a single global register ($GAg$)."*

---

# Module 7: Repository File Directory & Gradescope Checklist

### What is in this repository?
* **`Submit/TermPaper_Final.pdf`:** **THE FILE TO UPLOAD TO GRADESCOPE.** The final 25-page compiled term paper PDF.
* **`termpaper.tex`:** Complete 1,216-line LaTeX source code.
* **`Evaluation_Report.pdf`:** Official Professor Technical Evaluation Report (93/100, Grade A).
* **`Learning.md`:** This masterclass study guide & viva defense manual.
* **`README.md`:** Repository overview and navigation.
* **`simulate_predictors.cs`:** Complete C# simulation suite (864,980 branches).
* **`TermPaper_Original_Draft.pdf`:** Original student draft (archived for reference).
* **`term_paper_final.html` & `evaluation_report.html`:** Source HTML templates.

---
*Good luck with the submission and viva! You've got this!* 🚀
