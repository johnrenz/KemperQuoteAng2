using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Unity.Mvc5;
using Unity;
//using KdQuoteLibrary.Interfaces;
//using KdQuoteLibrary.Services;
using AutoQuoteLibrary.AbstractServices;
using AutoQuoteLibrary.Services;
using KemperQuoteAngular2.Controllers;

namespace KemperQuoteAngular2
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<ISessionServices, SessionServices>();
            container.RegisterType<IVINServices, VINServices>();
            container.RegisterType<ILoggingServices, LoggingServices>();
            container.RegisterType<IController, AjaxController>("Ajax");

            return container;
        }
    }

}