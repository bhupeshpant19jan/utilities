# US Investment Area Discovery

**A quick guide to setup, the weekly research workbook, and how to read the results.**

## What this tool does

It can score a city or a user-provided list of cities, discover promising US markets, and group nearby ZIP codes into geographic clusters. City reports calculate two separate scores: Rental Market Attractiveness (RMA) and Investment Feasibility (IF). ZIP discovery retains its own 0–100 score.

**It answers “where should I look?” — not “which house should I buy?”** Individual property searches and financial underwriting remain a separate workflow.

## Score one city or a city list

You can simply ask: **“Score Plano, TX”** or **“Compare Plano, Frisco, McKinney and Princeton, TX.”** The skill scores exactly the cities supplied and returns RMA, IF, a combined search-priority score, ratings, confidence and evidence. If a city name is ambiguous, it asks for the state. A city-only request does not trigger nationwide discovery.

- **RMA (0–10):** demand/transactions 25%; leasing velocity 20%; rental supply 20%; rent growth/stability 15%; renter-pool/employment diversity 10%; affordability/resilience 10%.
- **IF (0–10):** purchase affordability 20%; cap-rate potential 20%; DSCR 20%; reserve-adjusted cash flow 15%; initial cash 10%; carrying costs/taxes 10%; suitable inventory 5%.
- **Combined priority:** 40% RMA + 60% IF. Both underlying scores always remain visible; the combined score cannot override a property-level hard gate.

**Rating bands:** 9.0–10 Exceptional; 8.0–8.9 Very Strong; 7.0–7.9 Good; 6.0–6.9 Moderate; below 6.0 Weak. City scores are market-level comparisons, not guarantees or approval of a specific property. Small or weak data samples are marked provisional/low confidence.

## First run: guided setup

The first time you run the skill, it walks you through a short conversational setup. Known preferences are pre-filled; you only confirm or change them.

| Input | Starting setting |
|---|---|
| Purchase-price target | About $350,000 or less |
| Financing | Model 20% and 25% down; total initial cash ceiling is configured separately |
| Target rent | About $2,500/month, where local evidence supports it |
| Property type / size | SFR, townhome, rental-permitted condo, duplex or small multifamily; 2–4 bedrooms |
| Holding period | 10–15 years |
| Geography | Nationwide; DFW, Seattle, Florida, NJ, Boston and Atlanta are starting points, not limits |
| School requirement | Elementary, middle and high school each rated at least 8/10 |
| Report | One Excel workbook, four tabs; ZIP scorecard in wide format |

The wizard confirms the mandatory gates, scoring weights, and report layout before the first full research run. You can change an input later; changes should be documented in the workbook methodology.

## The Excel workbook: four tabs

For city-list mode, the Executive Summary ranks requested cities using RMA and IF; City Details shows each component and evidence. Nationwide mode retains the metro/ZIP discovery and wide ZIP scorecard.

1. **Executive Summary** — the quick decision view: strongest metros/clusters, ZIP codes, scores, strength, qualification, confidence, and why they rank.
2. **Metro & ZIP Details** — nearby ZIP codes grouped by metro and geographic cluster. Each ZIP still has its own score.
3. **ZIP Parameter Scorecard** — one row per ZIP. Separate columns show each parameter's score, evidence/metric, and confidence, plus mandatory-gate results and key risks.
4. **Methodology & Assumptions** — scoring weights, gate definitions, thresholds, assumptions, sources, and report date.

## How to interpret the results

**City scores (0–10)** report RMA and IF separately; combined search priority is 40% RMA + 60% IF. **ZIP score (0–100)** remains a separate weighted ZIP-code score. Do not compare the city and ZIP scales directly.

| Score | Strength |
|---:|---|
| 90–100 | Exceptional |
| 80–89 | Very Strong |
| 70–79 | Good |
| 60–69 | Moderate |
| Below 60 | Weak |

**Qualification is separate from score.**
- **Qualified:** available evidence supports all mandatory gates.
- **Conditional:** promising, but one or more items need verification or improvement.
- **Failed:** a mandatory gate is not met.
- **Insufficient Evidence:** there is not enough reliable information to decide.

A ZIP scoring 88 can still be disqualified if a mandatory school or safety gate fails. Do not use the score alone as a buy decision.

**Confidence** tells you how dependable the evidence is:
- **High:** strong official or ZIP-/address-specific evidence.
- **Medium:** credible local or market-level evidence with limitations.
- **Low:** proxy, asking-rent, stale, incomplete, or less-specific data.

For schools, check the elementary, middle, and high school ratings individually—never an average. School assignments, HOA rental rules, insurance, and property hazards often require a specific property address to confirm.

## What to do next

Use the Executive Summary to choose clusters, then use the ZIP scorecard to compare ZIP codes within them. Pass the selected ZIP codes to the separate property-search workflow for listing-level analysis, rent comps, financing, cash flow, and property due diligence.

**Important:** Market screening is not financial, legal, tax, insurance, or school-placement advice. Verify current facts and address-specific conditions before making an investment decision.
