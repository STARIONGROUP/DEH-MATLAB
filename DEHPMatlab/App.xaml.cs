﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="RHEA System S.A.">
// Copyright (c) 2020-2022 RHEA System S.A.
// 
// Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate.
// 
// This file is part of DEHPMatlab
// 
// The DEHPMatlab is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// The DEHPMatlab is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program; if not, write to the Free Software Foundation,
// Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DEHPMatlab
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using Autofac;

    using DEHPCommon;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.Services.MatlabConnector;
    using DEHPMatlab.Services.MatlabParser;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Dialogs;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.ViewModel.NetChangePreview;
    using DEHPMatlab.ViewModel.NetChangePreview.Interfaces;
    using DEHPMatlab.Views;

    using DevExpress.Xpf.Core;

    using NLog;

    using DXSplashScreenViewModel = DevExpress.Mvvm.DXSplashScreenViewModel;
    using SplashScreen = DEHPCommon.UserInterfaces.Views.SplashScreen;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class App
    {
        /// <summary>
        /// The <see cref="NLog"/> logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new <see cref="App"/>
        /// </summary>
        /// <param name="containerBuilder">An optional <see cref="Container"/></param>
        public App(ContainerBuilder containerBuilder = null)
        {
            this.LogAppStart();
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomainUnhandledException;

            var splashScreenViewModel = new DXSplashScreenViewModel()
            {
                Title = "DEHP-Matlab Adapter",
                Logo = new Uri($"pack://application:,,,/Resources/logo.png")
            };

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            SplashScreenManager.Create(() => new SplashScreen(), splashScreenViewModel).ShowOnStartup();
            containerBuilder ??= new ContainerBuilder();
            this.RegisterTypes(containerBuilder);
            this.RegisterViewModels(containerBuilder);
            AppContainer.BuildContainer(containerBuilder);
        }

        /// <summary>
        /// Registers all the view model so the dependencies can be injected
        /// </summary>
        /// <param name="containerBuilder">The <see cref="ContainerBuilder"/></param>
        private void RegisterViewModels(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();
            containerBuilder.RegisterType<HubDataSourceViewModel>().As<IHubDataSourceViewModel>().SingleInstance();
            containerBuilder.RegisterType<MatlabStatusBarControlViewModel>().As<IStatusBarControlViewModel>().SingleInstance();
            containerBuilder.RegisterType<DstDataSourceViewModel>().As<IDstDataSourceViewModel>();
            containerBuilder.RegisterType<DstBrowserHeaderViewModel>().As<IDstBrowserHeaderViewModel>();
            containerBuilder.RegisterType<DstVariablesControlViewModel>().As<IDstVariablesControlViewModel>();
            containerBuilder.RegisterType<DstConnectViewModel>().As<IDstConnectViewModel>();
            containerBuilder.RegisterType<DstMappingConfigurationDialogViewModel>().As<IDstMappingConfigurationDialogViewModel>();
            containerBuilder.RegisterType<HubMappingConfigurationDialogViewModel>().As<IHubMappingConfigurationDialogViewModel>();
            containerBuilder.RegisterType<MatlabTransferControlViewModel>().As<ITransferControlViewModel>().SingleInstance();
            containerBuilder.RegisterType<MappingViewModel>().As<IMappingViewModel>().SingleInstance();
            containerBuilder.RegisterType<DstNetChangePreviewViewModel>().As<IDstNetChangePreviewViewModel>().SingleInstance();
            containerBuilder.RegisterType<HubNetChangePreviewViewModel>().As<IHubNetChangePreviewViewModel>().SingleInstance();
            containerBuilder.RegisterType<DifferenceViewModel>().As<IDifferenceViewModel>().SingleInstance();
            containerBuilder.RegisterType<MappingConfigurationServiceDialogViewModel>().As<IMappingConfigurationServiceDialogViewModel>();
        }

        /// <summary>
        /// Registers the types that can be resolved by the <see cref="IContainer"/>
        /// </summary>
        /// <param name="containerBuilder">The <see cref="ContainerBuilder"/></param>
        private void RegisterTypes(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MatlabConnector>().As<IMatlabConnector>().SingleInstance();
            containerBuilder.RegisterType<DstController.DstController>().As<IDstController>().SingleInstance();
            containerBuilder.RegisterType<MatlabParser>().As<IMatlabParser>().SingleInstance();
            containerBuilder.RegisterType<MappingEngine>().As<IMappingEngine>().WithParameter(MappingEngine.ParameterName, Assembly.GetExecutingAssembly());
            containerBuilder.RegisterType<MappingConfigurationService>().As<IMappingConfigurationService>().SingleInstance();
        }

        /// <summary>
        /// Add a header to the log file
        /// </summary>
        private void LogAppStart()
        {
            this.logger.Info("-----------------------------------------------------------------------------------------");
            this.logger.Info($"Starting Matlab Adapter {Assembly.GetExecutingAssembly().GetName().Version}");
            this.logger.Info("-----------------------------------------------------------------------------------------");
        }

        /// <summary>
        /// Warn when an exception is thrown and log it 
        /// </summary>
        /// <param name="sender">The <see cref="object"/> sender</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/></param>
        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var errorMessage = $"{sender} has thrown {e.ExceptionObject.GetType()} \n\r {(e.ExceptionObject as Exception)?.Message}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.logger.Error(e.ExceptionObject);
        }

        /// <summary>
        /// Occurs when <see cref="Application"/> starts, starts a new <see cref="ILifetimeScope"/> and open the <see cref="Application.MainWindow"/>
        /// </summary>
        /// <param name="e">The <see cref="StartupEventArgs"/></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                scope.Resolve<INavigationService>().Show<MainWindow>();
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// Occurs when <see cref="Application"/> is closed
        /// </summary>
        /// <param name="e">The <see cref="ExitEventArgs"/></param>
        protected override void OnExit(ExitEventArgs e)
        {
            AppContainer.Container.Resolve<IDstController>().Disconnect();
            base.OnExit(e);
        }

        /// <summary>
        /// Handles dispatcher unhandled exception
        /// </summary>
        /// <param name="sender">The <see cref="object"/> sender</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/></param>
        public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.Exception);
            e.Handled = true;
        }
    }
}
