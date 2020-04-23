using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using SimpleInjector;
using Twinkle.View.Attributes;
using Twinkle.View.Infrastructure;

namespace Twinkle.View
{
    public class TwinkleView
    {
        public Dictionary<string, Object> Context { get; set; }
        private IConfiguration Config { get; set; }
        internal DI DI { get; set; }
        internal GlobalConfiguraionFramework GlobalConfiguraionFramework { get; set; }
        public Context SystemContext { get; internal set; }
        private Dictionary<Type, object> AllViews { get; set; }

        private List<Assembly> AssembliesViews = new List<Assembly>();

        Logger logger = LogManager.GetCurrentClassLogger();

        public TwinkleView(Dictionary<string, object> context = null)
        {
            //Initializing the user context
            Context = context ?? new Dictionary<string, object>();

            //Load TwinkleViewSettings.json and place it in this.Config
            LoadConfig();

            //Preparing the global configuration of this.GlobalConfiguraionFramework
            PrepareGlobalConfigurationFramework();
            
            //NLog initialization
            InitializeNlog();

            //Configure the DI and put it in this.DI
            ConfigureDI();

            DI.RunTimeRegister<TwinkleView>(() => this);
            
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
                        if (type.GetCustomAttributes(typeof(ViewAttribute), false).Length > 0)
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

            SystemContext = DI.Container.GetInstance<Context>();
            SystemContext.DI = DI;
            SystemContext.Config = Config;
            SystemContext.Views = new Views();
            SystemContext.GlobalConfiguraionFramework = GlobalConfiguraionFramework;

            if (SystemContext.Views.AllViews == null)
                SystemContext.Views.AllViews = new Dictionary<Type, object>();

            foreach (var view in AllViews)
            {
                var viewInstance = SystemContext.DI.Container.GetInstance(view.Key);
                viewInstance.GetType().GetProperty("TwinkleView")?.SetValue(viewInstance, this);
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
                        prop => Attribute.IsDefined(prop, typeof(ControlBaseAttribute))).ToList();
                    foreach (var propertyInfo in controlsView)
                    {
                        var type = propertyInfo.PropertyType;
                        var controlAttribute = propertyInfo.GetCustomAttributes(typeof(ControlBaseAttribute), true).FirstOrDefault() as
                            ControlBaseAttribute;

                        var prefix = ((ViewAttribute)view.Key.GetCustomAttribute(typeof(ViewAttribute), true)).Prefix;
                        Control newVal = (Control)Activator.CreateInstance(
                            type,
                            prefix + controlAttribute.Alias,
                            controlAttribute.XPath,
                            controlAttribute.Css,
                            this);

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
                        .Where(m => m.GetCustomAttributes(typeof(PreViewDefinitionCriteriaBaseAttribute), false).Length > 0)
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
        /// Getting a view by type
        /// </summary>
        /// <typeparam name="T">Type to which the view is to be given</typeparam>
        /// <returns>Object of view</returns>
        public T GetView<T>()
        {
            return (T)SystemContext.Views.AllViews[typeof(T)];
        }

        /// <summary>
        /// Getting a presentation on alias
        /// </summary>
        /// <typeparam name="T">Type to which the view is to be given</typeparam>
        /// <param name="alias">View alias</param>
        /// <returns></returns>
        public T GetView<T>(string alias)
        {
            return (T)SystemContext.Views.AllViews.FirstOrDefault(x=>x.Value.Equals(alias)).Value;
        }
        
        /// <summary>
        /// Creation of control by the transferred parameters
        /// </summary>
        /// <typeparam name="T">Type to which the control will be given</typeparam>
        /// <param name="alias">Name</param>
        /// <param name="xpath">Xpath</param>
        /// <param name="css">Css</param>
        /// <returns>Object of control</returns>
        public T CreateControl<T>(string alias = "", string xpath = "", string css = "")
        {
            var resultControl = (T) Activator.CreateInstance(typeof(T));
            typeof(T).GetProperty("Alias")?.SetValue(resultControl, alias);
            typeof(T).GetProperty("XPath")?.SetValue(resultControl, xpath);
            typeof(T).GetProperty("Css")?.SetValue(resultControl, css);
            typeof(T).GetProperty("TwinkleView")?.SetValue(resultControl, this);
            return resultControl;
        }

        /// <summary>
        /// A public method of obtaining active views
        /// </summary>
        public void DefiniteActiveViews()
        {
            GetActiveViews();
        }

        /// <summary>
        /// Public method of exporting active views
        /// </summary>
        public void ExportDefiniteActiveViews()
        {
            GetActiveViews();

            var pathToFileDataExchange = "DataExchange";
            var nameExchangeFile_ActiveViews = "activeViews.json";

            var currentActiveViews = SystemContext.Views.ActiveViews;
            var listAliasActiveViews = new List<string>();
            foreach (var currentActiveView in currentActiveViews)
            {
                listAliasActiveViews.Add(currentActiveView.Key.Name);
            }

            string json = JsonConvert.SerializeObject(listAliasActiveViews);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathToFileDataExchange);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, nameExchangeFile_ActiveViews), json);
        }
        
