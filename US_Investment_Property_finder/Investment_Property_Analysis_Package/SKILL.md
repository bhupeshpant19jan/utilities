---
name: investment-property-analysis
description: Research, screen, compare, and monitor residential investment-property opportunities using evidence-based rental underwriting, school verification, risk scoring, and concise aggregated reports. Use for listing evaluation, property shortlists, market monitoring, or detailed investment analysis.
---

# Investment Property Analysis Skill

## Purpose

Conduct disciplined residential investment-property research. The default workflow is an aggregated opportunity-discovery report. Generate a full property-level due-diligence report only when requested.

This skill supports research and analysis. It does not make investment decisions, guarantee returns, or replace legal, tax, lending, inspection, insurance, or other professional advice.

## Required companion files and how they work

Keep these files in the same skill folder:

- `SKILL.md` — entry point and execution workflow.
- `Investment_Property_Analysis_Prompt.md` — authoritative, detailed research specification.
- `INVESTOR_PROFILE_TEMPLATE.md` — suggested first-run defaults and the fields to configure.
- `README.md` — installation and customization notes.

**The skill uses the companion prompt file.** At the start of every new task, read `Investment_Property_Analysis_Prompt.md` and follow it as the source of truth for screening, underwriting, scoring, evidence standards, and report formats. Do not copy its rules into a second competing prompt or silently replace them with generic defaults. If the file is missing, explain that the package is incomplete and ask the user to provide it; do not pretend the full specification was loaded.

The companion prompt and this skill should remain together in the same folder so the relative file reference works reliably. If a platform requires a different installation layout, preserve the relationship and update the file path explicitly.

## First-run guided setup — mandatory wizard

Run this setup before the first search whenever no confirmed investor profile is available. Treat it as a short conversational wizard, not a long form.

### Wizard sequence

**Step 1 — Load and display defaults.** Read `INVESTOR_PROFILE_TEMPLATE.md` and the Investor Profile table in `Investment_Property_Analysis_Prompt.md`. Show the suggested settings in a compact table before asking questions. Explicitly say that DFW geography and thresholds are examples, not universal recommendations.

**Step 2 — Ask for the minimum required inputs.** Ask in one concise message, grouped so the user can answer naturally:
- Target market (metro/region and preferred cities or ZIP codes).
- Maximum purchase price and maximum total cash available for each acquisition.
- Investment horizon and objective (cash flow, appreciation, or balanced).
- Financing: down payment, interest rate, and loan term; if unknown, ask whether the user authorizes current market-based assumptions, clearly labeled as estimates.
- Confirm or change any non-negotiable requirement, especially school-quality/attendance-zone criteria and property types.

The user may simply say **“Accept the suggested defaults”** and answer only the financing items still missing. Permit partial answers; retain displayed defaults for unanswered optional preferences. Do not ask for information already present in the conversation or a confirmed profile.

**Step 3 — Validate only essential gaps.** Before searching, the minimum confirmed items are: target market, purchase-price ceiling, total-cash ceiling, investment horizon/objective, and a usable financing basis (user-provided or explicitly authorized estimate). Confirm the local school-quality rule or record that no school gate is requested. If an essential item is missing, ask only for that item in a single direct follow-up. Do not restart the wizard.

**Step 4 — Read back and confirm.** Summarize the proposed profile, clearly marking any market-based financing assumptions. Ask for confirmation only if a value is ambiguous or materially consequential; otherwise a clear acceptance of defaults is sufficient. Do not begin candidate screening until essential settings are confirmed.

**Step 5 — Persist and proceed.** Save the confirmed settings as `investor_profile.json` where the platform supports persistent skill files. Save only confirmed values. If persistence is unavailable, retain the profile in the active conversation and tell the user it may need to be provided in a new session. Then begin research using the companion prompt.

### Suggested DFW example defaults to display

| Setting | Suggested default |
|---|---|
| Market / areas | Dallas–Fort Worth; Plano, Frisco, Celina, Little Elm plus expanded DFW areas in the prompt |
| Purchase price | $350,000 maximum |
| Total initial cash | $150,000 maximum |
| Horizon / objective | 10 years / balanced income and appreciation |
| Property types | Single-family, townhomes, duplexes, other legally rentable residential |
| School gate | Assigned elementary and middle schools each GreatSchools 8/10+ (confirm local suitability) |
| Minimum weighted score | 7.0/10 |
| DSCR | 1.00+ baseline pass; 0.95–0.99 conditional with down-payment optimization; below 0.95 normally excluded |
| Cap rate | 6.0%+ preferred; 5.5–5.99% conditional; below 5.5% normally excluded absent a strong exception |
| Owner contribution | Maximum $300/month |
| Financing | Ask for down payment, rate, and term, or obtain permission to use current market-based estimates |

For other markets, do not carry over DFW geography, school ratings, tax assumptions, or rental rules without confirmation. Identify locally relevant sources and criteria.

## Configuration lifecycle

The confirmed investor profile supplies user-specific settings; the companion prompt supplies the research method. Never treat the example profile as confirmed for a new user.

- The confirmed investor profile is separate from the research prompt. The prompt defines the methodology; the profile defines whose market and constraints to apply.
- Load the confirmed profile before each run.
- If a required profile field is absent, ask only for that field.
- If the user explicitly asks to change their profile, show the current value and proposed value, confirm if the change materially affects screening, then update it.
- Never overwrite a confirmed profile with illustrative defaults.
- Never infer that a sample profile belongs to a new user.

