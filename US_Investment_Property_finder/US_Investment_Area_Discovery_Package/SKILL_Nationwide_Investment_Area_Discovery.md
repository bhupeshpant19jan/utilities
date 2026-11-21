---
name: nationwide-investment-area-discovery
description: Guided first-time setup and recurring research of US metropolitan areas, geographic clusters, and ZIP codes for residential rental-investment suitability. Produces a four-tab Excel workbook with a wide-format ZIP parameter scorecard. Area discovery only; not individual property listing searches.
---

# Nationwide Investment Area Discovery Skill

## Purpose and boundary

Answer **where should the investor look?** Discover promising US markets, group nearby ZIP codes into geographic clusters, score each ZIP, validate mandatory gates, and deliver a concise Excel workbook.

This skill does **not** search for or underwrite individual property listings. A separate property-discovery workflow evaluates actual properties in selected areas. Keep the two projects independent.

## Source of truth

Follow `US_Investment_Area_Discovery_Master_Prompt.md` for the complete investor constraints, mandatory gates, weights, evidence standards, workbook schema, and quality checks. The master prompt controls if any instruction conflicts with this skill. Do not change finalized criteria without explicit user direction.

## First-run guided setup wizard

**Run the setup wizard before conducting the first research cycle.** Do not jump straight into nationwide research.

### Wizard behavior

- Guide the user conversationally, one step at a time. Do not send a long questionnaire.
- Pre-populate known and finalized settings from the master prompt and prior setup context. Do not ask the user to repeat known information.
- At each step, show the current default and ask whether to keep or change it. Accept a simple “keep defaults” response when all values are correct.
- Ask only for missing or ambiguous inputs that materially affect the research.
- Explain briefly why an input matters when asking for it.
- Summarize the completed setup and obtain confirmation before starting the first full research run.
- If the user changes a value, update the working configuration and reflect it in the Methodology & Assumptions tab.
- Never silently loosen a mandatory gate or change a score weight to make more ZIP codes qualify.

### Wizard steps and default profile

**Step 1 — Investment profile**
Present the known defaults:
- Target purchase price: approximately $350,000 or less.
- Down-payment target: less than $100,000; closing costs, repairs, and reserves are separate.
- Target rent: approximately $2,500/month or more where local evidence supports it.
- Property types: single-family, townhome, rental-permitted condo, duplex, and small multifamily.
- Target size: 2–4 bedrooms.
- Investment horizon: 10–15 years.
- Objective: strong rental demand and a credible path to full carrying-cost break-even or positive cash flow.

Ask the user to confirm or identify changes. Do not treat the down-payment cap as a total cash-to-close cap.

**Step 2 — Geographic scope**
Default to nationwide discovery across all US markets. Treat DFW, Greater Seattle, Florida, New Jersey, Greater Boston, and Atlanta as initial candidate markets only—not an exclusive list. Ask whether to retain nationwide scope or apply any explicit exclusions. Do not assume the user's current location is the investment location.

**Step 3 — Mandatory qualification gates**
Show and confirm the finalized gates:
- Relevant 2–4 bedroom rental demand.
- Approximately $2,500 rent feasibility supported by local evidence.
- Elementary, middle, and high schools each at least 8/10; never average the three.
- No serious local safety/crime red flags.
- Plausible affordability within the purchase/down-payment targets.
- No known rental restriction that makes the strategy impractical.
- No obvious systemic physical-risk or insurance issue.

Explain that school boundaries, rental permissions, insurance, and hazards often require address-level verification later. ZIP-level qualification can be provisional when those details cannot be established at ZIP level.

**Step 4 — Scoring model**
Present the fixed weights and confirm they remain unchanged:
- Rental demand and tenant availability — 15
- Employment diversity and job accessibility — 15
- Tenant income and financial resilience — 10
- Rental yield and break-even potential — 20
- Crime and neighborhood safety — 10
- School quality and stability — 10
- Property tax, insurance, and HOA burden — 10
- Population growth and infrastructure — 5
- Resale liquidity and economic diversity — 5

Strength bands: Exceptional 90–100; Very Strong 80–89; Good 70–79; Moderate 60–69; Weak below 60. Explain that score and qualification status are separate.

**Step 5 — Report preferences**
Default to one Excel workbook with four tabs:
1. Executive Summary
2. Metro & ZIP Details
3. ZIP Parameter Scorecard — wide format, one row per ZIP
4. Methodology & Assumptions

Confirm the wide format. Ask only if the user requests a different deliverable.

**Step 6 — Review and confirm**
Provide a compact setup summary. Ask the user to confirm before launching the first full research run. After confirmation, create or update a concise configuration/profile note alongside the deliverable if the working environment supports it. Do not claim that setup is saved persistently unless the file or supported memory mechanism is actually created.

