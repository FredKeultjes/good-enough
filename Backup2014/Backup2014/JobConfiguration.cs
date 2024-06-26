﻿/*
Programming by Fred Keultjes

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup2014
{
    public class JobConfiguration
    {
        public List<SourceDefinition> Sources = new List<SourceDefinition>();

        public string TargetDirectory = string.Empty;
        public bool ConfirmAllActions = true;
        public bool ZipUpdatedAndDeletedFiles = false;
    }
}
