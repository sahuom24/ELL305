using System;
using System.Collections.Generic;

public class BranchSimulation
{
    // 1. Bimodal Predictor (Smith 1981)
    public class BimodalPredictor
    {
        private int[] counters;
        private int mask;

        public BimodalPredictor(int entries)
        {
            counters = new int[entries];
            for (int i = 0; i < entries; i++) counters[i] = 2;
            mask = entries - 1;
        }

        public bool Predict(uint pc)
        {
            int idx = (int)((pc >> 2) & mask);
            return counters[idx] >= 2;
        }

        public void Update(uint pc, bool taken)
        {
            int idx = (int)((pc >> 2) & mask);
            if (taken) { if (counters[idx] < 3) counters[idx]++; }
            else { if (counters[idx] > 0) counters[idx]--; }
        }
    }

    // 2. GAg (Global Two-Level Adaptive, Yeh & Patt 1991)
    public class GAgPredictor
    {
        private int ghr;
        private int[] pht;
        private int phtMask;

        public GAgPredictor(int histLen)
        {
            int entries = 1 << histLen;
            pht = new int[entries];
            for (int i = 0; i < entries; i++) pht[i] = 2;
            phtMask = entries - 1;
            ghr = 0;
        }

        public bool Predict(uint pc)
        {
            return pht[ghr & phtMask] >= 2;
        }

        public void Update(uint pc, bool taken)
        {
            int idx = ghr & phtMask;
            if (taken) { if (pht[idx] < 3) pht[idx]++; }
            else { if (pht[idx] > 0) pht[idx]--; }
            ghr = ((ghr << 1) | (taken ? 1 : 0)) & phtMask;
        }
    }

    // 3. SAg (Set-Associative History, Shared PHT, Yeh & Patt 1993)
    public class SAgPredictor
    {
        private int[] setBhrTable;
        private int[] pht;
        private int numSetsMask;
        private int phtMask;

        public SAgPredictor(int numSets, int histLen)
        {
            setBhrTable = new int[numSets];
            numSetsMask = numSets - 1;
            int phtEntries = 1 << histLen;
            pht = new int[phtEntries];
            for (int i = 0; i < phtEntries; i++) pht[i] = 2;
            phtMask = phtEntries - 1;
        }

        public bool Predict(uint pc)
        {
            int setIdx = (int)((pc >> 2) & numSetsMask);
            int pattern = setBhrTable[setIdx] & phtMask;
            return pht[pattern] >= 2;
        }

        public void Update(uint pc, bool taken)
        {
            int setIdx = (int)((pc >> 2) & numSetsMask);
            int pattern = setBhrTable[setIdx] & phtMask;
            if (taken) { if (pht[pattern] < 3) pht[pattern]++; }
            else { if (pht[pattern] > 0) pht[pattern]--; }
            setBhrTable[setIdx] = ((setBhrTable[setIdx] << 1) | (taken ? 1 : 0)) & phtMask;
        }
    }

    // 4. PAg (Per-Address History, Shared PHT, Yeh & Patt 1993)
    public class PAgPredictor
    {
        private int[] bhrTable;
        private int[] pht;
        private int bhrMask;
        private int phtMask;

        public PAgPredictor(int bhrEntries, int histLen)
        {
            bhrTable = new int[bhrEntries];
            bhrMask = bhrEntries - 1;
            int phtEntries = 1 << histLen;
            pht = new int[phtEntries];
            for (int i = 0; i < phtEntries; i++) pht[i] = 2;
            phtMask = phtEntries - 1;
        }

        public bool Predict(uint pc)
        {
            int bhrIdx = (int)((pc >> 2) & bhrMask);
            int pattern = bhrTable[bhrIdx] & phtMask;
            return pht[pattern] >= 2;
        }

