﻿#region

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UlteriusServer.Api;
using UlteriusServer.Api.Services.LocalSystem;
using UlteriusServer.Api.Win32;
using UlteriusServer.TerminalServer;
using UlteriusServer.Utilities;
using UlteriusServer.WebCams;
using UlteriusServer.WebServer;

#endregion

namespace UlteriusServer
{
    public class Ulterius
    {
        private SystemService systemService;
        private bool isService;

        public void Start(bool serviceMode = false)
        {
            isService = serviceMode;

            Setup();
        }

        /// <summary>
        ///     Starts various parts of the server than loop to keep everything alive.
        /// </summary>
        private void Setup()
        {
            

            Tools.RestartDaemon();
             Console.WriteLine("Creating settings");
             var settings = Config.Load();
            Console.WriteLine("Configuring up server");
            Tools.ConfigureServer();
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
            var useTerminal = settings.Terminal.AllowTerminal;
            var useWebServer = settings.WebServer.ToggleWebServer;
            var useWebCams = settings.Webcams.UseWebcams;
            if (useWebCams)
            {
                Console.WriteLine("Loading Webcams");
                WebCamManager.LoadCameras();
            }
            if (useWebServer)
            {
                Console.WriteLine("Setting up HTTP Server");
                HttpServer.Setup();
            }
            systemService = new SystemService();
            Console.WriteLine("Creating system service");
            systemService.Start();
            UlteriusApiServer.RunningAsService = Tools.RunningAsService();
            if (UlteriusApiServer.RunningAsService)
            {
                Console.Write($"Service: {UlteriusApiServer.RunningAsService}");
                DesktopWatcher.Start();
            }
            UlteriusApiServer.Start();
           
            if (useTerminal)
            {
                Console.WriteLine("Starting Terminal API");
                TerminalManagerServer.Start();
            }
            try
            {
                var useUpnp = settings.Network.UpnpEnabled;
                if (useUpnp)
                {
                    Console.WriteLine("Trying to forward ports");
                    Tools.ForwardPorts();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to forward ports");
            }
        }


    }
}