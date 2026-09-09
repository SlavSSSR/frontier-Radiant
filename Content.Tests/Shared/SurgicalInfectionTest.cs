using Content.Shared._Starlight.Medical.Surgery;
using NUnit.Framework;

namespace Content.Tests.Shared;

[TestFixture]
public sealed class SurgicalInfectionTest
{
    [Test]
    public void SmallClosedResidueIncubatesAndEventuallyClears()
    {
        var contamination = 4;
        var infection = 0;
        var recovery = 0;
        var exposure = 0;
        var cleanup = 0;
        for (var tick = 0; tick < 24; tick++)
        {
            infection = SurgicalInfectionRules.Advance(infection, contamination, ref recovery, ref exposure, false);
            contamination = SurgicalInfectionRules.ClearContamination(contamination, false, ref cleanup);
        }
        Assert.That(contamination, Is.Zero);
        Assert.That(infection, Is.EqualTo(8));
        for (var tick = 0; tick < 36; tick++)
            infection = SurgicalInfectionRules.Advance(infection, contamination, ref recovery, ref exposure, false);
        Assert.That(infection, Is.Zero);
    }

    [Test]
    public void OpenSiteDoesNotPassivelyClean()
    {
        var credit = 0;
        for (var tick = 0; tick < 60; tick++)
            Assert.That(SurgicalInfectionRules.ClearContamination(4, true, ref credit), Is.EqualTo(4));
        Assert.That(credit, Is.Zero);
    }

    [Test]
    public void AntibioticTreatsIncubationWithoutClearingContamination()
    {
        var recovery = 0;
        var exposure = 2;
        Assert.That(SurgicalInfectionRules.Advance(8, 4, ref recovery, ref exposure, true), Is.Zero);
        Assert.That(exposure, Is.Zero);
    }

    [TestCase(100, 70)]
    [TestCase(30, 0)]
    [TestCase(15, 0)]
    public void LavageReducesSeverityWithoutGoingNegative(int severity, int expected)
        => Assert.That(SurgicalInfectionRules.AfterLavage(severity), Is.EqualTo(expected));

    [TestCase(0, 1f)]
    [TestCase(1, 0.9f)]
    [TestCase(2, 0.8f)]
    [TestCase(3, 0.6f)]
    public void SlimeInstabilityUsesBoundedSpeedPenalty(int stage, float expected)
        => Assert.That(SurgicalInfectionRules.SlimeSpeed(stage), Is.EqualTo(expected));

    [Test]
    public void MultipleInfectedSitesHaveOnePatientDamageLimit()
    {
        var damage = 0f;
        for (var site = 0; site < 20; site++)
            damage = SurgicalInfectionRules.CombineDamage(damage, SurgicalInfectionRules.Damage(100));
        Assert.That(damage, Is.EqualTo(4f));
    }

    [TestCase(0, 0)]
    [TestCase(24, 0)]
    [TestCase(25, 1)]
    [TestCase(59, 1)]
    [TestCase(60, 2)]
    [TestCase(84, 2)]
    [TestCase(85, 3)]
    [TestCase(100, 3)]
    public void ContaminationControlsGrowth(int contamination, int expected)
        => Assert.That(SurgicalInfectionRules.Advance(0, contamination), Is.EqualTo(expected));

    [Test]
    public void WorstContaminationStillHasIncubationPeriod()
    {
        var infection = 0;
        for (var tick = 0; tick < 9; tick++)
        {
            infection = SurgicalInfectionRules.Advance(infection, 100);
            Assert.That(SurgicalInfectionRules.Damage(infection), Is.Zero);
        }
        infection = SurgicalInfectionRules.Advance(infection, 100);
        Assert.That(SurgicalInfectionRules.Damage(infection), Is.EqualTo(0.5f));
    }

    [Test]
    public void WashingStopsGrowthButDoesNotInstantlyCureInfection()
    {
        var infection = SurgicalInfectionRules.Advance(100, 0);
        Assert.That(infection, Is.EqualTo(98));
        for (var tick = 0; tick < 49; tick++)
            infection = SurgicalInfectionRules.Advance(infection, 0);
        Assert.That(infection, Is.Zero);
        Assert.That(SurgicalInfectionRules.Advance(infection, 0), Is.Zero);
        Assert.That(SurgicalInfectionRules.Advance(100, 100), Is.EqualTo(100));
    }

    [TestCase(false, 36)]
    [TestCase(true, 12)]
    public void FullRecoveryTakesSixMinutesOrTwoWithAntibiotic(bool antibiotic, int ticks)
    {
        var infection = 100;
        var credit = 0;
        for (var tick = 1; tick < ticks; tick++)
        {
            infection = SurgicalInfectionRules.Advance(infection, antibiotic ? 100 : 0, ref credit, antibiotic);
            Assert.That(infection, Is.GreaterThan(0));
        }
        infection = SurgicalInfectionRules.Advance(infection, antibiotic ? 100 : 0, ref credit, antibiotic);
        Assert.That(infection, Is.Zero);
        Assert.That(credit, Is.Zero);
    }

    [Test]
    public void StoppingAntibioticWithDirtyWoundResumesGrowth()
    {
        var credit = 0;
        var infection = SurgicalInfectionRules.Advance(100, 100, ref credit, true);
        var next = SurgicalInfectionRules.Advance(infection, 100, ref credit, false);
        Assert.That(next, Is.GreaterThan(infection));
        Assert.That(credit, Is.Zero);
    }

    [TestCase(29, 0f)]
    [TestCase(30, 0.5f)]
    [TestCase(59, 0.5f)]
    [TestCase(60, 1f)]
    [TestCase(84, 1f)]
    [TestCase(85, 2f)]
    public void DamageFollowsStage(int infection, float expected)
        => Assert.That(SurgicalInfectionRules.Damage(infection), Is.EqualTo(expected));
}
