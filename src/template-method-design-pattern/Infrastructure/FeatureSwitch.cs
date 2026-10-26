using TemplateMethodDesignPattern.Contracts;

namespace TemplateMethodDesignPattern.Infrastructure;

/// <summary>A simple switch so the demo can show the guard before and after it is enabled.</summary>
public sealed class FeatureSwitch(bool enabled) : IImportFeatureSwitch
{
    public Task<bool> IsEnabledAsync() => Task.FromResult(enabled);
}
