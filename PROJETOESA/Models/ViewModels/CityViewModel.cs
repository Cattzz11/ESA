﻿namespace PROJETOESA.Models.ViewModels
{
    public class CityViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public CountryViewModel Country { get; set; }
    }
}
