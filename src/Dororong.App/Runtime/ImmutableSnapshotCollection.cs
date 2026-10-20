namespace Dororong.App.Runtime;

// Compare against our owned immutable snapshot, never a producer's mutable
// list. Only the latest snapshot is retained; changes preserve order/identity.
internal static class ImmutableSnapshotCollection
{
    internal static IReadOnlyList<T> CopyOrReuse<T>(IReadOnlyList<T> source,IReadOnlyList<T>? previous){
        if(previous is not null&&source.Count==previous.Count){
            var same=true;var equality=EqualityComparer<T>.Default;
            for(var i=0;i<source.Count;i++)if(!equality.Equals(source[i],previous[i])){same=false;break;}
            if(same)return previous;
        }
        return Array.AsReadOnly(source.ToArray());
    }
}
