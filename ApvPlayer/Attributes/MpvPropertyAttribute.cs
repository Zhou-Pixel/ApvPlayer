using System;

namespace ApvPlayer.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MpvPropertyAttribute : Attribute
{
    public MpvPropertyAttribute(string mpvPropertyName)
    {
        MpvPropertyName = mpvPropertyName;
    }
    public string MpvPropertyName { get; }
}