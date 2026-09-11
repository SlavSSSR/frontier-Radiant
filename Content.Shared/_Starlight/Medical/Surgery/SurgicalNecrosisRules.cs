namespace Content.Shared._Starlight.Medical.Surgery;

/// <summary>Active seconds, independent of antibiotic recovery and wall-clock time.</summary>
public static class SurgicalNecrosisRules
{
    public const float SevereDuration = 300;
    public const float LimbDeath = 1200;
    public const float OrganDeath = 1200;
    public const float HeartDeath = 3600;

    public static float AdvanceSevere(float previous, int infection, float seconds)
        => SurgicalInfectionRules.Stage(infection) == 3 ? previous + seconds : 0;

    public static float HeartFunction(float age)
        => Math.Clamp(1f - age / HeartDeath, 0f, 1f);
}
