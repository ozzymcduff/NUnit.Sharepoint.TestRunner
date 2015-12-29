using System;

namespace NUnit.Hosted.Utilities
{
    public interface IMessage
    {
        Messages.Type Type { get; }
    }
}
