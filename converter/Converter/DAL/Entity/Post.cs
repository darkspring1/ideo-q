using System;
using System.Collections.Generic;
using System.Text;

namespace Converter.DAL.Entity
{
    class Post
    {
        public long ID { get; set; }

        public List<TermRelationship> TermRelationships { get; set; }
        public string post_type { get;  set; }
    }
}
