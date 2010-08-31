//-----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Service
{
    using System;
    using System.ComponentModel;
    using System.Configuration.Install;

    /// <summary>
    /// Implements <see cref="Installer"/> for the SThreeQL service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Initializes a new instance of the ProjectInstaller class.
        /// </summary>
        public ProjectInstaller()
        {
            this.InitializeComponent();
        }
    }
}