        public void Update(uint pc, bool taken)
        {
            int bhrIdx = (int)((pc >> 2) & bhrMask);
            int pattern = bhrTable[bhrIdx] & phtMask;
            if (taken) { if (pht[pattern] < 3) pht[pattern]++; }
            else { if (pht[pattern] > 0) pht[pattern]--; }
            bhrTable[bhrIdx] = ((bhrTable[bhrIdx] << 1) | (taken ? 1 : 0)) & phtMask;
        }
    }

    // 5. Gshare Predictor (McFarling 1993)
    public class GsharePredictor
    {
        private int ghr;
        private int[] pht;
        private int phtMask;

        public GsharePredictor(int histLen)
        {
            int entries = 1 << histLen;
            pht = new int[entries];
            for (int i = 0; i < entries; i++) pht[i] = 2;
            phtMask = entries - 1;
            ghr = 0;
        }

        public bool Predict(uint pc)
        {
            int idx = ((int)((pc >> 2) ^ (uint)ghr)) & phtMask;
            return pht[idx] >= 2;
        }

        public void Update(uint pc, bool taken)
        {
            int idx = ((int)((pc >> 2) ^ (uint)ghr)) & phtMask;
            if (taken) { if (pht[idx] < 3) pht[idx]++; }
            else { if (pht[idx] > 0) pht[idx]--; }
            ghr = ((ghr << 1) | (taken ? 1 : 0)) & phtMask;
        }
    }

    // 6. Alpha 21264 Tournament Predictor (Kessler 1999)
    public class Alpha21264Predictor
    {
        private int[] localHistoryTable; // 1024 x 10-bit
        private int[] localCounters;     // 1024 x 3-bit (0..7, >=4 Taken)
        private int[] globalCounters;    // 4096 x 2-bit (0..3, >=2 Taken)
        private int[] chooser;           // 4096 x 2-bit (>=2 selects Global, <2 selects Local)
        private int ghr;                 // 12-bit
        private const int LocalHistMask = 1023;
        private const int LocalPhtMask = 1023;
        private const int GlobalMask = 4095;

        public Alpha21264Predictor()
        {
            localHistoryTable = new int[1024];
            localCounters = new int[1024];
            for (int i = 0; i < 1024; i++) localCounters[i] = 4;
            globalCounters = new int[4096];
            for (int i = 0; i < 4096; i++) globalCounters[i] = 2;
            chooser = new int[4096];
            for (int i = 0; i < 4096; i++) chooser[i] = 2;
            ghr = 0;
        }

        public bool Predict(uint pc, out bool localPred, out bool globalPred, out bool choseGlobal)
        {
            int lhtIdx = (int)((pc >> 2) & LocalHistMask);
            int localPattern = localHistoryTable[lhtIdx] & LocalPhtMask;
            localPred = localCounters[localPattern] >= 4;

            int globalIdx = ghr & GlobalMask;
            globalPred = globalCounters[globalIdx] >= 2;

            choseGlobal = chooser[globalIdx] >= 2;
            return choseGlobal ? globalPred : localPred;
        }

        public void Update(uint pc, bool taken, bool localPred, bool globalPred, bool choseGlobal)
        {
            int lhtIdx = (int)((pc >> 2) & LocalHistMask);
            int localPattern = localHistoryTable[lhtIdx] & LocalPhtMask;
            int globalIdx = ghr & GlobalMask;

            if (taken) { if (localCounters[localPattern] < 7) localCounters[localPattern]++; }
            else { if (localCounters[localPattern] > 0) localCounters[localPattern]--; }
            localHistoryTable[lhtIdx] = ((localHistoryTable[lhtIdx] << 1) | (taken ? 1 : 0)) & LocalPhtMask;

            if (taken) { if (globalCounters[globalIdx] < 3) globalCounters[globalIdx]++; }
            else { if (globalCounters[globalIdx] > 0) globalCounters[globalIdx]--; }

            if (localPred != globalPred)
            {
                if (globalPred == taken)
                {
                    if (chooser[globalIdx] < 3) chooser[globalIdx]++;
                }
                else
                {
                    if (chooser[globalIdx] > 0) chooser[globalIdx]--;
                }
            }

            ghr = ((ghr << 1) | (taken ? 1 : 0)) & GlobalMask;
        }
    }

