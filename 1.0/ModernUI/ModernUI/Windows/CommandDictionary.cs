using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ModernUI.Windows
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
                             /// <summary>
                             ///     Represents a collection of commands keyed by a uri.
                             /// </summary>
    public class CommandDictionary
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
        : Dictionary<Uri, ICommand>
    {
    }
}