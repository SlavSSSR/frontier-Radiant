namespace Content.Shared._Starlight.Medical.Surgery;

/// <summary>One progression step every ten seconds of active time.</summary>
public static class SurgicalInfectionRules
{
    public const float Interval = 10f;
    public const float MaximumPatientDamage = 4f;

    public static int AfterLavage(int severity) => Math.Max(0, severity - 30);
    public static float SlimeSpeed(int stage) => stage switch { 0 => 1f, 1 => 0.9f, 2 => 0.8f, _ => 0.6f };

    public static int Advance(int infection, int contamination)
    {
        var credit = 0;
        return Advance(infection, contamination, ref credit, false);
    }

    public static int Advance(int infection, int contamination, ref int recoveryCredit, bool antibiotic)
    {
        var exposureCredit = 0;
        return Advance(infection, contamination, ref recoveryCredit, ref exposureCredit, antibiotic);
    }

    public static int Advance(int infection, int contamination, ref int recoveryCredit, ref int exposureCredit, bool antibiotic)
    {
        if (!antibiotic && contamination > 0 && contamination < SurgicalSterilityRules.ContaminatedSite)
        {
            recoveryCredit = 0;
            exposureCredit++;
            var incubating = Math.Min(100, infection + exposureCredit / 3);
            exposureCredit %= 3;
            return incubating;
        }
        exposureCredit = 0;
        if (!antibiotic && contamination >= SurgicalSterilityRules.ContaminatedSite)
        {
            recoveryCredit = 0;
            return Math.Clamp(infection + (contamination >= 85 ? 3 : contamination >= 60 ? 2 : 1), 0, 100);
        }
        // Integer remainder preserves exact maximum recovery times: 36 or 12 ten-second ticks.
        recoveryCredit += antibiotic ? 300 : 100;
        var result = Math.Max(0, infection - recoveryCredit / 36);
        recoveryCredit = result == 0 ? 0 : recoveryCredit % 36;
        return result;
    }

    /// <summary>Closed tissue clears one contamination point per minute. Medication does not affect this.</summary>
    public static int ClearContamination(int contamination, bool open, ref int credit)
    {
        if (open || contamination <= 0)
        {
            credit = 0;
            return contamination;
        }
        credit++;
        var remaining = Math.Max(0, contamination - credit / 6);
        credit = remaining == 0 ? 0 : credit % 6;
        return remaining;
    }

    public static int Stage(int infection)
        => infection >= 85 ? 3 : infection >= 60 ? 2 : infection >= 30 ? 1 : 0;

    public static float Damage(int infection)
        => Stage(infection) switch { 3 => 2f, 2 => 1f, 1 => 0.5f, _ => 0f };

    public static float CombineDamage(float total, float amount)
        => Math.Min(MaximumPatientDamage, total + amount);
}
