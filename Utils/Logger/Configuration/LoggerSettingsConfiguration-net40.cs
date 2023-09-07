﻿// Copyright 2015 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace Serilog.Configuration
{
    /// <summary>
    /// Allows additional setting sources to drive the logger configuration.
    /// </summary>
    public class LoggerSettingsConfiguration
    {
        readonly LoggerConfiguration _loggerConfiguration;

        internal LoggerSettingsConfiguration(LoggerConfiguration loggerConfiguration)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");
            _loggerConfiguration = loggerConfiguration;
        }

        /// <summary>
        /// Apply external settings to the logger configuration.
        /// </summary>
        /// <returns>Configuration object allowing method chaining.</returns>
        public LoggerConfiguration Settings(ILoggerSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            settings.Configure(_loggerConfiguration);

            return _loggerConfiguration;
        }
    }
}
