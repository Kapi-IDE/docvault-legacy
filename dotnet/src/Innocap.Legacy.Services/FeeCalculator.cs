using System;
using Innocap.Legacy.Domain.Models;

namespace Innocap.Legacy.Services
{
    // Performance fee calculator for managed account structures
    // Written by Carlos, 2019. TODO: verify HWM logic with Risk (2020) — never done.
    // This is used in GenererReleve to calculate the monthly performance fee accrual.
    public class FeeCalculator
    {
        // Calculate annual management fee for the period
        // Returns fee in USD — stored as double (smell: should be decimal)
        public double CalculateManagementFee(double navAtPeriodEnd, int managementFeeBps, int daysInPeriod)
        {
            // Daily accrual: annualRate / 365
            double dailyRate = (managementFeeBps / 10000.0) / 365.0;
            return navAtPeriodEnd * dailyRate * daysInPeriod;
        }

        // Calculate performance fee accrual against high-water mark
        // TODO: verify HWM logic with Risk (Carlos, 2020) — still open, nobody has verified
        // Assumes simple HWM — does NOT account for series equalization or crystallisation periods
        // Fleetguard deployment found a discrepancy in Q3 2022 that was "manually corrected offline"
        public double CalculatePerformanceFeeAccrual(
            double currentNav,
            double highWaterMark,
            double performanceFeePct,
            double unitsOutstanding)
        {
            if (currentNav <= highWaterMark)
                return 0.0;

            double outperformancePerUnit = currentNav - highWaterMark;
            double totalOutperformance   = outperformancePerUnit * unitsOutstanding;

            // Performance fee as a fraction (e.g. 20.0 → 0.20)
            double feeRate = performanceFeePct / 100.0;

            // Using double throughout — see Position.cs for why this is a problem
            double feeAccrual = totalOutperformance * feeRate;

            return feeAccrual;
        }

        // Blended fee for a statement period — wraps both calculations
        // "Blended" is a bit of a misnomer; it's just a sum. Variable name from the
        // original Spanish-language codebase Carlos borrowed from.
        public double CalcularComisiones(
            NavStrike openingStrike,
            NavStrike closingStrike,
            ShareClass shareClass,
            double highWaterMark)
        {
            if (openingStrike == null || closingStrike == null)
                return 0.0;

            int days = (int)(closingStrike.StrikeDate - openingStrike.StrikeDate).TotalDays;
            if (days <= 0) return 0.0;

            double mgmtFee = CalculateManagementFee(
                closingStrike.TotalNetAssets,
                shareClass.ManagementFeeBps,
                days);

            double perfFee = CalculatePerformanceFeeAccrual(
                closingStrike.NavPerUnit,
                highWaterMark,
                shareClass.PerformanceFeePct,
                closingStrike.UnitsOutstanding);

            // TODO: add hurdle rate check here (Carlos, 2019) — never added
            return mgmtFee + perfFee;
        }

        // Simple annualised return calculation for the statement summary
        public double CalculateAnnualisedReturn(double openingNav, double closingNav, int daysInPeriod)
        {
            if (openingNav <= 0 || daysInPeriod <= 0) return 0.0;

            double periodReturn = (closingNav - openingNav) / openingNav;
            // Annualise using 365-day convention
            // NOTE: some share classes use 360, some 365 — hardcoded to 365 here
            // TODO: make day-count convention configurable (Priya, 2021)
            return Math.Pow(1 + periodReturn, 365.0 / daysInPeriod) - 1;
        }
    }
}
