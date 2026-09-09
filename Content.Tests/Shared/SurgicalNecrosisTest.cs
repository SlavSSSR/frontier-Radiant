using Content.Shared._Starlight.Medical.Surgery;
using NUnit.Framework;

namespace Content.Tests.Shared;

[TestFixture]
public sealed class SurgicalNecrosisTest
{
    [Test]
    public void SevereInfectionRequiresFiveContinuousMinutes()
    {
        var elapsed = 0f;
        for (var tick = 0; tick < 29; tick++)
            elapsed = SurgicalNecrosisRules.AdvanceSevere(elapsed, 85, 10);
        Assert.That(elapsed, Is.LessThan(SurgicalNecrosisRules.SevereDuration));
        Assert.That(SurgicalNecrosisRules.AdvanceSevere(elapsed, 85, 10), Is.EqualTo(SurgicalNecrosisRules.SevereDuration));
        Assert.That(SurgicalNecrosisRules.AdvanceSevere(elapsed, 84, 10), Is.Zero);
    }

    [TestCase(0, 1)]
    [TestCase(1800, 0.5f)]
    [TestCase(3590, 10f / 3600)]
    [TestCase(3600, 0)]
    [TestCase(5000, 0)]
    public void HeartGraduallyFailsOverOneHour(float seconds, float expected)
        => Assert.That(SurgicalNecrosisRules.HeartFunction(seconds), Is.EqualTo(expected).Within(0.0001));

    [Test]
    public void LimbAndHeartHaveIndependentDeadlines()
    {
        Assert.That(SurgicalNecrosisRules.LimbDeath, Is.EqualTo(20 * 60));
        Assert.That(SurgicalNecrosisRules.HeartDeath, Is.EqualTo(60 * 60));
    }
}
