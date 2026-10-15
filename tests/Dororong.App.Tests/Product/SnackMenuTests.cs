using Dororong.App.Product;
using System.Windows.Forms;

namespace Dororong.App.Tests.Product;

public class SnackMenuTests
{
    [Fact] public void Both_new_tray_commands_preserve_existing_commands()
    {
        using var icon=(System.Drawing.Icon)System.Drawing.SystemIcons.Application.Clone();
        using var notify=new NotifyIcon();using var menu=new ContextMenuStrip();
        var snack=false;var startup=false;var sleep=true;var logs=0;var exit=0;
        using var tray=new TrayService((System.Drawing.Icon)icon.Clone(),notify,menu,()=>logs++,()=>exit++,false);
        tray.AddPetCommands(()=>snack=!snack,()=>snack,()=>startup=!startup,()=>startup,()=>sleep=!sleep,()=>sleep);
        Assert.Equal(6,menu.Items.Count);
        Assert.True(((ToolStripMenuItem)menu.Items[2]).Checked);
        menu.Items[2].PerformClick();Assert.False(sleep);Assert.False(((ToolStripMenuItem)menu.Items[2]).Checked);
        menu.Items[2].PerformClick();Assert.True(sleep);
        menu.Items[1].PerformClick();Assert.True(snack);
        menu.Items[3].PerformClick();Assert.True(startup);Assert.True(((ToolStripMenuItem)menu.Items[3]).Checked);
        menu.Items[1].PerformClick();Assert.False(snack);
        menu.Items[3].PerformClick();Assert.False(startup);Assert.False(((ToolStripMenuItem)menu.Items[3]).Checked);
        menu.Items[4].PerformClick();menu.Items[5].PerformClick();Assert.Equal(1,logs);Assert.Equal(1,exit);
    }
}
