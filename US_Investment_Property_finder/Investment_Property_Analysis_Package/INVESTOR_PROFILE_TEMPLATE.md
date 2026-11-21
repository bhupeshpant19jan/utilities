# Investor Profile — First-Run Setup

This is a setup template, not a universal recommendation. The DFW values are suggested examples from the companion research prompt. On first use, the skill should display these values and ask the user to accept or change them.

## Minimum details to confirm

| Setting | Suggested default | User-confirmed value |
|---|---|---|
| Target market / geography | Dallas–Fort Worth, Texas (example only) | |
| Preferred cities / ZIPs | Plano, Frisco, Celina, Little Elm; see prompt for expanded search areas | |
| Maximum purchase price | $350,000 | |
| Maximum total initial cash | $150,000 | |
| Investment horizon | 10 years | |
| Objective | Balanced: rental income + appreciation | |
| Property types | Single-family, townhomes, duplexes, other legally rentable residential | |
| Financing down payment | Ask user; do not invent | |
| Interest rate | Ask user or use a current, clearly labeled market-based assumption with permission | |
| Loan term | Ask user; if unknown, offer a clearly labeled assumption | |
| School-quality gate | Assigned elementary and middle schools each GreatSchools 8/10+, if suitable for market | |
| DSCR | 1.00+ pass; 0.95–0.99 conditional; below 0.95 normally exclude | |
| Cap rate | 6.0%+ preferred; 5.5–5.99% conditional; below 5.5% normally exclude | |
| Maximum monthly owner contribution | $300 | |

## First-run question to show the user

“Here are the suggested starting settings. The DFW market and thresholds are examples, not universal defaults.

- Market: Dallas–Fort Worth, Texas
- Areas: Plano, Frisco, Celina, Little Elm, plus the expanded DFW areas in the prompt
- Maximum purchase price: $350,000
- Maximum total initial cash: $150,000
- Horizon: 10 years
- Objective: balanced rental income and appreciation
- Property types: single-family, townhomes, duplexes, and other legally rentable residential
- School gate: assigned elementary and middle schools each GreatSchools 8/10+ (confirm local suitability)
- DSCR: 1.00+ pass; 0.95–0.99 conditional with an optimized down-payment scenario
- Cap rate: 6.0%+ preferred; 5.5–5.99% conditional
- Maximum owner contribution: $300/month
- Financing: down payment, interest rate, and loan term still need confirmation or permission to use current market-based assumptions

Would you like to accept the DFW example settings and provide the financing assumptions, or change the market and/or limits?”

The user may respond “Accept all defaults” and provide only the missing financing inputs, or list only the settings they want changed. Do not require them to restate unchanged defaults.

## Persistence

After the user confirms, save the confirmed values as `investor_profile.json` where the platform supports persistent files. Suggested shape:

```json
{
  "profile_confirmed": true,
  "market": "Dallas–Fort Worth, Texas",
  "areas": ["Plano", "Frisco", "Celina", "Little Elm"],
  "max_purchase_price": 350000,
  "max_total_initial_cash": 150000,
  "investment_horizon_years": 10,
  "objective": "balanced",
  "property_types": ["single-family", "townhome", "duplex", "other legally rentable residential"],
  "down_payment_percent": null,
  "interest_rate_percent": null,
  "loan_term_years": null,
  "school_gate": "Assigned elementary and middle schools each GreatSchools 8/10+, subject to local suitability",
  "dscr_pass_minimum": 1.00,
  "dscr_conditional_minimum": 0.95,
  "cap_rate_preferred_minimum": 6.0,
  "cap_rate_conditional_minimum": 5.5,
  "max_monthly_owner_contribution": 300
}
```

Do not save the profile as confirmed while required market/budget details are unresolved. Do not save credentials or sensitive account information.
