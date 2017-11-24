using System;

namespace CuteAnt
{
  /// <summary>Defines interface for base entity type. All entities in the system must implement this interface.</summary>
  /// <typeparam name="TPrimaryKey">Type of the primary key of the entity</typeparam>
  public interface IHasId<TPrimaryKey>
  {
    /// <summary>Unique identifier for this entity.</summary>
    TPrimaryKey Id { get; set; }
  }
}