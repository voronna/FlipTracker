using Web_Development_Coursework.Models;
using Web_Development_Coursework.Services;
using Xunit;

namespace Web_Development_Coursework.Tests;

public class DealScoreCalculatorTests
{
    private static readonly DateTime Today = new(2026, 11, 20);

    private static MiniatureModel Model(string name = "Intercessors") =>
        new() { Name = name, RrpPrice = 100m };

    private static Listing Item(decimal price, Material material = Material.Plastic) =>
        new()
        {
            Title = "test",
            Price = price,
            Shipping = 0m,
            Material = material,
            IsComplete = true,
            PostedAt = Today
        };

    private static Rule DefaultRule() => new(); // 20% discount, no metal, blocks Termagant, 30 days

    private static readonly DealScoreCalculator Calculator = new();

    private static DealScoreResult Score(Listing l, MiniatureModel m, Rule? r = null) =>
        Calculator.Calculate(l, m, r ?? DefaultRule(), null, 0, Today);

    [Fact]
    public void BigDiscount_IsBuy()
    {
        var result = Score(Item(60m), Model()); // 40% off -> 80
        Assert.Equal(Verdict.Buy, result.Verdict);
        Assert.Equal(80, result.Score);
    }

    [Fact]
    public void DiscountBelowMinimum_IsSkip()
    {
        var result = Score(Item(85m), Model()); // 15% < 20%
        Assert.Equal(Verdict.Skip, result.Verdict);
    }

    [Fact]
    public void Painted_ReducesScoreBy10()
    {
        var listing = Item(70m); // 30% off -> 60
        listing.IsPainted = true;
        var result = Score(listing, Model());
        Assert.Equal(50, result.Score);
        Assert.Equal(Verdict.Buy, result.Verdict);
    }

    [Fact]
    public void PaintedAndIncomplete_BecomesThink()
    {
        var listing = Item(70m); // 60 - 10 - 15 = 35
        listing.IsPainted = true;
        listing.IsComplete = false;
        var result = Score(listing, Model());
        Assert.Equal(35, result.Score);
        Assert.Equal(Verdict.Think, result.Verdict);
    }

    [Fact]
    public void StaleListing_ReducesScoreBy20()
    {
        var listing = Item(70m); // 60 - 20 = 40
        listing.PostedAt = Today.AddDays(-40);
        var result = Score(listing, Model());
        Assert.Equal(40, result.Score);
        Assert.Equal(Verdict.Think, result.Verdict);
    }

    [Fact]
    public void Metal_IsSkip_WhenNotAllowed()
    {
        var result = Score(Item(50m, Material.Metal), Model());
        Assert.Equal(Verdict.Skip, result.Verdict);
    }

    [Fact]
    public void Metal_IsAllowed_WhenRuleAllows()
    {
        var rule = new Rule { AllowMetal = true };
        var result = Score(Item(50m, Material.Metal), Model(), rule);
        Assert.NotEqual(Verdict.Skip, result.Verdict);
    }

    [Fact]
    public void BlockedModel_IsSkip()
    {
        var result = Score(Item(30m), Model("Termagant"));
        Assert.Equal(Verdict.Skip, result.Verdict);
    }

    [Fact]
    public void LooseBits_AboveTenPercent_IsSkip()
    {
        var listing = Item(50m);
        listing.IsBitsOnly = true;
        var result = Score(listing, Model());
        Assert.Equal(Verdict.Skip, result.Verdict);
    }
}