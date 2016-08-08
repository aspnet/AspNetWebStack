// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Represents a candidate action found via a direct route.
    /// </summary>
    /// <remarks>
    /// Members are get/set for testability of the 'best-match' algorithm.
    /// </remarks>
    internal class DirectRouteCandidate
    {
        public ActionDescriptor ActionDescriptor
        {
            get;
            set;
        }

        public IEnumerable<ActionNameSelector> ActionNameSelectors
        {
            get;
            set;
        }

        public IEnumerable<ActionSelector> ActionSelectors
        {
            get;
            set;
        }

        public ControllerDescriptor ControllerDescriptor
        {
            get;
            set;
        }

        public bool HasActionNameSelectors
        {
            get
            {
                return ActionNameSelectors != null && ActionNameSelectors.Any();
            }
        }

        public bool HasActionSelectors
        {
            get
            {
                return ActionSelectors != null && ActionSelectors.Any();
            }
        }

        public int Order
        {
            get;
            set;
        }

        public decimal Precedence
        {
            get;
            set;
        }

        public RouteData RouteData
        {
            get;
            set;
        }

        public static DirectRouteCandidate SelectBestCandidate(List<DirectRouteCandidate> candidates, ControllerContext controllerContext)
        {
            Debug.Assert(controllerContext != null);
            Debug.Assert(candidates != null);

            // These filters will allow actions to opt-out of execution via the provided public extensibility points.
            List<DirectRouteCandidate> filteredByActionName = ApplyActionNameFilters(candidates, controllerContext);
            List<DirectRouteCandidate> applicableCandidates = ApplyActionSelectors(filteredByActionName, controllerContext);

            // At this point all of the remaining actions are applicable - now we're just trying to find the
            // most specific match.
            //
            // Order is first, because it's the 'override' to our algorithm
            List<DirectRouteCandidate> filteredByOrder = FilterByOrder(applicableCandidates);
            List<DirectRouteCandidate> filteredByPrecedence = FilterByPrecedence(filteredByOrder);

            if (filteredByPrecedence.Count == 0)
            {
                return null;
            }
            else if (filteredByPrecedence.Count == 1)
            {
                return filteredByPrecedence[0];
            }
            else
            {
                throw CreateAmbiguiousMatchException(candidates);
            }
        }

        private static AmbiguousMatchException CreateAmbiguiousMatchException(List<DirectRouteCandidate> candidates)
        {
            string ambiguityList = CreateAmbiguousMatchList(candidates);
            string message = String.Format(
                CultureInfo.CurrentCulture,
                MvcResources.DirectRoute_AmbiguousMatch,
                ambiguityList);

            return new AmbiguousMatchException(message);
        }

        protected static string CreateAmbiguousMatchList(IEnumerable<DirectRouteCandidate> candidates)
        {
            StringBuilder exceptionMessageBuilder = new StringBuilder();
            foreach (DirectRouteCandidate candidate in candidates)
            {
                MethodInfo method = null;

                ReflectedActionDescriptor reflectedActionDescriptor = candidate.ActionDescriptor as ReflectedActionDescriptor;
                if (reflectedActionDescriptor == null)
                {
                    ReflectedAsyncActionDescriptor reflectedAsyncActionDescriptor = candidate.ActionDescriptor as ReflectedAsyncActionDescriptor;
                    if (reflectedAsyncActionDescriptor != null)
                    {
                        method = reflectedAsyncActionDescriptor.AsyncMethodInfo;
                    }
                }
                else
                {
                    method = reflectedActionDescriptor.MethodInfo;
                }

                string controllerAction = method == null ? candidate.ActionDescriptor.ActionName : Convert.ToString(method, CultureInfo.CurrentCulture);
                string controllerType = method.DeclaringType.FullName;

                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendFormat(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatchType, controllerAction, controllerType);
            }

            return exceptionMessageBuilder.ToString();
        }

        private static List<DirectRouteCandidate> ApplyActionNameFilters(List<DirectRouteCandidate> candidates, ControllerContext controllerContext)
        {
            List<DirectRouteCandidate> filtered = new List<DirectRouteCandidate>();
            foreach (DirectRouteCandidate candidate in candidates)
            {
                string actionName;
                candidate.RouteData.Values.TryGetValue<string>("action", out actionName);

                if (candidate.HasActionNameSelectors)
                {
                    // For the sake of consistency - we still want to call the action name selectors even if
                    // this route was matched without providing an action name.
                    actionName = actionName ?? candidate.ActionDescriptor.ActionName;

                    if (candidate.ActionNameSelectors.All(selector => selector(controllerContext, actionName)))
                    {
                        filtered.Add(candidate);
                    }
                }
                else if (actionName != null)
                {
                    if (String.Equals(actionName, candidate.ActionDescriptor.ActionName, StringComparison.OrdinalIgnoreCase))
                    {
                        filtered.Add(candidate);
                    }
                }
                else
                {
                    // No name-based filtering applies for this route.
                    filtered.Add(candidate);
                }
            }

            return filtered;
        }

        private static List<DirectRouteCandidate> ApplyActionSelectors(List<DirectRouteCandidate> candidates, ControllerContext controllerContext)
        {
            List<DirectRouteCandidate> matchesWithActionSelectors = new List<DirectRouteCandidate>();
            List<DirectRouteCandidate> matchesWithoutActionSelectors = new List<DirectRouteCandidate>();

            foreach (DirectRouteCandidate candidate in candidates)
            {
                if (candidate.HasActionSelectors)
                {
                    if (candidate.ActionSelectors.All(selector => selector(controllerContext)))
                    {
                        matchesWithActionSelectors.Add(candidate);
                    }
                }
                else
                {
                    matchesWithoutActionSelectors.Add(candidate);
                }
            }

            return matchesWithActionSelectors.Any() ? matchesWithActionSelectors : matchesWithoutActionSelectors;
        }

        private static List<DirectRouteCandidate> FilterByOrder(List<DirectRouteCandidate> candidates)
        {
            if (!candidates.Any())
            {
                return candidates;
            }

            int minimum = candidates.Min(c => c.Order);
            return candidates.Where(c => c.Order == minimum).AsList();
        }

        private static List<DirectRouteCandidate> FilterByPrecedence(List<DirectRouteCandidate> candidates)
        {
            if (!candidates.Any())
            {
                return candidates;
            }

            decimal minimum = candidates.Min(c => c.Precedence);
            return candidates.Where(c => c.Precedence == minimum).AsList();
        }
    }
}
