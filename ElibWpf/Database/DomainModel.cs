using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.DomainModel
{
    public class Autor
    {
        public long id { get; set; }
        public string ime { get; set; }
        public string opis { get; set; }

        public virtual ICollection<knjiga_autor> knjiga_autorValues { get; set; }
        public virtual ICollection<Serijal> SerijalValues { get; set; }
        public Autor()
        {
            knjiga_autorValues = new List<knjiga_autor>();
            SerijalValues = new List<Serijal>();
        }
    }

    public class Citat
    {
        public long id { get; set; }
        public string tekst { get; set; }
        public string nota { get; set; }
        public long knjigaId { get; set; }
    }

    public class Fajl
    {
        public long knjigaId { get; set; }
        public string format { get; set; }
        public byte[] fajl { get; set; }
    }

    public class Knjiga
    {
        public long id { get; set; }
        public string naziv { get; set; }
        public int serijalBr { get; set; }
        public byte[] cover { get; set; }
        public bool procitana { get; set; }
        public DateTime datProcitana { get; set; }
        public long serijalId { get; set; }

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
    }

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

    public class Serijal
    {
        public long id { get; set; }
        public string naziv { get; set; }
        public long autorId { get; set; }
        public virtual ICollection<Knjiga> KnjigaValues { get; set; }
        public virtual ICollection<Autor> AutorValues { get; set; }
        public Serijal()
        {
            KnjigaValues = new List<Knjiga>();
            AutorValues = new List<Autor>();
        }


    }

    public class knjiga_autor
    {
        public long knjiga_id { get; set; }
        public long autor_id { get; set; }
    }

    public class kolekcija_knjiga
    {
        public long kolekcija_id { get; set; }
        public long knjiga_id { get; set; }
    }

}
