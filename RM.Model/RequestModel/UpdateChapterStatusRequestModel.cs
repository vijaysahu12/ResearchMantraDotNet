﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class UpdateChapterStatusRequestModel
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
    }
}
