using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace WhenTheFireFades.Tests.TestHelpers;

public sealed class TestSessionFeature : ISessionFeature
{
    public ISession Session { get; set; } = null!;
}