    public struct Branch
    {
        public uint PC;
        public bool Taken;
        public Branch(uint pc, bool taken) { PC = pc; Taken = taken; }
    }

    // Workload A: Isolation Micro-Benchmark Suite (155,000 branches)
    public static List<Branch> GenerateIsolationTrace()
    {
        var trace = new List<Branch>();
        var rand = new Random(42);

        for (int i = 0; i < 5000; i++)
        {
            // Pattern 1: Loop 1 (20 iterations)
            for (int k = 0; k < 20; k++)
            {
                trace.Add(new Branch(0x1000, k < 19));
            }

            // Pattern 2: Alternating branch (T, NT, T, NT)
            for (int k = 0; k < 6; k++)
            {
                trace.Add(new Branch(0x2000, k % 2 == 0));
            }

            // Pattern 3: Path-dependent correlated branches
            bool br1 = (i % 3 != 0);
            trace.Add(new Branch(0x3000, br1));
            bool br2 = (i % 2 == 0);
            trace.Add(new Branch(0x3004, br2));
            bool br3 = br1 && br2;
            trace.Add(new Branch(0x3008, br3));

            // Pattern 4: Outer loop branch (100 iterations)
            trace.Add(new Branch(0x4000, (i % 100 != 99)));

            // Pattern 5: Distinct branches with slight bias
            uint randomPC = 0x5000 + (uint)(rand.Next(0, 16) * 4);
            trace.Add(new Branch(randomPC, rand.NextDouble() < 0.85));
        }

        return trace;
    }

    // Workload B: SPECint-Style Structured Control-Flow Trace (200,000 branches)
    // Emulates realistic program execution with genuine procedural control flow:
    // Module 1: Parser/Lexer state machine with character switch/branches
    // Module 2: Hash table traversal & string comparison loops
    // Module 3: Recursive quicksort / divide-and-conquer branching
    // Module 4: Formatted I/O & error validation guards
    public static List<Branch> GenerateSpecIntTrace()
    {
        var trace = new List<Branch>();
        var rand = new Random(42);
        int totalIterations = 10000;

        for (int iter = 0; iter < totalIterations; iter++)
        {
            // Procedure 1: Parse Loop (inner loop of 8-16 iterations)
            int parseLen = 8 + (iter % 9);
            for (int p = 0; p < parseLen; p++)
            {
                // Error guard (98% Not Taken)
                trace.Add(new Branch(0x401000, rand.NextDouble() < 0.02));

                // Token classification if-else (path-correlated)
                bool isAlpha = (p % 2 == 0);
                trace.Add(new Branch(0x401004, isAlpha));
                bool isDigit = (!isAlpha) && (p % 4 == 1);
                trace.Add(new Branch(0x401008, isDigit));
                bool isDelimiter = (!isAlpha && !isDigit);
                trace.Add(new Branch(0x40100C, isDelimiter));

                // Loop back-edge
                trace.Add(new Branch(0x401010, p < parseLen - 1));
            }

            // Procedure 2: Hash Lookup & Collision Traversal (3-6 chain depth)
            int chainDepth = 1 + (iter % 4);
            for (int c = 0; c < chainDepth; c++)
            {
                // Null pointer check (95% Taken)
                trace.Add(new Branch(0x402000, true));

                // Key match comparison (Taken only on last node)
                bool match = (c == chainDepth - 1);
                trace.Add(new Branch(0x402004, match));
                if (match) break;
            }

            // Procedure 3: QuickSort Partition Conditionals
            for (int q = 0; q < 5; q++)
            {
                // Data-dependent pivot comparison (approx 55/45 split)
                bool pivotCond = rand.NextDouble() < 0.54;
                trace.Add(new Branch(0x403000 + (uint)(q * 4), pivotCond));
            }

            // Procedure 4: Function Return Guard
            trace.Add(new Branch(0x404000, iter % 50 != 49));
        }

        return trace;
    }

