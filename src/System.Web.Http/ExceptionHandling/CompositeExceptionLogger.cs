// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    internal class CompositeExceptionLogger : IExceptionLogger
    {
        private readonly IExceptionLogger[] _loggers;

        public CompositeExceptionLogger(params IExceptionLogger[] loggers)
            : this((IEnumerable<IExceptionLogger>)loggers)
        {
        }

        public CompositeExceptionLogger(IEnumerable<IExceptionLogger> loggers)
        {
            if (loggers == null)
            {
                throw new ArgumentNullException("loggers");
            }

            _loggers = loggers.ToArray();
        }

        public IEnumerable<IExceptionLogger> Loggers
        {
            get { return _loggers; }
        }

        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>();

            Contract.Assert(_loggers != null);

            foreach (IExceptionLogger logger in _loggers)
            {
                if (logger == null)
                {
                    throw new InvalidOperationException(Error.Format(SRResources.TypeInstanceMustNotBeNull,
                        typeof(IExceptionLogger).Name));
                }

                Task task = logger.LogAsync(context, cancellationToken);
                tasks.Add(task);
            }

            return Task.WhenAll(tasks);
        }
    }
}
