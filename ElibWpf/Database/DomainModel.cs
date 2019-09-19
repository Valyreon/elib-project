using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.DomainModel
{
    [Table("Autor")]
    public class Autor
    {
        public long id { get; set; }
        public string ime { get; set; }
        public string opis { get; set; }
        public string metadata { get; set; }

        public virtual ICollection<knjiga_autor> knjiga_autorValues { get; set; }
        public virtual ICollection<Serijal> SerijalValues { get; set; }
        public Autor()
        {
            knjiga_autorValues = new List<knjiga_autor>();
            SerijalValues = new List<Serijal>();
        }
    }

    [Table("Citat")]
    public class Citat
    {
        public long id { get; set; }
        public string tekst { get; set; }
        public string nota { get; set; }
        public long knjigaId { get; set; }
        [ForeignKey("knjigaId")]
        public virtual Knjiga knjiga { get; set; }
    }

    [Table("Fajl")]
    public class Fajl
    {
        [Key, Column(Order = 0)]
        public long knjigaId { get; set; }
        [Key, Column(Order = 1)]
        public string format { get; set; }
        public byte[] fajl { get; set; }
        [ForeignKey("knjigaId")]
        public virtual Knjiga knjiga { get; set; }
    }

    [Table("Knjiga")]
    public class Knjiga
    {
        public long id { get; set; }
        public string naziv { get; set; }
        public int serijalBr { get; set; }
        public byte[] cover { get; set; }
        public bool procitana { get; set; }
        public DateTime datProcitana { get; set; }
        public long serijalId { get; set; }
        public string metadata { get; set; }

        public virtual ICollection<Citat> CitatValues { get; set; }
        public virtual ICollection<Fajl> FajlValues { get; set; }
        public virtual ICollection<knjiga_autor> knjiga_autorValues { get; set; }
        public virtual ICollection<kolekcija_knjiga> kolekcija_knjigaValues { get; set; }
        public Knjiga()
        {
            CitatValues = new List<Citat>();
            FajlValues = new List<Fajl>();
            knjiga_autorValues = new List<knjiga_autor>();
            kolekcija_knjigaValues = new List<kolekcija_knjiga>();
        }
        [ForeignKey("serijalId")]
        public virtual Serijal serijal { get; set; }
    }

    [Table("Kolekcija")]
    public class Kolekcija
    {
        public long id { get; set; }
        public string tag_naziv { get; set; }

        public virtual ICollection<kolekcija_knjiga> kolekcija_knjigaValues { get; set; }
        public Kolekcija()
        {
            kolekcija_knjigaValues = new List<kolekcija_knjiga>();
        }
    }

    [Table("Serijal")]
    public class Serijal
    {
        public long id { get; set; }
        public string naziv { get; set; }
        
        public long autorId { get; set; }
        public string metadata { get; set; }
        public virtual ICollection<Knjiga> KnjigaValues { get; set; }
        public Serijal()
        {
            KnjigaValues = new List<Knjiga>();
        }

        [ForeignKey("autorId")]
        public virtual Autor autor { get; set; }


    }

    [Table("knjiga_autor")]
    public class knjiga_autor
    {
        [Key, Column(Order = 0)]
        public long knjiga_id { get; set; }
        [Key, Column(Order = 1)]
        public long autor_id { get; set; }

        [ForeignKey("autor_id")]
        public virtual Autor autor { get; set; }
        [ForeignKey("knjiga_id")]
        public virtual Knjiga knjiga { get; set; }
    }

    [Table("kolekcija_knjiga")]
    public class kolekcija_knjiga
    {
        [Key, Column(Order = 0)]
        public virtual long kolekcija_id { get; set; }
        [Key, Column(Order = 1)]
        public virtual long knjiga_id { get; set; }
        [ForeignKey("kolekcija_id")]
        public virtual Kolekcija kolekcija { get; set; }
        [ForeignKey("knjiga_id")]
        public virtual Knjiga knjiga { get; set; }
    }

}
