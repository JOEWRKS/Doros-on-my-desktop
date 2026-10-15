using Dororong.Portable.Desktop;
if(args.Length!=1)return 2;
using var lease=SingleInstanceLease.TryAcquire(args[0]);
Console.WriteLine(lease is null?"DUPLICATE":"ACQUIRED");Console.Out.Flush();
if(lease is not null)Console.ReadLine();
return 0;
