using System.Collections;

namespace JurrianMod.Events
{
    public interface IChaosEvent
    {
        string Name { get; }
    
        float DurationInSeconds { get; }

        IEnumerator Run();
    }
}