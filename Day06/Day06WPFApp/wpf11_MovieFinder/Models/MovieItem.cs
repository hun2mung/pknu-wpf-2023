﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf11_MovieFinder.Models
{
    internal class MovieItem
    {
        public bool Adult { get; set; }
        public int Id { get; set; }
        public string Original_Language { get; set; }
        public string Original_Title { get; set;}
        public string Overview { get; set;}
        public double Popularity { get; set; }
        public string Poster_Path { get; set; }
        public string Release_date { get; set; }
        public string Title { get; set; }
        public double Vote_Average { get; set; }
    }
}
