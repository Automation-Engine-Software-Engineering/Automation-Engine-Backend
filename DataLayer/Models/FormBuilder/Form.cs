﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.FormBuilder
{
    public class Form
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public double SizeWidth { get; set; }
        public double SizeHeight { get; set; }
        public string BackgroundColor { get; set; } = "";
        public string BackgroundImgPath { get; set; } = "";
        public string? HtmlFormBody { get; set; }

        public List<Entity>? Entities { get; set; }
    }
}