## Workflow

### 1. Discover Candidates
- Search current listing sources across the configured geography and property types.
- Search systematically by city, ZIP, and relevant neighborhood/submarket.
- Confirm listing status, asking price, and direct listing URL.
- Avoid broad inventory dumps. Build a candidate set for screening.

### 2. Verify Hard Gates
Check price, total cash requirement, exact-address assigned elementary and middle schools and current ratings, rental legality/restrictions, property condition, taxes, insurance, flood/environmental concerns, other major risks, and weighted score.

A missing or unverifiable school assignment/rating is a failure pending verification, not a pass. Never substitute district averages or nearby schools.

### 3. Underwrite Rental Economics
Use consistent financing assumptions and separate:
- Gross scheduled rent.
- Vacancy/credit loss.
- Effective rental income.
- Operating expenses.
- Standard NOI.
- Mortgage principal and interest.
- Capital-expenditure reserves.
- Reserve-adjusted cash flow.

Calculate standard cap rate, DSCR, cash-on-cash return, break-even rent, and reserve-adjusted income yield. State formulas and assumptions in detailed reports.

### 4. Apply Financing Optimization
For borderline DSCR or owner-contribution cases:
- Model baseline financing first.
- Calculate the additional down payment required to reach target coverage and cash-flow levels.
- Recalculate total cash, cash-on-cash return, and remaining liquidity.
- Keep conditional candidates separate from baseline passes.
- Do not use extra equity to disguise poor property economics.

### 5. Stress Test
At minimum, test rent 5% below the base estimate and higher property-tax and insurance assumptions. Report effects on DSCR and owner contribution. State assumptions and uncertainty.

### 6. Score and Rank
Apply the configured weighted scorecard. Rank by evidence-supported risk-adjusted attractiveness. A high score cannot override a failed hard gate.

### 7. Produce the Daily Aggregated Report
Use four sections:
1. Executive summary.
2. Shortlist comparison table (normally 3–5 candidates maximum).
3. Shortlist highlights (3–5 bullets per candidate).
4. Watchlist/near-misses and next research.

Keep the daily report concise. Include links and enough source/assumption context for traceability. Clearly label PASS versus CONDITIONAL. Never imply a conditional candidate has passed.

Make the report easy to interpret: explain that PASS means all hard gates and configured baseline thresholds are met; CONDITIONAL means a specified financing or threshold improvement is required and is not a pass yet; WATCHLIST/NEAR-MISS means useful research lead that failed at least one gate. For each key metric, briefly indicate whether higher/lower is favorable and compare it with the user's configured threshold. Do not imply that a score alone makes a property investable.

If none qualifies, state "No qualifying opportunities today" and briefly explain the most relevant screening outcomes.

### 8. Produce Detailed Reports on Request
When the user asks for a deep dive or full underwriting, expand the selected candidate into a comprehensive report with listing/property facts, school evidence, comparable sales and rents, expenses, NOI, cap rate, DSCR, cash flow, cash-on-cash return, baseline and optimized down-payment cases, stress tests, sensitivity analysis, 5-/10-year projections, amortization, accelerated payoff, risks, missing data, due-diligence questions, and offer-price analysis.

Do not overload the daily report with this detail.

## Metric Definitions and Guardrails

- **NOI:** Effective rental income less ordinary operating expenses; exclude mortgage debt service.
- **Cap rate:** Annual NOI divided by purchase price; independent of financing.
- **DSCR:** NOI divided by annual debt service; state the debt-service convention.
- **Cash-on-cash return:** Annual cash flow divided by total initial cash invested; state whether cash flow is reserve-adjusted.
- **Reserve-adjusted cash flow:** Cash flow after debt service and realistic capital-expenditure reserves.
- **Mortgage payoff timeline:** Calculate from amortization and modeled extra principal payments. Cap rate alone does not determine payoff time.

Never mix standard NOI with reserve-adjusted cash flow without explaining the distinction. Do not treat asking rents as verified executed leases. Do not fabricate exact figures when evidence is incomplete.

## Evidence and Citation Standards

Prefer official primary sources for school assignments, taxes, zoning, and property facts. Cross-check critical facts using credible sources. Cite material claims with source links and dates checked. Label values as verified, estimated, projected, or unverified. Explain material source discrepancies. Disclose data-access limitations; do not claim MLS access or verified lease data without it.

## Output Quality

Help the investor decide what deserves further research; do not tell them to buy. Use concise, comparable tables and plain-language takeaways. Keep failed candidates out of the qualifying shortlist. Preserve detailed evidence and calculations so they can be expanded when requested.

## Portability

Before sharing with another investor or market:
1. Complete first-run setup; do not assume the DFW profile belongs to the new user.
2. Replace the DFW example geography and benchmark properties.
3. Replace purchase-price, cash, DSCR, cap-rate, owner-contribution, and score thresholds with that investor's settings.
4. Adapt school-rating systems and attendance-zone sources to the local market.
5. Adapt tax, assessment, insurance, rental-law, HOA, zoning, and financing checks to the jurisdiction.
6. Re-verify all source providers and data availability.
