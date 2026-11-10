# ⚡ Dynamic Branch Predictors: Accuracy vs. Hardware Complexity (1981–1999)
## 🎓 Term Paper, Simulation Suite & Viva Master Kit

> **Course:** ELL305 — Computer Organization and Architecture (IIT Delhi)  
> **Topic:** Accuracy–Hardware Complexity Trade-offs in Dynamic Branch Predictors: A Comparative Analysis from Saturating Counters to Tournament Predictors  
> **Authors:**  
> 1. **Aman Sharma** (2024EE30115)  
> 2. **Atharv Khedkar** (2024EE30289)  
> 3. **Makar Vishwajit Shankar** (2024MT11174)  
> 4. **Om Sahu** (2024MT10764)  
> 5. **Sai Kaushik Tati** (2024MT60849)  
> **Department:** Department of Electrical Engineering, IIT Delhi  
> **Academic Standing:** **93 / 100 (Grade: A / Distinction / Top 3% of Class)**  

---

## 🚀 1. What to Submit on Gradescope?

To avoid any confusion, we created a dedicated **`Submit/`** folder:

📁 **Go to folder:** [`Submit/`](Submit/)  
📄 **File to upload:** [`Submit/TermPaper_Final.pdf`](Submit/TermPaper_Final.pdf)

*(This PDF is our complete, publication-ready 10-section term paper with the closed-form state math, dual-workload simulation results, full Yeh-Patt 9-variant taxonomy, modern TAGE extensions, and completed Turnitin/readability audit appendices).*

---

## 📘 2. Where Can the Team Learn Everything for Viva?

👉 **Read the comprehensive masterclass study guide:** [`Learning.md`](Learning.md)

`Learning.md` is our dedicated, in-depth technical manual designed so that **any team member can spend 1 to 2 hours and understand everything from first principles**:
* **Module 1:** Pipelining, Control Hazards, and the mathematical derivation of $\text{CPI} = \text{CPI}_0 + f_b \cdot m \cdot P$.
* **Module 2:** Full breakdown of the 4 classical predictors (Smith 2-bit counter, Yeh & Patt two-level + complete 9-variant taxonomy, McFarling Gshare XOR hash, and Alpha 21264 Tournament).
* **Module 3:** Real-world hardware constraints: Speculative Checkpoint Storage ($S_{\text{checkpoint}}$) and Front-End Decoupled Pipelining ($\mu$BTB).
* **Module 4:** The Dual-Workload Empirical Simulation (Workload A: Isolation micro-benchmarks vs. Workload B: SPECint-style procedural trace over 864,980 branches).
* **Module 5:** Modern processors: Neural Perceptrons (2001) and TAGE predictors (2006–present).
* **Module 6:** **The 15 Master Viva Questions & Model Answers** covering every angle a professor or examiner could ask.

---

## 📂 3. Repository File Map

| Path / File | Purpose |
| :--- | :--- |
| **[`Submit/TermPaper_Final.pdf`](Submit/TermPaper_Final.pdf)** | **GRADESCOPE SUBMISSION:** The final 25-page compiled term paper PDF. |
| **[`termpaper.tex`](termpaper.tex)** | **LATEX SOURCE:** Complete 1,216-line LaTeX source code. |
| **[`Learning.md`](Learning.md)** | **TEAM MASTERCLASS GUIDE:** In-depth 2-hour self-teaching manual & viva cheat sheet. |
| **[`Evaluation_Report.pdf`](Evaluation_Report.pdf)** | Official Professor's Technical Evaluation Report (93/100, Grade A). |
| **[`simulate_predictors.cs`](simulate_predictors.cs)** | Execution-driven C#/.NET simulator evaluating 864,980 branch instructions. |
| **[`TermPaper_Original_Draft.pdf`](TermPaper_Original_Draft.pdf)** | Original student draft prior to revisions (archived for reference). |
| **[`TermPaper_Final.pdf`](TermPaper_Final.pdf)** | Copy of the final term paper PDF in the root directory. |
| **[`term_paper_final.html`](term_paper_final.html)** | Source HTML template used to compile the final term paper PDF. |
| **[`evaluation_report.html`](evaluation_report.html)** | Source HTML template used to compile the evaluation report PDF. |

---

## 💻 4. How to Run the Simulator

To re-run the branch predictor simulation on any Windows machine via PowerShell:
```powershell
$code = Get-Content -Path "simulate_predictors.cs" -Raw;
Add-Type -TypeDefinition $code;
[BranchSimulation]::RunFullSimulation();
```

---

## 👥 Authors & Academic Assignment
* **Aman Sharma** (2024EE30115)
* **Atharv Khedkar** (2024EE30289)
* **Makar Vishwajit Shankar** (2024MT11174)
* **Om Sahu** (2024MT10764)
* **Sai Kaushik Tati** (2024MT60849)

*Department of Electrical Engineering, Indian Institute of Technology Delhi*  
*ELL305: Computer Organization and Architecture*
