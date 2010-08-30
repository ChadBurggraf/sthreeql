//-----------------------------------------------------------------------
// <copyright file="ScheduleOperationType.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;

    /// <summary>
    /// Defines the possible schedule operation types.
    /// </summary>
    public enum ScheduleOperationType
    {
        /// <summary>
        /// Identifies that the schedule is performing a backup operation.
        /// </summary>
        Backup,

        /// <summary>
        /// Identifies that the schedule is performing a restore operation.
        /// </summary>
        Restore
    }
}
