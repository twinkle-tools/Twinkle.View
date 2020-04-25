using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Twinkle.View.Infrastructure;

namespace Twinkle.View
{
    public class TwinkleView
    {
        internal static InternalTwinkleView InternalTwinkleView => ThreadsManager.GetTargetFramework();

        public static UserContext Context
        {
            get
            {
                var targetFramework = ThreadsManager.GetTargetFramework();
                return targetFramework.Context;
            }
        }
        public static GlobalConfiguraionFramework Configuraion
        {
            get
            {
                var targetFramework = ThreadsManager.GetTargetFramework();
                return targetFramework.GlobalConfiguraionFramework;
            }
        }

        public static T GetView<T>()
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            var view = (T)targetFramework.SystemContext.Views.AllViews[typeof(T)];
            return view;
        }

        public static T GetView<T>(string alias)
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            return (T)targetFramework.SystemContext.Views.AllViews.FirstOrDefault(x=>x.Value.Equals(alias)).Value;
        }

        public static T CreateControl<T>(string alias = "", string xpath = "", string css = "")
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            var resultControl = (T) Activator.CreateInstance(typeof(T));
            typeof(T).GetProperty("Alias")?.SetValue(resultControl, alias);
            typeof(T).GetProperty("XPath")?.SetValue(resultControl, xpath);
            typeof(T).GetProperty("Css")?.SetValue(resultControl, css);
            return resultControl;
        }

        public static void DefiniteActiveViews()
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            targetFramework.GetActiveViews();
        }

        //todo переделать
        public static void ExportDefiniteActiveViews()
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            targetFramework.GetActiveViews();

            var pathToFileDataExchange = "DataExchange";
            var nameExchangeFile_ActiveViews = "activeViews.json";

            var currentActiveViews = targetFramework.SystemContext.Views.ActiveViews;
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

        public static void ExportEmptyActiveViews()
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            var pathToFileDataExchange = "DataExchange";
            var nameExchangeFile_ActiveViews = "activeViews.json";

            var listAliasActiveViews = new List<string>();

            string json = JsonConvert.SerializeObject(listAliasActiveViews);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathToFileDataExchange);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, nameExchangeFile_ActiveViews), json);
        }

        public static T GetControl<T>(string alias) where T : class
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            var iterationsCount = targetFramework.SystemContext.GlobalConfiguraionFramework.SearchControlNumberOfRetries;

            while (iterationsCount>0)
            {
                var result = targetFramework.TryGetControl<T>(alias);

                if (result != null)
                    return result;

                if(iterationsCount!=1)
                    //todo переделать
                    Thread.Sleep(targetFramework.SystemContext.GlobalConfiguraionFramework.SearchControlTimeBetweenRetries);

                --iterationsCount;
            }

            var strException = $"Control {alias} is not found in active views.\n" +
                               $"Аctive views:";
            foreach (var activeView in targetFramework.SystemContext.Views.ActiveViews)
            {
                strException += "\n" + activeView.Key;
            }
            
            throw new Exception(strException);
        }

        public static Type GetTypeControl(string alias)
        {
            var targetFramework = ThreadsManager.GetTargetFramework();
            var iterationsCount = targetFramework.SystemContext.GlobalConfiguraionFramework.SearchControlNumberOfRetries;

            while (iterationsCount > 0)
            {
                var result = targetFramework.TryGetTypeControl(alias);

                if (result != null)
                    return result;

                if (iterationsCount != 1)
                    //todo переделать
                    Thread.Sleep(targetFramework.SystemContext.GlobalConfiguraionFramework.SearchControlTimeBetweenRetries);

                --iterationsCount;
            }
            var strException = $"Control {alias} is not found in active views.\n" +
                               $"Active views:";
            foreach (var activeView in targetFramework.SystemContext.Views.ActiveViews)
            {
                strException += "\n" + activeView.Key;
            }
            
            throw new Exception(strException);
        }
    }
}