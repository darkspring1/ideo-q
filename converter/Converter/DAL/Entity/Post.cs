﻿using Converter.DAL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Converter.DAL.Entity
{
    class Post
    {
        Lazy<TermTaxonomy[]> _colours;
        Lazy<TermTaxonomy[]> _fcolours;

        Lazy<TermTaxonomy[]> _sizes;
        Lazy<TermTaxonomy[]> _fsizes;

        Lazy<TermTaxonomy[]> _category;

        Lazy<TermTaxonomy[]> CreateLazy(string taxonomy)
        {
            return new Lazy<TermTaxonomy[]>(() =>
            {
                if (TermRelationships == null)
                {
                    return new TermTaxonomy[0];
                }
                return TermRelationships.Where(x => x?.TermTaxonomy?.taxonomy == taxonomy).Select(x => x.TermTaxonomy).ToArray();
            });
        }

        private Post()
        {
            _colours = CreateLazy(Taxonomy.PA_COLOR);
            _fcolours = CreateLazy(Taxonomy.PA_FCOLOR);

            _sizes = CreateLazy(Taxonomy.PA_SIZE);
            _fsizes = CreateLazy(Taxonomy.PA_FSIZE);

            _category = CreateLazy(Taxonomy.PRODUCT_CAT);
        }

        public long ID { get; set; }
        public List<TermRelationship> TermRelationships { get; set; }
        public string post_type { get; set; }


        public TermTaxonomy[] Colours => _colours.Value;
        public TermTaxonomy[] FColours => _fcolours.Value;

        public TermTaxonomy[] Sizes => _sizes.Value;

        public TermTaxonomy[] FSizes => _fsizes.Value;

        public TermTaxonomy[] Categories => _category.Value;


        public bool SetFColour(TermTaxonomy termTaxonomy)
        {
            if (FColours.Any(x => x.Term.LowerName == termTaxonomy.Term.LowerName))
            {
                return false;
            }

            TermRelationships.Add(new TermRelationship
            {
                term_taxonomy_id = termTaxonomy.term_taxonomy_id,
                TermTaxonomy = termTaxonomy
            });

            _fcolours = CreateLazy(Taxonomy.PA_FCOLOR);

            return true;
        }

        public bool SetFSize(TermTaxonomy termTaxonomy)
        {
            if (FSizes.Any(x => x.Term.LowerName == termTaxonomy.Term.LowerName))
            {
                return false;
            }

            TermRelationships.Add(new TermRelationship
            {
                term_taxonomy_id = termTaxonomy.term_taxonomy_id,
                TermTaxonomy = termTaxonomy
            });

            _fsizes = CreateLazy(Taxonomy.PA_FSIZE);

            return true;
        }

    }
}
