using Dororong.App.Product;

namespace Dororong.App.Tests.Product;

public class BehaviorPreferencesTests
{
    [Fact]
    public void Default_is_enabled_without_writing_and_disabled_choice_survives_new_instance()
    {
        object? value = null;
        var writes = 0;
        BehaviorPreferences Create() => new(() => value, v => { value = v; writes++; });
        var preferences = Create();
        Assert.True(preferences.AutomaticSleepEnabled);
        Assert.Equal(0, writes);
        preferences.SetAutomaticSleepEnabled(false);
        Assert.Equal(0, value);
        Assert.False(Create().AutomaticSleepEnabled);
        preferences.SetAutomaticSleepEnabled(true);
        Assert.Equal(1, value);
        Assert.True(Create().AutomaticSleepEnabled);
    }

    [Theory]
    [InlineData("0")] [InlineData(2)] [InlineData(-1)]
    public void Invalid_value_is_reported_without_overwriting(object value)
    {
        var writes = 0;
        var preferences = new BehaviorPreferences(() => value, _ => writes++);
        Assert.Throws<System.IO.IOException>(() => preferences.AutomaticSleepEnabled);
        Assert.Throws<System.IO.IOException>(() => preferences.SetAutomaticSleepEnabled(false));
        Assert.Equal(0, writes);
    }

    [Fact]
    public void Failed_write_is_reported_without_changing_observed_setting()
    {
        var preferences = new BehaviorPreferences(() => null, _ => { });
        Assert.Throws<System.IO.IOException>(() => preferences.SetAutomaticSleepEnabled(false));
        Assert.True(preferences.AutomaticSleepEnabled);
        preferences = new BehaviorPreferences(() => 1, _ => throw new UnauthorizedAccessException());
        Assert.Throws<UnauthorizedAccessException>(() => preferences.SetAutomaticSleepEnabled(false));
        Assert.True(preferences.AutomaticSleepEnabled);
    }
}
