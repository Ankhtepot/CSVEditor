﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSVEditor.Model
{
    /// <summary>
    /// Class describing CSv File Configuration, intended to be used for serialization
    /// </summary>
    public class CsvFileConfiguration
    {
        public List<CsvColumnConfiguration> ColumnConfigurations { get; set; }

        public string AbsoluteFilePath { get; set; }

        public string FileName 
        {
            get 
            {
                return !string.IsNullOrEmpty(AbsoluteFilePath) && File.Exists(AbsoluteFilePath)
                    ? Path.GetFileName(AbsoluteFilePath)
                    : "";                
            } 
        }
    }
}