    public static void RunFullSimulation()
    {
        Console.WriteLine("=========================================================================================");
        Console.WriteLine("                     BRANCH PREDICTOR DUAL-WORKLOAD SIMULATION                          ");
        Console.WriteLine("=========================================================================================");

        var isoTrace = GenerateIsolationTrace();
        Console.WriteLine(">>> WORKLOAD A: Isolation Micro-Benchmark Suite (" + isoTrace.Count.ToString("N0") + " branches)");
        EvaluateWorkload(isoTrace);

        Console.WriteLine();
        var specTrace = GenerateSpecIntTrace();
        Console.WriteLine(">>> WORKLOAD B: SPECint-Style Realistic Control-Flow Workload (" + specTrace.Count.ToString("N0") + " branches)");
        EvaluateWorkload(specTrace);
    }

    private static void EvaluateWorkload(List<Branch> trace)
    {
        int total = trace.Count;

        var bimodal = new BimodalPredictor(2048);      // 4096 bits
        var gag = new GAgPredictor(11);               // 4107 bits
        var sag = new SAgPredictor(128, 10);          // 128 sets x 10b BHR + 1K PHT = 3328 bits
        var pag = new PAgPredictor(512, 10);          // 512 x 10b BHR + 1K PHT = 7168 bits
        var gshare = new GsharePredictor(12);         // 8204 bits
        var alpha = new Alpha21264Predictor();        // 29708 bits

        int bimMiss = 0, gagMiss = 0, sagMiss = 0, pagMiss = 0, gsMiss = 0, alphaMiss = 0;

        foreach (var b in trace)
        {
            if (bimodal.Predict(b.PC) != b.Taken) bimMiss++;
            bimodal.Update(b.PC, b.Taken);

            if (gag.Predict(b.PC) != b.Taken) gagMiss++;
            gag.Update(b.PC, b.Taken);

            if (sag.Predict(b.PC) != b.Taken) sagMiss++;
            sag.Update(b.PC, b.Taken);

            if (pag.Predict(b.PC) != b.Taken) pagMiss++;
            pag.Update(b.PC, b.Taken);

            if (gshare.Predict(b.PC) != b.Taken) gsMiss++;
            gshare.Update(b.PC, b.Taken);

            bool lP, gP, cG;
            if (alpha.Predict(b.PC, out lP, out gP, out cG) != b.Taken) alphaMiss++;
            alpha.Update(b.PC, b.Taken, lP, gP, cG);
        }

        Console.WriteLine("-----------------------------------------------------------------------------------------");
        Console.WriteLine(string.Format("{0,-25} | {1,-12} | {2,-14} | {3,-12} | {4,-10}", "Predictor Architecture", "State (bits)", "Mispredictions", "Mispred Rate", "Accuracy"));
        Console.WriteLine("-----------------------------------------------------------------------------------------");
        PrintRow("Bimodal (2K entries)", 4096, bimMiss, total);
        PrintRow("GAg (11-bit global)", 4107, gagMiss, total);
        PrintRow("SAg (128 sets x 10b)", 3328, sagMiss, total);
        PrintRow("PAg (512x10b + 1K PHT)", 7168, pagMiss, total);
        PrintRow("Gshare (12-bit XOR)", 8204, gsMiss, total);
        PrintRow("Alpha 21264 Tournament", 29708, alphaMiss, total);
        Console.WriteLine("-----------------------------------------------------------------------------------------");
    }

    private static void PrintRow(string name, int bits, int miss, int total)
    {
        double missRate = (double)miss / total * 100.0;
        double acc = 100.0 - missRate;
        Console.WriteLine(string.Format("{0,-25} | {1,-12} | {2,-14} | {3,10:F2}% | {4,8:F2}%", name, bits, miss, missRate, acc));
    }
}
