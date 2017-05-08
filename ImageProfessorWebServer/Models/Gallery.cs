using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProfessorWebServer.Models
{
    public class Gallery
    {
        [Key]
        public int ID { get; set; }
        public string UploadUser { get; set; }

        [Display(Name = "Upload Datetime")]
        [DataType(DataType.DateTime)]
        public DateTime UploadDateTime { get; set; }
        public string SourceImagePath { get; set; }
        public string ResultImagePath { get; set; }
        public bool IsProcessing { get; set; }
    }
}
