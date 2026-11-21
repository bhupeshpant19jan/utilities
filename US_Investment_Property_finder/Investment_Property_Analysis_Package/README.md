# Investment Property Analysis

A reusable research skill for screening residential rental investments, comparing the numbers, and deciding which properties deserve deeper due diligence.

## Package files

| File | Purpose |
|---|---|
| `SKILL.md` | Workflow and first-time setup wizard |
| `Investment_Property_Analysis_Prompt.md` | Research rules, financial formulas, scoring and report format |
| `INVESTOR_PROFILE_TEMPLATE.md` | Suggested defaults and profile fields |
| `README.md` / `README.html` | This quick-start and output guide |

Keep these files together. The skill reads the prompt as its source of truth; the investor profile stores the user's confirmed market and preferences.

## First run: guided setup

Before searching, the skill displays suggested defaults and asks the user to accept or change them. DFW settings are examples, not universal recommendations.

**Minimum inputs to confirm**

1. **Market:** metro/region and target cities or ZIP codes.
2. **Budget:** maximum purchase price and maximum total cash available per property, including down payment, closing costs, repairs and reserves.
3. **Strategy:** investment horizon and objective — cash flow, appreciation, or balanced.
4. **Financing:** down payment, interest rate and loan term, or permission to use current market-based estimates clearly labeled as assumptions.
5. **Must-haves:** property types and any school-quality, rental-use or other non-negotiable gate.

The user can say **“Accept the suggested defaults”** and provide only missing financing details. The wizard asks only for essential gaps, reads back the confirmed profile, and saves it as `investor_profile.json` where persistent storage is supported. Later runs reuse that profile instead of repeating setup.

**Example DFW defaults:** $350,000 purchase-price ceiling; $150,000 initial-cash ceiling; 10-year horizon; balanced strategy; 7.0/10 minimum score; assigned elementary and middle schools each 8/10+; DSCR 1.00+ baseline; cap rate 6.0%+ preferred; and maximum $300/month owner contribution. DSCR 0.95–0.99 and cap rate 5.5–5.99% are conditional ranges, not automatic passes. Confirm local school sources and financing assumptions before using these settings elsewhere.

## How to read the report

| Label / metric | How to interpret it |
|---|---|
| **PASS** | Meets configured hard gates and baseline financial rules. Normal due diligence is still required. |
| **CONDITIONAL** | Could qualify only after a stated change, such as additional down payment. Not a pass yet. |
| **WATCHLIST / NEAR-MISS** | Research lead that failed a gate or needs verification; not a recommendation. |
| **Score (0–10)** | Weighted comparison of rental economics, cash return, risk, demand, appreciation and other factors. A high score cannot override a failed hard gate. |
| **NOI** | Rental income after operating expenses, before mortgage payments. |
| **Cap rate** | NOI ÷ purchase price. Property-level operating yield; financing does not change it. Higher is generally better, all else equal. |
| **DSCR** | NOI ÷ annual mortgage principal-and-interest payments. 1.00 means NOI exactly covers that debt service; above 1.00 provides more coverage. |
| **Monthly cash flow** | Remaining cash after operating costs, mortgage payments and stated reserves. Positive is preferable; negative means owner contribution. |
| **Cash-on-cash return** | Annual reserve-adjusted cash flow ÷ total cash invested. Compare only with consistent assumptions. |
| **Total initial cash** | Estimated cash needed to acquire and prepare the property, including down payment, closing costs, repairs and reserves. |

**Important:** Cap rate, DSCR and cash flow answer different questions. A property can have a reasonable cap rate but weak DSCR because of its loan terms. A conditional DSCR case should show the extra down payment and its effect on cash invested and return. School ratings must be for the exact assigned schools, not nearby schools or district averages.

## Daily report vs. deep dive

The daily report is intentionally brief: executive summary, top 3–5 candidates, key metrics and risks, then a small watchlist. Ask for a **detailed property analysis** to see rent and sale comparables, expense assumptions, financing alternatives, stress tests, long-term projections, payoff scenarios, due-diligence questions and offer-price analysis.

## Use in another market

Install the complete folder, run the setup wizard, and replace the DFW example with the new investor's market and limits. Re-check local school assignment sources, taxes, insurance, zoning, rental rules, HOA restrictions and available listing/rent data. This is research support, not a buy/sell decision or professional legal, tax, lending or inspection advice.
