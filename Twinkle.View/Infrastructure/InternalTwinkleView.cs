using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using NLog;
using SimpleInjector;
using Twinkle.View.Attributes;

namespace Twinkle.View.Infrastructure
{
    internal class InternalTwinkleView
    {
        internal UserContext Context { get; set; }
        internal IConfiguration Config { get; set; }
        internal DI DI { get; set; }
        internal GlobalConfiguraionFramework GlobalConfiguraionFramework { get; set; }
        internal Context SystemContext { get; set; }
        private Dictionary<Type, object> AllViews { get; set; }

        private List<Assembly> AssembliesViews = new List<Assembly>();

        Logger logger = LogManager.GetCurrentClassLogger();

        public InternalTwinkleView(Dictionary<string, object> context = null)
        {
            //Initialize user context
            Context = new UserContext();
            
            //Load TwinkleViewSettings.json and place it in this.Config
            LoadConfig();

            //Preparing the global configuration of this.GlobalConfiguraionFramework
            PrepareGlobalConfigurationFramework();
            
            //NLog initialization
            InitializeNlog();

            //Configure the DI and put it in this.DI
            ConfigureDI();

            DI.RunTimeRegister<InternalTwinkleView>(() => this);
            
            //Getting all project views, registering them in DI and initializing them.
            GetAndInitializeAllView();
            
            //Forming the execution context of this.SystemContext
            ConfigureSystemContext();

            //Initialization of all controls of all view
            InitializeAllControls();
        }

        /// <summary>
        /// NLog initialization
        /// </summary>
        private void InitializeNlog()
        {
            if (!GlobalConfiguraionFramework.LoggingEnable)
                return;

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = $"{AppContext.BaseDirectory}/twinkle_view_log.txt"
            };
            
            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            
            // Apply config           
            LogManager.Configuration = config;
        }
        
        /// <summary>
        /// Preparation of global configuration Twinkle.View
        /// </summary>
        private void PrepareGlobalConfigurationFramework()
        {
            logger.Info("Preparing the global configuration of Twinkle.View");
            GlobalConfiguraionFramework = new GlobalConfiguraionFramework(Config);
        }

