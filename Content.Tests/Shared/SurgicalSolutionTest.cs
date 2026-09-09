using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared._Starlight.Medical.Surgery;
using Content.Shared._Starlight.Medical.Surgery.Components;
using NUnit.Framework;
using Robust.Shared.GameObjects;

namespace Content.Tests.Shared;

[TestFixture]
public sealed class SurgicalSolutionTest
{
    [TestCase("Ethanol", 5, 5, true)]
    [TestCase("Ethanol", 4, 5, false)]
    [TestCase("Saline", 10, 10, true)]
    [TestCase("Saline", 9, 10, false)]
    [TestCase("Saline", 0, 10, false)]
    public void PureSolutionMustContainEnough(string reagent, int volume, int required, bool expected)
        => Assert.That(SurgicalSolutionRules.IsPure(new Solution(reagent, volume), reagent, required), Is.EqualTo(expected));

    [TestCase("Ethanol", "Saline")]
    [TestCase("Ethanol", "Water")]
    [TestCase("Saline", "Ethanol")]
    [TestCase("Saline", "Surgicillin")]
    public void EvenSmallAdmixturePreventsTreatment(string reagent, string impurity)
    {
        var liquid = new Solution(reagent, 100);
        liquid.AddReagent(impurity, FixedPoint2.New(0.01));
        Assert.That(SurgicalSolutionRules.IsPure(liquid, reagent, 5), Is.False);
    }

    [Test]
    public void MixtureAddedDuringActionInvalidatesRecheck()
    {
        var liquid = new Solution("Ethanol", 10);
        Assert.That(SurgicalSolutionRules.IsPure(liquid, "Ethanol", 5), Is.True);
        liquid.AddReagent("Water", 1);
        Assert.That(SurgicalSolutionRules.IsPure(liquid, "Ethanol", 5), Is.False);
    }

    [Test]
    public void BoneGelDoesNotUseGenericContainerSelection()
        => Assert.That(SharedSurgerySystem.SurgicalSolutionReagent(new BoneGelComponent()), Is.Null);

    [Test]
    public void OrdinaryContainerLeavesSmallContactPenaltyUntilSterilized()
    {
        var item = new SurgicalItemSterilityComponent
        {
            ContainerContamination = SurgicalSolutionRules.OrdinaryContainerContamination,
        };
        Assert.That(SurgicalSterilityRules.Contact(item, new EntityUid(1), 0, true), Is.EqualTo(5));
        Assert.That(item.Dirty, Is.False);
        SurgicalSterilityRules.Disinfect(item);
        Assert.That(item.ContainerContamination, Is.Zero);
        Assert.That(SurgicalSterilityRules.Contact(item, new EntityUid(1), 0, true), Is.Zero);
    }
}
