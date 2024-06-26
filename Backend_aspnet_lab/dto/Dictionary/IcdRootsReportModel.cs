﻿using System.Text.Json.Serialization;
using System;

namespace Backend_aspnet_lab.dto.Dictionary
{
    public class IcdRootsReportModel
    {
        public IcdRootsReportFiltersModel Filters { get; set; }

        public List<IcdRootsReportRecordModel> Records { get; set; }

        public Dictionary<string, int> SummaryByRoot { get; set; }
    }
}
