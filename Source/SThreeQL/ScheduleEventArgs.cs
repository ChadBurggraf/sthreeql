//-----------------------------------------------------------------------
// <copyright file="ScheduleEventArgs.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using SThreeQL.Configuration;

    /// <summary>
    /// Arguments for schedule events.
    /// </summary>
    [Serializable]
    public class ScheduleEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the exception that caused the event to be thrown, if applicable.
        /// </summary>
        public Exception ErrorException { get; set; }

        /// <summary>
        /// Gets or sets the schedule's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of operation being performed.
        /// </summary>
        public ScheduleOperationType OperationType { get; set; }

        /// <summary>
        /// Gets or sets the schedule's repeat type.
        /// </summary>
        public ScheduleRepeatType RepeatType { get; set; }

        /// <summary>
        /// Gets or sets the schedule's start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the target being operated on.
        /// </summary>
        public string TargetName { get; set; }
    }
}
