using System.Diagnostics;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Serialization;

namespace MPM_Betting.Services;

[PSerializable]
[MulticastAttributeUsage(MulticastTargets.Method)]
public class ProfileAttribute : MethodInterceptionAspect
{
    public override void OnInvoke(MethodInterceptionArgs args)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        args.Proceed();
        stopwatch.Stop();
        Console.WriteLine($"{args.Method.Name} took: {stopwatch.Elapsed}");
    }
}