﻿using MVVMLibrary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("UserCollections")]
    public class UserCollection : ObservableObject
    {
        private string tag;

        public ICollection<Book> Books { get; set; }
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Tag
        {
            get => this.tag;
            set => Set(() => Tag, ref tag, value);
        }
    }
}