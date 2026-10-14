using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dororong.App.Product;
using Dororong.App.Tests.Controls;

namespace Dororong.App.Tests.Product;

public class PetContextMenuTests
{
    [Fact]
    public void Embedded_font_is_not_a_machine_installed_font_fallback()
    {
        EdgePerchPresentationTests.Sta(() =>
        {
            var menu = new PetContextMenu();
            if (SystemParameters.HighContrast) return;
            var family = (FontFamily)menu.Resources["PetMenuFont"];
            foreach (var weight in new[] { FontWeights.Normal, FontWeights.SemiBold, FontWeights.Bold })
            {
                var typeface = new Typeface(family, FontStyles.Normal, weight, FontStretches.Normal);
                Assert.True(typeface.TryGetGlyphTypeface(out var glyph));
                Assert.Contains("dororong.app", glyph.FontUri.ToString().ToLowerInvariant());
                Assert.Contains("pretendard", glyph.FontUri.ToString().ToLowerInvariant());
                Assert.True(glyph.CharacterToGlyphMap.ContainsKey('롱'));
            }
        });
    }

    [Fact]
    public void Approved_dimensions_roles_and_close_button_are_preserved()
    {
        EdgePerchPresentationTests.Sta(() =>
        {
            var menu = new PetContextMenu();
            var snack = new MenuItem { Header = "간식 주기", Tag = "snack" };
            var startup = new MenuItem { Header = "Windows 로그인 시 실행", IsCheckable = true };
            menu.Items.Add(snack); menu.Items.Add(startup);
            menu.Measure(new Size(500, 700)); menu.Arrange(new Rect(menu.DesiredSize)); menu.UpdateLayout();
            if (SystemParameters.HighContrast) return;
            Assert.Equal(312, menu.ActualWidth);
            Assert.Equal(69, snack.ActualHeight);
            Assert.Equal(54, startup.ActualHeight);
            Assert.IsType<Button>(menu.Template.FindName("PART_Close", menu));
        });
    }
}