        /// <summary>
        /// Public method of exporting a blank list
        /// </summary>
        public void ExportEmptyActiveViews()
        {
            var pathToFileDataExchange = "DataExchange";
            var nameExchangeFile_ActiveViews = "activeViews.json";

            var listAliasActiveViews = new List<string>();

            string json = JsonConvert.SerializeObject(listAliasActiveViews);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathToFileDataExchange);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, nameExchangeFile_ActiveViews), json);
        }

        /// <summary>
        /// Getting control from active views
        /// </summary>
        /// <typeparam name="T">Type to which the control will be given</typeparam>
        /// <param name="alias">Alias Control</param>
        /// <returns>Object of control</returns>
        public T GetControl<T>(string alias) where T : class
        {
            var iterationsCount = SystemContext.GlobalConfiguraionFramework.SearchControlNumberOfRetries;

            while (iterationsCount>0)
            {
                var result = TryGetControl<T>(alias);

                if (result != null)
                    return result;

                if(iterationsCount!=1)
                    Thread.Sleep(SystemContext.GlobalConfiguraionFramework.SearchControlTimeBetweenRetries);

                --iterationsCount;
            }

            var strException = $"Control {alias} is not found in active views.\n" +
                               $"Аctive views:";
            foreach (var activeView in SystemContext.Views.ActiveViews)
            {
                strException += "\n" + activeView.Key;
            }
            
            throw new Exception(strException);
        }

        /// <summary>
        /// Getting control from active views 
        /// </summary>
        /// <typeparam name="T">Type to which the control will be given</typeparam>
        /// <param name="alias">Alias Control</param>
        /// <returns>Object of control</returns>
        private T TryGetControl<T>(string alias) where T : class
        {
            GetActiveViews();

            foreach (var view in SystemContext.Views.ActiveViews)
            {
                var controlsView = view.Key.GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(ControlBaseAttribute))).ToList();

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
        private Type TryGetTypeControl(string alias)
        {
            GetActiveViews();

            foreach (var view in SystemContext.Views.ActiveViews)
            {
                var controlsView = view.Key.GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(ControlBaseAttribute))).ToList();

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

        /// <summary>
        /// Getting control type
        /// </summary>
        /// <param name="alias">Alias Control</param>
        /// <returns>Type of control</returns>
        public Type GetTypeControl(string alias)
        {
            var iterationsCount = SystemContext.GlobalConfiguraionFramework.SearchControlNumberOfRetries;

            while (iterationsCount > 0)
            {
                var result = TryGetTypeControl(alias);

                if (result != null)
                    return result;

                if (iterationsCount != 1)
                    Thread.Sleep(SystemContext.GlobalConfiguraionFramework.SearchControlTimeBetweenRetries);

                --iterationsCount;
            }
            var strException = $"Control {alias} is not found in active views.\n" +
                               $"Active views:";
            foreach (var activeView in SystemContext.Views.ActiveViews)
            {
                strException += "\n" + activeView.Key;
            }
            
            throw new Exception(strException);
        }
    }
}