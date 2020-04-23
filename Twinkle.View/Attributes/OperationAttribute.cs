using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using Twinkle.View.Attributes;
using Twinkle.View.Enums;

[module: Operation]

namespace Twinkle.View.Attributes
{
    /// <summary>
    /// The Operation attribute marks methods semantically responsible for control methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly | AttributeTargets.Module,
        AllowMultiple = false, Inherited = true)]
    public class OperationAttribute : TwinkleBaseAttribute, IMethodDecorator
    {
        /// <summary>
        /// Alias operations
        /// </summary>
        public string Alias { get; set; }
        
        /// <summary>
        /// Control method marked with Operation attribute
        /// </summary>
        private MethodBase _method;
        
        /// <summary>
        /// Object of control
        /// </summary>
        private Object _instanse;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="instance">Object of control</param>
        /// <param name="method">Control method marked with Operation attribute</param>
        /// <param name="args">Method parameters</param>
        public void Init(object instance, MethodBase method, object[] args)
        {
            _method = method;
            _instanse = instance;
        }

        /// <summary>
        /// Actions before method execution
        /// </summary>
        public void OnEntry()
        {
            //todo Check configuration parameter "automatic_view_detection"
            ClearActiveViews();
            ((TwinkleView)_instanse.GetType().GetProperty("TwinkleView").GetValue(_instanse)).GetActiveViews();
            FindHooks(true);
        }
        
        /// <summary>
        /// Clearing active views
        /// </summary>
        private void ClearActiveViews()
        {
            var activeViews = ((TwinkleView)_instanse.GetType().GetProperty("TwinkleView").GetValue(_instanse)).SystemContext.Views.ActiveViews;
            if (activeViews != null)
                activeViews.Clear();
        }

        /// <summary>
        /// Actions after method execution
        /// </summary>
        public void OnExit()
        {
            FindHooks(false);
        }

        /// <summary>
        /// If exception on execute hooks
        /// </summary>
        /// <param name="exception"></param>
        public void OnException(Exception exception)
        {
            //ignore
        }

        /// <summary>
        /// Find and execute operation hooks
        /// </summary>
        private void FindHooks(bool isPreHook)
        { 
            var targetListPreHooks = new List<Hook>();
            var targetListPostHooks = new List<Hook>();
            
            var operationAttributes = _method.GetCustomAttributes(typeof(OperationAttribute), true).FirstOrDefault() as OperationAttribute;

            if (operationAttributes is null)
                return;

            var targetControlObject = _instanse as Control;

            if (targetControlObject.Hooks is null)
                return;

            var operationAlias = operationAttributes.Alias;

            foreach (var controlHook in targetControlObject.Hooks)
            {
                if (controlHook.AliasOperation.Equals(operationAlias))
                {
                
                    if (isPreHook && controlHook.Type == TypeHook.PRE)
                    {
                        targetListPreHooks.Add(controlHook);
                    }
                    
                    if (!isPreHook && controlHook.Type == TypeHook.POST)
                    {
                        targetListPostHooks.Add(controlHook);
                    }
                }
            }

            if (targetListPreHooks.Any())
                targetListPreHooks.ForEach(x=>x.Handler.Invoke());
            
            if (targetListPostHooks.Any())
                targetListPostHooks.ForEach(x=>x.Handler.Invoke());
        }
    }
}