        /// <summary>
        /// Configuration file upload
        /// </summary>
        private void LoadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.AppContext.BaseDirectory)
                .AddJsonFile("TwinkleViewSettings.json",
                    optional: false,
                    reloadOnChange: true);
            Config = builder.Build();
        }

        /// <summary>
        /// DI configuration
        /// </summary>
        private void ConfigureDI()
        {
            logger.Info("DI configuration");
            try
            {
                var di = new DI();
                di.Configure();
                DI = di;
            }
            catch (Exception e)
            {
                logger.Error(e, "DI configuration error");
            }
        }
        
        /// <summary>
        /// Getting and initializing views from the specified dll in the configuration file.
        /// </summary>
        private void GetAndInitializeAllView()
        {
            try
            {
                logger.Info("Getting all types of views and initialization...");
                
                var swGetAndInitializeAllView = new Stopwatch();
                swGetAndInitializeAllView.Start();

                var dllWithViews = GlobalConfiguraionFramework.DllContainingViews;

                if (AllViews == null)
                    AllViews = new Dictionary<Type, object>();

                foreach (var dll in dllWithViews)
                {
                    var assembly = GetAssemblyByName(dll.Trim());
                    if (assembly == null)
                        throw new Exception($"assembly {dll} is null");

                    List<Type> collectionTypes = assembly.GetTypes().ToList();
                    AssembliesViews.Add(assembly);

                    foreach (Type type in collectionTypes)
                    {
                        if (type.IsSubclassOf(typeof(View)))
                        {
                            DI.RunTimeRegister(type, Lifestyle.Singleton);                            
                            AllViews.Add(type, null);
                        }
                    }                    
                }
                swGetAndInitializeAllView.Stop();
                
                logger.Info("Getting the types of all views and initialization completed:" + "\r\n" +
                            $"Number of views: {AllViews.Count}" + "\r\n" +
                            $"Execution time: {swGetAndInitializeAllView.ElapsedMilliseconds} ms.");
            }
            catch (Exception e)
            {
                logger.Error(e, "Error of receiving all types of views or during initialization");
                throw;
            }
        }

        /// <summary>
        /// Configuration of the system context for the framework
        /// </summary>
        private void ConfigureSystemContext()
        {            
            var swConfigureContext = new Stopwatch();
            swConfigureContext.Start();

            SystemContext = new Context
            {
                DI = DI,
                Config = Config,
                Views = new Views(),
                GlobalConfiguraionFramework = GlobalConfiguraionFramework
            };

            if (SystemContext.Views.AllViews == null)
                SystemContext.Views.AllViews = new Dictionary<Type, object>();

            foreach (var view in AllViews)
            {
                var viewInstance = SystemContext.DI.Container.GetInstance(view.Key);
                SystemContext.Views.AllViews[view.Key] = viewInstance;
            }
            swConfigureContext.Stop();
            logger.Info("Context formation complete:" + "\r\n" +
                            $"Execution time: {swConfigureContext.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Initialization of controls in all views
        /// </summary>
        private void InitializeAllControls()
        {
            try
            {
                logger.Info("Initialization of controls...");
                var swInitializeAllControls = new Stopwatch();
                swInitializeAllControls.Start();
                foreach (var view in SystemContext.Views.AllViews)
                {
                    List<PropertyInfo> controlsView = view.Key.GetProperties().Where(
                        prop => Attribute.IsDefined(prop, typeof(ControlAttribute))).ToList();
                    foreach (var propertyInfo in controlsView)
                    {
                        var type = propertyInfo.PropertyType;
                        var controlAttribute = propertyInfo.GetCustomAttributes(typeof(ControlAttribute), true).FirstOrDefault() as
                            ControlAttribute;

                        var prefix = ((View)view.Value).Prefix;
                        Control newVal = (Control)Activator.CreateInstance(type);
                        newVal.Alias = prefix + controlAttribute.Alias;
                        newVal.XPath = controlAttribute.XPath;
                        newVal.Css = controlAttribute.Css;

                        var listHooks = ((View) view.Value).Hooks.FindAll(x => x.AliasControl.Equals(controlAttribute.Alias));
                        newVal.Hooks = new List<Hook>(listHooks);

                        propertyInfo.SetValue(view.Value, newVal);
                        
                    }
                }
                swInitializeAllControls.Stop();
                logger.Info("Initialization of controls completed:" + "\r\n" +
                            $"Execution time: {swInitializeAllControls.ElapsedMilliseconds} ms.");
            }
            catch (Exception e)
            {
                logger.Error(e, "Error during control initialization");
            }
        }

        /// <summary>
        /// Receiving active views
        /// </summary>
        internal void GetActiveViews()
        {
            logger.Info("Receiving active views");

            try
            {
                var swGetActiveViews = new Stopwatch();
                swGetActiveViews.Start();

                SystemContext.Views.ActiveViews?.Clear();
                if (SystemContext.Views.ActiveViews == null)
                    SystemContext.Views.ActiveViews = new Dictionary<Type, object>();
                foreach (var view in SystemContext.Views.AllViews)
                {
                    ((View)view.Value).IsActive = false;
                }

                foreach (var assemblyViews in AssembliesViews)
                {
                    var viewExtensionsMethods = ReflectionExtensions.GetExtensionMethods(typeof(View), assemblyViews);
                    var preViewExtensionsMethods = viewExtensionsMethods
                        .Where(m => m.GetCustomAttributes(typeof(PreViewDefinitionCriteriaAttribute), false).Length > 0)
                        .ToList();
                    foreach (var preViewExtensionsMethod in preViewExtensionsMethods)
                    {
                        preViewExtensionsMethod.Invoke(SystemContext.Views.AllViews.First().Value, new []{SystemContext.Views.AllViews.First().Value});
                    }
                }

                foreach (var view in SystemContext.Views.AllViews)
                {
                    var resultCriteria = new List<bool>();

                    var criteriaTargetView = ((View) view.Value).ViewDefinitionCriteria;

                    foreach (var criteria in criteriaTargetView)
                    {
                        resultCriteria.Add(criteria.Invoke());
                    }

                    if (criteriaTargetView.Count != 0 && !resultCriteria.Contains(false))
                    {
                        SystemContext.Views.ActiveViews.Add(view.Key, view.Value);
                        ((View) view.Value).IsActive = true;
                    }
                }
                swGetActiveViews.Stop();
                logger.Info("Identification of active views completed:" + "\r\n" +
                            $"Defined {SystemContext.Views.ActiveViews.Count} of active views" + "\r\n" +
                            $"Execution time: {swGetActiveViews.ElapsedMilliseconds} ms.");
            }
            catch (Exception e)
            {
                logger.Error(e, "Error in receiving active views.");
                throw;
            }
        }

        /// <summary>
        /// Getting the assembly by name
        /// </summary>
        /// <param name="name">Name assembly</param>
        /// <returns></returns>
        private Assembly GetAssemblyByName(string name)
        {
            logger.Info($"Getting the assembly \"{name}\"");
            try
            {
                AppDomain.CurrentDomain.Load(name);
                return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error on receiving assembly with name \"{name}\"");
                throw;
            }
        }

        /// <summary>
        /// Getting control from active views 
        /// </summary>
        /// <typeparam name="T">Type to which the control will be given</typeparam>
        /// <param name="alias">Alias Control</param>
        /// <returns>Object of control</returns>
        internal T TryGetControl<T>(string alias) where T : class
        {
            GetActiveViews();

            foreach (var view in SystemContext.Views.ActiveViews)
            {
                var controlsView = view.Key.GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(ControlAttribute))).ToList();

                foreach (var propertyInfo in controlsView)
                {
                    var control = (Control)propertyInfo.GetValue(view.Value);

                    if (control.Alias.Equals(alias))
                    {
                        return (T)propertyInfo.GetValue(view.Value);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Getting control type
        /// </summary>
        /// <param name="alias">Alias Control</param>
        /// <returns>Type of control</returns>
        internal Type TryGetTypeControl(string alias)
        {
            GetActiveViews();

            foreach (var view in SystemContext.Views.ActiveViews)
            {
                var controlsView = view.Key.GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(ControlAttribute))).ToList();

                foreach (var propertyInfo in controlsView)
                {
                    var control = (Control)propertyInfo.GetValue(view.Value);

                    if (control.Alias.Equals(alias))
                    {
                        return propertyInfo.PropertyType;
                    }
                }
            }
            return null;
        }

    }
}