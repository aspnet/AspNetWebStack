// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents the catch block location for an <see cref="ExceptionContext"/>.</summary>
    [DebuggerDisplay("Name: {Name}, IsTopLevel: {IsTopLevel}")]
    public class ExceptionContextCatchBlock
    {
        private readonly string _name;
        private readonly bool _isTopLevel;
        private readonly bool _callsHandler;

        /// <summary>
        /// Initializes a new instance of <see cref="ExceptionContextCatchBlock"/> with the values provided.
        /// </summary>
        /// <param name="name">The label for the catch block where the exception was caught.</param>
        /// <param name="isTopLevel">
        /// A value indicating whether the catch block where the exception was caught is the last one before the host.
        /// </param>
        /// <param name="callsHandler">
        /// A value indicating whether exceptions in the catch block can be handled after they are logged.
        /// </param>
        /// <remarks>
        /// To compare an exception catch block with a well-known value, see classes like
        /// <see cref="ExceptionCatchBlocks"/> for the specific objects to use.
        /// This constructor is only intended for use within static classes that define such well-known catch blocks.
        /// </remarks>
        public ExceptionContextCatchBlock(string name, bool isTopLevel, bool callsHandler)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            _name = name;
            _isTopLevel = isTopLevel;
            _callsHandler = callsHandler;
        }

        /// <summary>Gets a label for the catch block in which the exception was caught.</summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets a value indicating whether the catch block where the exception was caught is the last one before the
        /// host.
        /// </summary>
        public bool IsTopLevel
        {
            get { return _isTopLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether exceptions in the catch block can be handled after they are logged.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Some exceptions are caught after a response is already partially sent, which prevents sending a new
        /// response to handle the exception. In such cases, <see cref="IExceptionLogger"/> will be called to log the
        /// exception, but the <see cref="IExceptionHandler"/> will not be called.
        /// </para>
        /// <para>
        /// If this value is <see langword="true"/>, exceptions from this catch block will be provided to both
        /// <see cref="IExceptionLogger"/> and <see cref="IExceptionHandler"/>. If this value is
        /// see langword="false"/>, exceptions from this catch block cannot be handled and will only be provided to
        /// <see cref="IExceptionLogger"/>.
        /// </para>
        /// </remarks>
        public bool CallsHandler
        {
            get { return _callsHandler; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _name;
        }
    }
}
