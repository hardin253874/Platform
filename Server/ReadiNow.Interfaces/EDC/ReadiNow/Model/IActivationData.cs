// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Contains data to create an entity.
    /// </summary>
    public interface IActivationData
    {
        /// <summary>
        /// Create an instance of the entity.
        /// </summary>
        /// <returns></returns>
        IEntity CreateEntity( );
    }
}
