using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public enum SyncMode
    {
        /// <summary>
        /// In volatile mode, edits/updates are expected to occur to the original reference
        /// of the object.  Synchronizing shall update the clone
        /// </summary>
        Volatile,
        /// <summary>
        /// In cloned mode, edits/updates are expected to occur on a clone of the original
        /// reference of the object.  Synchronizing shall update the original
        /// </summary>
        Cloned,
        /// <summary>
        /// In value mode, it is assumed the data is not a reference but a value.  In this case,
        /// operation is similar to Cloned mode, except the actual cloning of the object is
        /// expected to be much simpler as it is a value vs reference type
        /// </summary>
        Value
    }

    /// <summary>
    /// Indicates this object can detect if changes have been made to a
    /// particular object-specific data area
    /// </summary>
    public interface IDirtyMarker
    {
        /// <summary>
        /// Whether data presently matches data when object was first initialized
        /// </summary>
        bool IsDirty { get; }

        event Action<IDirtyMarker> DirtyStateChanged;
    }

    /// <summary>
    /// Interface to retrieve similarity status of identical or similar data sets
    /// </summary>
    public interface ISync : IDirtyMarker
    {
        SyncMode SyncMode { get; }

        /// <summary>
        /// Bring original and new data into sync with each other
        /// </summary>
        void Sync();
    }


    public interface ISyncWithValue : ISync
    {
        /// <summary>
        /// Original data object.  In Volatile mode, this is a clone of the original reference.  In Clone
        /// mode, this is the original reference.
        /// The setter updates ISyncWithValue's copy of the original reference, and
        /// reclones.  In Volatile mode, getter reflects the fresh clone.
        /// </summary>
        object Original { get; set; }
        /// <summary>
        /// Updated version of object.  In Volatile mode, this is the original reference.  In Cloned mode, this
        /// is the clone.
        /// </summary>
        object Updated { get; }
    }


    /// <summary>
    /// Interface to abstract a single undo operation
    /// </summary>
    public interface IUndoable
    {
        void Undo();
    }


    /// <summary>
    /// NOTE:  In context of ISync, will tend to be an alias over the Sync() operation
    /// EXPERIMENTAL
    /// </summary>
    public interface ICommitable
    {
        void Commit();
    }
}
