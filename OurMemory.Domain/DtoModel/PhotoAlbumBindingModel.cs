﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurMemory.Domain.DtoModel
{
    public class PhotoAlbumBindingModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public virtual IEnumerable<ImageReference> Images { get; set; }
    }
}