### If setup is incomplete during a scheduled run

A scheduled run must not repeatedly ask the user setup questions. If the profile has not been confirmed, produce a short setup-needed notice and request completion of the wizard; do not invent missing preferences or proceed with assumptions that materially affect ranking. Once confirmed, scheduled runs should execute without reopening the wizard unless a material input is missing or the user asks to change it.

## Research workflow after setup

### 1. Establish the research universe

- Search nationwide; do not constrain the universe to DFW or any previously named metro.
- Include initial candidate markets as starting points, not a closed list.
- Add other metros when evidence suggests stronger opportunities.
- Record metros considered, inclusion/exclusion rationale, and evidence confidence.
- Use metro data for discovery and ZIP-specific data for qualification.

### 2. Build geographic clusters

Organize as US → metro/region → geographic cluster → city → ZIP code.

- Group geographically close ZIP codes to make later property research structured.
- Give every ZIP a unique stable ID, such as `DFW-NORTH-75009`.
- Every ZIP receives its own score and qualification status; never substitute a cluster average.
- Note when ZIPs cross city, school district, police reporting, or other administrative boundaries.

### 3. Research each candidate ZIP

Collect evidence for:
- Rental demand and actual leased rent comps where available; label asking rents separately.
- Employment sectors, major operating employers, job accessibility and commute context.
- Household income and rent affordability.
- Housing prices, rental yields, vacancy, inventory, concessions and competing supply.
- Violent and property crime, local trends and geographic limitations.
- Assigned elementary, middle and high schools, each individually rated; the mandatory threshold is at least 8/10 for all three.
- Property tax, rental/non-homestead tax treatment, insurance pressure and HOA/rental restrictions.
- Population growth, infrastructure, resale liquidity and economic diversity.
- Flood, wildfire, hail, storm and other material physical/insurance risks.

Use strong local/official sources and record source URLs, dates, geographic coverage, and confidence. Do not invent missing data.

### 4. Apply mandatory gates

Apply gates before calculating recommendation status. A strong weighted score cannot override a failed mandatory gate.

Use `Qualified`, `Conditional`, `Failed`, or `Insufficient Evidence`. A ZIP is `Qualified` only when all mandatory gates are supported. Where school boundaries or rental rules are property-specific, flag address-level verification; do not assume a ZIP-wide pass.

### 5. Calculate weighted scores

Use the fixed 100-point model from the master prompt. Score each parameter 0–100 and calculate the weighted total. If a material parameter lacks sufficient evidence, mark it `N/A` and the overall score provisional rather than inventing a value or treating missing evidence as average.

Strength and qualification status are separate. A high-scoring ZIP that fails a mandatory gate must not be represented as qualified.

### 6. Create the Excel workbook

Create one workbook with exactly these four primary tabs:

1. **Executive Summary** — ranked metros/clusters and ZIPs, individual ZIP score, strength, qualification status, confidence, and brief rationale.
2. **Metro & ZIP Details** — geographic hierarchy, cluster grouping, cities, ZIPs, and cluster context.
3. **ZIP Parameter Scorecard** — wide format, one row per ZIP, separate score/evidence/confidence columns for each parameter, mandatory gate results, risks, source links and research dates.
4. **Methodology & Assumptions** — weights, gates, thresholds, confidence definitions, source hierarchy, financial screening assumptions, version and date.

Use filters, frozen headers, readable widths, wrapped evidence cells, restrained conditional formatting, and stable ZIP IDs. Keep the workbook concise and easy to scan. Do not create a property-listing report.

### 7. Refresh and compare

On each weekly run:
- Revisit shortlisted ZIPs and search for newly emerging markets nationwide.
- Capture meaningful changes and preserve previous scores where available.
- Report score delta and reason for change.
- Mark stale or unavailable data honestly.
- Do not treat missing new data as proof that nothing changed.
- Keep the separate property-search automation independent.

### 8. Deliver

Provide the Excel workbook and a short decision summary:
- Best-performing metros and clusters.
- Highest-scoring qualified ZIP codes.
- Conditional candidates worth watching.
- Failed gates and key risks.
- New entrants, score changes, and data limitations.

## Non-negotiable quality rules

- Nationwide discovery, not DFW-only.
- Every ZIP has its own score and gate status.
- All three school levels are checked separately; no averaging.
- No high score can override a mandatory-gate failure.
- Separate observed data, estimates, and proxies.
- Cite material evidence and record research dates.
- Do not use protected demographic characteristics as investment-quality or tenant-screening criteria.
- No individual listing research in this project.
