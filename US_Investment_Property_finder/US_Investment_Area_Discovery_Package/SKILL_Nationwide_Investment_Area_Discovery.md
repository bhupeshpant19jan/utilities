---
name: nationwide-investment-area-discovery
description: Score one or more user-specified cities or discover US markets using separate Rental Market Attractiveness and Investment Feasibility scores, plus ZIP-level discovery. Produces evidence-backed city rankings and a four-tab Excel workbook. Area discovery only; not individual property listing searches.
---

# Nationwide Investment Area Discovery Skill

## Purpose and boundary

Answer **where should the investor look?** Support three modes: (1) score a city or user-supplied city list, (2) discover promising US markets nationwide, and (3) group nearby ZIP codes into geographic clusters and score each ZIP. City scoring must calculate Rental Market Attractiveness (RMA) and Investment Feasibility (IF) separately, then provide a clearly labeled combined search-priority score. ZIP-level scoring and mandatory gates remain separate.

This skill does **not** search for or underwrite individual property listings. A separate property-discovery workflow evaluates actual properties in selected areas. Keep the two projects independent.

## Source of truth

Follow `US_Investment_Area_Discovery_Master_Prompt.md` for the complete investor constraints, mandatory gates, weights, evidence standards, workbook schema, and quality checks. The master prompt controls if any instruction conflicts with this skill. Do not change finalized criteria without explicit user direction.

## Operating modes — select from the user's request

Choose the narrowest mode that fulfills the request; do not force the nationwide wizard for a simple city-score request.

1. **City Score Mode:** If the user supplies one city or a list of cities, score exactly those cities. Accept inputs such as `Plano, TX`, `Frisco, TX`, or a comma/newline-separated list. If a city name is ambiguous, ask one concise clarification for state/metro. Do not silently add nearby cities. If the user supplies a state/metro, preserve that scope.
2. **Nationwide Discovery Mode:** If the user asks to discover/find/rank the best US markets or ZIP codes, run the nationwide workflow and wizard/profile rules.
3. **Refresh/Compare Mode:** If the user asks to update, compare, or rerank previously scored cities/ZIPs, reuse the prior scope and confirmed profile; update only the requested dimension.

City Score Mode must work with a single city or multiple cities. Reuse the confirmed investor profile where available. If essential assumptions are missing, ask only for the missing item(s); otherwise use the confirmed profile and proceed. For this user's current DFW investment profile, use: purchase price cap $350,000; 20% and 25% down-payment scenarios; total initial cash ceiling $150,000; maximum reserve-adjusted owner contribution $300/month; 10-year horizon; balanced rental income/appreciation; legally rentable residential property. Treat these as user-specific defaults only when the active profile confirms them; for other users, use their saved profile or ask.

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
Explain the three distinct score systems; never blend or present them as interchangeable:

- **City RMA (0–10):** rental demand/transaction activity 25%; leasing velocity 20%; rental supply/competition 20%; rent growth/stability 15%; renter-pool/employment diversity 10%; rental affordability/resilience 10%.
- **City IF (0–10):** purchase-price affordability 20%; rental yield/cap-rate potential 20%; financing/DSCR feasibility 20%; reserve-adjusted cash flow 15%; total initial cash 10%; carrying costs/tax exposure 10%; suitable inventory 5%.
- **Combined city search priority (0–10):** 40% RMA + 60% IF; always show both underlying scores.
- **ZIP discovery score (0–100):** retain the existing weights: rental demand 15; employment diversity/access 15; tenant income/resilience 10; rental yield/break-even 20; crime/safety 10; school quality/stability 10; tax/insurance/HOA 10; population/infrastructure 5; resale liquidity/economic diversity 5.

City rating bands: 9.0–10 Exceptional; 8.0–8.9 Very Strong; 7.0–7.9 Good; 6.0–6.9 Moderate; below 6.0 Weak. ZIP strength bands remain 90–100 Exceptional; 80–89 Very Strong; 70–79 Good; 60–69 Moderate; below 60 Weak. Explain that scores and qualification status are separate.

**Step 5 — Report preferences**
For a short city list, default to a concise comparison in chat; create a workbook when the list is large or the user requests one. For nationwide discovery, default to one Excel workbook with four tabs:
1. Executive Summary
2. Metro & ZIP Details
3. ZIP Parameter Scorecard — wide format, one row per ZIP
4. Methodology & Assumptions

