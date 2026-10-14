using System.IO;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using Dororong.App.Tests.Controls;

namespace Dororong.App.Tests.Product;

public class ProductIconTests
{
    [Fact]
    public void Head_source_is_the_existing_separated_art_and_ico_contains_native_sizes()
    {
        var root = EdgePerchPresentationTests.ProjectRoot();
        var source = File.ReadAllBytes(Path.Combine(root, "src/Dororong.App/Assets/dororong-head-source.png"));
        Assert.Equal("73E2C45B0BE6C61141B9442905BFC9DF99A0509643457B896B3EF8D619150854", Convert.ToHexString(SHA256.HashData(source)));
        using var stream = typeof(App).Assembly.GetManifestResourceStream("Dororong.App.Assets.dororong.ico")!;
        using var reader = new BinaryReader(stream);
        Assert.Equal(0, reader.ReadUInt16()); Assert.Equal(1, reader.ReadUInt16());
        int[] sizes = [16, 20, 24, 32, 40, 48, 64, 96, 128, 256];
        Assert.Equal(sizes.Length, reader.ReadUInt16());
        foreach (var size in sizes)
        {
            Assert.Equal(size == 256 ? 0 : size, reader.ReadByte());
            Assert.Equal(size == 256 ? 0 : size, reader.ReadByte());
            reader.ReadBytes(2); Assert.Equal(1, reader.ReadUInt16()); Assert.Equal(32, reader.ReadUInt16());
            var length = reader.ReadUInt32(); var offset = reader.ReadUInt32();
            Assert.InRange(offset + length, 1, stream.Length);
            var directoryPosition = stream.Position;
            stream.Position = offset;
            using var png = new MemoryStream(reader.ReadBytes((int)length));
            using var bitmap = new System.Drawing.Bitmap(png);
            Assert.Equal(size, bitmap.Width); Assert.Equal(size, bitmap.Height);
            Assert.Equal(0, bitmap.GetPixel(0, 0).A);
            var proof = Path.Combine(root, "artifacts/ui-menu-head-icon-20261014");
            Directory.CreateDirectory(proof);
            bitmap.Save(Path.Combine(proof, $"icon-{size}.png"), System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = directoryPosition;
        }
        stream.Position = 0;
        using var icon = new System.Drawing.Icon(stream);
        // GDI+'s Icon size selector treats the 256 entry's zero dimension as zero;
        // validate that frame's actual PNG above and WPF decoder below instead.
        foreach (var size in sizes.Where(size => size <= 128))
        {
            using var frame = new System.Drawing.Icon(icon, size, size);
            using var bitmap = frame.ToBitmap();
            Assert.Equal(size, bitmap.Width); Assert.Equal(size, bitmap.Height);
            Assert.Equal(0, bitmap.GetPixel(0, 0).A);
            var ink = 0;
            for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
                if (bitmap.GetPixel(x, y).A > 0) ink++;
            Assert.InRange(ink, size * size / 4, size * size * 3 / 4);
        }
    }

    [Fact]
    public void Window_and_menu_use_packaged_head_assets_without_changing_product_name()
    {
        EdgePerchPresentationTests.Sta(() =>
        {
            var window = new MainWindow();
            try
            {
                Assert.Equal("도로롱이 내 컴에?!", window.Title);
                var icon = Assert.IsAssignableFrom<BitmapFrame>(window.Icon);
                Assert.Equal(10, icon.Decoder.Frames.Count);
                Assert.Equal(256, icon.Decoder.Frames.Last().PixelWidth);
                var head = new BitmapImage(new Uri("pack://application:,,,/Dororong.App;component/Assets/dororong-head.png"));
                Assert.Equal(96, head.PixelWidth); Assert.Equal(96, head.PixelHeight);
            }
            finally { window.Close(); }
        });
    }
}
