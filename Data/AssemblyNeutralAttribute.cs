﻿using System;

namespace SQL.Data
{
    [AssemblyNeutral]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class AssemblyNeutralAttribute : Attribute
    {
    }
}