For a city-mode workbook, Tab 3 becomes a City Scorecard with one row per city and separate RMA/IF component columns. Ask only if the user requests a different deliverable.

**Step 6 — Review and confirm**
Provide a compact setup summary. Ask the user to confirm before launching the first full research run. After confirmation, create or update a concise configuration/profile note alongside the deliverable if the working environment supports it. Do not claim that setup is saved persistently unless the file or supported memory mechanism is actually created.

### If setup is incomplete during a scheduled run

A scheduled run must not repeatedly ask the user setup questions. If the profile has not been confirmed, produce a short setup-needed notice and request completion of the wizard; do not invent missing preferences or proceed with assumptions that materially affect ranking. Once confirmed, scheduled runs should execute without reopening the wizard unless a material input is missing or the user asks to change it.

## City Score Mode — required scoring method

For every requested city, calculate and report both scores independently using the complete formulas in the master prompt:

- **Rental Market Attractiveness (RMA), 0–10:** rental demand/transaction activity 25%; leasing velocity 20%; rental supply/competition 20%; rent growth/stability 15%; renter-pool/employment diversity 10%; rental affordability/resilience 10%.
- **Investment Feasibility (IF), 0–10:** purchase-price affordability 20%; rental yield/cap-rate potential 20%; financing/DSCR feasibility 20%; reserve-adjusted cash flow 15%; total initial cash requirement 10%; carrying costs/tax exposure 10%; suitable inventory availability 5%.
- **Combined Search Priority (0–10):** `0.40 × RMA + 0.60 × IF`. This is a prioritization aid only; always display RMA and IF beside it. Never let the combined score conceal a weak component or override a hard gate.

Score each component from 0–10 using the explicit metric anchors in the master prompt. For peer-relative metrics, benchmark against a consistent cohort of at least 10 comparable US cities/metros where possible, matched for market size, target property type, housing-cost context and data availability. Score all requested cities against the same benchmark cohort and use the requested list for the final ranking. For a single city, use the same national/comparable-market benchmark cohort plus its multi-year trend. If fewer than 10 credible peers are available, disclose the cohort size and lower confidence; never manufacture a peer percentile. Show each component score, weight, weighted contribution, key underlying metrics, evidence date, source, and confidence.

Use representative city-level property samples, not hand-picked best deals. Target at least 10 relevant active/closed sale and rental comparables per city; 5–9 supports a provisional score; fewer than 5 means mark the affected IF components and overall IF as low-confidence/provisional. Stratify by property type and bedroom count where possible. Do not use a city median across all property types if the investor profile targets single-family/townhomes, etc. Do not imply that city-level underwriting confirms any specific property.

**City score ratings (0–10):** 9.0–10.0 Exceptional; 8.0–8.9 Very Strong; 7.0–7.9 Good; 6.0–6.9 Moderate; below 6.0 Weak. Also label combined search priority using the same bands. Round displayed scores to one decimal, but retain full precision for calculations.

**City-level qualification is not a property pass.** Show a separate city research status: `Strong Search Market`, `Selective`, `Low Priority`, or `Insufficient Evidence`. Property-specific school assignment, legal rental status, HOA, condition, hazard, and exact financial gates remain unverified until an address is selected. Do not mark an entire city as passing the address-specific school gate. If the user asks for a city score only, do not search individual property listings or produce property recommendations.

**City Score Mode output:**
1. Scope and as-of date; exact cities evaluated; profile/financing assumptions.
2. Ranked comparison table: city/state, RMA, RMA rating, IF, IF rating, combined priority, status, confidence.
3. Two score breakdowns per city with component scores/weights and decisive metrics.
4. Strengths, constraints, data gaps and why each city ranks where it does.
5. Sources and dates; distinguish closed/achieved rents from asking rents and observed facts from estimates.
6. Clear statement that rankings are relative to the requested list and current evidence, not a guarantee or property-level recommendation.

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

- Nationwide discovery when requested; when a city/list is supplied, score exactly that scope.
- City RMA and IF are separate, transparent 0–10 scores; combined priority is supplementary.
- Every ZIP has its own score and gate status.
- All three school levels are checked separately; no averaging.
- No high score can override a mandatory-gate failure.
- Separate observed data, estimates, and proxies.
- Cite material evidence and record research dates.
- Do not use protected demographic characteristics as investment-quality or tenant-screening criteria.
- No individual listing research in this project.
