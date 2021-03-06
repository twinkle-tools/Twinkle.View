﻿using System.Collections.Generic;
using Twinkle.View.Enums;
using Twinkle.View.Infrastructure;

namespace Twinkle.View
{
    public abstract class View
    {
        public string Alias { get; set; }
        public string Prefix { get; set; }
        public virtual bool IsActive { get; set; }
        public delegate bool ViewDefinitionCriteriaDelegat();
        public List<ViewDefinitionCriteriaDelegat> ViewDefinitionCriteria { get; private set; }
        public List<Hook> Hooks { get; private set; }

        /// <summary>
        /// Method of adding a criterion for determining the activity of a view
        /// </summary>
        /// <param name="criteria"></param>
        protected void AddViewDefinitionCriteria(ViewDefinitionCriteriaDelegat criteria)
        {
            if(ViewDefinitionCriteria==null)
                ViewDefinitionCriteria = new List<ViewDefinitionCriteriaDelegat>();

            ViewDefinitionCriteria.Add(criteria);
        }

        /// <summary>
        /// Method of adding a hook to the view control
        /// </summary>
        /// <param name="hook"></param>
        protected void AddHook(Hook hook)
        {
            if (Hooks == null)
                Hooks = new List<Hook>();

            Hooks.Add(hook);
        }
        
        /// <summary>
        /// Method of adding a hook to the view control
        /// </summary>
        /// <param name="hook"></param>
        protected void AddHook(TypeHook type, string aliasControl, string aliasOperation, Hook.HandlerHookDelegate handler)
        {
            if (Hooks == null)
                Hooks = new List<Hook>();
        
            Hooks.Add(new Hook(type, aliasControl, aliasOperation, handler));
        }
    }
}
