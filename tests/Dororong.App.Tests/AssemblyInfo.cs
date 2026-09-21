using Xunit;

// These tests create WPF presenters on short-lived STA threads. WPF's shared
// pack-resource cache is process-wide and PackagePart stream tracking is not
// safe under concurrent LoadComponent calls. Match the product's single UI
// thread instead of racing unrelated test classes through the same BAML cache.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

internal static class WpfTestBootstrap
{
    // A test runner does not start WPF Application like the product does.
    // Register the pack URI parser before any test loads a static image bank,
    // independent of xUnit's test-class ordering.
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void InitializePackUriParser() =>
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
            typeof(System.Windows.Application).TypeHandle);
}
