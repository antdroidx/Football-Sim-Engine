using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using System.Text.Json;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class PreSnapTests
{
    [Fact]
    public void PreSnapChoicesValidateAndSupportShiftOrMotionOnly()
    {
        var choice = PreSnapCatalog.Motion("Left", 1.5);

        Assert.Equal(PreSnapType.Motion, choice.Type);
        Assert.Equal("Left", choice.Side);
        Assert.Equal(1.5, choice.Yards);
        Assert.True(PreSnapCatalog.IsValid(choice));
        Assert.False(PreSnapCatalog.IsValid(choice with { Type = (PreSnapType)3 }));
        Assert.False(PreSnapCatalog.IsValid(choice with { Yards = 4.5 }));
    }
}
