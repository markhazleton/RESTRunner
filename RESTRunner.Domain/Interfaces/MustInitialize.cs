﻿namespace RESTRunner.Domain.Interfaces
{
    /// <summary>
    /// MustInitialize
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MustInitialize<T>
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public MustInitialize(T parameters)
        {
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
