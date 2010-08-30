//-----------------------------------------------------------------------
// <copyright file="SThreeQLService.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Service
{
    using System;
    using System.ServiceProcess;

    /// <summary>
    /// Provides service-based execution of SThreeQL tasks.
    /// </summary>
    public class SThreeQLService
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        public static void Main()
        {
            ServiceBase.Run(new ServiceBase[] { new SThreeQLProcessor() });
        }
    }
}
