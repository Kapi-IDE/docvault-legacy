# nav_house_rules — included via {{>nav_house_rules}}

- NAV is struck once per fund per business day, after market close
  and FX cutoff, by the fund accounting team. Until "struck and
  released," intra-day NAV figures are MNPI.
- Trial balance must reconcile to the sub-ledger to within 0.01 base
  currency before NAV is considered final.
- FX rates: WM/Reuters 4pm London close for the as-of date. Anything
  else is an explicit override and must be flagged.
- Pricing: marked at the official source per asset class (Bloomberg
  BVAL for fixed income, exchange close for listed equities,
  third-party PE valuation for level-3 assets).
- Anomalies route to ops review; agents propose, humans dispose. No
  agent posts to the books.
