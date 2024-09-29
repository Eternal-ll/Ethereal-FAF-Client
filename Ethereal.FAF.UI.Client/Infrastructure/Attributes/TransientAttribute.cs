using System;
using System.Diagnostics.CodeAnalysis;

namespace Ethereal.FAF.UI.Client.Infrastructure.Attributes
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    [AttributeUsage(AttributeTargets.Class)]
    public class TransientAttribute : Attribute
    {
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        public Type? InterfaceType { get; }

        public TransientAttribute() { }

        public TransientAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}
